using Kaizentro.Domain;

namespace Kaizentro.Application;

public sealed class ValueStreamAnalysisService
{
    public ValueStreamMap BuildCurrentState(
        Material material,
        ProductionDemand demand,
        IReadOnlyDictionary<Guid, WorkCenter> workCenters,
        IReadOnlyList<RoutingOperation> routing)
    {
        if (demand.DailyDemand <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(demand), "Daily demand must be greater than zero.");
        }

        if (demand.AvailableProductionMinutesPerDay <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(demand), "Available production minutes must be greater than zero.");
        }

        var taktTimeSeconds = demand.AvailableProductionMinutesPerDay * 60m / demand.DailyDemand;
        var ordered = routing.OrderBy(operation => operation.Sequence).ToList();
        var maxCycle = ordered.Count == 0 ? 0m : ordered.Max(operation => operation.CycleTimeSeconds);

        var opportunities = new List<KaizenOpportunity>();
        var processBoxes = new List<ProcessBox>();

        foreach (var operation in ordered)
        {
            if (!workCenters.TryGetValue(operation.WorkCenterId, out var workCenter))
            {
                throw new InvalidOperationException($"Missing work center for operation {operation.Sequence}.");
            }

            var isBottleneck = operation.CycleTimeSeconds >= maxCycle || operation.CycleTimeSeconds > taktTimeSeconds;

            processBoxes.Add(new ProcessBox(
                operation.Sequence,
                workCenter.Code,
                operation.OperationName,
                operation.CycleTimeSeconds,
                operation.SetupTimeMinutes,
                operation.QueueTimeMinutes,
                operation.MoveTimeMinutes,
                operation.Operators,
                operation.YieldPercent,
                isBottleneck));

            opportunities.AddRange(AnalyzeOperation(operation, workCenter, taktTimeSeconds));
        }

        var totalCycleTimeSeconds = ordered.Sum(operation => operation.CycleTimeSeconds);
        var totalLeadTimeMinutes = ordered.Sum(operation => operation.CycleTimeSeconds / 60m + operation.SetupTimeMinutes + operation.QueueTimeMinutes + operation.MoveTimeMinutes);
        var valueAddedMinutes = totalCycleTimeSeconds / 60m;
        var valueAddedRatio = totalLeadTimeMinutes == 0m ? 0m : decimal.Round(valueAddedMinutes / totalLeadTimeMinutes * 100m, 2);

        return new ValueStreamMap(
            material.Id,
            material.PartNumber,
            decimal.Round(taktTimeSeconds, 2),
            decimal.Round(totalCycleTimeSeconds, 2),
            decimal.Round(totalLeadTimeMinutes, 2),
            valueAddedRatio,
            processBoxes,
            opportunities.OrderByDescending(item => item.PriorityScore).ToList());
    }

    private static IEnumerable<KaizenOpportunity> AnalyzeOperation(RoutingOperation operation, WorkCenter workCenter, decimal taktTimeSeconds)
    {
        if (operation.CycleTimeSeconds > taktTimeSeconds)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Waiting / Constraint",
                $"{operation.OperationName} cycle time is above takt.",
                "Review line balance, standard work, staffing, and equipment constraints before adding capital.",
                EstimateSavings(operation.CycleTimeSeconds - taktTimeSeconds, operation.Operators),
                95);
        }

        if (operation.SetupTimeMinutes > 30m)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Changeover",
                $"{operation.OperationName} setup time is {operation.SetupTimeMinutes} minutes.",
                "Launch a SMED event. Separate internal/external work, pre-stage tooling, and standardize changeover sequence.",
                EstimateSavings(operation.SetupTimeMinutes * 60m, operation.Operators),
                88);
        }

        if (operation.QueueTimeMinutes > operation.CycleTimeSeconds / 60m * 5m)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Inventory / Waiting",
                $"{operation.OperationName} has excessive queue time compared with processing time.",
                "Evaluate FIFO lanes, supermarket sizing, scheduling frequency, and batch-size reduction.",
                25000m,
                78);
        }

        if (operation.YieldPercent < 98m)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Defects",
                $"{operation.OperationName} yield is below 98%.",
                "Open an A3 / root-cause project focused on defect containment, source inspection, and process capability.",
                40000m,
                82);
        }
    }

    private static decimal EstimateSavings(decimal secondsLost, int operators)
    {
        const decimal loadedLaborRate = 38m;
        const decimal annualProductionDays = 250m;
        var annualHours = secondsLost / 3600m * annualProductionDays * Math.Max(operators, 1);
        return decimal.Round(annualHours * loadedLaborRate, 2);
    }
}
