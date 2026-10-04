using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Interfaces.REST;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WorkshopTask = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Acceptance;

[TestClass]
public class WorkOrderTaskFlowAcceptanceTests
{
    [TestMethod]
    public async System.Threading.Tasks.Task CreateOrder_AssignAndCompleteTask_ShouldExposeFullProgress()
    {
        // Arrange: in-memory state behind the real services and controllers.
        WorkOrder? storedOrder = null;
        WorkshopTask? storedTask = null;

        var workOrderRepository = new Mock<IWorkOrderRepository>();
        var taskRepository = new Mock<ITaskRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var inventoryService = new Mock<IInventoryItemService>();

        workOrderRepository
            .Setup(value => value.AddAsync(
                It.IsAny<WorkOrder>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<WorkOrder, CancellationToken>(
                (order, _) => storedOrder = order
            )
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        taskRepository
            .Setup(value => value.AddAsync(
                It.IsAny<WorkshopTask>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<WorkshopTask, CancellationToken>(
                (task, _) => storedTask = task
            )
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        taskRepository
            .Setup(value => value.FindByIdWithPartsAsync(0))
            .ReturnsAsync(() => storedTask);

        taskRepository
            .Setup(value => value.FindByWorkOrderIdAsync(0))
            .ReturnsAsync(() =>
                storedTask == null
                    ? Array.Empty<WorkshopTask>()
                    : new[] { storedTask }
            );

        unitOfWork
            .Setup(value => value.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthenticationHandler
            >("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(WorkOrdersController).Assembly);

        builder.Services.AddSingleton(workOrderRepository.Object);
        builder.Services.AddSingleton(taskRepository.Object);
        builder.Services.AddSingleton(unitOfWork.Object);
        builder.Services.AddSingleton(inventoryService.Object);
        builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
        builder.Services.AddScoped<ITaskService, TaskService>();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();

        // Act 1: create an order from the reported customer problem.
        var orderResponse = await client.PostAsJsonAsync(
            "/api/v1/workorders",
            new CreateWorkOrderResource(
                15,
                23,
                8,
                "Brake noise reported by the customer",
                "2026-10-05"
            )
        );

        // Act 2: create and assign an approved repair task.
        var taskResponse = await client.PostAsJsonAsync(
            "/api/v1/tasks",
            new CreateTaskResource(
                0,
                8,
                "Replace front brake pads",
                "HIGH",
                90,
                180m,
                new List<CreateTaskPartResource>(),
                75m,
                "Front pads are worn",
                "APPROVED"
            )
        );

        // Act 3: update the task from pending to in progress and completed.
        var startResponse = await client.PatchAsJsonAsync(
            "/api/v1/tasks/0",
            new PatchTaskResource(
                "IN_PROGRESS",
                "Front pads are below the safe thickness",
                "The front brake pads must be replaced",
                "Inspect the rotors before delivery",
                "evidence/brakes-before.jpg",
                "APPROVED"
            )
        );

        var completeResponse = await client.PatchAsJsonAsync(
            "/api/v1/tasks/0",
            new PatchTaskResource(
                "COMPLETED",
                null,
                null,
                null,
                "evidence/brakes-after.jpg",
                null
            )
        );

        // Act 4: retrieve tasks as the Web/Mobile clients do to show progress.
        var tasksResponse = await client.GetAsync(
            "/api/v1/tasks?workOrderId=0"
        );

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, orderResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.Created, taskResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, startResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, completeResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, tasksResponse.StatusCode);
        Assert.IsNotNull(storedOrder);
        Assert.IsNotNull(storedTask);
        Assert.AreEqual(8, storedTask.MechanicId);
        Assert.AreEqual("COMPLETED", storedTask.Status);

        var tasksJson = await tasksResponse.Content
            .ReadFromJsonAsync<JsonElement>();
        var total = tasksJson.GetArrayLength();
        var completed = tasksJson
            .EnumerateArray()
            .Count(task =>
                task.GetProperty("status").GetString() == "COMPLETED"
            );
        var progress = total == 0 ? 0 : completed * 100 / total;

        Assert.AreEqual(1, total);
        Assert.AreEqual(100, progress);
    }
}
