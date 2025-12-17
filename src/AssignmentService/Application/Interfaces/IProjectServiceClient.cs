using System.Net;

namespace AssignmentService.Application.Interfaces
{
    public interface IProjectServiceClient
    {
        Task<HttpStatusCode> GetProjectStatusAsync(int projectId, CancellationToken ct);
    }
}
