using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Kaizentro.Application;
using Kaizentro.Application.Import;
using Kaizentro.Application.Visualization;
using Kaizentro.Domain;
using Microsoft.Win32;

namespace Kaizentro.Desktop;

public partial class MainWindow : Window
{
    private readonly ValueStreamAnalysisService _analysisService = new();
    private readonly ValueStreamDiagramService _diagramService = new();
    private readonly RoutingCsvImporter _importer = new();

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
        var map = _analysisService.BuildCurrentState(material, demand, workCenters, routing);
        LoadMap(map, "Sample 56-frame routing loaded. Import a CSV export to replace it.");
    }

    private void LoadCsvButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import SAP / Excel routing CSV",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var import = _importer.Import(File.ReadAllText(dialog.FileName));
            if (!import.HasUsableRouting)
            {
                ImportStatusText.Text = $"Import failed validation. {string.Join(" ", import.Issues.Take(3).Select(issue => $"Row {issue.RowNumber}: {issue.Message}"))}";
                return;
            }

            var map = _analysisService.BuildCurrentState(import.Material, import.Demand, import.WorkCenters, import.Operations);
            var issueText = import.HasIssues
                ? $" Loaded with {import.Issues.Count} validation note(s). First: {import.Issues[0].Message}"
                : " Loaded with no validation issues.";

            LoadMap(map, $"Imported {System.IO.Path.GetFileName(dialog.FileName)}.{issueText}");
        }
        catch (Exception ex)
        {
            ImportStatusText.Text = $"Import failed: {ex.Message}";
        }
    }

    private void LoadMap(ValueStreamMap map, string status)
    {
        TaktText.Text = $"{map.TaktTimeSeconds:N0} sec";
        LeadTimeText.Text = $"{map.TotalLeadTimeMinutes:N1} min";
        VaRatioText.Text = $"{map.ValueAddedRatioPercent:N1}%";
        KaizenCountText.Text = map.Opportunities.Count.ToString();
        ImportStatusText.Text = status;

        var diagram = _diagramService.BuildDiagram(map);
        VsmSummaryText.Text = diagram.ExecutiveSummary;

        ProcessList.ItemsSource = map.Processes.Select(process =>
            $"{process.Sequence} | {process.WorkCenterCode} | {process.OperationName} | CT {process.CycleTimeSeconds:N0}s | Setup {process.SetupTimeMinutes:N0}m | Queue {process.QueueTimeMinutes:N0}m | Bottleneck: {(process.IsBottleneck ? "YES" : "No")}");

        OpportunityList.ItemsSource = map.Opportunities.Select(opportunity =>
            $"[{opportunity.PriorityScore}] {opportunity.Area} - {opportunity.WasteType}: {opportunity.Recommendation} Est. ${opportunity.EstimatedAnnualSavings:N0}/yr");

        DrawValueStream(diagram);
    }

    private void DrawValueStream(ValueStreamDiagram diagram)
    {
        ValueStreamCanvas.Children.Clear();

        const double left = 24;
        const double top = 48;
        const double boxWidth = 170;
        const double boxHeight = 94;
        const double gap = 78;
        var totalWidth = Math.Max(1000, left * 2 + diagram.Nodes.Count * boxWidth + Math.Max(0, diagram.Nodes.Count - 1) * gap);
        ValueStreamCanvas.Width = totalWidth;

        for (var i = 0; i < diagram.Nodes.Count; i++)
        {
            var node = diagram.Nodes[i];
            var x = left + i * (boxWidth + gap);

            var border = new Border
            {
                Width = boxWidth,
                Height = boxHeight,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                Background = node.IsBottleneck ? new SolidColorBrush(Color.FromRgb(255, 235, 235)) : Brushes.White,
                BorderBrush = node.IsBottleneck ? Brushes.Firebrick : new SolidColorBrush(Color.FromRgb(11, 61, 145)),
                BorderThickness = new Thickness(2)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = $"{node.Sequence} | {node.WorkCenterCode}",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(20, 45, 75)),
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock { Text = node.OperationName, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) });
            stack.Children.Add(new TextBlock { Text = $"CT {node.CycleTimeSeconds:N0}s | Setup {node.SetupTimeMinutes:N0}m", FontSize = 12, Foreground = Brushes.DimGray, Margin = new Thickness(0, 4, 0, 0) });
            stack.Children.Add(new TextBlock { Text = node.IsBottleneck ? "BOTTLENECK" : "Flow OK", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = node.IsBottleneck ? Brushes.Firebrick : Brushes.SeaGreen });
            border.Child = stack;

            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, top);
            ValueStreamCanvas.Children.Add(border);

            if (i < diagram.Nodes.Count - 1)
            {
                DrawArrow(x + boxWidth + 8, top + boxHeight / 2, x + boxWidth + gap - 8, top + boxHeight / 2);
                DrawInventoryTriangle(x + boxWidth + gap / 2 - 18, top + boxHeight + 24, node.QueueTimeMinutes);
            }
        }
    }

    private void DrawArrow(double x1, double y1, double x2, double y2)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = new SolidColorBrush(Color.FromRgb(37, 99, 151)),
            StrokeThickness = 2
        };
        ValueStreamCanvas.Children.Add(line);

        var arrowHead = new Polygon
        {
            Fill = new SolidColorBrush(Color.FromRgb(37, 99, 151)),
            Points = new PointCollection
            {
                new Point(x2, y2),
                new Point(x2 - 10, y2 - 5),
                new Point(x2 - 10, y2 + 5)
            }
        };
        ValueStreamCanvas.Children.Add(arrowHead);
    }

    private void DrawInventoryTriangle(double x, double y, decimal queueMinutes)
    {
        var triangle = new Polygon
        {
            Stroke = Brushes.DarkOrange,
            Fill = new SolidColorBrush(Color.FromRgb(255, 248, 232)),
            StrokeThickness = 2,
            Points = new PointCollection
            {
                new Point(x + 18, y),
                new Point(x, y + 34),
                new Point(x + 36, y + 34)
            }
        };
        ValueStreamCanvas.Children.Add(triangle);

        var label = new TextBlock
        {
            Text = $"Queue {queueMinutes:N0}m",
            FontSize = 11,
            Foreground = Brushes.DimGray
        };
        Canvas.SetLeft(label, x - 8);
        Canvas.SetTop(label, y + 38);
        ValueStreamCanvas.Children.Add(label);
    }
}
