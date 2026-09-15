using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.CustomerManagement.Domain.Services;

/// <summary>
/// Defines the service contract for managing customer domain operations.
/// </summary>
public interface ICustomerService
{
    #region Methods

    /// <summary>
    /// Coordinates the creation of a new customer.
    /// </summary>
    /// <param name="customer">The customer entity data to register.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Customer"/> or <see langword="null"/> if the operation fails.</returns>
    Task<Customer?> CreateAsync(Customer customer);

    /// <summary>
    /// Coordinates the retrieval of all registered customers.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Customer"/> entities.</returns>
    Task<IEnumerable<Customer>> ListAsync();

    /// <summary>
    /// Coordinates the retrieval of a specific customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="Customer"/>, or <see langword="null"/> if no match is found.</returns>
    Task<Customer?> GetByIdAsync(int id);

    /// <summary>
    /// Coordinates the update of an existing customer's data.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="customer">The customer entity containing the updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Customer"/>, or <see langword="null"/> if the customer does not exist.</returns>
    Task<Customer?> UpdateAsync(int id, Customer customer);

    /// <summary>
    /// Coordinates the deletion of a customer from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(int id);

    #endregion
}