namespace AutoServiceAW.API.Shared.Domain.Repositories;

/// <summary>
/// Defines the contract for the Unit of Work pattern to coordinate structural transaction commits across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    #region Methods

    /// <summary>
    /// Commits all tracked context state alterations directly to the underlying persistence transaction asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CompleteAsync();

    #endregion
}