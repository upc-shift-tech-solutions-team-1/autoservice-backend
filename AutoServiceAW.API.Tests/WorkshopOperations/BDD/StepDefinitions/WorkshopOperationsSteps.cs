
using System.Net;
using System.Net.Http.Json;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Interfaces.REST;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.Tests.WorkshopOperations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Reqnroll;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.Tests.WorkshopOperations.BDD.StepDefinitions;

[Binding]
public sealed class WorkshopOperationsSteps
{
    private readonly Mock<ITaskService> _taskServiceMock = new();
    private readonly Mock<IWorkOrderService> _workOrderServiceMock = new();
    private readonly Mock<IInventoryItemService> _inventoryServiceMock = new();
    private readonly Mock<IMechanicRepository> _mechanicRepositoryMock = new();
    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private bool _useRealTaskService;
    private WorkOrder? _workOrder;
    private WorkshopTask? _task;
    private HttpResponseMessage? _response;
    private WebApplication? _app;


    [Given("an active work order exists")]
    public void GivenAnActiveWorkOrderExists()
    {
        _workOrder = new WorkOrder(
            "WS-01",
            15,
            23,
            8,
            "Customer reported brake noise",
            "2026-10-25",
            500m
        );

        _workOrderServiceMock
            .Setup(service => service.GetByIdAsync(20))
            .ReturnsAsync(_workOrder);
    }

    private async System.Threading.Tasks.Task<HttpClient>
        CreateAdminClientAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthenticationHandler
            >("Test", _ => { });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(TasksController).Assembly);


        if (_useRealTaskService)
        {
            builder.Services.AddSingleton<ITaskRepository>(
                _taskRepositoryMock.Object
            );

            builder.Services.AddSingleton<IUnitOfWork>(
                _unitOfWorkMock.Object
            );

            builder.Services.AddScoped<ITaskService, TaskService>();
        }
        else
        {
            builder.Services.AddSingleton<ITaskService>(
                _taskServiceMock.Object
            );
        }

        builder.Services.AddSingleton(_workOrderServiceMock.Object);
        builder.Services.AddSingleton(_inventoryServiceMock.Object);
        builder.Services.AddSingleton(_mechanicRepositoryMock.Object);

        _app = builder.Build();

        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();

        return _app.GetTestClient();
    }

    [When("an administrator registers a task with its description, mechanic, status, and estimated time")]
    public async System.Threading.Tasks.Task
        WhenAnAdministratorRegistersATask()
    {
        _taskServiceMock
            .Setup(service => service.CreateAsync(
                It.IsAny<WorkshopTask>()))
            .ReturnsAsync((WorkshopTask task) =>
            {
                _task = task;
                return task;
            });

        var client = await CreateAdminClientAsync();

        var request = new CreateTaskResource(
            WorkOrderId: 20,
            MechanicId: 8,
            Description: "Replace brake pads",
            Priority: "HIGH",
            EstimatedTime: 90,
            LaborPrice: 250m,
            Parts: null
        );

        _response = await client.PostAsJsonAsync(
            "/api/v1/tasks",
            request
        );

        Assert.AreEqual(
            HttpStatusCode.Created,
            _response.StatusCode
        );
    }

    [Then("the task is associated with the work order")]
    public void ThenTheTaskIsAssociatedWithTheWorkOrder()
    {
        Assert.IsNotNull(
            _task,
            "The task should have been created."
        );

        Assert.AreEqual(
            20,
            _task.WorkOrderId,
            "The task must belong to the original work order."
        );

        _taskServiceMock.Verify(
            service => service.CreateAsync(
                It.Is<WorkshopTask>(
                    task => task.WorkOrderId == 20
                )
            ),
            Times.Once
        );
    }


    [Then("the task is saved with its assigned mechanic and estimated time")]
    public void ThenTheTaskIsSavedWithItsAssignedMechanicAndEstimatedTime()
    {
        Assert.IsNotNull(
            _task,
            "The task should have been created."
        );

        Assert.AreEqual(8, _task.MechanicId);
        Assert.AreEqual(90, _task.EstimatedTime);
        Assert.AreEqual("Replace brake pads", _task.Description);
        Assert.AreEqual("PENDING", _task.Status);

        _taskServiceMock.Verify(
            service => service.CreateAsync(
                It.Is<WorkshopTask>(task =>
                    task.WorkOrderId == 20 &&
                    task.MechanicId == 8 &&
                    task.EstimatedTime == 90 &&
                    task.Status == "PENDING"
                )
            ),
            Times.Once
        );
    }

    [Given("a maintenance task belongs to an active work order")]
    public void GivenAMaintenanceTaskBelongsToAnActiveWorkOrder()
    {
        GivenAnActiveWorkOrderExists();

        _task = new WorkshopTask(
            20,
            8,
            "Replace brake pads",
            "PENDING",
            "HIGH",
            90,
            250m
        );

        _taskServiceMock
            .Setup(service => service.GetByIdAsync(5))
            .ReturnsAsync(_task);

        _useRealTaskService = true;

        _taskRepositoryMock
            .Setup(repository => repository.FindByIdWithPartsAsync(5))
            .ReturnsAsync(_task);

        _unitOfWorkMock
            .Setup(unit => unit.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);

    }


    [When("an administrator or mechanic updates the task status and estimated time")]
    public async System.Threading.Tasks.Task
        WhenAnAdministratorUpdatesTheTask()
    {
        var client = await CreateAdminClientAsync();

        var request = new UpdateTaskResource(
            Description: "Replace brake pads",
            Status: "IN_PROGRESS",
            Priority: "HIGH",
            EstimatedTime: 120,
            LaborPrice: 250m,
            MechanicId: 8
        );

        _response = await client.PutAsJsonAsync(
            "/api/v1/tasks/5",
            request
        );

        Assert.AreEqual(
            HttpStatusCode.OK,
            _response.StatusCode
        );
    }
    [Then("the task reflects the new status and estimated time")]
    public void ThenTheTaskReflectsTheNewStatusAndEstimatedTime()
    {
        Assert.IsNotNull(
            _task,
            "The maintenance task should exist."
        );

        Assert.AreEqual(
            "IN_PROGRESS",
            _task.Status
        );

        Assert.AreEqual(
            120,
            _task.EstimatedTime
        );

        _taskRepositoryMock.Verify(
            repository => repository.Update(
                It.Is<WorkshopTask>(task =>
                    task.Status == "IN_PROGRESS" &&
                    task.EstimatedTime == 120
                )
            ),
            Times.Once
        );
    }

    [Then("the task remains associated with the same work order")]
    public void ThenTheTaskRemainsAssociatedWithTheSameWorkOrder()
    {
        Assert.IsNotNull(
            _task,
            "The maintenance task should exist."
        );

        Assert.AreEqual(
            20,
            _task.WorkOrderId,
            "The task must remain associated with its original work order."
        );

        _taskRepositoryMock.Verify(
            repository => repository.Update(
                It.Is<WorkshopTask>(task =>
                    task.WorkOrderId == 20 &&
                    task.Status == "IN_PROGRESS" &&
                    task.EstimatedTime == 120
                )
            ),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unit => unit.CompleteAsync(),
            Times.Once
        );
    }

    [AfterScenario]
    public async System.Threading.Tasks.Task CleanupAsync()
    {
        _response?.Dispose();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
}
