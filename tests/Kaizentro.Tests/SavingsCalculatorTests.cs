using Kaizentro.Application;
using Kaizentro.Domain;
using Xunit;

namespace Kaizentro.Tests;

public sealed class SavingsCalculatorTests
{
    private static SavingsInput Labor() => new()
    {
        MaterialId = Guid.NewGuid(), OperationSequence = 10, WasteType = "Changeover",
        Currency = "USD", EvidenceReference = "Time study TS-17; payroll LR-4; schedule FY27",
        Basis = SavingsBasis.LaborCapacity, ProductionDaysPerYear = 200,
        EventsPerDay = 2, MeasuredSecondsReducedPerEvent = 900,
        AffectedOperators = 2, LoadedLaborRatePerHour = 40
    };

    [Fact]
    public void Labor_Uses3600SecondsPerHourAndActualEventFrequency()
    {
        var result = SavingsCalculator.Calculate(Labor());
        Assert.Equal(8000m, result.ProjectedAnnualAmount);
        Assert.Contains("not cash savings", result.ProjectionKind);
        Assert.Null(result.VerifiedRealized);
        Assert.Equal(Labor().EvidenceReference, result.Assumptions!.EvidenceReference);
    }

    [Fact]
    public void MissingInputsAreNotZero()
    {
        Assert.Null(SavingsCalculator.Calculate(null).ProjectedAnnualAmount);
        Assert.Null(SavingsCalculator.Calculate(Labor() with { EventsPerDay = null }).ProjectedAnnualAmount);
        Assert.Equal(0m, SavingsCalculator.Calculate(Labor() with { EventsPerDay = 0 }).ProjectedAnnualAmount);
    }

    [Fact]
    public void Scrap_UsesPercentagePointsAndAvoidableUnitCost()
    {
        var result = SavingsCalculator.Calculate(Labor() with { Basis = SavingsBasis.Scrap,
            UnitsStartedPerDay = 1000, BaselineYieldPercent = 95,
            ImprovedYieldPercent = 98, AvoidableScrapCostPerUnit = 12.5m });
        Assert.Equal(75000m, result.ProjectedAnnualAmount);
    }

    [Fact]
    public void Throughput_IsCappedByDemandAndUsesMargin()
    {
        var result = SavingsCalculator.Calculate(Labor() with { Basis = SavingsBasis.ThroughputContribution,
            AdditionalSaleableUnitsPerDay = 20, DemandBackedAdditionalUnitsPerDay = 8,
            ContributionMarginPerUnit = 15 });
        Assert.Equal(24000m, result.ProjectedAnnualAmount);
    }

    [Fact]
    public void Realized_IsSeparatePeriodTotalEvenWithoutProjectionInputs()
    {
        var evidence = new RealizedSavingsEvidence(1234.56m, new(2026, 1, 1), new(2026, 1, 31), "Controller", "GL-123");
        var result = SavingsCalculator.Calculate(Labor() with { EventsPerDay = null, Realized = evidence });
        Assert.Null(result.ProjectedAnnualAmount);
        Assert.Equal(evidence, result.VerifiedRealized);
    }

    [Fact]
    public void InvalidInputsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SavingsCalculator.Calculate(Labor() with { ProductionDaysPerYear = 367 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => SavingsCalculator.Calculate(Labor() with { MeasuredSecondsReducedPerEvent = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => SavingsCalculator.Calculate(Labor() with { AffectedOperators = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => SavingsCalculator.Calculate(Labor() with { BaselineYieldPercent = 101 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => SavingsCalculator.Calculate(Labor() with { BaselineYieldPercent = 98, ImprovedYieldPercent = 95 }));
        Assert.Throws<ArgumentException>(() => SavingsCalculator.Calculate(Labor() with { EvidenceReference = "" }));
        Assert.Throws<ArgumentException>(() => SavingsCalculator.Calculate(Labor() with { Currency = "" }));
        Assert.Throws<ArgumentException>(() => SavingsCalculator.Calculate(Labor() with { Realized = new(100, new(2026, 1, 1), new(2026, 1, 31), "", "") }));
        var input = Labor();
        Assert.Throws<ArgumentException>(() => SavingsCalculator.ValidateInputs(new[] { input, input }));
        Assert.Throws<ArgumentOutOfRangeException>(() => SavingsCalculator.Calculate(Labor() with { EventsPerDay = decimal.MaxValue }));
    }
}
