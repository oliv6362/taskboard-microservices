using Microsoft.AspNetCore.Mvc;
using ProjectService.Api.Contracts.Dtos;
using ProjectService.Api.Contracts.Mappings;
using ProjectService.Application.Interfaces;
using System.Net;

namespace ProjectService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController(IProjectService projectService, IUserServiceClient userClient) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest projectRequest, CancellationToken ct)
        {
            var status = await userClient.GetUserStatusAsync(projectRequest.OwnerUserId, ct);

            if (status == HttpStatusCode.NotFound)
                return BadRequest("Owner user does not exist.");

            if (status != HttpStatusCode.OK)
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"UserService returned {((int)status)} while validating OwnerUserId.");

            var project = projectRequest.ToEntity();
            var created = await projectService.CreateProject(project, ct);
            return CreatedAtAction(nameof(GetProjectById), new { id = created.ProjectId }, created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProjectById(int id, CancellationToken ct)
        {
            var project = await projectService.GetProjectById(id, ct);
            return project is null ? NotFound() : Ok(project);
        }
    }
}
