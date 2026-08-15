using System.Globalization;
using Kaizentro.Domain;

namespace Kaizentro.Application.Import;

public sealed class RoutingCsvImporter
{
    public RoutingImportResult Import(string csvContent)
    {
        var issues = new List<RoutingImportIssue>();
        var materialId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var workCenters = new Dictionary<string, WorkCenter>(StringComparer.OrdinalIgnoreCase);
        var operations = new List<RoutingOperation>();

        if (string.IsNullOrWhiteSpace(csvContent))
        {
            issues.Add(new RoutingImportIssue(0, "File", "Import file is empty."));
            return EmptyResult(materialId, issues);
        }

        var lines = csvContent
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (lines.Count < 2)
        {
            issues.Add(new RoutingImportIssue(0, "File", "Import file must include a header and at least one data row."));
            return EmptyResult(materialId, issues);
        }

        var headers = BuildHeaderIndex(ParseCsvLine(lines[0]));
        var partNumber = string.Empty;
        var description = string.Empty;
        var productFamily = string.Empty;
        var dailyDemand = 60;
        var availableMinutesPerDay = 430m;

        for (var lineIndex = 1; lineIndex < lines.Count; lineIndex++)
        {
            var rowNumber = lineIndex + 1;
            var row = ParseCsvLine(lines[lineIndex]);
            if (row.Count == 0 || row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var rowPartNumber = Cell(row, headers, "partnumber", "material", "materialnumber", "sapmaterial");
            if (string.IsNullOrWhiteSpace(rowPartNumber))
            {
                issues.Add(new RoutingImportIssue(rowNumber, "PartNumber", "Part number / material is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(partNumber))
            {
                partNumber = rowPartNumber.Trim();
                description = Cell(row, headers, "description", "materialdescription", "partdescription").Trim();
                productFamily = Cell(row, headers, "productfamily", "family", "productline").Trim();
                dailyDemand = ReadInt(row, headers, rowNumber, issues, dailyDemand, false, "dailydemand", "demand", "qtyperday", "quantityperday");
                availableMinutesPerDay = ReadDecimal(row, headers, rowNumber, issues, availableMinutesPerDay, false, "availableminutesperday", "availableminutes", "netavailableminutes");
            }
            else if (!string.Equals(partNumber, rowPartNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new RoutingImportIssue(rowNumber, "PartNumber", "Build 002 supports one material per import. Split mixed-material exports before import."));
                continue;
            }

            var workCenterCode = Cell(row, headers, "workcentercode", "workcenter", "wc", "sapworkcenter").Trim();
            if (string.IsNullOrWhiteSpace(workCenterCode))
            {
                issues.Add(new RoutingImportIssue(rowNumber, "WorkCenterCode", "Work center code is required."));
                continue;
            }

            var sequence = ReadInt(row, headers, rowNumber, issues, 0, true, "sequence", "operation", "operationsequence", "op");
            var cycleTimeSeconds = ReadDecimal(row, headers, rowNumber, issues, 0m, true, "cycletimeseconds", "cycletime", "ctseconds", "standardtimeseconds");
            if (sequence <= 0 || cycleTimeSeconds <= 0m)
            {
                continue;
            }

            if (!workCenters.TryGetValue(workCenterCode, out var workCenter))
            {
                var workCenterName = Cell(row, headers, "workcentername", "workcenterdescription").Trim();
                workCenter = new WorkCenter(
                    Guid.NewGuid(),
                    departmentId,
                    workCenterCode,
                    string.IsNullOrWhiteSpace(workCenterName) ? workCenterCode : workCenterName,
                    availableMinutesPerDay,
                    85m);
                workCenters.Add(workCenterCode, workCenter);
            }

            var operationName = Cell(row, headers, "operationname", "operationdescription", "activity", "workstep").Trim();
            operations.Add(new RoutingOperation(
                sequence,
                materialId,
                workCenter.Id,
                string.IsNullOrWhiteSpace(operationName) ? $"Operation {sequence}" : operationName,
                cycleTimeSeconds,
                ReadDecimal(row, headers, rowNumber, issues, 0m, false, "setuptimeminutes", "setuptime", "changeovertime", "changeoverminutes"),
                ReadDecimal(row, headers, rowNumber, issues, 0m, false, "queuetimeminutes", "queue", "waittime", "waitingtime"),
                ReadDecimal(row, headers, rowNumber, issues, 0m, false, "movetimeminutes", "movetime", "transporttime"),
                Math.Max(1, ReadInt(row, headers, rowNumber, issues, 1, false, "operators", "operatorcount", "labor")),
                ReadDecimal(row, headers, rowNumber, issues, 100m, false, "yieldpercent", "yield", "fpypct")));
        }

        if (operations.Count == 0)
        {
            issues.Add(new RoutingImportIssue(0, "Routing", "No valid routing operations were imported."));
        }

        var material = new Material(
            materialId,
            string.IsNullOrWhiteSpace(partNumber) ? "UNKNOWN" : partNumber,
            string.IsNullOrWhiteSpace(description) ? "Imported material" : description,
            string.IsNullOrWhiteSpace(productFamily) ? "Imported" : productFamily);

        var demand = new ProductionDemand(materialId, Math.Max(1, dailyDemand), Math.Max(1m, availableMinutesPerDay));
        return new RoutingImportResult(material, demand, workCenters.Values.ToDictionary(item => item.Id), operations, issues);
    }

    private static RoutingImportResult EmptyResult(Guid materialId, IReadOnlyList<RoutingImportIssue> issues)
    {
        var material = new Material(materialId, "UNKNOWN", "No import data", "Unknown");
        var demand = new ProductionDemand(materialId, 1, 1m);
        return new RoutingImportResult(material, demand, new Dictionary<Guid, WorkCenter>(), Array.Empty<RoutingOperation>(), issues);
    }

    private static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headers)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            var normalized = Normalize(headers[i]);
            if (!string.IsNullOrWhiteSpace(normalized) && !index.ContainsKey(normalized))
            {
                index.Add(normalized, i);
            }
        }

        return index;
    }

    private static string Cell(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headers, params string[] aliases)
    {
        foreach (var alias in aliases.Select(Normalize))
        {
            if (headers.TryGetValue(alias, out var index) && index < row.Count)
            {
                return row[index];
            }
        }

        return string.Empty;
    }

    private static int ReadInt(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headers, int rowNumber, ICollection<RoutingImportIssue> issues, int defaultValue, bool required, params string[] aliases)
    {
        var value = Cell(row, headers, aliases);
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                issues.Add(new RoutingImportIssue(rowNumber, aliases[0], "Required whole-number field is missing."));
            }

            return defaultValue;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        issues.Add(new RoutingImportIssue(rowNumber, aliases[0], $"Value '{value}' is not a valid whole number."));
        return defaultValue;
    }

    private static decimal ReadDecimal(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headers, int rowNumber, ICollection<RoutingImportIssue> issues, decimal defaultValue, bool required, params string[] aliases)
    {
        var value = Cell(row, headers, aliases);
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                issues.Add(new RoutingImportIssue(rowNumber, aliases[0], "Required numeric field is missing."));
            }

            return defaultValue;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        issues.Add(new RoutingImportIssue(rowNumber, aliases[0], $"Value '{value}' is not a valid number."));
        return defaultValue;
    }

    private static string Normalize(string value)
    {
        var characters = value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant);
        return new string(characters.ToArray());
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        values.Add(current.ToString().Trim());
        return values;
    }
}
