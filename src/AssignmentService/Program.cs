using AssignmentService.Application.Interfaces;
using AssignmentService.Application.Services;
using AssignmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AssignmentService.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<AssignmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AssignmentDb")));

// Register AssignmentService
builder.Services.AddScoped<IAssignmentService, AssignmentAppService>();

// Configure RestClient for ProjectService
builder.Services.AddHttpClient<IProjectServiceClient, ProjectServiceClient>(http =>
{
    var baseUrl = builder.Configuration["ServiceUrls:ProjectService"]
        ?? throw new InvalidOperationException("ProjectService URL not configured");

    http.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (app.Environment.IsEnvironment("ContractTest"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AssignmentDbContext>();

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

if (!app.Environment.IsEnvironment("ContractTest"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();

public partial class Program { }