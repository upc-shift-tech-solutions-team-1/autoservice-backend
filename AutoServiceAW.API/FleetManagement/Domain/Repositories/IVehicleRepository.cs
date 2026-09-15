using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.FleetManagement.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Vehicle"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{Vehicle}"/>.
/// </summary>
public interface IVehicleRepository : IBaseRepository<Vehicle>
{
}