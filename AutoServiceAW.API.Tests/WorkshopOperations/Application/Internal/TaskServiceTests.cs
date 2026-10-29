using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;
using SystemTask = System.Threading.Tasks.Task;
using Moq;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Application.Internal;

[TestClass]
public class TaskServiceTests
{
    /// <summary>
    /// Verifies that creating a task persists it through the repository,
    /// completes the unit of work, and returns the created instance.
    /// </summary>
    [TestMethod]
    public async System.Threading.Tasks.Task CreateAsync_WithValidTask_ShouldPersistTaskAndCompleteUnitOfWork()
    {
        // Arrange
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var taskService = new TaskService(
            taskRepositoryMock.Object,
            unitOfWorkMock.Object,
            workOrderRepositoryMock.Object
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

        taskRepositoryMock
            .Setup(repository => repository.FindByWorkOrderIdAsync(12))
            .ReturnsAsync(Array.Empty<WorkshopTask>());
        var workOrder = new WorkOrder("WS-01", 1, 2, 3, "Service", "2026-10-20", 0m);
        workOrderRepositoryMock
            .Setup(repository => repository.FindByIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);
        taskRepositoryMock
            .Setup(repository => repository.AddAsync(
                task,
                It.IsAny<CancellationToken>()
            ))
            .Returns(SystemTask.CompletedTask);

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(SystemTask.CompletedTask);

        // Act
        var result = await taskService.CreateAsync(task);

        // Assert
        Assert.AreSame(task, result);
        Assert.AreEqual(250m, workOrder.Price);
        taskRepositoryMock.Verify(
            repository => repository.AddAsync(
                task,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
        workOrderRepositoryMock.Verify(repository => repository.Update(workOrder), Times.Once);
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpdateAsync_ShouldKeepPartsAndSynchronizeWorkOrderPrice()
    {
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var existingTask = new WorkshopTask(12, 34, "Existing repair", "PENDING", "HIGH", 60, 100m);
        existingTask.UpdateMaterialsCost(40m, 20m);
        existingTask.AddPart(new TaskPart(0, 91, "Brake pad", 1, 40m, 20m));
        var workOrder = new WorkOrder("WS-01", 1, 2, 3, "Service", "2026-10-20", 999m);

        taskRepositoryMock.Setup(repository => repository.FindByIdWithPartsAsync(7)).ReturnsAsync(existingTask);
        taskRepositoryMock.Setup(repository => repository.FindByWorkOrderIdAsync(12)).ReturnsAsync(new[] { existingTask });
        workOrderRepositoryMock.Setup(repository => repository.FindByIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(workOrder);
        unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(SystemTask.CompletedTask);

        var service = new TaskService(taskRepositoryMock.Object, unitOfWorkMock.Object, workOrderRepositoryMock.Object);
        var update = new WorkshopTask(12, 34, "Updated repair", "PENDING", "NORMAL", 45, 150m);

        var result = await service.UpdateAsync(7, update);

        Assert.IsNotNull(result);
        Assert.AreEqual("Brake pad", result.Parts.Single().Name);
        Assert.AreEqual(190m, workOrder.Price);
        workOrderRepositoryMock.Verify(repository => repository.Update(workOrder), Times.Once);
        unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [TestMethod]
    public async System.Threading.Tasks.Task DeleteAsync_ShouldRecalculateWorkOrderPrice()
    {
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var existingTask = new WorkshopTask(12, 34, "Repair", "PENDING", "NORMAL", 30, 100m);
        existingTask.UpdateMaterialsCost(20m, 10m);
        var workOrder = new WorkOrder("WS-01", 1, 2, 3, "Service", "2026-10-20", 120m);

        taskRepositoryMock.Setup(repository => repository.FindByIdWithPartsAsync(7)).ReturnsAsync(existingTask);
        taskRepositoryMock.Setup(repository => repository.FindByWorkOrderIdAsync(12)).ReturnsAsync(new[] { existingTask });
        workOrderRepositoryMock.Setup(repository => repository.FindByIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(workOrder);
        unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).Returns(SystemTask.CompletedTask);

        var service = new TaskService(taskRepositoryMock.Object, unitOfWorkMock.Object, workOrderRepositoryMock.Object);

        await service.DeleteAsync(7);

        Assert.AreEqual(0m, workOrder.Price);
        taskRepositoryMock.Verify(repository => repository.Remove(existingTask), Times.Once);
        workOrderRepositoryMock.Verify(repository => repository.Update(workOrder), Times.Once);
        unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that patching a task's technical data updates the task,
    /// persists the changes, and completes the unit of work.
    /// </summary>
    [TestMethod]
    public async System.Threading.Tasks.Task PatchStatusAsync_WithTechnicalUpdates_ShouldUpdateTaskAndPersistChanges()
    {
        // Arrange
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var taskService = new TaskService(
            taskRepositoryMock.Object,
            unitOfWorkMock.Object,
            Mock.Of<IWorkOrderRepository>()
        );
        var existingTask = new WorkshopTask(
            12,
            34,
            "Replace brake pads",
            "PENDING",
            "HIGH",
            90,
            250m
        );

        taskRepositoryMock
            .Setup(repository => repository.FindByIdWithPartsAsync(5))
            .ReturnsAsync(existingTask);
        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(SystemTask.CompletedTask);

        // Act
        var result = await taskService.PatchStatusAsync(
            5,
            "IN_PROGRESS",
            "Brake pads are worn",
            "The brake pads need replacement",
            "Awaiting repair",
            "https://example.test/evidence/5",
            "APPROVED"
        );

        // Assert
        Assert.AreSame(existingTask, result);
        Assert.AreEqual("IN_PROGRESS", existingTask.Status);
        Assert.AreEqual("Brake pads are worn", existingTask.TechnicalDiagnosis);
        Assert.AreEqual("The brake pads need replacement", existingTask.CustomerExplanation);
        Assert.AreEqual("Awaiting repair", existingTask.InternalObservation);
        Assert.AreEqual("https://example.test/evidence/5", existingTask.EvidenceRegistered);
        Assert.AreEqual("APPROVED", existingTask.AdminReviewStatus);
        taskRepositoryMock.Verify(
            repository => repository.Update(existingTask),
            Times.Once
        );
        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
    }
}
