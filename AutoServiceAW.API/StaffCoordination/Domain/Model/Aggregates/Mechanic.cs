namespace AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;

/// <summary>
/// Represents a Mechanic aggregate root within the Staff Coordination context boundary domain.
/// </summary>
public class Mechanic
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier for the mechanic.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the full legal or preferred name configuration values of the technical member.
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Gets the specialized automotive skill sector focus category (e.g., Electronics, Brakes).
    /// </summary>
    public string Specialty { get; private set; }

    /// <summary>
    /// Gets the unique internal platform coordination contact email address.
    /// </summary>
    public string Email { get; private set; }
    
    /// <summary>
    /// Gets the maximum active tracking operational capacity workload score index metric.
    /// </summary>
    public int MaxCapacity { get; private set; }
    public string Password { get; private set; }

    /// <summary>
    /// Gets the unique assigned tenant business workshop context identifier string.
    /// </summary>
    public string WorkshopId { get; private set; } 

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Mechanic"/> class setting metric limits and company scope dependencies.
    /// </summary>
    /// <param name="fullName">The full descriptor name reference.</param>
    /// <param name="specialty">The core knowledge specialization label.</param>
    /// <param name="maxCapacity">The baseline workflow tracking threshold capability score.</param>
    /// <param name="email">The tracking identifier address point.</param>
    /// <param name="workshopId">The assigned business tenant scope context identifier.</param>
    public Mechanic(string fullName, string specialty, int maxCapacity, string email, string workshopId, string password)
    {
        FullName = fullName;
        Specialty = specialty;
        MaxCapacity = maxCapacity;
        Email = email;
        WorkshopId = workshopId;
        Password = password;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mechanic"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>
    protected Mechanic() 
    {
        FullName = string.Empty;
        Specialty = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        WorkshopId = string.Empty;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates core profile structures, specialized capability metadata, and contact details records.
    /// </summary>
    /// <param name="fullName">The altered legal full name text.</param>
    /// <param name="specialty">The updated knowledge specialty designation.</param>
    /// <param name="maxCapacity">The structural capacity index adjustments.</param>
    /// <param name="email">The modified coordination communication email mapping context.</param>
    public void Update(string fullName, string specialty, int maxCapacity, string email, string password)
    {
        FullName = fullName;
        Specialty = specialty;
        MaxCapacity = maxCapacity;
        Email = email;
        Password = password;
    }

    #endregion
}