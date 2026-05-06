# Phase LXVI - Weak-Coupling Uncertainty Propagation

## Goal

Close the uncertainty blocker for the Phase LXV weak-coupling candidate.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WeakCouplingUncertaintyPropagator.cs`
- `tests/Gu.Phase5.Reporting.Tests/WeakCouplingUncertaintyPropagatorTests.cs`
- `studies/phase66_weak_coupling_uncertainty_001/weak_coupling_uncertainty.json`
- `studies/phase66_weak_coupling_uncertainty_001/phase61_candidate_audit.json`
- `studies/phase66_weak_coupling_uncertainty_001/STUDY.md`

The propagator computes:

`sigma_g = sqrt((s * sigma_raw)^2 + (raw * sigma_s)^2)`

for raw matrix-element magnitude `raw`, generator scale `s`, raw matrix-element uncertainty `sigma_raw`, and generator-scale uncertainty `sigma_s`.

## Finding

The candidate now has finite non-negative uncertainty. It remains blocked only on missing branch-stability evidence.

## Next Step

Phase LXVII should attach branch-stability evidence across accepted variants.

## Validation

Completed:

- `jq -e . studies/phase66_weak_coupling_uncertainty_001/weak_coupling_uncertainty.json`
- `jq -e . studies/phase66_weak_coupling_uncertainty_001/phase61_candidate_audit.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 257, Failed: 0, Skipped: 0
- `git diff --check`
