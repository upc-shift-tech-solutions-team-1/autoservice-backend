using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Repositories;

namespace AutoServiceAW.API.TenantManagement.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IWorkshopRepository"/> contract 
/// using Entity Framework Core and inheriting generic operations from <see cref="BaseRepository{Workshop}"/>.
/// </summary>
public class WorkshopRepository(AppDbContext context) : BaseRepository<Workshop>(context), IWorkshopRepository
{
}