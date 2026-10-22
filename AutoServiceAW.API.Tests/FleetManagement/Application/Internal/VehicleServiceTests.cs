using AutoServiceAW.API.FleetManagement.Application.Internal;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Moq;

namespace AutoServiceAW.API.Tests.FleetManagement.Application.Internal;

[TestClass]
public class VehicleServiceTests
{
    private Mock<IVehicleRepository> _vehicleRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private VehicleService _vehicleService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vehicleService = new VehicleService(
            _vehicleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    /// <summary>
    /// Verifies that creating a vehicle persists the entity through the repository,
    /// completes the unit of work transaction, and returns the persisted vehicle.
    /// </summary>
    [TestMethod]
    public async Task CreateAsync_WithValidVehicle_ShouldPersistVehicleAndCompleteUnitOfWork()
    {
        // ARRANGE
        var vehicle = new Vehicle(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "vehicle.jpg",
            1
        );

        _vehicleRepositoryMock
            .Setup(repository => repository.AddAsync(
                vehicle,
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        // ACT
        var result = await _vehicleService.CreateAsync(vehicle);

        // ASSERT
        Assert.IsNotNull(result);
        Assert.AreSame(vehicle, result);
        Assert.AreEqual("ABC-123", result.Plate);
        Assert.AreEqual("Toyota", result.Brand);
        Assert.AreEqual("ACTIVE", result.Status);

        _vehicleRepositoryMock.Verify(
            repository => repository.AddAsync(
                vehicle,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that updating an existing vehicle modifies its properties,
    /// updates the repository, and completes the unit of work transaction.
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WhenVehicleExists_ShouldUpdateVehicleDetailsAndCompleteUnitOfWork()
    {
        // ARRANGE
        const int vehicleId = 10;
        var existingVehicle = new Vehicle(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "old-vehicle.jpg",
            1
        );

        var updatedVehicleData = new Vehicle(
            "XYZ-789",
            "Honda",
            "Civic",
            "2025",
            "White",
            "IN_REPAIR",
            "new-vehicle.jpg",
            2
        );

        _vehicleRepositoryMock
            .Setup(repository => repository.FindByIdAsync(
                vehicleId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(existingVehicle);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        // ACT
        var result = await _vehicleService.UpdateAsync(vehicleId, updatedVehicleData);

        // ASSERT
        Assert.IsNotNull(result);
        Assert.AreSame(existingVehicle, result);
        Assert.AreEqual("XYZ-789", existingVehicle.Plate);
        Assert.AreEqual("Honda", existingVehicle.Brand);
        Assert.AreEqual("Civic", existingVehicle.Model);
        Assert.AreEqual("2025", existingVehicle.Year);
        Assert.AreEqual("White", existingVehicle.Color);
        Assert.AreEqual("IN_REPAIR", existingVehicle.Status);
        Assert.AreEqual("new-vehicle.jpg", existingVehicle.Image);
        Assert.AreEqual(2, existingVehicle.CustomerId);

        _vehicleRepositoryMock.Verify(
            repository => repository.FindByIdAsync(
                vehicleId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.Update(existingVehicle),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that updating a non-existent vehicle returns null
    /// without invoking repository update or unit of work.
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WhenVehicleDoesNotExist_ShouldReturnNullAndNotCommitUnitOfWork()
    {
        // ARRANGE
        const int nonExistentId = 999;
        var updatedVehicleData = new Vehicle(
            "XYZ-789",
            "Honda",
            "Civic",
            "2025",
            "White",
            "IN_REPAIR",
            "new-vehicle.jpg",
            2
        );

        _vehicleRepositoryMock
            .Setup(repository => repository.FindByIdAsync(
                nonExistentId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Vehicle?)null);

        // ACT
        var result = await _vehicleService.UpdateAsync(nonExistentId, updatedVehicleData);

        // ASSERT
        Assert.IsNull(result);

        _vehicleRepositoryMock.Verify(
            repository => repository.FindByIdAsync(
                nonExistentId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Vehicle>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Never
        );
    }

    /// <summary>
    /// Verifies that deleting an existing vehicle invokes repository removal
    /// and completes the unit of work transaction.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_WhenVehicleExists_ShouldRemoveVehicleAndCompleteUnitOfWork()
    {
        // ARRANGE
        const int vehicleId = 15;
        var existingVehicle = new Vehicle(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "vehicle.jpg",
            1
        );

        _vehicleRepositoryMock
            .Setup(repository => repository.FindByIdAsync(
                vehicleId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(existingVehicle);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        // ACT
        await _vehicleService.DeleteAsync(vehicleId);

        // ASSERT
        _vehicleRepositoryMock.Verify(
            repository => repository.FindByIdAsync(
                vehicleId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.Remove(existingVehicle),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
    }
}
