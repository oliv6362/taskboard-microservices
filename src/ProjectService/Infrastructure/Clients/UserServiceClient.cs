using ProjectService.Application.Interfaces;
using System.Net;

namespace ProjectService.Infrastructure.Clients
{ 
    public sealed class UserServiceClient(HttpClient http) : IUserServiceClient
    {
        public async Task<HttpStatusCode> GetUserStatusAsync(int userId, CancellationToken ct)
        {
            // Fire-and-hope approach
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

            try
            {
                using var res = await http.GetAsync($"/api/User/{userId}", timeoutCts.Token);
                return res.StatusCode;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return HttpStatusCode.RequestTimeout;
            }
            catch (HttpRequestException)
            {
                return HttpStatusCode.ServiceUnavailable;
            }
        }
    }
}