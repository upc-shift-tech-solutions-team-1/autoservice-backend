namespace AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;

/// <summary>
/// Represents a Workshop aggregate root acting as the main tenant boundary within the Tenant Management domain.
/// </summary>
public class Workshop
{
    #region Properties

    /// <summary>
    /// Gets the unique structural database identifier key for the workshop instance.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the commercial brand or business name of the workshop.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the unique custom-generated tenant key indicator token used for cross-context tracking segregation.
    /// </summary>
    public string TenantId { get; private set; } 

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Workshop"/> class and maps its baseline unique upper-case Tenant identity token string.
    /// </summary>
    /// <param name="name">The commercial or corporate name profile text criteria.</param>
    public Workshop(string name)
    {
        Name = name;
        TenantId = $"WS-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Workshop"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>
    protected Workshop() 
    {
        Name = string.Empty;
        TenantId = string.Empty;
    }

    #endregion
}