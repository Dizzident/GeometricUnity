# Phase LXXII - Absolute W/Z Scale Calibration From Validated Bridge

## Goal

Feed the Phase LXXI validated bridge through the existing electroweak absolute scale calibration builder.

## Implementation

Added:

- `tests/Gu.Phase5.Reporting.Tests/Phase72To74WzAbsolutePredictionPipelineTests.cs`
- `studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json`
- `studies/phase72_wz_absolute_scale_calibration_001/STUDY.md`

The calibration combines:

- Phase LIV Fermi-derived electroweak scale `v`;
- Phase LXXI validated electroweak bridge;
- W/Z physical mass mappings.

## Finding

The shared scale is validated:

- `scaleFactorGeVPerInternalMassUnit = 62413568563037690`;
- `scaleUncertaintyGeVPerInternalMassUnit = 321043849913151.3`.

## Next Step

Phase LXXIII should project absolute W/Z mass observables using this scale.

## Validation

Completed:

- `jq -e . studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 267, Failed: 0, Skipped: 0
- `git diff --check`
