using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Provides an abstract infrastructure implementation for the generic <see cref="IBaseRepository{TEntity}"/> contract
/// using Entity Framework Core to handle standard database operations.
/// </summary>
/// <typeparam name="TEntity">The class type representing the tracked domain entity.</typeparam>
public abstract class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    #region Properties

    /// <summary>
    /// Gets the underlying Entity Framework database context instance.
    /// </summary>
    protected readonly AppDbContext Context = context;

    #endregion

    #region Methods

    /// <summary>
    /// Enqueues a new entity instance to be inserted into the database context tracking model asynchronously.
    /// </summary>
    /// <param name="entity">The entity instance to persist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);

    /// <summary>
    /// Resolves and retrieves a single entity instance filtering strictly by its unique primary key identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier integer key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching entity, or <see langword="null"/>.</returns>
    public async Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>().FindAsync([id], cancellationToken);

    /// <summary>
    /// Modifies the structural state mapping of an existing entity instance, marking it as modified within the memory scope.
    /// </summary>
    /// <param name="entity">The entity database record instance to update.</param>
    public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

    /// <summary>
    /// Prepares an tracked entity instance to be physically removed from the persistence store data layer.
    /// </summary>
    /// <param name="entity">The tracked entity database record instance to delete.</param>
    public void Remove(TEntity entity) => Context.Set<TEntity>().Remove(entity);

    /// <summary>
    /// Queries and maps the complete sequence of records matching the schema definition into an enumerable evaluation sequence collection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of entities.</returns>
    public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>().ToListAsync(cancellationToken);

    #endregion
}