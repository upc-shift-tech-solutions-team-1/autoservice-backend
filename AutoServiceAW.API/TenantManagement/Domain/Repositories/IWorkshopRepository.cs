using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.TenantManagement.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Workshop"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{Workshop}"/>.
/// </summary>
public interface IWorkshopRepository : IBaseRepository<Workshop>
{
}