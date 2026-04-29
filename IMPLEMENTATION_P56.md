# Phase LVI - Electroweak Bridge Validator

## Goal

Turn the Phase LV weak-coupling bridge contract into executable validation code.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/ElectroweakBridgeRecord.cs`
- `tests/Gu.Phase5.Reporting.Tests/ElectroweakBridgeValidatorTests.cs`
- `studies/phase56_electroweak_bridge_validator_001/bridge_validator_contract.json`
- `studies/phase56_electroweak_bridge_validator_001/STUDY.md`

The validator accepts only:

- `normalized-internal-weak-coupling`;
- `validated-internal-mass-generation-relation`.

It rejects:

- finite-difference current profile hashes;
- coupling-profile magnitude statistics;
- W/Z target-fitted scales;
- missing target exclusions;
- missing uncertainty, normalization convention, or mass-generation relation.

## Finding

Absolute W/Z projection now has an executable bridge gate. This prevents future
phases from accidentally promoting the Phase25/Phase27 coupling proxy features
or any W/Z target-fit scale into a physical calibration.

## Next Step

Phase LVII should derive an actual `ElectroweakBridgeRecord` from normalized
internal weak-coupling or mass-generation evidence and run it through
`ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection`.

## Validation

Completed:

- `jq -e . studies/phase56_electroweak_bridge_validator_001/bridge_validator_contract.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 228, Failed: 0, Skipped: 0
- `git diff --check`
