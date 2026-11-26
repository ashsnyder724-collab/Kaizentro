# Application SBOM requirement

Every application added to this repository must generate a CycloneDX software bill of materials (SBOM).

The Software Bill of Materials workflow inventories detectable source dependencies on main pushes, pull requests, and manual runs. Download source-sbom-<commit> under the run's Artifacts. Artifacts are retained for 90 days, subject to repository limits.

This repository currently contains only a license and these SBOM controls. An empty inventory is expected until application manifests and code are added; it is not evidence that a released application has no dependencies.

When adding application builds, also generate an SBOM from the exact resolved runtime dependencies for each platform. Include direct and transitive component versions, dependency relationships, and available license metadata. Attach that SBOM beside each APK, installer, ZIP, or container, and include its SHA-256 in release checksums. Fail dependency-bearing application builds if SBOM generation fails or yields an empty inventory. Keep vulnerability scanning as a separate check.

Copy .github/workflows/sbom.yml into each new application repository as a starting point; source detection does not replace a platform-specific release inventory.
