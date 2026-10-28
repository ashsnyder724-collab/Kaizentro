using Kaizentro.Application;
using Kaizentro.Domain;
using Xunit;

namespace Kaizentro.Tests;

public sealed class ValueStreamAnalysisServiceTests
{
    [Fact]
    public void BuildCurrentState_FlagsOperationAboveTaktAsOpportunity()
    {
        var departmentId = Guid.NewGuid();
        var material = new Material(Guid.NewGuid(), "TEST-001", "Test Part", "Test Family");
        var workCenter = new WorkCenter(Guid.NewGuid(), departmentId, "WC-1", "Constraint Cell", 430m, 85m);
        var routing = new[]
        {
            new RoutingOperation(10, material.Id, workCenter.Id, "Constraint Operation", 500m, 45m, 120m, 5m, 1, 97m)
        };
        var demand = new ProductionDemand(material.Id, 60, 430m);

        var map = new ValueStreamAnalysisService().BuildCurrentState(
            material,
            demand,
            new Dictionary<Guid, WorkCenter> { [workCenter.Id] = workCenter },
            routing);

        Assert.All(map.Opportunities, opportunity => {
            Assert.Null(opportunity.EstimatedAnnualSavings);
            Assert.Equal("Unquantified", opportunity.Savings!.ProjectionKind);
            Assert.Null(opportunity.Savings.VerifiedRealized);
        });
        var financialInput = new SavingsInput {
            MaterialId = material.Id, OperationSequence = 10, WasteType = "Changeover",
            Currency = "USD", EvidenceReference = "TS-1", Basis = SavingsBasis.LaborCapacity,
            ProductionDaysPerYear = 200, EventsPerDay = 2, MeasuredSecondsReducedPerEvent = 900,
            AffectedOperators = 2, LoadedLaborRatePerHour = 40
        };
        var service = new ValueStreamAnalysisService();
        var centers = new Dictionary<Guid, WorkCenter> { [workCenter.Id] = workCenter };
        var quantified = service.BuildCurrentState(material, demand, centers, routing, new[] { financialInput });
        Assert.Equal(8000m, quantified.Opportunities.Single(item => item.WasteType == "Changeover").EstimatedAnnualSavings);
        Assert.All(quantified.Opportunities.Where(item => item.WasteType != "Changeover"), item => Assert.Null(item.EstimatedAnnualSavings));
        Assert.Throws<ArgumentException>(() => service.BuildCurrentState(material, demand, centers, routing,
            new[] { financialInput with { OperationSequence = 999 } }));
        Assert.Throws<ArgumentException>(() => service.BuildCurrentState(material, demand, centers, routing,
            new[] { financialInput with { MaterialId = Guid.NewGuid() } }));
        Assert.True(map.Processes.Single().IsBottleneck);
        Assert.Contains(map.Opportunities, opportunity => opportunity.WasteType.Contains("Constraint"));
        Assert.Contains(map.Opportunities, opportunity => opportunity.WasteType.Contains("Changeover"));
    }
}

