# Phase 111 - Replayed Matrix-Element Repair Attempt

## Goal

Execute the replayed analytic raw matrix-element strategy from Phase110.

## Completed

- Added `studies/phase111_replayed_matrix_element_repair_attempt_001`.
- Consumed the Phase99 replay package summary, Phase108 candidate-3 closure,
  Phase110 repair contract, and Phase63 generator normalization.
- Verified that candidate-3 replayed matrix-element evidence is validated, but
  not compatible with W/Z absolute repair.

## Result

Terminal status:

`replayed-matrix-element-evidence-valid-but-wz-repair-incompatible`

The normalized candidate-3 coupling is `0.00007550296271372928`, while the
Phase110 target-implied weak coupling is `0.6522081710229882`. More importantly,
Phase108 closes candidate-3 as internal-only, so this evidence cannot repair the
W/Z absolute mass path.

## Verification

- `dotnet build studies/phase111_replayed_matrix_element_repair_attempt_001/Phase111ReplayedMatrixElementRepairAttempt.csproj --verbosity minimal`
- `dotnet run --project studies/phase111_replayed_matrix_element_repair_attempt_001/Phase111ReplayedMatrixElementRepairAttempt.csproj --no-build`
