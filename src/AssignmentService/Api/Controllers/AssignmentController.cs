using AssignmentService.Api.Contracts.Dtos;
using AssignmentService.Api.Contracts.Mappings;
using AssignmentService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Net;

namespace AssignmentService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController(IAssignmentService assignmentService, RestClient restClient ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest assignmentRequest, CancellationToken ct)
        {
            //Fire-and-Hope
            var request = new RestRequest($"/api/Project/{assignmentRequest.ProjectId}", Method.Get)
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            RestResponse response;
            response = await restClient.ExecuteAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return BadRequest("Project does not exist.");

            if (!response.IsSuccessful)
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"ProjectService returned {((int)response.StatusCode)} while validating ProjectId.");

            var assignment = assignmentRequest.ToEntity();
            var created = await assignmentService.CreateAssignment(assignment, ct);
            return CreatedAtAction(nameof(GetAssignmentById), new { id = created.AssignmentId }, created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAssignmentById(int id, CancellationToken ct)
        {
            var assignment = await assignmentService.GetAssignmentById(id, ct);
            return assignment is null ? NotFound() : Ok(assignment);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateAssignmentStatus(int id, [FromBody] UpdateAssignmentStatusRequest assignmentRequest, CancellationToken ct)
        {
            var assignment = await assignmentService.GetAssignmentById(id, ct);
            if (assignment is null)
                return NotFound();

            assignment.ApplyUpdate(assignmentRequest);

            await assignmentService.UpdateAssignmentStatus(assignment, ct);

            return Ok(assignment);
        }
    }
}
