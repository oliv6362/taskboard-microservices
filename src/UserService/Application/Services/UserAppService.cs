using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Application.Services
{
    public class UserAppService(UserDbContext db) : IUserService
    {
        public async Task<User> CreateUser(User user, CancellationToken ct)
        {
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            return user;
        }

        public async Task<User?> GetUserById(int id, CancellationToken ct) =>
            await db.Users.FindAsync([id], ct);
    }
}
