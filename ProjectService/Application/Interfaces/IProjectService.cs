using ProjectService.Domain.Entities;

namespace ProjectService.Application.Interfaces
{
    public interface IProjectService
    {
        Task<Project> CreateProject(Project project, CancellationToken ct);
        Task<Project?> GetProjectById(int id, CancellationToken ct);
    }
}
