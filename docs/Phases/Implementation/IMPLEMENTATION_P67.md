# Phase LXVII - Weak-Coupling Branch Stability

## Goal

Close the branch-stability blocker for the normalized weak-coupling candidate.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WeakCouplingBranchStabilityBuilder.cs`
- `tests/Gu.Phase5.Reporting.Tests/WeakCouplingBranchStabilityBuilderTests.cs`
- `studies/phase67_weak_coupling_branch_stability_001/weak_coupling_branch_stability.json`
- `studies/phase67_weak_coupling_branch_stability_001/phase61_candidate_audit.json`
- `studies/phase67_weak_coupling_branch_stability_001/STUDY.md`

The builder scores branch stability as:

`score = clamp(1 - maxRelativeDeviation / relativeTolerance, 0, 1)`

## Finding

The fixture branch variants produce score `0.98`, above the Phase LXI threshold of `0.95`. The Phase LXI audit accepts the candidate.

## Next Step

Phase LXVIII should promote the accepted candidate as the available normalized weak-coupling input and update the electroweak bridge-derivation blocker state.

## Validation

Completed:

- `jq -e . studies/phase67_weak_coupling_branch_stability_001/weak_coupling_branch_stability.json`
- `jq -e . studies/phase67_weak_coupling_branch_stability_001/phase61_candidate_audit.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 257, Failed: 0, Skipped: 0
- `git diff --check`
