using Microsoft.EntityFrameworkCore;
using ProjectService.Application.Interfaces;
using ProjectService.Application.Services;
using ProjectService.Infrastructure.Clients;
using ProjectService.Infrastructure.Data;
using ProjectService.Api.ContractTests.Fakes;

namespace ProjectService.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectService(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ProjectDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("ProjectDb")));

        services.AddScoped<IProjectService, ProjectAppService>();
        return services;
    }

    public static IServiceCollection AddUserServiceClient(this IServiceCollection services, IConfiguration config)
    {
        var baseUrl = config["ServiceUrls:UserService"]
                      ?? throw new InvalidOperationException("UserService URL not configured");

        services.AddHttpClient<IUserServiceClient, UserServiceClient>(http =>
        {
            http.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }

    public static IServiceCollection AddContractTestDoubles(this IServiceCollection services)
    {
        // Override external dependency with deterministic fake
        services.AddSingleton<IUserServiceClient, FakeUserServiceClient>();
        return services;
    }
}
