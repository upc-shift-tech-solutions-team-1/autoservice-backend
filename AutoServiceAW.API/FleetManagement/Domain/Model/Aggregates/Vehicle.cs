namespace AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;

/// <summary>
/// Represents a Vehicle aggregate root within the Fleet Management domain.
/// </summary>
public class Vehicle
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier for the vehicle.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the license plate number of the vehicle.
    /// </summary>
    public string Plate { get; private set; }

    /// <summary>
    /// Gets the manufacturer or brand of the vehicle.
    /// </summary>
    public string Brand { get; private set; }

    /// <summary>
    /// Gets the specific model of the vehicle.
    /// </summary>
    public string Model { get; private set; }

    /// <summary>
    /// Gets the manufacturing year of the vehicle.
    /// </summary>
    public string Year { get; private set; }

    /// <summary>
    /// Gets the exterior color of the vehicle.
    /// </summary>
    public string Color { get; private set; }

    /// <summary>
    /// Gets the current status of the vehicle (e.g., In Repair, Ready, Active).
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// Gets the image URL or path associated with the vehicle.
    /// </summary>
    public string Image { get; private set; }

    /// <summary>
    /// Gets the identifier of the customer who owns the vehicle.
    /// </summary>
    public int CustomerId { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Vehicle"/> class with specified technical and owner details.
    /// </summary>
    /// <param name="plate">The vehicle's license plate.</param>
    /// <param name="brand">The vehicle's brand.</param>
    /// <param name="model">The vehicle's model.</param>
    /// <param name="year">The vehicle's manufacturing year.</param>
    /// <param name="color">The vehicle's color.</param>
    /// <param name="status">The current maintenance or operational status.</param>
    /// <param name="image">The image file path or reference.</param>
    /// <param name="customerId">The unique identifier of the owning customer.</param>
    public Vehicle(string plate, string brand, string model, string year, string color, string status, string image, int customerId)
    {
        Plate = plate;
        Brand = brand;
        Model = model;
        Year = year;
        Color = color;
        Status = status;
        Image = image;
        CustomerId = customerId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vehicle"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>
    protected Vehicle()
    {
        Plate = string.Empty;
        Brand = string.Empty;
        Model = string.Empty;
        Year = string.Empty;
        Color = string.Empty;
        Status = string.Empty;
        Image = string.Empty;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the vehicle's technical details, status, and owner assignment.
    /// </summary>
    /// <param name="plate">The new license plate number.</param>
    /// <param name="brand">The new brand name.</param>
    /// <param name="model">The new model name.</param>
    /// <param name="year">The new manufacturing year.</param>
    /// <param name="color">The new exterior color.</param>
    /// <param name="status">The updated operational status.</param>
    /// <param name="image">The updated image resource identifier.</param>
    /// <param name="customerId">The new customer identifier if ownership changes.</param>
    public void Update(string plate, string brand, string model, string year, string color, string status, string image, int customerId)
    {
        Plate = plate;
        Brand = brand;
        Model = model;
        Year = year;
        Color = color;
        Status = status;
        Image = image;
        CustomerId = customerId;
    }

    #endregion
}