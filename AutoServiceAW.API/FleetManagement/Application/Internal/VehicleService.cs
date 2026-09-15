using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.FleetManagement.Application.Internal;

/// <summary>
/// Provides internal application services for managing vehicle-related operations within the fleet.
/// </summary>
public class VehicleService(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork) : IVehicleService
{
    #region Methods

    /// <summary>
    /// Creates a new vehicle in the system asynchronously.
    /// </summary>
    /// <param name="vehicle">The vehicle entity to be created.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Vehicle"/> entity.</returns>
    public async Task<Vehicle?> CreateAsync(Vehicle vehicle)
    {
        await vehicleRepository.AddAsync(vehicle);
        await unitOfWork.CompleteAsync();
        return vehicle;
    }

    /// <summary>
    /// Retrieves all vehicles registered in the system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="Vehicle"/> entities.</returns>
    public async Task<IEnumerable<Vehicle>> ListAsync()
    {
        return await vehicleRepository.ListAsync();
    }
    
    /// <summary>
    /// Retrieves a specific vehicle by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="Vehicle"/>, or <see langword="null"/> if not found.</returns>
    public async Task<Vehicle?> GetByIdAsync(int id) => await vehicleRepository.FindByIdAsync(id);

    /// <summary>
    /// Updates an existing vehicle's details asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to update.</param>
    /// <param name="updatedVehicle">The vehicle entity containing the updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Vehicle"/>, or <see langword="null"/> if the vehicle does not exist.</returns>
    public async Task<Vehicle?> UpdateAsync(int id, Vehicle updatedVehicle)
    {
        var existingVehicle = await vehicleRepository.FindByIdAsync(id);
        if (existingVehicle == null) return null;

        existingVehicle.Update(updatedVehicle.Plate, updatedVehicle.Brand, updatedVehicle.Model, updatedVehicle.Year, updatedVehicle.Color, updatedVehicle.Status, updatedVehicle.Image, updatedVehicle.CustomerId);
        vehicleRepository.Update(existingVehicle);
        await unitOfWork.CompleteAsync();
        return existingVehicle;
    }

    /// <summary>
    /// Deletes a specific vehicle from the system by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAsync(int id)
    {
        var existingVehicle = await vehicleRepository.FindByIdAsync(id);
        if (existingVehicle != null)
        {
            vehicleRepository.Remove(existingVehicle);
            await unitOfWork.CompleteAsync();
        }
    }

    #endregion
}