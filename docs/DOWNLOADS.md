# Kaizentro Downloads

## Build 002 Alpha

Build 002 is packaged as a Windows x64 ZIP artifact produced by GitHub Actions.

### Latest validated artifact

- Workflow run: `31907597403`
- Artifact: `Kaizentro-Build002-win-x64`
- Artifact ID: `9252791843`
- Approximate size: 67.5 MB
- SHA-256 digest: `04a8c6fa7c0cc88de5a5f8b50f739b8d939e4f7d2f0c1ba83c127f9447d36191`
- Expires: 2026-09-14 unless promoted to a formal GitHub Release asset

### What it includes

- Kaizentro WPF desktop shell
- SAP-style CSV routing import
- Import validation notes
- Generated current-state value stream visual
- Current-state value stream calculation service
- Basic Kaizen opportunity rules
- Takt time, lead time, value-added ratio, bottleneck, and opportunity output
- SQL Server starter schema with import tracking tables

### How to download

1. Open the repository in GitHub.
2. Go to the **Actions** tab.
3. Select **Kaizentro Build and Package**.
4. Open the successful run for workflow run `31907597403`.
5. Download **Kaizentro-Build002-win-x64** from the run artifacts.
6. Extract the ZIP file.
7. Run `Kaizentro.exe`.

### How to test import

Use the sample file:

```text
samples/sap-routing-export.csv
```

Inside the app, click **Import SAP / Excel CSV Routing** and select the sample CSV file.

### Requirements

- Windows x64
- No separate .NET install required for the self-contained artifact

### Known limitations

- This is an alpha build, not a finished commercial release.
- The package is a ZIP artifact, not an MSI/MSIX installer yet.
- Build 002 analyzes imported data in memory.
- SQL persistence wiring, native `.xlsx` import, SAP connector work, digital twin, and full AI future-state generation are planned later builds.

### Next packaging target

The packaging roadmap is:

1. Build 001: ZIP artifact from GitHub Actions.
2. Build 002: ZIP artifact with CSV import, validation, and generated VSM visual.
3. Build 003: SQL persistence and project save/load.
4. Build 004: Signed MSIX package.
5. Build 005: MSI installer with shortcuts, uninstall support, and update notes.
