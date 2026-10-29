namespace AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

/// <summary>
/// Records the estimated delivery date declared by the workshop and each later change.
/// </summary>
public class WorkOrderEstimatedDateHistory
{
    public int Id { get; private set; }
    public int WorkOrderId { get; private set; }
    public string? PreviousEstimatedDate { get; private set; }
    public string EstimatedDate { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
    public WorkOrder WorkOrder { get; private set; } = null!;

    public WorkOrderEstimatedDateHistory(
        string? previousEstimatedDate,
        string estimatedDate,
        string? reason,
        DateTime changedAtUtc)
    {
        PreviousEstimatedDate = previousEstimatedDate;
        EstimatedDate = estimatedDate;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedAtUtc = DateTime.SpecifyKind(changedAtUtc, DateTimeKind.Utc);
    }

    protected WorkOrderEstimatedDateHistory()
    {
        EstimatedDate = string.Empty;
    }
}
