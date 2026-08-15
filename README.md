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

This first foundation includes:

- .NET solution structure
- Windows WPF desktop shell
- Domain model for plants, departments, work centers, routings, VSM, and Kaizen opportunities
- Application service for takt, capacity, lead time, bottleneck, and Kaizen-rule calculations
- SQL Server starter schema
- Unit test project
- GitHub Actions build and packaging workflow
- Architecture, roadmap, and installer documentation

## Downloadable Package

Build 001 produces a downloadable Windows package from GitHub Actions.

1. Open the **Actions** tab in GitHub.
2. Select **Kaizentro Build and Package**.
3. Open the latest successful workflow run.
4. Download the artifact named **Kaizentro-Build001-win-x64**.
5. Extract the ZIP file on a Windows x64 machine.
6. Run `Kaizentro.exe`.

This is an early alpha package. It is not an MSI installer yet. The MSI/MSIX installer workstream is planned after the Excel import, database wiring, and generated VSM screen are stable.

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
  DOWNLOADS.md

installer/
  README.md
```

## Build Target

- Windows desktop application
- .NET 8 / WPF foundation
- SQL Server-ready data model
- Self-contained Windows x64 ZIP artifact from GitHub Actions

## Status

Current release state: **0.0.1-alpha / Build 001**

This is the starting scaffold, not the finished commercial package. Next build should add Excel import, persistent database wiring, and the first generated current-state VSM screen.
