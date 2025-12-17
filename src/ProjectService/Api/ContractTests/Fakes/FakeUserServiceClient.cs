using ProjectService.Application.Interfaces;
using System.Net;

namespace ProjectService.Api.ContractTests.Fakes;

/// <summary>
/// Test double used only during Pact provider verification.
/// Keeps provider tests isolated from external dependencies.
/// </summary>
public sealed class FakeUserServiceClient : IUserServiceClient
{
    public Task<HttpStatusCode> GetUserStatusAsync(int userId, CancellationToken ct)
    {
        return Task.FromResult(userId switch
        {
            999 => HttpStatusCode.NotFound,
            _ => HttpStatusCode.OK
        });
    }
}
