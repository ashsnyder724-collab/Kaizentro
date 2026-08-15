using Kaizentro.Domain;

namespace Kaizentro.Application.Visualization;

public sealed record ValueStreamDiagramNode(
    int Sequence,
    string WorkCenterCode,
    string OperationName,
    decimal CycleTimeSeconds,
    decimal SetupTimeMinutes,
    decimal QueueTimeMinutes,
    bool IsBottleneck);

public sealed record ValueStreamDiagramEdge(int FromSequence, int ToSequence, string Label);

public sealed record ValueStreamDiagram(
    string PartNumber,
    decimal TaktTimeSeconds,
    decimal LeadTimeMinutes,
    decimal ValueAddedRatioPercent,
    IReadOnlyList<ValueStreamDiagramNode> Nodes,
    IReadOnlyList<ValueStreamDiagramEdge> Edges)
{
    public string ExecutiveSummary =>
        $"{PartNumber}: takt {TaktTimeSeconds:N1}s, lead time {LeadTimeMinutes:N1} min, VA ratio {ValueAddedRatioPercent:N1}%.";
}

public sealed class ValueStreamDiagramService
{
    public ValueStreamDiagram BuildDiagram(ValueStreamMap map)
    {
        var nodes = map.Processes
            .OrderBy(process => process.Sequence)
            .Select(process => new ValueStreamDiagramNode(
                process.Sequence,
                process.WorkCenterCode,
                process.OperationName,
                process.CycleTimeSeconds,
                process.SetupTimeMinutes,
                process.QueueTimeMinutes,
                process.IsBottleneck))
            .ToList();

        var edges = nodes
            .Zip(nodes.Skip(1), (from, to) => new ValueStreamDiagramEdge(
                from.Sequence,
                to.Sequence,
                $"FIFO / move to {to.WorkCenterCode}"))
            .ToList();

        return new ValueStreamDiagram(
            map.PartNumber,
            map.TaktTimeSeconds,
            map.TotalLeadTimeMinutes,
            map.ValueAddedRatioPercent,
            nodes,
            edges);
    }
}
