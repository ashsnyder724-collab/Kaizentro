using Kaizentro.Domain;

namespace Kaizentro.Application;

public sealed class ValueStreamAnalysisService
{
    public ValueStreamMap BuildCurrentState(
        Material material,
        ProductionDemand demand,
        IReadOnlyDictionary<Guid, WorkCenter> workCenters,
        IReadOnlyList<RoutingOperation> routing,
        IReadOnlyList<SavingsInput>? savingsInputs = null)
    {
        if (demand.DailyDemand <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(demand), "Daily demand must be greater than zero.");
        }

        if (demand.AvailableProductionMinutesPerDay <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(demand), "Available production minutes must be greater than zero.");
        }

        var inputs = savingsInputs ?? Array.Empty<SavingsInput>();
        SavingsCalculator.ValidateInputs(inputs);
        if (inputs.Any(input => input.MaterialId != material.Id ||
            !routing.Any(operation => operation.Sequence == input.OperationSequence)))
            throw new ArgumentException("Savings inputs must match this material and routing.", nameof(savingsInputs));
        if (routing.Select(operation => operation.Sequence).Distinct().Count() != routing.Count)
            throw new ArgumentException("Operation sequences must be unique.", nameof(routing));

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

            foreach (var opportunity in AnalyzeOperation(operation, workCenter, taktTimeSeconds))
            {
                var input = inputs.SingleOrDefault(item => item.OperationSequence == operation.Sequence && item.WasteType == opportunity.WasteType);
                var savings = SavingsCalculator.Calculate(input);
                opportunities.Add(opportunity with { EstimatedAnnualSavings = savings.ProjectedAnnualAmount, Savings = savings });
            }
        }

        if (inputs.Any(input => !ordered.Any(operation => operation.Sequence == input.OperationSequence &&
            AnalyzeOperation(operation, workCenters[operation.WorkCenterId], taktTimeSeconds)
                .Any(opportunity => opportunity.WasteType == input.WasteType))))
            throw new ArgumentException("Savings scenario does not match an identified opportunity.", nameof(savingsInputs));

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
                null,
                95);
        }

        if (operation.SetupTimeMinutes > 30m)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Changeover",
                $"{operation.OperationName} setup time is {operation.SetupTimeMinutes} minutes.",
                "Launch a SMED event. Separate internal/external work, pre-stage tooling, and standardize changeover sequence.",
                null,
                88);
        }

        if (operation.QueueTimeMinutes > operation.CycleTimeSeconds / 60m * 5m)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Inventory / Waiting",
                $"{operation.OperationName} has excessive queue time compared with processing time.",
                "Evaluate FIFO lanes, supermarket sizing, scheduling frequency, and batch-size reduction.",
                null,
                78);
        }

        if (operation.YieldPercent < 98m)
        {
            yield return new KaizenOpportunity(
                workCenter.Code,
                "Defects",
                $"{operation.OperationName} yield is below 98%.",
                "Open an A3 / root-cause project focused on defect containment, source inspection, and process capability.",
                null,
                82);
        }
    }

}
