using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using Moq;
using WorkshopTask = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Application.Internal;

[TestClass]
public class WorkshopOperationsServiceTests
{
    [TestMethod]
    public async System.Threading.Tasks.Task PatchTaskStatus_WithDiagnosisAndEvidence_ShouldPersistTechnicalProgress()
    {
        // Arrange
        var existingTask = new WorkshopTask(
            41,
            8,
            "Inspect the reported brake noise",
            "PENDING",
            "HIGH",
            90,
            180m,
            75m
        );

        var repository = new Mock<ITaskRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(value => value.FindByIdWithPartsAsync(7))
            .ReturnsAsync(existingTask);

        unitOfWork
            .Setup(value => value.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var service = new TaskService(
            repository.Object,
            unitOfWork.Object
        );

        // Act
        var result = await service.PatchStatusAsync(
            7,
            "IN_PROGRESS",
            "Front pads are below the safe thickness",
            "The front brake pads must be replaced",
            "Verify both rotors before closing the order",
            "evidence/brakes-before.jpg",
            "APPROVED"
        );

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("IN_PROGRESS", result.Status);
        Assert.AreEqual(
            "Front pads are below the safe thickness",
            result.TechnicalDiagnosis
        );
        Assert.AreEqual(
            "evidence/brakes-before.jpg",
            result.EvidenceRegistered
        );
        Assert.AreEqual("APPROVED", result.AdminReviewStatus);

        repository.Verify(
            value => value.Update(existingTask),
            Times.Once
        );
        unitOfWork.Verify(
            value => value.CompleteAsync(),
            Times.Once
        );
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpdateWorkOrder_WithValidatedChecklist_ShouldPersistClosedOrder()
    {
        // Arrange
        var existingOrder = new WorkOrder(
            "WS-01",
            15,
            23,
            8,
            "Brake noise reported by the customer",
            "2026-10-05",
            0m
        );

        var requestedUpdate = new WorkOrder(
            string.Empty,
            0,
            0,
            0,
            "Brake repair completed and validated",
            "2026-10-04",
            420m
        );
        requestedUpdate.UpdateChecklist(true, true, true, true, true);
        requestedUpdate.UpdateStatus("COMPLETED");

        var repository = new Mock<IWorkOrderRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(value => value.FindByIdAsync(41, default))
            .ReturnsAsync(existingOrder);

        unitOfWork
            .Setup(value => value.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        var service = new WorkOrderService(
            repository.Object,
            unitOfWork.Object
        );

        // Act
        var result = await service.UpdateAsync(41, requestedUpdate);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("COMPLETED", result.Status);
        Assert.AreEqual(420m, result.Price);
        Assert.IsTrue(result.TasksCompleted);
        Assert.IsTrue(result.SparePartsChecked);
        Assert.IsTrue(result.DiagnosisValidated);
        Assert.IsTrue(result.CleaningDone);
        Assert.IsTrue(result.FinalTestDone);

        repository.Verify(
            value => value.Update(existingOrder),
            Times.Once
        );
        unitOfWork.Verify(
            value => value.CompleteAsync(),
            Times.Once
        );
    }
}
