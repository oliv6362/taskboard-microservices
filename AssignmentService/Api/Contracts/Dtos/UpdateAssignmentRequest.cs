using AssignmentService.Domain.Entities;

namespace AssignmentService.Api.Contracts.Dtos
{
    public record UpdateAssignmentRequest
    (
        AssignmentStatus Status
    );
}
