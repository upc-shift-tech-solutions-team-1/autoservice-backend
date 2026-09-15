namespace AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;

/// <summary>
/// Represents a Customer aggregate root within the Customer Management domain.
/// </summary>
public class Customer
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier for the customer.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the workshop associated with this customer.
    /// </summary>
    public string WorkshopId { get; private set; } 

    /// <summary>
    /// Gets the full name of the customer.
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Gets the National Identity Document (DNI) of the customer.
    /// </summary>
    public string Dni { get; private set; }

    /// <summary>
    /// Gets the email address of the customer.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Gets the telephone or phone number of the customer.
    /// </summary>
    public string Phone { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class with specified details.
    /// </summary>
    /// <param name="workshopId">The identifier of the workshop.</param>
    /// <param name="fullName">The full name of the customer.</param>
    /// <param name="dni">The National Identity Document (DNI) of the customer.</param>
    /// <param name="email">The email address of the customer.</param>
    /// <param name="phone">The phone number of the customer.</param>
    public Customer(string workshopId, string fullName, string dni, string email, string phone)
    {
        WorkshopId = workshopId;
        FullName = fullName;
        Dni = dni;
        Email = email;
        Phone = phone;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>
    protected Customer()
    {
        WorkshopId = string.Empty;
        FullName = string.Empty;
        Dni = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the customer's profile details.
    /// </summary>
    /// <param name="fullName">The new full name of the customer.</param>
    /// <param name="dni">The new National Identity Document (DNI) of the customer.</param>
    /// <param name="email">The new email address of the customer.</param>
    /// <param name="phone">The new phone number of the customer.</param>
    public void Update(string fullName, string dni, string email, string phone)
    {
        FullName = fullName;
        Dni = dni;
        Email = email;
        Phone = phone;
    }

    #endregion
}