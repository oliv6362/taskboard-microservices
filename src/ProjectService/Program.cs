using Microsoft.EntityFrameworkCore;
using ProjectService.Api.Extensions;
using ProjectService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProjectService(builder.Configuration);

if (builder.Environment.IsEnvironment("ContractTest"))
{
    builder.Services.AddContractTestDoubles();
}
else
{
    builder.Services.AddUserServiceClient(builder.Configuration);
}

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

/// Executes database migrations automatically when running in the ContractTest environment.
if (app.Environment.IsEnvironment("ContractTest"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

    var migrated = false;
    for (var i = 0; i < 10; i++)
    {
        try
        {
            logger.LogInformation("[ContractTest] Running DB migration (attempt {Attempt}/10)", i + 1);
            db.Database.Migrate();
            logger.LogInformation("[ContractTest] DB migrated");
            migrated = true;
            break;
        }
        catch (Exception ex) when (i < 9)
        {
            logger.LogWarning(ex, "[ContractTest] Migrate failed, retrying...");
            Thread.Sleep(500);
        }
    }

    if (!migrated)
        throw new Exception("[ContractTest] Database migration failed after retries.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

/// Enables HTTPS redirection for all environments except ContractTest.
if (!app.Environment.IsEnvironment("ContractTest"))
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
app.Run();
public partial class Program { }
