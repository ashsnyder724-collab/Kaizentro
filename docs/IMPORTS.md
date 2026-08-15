# Kaizentro Import Format

Build 002 supports a SAP-style CSV routing export. This is the first import path toward SAP/ERP integration.

## Supported file type

- `.csv`
- One material / part number per import file
- One row per routing operation

## Required columns

Kaizentro accepts several common SAP/Excel header names. Headers are normalized, so spaces and underscores do not matter.

| Required data | Preferred header | Also accepted |
| --- | --- | --- |
| Part number / material | `PartNumber` | `Material`, `MaterialNumber`, `SAPMaterial` |
| Work center | `WorkCenterCode` | `WorkCenter`, `WC`, `SAPWorkCenter` |
| Operation sequence | `Sequence` | `Operation`, `OperationSequence`, `Op` |
| Cycle time seconds | `CycleTimeSeconds` | `CycleTime`, `CTSeconds`, `StandardTimeSeconds` |

## Optional columns

| Data | Preferred header | Also accepted |
| --- | --- | --- |
| Description | `Description` | `MaterialDescription`, `PartDescription` |
| Product family | `ProductFamily` | `Family`, `ProductLine` |
| Daily demand | `DailyDemand` | `Demand`, `QtyPerDay`, `QuantityPerDay` |
| Available minutes per day | `AvailableMinutesPerDay` | `AvailableMinutes`, `NetAvailableMinutes` |
| Work center name | `WorkCenterName` | `WorkCenterDescription` |
| Operation name | `OperationName` | `OperationDescription`, `Activity`, `WorkStep` |
| Setup / changeover | `SetupTimeMinutes` | `SetupTime`, `ChangeoverTime`, `ChangeoverMinutes` |
| Queue time | `QueueTimeMinutes` | `Queue`, `WaitTime`, `WaitingTime` |
| Move time | `MoveTimeMinutes` | `MoveTime`, `TransportTime` |
| Operators | `Operators` | `OperatorCount`, `Labor` |
| Yield | `YieldPercent` | `Yield`, `FPYPct` |

## Build 002 behavior

When a file is imported, Kaizentro:

1. Validates the file structure.
2. Builds material, work center, demand, and routing objects in memory.
3. Calculates takt time, lead time, value-added ratio, bottlenecks, and Kaizen opportunities.
4. Generates a current-state value stream visual automatically.

## Current limitations

- One material per import file.
- Data is analyzed in memory in Build 002.
- SQL schema now includes import tracking tables, but full persistent save/load wiring is planned for Build 003.
- Native `.xlsx` parsing is planned after the CSV workflow stabilizes.
