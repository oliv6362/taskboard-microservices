using PactNet;
using AssignmentService.Application.Interfaces;
using AssignmentService.Infrastructure.Clients;
using System.Net;

/// <summary>
/// Consumer-driven contract tests for the interaction between
/// AssignmentService (consumer) and ProjectService (provider).
///
/// These tests define the HTTP contract that AssignmentService relies on
/// when validating project existence via ProjectService.
///
/// The contract is verified using Pact and executed against the real
/// consumer implementation (ProjectServiceClient).
/// </summary>
public class AssignmentServiceConsumerPactTests
{
    private const string ConsumerName = "AssignmentService";
    private const string ProviderName = "ProjectService";

    private const string State_Project1Exists = "Project with ID 1 exists";
    private const string State_Project999Missing = "Project with ID 999 does not exist";

    private const int ProjectId_Exists = 1;
    private const int ProjectId_Missing = 999;

    /// <summary>
    /// Creates the Pact configuration used by the consumer tests.
    /// The generated pact file is written to the shared contracts
    /// directory so it can be verified by the provider service.
    /// </summary>
    private static PactConfig CreatePactConfig() => new()
    {
        PactDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "contracts", "pacts"))
    };

    /// <summary>
    /// Creates a Pact V4 HTTP interaction builder for defining
    /// synchronous HTTP contracts between AssignmentService and ProjectService.
    /// </summary>
    private static IPactBuilderV4 CreatePact() =>
        Pact.V4(ConsumerName, ProviderName, CreatePactConfig())
            .WithHttpInteractions();

    /// <summary>
    /// Verifies that AssignmentService can successfully validate an existing project.
    ///
    /// Given that a project with ID 1 exists in ProjectService, the consumer
    /// expects a HTTP 200 OK response when requesting that project.
    /// </summary>
    [Fact]
    public async Task GetProject_ProjectExists_Returns200()
    {
        // Arrange
        var pact = CreatePact();

        pact
            .UponReceiving("a request to get an existing project")
            .Given(State_Project1Exists)
            .WithRequest(HttpMethod.Get, $"/api/Project/{ProjectId_Exists}")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK);

        // Act & Assert
        await pact.VerifyAsync(async ctx =>
        {
            using var http = new HttpClient { BaseAddress = ctx.MockServerUri };
            IProjectServiceClient client = new ProjectServiceClient(http);

            var status = await client.GetProjectStatusAsync(ProjectId_Exists, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, status);
        });
    }

    /// <summary>
    /// Verifies that AssignmentService handles a missing project correctly.
    ///
    /// Given that a project with ID 999 does not exist in ProjectService, the
    /// consumer expects a HTTP 404 Not Found response.
    /// </summary>
    [Fact]
    public async Task GetProject_ProjectMissing_Returns404()
    {
        // Arrange
        var pact = CreatePact();

        pact
            .UponReceiving("a request to get a missing project")
            .Given(State_Project999Missing)
            .WithRequest(HttpMethod.Get, $"/api/Project/{ProjectId_Missing}")
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        // Act & Assert
        await pact.VerifyAsync(async ctx =>
        {
            using var http = new HttpClient { BaseAddress = ctx.MockServerUri };
            IProjectServiceClient client = new ProjectServiceClient(http);

            var status = await client.GetProjectStatusAsync(ProjectId_Missing, CancellationToken.None);

            Assert.Equal(HttpStatusCode.NotFound, status);
        });
    }
}
