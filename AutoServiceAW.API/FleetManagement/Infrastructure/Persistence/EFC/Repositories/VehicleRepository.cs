using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace AutoServiceAW.API.FleetManagement.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IVehicleRepository"/> contract 
/// using Entity Framework Core and inheriting from the generic <see cref="BaseRepository{Vehicle}"/>.
/// </summary>
public class VehicleRepository(AppDbContext context) : BaseRepository<Vehicle>(context), IVehicleRepository
{
}