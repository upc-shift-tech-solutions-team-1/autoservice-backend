namespace AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

/// <summary>
/// Represents a WorkOrder aggregate root managing vehicle processing folders and checklist execution progress within a tenant scope.
/// </summary>
public class WorkOrder
{
    #region Properties

    /// <summary>
    /// Gets the unique operational tracking database identifier primary key index row code.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the unique custom tenant identifier token isolating data operations inside the multi-tenant context.
    /// </summary>
    public string WorkshopId { get; private set; }

    /// <summary>
    /// Gets the automatically generated alphanumeric public tracking tracking code string identifier.
    /// </summary>
    public string TrackingCode { get; private set; }

    /// <summary>
    /// Gets the unique identifier tracking index matching the vehicle asset folder under repair.
    /// </summary>
    public int VehicleId { get; private set; }

    /// <summary>
    /// Gets the unique identifier tracking index matching the customer client owner reference profile.
    /// </summary>
    public int CustomerId { get; private set; }

    /// <summary>
    /// Gets the unique identifier tracking index matching the manager or main mechanic technician actor in charge.
    /// </summary>
    public int MechanicId { get; private set; }

    /// <summary>
    /// Gets the deep descriptive initial vehicle entry reception summary or diagnosis target instructions.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the high-level global state index condition string marking overall completion metrics.
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// Gets the aggregated calculation price fee summary including labor valuations and spare parts total metrics.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Gets the expected commitment target date text sequence boundary for vehicle delivery.
    /// </summary>
    public string EstimatedDate { get; private set; }

    /// <summary>
    /// Gets the initialization timeline event registration record timestamp string.
    /// </summary>
    public string StartDate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether all sub-tasks tasks validations inside the service collection are finished.
    /// </summary>
    public bool TasksCompleted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether allocated warehouse tracking items and replacement spare parts allocations are cross-checked.
    /// </summary>
    public bool SparePartsChecked { get; private set; }

    /// <summary>
    /// Gets a value indicating whether global automotive diagnosis reports are validated and approved.
    /// </summary>
    public bool DiagnosisValidated { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the final automotive interior/exterior cleaning maintenance block steps are finished.
    /// </summary>
    public bool CleaningDone { get; private set; }

    /// <summary>
    /// Gets a value indicating whether road test safety examinations or mechanical simulation proofs are concluded.
    /// </summary>
    public bool FinalTestDone { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkOrder"/> class setting core parameters and compiling identity tracking tokens.
    /// </summary>
    /// <param name="workshopId">The tenant workshop identification custom code.</param>
    /// <param name="vehicleId">The target automotive unit asset tracking key.</param>
    /// <param name="customerId">The target owner client tracking profile index.</param>
    /// <param name="mechanicId">The tracking index reference allocation point mapping the technical lead.</param>
    /// <param name="description">The initial diagnosis service descriptive requirements script text.</param>
    /// <param name="estimatedDate">The expected commitment target date parameters text timeline.</param>
    /// <param name="price">The base calculation global pricing estimation entry framework metric.</param>
    public WorkOrder(string workshopId, int vehicleId, int customerId, int mechanicId, string description, string estimatedDate, decimal price)
    {
        WorkshopId = workshopId;
        VehicleId = vehicleId;
        CustomerId = customerId;
        MechanicId = mechanicId;
        Description = description;
        EstimatedDate = estimatedDate;
        Price = price;
        Status = "PENDING"; 
        TrackingCode = $"WO-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        StartDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkOrder"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>
    protected WorkOrder()
    {
        WorkshopId = string.Empty;
        TrackingCode = string.Empty;
        Description = string.Empty;
        Status = string.Empty;
        EstimatedDate = string.Empty;
        StartDate = string.Empty;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates structural customer core descriptions, timeline milestones, and global pricing configurations.
    /// </summary>
    /// <param name="description">The revised descriptive summary text script parameters.</param>
    /// <param name="estimatedDate">The revised expected delivery tracking target timeline date text.</param>
    /// <param name="price">The modified total financial billing price valuation indicator metric.</param>
    public void Update(string description, string estimatedDate, decimal price)
    {
        Description = description;
        EstimatedDate = estimatedDate;
        Price = price;
    }

    /// <summary>
    /// Updates structural inspection checklist parameters tracking operational execution quality validation checkpoints.
    /// </summary>
    /// <param name="tasksCompleted">The current evaluation flag state matching assigned task completions.</param>
    /// <param name="sparePartsChecked">The current evaluation flag state tracking warehouse asset supply allocations.</param>
    /// <param name="diagnosisValidated">The current evaluation flag state tracing master analytical report sign-offs.</param>
    /// <param name="cleaningDone">The current evaluation flag state marking car detailing completion steps.</param>
    /// <param name="finalTestDone">The current evaluation flag state marking road test execution checkoffs.</param>
    public void UpdateChecklist(bool tasksCompleted, bool sparePartsChecked, bool diagnosisValidated, bool cleaningDone, bool finalTestDone)
    {
        TasksCompleted = tasksCompleted;
        SparePartsChecked = sparePartsChecked;
        DiagnosisValidated = diagnosisValidated;
        CleaningDone = cleaningDone;
        FinalTestDone = finalTestDone;
    }

    /// <summary>
    /// Safely updates the high-level operational lifecycle flow tracking parameter configuration state.
    /// </summary>
    /// <param name="status">The incoming state target progress indicator identifier string evaluation criteria.</param>
    public void UpdateStatus(string status)
    {
        if (!string.IsNullOrEmpty(status))
        {
            Status = status;
        }
    }

    #endregion
}