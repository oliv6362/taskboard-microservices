using System.Net;
using System.Net.Http.Json;
using Taskboard.E2ETests.Fixtures;

namespace Taskboard.E2ETests;

/// <summary>
/// End-to-end (E2E) tests that validate a "user journey" across multiple Taskboard microservices.
///
/// This test class uses <see cref="TaskboardE2eFixture"/> to spin up and expose the running services,
/// then exercises the system through real HTTP calls to ensure:
/// - A user can be created in UserService
/// - A project can be created in ProjectService using the created user
/// - An assignment can be created in AssignmentService using the created project
/// - The created assignment can be retrieved and matches the expected persisted data
///
/// The goal is to verify that the services work together correctly, and not just in isolation.
/// </summary>
public class UserJourneyE2eTests : IClassFixture<TaskboardE2eFixture>
{
    private readonly TaskboardE2eFixture _fx;

    private const string UsersRoute = "/api/User";
    private const string ProjectsRoute = "/api/Project";
    private const string AssignmentsRoute = "/api/Assignment";

    public UserJourneyE2eTests(TaskboardE2eFixture fx) => _fx = fx;

    /// <summary>
    /// Verifies the full user journey works across the system:
    /// 1) Create a user (UserService)
    /// 2) Create a project owned by that user (ProjectService depends on UserService)
    /// 3) Create an assignment for that project (AssignmentService depends on ProjectService)
    /// 4) Fetch the created assignment by ID and validate the persisted data
    /// </summary>
    [Fact]
    public async Task UserJourney_CreateUser_CreateProject_CreateAssignment_Works()
    {
        // Arrange
        using var userHttp = new HttpClient { BaseAddress = _fx.UserServiceBaseUrl };
        using var projectHttp = new HttpClient { BaseAddress = _fx.ProjectServiceBaseUrl };
        using var assignmentHttp = new HttpClient { BaseAddress = _fx.AssignmentServiceBaseUrl };

        // 1) Act + Assert 1: Create user
        var user = await PostAndRead<UserResponse>(
            userHttp,
            UsersRoute,
            new CreateUserRequest("John", "JohnDoe@gmail.com"),
            expectedStatus: HttpStatusCode.Created);

        Assert.True(user.UserId > 0);

        // 2) Act + Assert 2: Create project (depends on UserService)
        var project = await PostAndRead<ProjectResponse>(
            projectHttp,
            ProjectsRoute,
            new CreateProjectRequest("My Project", "E2E project", user.UserId),
            expectedStatus: HttpStatusCode.Created);

        Assert.True(project.ProjectId > 0);

        // 3) Act + Assert 3: Create assignment (depends on ProjectService)
        var assignment = await PostAndRead<AssignmentResponse>(
            assignmentHttp,
            AssignmentsRoute,
            new CreateAssignmentRequest("First task", "E2E assignment", project.ProjectId),
            expectedStatus: HttpStatusCode.Created);

        Assert.True(assignment.AssignmentId > 0);
        Assert.Equal(project.ProjectId, assignment.ProjectId);
        Assert.Equal(user.UserId, project.OwnerUserId);

        // 4) Verify persisted via GET
        var getRes = await assignmentHttp.GetAsync($"{AssignmentsRoute}/{assignment.AssignmentId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var fetched = await getRes.Content.ReadFromJsonAsync<AssignmentResponse>();
        Assert.NotNull(fetched);

        Assert.Equal("First task", fetched!.Title);
        Assert.Equal(project.ProjectId, fetched.ProjectId);
    }

    /// <summary>
    /// Helper that POSTs a JSON request body to a route, asserts the expected HTTP status,
    /// then deserializes and returns the JSON response payload.
    /// </summary>
    private static async Task<TResponse> PostAndRead<TResponse>(HttpClient http, string route, object body, HttpStatusCode expectedStatus)
    {
        var res = await http.PostAsJsonAsync(route, body);
        Assert.Equal(expectedStatus, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<TResponse>();
        Assert.NotNull(payload);

        return payload!;
    }

    /// <summary>
    /// Request DTOs for creating a user, project, and assignment in their respective services.
    /// </summary>
    public record CreateUserRequest(string Username, string Email);
    public record CreateProjectRequest(string Name, string Description, int OwnerUserId);
    public record CreateAssignmentRequest(string Title, string Description, int ProjectId);

    /// <summary>
    /// Response DTOs returned by UserService, ProjectService, and AssignmentService after creating or retrieving a user, project, or assignment.
    /// </summary>
    public record UserResponse(int UserId, string Username, string Email, DateTime CreatedAt);
    public record ProjectResponse(int ProjectId, string Name, string? Description, int OwnerUserId, DateTime CreatedAt);
    public record AssignmentResponse(int AssignmentId, string Title, string? Description, int Status, int ProjectId, DateTime CreatedAt, DateTime? UpdatedAt);
}
    