using Kaizentro.Domain;

namespace Kaizentro.Application.Import;

public sealed record RoutingImportIssue(int RowNumber, string Field, string Message);

public sealed record RoutingImportResult(
    Material Material,
    ProductionDemand Demand,
    IReadOnlyDictionary<Guid, WorkCenter> WorkCenters,
    IReadOnlyList<RoutingOperation> Operations,
    IReadOnlyList<RoutingImportIssue> Issues)
{
    public bool HasIssues => Issues.Count > 0;
    public bool HasUsableRouting => Operations.Count > 0 && Demand.DailyDemand > 0 && Demand.AvailableProductionMinutesPerDay > 0;
}
