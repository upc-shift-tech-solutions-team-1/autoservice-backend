using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.Tests.Unit;

public class VehicleTests
{
    [Fact]
    public void Constructor_ShouldCreateVehicleWithProvidedData()
    {
        // Arrange & Act
        var vehicle = new Vehicle(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "vehicle.jpg",
            1);

        // Assert
        Assert.Equal("ABC-123", vehicle.Plate);
        Assert.Equal("Toyota", vehicle.Brand);
        Assert.Equal("Corolla", vehicle.Model);
        Assert.Equal("2024", vehicle.Year);
        Assert.Equal("Black", vehicle.Color);
        Assert.Equal("ACTIVE", vehicle.Status);
        Assert.Equal("vehicle.jpg", vehicle.Image);
        Assert.Equal(1, vehicle.CustomerId);
    }

    [Fact]
    public void Update_ShouldModifyVehicleInformation()
    {
        // Arrange
        var vehicle = new Vehicle(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "vehicle.jpg",
            1);

        // Act
        vehicle.Update(
            "XYZ-789",
            "Honda",
            "Civic",
            "2025",
            "White",
            "IN_REPAIR",
            "new-image.jpg",
            2);

        // Assert
        Assert.Equal("XYZ-789", vehicle.Plate);
        Assert.Equal("Honda", vehicle.Brand);
        Assert.Equal("Civic", vehicle.Model);
        Assert.Equal("2025", vehicle.Year);
        Assert.Equal("White", vehicle.Color);
        Assert.Equal("IN_REPAIR", vehicle.Status);
        Assert.Equal("new-image.jpg", vehicle.Image);
        Assert.Equal(2, vehicle.CustomerId);
    }
}