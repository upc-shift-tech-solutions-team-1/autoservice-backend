using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.StaffCoordination.Domain.Model.Aggregates;

[TestClass]
public class MechanicTests
{
    [TestMethod]
    public void Constructor_WithValidData_ShouldInitializeMechanic()
    {
        // Arrange & Act
        var mechanic = new Mechanic(
            "Juan Perez",
            "Brakes",
            5,
            "juan@example.com",
            "WS-1234",
            "password123");

        // Assert
        Assert.AreEqual("Juan Perez", mechanic.FullName);
        Assert.AreEqual("Brakes", mechanic.Specialty);
        Assert.AreEqual(5, mechanic.MaxCapacity);
        Assert.AreEqual("juan@example.com", mechanic.Email);
        Assert.AreEqual("WS-1234", mechanic.WorkshopId);
        Assert.AreEqual("password123", mechanic.Password);
    }

    [TestMethod]
    public void Update_WithValidData_ShouldChangeProfileWithoutChangingWorkshop()
    {
        // Arrange
        var mechanic = new Mechanic(
            "Juan Perez",
            "Brakes",
            5,
            "juan@example.com",
            "WS-1234",
            "oldPassword");

        // Act
        mechanic.Update(
            "Carlos Gomez",
            "Engine",
            8,
            "carlos@example.com",
            "newPassword");

        // Assert
        Assert.AreEqual("Carlos Gomez", mechanic.FullName);
        Assert.AreEqual("Engine", mechanic.Specialty);
        Assert.AreEqual(8, mechanic.MaxCapacity);
        Assert.AreEqual("carlos@example.com", mechanic.Email);
        Assert.AreEqual("newPassword", mechanic.Password);
        Assert.AreEqual("WS-1234", mechanic.WorkshopId);
    }
}