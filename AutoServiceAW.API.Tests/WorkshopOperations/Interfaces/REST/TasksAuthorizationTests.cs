using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Interfaces.REST;

[TestClass]
public class TasksAuthorizationTests
{
    [TestMethod]
    public void TasksController_ShouldRequireAuthentication()
    {
        // Arrange
        var controllerType = typeof(
            AutoServiceAW.API.WorkshopOperations.Interfaces.REST.TasksController
        );

        // Act
        var authorizeAttribute = Attribute.GetCustomAttribute(
            controllerType,
            typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)
        );

        // Assert
        Assert.IsNotNull(
            authorizeAttribute,
            "TasksController debe exigir autenticación."
        );
    }
    [TestMethod]
    public void DeleteTask_ShouldRequireAdminRole()
    {
        // Arrange
        var method = typeof(
            AutoServiceAW.API.WorkshopOperations.Interfaces.REST.TasksController
        ).GetMethod("DeleteTask");

        // Act
        var authorizeAttribute = method?
            .GetCustomAttributes(
                typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),
                true
            )
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.IsNotNull(authorizeAttribute);
        Assert.AreEqual("admin", authorizeAttribute.Roles);
    }
    [TestMethod]
    public void UpdateTask_ShouldRequireAdminRole()
    {
        // Arrange
        var method = typeof(
            AutoServiceAW.API.WorkshopOperations.Interfaces.REST.TasksController
        ).GetMethod("UpdateTask");

        // Act
        var authorizeAttribute = method?
            .GetCustomAttributes(
                typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute),
                true
            )
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.IsNotNull(authorizeAttribute);
        Assert.AreEqual("admin", authorizeAttribute.Roles);
    }
    [TestMethod]
    public async System.Threading.Tasks.Task
    DeleteTask_FromAnotherWorkshop_ShouldReturnNotFound()
    {
    // Arrange
    var taskServiceMock = new Moq.Mock<
        AutoServiceAW.API.WorkshopOperations.Domain.Services.ITaskService>();

    var workOrderServiceMock = new Moq.Mock<
        AutoServiceAW.API.WorkshopOperations.Domain.Services.IWorkOrderService>();

    var inventoryServiceMock = new Moq.Mock<
        AutoServiceAW.API.InventoryManagement.Domain.Services.IInventoryItemService>();

    var mechanicRepositoryMock = new Moq.Mock<
        AutoServiceAW.API.StaffCoordination.Domain.Repositories.IMechanicRepository>();

    var task = new AutoServiceAW.API.WorkshopOperations
        .Domain.Model.Aggregates.Task(
            20, 8, "Revisar frenos", "PENDING",
            "HIGH", 60, 0m
        );

    var workOrder = new AutoServiceAW.API.WorkshopOperations
        .Domain.Model.Aggregates.WorkOrder(
            "WS-02", 15, 23, 8,
            "Revisión de frenos", "2026-10-20", 0m
        );

    taskServiceMock
        .Setup(service => service.GetByIdAsync(5))
        .ReturnsAsync(task);

    workOrderServiceMock
        .Setup(service => service.GetByIdAsync(20))
        .ReturnsAsync(workOrder);

    var controller = new AutoServiceAW.API.WorkshopOperations
        .Interfaces.REST.TasksController(
            taskServiceMock.Object,
            workOrderServiceMock.Object,
            inventoryServiceMock.Object,
            mechanicRepositoryMock.Object
        );

    var claims = new[]
    {
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Role, "admin"
        ),
        new System.Security.Claims.Claim("WorkshopId", "WS-01")
    };

    controller.ControllerContext =
        new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        claims, "Test"
                    )
                )
            }
        };

    // Act
    var result = await controller.DeleteTask(5);

    // Assert
    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsInstanceOfType<
        Microsoft.AspNetCore.Mvc.NotFoundResult>(result);

    taskServiceMock.Verify(
        service => service.DeleteAsync(
            Moq.It.IsAny<int>()
        ),
        Moq.Times.Never
    );
    }
