using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Interfaces.REST;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Encodings.Web;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Interfaces.REST;

[TestClass]
public class TasksIntegrationTests
{
    /// <summary>
    /// Verifies that posting a valid task request returns HTTP 201, persists
    /// the task with the expected values, and completes the unit of work.
    /// </summary>
    [TestMethod]
    public async Task CreateTask_WithValidRequest_ShouldReturnCreatedTaskAndPersistIt()
    {
        // Arrange
        WorkshopTask? persistedTask = null;
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        taskRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<WorkshopTask>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<WorkshopTask, CancellationToken>(
                (task, _) => persistedTask = task
            )
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TasksController).Assembly);
        builder.Services.AddSingleton<ITaskRepository>(
            taskRepositoryMock.Object
        );
        builder.Services.AddSingleton<IUnitOfWork>(
            unitOfWorkMock.Object
        );
        builder.Services.AddSingleton<IInventoryItemService>(
            new Mock<IInventoryItemService>().Object
        );
        builder.Services.AddSingleton<IWorkOrderService>(
            new Mock<IWorkOrderService>().Object
        );
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                "Test",
                _ => { }
            );
        builder.Services.AddAuthorization();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();
        var request = new CreateTaskResource(
            12,
            34,
            "Replace brake pads",
            "HIGH",
            90,
            250m,
            null
        );

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/tasks",
            request
        );

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(persistedTask);
        Assert.AreEqual(12, persistedTask.WorkOrderId);
        Assert.AreEqual(34, persistedTask.MechanicId);
        Assert.AreEqual("Replace brake pads", persistedTask.Description);
        Assert.AreEqual("PENDING", persistedTask.Status);
        Assert.AreEqual("HIGH", persistedTask.Priority);
        Assert.AreEqual(90, persistedTask.EstimatedTime);

        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(
            "PENDING",
            responseBody.GetProperty("status").GetString()
        );
        taskRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<WorkshopTask>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
    }
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(
    options,
    logger,
    encoder
)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "test-user")],
            Scheme.Name
        );
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
