using System.Windows;
using Kaizentro.Application;
using Kaizentro.Domain;

namespace Kaizentro.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadSampleDashboard();
    }

    private void LoadSampleDashboard()
    {
        var departmentId = Guid.NewGuid();
        var material = new Material(Guid.NewGuid(), "DEMO-56-FRAME", "Demo 56 Frame Motor", "Motors");
        var workCenters = new[]
        {
            new WorkCenter(Guid.NewGuid(), departmentId, "WC-100", "Shell Press", 430m, 85m),
            new WorkCenter(Guid.NewGuid(), departmentId, "WC-200", "Winding", 430m, 82m),
            new WorkCenter(Guid.NewGuid(), departmentId, "WC-300", "Paint", 430m, 78m)
        }.ToDictionary(item => item.Id);

        var routing = new List<RoutingOperation>
        {
            new(10, material.Id, workCenters.Values.ElementAt(0).Id, "Shell Press", 68m, 22m, 120m, 10m, 1, 99m),
            new(20, material.Id, workCenters.Values.ElementAt(1).Id, "Winding", 420m, 18m, 240m, 20m, 2, 97.5m),
            new(30, material.Id, workCenters.Values.ElementAt(2).Id, "Paint", 95m, 64m, 480m, 40m, 1, 98.5m)
        };

        var demand = new ProductionDemand(material.Id, 60, 430m);
        var map = new ValueStreamAnalysisService().BuildCurrentState(material, demand, workCenters, routing);

        TaktText.Text = $"{map.TaktTimeSeconds:N0} sec";
        LeadTimeText.Text = $"{map.TotalLeadTimeMinutes:N1} min";
        VaRatioText.Text = $"{map.ValueAddedRatioPercent:N1}%";
        KaizenCountText.Text = map.Opportunities.Count.ToString();

        ProcessList.ItemsSource = map.Processes.Select(process =>
            $"{process.Sequence} | {process.WorkCenterCode} | {process.OperationName} | CT {process.CycleTimeSeconds:N0}s | Setup {process.SetupTimeMinutes:N0}m | Bottleneck: {(process.IsBottleneck ? "YES" : "No")}");

        OpportunityList.ItemsSource = map.Opportunities.Select(opportunity =>
            $"[{opportunity.PriorityScore}] {opportunity.Area} - {opportunity.WasteType}: {opportunity.Recommendation} Est. ${opportunity.EstimatedAnnualSavings:N0}/yr");
    }
}
