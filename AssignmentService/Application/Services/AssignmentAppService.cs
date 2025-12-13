using AssignmentService.Domain.Entities;
using AssignmentService.Application.Interfaces;
using AssignmentService.Infrastructure.Data;

namespace AssignmentService.Application.Services
{
    public class AssignmentAppService(AssignmentDbContext db) : IAssignmentService
    {
        public async Task<Assignment> CreateAssignment(Assignment assignment, CancellationToken ct)
        {
            db.Add(assignment);
            await db.SaveChangesAsync(ct);
            return assignment;
        }

        public async Task<Assignment?> GetAssignmentById(int id, CancellationToken ct) =>
            await db.Assignments.FindAsync([id], ct);
    }
}
