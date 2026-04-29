# Phase LVII - Absolute W/Z Bridge Gate Integration

## Goal

Integrate the Phase LVI electroweak bridge validator into the physical
prediction projector so absolute W/Z mass predictions cannot bypass the bridge
contract.

## Implementation

Updated:

- `src/Gu.Phase5.Reporting/PhysicalPredictionProjector.cs`
- `tests/Gu.Phase5.Reporting.Tests/PhysicalObservableContractTests.cs`

Added:

- `studies/phase57_absolute_wz_bridge_gate_integration_001/projector_bridge_gate.json`
- `studies/phase57_absolute_wz_bridge_gate_integration_001/STUDY.md`

`PhysicalPredictionProjector.Project` now accepts an optional
`ElectroweakBridgeTable`. For absolute W/Z mass mappings, the projector blocks
prediction unless at least one bridge passes
`ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection`.

## Finding

The prediction path is now protected against accidental absolute W/Z promotion.
A validated calibration alone is no longer enough for
`physical-w-boson-mass-gev` or `physical-z-boson-mass-gev`.

## Next Step

Phase LVIII should derive a real `ElectroweakBridgeRecord` from normalized
internal weak-coupling or mass-generation evidence. If it passes the bridge
validator, the following phase can combine it with the Phase LIV Fermi-derived
scale and emit absolute W/Z observables.

## Validation

Completed:

- `jq -e . studies/phase57_absolute_wz_bridge_gate_integration_001/projector_bridge_gate.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 230, Failed: 0, Skipped: 0
- `git diff --check`
