# Kaizentro Architecture

Kaizentro is organized as a modular manufacturing intelligence platform.

## Layers

1. **Desktop** - Windows user interface for engineers, supervisors, and plant leadership.
2. **Application** - Use cases, calculations, orchestration, and service-level rules.
3. **Domain** - Manufacturing concepts such as plant, work center, routing, value stream, and Kaizen opportunity.
4. **Infrastructure** - SQL Server persistence and future ERP integration adapters.
5. **Tests** - Regression coverage for Lean calculations and rules.

## Core Flow

```text
ERP / Excel Data
    -> Routing + Capacity Model
    -> Takt / Lead Time / Capacity Calculations
    -> Current State Value Stream
    -> Kaizen Opportunity Ranking
    -> Dashboard + Reports
```

## Rule Engine Direction

Build 001 uses deterministic Lean rules so recommendations are explainable and auditable.

Examples:

- Cycle time above takt = constraint risk
- Setup time above 30 minutes = SMED opportunity
- Queue time high versus cycle time = inventory / waiting opportunity
- Yield below 98% = defect / quality opportunity

Machine learning and LLM-based advisory features can be added later, but the platform should keep the calculation backbone deterministic.
