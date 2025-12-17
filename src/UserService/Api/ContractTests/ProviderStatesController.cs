using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Infrastructure.Data;

namespace UserService.Api.ContractTests;

/// <summary>
/// Handles provider state setup for consumer-driven contract tests.
/// This controller prepares the database in a known state before
/// Pact provider verification is executed.
/// This controller exists only for contract testing.
/// </summary>
[ApiController]
[Route("provider-states")]
public class ProviderStatesController(UserDbContext db) : ControllerBase
{
    private const string State_User1Exists = "User with ID 1 exists";
    private const string State_User999Missing = "User with ID 999 does not exist";

    private const int UserId_Exists = 1;
    private const int UserId_Missing = 999;

    private const string Set_IDENTITY_INSERT_On = "SET IDENTITY_INSERT [User] ON;";
    private const string Set_IDENTITY_INSERT_Off = "SET IDENTITY_INSERT [User] OFF;";

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
            case State_User1Exists:
                await db.Users.ExecuteDeleteAsync(ct);

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync(Set_IDENTITY_INSERT_On, ct);

                    db.Users.Add(new User
                    {
                        UserId = UserId_Exists,
                        Username = "John",
                        Email = "JohnDoe@gmail.com",
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

            case State_User999Missing:
                await db.Users.Where(u => u.UserId == UserId_Missing).ExecuteDeleteAsync(ct);
                return Ok();

            default:
                return BadRequest($"Unknown provider state: {state.State}");
        }
    }
}
