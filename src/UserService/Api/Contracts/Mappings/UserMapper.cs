using System.Runtime.CompilerServices;
using UserService.Api.Contracts.Dtos;
using UserService.Domain.Entities;

namespace UserService.Api.Contracts.Mappings
{
    public static class UserMapper
    {
        public static User ToEntity (this CreateUserRequest dto) =>
            new()
            {             
                Username = dto.Username,
                Email = dto.Email
            };
    }
}
