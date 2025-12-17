using AssignmentService.Api.Contracts.Dtos;
using AssignmentService.Api.Contracts.Mappings;
using AssignmentService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AssignmentService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController(IAssignmentService assignmentService, IProjectServiceClient projectClient ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest assignmentRequest, CancellationToken ct)
        {
            var status = await projectClient.GetProjectStatusAsync(assignmentRequest.ProjectId, ct);

            if (status == HttpStatusCode.NotFound)
                return BadRequest("Project does not exist.");

            if (status != HttpStatusCode.OK)
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"ProjectService returned {((int)status)} while validating ProjectId.");

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
