namespace Kaizentro.Domain;

public enum SavingsBasis { LaborCapacity, Scrap, ThroughputContribution }

// One non-additive scenario per material/operation/waste. No financial defaults.
public sealed record SavingsInput
{
    public Guid MaterialId { get; init; }
    public int OperationSequence { get; init; }
    public string WasteType { get; init; } = "";
    public string Currency { get; init; } = "";
    public string EvidenceReference { get; init; } = "";
    public SavingsBasis Basis { get; init; }
    public decimal? ProductionDaysPerYear { get; init; }
    public decimal? EventsPerDay { get; init; }
    public decimal? MeasuredSecondsReducedPerEvent { get; init; }
    public int? AffectedOperators { get; init; }
    public decimal? LoadedLaborRatePerHour { get; init; }
    public decimal? UnitsStartedPerDay { get; init; }
    public decimal? BaselineYieldPercent { get; init; }
    public decimal? ImprovedYieldPercent { get; init; }
    public decimal? AvoidableScrapCostPerUnit { get; init; }
    public decimal? AdditionalSaleableUnitsPerDay { get; init; }
    public decimal? DemandBackedAdditionalUnitsPerDay { get; init; }
    public decimal? ContributionMarginPerUnit { get; init; }
    public RealizedSavingsEvidence? Realized { get; init; }
}

// Actual reconciled benefit for a closed observation period; never annualized.
public sealed record RealizedSavingsEvidence(
    decimal Amount, DateOnly PeriodStart, DateOnly PeriodEnd,
    string VerifiedBy, string EvidenceReference);

public sealed record SavingsAssessment(
    decimal? ProjectedAnnualAmount, string ProjectionKind, string Currency,
    string Formula, string Explanation, SavingsInput? Assumptions,
    RealizedSavingsEvidence? VerifiedRealized);
