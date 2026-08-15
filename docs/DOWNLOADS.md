# Kaizentro Downloads

## Build 001 Alpha

Build 001 is packaged as a Windows x64 ZIP artifact produced by GitHub Actions.

### What it includes

- Kaizentro WPF desktop shell
- Sample executive dashboard
- Current-state value stream calculation service
- Basic Kaizen opportunity rules
- Takt time, lead time, value-added ratio, bottleneck, and opportunity output

### How to download

1. Open the repository in GitHub.
2. Go to the **Actions** tab.
3. Select **Kaizentro Build and Package**.
4. Open the latest successful run.
5. Download **Kaizentro-Build001-win-x64** from the run artifacts.
6. Extract the ZIP file.
7. Run `Kaizentro.exe`.

### Requirements

- Windows x64
- No separate .NET install required for the self-contained artifact

### Known limitations

- This is an alpha build, not a finished commercial release.
- The package is a ZIP artifact, not an MSI/MSIX installer yet.
- Data is sample/demo data only until Build 002 adds Excel import and database persistence.
- SAP integration, digital twin, and full AI future-state generation are planned later builds.

### Next packaging target

The packaging roadmap is:

1. Build 001: ZIP artifact from GitHub Actions.
2. Build 002: ZIP artifact with Excel import and data validation.
3. Build 003: Signed MSIX package.
4. Build 004: MSI installer with shortcuts, uninstall support, and update notes.