[TestMethod]
public async System.Threading.Tasks.Task
    PatchTask_AssignedToAnotherMechanic_ShouldReturnForbidden()
{
    // Arrange
    var taskServiceMock = new Moq.Mock<
        AutoServiceAW.API.WorkshopOperations.Domain.Services.ITaskService>();

    var workOrderServiceMock = new Moq.Mock<
        AutoServiceAW.API.WorkshopOperations.Domain.Services.IWorkOrderService>();

    var inventoryServiceMock = new Moq.Mock<
        AutoServiceAW.API.InventoryManagement.Domain.Services.IInventoryItemService>();

    var mechanicRepositoryMock = new Moq.Mock<
        AutoServiceAW.API.StaffCoordination.Domain.Repositories.IMechanicRepository>();

    var task = new AutoServiceAW.API.WorkshopOperations
        .Domain.Model.Aggregates.Task(
            20, 34, "Revisar frenos", "PENDING",
            "HIGH", 60, 0m
        );

    var workOrder = new AutoServiceAW.API.WorkshopOperations
        .Domain.Model.Aggregates.WorkOrder(
            "WS-01", 15, 23, 8,
            "Revisión de frenos", "2026-10-20", 0m
        );

    var mechanic = new AutoServiceAW.API.StaffCoordination
        .Domain.Model.Aggregates.Mechanic(
            "Mecánico de prueba",
            "Frenos",
            5,
            "mecanico@test.com",
            "WS-01",
            "test-password"
        );

    typeof(AutoServiceAW.API.StaffCoordination
        .Domain.Model.Aggregates.Mechanic)
        .GetProperty("Id")?
        .SetValue(mechanic, 8);

    taskServiceMock
        .Setup(service => service.GetByIdAsync(5))
        .ReturnsAsync(task);

    workOrderServiceMock
        .Setup(service => service.GetByIdAsync(20))
        .ReturnsAsync(workOrder);

    mechanicRepositoryMock
        .Setup(repository =>
            repository.FindByEmailAsync("mecanico@test.com"))
        .ReturnsAsync(mechanic);

    var controller = new AutoServiceAW.API.WorkshopOperations
        .Interfaces.REST.TasksController(
            taskServiceMock.Object,
            workOrderServiceMock.Object,
            inventoryServiceMock.Object,
            mechanicRepositoryMock.Object
        );

    var claims = new[]
    {
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Role, "mechanic"
        ),
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Email, "mecanico@test.com"
        ),
        new System.Security.Claims.Claim("WorkshopId", "WS-01")
    };

    controller.ControllerContext =
        new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        claims, "Test"
                    )
                )
            }
        };

    // Act
    var result = await controller.PatchTask(
        5,
        new AutoServiceAW.API.WorkshopOperations.Interfaces.REST
            .PatchTaskResource(
                "IN_PROGRESS",
                null,
                null,
                null,
                null,
                null
            )
    );

    // Assert
    Microsoft.VisualStudio.TestTools.UnitTesting.Assert
        .IsInstanceOfType<
            Microsoft.AspNetCore.Mvc.ForbidResult>(result);

    taskServiceMock.Verify(
        service => service.PatchStatusAsync(
            Moq.It.IsAny<int>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>()
        ),
        Moq.Times.Never
    );

    inventoryServiceMock.VerifyNoOtherCalls();
}

[TestMethod]
public async System.Threading.Tasks.Task
    PatchTask_AssignedMechanic_ShouldAllowTechnicalDiagnosis()
{
    // Arrange
    var taskServiceMock = new Moq.Mock<
        AutoServiceAW.API.WorkshopOperations.Domain.Services.ITaskService>();

    var workOrderServiceMock = new Moq.Mock<
        AutoServiceAW.API.WorkshopOperations.Domain.Services.IWorkOrderService>();

    var inventoryServiceMock = new Moq.Mock<
        AutoServiceAW.API.InventoryManagement.Domain.Services.IInventoryItemService>();

    var mechanicRepositoryMock = new Moq.Mock<
        AutoServiceAW.API.StaffCoordination.Domain.Repositories.IMechanicRepository>();

    var task = new AutoServiceAW.API.WorkshopOperations
        .Domain.Model.Aggregates.Task(
            20, 8, "Revisar frenos", "PENDING",
            "HIGH", 60, 0m
        );

    var workOrder = new AutoServiceAW.API.WorkshopOperations
        .Domain.Model.Aggregates.WorkOrder(
            "WS-01", 15, 23, 8,
            "Revisión de frenos", "2026-10-20", 0m
        );

    var mechanic = new AutoServiceAW.API.StaffCoordination
        .Domain.Model.Aggregates.Mechanic(
            "Mecánico de prueba",
            "Frenos",
            5,
            "mecanico@test.com",
            "WS-01",
            "test-password"
        );

    typeof(AutoServiceAW.API.StaffCoordination
        .Domain.Model.Aggregates.Mechanic)
        .GetProperty("Id")?
        .SetValue(mechanic, 8);

    taskServiceMock
        .Setup(service => service.GetByIdAsync(5))
        .ReturnsAsync(task);

    workOrderServiceMock
        .Setup(service => service.GetByIdAsync(20))
        .ReturnsAsync(workOrder);

    mechanicRepositoryMock
        .Setup(repository =>
            repository.FindByEmailAsync("mecanico@test.com"))
        .ReturnsAsync(mechanic);

    taskServiceMock
        .Setup(service => service.PatchStatusAsync(
            5,
            "",
            "Las pastillas de freno están desgastadas",
            "",
            "",
            "",
            ""
        ))
        .ReturnsAsync(task);

    var controller = new AutoServiceAW.API.WorkshopOperations
        .Interfaces.REST.TasksController(
            taskServiceMock.Object,
            workOrderServiceMock.Object,
            inventoryServiceMock.Object,
            mechanicRepositoryMock.Object
        );

    var claims = new[]
    {
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Role, "mechanic"
        ),
        new System.Security.Claims.Claim(
            System.Security.Claims.ClaimTypes.Email, "mecanico@test.com"
        ),
        new System.Security.Claims.Claim("WorkshopId", "WS-01")
    };

    controller.ControllerContext =
        new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        claims, "Test"
                    )
                )
            }
        };

    // Act
    var result = await controller.PatchTask(
        5,
        new AutoServiceAW.API.WorkshopOperations.Interfaces.REST
            .PatchTaskResource(
                null,
                "Las pastillas de freno están desgastadas",
                null,
                null,
                null,
                "APPROVED"
            )
    );

    // Assert
    Microsoft.VisualStudio.TestTools.UnitTesting.Assert
        .IsInstanceOfType<
            Microsoft.AspNetCore.Mvc.OkObjectResult>(result);

    taskServiceMock.Verify(
        service => service.PatchStatusAsync(
            5,
            "",
            "Las pastillas de freno están desgastadas",
            "",
            "",
            "",
            ""
        ),
        Moq.Times.Once
    );

    inventoryServiceMock.VerifyNoOtherCalls();
}

}