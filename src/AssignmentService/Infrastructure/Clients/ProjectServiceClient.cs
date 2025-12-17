using System.Net;
using AssignmentService.Application.Interfaces;

namespace AssignmentService.Infrastructure.Clients
{
    public sealed class ProjectServiceClient(HttpClient http) : IProjectServiceClient
    {
        public async Task<HttpStatusCode> GetProjectStatusAsync(int projectId, CancellationToken ct)
        {
            // Fire-and-hope approach
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

            try
            {
                using var res = await http.GetAsync($"/api/Project/{projectId}", timeoutCts.Token);
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
