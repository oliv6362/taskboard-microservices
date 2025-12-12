using ProjectService.Application.Interfaces;
using ProjectService.Domain.Entities;
using ProjectService.Infrastructure.Data;

namespace ProjectService.Application.Services
{
    public class ProjectAppService(ProjectDbContext db) : IProjectService
    {
        public async Task<Project> CreateProject(Project project, CancellationToken ct)
        {
            db.Projects.Add(project);
            await db.SaveChangesAsync(ct);
            return project;
        }

        public async Task<Project?> GetProjectById(int id, CancellationToken ct) =>
            await db.Projects.FindAsync([id], ct);
    }
}
