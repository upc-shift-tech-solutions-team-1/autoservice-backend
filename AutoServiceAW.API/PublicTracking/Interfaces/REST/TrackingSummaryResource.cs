namespace AutoServiceAW.API.PublicTracking.Interfaces.REST;

public sealed record TrackingSummaryResource(
    string TrackingCode,
    string Status,
    decimal ProgressPercentage,
    string EstimatedDate,
    IReadOnlyList<TrackingTaskResource> Tasks,
    TrackingCostBreakdownResource Costs,
    IReadOnlyList<TrackingStageResource> History
);

public sealed record TrackingTaskResource(
    string Description,
    string Status,
    string TechnicalDiagnosis,
    string CustomerExplanation,
    string EvidenceRegistered,
    decimal LaborPrice,
    decimal MaterialsCost,
    decimal TotalCost,
    IReadOnlyList<TrackingTaskPartResource> Parts
);

public sealed record TrackingWorkOrderResource(
    string TrackingCode,
    string Status,
    string EstimatedDate,
    int VehicleId,
    int CustomerId,
    string WorkshopId
);

public sealed record TrackingTaskPartResource(
    string Name,
    int Quantity,
    decimal UnitPrice
);

public sealed record TrackingCostBreakdownResource(
    decimal LaborSubtotal,
    decimal MaterialsSubtotal,
    decimal Total
);

public sealed record TrackingStageResource(
    string Status,
    DateTime ChangedAtUtc
);
