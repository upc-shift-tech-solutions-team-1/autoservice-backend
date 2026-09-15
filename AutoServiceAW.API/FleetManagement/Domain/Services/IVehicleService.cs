using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.FleetManagement.Domain.Services;

/// <summary>
/// Defines the service contract for managing vehicle domain operations within the fleet.
/// </summary>
public interface IVehicleService
{
    #region Methods

    /// <summary>
    /// Coordinates the creation of a new vehicle in the fleet.
    /// </summary>
    /// <param name="vehicle">The vehicle entity data to register.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Vehicle"/> or <see langword="null"/> if the operation fails.</returns>
    Task<Vehicle?> CreateAsync(Vehicle vehicle);

    /// <summary>
    /// Coordinates the retrieval of all registered vehicles in the fleet.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Vehicle"/> entities.</returns>
    Task<IEnumerable<Vehicle>> ListAsync();

    /// <summary>
    /// Coordinates the retrieval of a specific vehicle by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="Vehicle"/>, or <see langword="null"/> if no match is found.</returns>
    Task<Vehicle?> GetByIdAsync(int id);

    /// <summary>
    /// Coordinates the update of an existing vehicle's data.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to update.</param>
    /// <param name="vehicle">The vehicle entity containing the updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Vehicle"/>, or <see langword="null"/> if the vehicle does not exist.</returns>
    Task<Vehicle?> UpdateAsync(int id, Vehicle vehicle);

    /// <summary>
    /// Coordinates the deletion of a vehicle from the fleet.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(int id);

    #endregion
}