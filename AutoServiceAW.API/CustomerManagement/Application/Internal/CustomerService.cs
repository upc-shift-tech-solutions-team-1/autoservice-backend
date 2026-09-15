using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.CustomerManagement.Domain.Repositories;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.CustomerManagement.Application.Internal;

/// <summary>
/// Provides internal application services for managing customer-related operations.
/// </summary>
public class CustomerService(ICustomerRepository customerRepository, IUnitOfWork unitOfWork) : ICustomerService
{
    #region Methods

    /// <summary>
    /// Creates a new customer in the system asynchronously.
    /// </summary>
    /// <param name="customer">The customer entity to be created.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Customer"/> entity.</returns>
    public async Task<Customer?> CreateAsync(Customer customer)
    {
        await customerRepository.AddAsync(customer);
        await unitOfWork.CompleteAsync();
        return customer;
    }

    /// <summary>
    /// Retrieves all customers registered in the system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="Customer"/> entities.</returns>
    public async Task<IEnumerable<Customer>> ListAsync()
    {
        return await customerRepository.ListAsync();
    }

    /// <summary>
    /// Retrieves a specific customer by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="Customer"/>, or <see langword="null"/> if not found.</returns>
    public async Task<Customer?> GetByIdAsync(int id) => await customerRepository.FindByIdAsync(id);

    /// <summary>
    /// Updates an existing customer's details asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="updatedCustomer">The customer entity containing the updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Customer"/>, or <see langword="null"/> if the customer does not exist.</returns>
    public async Task<Customer?> UpdateAsync(int id, Customer updatedCustomer)
    {
        var existingCustomer = await customerRepository.FindByIdAsync(id);
        if (existingCustomer == null) return null;

        existingCustomer.Update(updatedCustomer.FullName, updatedCustomer.Dni, updatedCustomer.Email, updatedCustomer.Phone);
        customerRepository.Update(existingCustomer);
        await unitOfWork.CompleteAsync();
        return existingCustomer;
    }

    /// <summary>
    /// Deletes a specific customer from the system by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAsync(int id)
    {
        var existingCustomer = await customerRepository.FindByIdAsync(id);
        if (existingCustomer != null)
        {
            customerRepository.Remove(existingCustomer);
            await unitOfWork.CompleteAsync();
        }
    }

    #endregion
}