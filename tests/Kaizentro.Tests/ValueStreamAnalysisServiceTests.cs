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

        Assert.True(map.Processes.Single().IsBottleneck);
        Assert.Contains(map.Opportunities, opportunity => opportunity.WasteType.Contains("Constraint"));
        Assert.Contains(map.Opportunities, opportunity => opportunity.WasteType.Contains("Changeover"));
    }
}
