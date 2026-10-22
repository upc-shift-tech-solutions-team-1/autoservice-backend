using System.Security.Claims;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Interfaces.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Interfaces.REST;

[TestClass]
public class WorkOrdersAuthorizationTests
{
    [TestMethod]
    public async System.Threading.Tasks.Task
        GetWorkOrder_FromAnotherWorkshop_ShouldReturnNotFound()
    {
        // Arrange
        var workOrderServiceMock = new Mock<IWorkOrderService>();

        var foreignOrder = new WorkOrder(
            "WS-02",
            15,
            23,
            8,
            "Brake replacement",
            "2026-10-25",
            500m
        );

        workOrderServiceMock
            .Setup(service => service.GetByIdAsync(10))
            .ReturnsAsync(foreignOrder);

        var controller = new WorkOrdersController(
            workOrderServiceMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.Role, "admin"),
                            new Claim("WorkshopId", "WS-01")
                        },
                        "Test"
                    )
                )
            }
        };

        // Act
        var result = await controller.GetWorkOrder(10);

        // Assert
        Assert.IsInstanceOfType<NotFoundResult>(
            result,
            "Un usuario no debe consultar órdenes de otro taller."
        );
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        UpdateWorkOrder_FromAnotherWorkshop_ShouldReturnNotFoundWithoutSaving()
    {
        // Arrange
        var serviceMock = new Mock<IWorkOrderService>();

        var foreignOrder = new WorkOrder(
            "WS-02",
            15,
            23,
            8,
            "Original brake repair",
            "2026-10-25",
            500m
        );

        serviceMock
            .Setup(service => service.GetByIdAsync(10))
            .ReturnsAsync(foreignOrder);

        var controller = new WorkOrdersController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(ClaimTypes.Role, "admin"),
                                new Claim("WorkshopId", "WS-01")
                            },
                            "Test"
                        )
                    )
                }
            }
        };

        var request = new UpdateWorkOrderResource(
            "Unauthorized modification",
            "2026-11-01",
            900m,
            "COMPLETED",
            true,
            true,
            true,
            true,
            true
        );

        // Act
        var result = await controller.UpdateWorkOrder(10, request);

        // Assert
        Assert.IsInstanceOfType<NotFoundResult>(result);

        serviceMock.Verify(
            service => service.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<WorkOrder>()),
            Times.Never
        );

        Assert.AreEqual("Original brake repair", foreignOrder.Description);
        Assert.AreEqual("PENDING", foreignOrder.Status);
        Assert.AreEqual(500m, foreignOrder.Price);
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        DeleteWorkOrder_FromAnotherWorkshop_ShouldReturnNotFoundWithoutDeleting()
    {
        // Arrange
        var serviceMock = new Mock<IWorkOrderService>();

        var foreignOrder = new WorkOrder(
            "WS-02",
            15,
            23,
            8,
            "Brake replacement",
            "2026-10-25",
            500m
        );

        serviceMock
            .Setup(service => service.GetByIdAsync(10))
            .ReturnsAsync(foreignOrder);

        var controller = new WorkOrdersController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(ClaimTypes.Role, "admin"),
                                new Claim("WorkshopId", "WS-01")
                            },
                            "Test"
                        )
                    )
                }
            }
        };

        // Act
        var result = await controller.DeleteWorkOrder(10);

        // Assert
        Assert.IsInstanceOfType<NotFoundResult>(result);

        serviceMock.Verify(
            service => service.DeleteAsync(
                It.IsAny<int>(),
                It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        GetWorkOrder_FromOwnWorkshop_ShouldReturnOk()
    {
        // Arrange
        var serviceMock = new Mock<IWorkOrderService>();

        var ownOrder = new WorkOrder(
            "WS-01",
            15,
            23,
            8,
            "Brake replacement",
            "2026-10-25",
            500m
        );

        serviceMock
            .Setup(service => service.GetByIdAsync(10))
            .ReturnsAsync(ownOrder);

        var controller = new WorkOrdersController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[]
                            {
                                new Claim(ClaimTypes.Role, "admin"),
                                new Claim("WorkshopId", "WS-01")
                            },
                            "Test"
                        )
                    )
                }
            }
        };

        // Act
        var result = await controller.GetWorkOrder(10);

        // Assert
        var okResult = Assert.IsInstanceOfType<OkObjectResult>(result);

        Assert.AreSame(ownOrder, okResult.Value);

        serviceMock.Verify(
            service => service.GetByIdAsync(10),
            Times.Once
        );
    }


[TestMethod]
public async System.Threading.Tasks.Task
    UpdateWorkOrder_FromOwnWorkshop_ShouldCallServiceAndReturnOk()
{
    // Arrange
    var serviceMock = new Mock<IWorkOrderService>();

    var existingOrder = new WorkOrder(
        "WS-01", 15, 23, 8,
        "Original diagnosis", "2026-10-25", 500m
    );

    serviceMock
        .Setup(service => service.GetByIdAsync(10))
        .ReturnsAsync(existingOrder);


    serviceMock
        .Setup(service => service.UpdateAsync(
            10,
            "WS-01",
            It.IsAny<WorkOrder>()))
        .ReturnsAsync((int _, string _, WorkOrder updated) => updated);


    var controller = new WorkOrdersController(serviceMock.Object)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.Role, "admin"),
                            new Claim("WorkshopId", "WS-01")
                        },
                        "Test"
                    )
                )
            }
        }
    };

    var request = new UpdateWorkOrderResource(
        "Updated brake repair",
        "2026-11-05",
        750m,
        "IN_PROGRESS",
        true, false, true, false, false
    );

    // Act
    var result = await controller.UpdateWorkOrder(10, request);

    // Assert
    var okResult = Assert.IsInstanceOfType<OkObjectResult>(result);
    var updatedOrder = Assert.IsInstanceOfType<WorkOrder>(okResult.Value);

    Assert.AreEqual("WS-01", updatedOrder.WorkshopId);
    Assert.AreEqual("Updated brake repair", updatedOrder.Description);
    Assert.AreEqual("2026-11-05", updatedOrder.EstimatedDate);
    Assert.AreEqual(750m, updatedOrder.Price);
    Assert.AreEqual("IN_PROGRESS", updatedOrder.Status);


    serviceMock.Verify(
        service => service.UpdateAsync(
            10,
            "WS-01",
            It.Is<WorkOrder>(order =>
                order.WorkshopId == "WS-01" &&
                order.Description == "Updated brake repair")),
        Times.Once
    );

}

[TestMethod]
public async System.Threading.Tasks.Task
    DeleteWorkOrder_FromOwnWorkshop_ShouldReturnNoContentAndCallService()
{
    // Arrange
    var serviceMock = new Mock<IWorkOrderService>();

    var ownOrder = new WorkOrder(
        "WS-01",
        15,
        23,
        8,
        "Brake replacement",
        "2026-10-25",
        500m
    );

    serviceMock
        .Setup(service => service.GetByIdAsync(10))
        .ReturnsAsync(ownOrder);

    var controller = new WorkOrdersController(serviceMock.Object)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.Role, "admin"),
                            new Claim("WorkshopId", "WS-01")
                        },
                        "Test"
                    )
                )
            }
        }
    };

    // Act
    var result = await controller.DeleteWorkOrder(10);

    // Assert
    Assert.IsInstanceOfType<NoContentResult>(result);

    serviceMock.Verify(
        service => service.GetByIdAsync(10),
        Times.Once
    );

    serviceMock.Verify(
        service => service.DeleteAsync(10, "WS-01"),
        Times.Once
    );
}

}