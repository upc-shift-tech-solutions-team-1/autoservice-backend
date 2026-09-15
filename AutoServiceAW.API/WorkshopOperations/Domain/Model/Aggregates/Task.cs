namespace AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

public class Task
{
    public int Id { get; private set; }
    public int WorkOrderId { get; private set; }
    public int? MechanicId { get; private set; }
    public string Description { get; private set; }
    public string Status { get; private set; }
    public string Priority { get; private set; }
    public int EstimatedTime { get; private set; }
    public decimal LaborCost { get; private set; }
    public decimal LaborPrice { get; private set; }
    public decimal MaterialsPurchaseCost { get; private set; }
    public decimal MaterialsCost { get; private set; }
    public string TechnicalDiagnosis { get; private set; } = string.Empty;
    public string CustomerExplanation { get; private set; } = string.Empty;
    public string InternalObservation { get; private set; } = string.Empty;
    public string EvidenceRegistered { get; private set; } = string.Empty;
    public string AdminReviewStatus { get; private set; } = string.Empty;

    public decimal TotalCost => LaborPrice + MaterialsCost;
    public decimal TotalInternalCost => LaborCost + MaterialsPurchaseCost;
    public decimal GrossProfit => TotalCost - TotalInternalCost;
    public decimal MarginPercentage => TotalCost == 0
        ? 0
        : Math.Round((GrossProfit / TotalCost) * 100, 2);

    public ICollection<TaskPart> Parts { get; private set; } = new List<TaskPart>();

    public Task(
        int workOrderId,
        int? mechanicId,
        string description,
        string status,
        string priority,
        int estimatedTime,
        decimal laborPrice,
        decimal laborCost = 0)
    {
        WorkOrderId = workOrderId;
        MechanicId = mechanicId;
        Description = description;
        Status = status;
        Priority = priority;
        EstimatedTime = estimatedTime;
        LaborPrice = laborPrice;
        LaborCost = ResolveLaborCost(laborCost, laborPrice);
    }

    protected Task()
    {
        Description = string.Empty;
        Status = string.Empty;
        Priority = string.Empty;
    }

    public void Update(
        string description,
        string status,
        string priority,
        int estimatedTime,
        decimal laborPrice,
        int? mechanicId,
        decimal laborCost = 0)
    {
        Description = description;
        Status = status;
        Priority = priority;
        EstimatedTime = estimatedTime;
        LaborPrice = laborPrice;
        LaborCost = ResolveLaborCost(laborCost, laborPrice);
        MechanicId = mechanicId;
    }

    public void PatchTechnicalData(
        string status,
        string diagnosis,
        string explanation,
        string observation,
        string evidence,
        string reviewStatus)
    {
        Status = string.IsNullOrEmpty(status) ? Status : status;
        TechnicalDiagnosis = diagnosis ?? string.Empty;
        CustomerExplanation = explanation ?? string.Empty;
        InternalObservation = observation ?? string.Empty;
        EvidenceRegistered = evidence ?? string.Empty;
        AdminReviewStatus = reviewStatus ?? string.Empty;
    }

    public void UpdateMaterialsCost(decimal materialsCost, decimal materialsPurchaseCost)
    {
        MaterialsCost = materialsCost;
        MaterialsPurchaseCost = materialsPurchaseCost;
    }

    public void AddPart(TaskPart part)
    {
        Parts.Add(part);
    }

    private static decimal ResolveLaborCost(decimal laborCost, decimal laborPrice)
    {
        if (laborCost > 0)
            return laborCost;

        return laborPrice > 0 ? Math.Round(laborPrice * 0.50m, 2) : 0;
    }
}
