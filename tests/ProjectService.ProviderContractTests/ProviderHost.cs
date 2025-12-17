using System.Diagnostics;
using System.Net.Sockets;
using DotNetEnv;
using Testcontainers.MsSql;
using Xunit.Abstractions;

namespace ProjectService.ProviderContractTests;

/// <summary>
/// Manages the lifecycle of a real ProjectService instance for provider
/// contract verification.
///
/// This host starts an isolated SQL Server using Testcontainers,
/// launches the ProjectService API as a separate process, waits until
/// the service is healthy, and exposes the base address used during
/// Pact verification.
/// </summary>
public sealed class ProviderHost : IAsyncDisposable
{
    private readonly ITestOutputHelper _output;

    private MsSqlContainer _db = null!;
    private Process _api = null!;
    public Uri BaseUri { get; private set; } = null!;

    public ProviderHost(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Starts the provider environment by launching a SQL Server
    /// container and running the ProjectService API as a separate process.
    ///
    /// The method blocks until the API reports itself as healthy or
    /// a timeout is reached.
    /// </summary>
    public async Task StartAsync(CancellationToken ct = default)
    {
        Env.Load();

        var dbPassword =
            Environment.GetEnvironmentVariable("TEST_SQL_PASSWORD")
            ?? throw new InvalidOperationException("TEST_SQL_PASSWORD not set.");

        // Create and start SQL Server container
        _db = new MsSqlBuilder()
            .WithPassword(dbPassword)
            .Build();

        await _db.StartAsync(ct);

        // Pick a free port and build the base URL
        BaseUri = new Uri($"http://127.0.0.1:{GetFreePort()}");

        var csprojPath = FindProjectServiceCsproj();

        // Start ProjectService as a separate process
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{csprojPath}\" --urls \"{BaseUri}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Pass environment variables to the API process
        psi.Environment["ConnectionStrings__ProjectDb"] = _db.GetConnectionString();
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "ContractTest";
        psi.Environment["DOTNET_ENVIRONMENT"] = "ContractTest";

        _api = Process.Start(psi)!;

        // Capture API logs into xUnit output
        _api.OutputDataReceived += (_, e) => { if (e.Data != null) _output.WriteLine(e.Data); };
        _api.ErrorDataReceived += (_, e) => { if (e.Data != null) _output.WriteLine(e.Data); };
        _api.BeginOutputReadLine();
        _api.BeginErrorReadLine();

        await WaitForHealthyAsync(BaseUri, timeoutSeconds: 30, ct);
    }

    /// <summary>
    /// Stops the ProjectService process and disposes of the SQL Server
    /// container used during provider verification.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_api is not null && !_api.HasExited)
            {
                _api.Kill(entireProcessTree: true);
                await _api.WaitForExitAsync();
            }
        }
        catch { }

        if (_db is not null)
            await _db.DisposeAsync();
    }

    /// <summary>
    /// Ensures an available TCP port is found for the Provider API.
    /// </summary>
    private static int GetFreePort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// Keep checking the provider health endpoint until the service reports
    /// a healthy status or a timeout is reached.
    /// </summary>
    private static async Task WaitForHealthyAsync(Uri baseUri, int timeoutSeconds, CancellationToken ct)
    {
        using var http = new HttpClient { BaseAddress = baseUri };
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var res = await http.GetAsync("/health", ct);
                if (res.IsSuccessStatusCode) return;
            }
            catch { }

            await Task.Delay(300, ct);
        }

        throw new TimeoutException($"ProjectService did not become healthy at {baseUri}/health within {timeoutSeconds}s.");
    }

    /// <summary>
    /// Locates the ProjectService project file so the test can start the real ProjectService.
    /// </summary>
    private static string FindProjectServiceCsproj()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src")))
            dir = dir.Parent;

        if (dir is null)
            throw new DirectoryNotFoundException("Could not find repo root with /src folder.");

        var csprojPath = Path.Combine(dir.FullName, "src", "ProjectService", "ProjectService.csproj");
        if (!File.Exists(csprojPath))
            throw new FileNotFoundException($"Could not find {csprojPath}. Update FindProjectServiceCsproj().");

        return csprojPath;
    }
}