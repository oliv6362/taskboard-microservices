using PactNet.Verifier;
using Xunit.Abstractions;

namespace UserService.ProviderContractTests;

/// <summary>
/// Verifies that the UserService provider fulfills the expectations
/// defined in the consumer-driven contracts (Pacts).
/// 
/// This test boots a real instance of the UserService, loads Pact files
/// produced by consumers, and validates that all documented interactions
/// are correctly implemented by the provider.
/// </summary>
public class UserServiceProviderPactTests : IAsyncLifetime
{
    private readonly ProviderHost _host;
    private readonly ITestOutputHelper _output;

    public UserServiceProviderPactTests(ITestOutputHelper output)
    {
        _output = output;
        _host = new ProviderHost(output);
    }

    public Task InitializeAsync() => _host.StartAsync();
    public async Task DisposeAsync() => await _host.DisposeAsync();

    /// <summary>
    /// Verifies that the UserService implementation satisfies the
    /// contract defined by the ProjectService consumer Pact.
    /// 
    /// The test loads the Pact file, configures provider states,
    /// and executes Pact verification against the running provider.
    /// </summary>
    [Fact]
    public void Verify_UserService_Against_ProjectService_Pact()
    {
        // Arrange         
        var pactFile = PactPath("ProjectService-UserService.json");

        // Act & Assert
        new PactVerifier("UserService")
            .WithHttpEndpoint(_host.BaseUri)
            .WithFileSource(pactFile)
            .WithProviderStateUrl(new Uri(_host.BaseUri, "/provider-states"))
            .Verify();
    }

    /// <summary>
    /// Resolves and validates the path to a Pact file
    /// located in the shared contracts directory.
    /// </summary>
    private static FileInfo PactPath(string pactFileName)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "contracts", "pacts",
            pactFileName));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Pact file not found: {path}");

        return new FileInfo(path);
    }
}
