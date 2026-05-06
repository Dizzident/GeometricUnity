# Phase LII - Electroweak Absolute Scale Audit

## Goal

Advance from the Phase LI readiness matrix to the first concrete blocker for
absolute boson masses: the missing GeV scale for the already validated W and Z
internal modes.

## Implementation

Added:

- `studies/phase52_electroweak_absolute_scale_audit_001/absolute_scale_audit.json`
- `studies/phase52_electroweak_absolute_scale_audit_001/absolute_mass_projection_contract.json`
- `studies/phase52_electroweak_absolute_scale_audit_001/STUDY.md`
- `tests/Gu.Phase5.Reporting.Tests/Phase52ElectroweakAbsoluteScaleAuditTests.cs`

The audit records:

- the validated internal W and Z mode values;
- two rejected target-fit scale controls;
- the exact projection contract required for a target-independent W/Z absolute
  mass campaign.

## Finding

The current path cannot honestly claim absolute W or Z mass predictions yet.
Fitting one target mass is not an allowed calibration, and the fitted controls
do not cross-check cleanly:

- W-fitted scale predicts Z at `91.36071050377454 GeV`;
- Z-fitted scale predicts W at `80.2172681143632 GeV`.

This means the next useful implementation work is a scale-source input and
projection phase, not another claim-promotion phase.

## Next Step

Phase LIII should add a replayable target-independent electroweak scale source.
Valid sources may be either:

- a derived GU internal scale with GeV units; or
- an external/disjoint physical input contract that excludes W and Z masses.

The output must emit `physical-w-boson-mass-gev` and
`physical-z-boson-mass-gev` observables with propagated uncertainty and run a
target comparison only after projection.

## Validation

Completed:

- `jq -e . studies/phase52_electroweak_absolute_scale_audit_001/absolute_scale_audit.json`
- `jq -e . studies/phase52_electroweak_absolute_scale_audit_001/absolute_mass_projection_contract.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 211, Failed: 0, Skipped: 0
- `git diff --check`
