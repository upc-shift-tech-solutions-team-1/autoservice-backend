
using System.Net;
using System.Net.Http.Json;
using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Interfaces.REST;
using AutoServiceAW.API.Tests.WorkshopOperations.BDD.Support;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using Moq;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

using AutoServiceAW.API.PublicTracking.Application.Internal;
using Reqnroll;

namespace AutoServiceAW.API.Tests.WorkshopOperations.BDD.StepDefinitions;

[Binding]
public sealed class WorkOrderTaskLifecycleSteps
{

    private readonly Mock<ITaskService> _taskServiceMock = new();
    private readonly Mock<IWorkOrderService> _workOrderServiceMock = new();
    private readonly Mock<IInventoryItemService> _inventoryServiceMock = new();
    private readonly Mock<IMechanicRepository> _mechanicRepositoryMock = new();

    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private WorkshopTask? _task;
    private WorkOrder? _workOrder;
    private Mechanic? _mechanic;
    private HttpResponseMessage? _response;
    private WebApplication? _app;
    private bool _useRealTaskService;

    private void ConfigureRealTaskService(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IWorkOrderRepository>(
            Mock.Of<IWorkOrderRepository>()
        );

        builder.Services.AddSingleton<ITaskRepository>(
            _taskRepositoryMock.Object
        );

        builder.Services.AddSingleton<IUnitOfWork>(
            _unitOfWorkMock.Object
        );

        builder.Services.AddScoped<ITaskService, TaskService>();
    }


    private async System.Threading.Tasks.Task<HttpClient>
        CreateApprovedTaskClientAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<
                AuthenticationSchemeOptions,
                MechanicBddAuthenticationHandler
            >("Test", _ => { });

