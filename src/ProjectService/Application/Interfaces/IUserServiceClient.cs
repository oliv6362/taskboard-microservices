using System.Net;

namespace ProjectService.Application.Interfaces
{
    public interface IUserServiceClient
    {
        Task<HttpStatusCode> GetUserStatusAsync(int userId, CancellationToken ct);
    }
}
