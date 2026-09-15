using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.CustomerManagement.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Customer"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{Customer}"/>.
/// </summary>
public interface ICustomerRepository : IBaseRepository<Customer>
{
}