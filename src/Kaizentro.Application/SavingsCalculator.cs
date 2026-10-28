using Kaizentro.Domain;

namespace Kaizentro.Application;

public static class SavingsCalculator
{
    private static readonly string[] WasteTypes = { "Waiting / Constraint", "Changeover", "Inventory / Waiting", "Defects" };

    public static void ValidateInputs(IReadOnlyList<SavingsInput> inputs)
    {
        if (inputs.Any(input => input is null))
            throw new ArgumentException("Savings entries cannot be null.", nameof(inputs));
        if (inputs.GroupBy(input => (input.MaterialId, input.OperationSequence, input.WasteType)).Any(group => group.Count() > 1))
            throw new ArgumentException("Duplicate savings scenario for an operation and waste.", nameof(inputs));
        foreach (var input in inputs) Validate(input);
    }

    private static void Validate(SavingsInput input)
    {
        if (!Enum.IsDefined(input.Basis) || !WasteTypes.Contains(input.WasteType))
            throw new ArgumentException("Unknown savings basis or waste type.");
        if (input.Currency is null || input.Currency.Length != 3 || !input.Currency.All(c => c >= 'A' && c <= 'Z'))
            throw new ArgumentException("Supply a three-letter uppercase currency code.");
        if (string.IsNullOrWhiteSpace(input.EvidenceReference))
            throw new ArgumentException("A documented input evidence reference is required.");
        decimal?[] values = { input.EventsPerDay, input.MeasuredSecondsReducedPerEvent,
            input.AffectedOperators, input.LoadedLaborRatePerHour, input.UnitsStartedPerDay,
            input.AvoidableScrapCostPerUnit, input.AdditionalSaleableUnitsPerDay,
            input.DemandBackedAdditionalUnitsPerDay, input.ContributionMarginPerUnit };
        if (values.Any(value => value < 0) || input.AffectedOperators == 0 ||
            input.ProductionDaysPerYear is <= 0 or > 366 ||
            input.BaselineYieldPercent is < 0 or > 100 || input.ImprovedYieldPercent is < 0 or > 100 ||
            input.ImprovedYieldPercent < input.BaselineYieldPercent)
            throw new ArgumentOutOfRangeException(nameof(input), "Invalid schedule, rate, quantity, time, operator count, or yield.");
        if (input.Realized is { } actual &&
            (actual.Amount < 0 || actual.PeriodStart == default || actual.PeriodEnd < actual.PeriodStart ||
             actual.PeriodEnd >= DateOnly.FromDateTime(DateTime.UtcNow) ||
             string.IsNullOrWhiteSpace(actual.VerifiedBy) || string.IsNullOrWhiteSpace(actual.EvidenceReference)))
            throw new ArgumentException("Realized savings require a closed period, nonnegative amount, verifier and evidence.");
    }

    public static SavingsAssessment Calculate(SavingsInput? input)
    {
        if (input is null)
            return new(null, "Unquantified", "", "", "No documented financial inputs supplied.", null, null);
        Validate(input);
        decimal? amount;
        string formula;
        string kind;
        try
        {
            switch (input.Basis)
            {
                case SavingsBasis.LaborCapacity:
                    formula = "seconds reduced/event / 3600 × events/day × production days/year × affected operators × loaded hourly rate";
                    kind = "Projected labor capacity value (not cash savings)";
                    amount = input.MeasuredSecondsReducedPerEvent / 3600m * input.EventsPerDay *
                        input.ProductionDaysPerYear * input.AffectedOperators * input.LoadedLaborRatePerHour;
                    break;
                case SavingsBasis.Scrap:
                    formula = "units started/day × (improved yield% − baseline yield%) / 100 × avoidable scrap cost/unit × production days/year";
                    kind = "Projected avoidable scrap cost";
                    amount = input.UnitsStartedPerDay * (input.ImprovedYieldPercent - input.BaselineYieldPercent) /
                        100m * input.AvoidableScrapCostPerUnit * input.ProductionDaysPerYear;
                    break;
                default:
                    formula = "min(additional saleable units/day, demand-backed additional units/day) × contribution margin/unit × production days/year";
                    kind = "Projected throughput contribution (not cost savings)";
                    amount = input.AdditionalSaleableUnitsPerDay.HasValue && input.DemandBackedAdditionalUnitsPerDay.HasValue
                        ? Math.Min(input.AdditionalSaleableUnitsPerDay.Value, input.DemandBackedAdditionalUnitsPerDay.Value) *
                            input.ContributionMarginPerUnit * input.ProductionDaysPerYear
                        : null;
                    break;
            }
            amount = amount.HasValue ? decimal.Round(amount.Value, 2, MidpointRounding.AwayFromZero) : null;
        }
        catch (OverflowException ex)
        {
            throw new ArgumentOutOfRangeException(nameof(input), $"Savings calculation exceeds supported decimal range: {ex.Message}");
        }
        return new(amount, amount.HasValue ? kind : "Unquantified", input.Currency, formula,
            amount.HasValue ? "Scenario only; do not sum overlapping benefits. Realized amounts are separately evidenced period totals."
                : "Required inputs for the selected formula are missing; no zero or default has been substituted.",
            input, input.Realized);
    }
}
