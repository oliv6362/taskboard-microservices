using AssignmentService.Api.Contracts.Dtos;
using AssignmentService.Api.Contracts.Mappings;
using AssignmentService.Application.Interfaces;
using AssignmentService.Domain.Entities;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest assignmentRequest, CancellationToken ct)
        {
            var assignment = assignmentRequest.ToEntity();
            var created = await assignmentService.CreateAssignment(assignment, ct);
            return Ok(created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAssignmentById(int id, CancellationToken ct)
        {
            var assignment = await assignmentService.GetAssignmentById(id, ct);
            return Ok(assignment);
        }

        [HttpPut("status/{id:int}")]
        public async Task<IActionResult> UpdateAssignmentStatus(int id, [FromBody] UpdateAssignmentRequest assignmentRequest, CancellationToken ct)
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
