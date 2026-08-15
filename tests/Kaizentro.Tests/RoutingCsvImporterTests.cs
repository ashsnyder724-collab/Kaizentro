using Kaizentro.Application;
using Kaizentro.Application.Import;
using Kaizentro.Application.Visualization;
using Xunit;

namespace Kaizentro.Tests;

public sealed class RoutingCsvImporterTests
{
    [Fact]
    public void Import_LoadsSapStyleRoutingExport()
    {
        const string csv = "PartNumber,Description,ProductFamily,DailyDemand,AvailableMinutesPerDay,WorkCenterCode,WorkCenterName,Sequence,OperationName,CycleTimeSeconds,SetupTimeMinutes,QueueTimeMinutes,MoveTimeMinutes,Operators,YieldPercent\n" +
                           "MTR-56,56 Frame Motor,Motors,60,430,WC-100,Shell Press,10,Shell Press,68,22,120,10,1,99\n" +
                           "MTR-56,56 Frame Motor,Motors,60,430,WC-200,Winding,20,Winding,420,18,240,20,2,97.5\n";

        var result = new RoutingCsvImporter().Import(csv);

        Assert.False(result.HasIssues);
        Assert.Equal("MTR-56", result.Material.PartNumber);
        Assert.Equal(60, result.Demand.DailyDemand);
        Assert.Equal(2, result.Operations.Count);
        Assert.Equal(2, result.WorkCenters.Count);
    }

    [Fact]
    public void Import_ThenAnalyze_GeneratesDiagramNodes()
    {
        const string csv = "Material,MaterialDescription,ProductFamily,Demand,AvailableMinutes,WorkCenter,WorkCenterDescription,Operation,OperationDescription,CycleTime,ChangeoverMinutes,Queue,MoveTime,Operators,Yield\n" +
                           "MTR-56,56 Frame Motor,Motors,60,430,WC-100,Shell Press,10,Shell Press,68,22,120,10,1,99\n" +
                           "MTR-56,56 Frame Motor,Motors,60,430,WC-300,Paint,30,Paint,95,64,480,40,1,98.5\n";

        var import = new RoutingCsvImporter().Import(csv);
        var map = new ValueStreamAnalysisService().BuildCurrentState(
            import.Material,
            import.Demand,
            import.WorkCenters,
            import.Operations);

        var diagram = new ValueStreamDiagramService().BuildDiagram(map);

        Assert.Equal(2, diagram.Nodes.Count);
        Assert.Single(diagram.Edges);
        Assert.Contains(map.Opportunities, item => item.WasteType.Contains("Changeover"));
    }
}
