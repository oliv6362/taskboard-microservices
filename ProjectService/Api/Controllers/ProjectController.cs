using Microsoft.AspNetCore.Mvc;
using ProjectService.Api.Contracts.Dtos;
using ProjectService.Application.Interfaces;
using ProjectService.Api.Contracts.Mappings;

namespace ProjectService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController(IProjectService projectService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest projectRequest, CancellationToken ct)
        {
            var project = projectRequest.ToEntity();
            var created = await projectService.CreateProject(project, ct);
            return Ok(created);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectById(int id, CancellationToken ct)
        {
            var project = await projectService.GetProjectById(id, ct);
            return Ok();
        }
    }
}
