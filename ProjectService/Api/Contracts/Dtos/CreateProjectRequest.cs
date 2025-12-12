namespace ProjectService.Api.Contracts.Dtos
{
    public record CreateProjectRequest
    (
        string Name,
        string Description,
        int OwnerUserId
    );
}
