using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUser(User user, CancellationToken ct);
        Task<User?> GetUserById(int id, CancellationToken ct);
    }
}
