using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IUnitOfWork"/> contract,
/// managing atomic transactional boundaries by saving state changes within the shared database context.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    #region Methods

    /// <summary>
    /// Saves all changes made in the context coordinates to the underlying database storage atomically.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CompleteAsync() => await context.SaveChangesAsync();

    #endregion
}