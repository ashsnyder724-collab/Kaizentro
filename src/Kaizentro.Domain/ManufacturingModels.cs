namespace Kaizentro.Domain;

public sealed record Plant(Guid Id, string Name, string Location, string TimeZone);

public sealed record Department(Guid Id, Guid PlantId, string Name, string? Manager);

public sealed record WorkCenter(
    Guid Id,
    Guid DepartmentId,
    string Code,
    string Name,
    decimal AvailableMinutesPerShift,
    decimal EfficiencyPercent);

public sealed record Material(
    Guid Id,
    string PartNumber,
    string Description,
    string ProductFamily);

public sealed record RoutingOperation(
    int Sequence,
    Guid MaterialId,
    Guid WorkCenterId,
    string OperationName,
    decimal CycleTimeSeconds,
    decimal SetupTimeMinutes,
    decimal QueueTimeMinutes,
    decimal MoveTimeMinutes,
    int Operators,
    decimal YieldPercent);

public sealed record ProductionDemand(
    Guid MaterialId,
    int DailyDemand,
    decimal AvailableProductionMinutesPerDay);

public sealed record ProcessBox(
    int Sequence,
    string WorkCenterCode,
    string OperationName,
    decimal CycleTimeSeconds,
    decimal SetupTimeMinutes,
    decimal QueueTimeMinutes,
    decimal MoveTimeMinutes,
    int Operators,
    decimal YieldPercent,
    bool IsBottleneck);

public sealed record ValueStreamMap(
    Guid MaterialId,
    string PartNumber,
    decimal TaktTimeSeconds,
    decimal TotalCycleTimeSeconds,
    decimal TotalLeadTimeMinutes,
    decimal ValueAddedRatioPercent,
    IReadOnlyList<ProcessBox> Processes,
    IReadOnlyList<KaizenOpportunity> Opportunities);

public sealed record KaizenOpportunity(
    string Area,
    string WasteType,
    string ProblemStatement,
    string Recommendation,
    decimal EstimatedAnnualSavings,
    int PriorityScore);
