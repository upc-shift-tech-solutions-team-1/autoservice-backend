using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.CustomerManagement.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace AutoServiceAW.API.CustomerManagement.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="ICustomerRepository"/> contract 
/// using Entity Framework Core and inheriting from the generic <see cref="BaseRepository{Customer}"/>.
/// </summary>
public class CustomerRepository(AppDbContext context) : BaseRepository<Customer>(context), ICustomerRepository
{
}