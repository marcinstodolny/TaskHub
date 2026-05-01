using System.Net;
using System.Net.Http.Json;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Contracts.Auth;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;
using TaskHub.Domain.Enums;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AuthTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Register_WithValidRequest_ShouldReturn201AndCreatedUser()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var request = new RegistrationRequest(" Register-Success-User ", "RegisterPass123!", " Register Success User ");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<RegistrationResponse>();

        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload!.UserId);
        Assert.Equal("register-success-user", payload.Username);
        Assert.Equal("Register Success User", payload.DisplayName);
        Assert.Equal("User", payload.Role);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturn400()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var request = new RegistrationRequest("duplicate-user", "RegisterPass123!", "Duplicate User");

        var firstResponse = await client.PostAsJsonAsync("/api/auth/register", request);
        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUsernameDifferentCasing_ShouldReturn400()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/api/auth/register", new RegistrationRequest(
            "Marcin",
            "RegisterPass123!",
            "Marcin"));
        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await client.PostAsJsonAsync("/api/auth/register", new RegistrationRequest(
            "MARCIN",
            "RegisterPass123!",
            "Marcin Upper"));

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithRegisteredUserAndValidPassword_ShouldReturn200AndToken()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var registrationRequest = new RegistrationRequest("login-success-user", "LoginPass123!", "Login Success User");
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registrationRequest);
        registerResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest(
            registrationRequest.Username,
            registrationRequest.Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task Login_WithRegisteredUserAndDifferentUsernameCasing_ShouldReturn200AndToken()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var registrationRequest = new RegistrationRequest("Marcin", "LoginPass123!", "Marcin");
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registrationRequest);
        registerResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest(
            "MARCIN",
            registrationRequest.Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task Login_WithRegisteredUserAndInvalidPassword_ShouldReturn401()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var registrationRequest = new RegistrationRequest("login-invalid-password-user", "LoginPass123!", "Login Invalid Password User");
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registrationRequest);
        registerResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/token", new TokenRequest(
            registrationRequest.Username,
            "WrongPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskLists_WithoutToken_ShouldReturn401()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();

        var response = await client.GetAsync("/api/TaskList?page=1&count=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskLists_WithValidToken_ShouldReturn200()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var createResponse = await client.PostAsJsonAsync("/api/TaskList", new CreateTaskListRequest("Secured list"));
        createResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/TaskList?page=1&count=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskListLightResponse>>();

        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Items);
    }

    [Fact]
    public async Task GetTaskListById_ForForeignOwner_ShouldReturn404()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsUserAsync(new CreateTaskListCommand("Foreign list"), "foreign-user");

        using var client = await fixture.CreateAuthorizedClientAsync();
        var response = await client.GetAsync($"/api/TaskList/{createListResult.Value.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTaskList_WithoutToken_ShouldReturn401()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/TaskList", new CreateTaskListRequest("Unauthorized list"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TaskItemEndpoints_WithoutToken_ShouldReturn401()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("Secured list"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Secured task",
            "Description",
            TaskPriority.Normal));

        using var client = fixture.ApiFactory.CreateClient();

        var getBoardResponse = await client.GetAsync($"/api/TaskItem/board/{createListResult.Value.Id}");
        var createResponse = await client.PostAsJsonAsync("/api/TaskItem", new CreateTaskItemRequest(
            createListResult.Value.Id,
            "Unauthorized task",
            "Description",
            TaskPriority.Normal));
        var updateStatusResponse = await client.PostAsJsonAsync($"/api/TaskItem/{createItemResult.Value}/status", new UpdateTaskStatusRequest(TaskStatus.Done));

        Assert.Equal(HttpStatusCode.Unauthorized, getBoardResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, updateStatusResponse.StatusCode);
    }
}
