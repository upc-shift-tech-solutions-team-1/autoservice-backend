using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;
using Moq;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Application.Internal;

[TestClass]
public class TaskServiceTests
{
    [TestMethod]
    public async Task CreateAsync_WithValidTask_ShouldPersistTaskAndCompleteUnitOfWork()
    {
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var taskService = new TaskService(
            taskRepositoryMock.Object,
            unitOfWorkMock.Object
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
            .Setup(repository => repository.AddAsync(
                task,
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        var result = await taskService.CreateAsync(task);

        Assert.AreSame(task, result);
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
    }

    [TestMethod]
    public async Task PatchStatusAsync_WithTechnicalUpdates_ShouldUpdateTaskAndPersistChanges()
    {
        var taskRepositoryMock = new Mock<ITaskRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var taskService = new TaskService(
            taskRepositoryMock.Object,
            unitOfWorkMock.Object
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
            .Returns(Task.CompletedTask);

        var result = await taskService.PatchStatusAsync(
            5,
            "IN_PROGRESS",
            "Brake pads are worn",
            "The brake pads need replacement",
            "Awaiting repair",
            "https://example.test/evidence/5",
            "APPROVED"
        );

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
