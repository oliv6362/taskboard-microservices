using PactNet.Verifier;
using Xunit.Abstractions;

namespace ProjectService.ProviderContractTests;

/// <summary>
/// Verifies that the ProjectService provider fulfills the expectations
/// defined in the consumer-driven contracts (Pacts).
///
/// This test boots a real instance of the ProjectService, loads Pact files
/// produced by consumers, and validates that all documented interactions
/// are correctly implemented by the provider.
/// </summary>
public class ProjectServiceProviderPactTests : IAsyncLifetime
{
    private readonly ProviderHost _host;
    private readonly ITestOutputHelper _output;

    public ProjectServiceProviderPactTests(ITestOutputHelper output)
    {
        _output = output;
        _host = new ProviderHost(output);
    }

    public Task InitializeAsync() => _host.StartAsync();
    public async Task DisposeAsync() => await _host.DisposeAsync();

    /// <summary>
    /// Verifies that the ProjectService implementation satisfies the
    /// contract defined by the AssignmentService consumer Pact.
    ///
    /// The test loads the Pact file, configures provider states,
    /// and executes Pact verification against the running provider.
    /// </summary>
    [Fact]
    public void Verify_ProjectService_Against_AssignmentService_Pact()
    {
        // Arrange
        var pactFile = PactPath("AssignmentService-ProjectService.json");

        // Act & Assert
        new PactVerifier("ProjectService")
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
