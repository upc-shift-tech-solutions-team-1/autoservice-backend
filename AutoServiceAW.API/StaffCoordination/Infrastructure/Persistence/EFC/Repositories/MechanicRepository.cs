using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceAW.API.StaffCoordination.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IMechanicRepository"/> contract
/// using Entity Framework Core and inheriting generic operations from <see cref="BaseRepository{Mechanic}"/>.
/// </summary>
public class MechanicRepository(AppDbContext context) : BaseRepository<Mechanic>(context), IMechanicRepository
{
    #region Methods

    /// <summary>
    /// Evaluates structural storage matching states to capture a single mechanic row profile filtered by their registration email address.
    /// </summary>
    /// <param name="email">The unique target evaluation query matching string parameter.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the evaluated <see cref="Mechanic"/> state representation, or <see langword="null"/>.</returns>
    public async Task<Mechanic?> FindByEmailAsync(string email)
    {
        return await Context.Set<Mechanic>()
            .FirstOrDefaultAsync(m => m.Email == email);
    }

    #endregion
}