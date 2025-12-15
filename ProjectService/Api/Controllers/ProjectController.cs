using Microsoft.AspNetCore.Mvc;
using ProjectService.Api.Contracts.Dtos;
using ProjectService.Api.Contracts.Mappings;
using ProjectService.Application.Interfaces;
using RestSharp;
using System.Net;

namespace ProjectService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController(IProjectService projectService) : ControllerBase
    {
        private static readonly RestClient restClient = new RestClient("http://user-service:8080");
        
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest projectRequest, CancellationToken ct)
        {
            //Fire-and-Hope
            var request = new RestRequest($"/api/User/{projectRequest.OwnerUserId}", Method.Get)
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            RestResponse response;
            response = await restClient.ExecuteAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return BadRequest("Owner user does not exist.");

            if (!response.IsSuccessful)
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"UserService returned {((int)response.StatusCode)} while validating OwnerUserId.");

            var project = projectRequest.ToEntity();
            var created = await projectService.CreateProject(project, ct);
            return Ok(created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProjectById(int id, CancellationToken ct)
        {
            var project = await projectService.GetProjectById(id, ct);
            return project is null ? NotFound() : Ok(project);
        }
    }
}
