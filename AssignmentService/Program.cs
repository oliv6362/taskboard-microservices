using AssignmentService.Application.Interfaces;
using AssignmentService.Application.Services;
using AssignmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RestSharp;

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
builder.Services.AddSingleton(sp =>
{
    var baseUrl = builder.Configuration["ServiceUrls:ProjectService"]
        ?? throw new InvalidOperationException("ProjectService URL not configured");

    return new RestClient(baseUrl);
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
