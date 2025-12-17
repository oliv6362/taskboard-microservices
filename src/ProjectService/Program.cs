using Microsoft.EntityFrameworkCore;
using ProjectService.Application.Interfaces;
using ProjectService.Application.Services;
using ProjectService.Infrastructure.Data;
using ProjectService.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProjectDb")));

// Register ProjectService
builder.Services.AddScoped<IProjectService, ProjectAppService>();

// Configure RestClient for UserService
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(http =>
{
    var baseUrl = builder.Configuration["ServiceUrls:UserService"]
        ?? throw new InvalidOperationException("UserService URL not configured");

    http.BaseAddress = new Uri(baseUrl);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
app.Run();
public partial class Program { }
