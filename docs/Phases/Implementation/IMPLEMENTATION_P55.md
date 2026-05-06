# Phase LV - Internal Electroweak Bridge Candidate Audit

## Goal

Evaluate whether existing internal electroweak coupling features can bridge the
Phase LIV Fermi-derived electroweak scale to the validated W/Z internal mass
unit.

## Implementation

Added:

- `studies/phase55_internal_electroweak_bridge_candidate_audit_001/internal_bridge_candidate_audit.json`
- `studies/phase55_internal_electroweak_bridge_candidate_audit_001/weak_coupling_normalization_contract.json`
- `studies/phase55_internal_electroweak_bridge_candidate_audit_001/STUDY.md`
- `tests/Gu.Phase5.Reporting.Tests/Phase55InternalElectroweakBridgeCandidateAuditTests.cs`

The phase evaluates a rejected control that treats the Phase25/Phase27
current-coupling mean magnitudes as weak couplings.

## Finding

The current-coupling features are valid for W/Z identity selection but not for
absolute mass scale calibration. They are finite-difference profile statistics,
not normalized weak couplings.

The rejected control gives:

- W proxy mass: `7.139609437859668 GeV`;
- Z proxy mass: `8.20504516541525 GeV`;
- proxy mass ratio: `0.8701487065486907`;
- W implied scale: `6398611835505125 GeV/internal-mass-unit`;
- Z implied scale: `6468781217684903 GeV/internal-mass-unit`.

That is not a validated shared bridge.

## Next Step

Phase LVI should implement a normalized weak-coupling extraction contract or a
mass-generation bridge. The bridge must declare a normalization convention,
propagate uncertainty, and show that W and Z imply one shared
GeV-per-internal-mass-unit scale without using W or Z target masses.

## Validation

Completed:

- `jq -e . studies/phase55_internal_electroweak_bridge_candidate_audit_001/internal_bridge_candidate_audit.json`
- `jq -e . studies/phase55_internal_electroweak_bridge_candidate_audit_001/weak_coupling_normalization_contract.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 223, Failed: 0, Skipped: 0
- `git diff --check`
