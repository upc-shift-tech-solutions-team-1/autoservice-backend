using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceAW.API.IAM.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IUserRepository"/> contract 
/// using Entity Framework Core and inheriting generic operations from <see cref="BaseRepository{User}"/>.
/// </summary>
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    #region Methods

    /// <summary>
    /// Retrieves a user from the Entity Framework context by mapping their unique email address.
    /// </summary>
    /// <param name="email">The email address reference criteria.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the evaluated <see cref="User"/> or <see langword="null"/>.</returns>
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    #endregion
}