        builder.Services.AddAuthorization();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TasksController).Assembly);

        ConfigureRealTaskService(builder);

        builder.Services.AddSingleton<IWorkOrderService>(
            _workOrderServiceMock.Object
        );

        builder.Services.AddSingleton<IInventoryItemService>(
            _inventoryServiceMock.Object
        );

        builder.Services.AddSingleton<IMechanicRepository>(
            _mechanicRepositoryMock.Object
        );

        _app = builder.Build();

        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();

        return _app.GetTestClient();
    }

    [Given("a work order exists for a customer reported vehicle problem")]
    public void GivenAWorkOrderExists()
    {
        // Arrange: crear una orden perteneciente al taller WS-01.
        _workOrder = new WorkOrder(
            "WS-01",
            15,
            23,
            8,
            "Customer reported brake noise",
            "2026-10-25",
            500m
        );

        // Simular la consulta de la orden desde el controlador.
        _workOrderServiceMock
            .Setup(service => service.GetByIdAsync(20))
            .ReturnsAsync(_workOrder);
    }



    [Given("a pending task has not been approved by an administrator")]
    public void GivenAnUnapprovedTaskExists()
    {
        // Arrange: tarea asignada al mecánico 8.
        _task = new WorkshopTask(
            20,
            8,
            "Replace brake pads",
            "PENDING",
            "HIGH",
            90,
            250m
        );

        // La tarea no tiene aprobación administrativa.
        _task.PatchTechnicalData(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "PENDING"
        );

        _taskServiceMock
            .Setup(service => service.GetByIdAsync(5))
            .ReturnsAsync(_task);

        // Mecánico del taller WS-01.
        _mechanic = new Mechanic(
            "Test Mechanic",
            "Brakes",
            5,
            "mecanico@test.com",
            "WS-01",
            "test-password"
        );

        typeof(Mechanic)
            .GetProperty("Id")!
            .SetValue(_mechanic, 8);

        _mechanicRepositoryMock
            .Setup(repository =>
                repository.FindByEmailAsync("mecanico@test.com"))
            .ReturnsAsync(_mechanic);
    }



    [Given("an approved task is assigned to a mechanic")]
    public void GivenAnApprovedTaskIsAssignedToAMechanic()
    {
        // Arrange: crear una tarea pendiente de ejecución.
        _task = new WorkshopTask(
            20,
            8,
            "Replace brake pads",
            "PENDING",
            "HIGH",
            90,
            250m
        );

        // Registrar la aprobación del administrador.
        _task.PatchTechnicalData(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "APPROVED"
        );

        _taskServiceMock
            .Setup(service => service.GetByIdAsync(5))
            .ReturnsAsync(_task);

        // Comprobar el estado inicial del escenario.
        Assert.AreEqual("PENDING", _task.Status);
        Assert.AreEqual("APPROVED", _task.AdminReviewStatus);

        Assert.AreEqual(
            0m,
            TrackingSummaryFactory.CalculateProgress(
                new[] { _task }
            )
        );

        // Configurar el repositorio utilizado por TaskService.
        _taskRepositoryMock
            .Setup(repository => repository.FindByIdWithPartsAsync(5))
            .ReturnsAsync(() => _task);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _useRealTaskService = true;

        // Preparar al mecánico asignado a la tarea.
        _mechanic = new Mechanic(
            "Test Mechanic",
            "Brakes",
            5,
            "mecanico@test.com",
            "WS-01",
            "test-password"
        );

        typeof(Mechanic)
            .GetProperty("Id")!
            .SetValue(_mechanic, 8);

        _mechanicRepositoryMock
            .Setup(repository =>
                repository.FindByEmailAsync("mecanico@test.com"))
            .ReturnsAsync(_mechanic);


    }


    [When("the mechanic starts and completes the assigned task")]
    public async System.Threading.Tasks.Task
        WhenTheMechanicStartsAndCompletesTheAssignedTask()
    {
        Assert.IsNotNull(_task);

        using var client = await CreateApprovedTaskClientAsync();

        // Primero, el mecánico inicia la tarea aprobada.
        var startRequest = new PatchTaskResource(
            "IN_PROGRESS",
            null, null, null, null, null
        );

        using var startResponse = await client.PatchAsJsonAsync(
            "/api/v1/tasks/5",
            startRequest
        );

        Assert.AreEqual(
            HttpStatusCode.OK,
            startResponse.StatusCode,
            "La tarea aprobada debe poder iniciarse."
        );

        Assert.AreEqual("IN_PROGRESS", _task.Status);

        // Después, el mecánico completa la tarea.
        var completeRequest = new PatchTaskResource(
            "COMPLETED",
            null, null, null, null, null
        );

        using var completeResponse = await client.PatchAsJsonAsync(
            "/api/v1/tasks/5",
            completeRequest
        );

        Assert.AreEqual(
            HttpStatusCode.OK,
            completeResponse.StatusCode,
            "La tarea iniciada debe poder completarse."
        );
    }


    [When("the mechanic attempts to start the task")]
    public async System.Threading.Tasks.Task WhenTheMechanicAttemptsToStartTheTask()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<
                AuthenticationSchemeOptions,
                MechanicBddAuthenticationHandler
            >("Test", _ => { });

        builder.Services.AddAuthorization();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TasksController).Assembly);

        if (_useRealTaskService)
        {
            ConfigureRealTaskService(builder);
        }
        else
        {
            builder.Services.AddSingleton<ITaskService>(
                _taskServiceMock.Object
            );
        }

        builder.Services.AddSingleton<IWorkOrderService>(
            _workOrderServiceMock.Object
        );

        builder.Services.AddSingleton<IInventoryItemService>(
            _inventoryServiceMock.Object
        );

        builder.Services.AddSingleton<IMechanicRepository>(
            _mechanicRepositoryMock.Object
        );

        _app = builder.Build();
        var app = _app;

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

        _response = await client.PatchAsJsonAsync(
            "/api/v1/tasks/5",
            request
        );
    }



    [Then("the request should be rejected")]
    public async System.Threading.Tasks.Task ThenTheRequestShouldBeRejected()
    {
        // Assert: el servidor debe rechazar la solicitud.
        Assert.IsNotNull(_response);

        Assert.AreEqual(
            HttpStatusCode.BadRequest,
            _response.StatusCode
        );

        // Comprobar que el rechazo se debe a la falta de aprobación.
        var responseBody = await _response.Content.ReadAsStringAsync();

        StringAssert.Contains(
            responseBody,
            "La tarea debe ser aprobada antes de iniciarse."
        );

        // La tarea debe permanecer pendiente.
        Assert.IsNotNull(_task);
        Assert.AreEqual("PENDING", _task.Status);

        // No debe ejecutarse una actualización de estado.
        _taskServiceMock.Verify(
            service => service.PatchStatusAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never
        );
    }



    [Then("no inventory material should be consumed")]
    public void ThenNoInventoryMaterialShouldBeConsumed()
    {
        // No debe ejecutarse ninguna operación del servicio
        // de inventario durante el intento rechazado.
        _inventoryServiceMock.VerifyNoOtherCalls();

        // La tarea debe continuar pendiente.
        Assert.IsNotNull(_task);
        Assert.AreEqual("PENDING", _task.Status);
        Assert.AreEqual("PENDING", _task.AdminReviewStatus);
    }

    [Then("the task status should be {string}")]
    public void ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        // Assert: comprobar el estado final de la tarea.
        Assert.IsNotNull(_task);

        Assert.AreEqual(
            expectedStatus,
            _task.Status
        );
    }

    [Then("the work order progress should show {int} percent")]
    public void ThenTheWorkOrderProgressShouldShowPercent(
        int expectedProgress)
    {
        // Assert: calcular el progreso utilizando
        // la implementación real de AutoService.
        Assert.IsNotNull(_task);

        var actualProgress =
            TrackingSummaryFactory.CalculateProgress(
                new[] { _task }
            );

        Assert.AreEqual(
            (decimal)expectedProgress,
            actualProgress
        );
    }

    [AfterScenario]
    public async System.Threading.Tasks.Task CleanupAsync()
    {
        _response?.Dispose();

        if (_app != null)
        {
            await _app.DisposeAsync();
            _app = null;
        }
    }

}
