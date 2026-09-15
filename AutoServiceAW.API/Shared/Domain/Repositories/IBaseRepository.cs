namespace AutoServiceAW.API.Shared.Domain.Repositories;

/// <summary>
/// Defines the generic base repository contract for standard CRUD operations on domain entities.
/// </summary>
/// <typeparam name="TEntity">The type of the domain entity.</typeparam>
public interface IBaseRepository<TEntity>
{
    #region Methods

    /// <summary>
    /// Adds a new entity to the data persistence tracking context asynchronously.
    /// </summary>
    /// <param name="entity">The entity instance to be registered.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an entity by its unique integer identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique integer identity key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching entity, or <see langword="null"/>.</returns>
    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity as modified within the tracking context.
    /// </summary>
    /// <param name="entity">The entity instance containing updated information.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Marks an existing entity to be removed from the persistence store.
    /// </summary>
    /// <param name="entity">The entity instance to be deleted.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Retrieves all recorded entities from the data persistence store asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of entities.</returns>
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    #endregion
}