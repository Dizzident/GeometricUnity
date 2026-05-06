# Phase LVIII - Electroweak Absolute Scale Calibration Builder

## Goal

Add the code that will turn a future validated electroweak bridge into shared
absolute W/Z mass calibrations.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/ElectroweakAbsoluteScaleCalibrationBuilder.cs`
- `tests/Gu.Phase5.Reporting.Tests/ElectroweakAbsoluteScaleCalibrationBuilderTests.cs`
- `studies/phase58_electroweak_absolute_scale_calibration_builder_001/calibration_builder_contract.json`
- `studies/phase58_electroweak_absolute_scale_calibration_builder_001/STUDY.md`

The builder computes:

- `scale = externalElectroweakScaleGeV * dimensionlessBridgeValue`;
- `scaleUncertainty = sqrt((externalUncertainty * bridgeValue)^2 + (externalScale * bridgeUncertainty)^2)`.

It emits one shared scale into W and Z `PhysicalCalibrationRecord` entries only
when the bridge passes `ElectroweakBridgeValidator`.

## Finding

The calibration construction path is ready, but it is intentionally inert until
a real bridge exists. This closes another software gap without weakening the
scientific gate.

## Next Step

Phase LIX should derive a real bridge record or, if no derivation is available,
implement the bridge-derivation failure as executable diagnostics over the
current internal coupling and mass-generation artifacts.

## Validation

Completed:

- `jq -e . studies/phase58_electroweak_absolute_scale_calibration_builder_001/calibration_builder_contract.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 233, Failed: 0, Skipped: 0
- `git diff --check`
