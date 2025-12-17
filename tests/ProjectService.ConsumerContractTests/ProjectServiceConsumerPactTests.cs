using PactNet;
using ProjectService.Application.Interfaces;
using ProjectService.Infrastructure.Clients;
using System.Net;

/// <summary>
/// Consumer-driven contract tests for the interaction between
/// ProjectService (consumer) and UserService (provider).
///
/// These tests define the HTTP contract that ProjectService relies on
/// when validating user existence via UserService. 
/// 
/// The contract is verified using Pact and executed against the real consumer
/// implementation (UserServiceClient).
/// </summary>
public class ProjectServiceConsumerPactTests
{
    private const string ConsumerName = "ProjectService";
    private const string ProviderName = "UserService";

    private const string State_User1Exists = "User with ID 1 exists";
    private const string State_User999Missing = "User with ID 999 does not exist";

    private const int UserId_Exists = 1;
    private const int UserId_Missing = 999;

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
    /// synchronous HTTP contracts between ProjectService and UserService.
    /// </summary>
    private static IPactBuilderV4 CreatePact() =>
        Pact.V4(ConsumerName, ProviderName, CreatePactConfig())
            .WithHttpInteractions();

    /// <summary>
    /// Verifies that ProjectService can successfully validate an existing user.
    ///
    /// Given that a user with ID 1 exists in UserService, the consumer
    /// expects a HTTP 200 OK response when requesting that user.
    /// </summary>
    [Fact]
    public async Task GetUser_UserExists_Returns200()
    {
        // Arrange
        var pact = CreatePact();

        pact
            .UponReceiving("a request to get an existing user")
            .Given(State_User1Exists)
            .WithRequest(HttpMethod.Get, $"/api/User/{UserId_Exists}")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK);

        // Act & Assert
        await pact.VerifyAsync(async ctx =>
        {
            // Act
            using var http = new HttpClient { BaseAddress = ctx.MockServerUri };
            IUserServiceClient client = new UserServiceClient(http);

            var status = await client.GetUserStatusAsync(UserId_Exists, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, status);
        });
    }

    /// <summary>
    /// Verifies that ProjectService handles a missing user correctly.
    ///
    /// Given that a user with ID 999 does not exist in UserService, the
    /// consumer expects a HTTP 404 Not Found response.
    /// </summary>
    [Fact]
    public async Task GetUser_UserMissing_Returns404()
    {
        // Arrange
        var pact = CreatePact();

        pact
            .UponReceiving("a request to get a missing user")
            .Given(State_User999Missing)
            .WithRequest(HttpMethod.Get, $"/api/User/{UserId_Missing}")
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        // Act & Assert
        await pact.VerifyAsync(async ctx =>
        {
            // Act
            using var http = new HttpClient { BaseAddress = ctx.MockServerUri };
            IUserServiceClient client = new UserServiceClient(http);

            var status = await client.GetUserStatusAsync(UserId_Missing, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, status);
        });
    }
}
