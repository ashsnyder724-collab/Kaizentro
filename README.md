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

## Build 002 Scope

Build 002 adds the first usable import-to-VSM workflow:

- Windows WPF desktop shell
- SAP-style CSV routing import
- Import validation notes
- Generated current-state VSM visual screen
- Domain model for plants, departments, work centers, routings, VSM, and Kaizen opportunities
- Application service for takt, capacity, lead time, bottleneck, and Kaizen-rule calculations
- SQL Server starter schema with import tracking tables
- Unit tests for VSM logic, import parsing, and diagram generation
- GitHub Actions build and downloadable Windows package workflow

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
  IMPORTS.md
  ROADMAP.md
  DOWNLOADS.md

installer/
  README.md

samples/
  sap-routing-export.csv
```

## Build Target

- Windows desktop application
- .NET 8 / WPF foundation
- SQL Server-ready data model
- Windows x64 self-contained downloadable artifact

## Latest Validated Package

- Workflow run: `31907597403`
- Artifact: `Kaizentro-Build002-win-x64`
- Artifact ID: `9252791843`
- Approximate size: 67.5 MB
- Result: restore, build, tests, publish, and upload all passed

## Downloading the Alpha Package

1. Open the **Actions** tab in GitHub.
2. Select **Kaizentro Build and Package**.
3. Open workflow run `31907597403`.
4. Download **Kaizentro-Build002-win-x64** from artifacts.
5. Extract the ZIP.
6. Run `Kaizentro.exe`.

## Importing sample data

Use `samples/sap-routing-export.csv` to test the Build 002 import workflow.

## Status

Current release state: **0.0.2-alpha / Build 002**

This is still an early alpha, not the finished commercial package. Build 003 should add persistent SQL save/load, project files, native Excel import, and a cleaner generated VSM layout.
