using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoServiceAW.API.Tests.CustomerManagement.Domain.Model.Aggregates;

[TestClass]
public class CustomerTests
{
    [TestMethod]
    public void Constructor_ShouldCreateCustomerWithProvidedData()
    {
        // Arrange
        const string workshopId = "WORKSHOP-001";
        const string fullName = "Juan Sanchez";
        const string dni = "12345678";
        const string email = "juan@email.com";
        const string phone = "999999999";

        // Act
        var customer = new Customer(
            workshopId,
            fullName,
            dni,
            email,
            phone);

        // Assert
        Assert.AreEqual(workshopId, customer.WorkshopId);
        Assert.AreEqual(fullName, customer.FullName);
        Assert.AreEqual(dni, customer.Dni);
        Assert.AreEqual(email, customer.Email);
        Assert.AreEqual(phone, customer.Phone);
    }

    [TestMethod]
    public void Update_ShouldModifyCustomerInformation()
    {
        // Arrange
        var customer = CreateCustomer();

        // Act
        customer.Update(
            "Juan Carlos Sanchez",
            "87654321",
            "juan.carlos@email.com",
            "988888888");

        // Assert
        Assert.AreEqual("Juan Carlos Sanchez", customer.FullName);
        Assert.AreEqual("87654321", customer.Dni);
        Assert.AreEqual("juan.carlos@email.com", customer.Email);
        Assert.AreEqual("988888888", customer.Phone);
        Assert.AreEqual("WORKSHOP-001", customer.WorkshopId);
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            "WORKSHOP-001",
            "Juan Sanchez",
            "12345678",
            "juan@email.com",
            "999999999");
    }
}