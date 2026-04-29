# Phase LXXIII - Absolute W/Z Mass Projection

## Goal

Use the Phase LXXII calibration to emit absolute W and Z mass predictions.

## Implementation

Added:

- `studies/phase73_wz_absolute_mass_projection_001/wz_absolute_mass_projection.json`
- `studies/phase73_wz_absolute_mass_projection_001/STUDY.md`

The existing `WzAbsoluteMassObservableProjector` applies the validated shared scale to the validated internal W/Z modes.

## Finding

Projected values:

- W: `69.64143389516731 +/- 0.3679656283006216 GeV`;
- Z: `79.16578591256517 +/- 0.4189929362561984 GeV`.

## Next Step

Phase LXXIV should compare these projected values to the physical targets.

## Validation

Completed:

- `jq -e . studies/phase73_wz_absolute_mass_projection_001/wz_absolute_mass_projection.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 267, Failed: 0, Skipped: 0
- `git diff --check`
