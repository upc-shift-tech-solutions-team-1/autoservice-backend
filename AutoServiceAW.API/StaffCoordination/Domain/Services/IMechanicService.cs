using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;

namespace AutoServiceAW.API.StaffCoordination.Domain.Services;

/// <summary>
/// Defines the service contract for coordinating mechanic profiles and tracking capacities within the workshop network.
/// </summary>
public interface IMechanicService
{
    #region Methods

    /// <summary>
    /// Coordinates the setup operation bounds of a mechanic along with their remote system security login structures.
    /// </summary>
    /// <param name="mechanic">The domain parameters dataset aggregate package.</param>
    /// <param name="password">The raw text value needed to provision credential layers.</param>
    /// <param name="workshopId">The corporate platform tenant owner allocation identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the validated domain model result.</returns>
    Task<Mechanic?> CreateAsync(Mechanic mechanic, string password, string workshopId);
    
    /// <summary>
    /// Coordinates the asynchronous collection aggregation retrieval pipelines of technical actors.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Mechanic"/> records.</returns>
    Task<IEnumerable<Mechanic>> ListAsync();

    /// <summary>
    /// Coordinates target entity record discovery matching the baseline numerical identifier index key.
    /// </summary>
    /// <param name="id">The target database tracking identity sequence index code.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the discovered asset match envelope model.</returns>
    Task<Mechanic?> GetByIdAsync(int id);

    /// <summary>
    /// Coordinates the entity modification lifecycle workflow steps over the tracking profile.
    /// </summary>
    /// <param name="id">The target tracking entity index record lookup parameter filter.</param>
    /// <param name="mechanic">The altered parameters set state blueprint.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the evaluated update status object outcome.</returns>
    Task<Mechanic?> UpdateAsync(int id, Mechanic mechanic);

    /// <summary>
    /// Coordinates irreversible structural physical wipe routines removing tracked staff data.
    /// </summary>
    /// <param name="id">The technical staff tracking index identifier to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(int id);

    #endregion
}