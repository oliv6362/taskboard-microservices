using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts.Dtos;
using UserService.Api.Contracts.Mappings;
using UserService.Application.Interfaces;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest userRequest, CancellationToken ct)
        {
            var user = userRequest.ToEntity();
            var created = await userService.CreateUser(user, ct);
            return Ok(created);
        }
    

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id, CancellationToken ct)
        {   
            var user = await userService.GetUserById(id, ct);
            return Ok(user);
        }
    }
}
