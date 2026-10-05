using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using AutoServiceAW.API.InventoryManagement.Application.Internal;
using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Repositories;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using TaskPart = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.TaskPart;
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
public class TasksApiIntegrationTests
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

    /// <summary>
    /// Verifies that starting an approved task consumes the allocated stock
    /// through the Inventory Management application service.
    /// </summary>
    [TestMethod]
    public async Task StartApprovedTask_WithAllocatedParts_ShouldConsumeInventoryStock()
    {
        // Arrange
        const int inventoryItemId = 0;
        const int allocatedQuantity = 3;
        var inventoryItem = new InventoryItem(
            "Brake pad",
            "SPARE_PART",
            "Acme",
            80m,
            10,
            1,
            string.Empty,
            50m
        );
        var task = new WorkshopTask(
            12,
            34,
            "Replace brake pads",
            "PENDING",
            "HIGH",
            90,
            250m
        );
        task.AddPart(new TaskPart(5, inventoryItemId, "Brake pad", allocatedQuantity, 80m, 50m));
        task.PatchTechnicalData(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "APPROVED"
        );

        var inventoryRepositoryMock = new Mock<IInventoryItemRepository>();
        inventoryRepositoryMock
            .Setup(repository => repository.FindByIdAsync(
                inventoryItemId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(inventoryItem);

        var taskRepositoryMock = new Mock<ITaskRepository>();
        taskRepositoryMock
            .Setup(repository => repository.FindByIdWithPartsAsync(5))
            .ReturnsAsync(task);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TasksController).Assembly);
        builder.Services.AddSingleton<ITaskRepository>(taskRepositoryMock.Object);
        builder.Services.AddSingleton<IInventoryItemRepository>(inventoryRepositoryMock.Object);
        builder.Services.AddSingleton<IUnitOfWork>(unitOfWorkMock.Object);
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();
        builder.Services.AddSingleton<IWorkOrderService>(
            new Mock<IWorkOrderService>().Object
        );
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
        var request = new PatchTaskResource(
            "IN_PROGRESS",
            null,
            null,
            null,
            null,
            null
        );

        // Act
        var response = await client.PatchAsJsonAsync(
            "/api/v1/tasks/5",
            request
        );

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(10 - allocatedQuantity, inventoryItem.Stock);
        Assert.AreEqual("IN_PROGRESS", task.Status);
        inventoryRepositoryMock.Verify(
            repository => repository.Update(inventoryItem),
            Times.Once
        );
        taskRepositoryMock.Verify(
            repository => repository.Update(task),
            Times.Once
        );
        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Exactly(2)
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
