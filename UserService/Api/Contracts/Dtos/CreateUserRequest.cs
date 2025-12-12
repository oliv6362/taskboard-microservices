namespace UserService.Api.Contracts.Dtos
{
    public record CreateUserRequest
    (
        string Username,
        string Email
    );
}
