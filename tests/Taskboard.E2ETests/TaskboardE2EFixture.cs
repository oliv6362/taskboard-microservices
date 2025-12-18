using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.MsSql;
using DotNetEnv;

namespace Taskboard.E2ETests.Fixtures;


/// <summary>
/// End-to-end test fixture responsible for provisioning and managing
/// the full Taskboard microservice environment using Testcontainers.
///
/// This fixture spins up:
/// - A dedicated Docker network
/// - Three SQL Server containers (User, Project, Assignment)
/// - Three microservice containers wired together via internal DNS
///
/// The fixture exposes base URLs for each service so tests can execute
/// real HTTP workflows across all components.
///
/// Containers are started once before the test suite executes and disposed afterwards.
/// </summary>
public sealed class TaskboardE2eFixture : IAsyncLifetime
{
    private const ushort ServicePort = 8080;

    // Docker network
    private readonly INetwork _network;

    // Databases
    private readonly MsSqlContainer _userDb;
    private readonly MsSqlContainer _projectDb;
    private readonly MsSqlContainer _assignmentDb;

    private readonly string _saUser;
    private readonly string _saPassword;

    // Services
    private readonly IContainer _userService;
    private readonly IContainer _projectService;
    private readonly IContainer _assignmentService;

    public Uri UserServiceBaseUrl { get; private set; } = default!;
    public Uri ProjectServiceBaseUrl { get; private set; } = default!;
    public Uri AssignmentServiceBaseUrl { get; private set; } = default!;


    /// <summary>
    /// Initializes the fixture configuration by loading environment variables,
    /// creating the Docker network, and defining all database and service containers.
    /// </summary>
    public TaskboardE2eFixture()
    {
        Env.Load();

        _saUser = Environment.GetEnvironmentVariable("SA_USERNAME")
            ?? throw new InvalidOperationException("SA_USERNAME not set");

        _saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD")
            ?? throw new InvalidOperationException("SA_PASSWORD not set");

        _network = new NetworkBuilder()
            .WithName($"taskboard-e2e-{Guid.NewGuid():N}")
            .Build();

        // DBs 
        _userDb = CreateSqlServer("user-db");
        _projectDb = CreateSqlServer("project-db");
        _assignmentDb = CreateSqlServer("assignment-db");

        // Services
        _userService = CreateService(
            image: "user-service",
            serviceAlias: "user-service",
            dbConnEnvKey: "ConnectionStrings__UserDb",
            dbConnValue: BuildDbConn("user-db", "UserDb"));

        _projectService = CreateService(
            image: "project-service",
            serviceAlias: "project-service",
            dbConnEnvKey: "ConnectionStrings__ProjectDb",
            dbConnValue: BuildDbConn("project-db", "ProjectDb"),
            extraEnv: new Dictionary<string, string>
            {
                ["ServiceUrls__UserService"] = $"http://user-service:{ServicePort}"
            });

        _assignmentService = CreateService(
            image: "assignment-service",
            serviceAlias: "assignment-service",
            dbConnEnvKey: "ConnectionStrings__AssignmentDb",
            dbConnValue: BuildDbConn("assignment-db", "AssignmentDb"),
            extraEnv: new Dictionary<string, string>
            {
                ["ServiceUrls__ProjectService"] = $"http://project-service:{ServicePort}"
            });
    }

    /// <summary>
    /// Starts the Docker network, database containers, and service containers
    /// in the correct dependency order.
    ///
    /// Once all services are healthy, the base URLs are resolved and exposed to the test suite.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _network.CreateAsync();

        // Start DBs first
        await _userDb.StartAsync();
        await _projectDb.StartAsync();
        await _assignmentDb.StartAsync();

        // Then services
        await _userService.StartAsync();
        await _projectService.StartAsync();
        await _assignmentService.StartAsync();

        // Expose base urls to the tests
        UserServiceBaseUrl = GetBaseUrl(_userService);
        ProjectServiceBaseUrl = GetBaseUrl(_projectService);
        AssignmentServiceBaseUrl = GetBaseUrl(_assignmentService);
    }

    /// <summary>
    /// Disposes all containers and the Docker network in reverse dependency order.
    /// Services are stopped before their databases to ensure clean shutdown
    /// </summary>
    public async Task DisposeAsync()
    {
        // Stop services first
        await _assignmentService.DisposeAsync();
        await _projectService.DisposeAsync();
        await _userService.DisposeAsync();

        // Then DBs
        await _assignmentDb.DisposeAsync();
        await _projectDb.DisposeAsync();
        await _userDb.DisposeAsync();

        await _network.DeleteAsync();
        await _network.DisposeAsync();
    }

    /// <summary>
    /// Creates a SQL Server container attached to the shared Docker network
    /// </summary>
    private MsSqlContainer CreateSqlServer(string networkAlias) =>
        new MsSqlBuilder()
            .WithNetwork(_network)
            .WithNetworkAliases(networkAlias)
            .WithPassword(_saPassword)
            .Build();

    /// <summary>
    /// Creates a microservice container configured with networking,
    /// environment variables, port bindings, and a health check.
    /// </summary>
    private IContainer CreateService(
        string image,
        string serviceAlias,
        string dbConnEnvKey,
        string dbConnValue,
        IDictionary<string, string>? extraEnv = null)
    {
        var builder = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(_network)
            .WithNetworkAliases(serviceAlias)
            .WithEnvironment("ASPNETCORE_URLS", $"http://+:{ServicePort}")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "ContractTest")
            .WithEnvironment(dbConnEnvKey, dbConnValue)
            .WithPortBinding(hostPort: 0, containerPort: ServicePort)
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r =>
                    r.ForPort(ServicePort)
                     .ForPath("/health")
                     .ForStatusCode(HttpStatusCode.OK)));

        if (extraEnv is not null)
        {
            foreach (var (key, value) in extraEnv)
                builder = builder.WithEnvironment(key, value);
        }

        return builder.Build();
    }

    /// <summary>
    /// Resolves the base URL for a container by mapping its internal service port to a random host port.
    /// </summary>
    private static Uri GetBaseUrl(IContainer serviceContainer)
    {
        var port = serviceContainer.GetMappedPublicPort(ServicePort);
        return new Uri($"http://localhost:{port}");
    }

    /// <summary>
    /// Builds a SQL Server connection string that targets a database container
    /// </summary>
    private string BuildDbConn(string serverAlias, string database) =>
        $"Server={serverAlias};Database={database};User ID={_saUser};Password={_saPassword};" +
        "TrustServerCertificate=True;Encrypt=False;";
}
