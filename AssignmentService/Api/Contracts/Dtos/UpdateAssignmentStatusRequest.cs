using AssignmentService.Domain.Entities;

namespace AssignmentService.Api.Contracts.Dtos
{
    public record UpdateAssignmentStatusRequest
    (
        AssignmentStatus Status
    );
}
