# Phase LIV - External Electroweak Scale Input

## Goal

Implement one Phase LIII scale-source lane by ingesting a sourced external
disjoint electroweak input, then decide whether it is enough to project absolute
W/Z masses.

## Implementation

Added:

- `studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json`
- `studies/phase54_external_electroweak_scale_input_001/internal_bridge_search_report.json`
- `studies/phase54_external_electroweak_scale_input_001/STUDY.md`
- `tests/Gu.Phase5.Reporting.Tests/Phase54ExternalElectroweakScaleInputTests.cs`

The phase ingests the CODATA 2022/NIST Fermi coupling constant:

- `G_F/(hbar c)^3 = 1.1663787(6)e-5 GeV^-2`;
- derived `v = 1 / sqrt(sqrt(2) * G_F) = 246.21965079413738 GeV`.

## Finding

The external scale input is now present and disjoint from W/Z target masses, but
absolute W/Z mass projection remains blocked. The repository still lacks a
validated internal bridge from the Fermi-derived electroweak scale to
`internal-mass-unit`.

Existing internal artifacts are insufficient:

- W/Z physical mode records contain internal mass values but no GeV bridge;
- Phase XII coupling proxies are not validated physical weak couplings;
- Phase XXXIII normalization is internal and dimensionless.

## Next Step

Phase LV should derive or validate an internal electroweak bridge observable.
The bridge must provide a target-independent dimensionless relation between the
external electroweak scale and the validated W/Z internal modes. Once that
passes, the project can create a GeV-per-internal-mass-unit calibration and emit
absolute W/Z predictions.

## Validation

Completed:

- `jq -e . studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json`
- `jq -e . studies/phase54_external_electroweak_scale_input_001/internal_bridge_search_report.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 219, Failed: 0, Skipped: 0
- `git diff --check`
