using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Services;

namespace AutoServiceAW.API.StaffCoordination.Application.Internal;

/// <summary>
/// Provides internal application services for managing mechanic staff coordination,
/// orchestrating synchronization between domain registration and access identities (IAM).
/// </summary>
public class MechanicService(IMechanicRepository mechanicRepository, IAuthService authService, IUnitOfWork unitOfWork) : IMechanicService
{
    #region Methods

    /// <summary>
    /// Coordinates the registration of a mechanic profile and forces a synchronization entry with identity access controls.
    /// </summary>
    /// <param name="mechanic">The technical staff profile aggregate root metadata reference.</param>
    /// <param name="password">The plain text security string required for creating matching access credentials.</param>
    /// <param name="workshopId">The system tenant unique target workshop context boundary identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Mechanic"/> profile state.</returns>
    /// <exception cref="Exception">Thrown when authentication credential creation fails under the IAM service context.</exception>
    public async Task<Mechanic?> CreateAsync(Mechanic mechanic, string password, string workshopId)
    {
        // 1. Cross-context sync: Create credentials first in the IAM context
        var user = await authService.SignUpAsync(mechanic.Email, password, "mechanic", workshopId);
        
        if (user == null) 
            throw new Exception("Error generating access credentials.");

        // 2. Persist mechanic aggregate domain metrics data state
        await mechanicRepository.AddAsync(mechanic);
        await unitOfWork.CompleteAsync();
        
        return mechanic;
    }

    /// <summary>
    /// Coordinates the retrieval of all mechanic profiles registered in the system asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="Mechanic"/> entities.</returns>
    public async Task<IEnumerable<Mechanic>> ListAsync() => await mechanicRepository.ListAsync();
    
    /// <summary>
    /// Coordinates the retrieval of a specific mechanic by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique integer identity identifier key sequence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="Mechanic"/>, or <see langword="null"/>.</returns>
    public async Task<Mechanic?> GetByIdAsync(int id) => await mechanicRepository.FindByIdAsync(id);

    /// <summary>
    /// Coordinates the update of an existing mechanic's information profile metrics data state asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the target mechanic entity to alter.</param>
    /// <param name="updatedMechanic">The temporary instance domain container holding the updated entity parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Mechanic"/>, or <see langword="null"/>.</returns>
    public async Task<Mechanic?> UpdateAsync(int id, Mechanic updatedMechanic)
    {
        var existingMechanic = await mechanicRepository.FindByIdAsync(id);
        if (existingMechanic == null) return null;

        existingMechanic.Update(updatedMechanic.FullName, updatedMechanic.Specialty, updatedMechanic.MaxCapacity, updatedMechanic.Email, updatedMechanic.Password);
        mechanicRepository.Update(existingMechanic);
        await unitOfWork.CompleteAsync();
        
        return existingMechanic;
    }

    /// <summary>
    /// Deletes a specific mechanic from tracking structures by their unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the target mechanic profile to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAsync(int id)
    {
        var existingMechanic = await mechanicRepository.FindByIdAsync(id);
        if (existingMechanic != null)
        {
            mechanicRepository.Remove(existingMechanic);
            await unitOfWork.CompleteAsync();
        }
    }

    #endregion
}