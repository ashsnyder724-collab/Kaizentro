# Kaizentro

**Kaizentro** is a Lean Manufacturing Intelligence Platform designed to turn ERP, routing, capacity, and production data into actionable continuous improvement insight.

This repository is the foundation for the Kaizentro Enterprise software package.

## Product Direction

Kaizentro is being built as a Windows-first manufacturing intelligence platform for:

- Automatic Value Stream Mapping
- SAP/ERP routing and capacity analysis
- Bottleneck detection
- Takt, cycle time, lead time, and capacity calculations
- Kaizen opportunity identification
- Future-state VSM generation
- Digital twin scenario analysis
- Manufacturing executive dashboards

## Build 001 Scope

This first foundation commit includes:

- .NET solution structure
- Windows WPF desktop shell
- Domain model for plants, departments, work centers, routings, VSM, and Kaizen opportunities
- Application service for takt, capacity, lead time, bottleneck, and Kaizen-rule calculations
- SQL Server starter schema
- Unit test project
- GitHub Actions build workflow
- Architecture and roadmap documentation

## Repository Structure

```text
src/
  Kaizentro.Desktop/
  Kaizentro.Application/
  Kaizentro.Domain/
  Kaizentro.Infrastructure/

tests/
  Kaizentro.Tests/

database/
  schema.sql

docs/
  ARCHITECTURE.md
  ROADMAP.md

installer/
  README.md
```

## Build Target

- Windows desktop application
- .NET 8 / WPF foundation
- SQL Server-ready data model

## Status

Current release state: **0.0.1-alpha / Build 001**

This is the starting scaffold, not the finished commercial package. Next build should add Excel import, persistent database wiring, and the first generated current-state VSM screen.
