using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;


namespace AutoServiceAW.Tests.Unit;

public class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomerWithProvidedData()
    {
        // Arrange
        var customer = new Customer(
            "WORKSHOP-001",
            "Juan Sanchez",
            "12345678",
            "juan@email.com",
            "999999999");

        // Act
        var result = customer;

        // Assert
        Assert.Equal("WORKSHOP-001", result.WorkshopId);
        Assert.Equal("Juan Sanchez", result.FullName);
        Assert.Equal("12345678", result.Dni);
        Assert.Equal("juan@email.com", result.Email);
        Assert.Equal("999999999", result.Phone);
    }

    [Fact]
    public void Update_ShouldModifyCustomerInformation()
    {
        // Arrange
        var customer = new Customer(
            "WORKSHOP-001",
            "Juan Sanchez",
            "12345678",
            "juan@email.com",
            "999999999");

        // Act
        customer.Update(
            "Juan Carlos Sanchez",
            "87654321",
            "juan.carlos@email.com",
            "988888888");

        // Assert
        Assert.Equal("Juan Carlos Sanchez", customer.FullName);
        Assert.Equal("87654321", customer.Dni);
        Assert.Equal("juan.carlos@email.com", customer.Email);
        Assert.Equal("988888888", customer.Phone);
    }
}