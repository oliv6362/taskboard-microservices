using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDb")));

builder.Services.AddScoped<IUserService, UserAppService>();

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
app.Run();
