using AssignmentService.Domain.Entities;

namespace AssignmentService.Application.Interfaces
{
    public interface IAssignmentService
    {
        Task<Assignment> CreateAssignment(Assignment assignment, CancellationToken ct);
        Task<Assignment?> GetAssignmentById(int id, CancellationToken ct);
    }
}
