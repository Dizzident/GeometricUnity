# Phase LX - Electroweak Bridge Derivation Input Audit

## Goal

Make the remaining electroweak bridge blocker executable and precise.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/ElectroweakBridgeDerivationInputAuditor.cs`
- `tests/Gu.Phase5.Reporting.Tests/ElectroweakBridgeDerivationInputAuditorTests.cs`
- `studies/phase60_electroweak_bridge_derivation_input_audit_001/bridge_derivation_input_audit.json`
- `studies/phase60_electroweak_bridge_derivation_input_audit_001/STUDY.md`

The audit records which bridge derivation inputs are available and which are
missing.

## Finding

Available:

- validated W/Z internal modes;
- Fermi-derived external electroweak scale.

Missing:

- normalized internal weak coupling;
- validated internal mass-generation relation;
- scalar/Higgs-sector bridge evidence;
- shared W/Z scale check.

## Next Step

Phase LXI should implement one bridge-source lane:

- normalized weak-coupling extraction from internal operator artifacts; or
- scalar-sector/mass-generation relation extraction.

Once one lane exists, materialize an `ElectroweakBridgeRecord` and pass it
through `ElectroweakBridgeValidator`.

## Validation

Completed:

- `jq -e . studies/phase60_electroweak_bridge_derivation_input_audit_001/bridge_derivation_input_audit.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 238, Failed: 0, Skipped: 0
- `git diff --check`
