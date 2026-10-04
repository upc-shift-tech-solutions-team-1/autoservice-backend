using System.Text.Json.Serialization;

namespace AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

/// <summary>
/// Records a real work-order status change with its UTC timestamp.
/// </summary>
public class WorkOrderStatusHistory
{
    public int Id { get; private set; }
    public int WorkOrderId { get; private set; }

    [JsonIgnore]
    public WorkOrder WorkOrder { get; private set; } = null!;

    public string Status { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }

    protected WorkOrderStatusHistory()
    {
        Status = string.Empty;
    }

    public WorkOrderStatusHistory(string status, DateTime changedAtUtc)
    {
        Status = status;
        ChangedAtUtc = DateTime.SpecifyKind(changedAtUtc, DateTimeKind.Utc);
    }
}
