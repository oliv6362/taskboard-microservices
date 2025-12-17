using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectService.Domain.Entities;
using ProjectService.Infrastructure.Data;

namespace ProjectService.Api.ContractTests;

/// <summary>
/// Handles provider state setup for consumer-driven contract tests.
/// This controller prepares the database in a known state before
/// Pact provider verification is executed.
/// This controller exists only for contract testing.
/// </summary>
[ApiController]
[Route("provider-states")]
public class ProviderStatesController(ProjectDbContext db) : ControllerBase
{
    private const string State_Project1Exists = "Project with ID 1 exists";
    private const string State_Project999Missing = "Project with ID 999 does not exist";

    private const int ProjectId_Exists = 1;
    private const int ProjectId_Missing = 999;

    private const string Set_IDENTITY_INSERT_On = "SET IDENTITY_INSERT [Project] ON;";
    private const string Set_IDENTITY_INSERT_Off = "SET IDENTITY_INSERT [Project] OFF;";

    public record ProviderState(string State);

    /// <summary>
    /// Configures the database to match the requested provider state.
    /// This endpoint is invoked automatically by Pact before each
    /// provider verification to ensure deterministic test conditions.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Setup([FromBody] ProviderState state, CancellationToken ct)
    {
        switch (state.State)
        {
            case State_Project1Exists:
                await db.Projects.ExecuteDeleteAsync(ct);

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync(Set_IDENTITY_INSERT_On, ct);

                    db.Projects.Add(new Project
                    {
                        ProjectId = ProjectId_Exists,
                        Name = "TaskBoard Project",
                        Description = "This is a Project about making a TaskBoard",
                        OwnerUserId = 1,
                        CreatedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync(ct);
                }
                finally
                {
                    await db.Database.ExecuteSqlRawAsync(Set_IDENTITY_INSERT_Off, ct);
                    await db.Database.CloseConnectionAsync();
                }

                return Ok();

            case State_Project999Missing:
                await db.Projects.Where(p => p.ProjectId == ProjectId_Missing).ExecuteDeleteAsync(ct);
                return Ok();

            default:
                return BadRequest($"Unknown provider state: {state.State}");
        }
    }
}
