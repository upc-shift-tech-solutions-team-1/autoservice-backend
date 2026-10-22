using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.FleetManagement.Domain.Model.Aggregates;

[TestClass]
public class VehicleTests
{
    /// <summary>
    /// Verifies that the Vehicle constructor sets all initial properties correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_ShouldInitializeAllPropertiesCorrectly()
    {
        // ARRANGE & ACT
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

        // ASSERT
        Assert.AreEqual("ABC-123", vehicle.Plate);
        Assert.AreEqual("Toyota", vehicle.Brand);
        Assert.AreEqual("Corolla", vehicle.Model);
        Assert.AreEqual("2024", vehicle.Year);
        Assert.AreEqual("Black", vehicle.Color);
        Assert.AreEqual("ACTIVE", vehicle.Status);
        Assert.AreEqual("vehicle.jpg", vehicle.Image);
        Assert.AreEqual(1, vehicle.CustomerId);
    }

    /// <summary>
    /// Verifies that the Update method mutates all properties of the vehicle aggregate.
    /// </summary>
    [TestMethod]
    public void Update_ShouldModifyAllVehicleProperties()
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

        // ACT
        vehicle.Update(
            "XYZ-789",
            "Honda",
            "Civic",
            "2025",
            "White",
            "IN_REPAIR",
            "new-vehicle.jpg",
            2
        );

        // ASSERT
        Assert.AreEqual("XYZ-789", vehicle.Plate);
        Assert.AreEqual("Honda", vehicle.Brand);
        Assert.AreEqual("Civic", vehicle.Model);
        Assert.AreEqual("2025", vehicle.Year);
        Assert.AreEqual("White", vehicle.Color);
        Assert.AreEqual("IN_REPAIR", vehicle.Status);
        Assert.AreEqual("new-vehicle.jpg", vehicle.Image);
        Assert.AreEqual(2, vehicle.CustomerId);
    }
}
