namespace AssignmentService.Api.Contracts.Dtos
{
    public record CreateAssignmentRequest
    (
        string Title,
        string Description,
        int ProjectId
    );
}
