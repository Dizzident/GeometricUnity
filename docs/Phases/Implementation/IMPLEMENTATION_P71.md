# Phase LXXI - Shared W/Z Scale Bridge

## Goal

Close the shared W/Z scale check and materialize a validated electroweak bridge record.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/SharedWzScaleBridgeBuilder.cs`
- `tests/Gu.Phase5.Reporting.Tests/SharedWzScaleBridgeBuilderTests.cs`
- `studies/phase71_shared_wz_scale_bridge_001/shared_wz_scale_bridge.json`
- `studies/phase71_shared_wz_scale_bridge_001/bridge_derivation_input_audit_after_phase71.json`
- `studies/phase71_shared_wz_scale_bridge_001/STUDY.md`

The shared-scale check compares the W-derived and Z-derived bridge values and emits a validated `ElectroweakBridgeRecord` when the relative spread is inside tolerance.

## Finding

All electroweak bridge derivation inputs are now available. The next step is to feed the validated bridge through the existing absolute W/Z calibration builder and projector.

## Validation

Completed:

- `jq -e . studies/phase71_shared_wz_scale_bridge_001/shared_wz_scale_bridge.json`
- `jq -e . studies/phase71_shared_wz_scale_bridge_001/bridge_derivation_input_audit_after_phase71.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 263, Failed: 0, Skipped: 0
- `git diff --check`
