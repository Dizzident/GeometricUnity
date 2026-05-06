# Phase 107 - Candidate-3 Target-Independent Normalization

## Goal

Attempt the second Phase105 prerequisite: derive a target-independent
normalization for candidate-3.

## Completed

- Added `studies/phase107_candidate3_target_independent_normalization_001`.
- Consumed the Phase106 observable identity result.
- Emitted a blocked normalization artifact with no scale factor.

## Result

Terminal status:

`candidate3-target-independent-normalization-blocked`

Normalization is blocked because Phase106 rejects candidate-3 as internal-only.
No target-derived scale is emitted.

## Verification

- `dotnet build studies/phase107_candidate3_target_independent_normalization_001/Phase107Candidate3TargetIndependentNormalization.csproj --verbosity minimal`
- `dotnet run --project studies/phase107_candidate3_target_independent_normalization_001/Phase107Candidate3TargetIndependentNormalization.csproj --no-build`
