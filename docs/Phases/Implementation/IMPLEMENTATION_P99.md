# Implementation P99: Selector Eigenvector Full Lift

## Objective

Fix the concrete residual blocker from Phase98: the selected Phase43 selector
eigenmode did not have a persisted full `576`-length connection-space vector.

## Changes

- Added `studies/phase99_selector_eigenvector_full_lift_001/materialize_selector_eigenvector_full_lift.py`.
- The script reads the Phase43 candidate-3 selector spectrum for:
  - branch `bg-variant-53b598740d9569b4`;
  - refinement `L1-4x4`;
  - environment `env-structured-4x4`.
- It verifies that the selected selector eigenmode is the one-hot source-basis vector `[1, 0, 0]`.
- It lifts that selector basis vector through the available full connection-space basis:
  - basis axis 0 is the Phase96 source-backed `576`-length refinement vector.
- It writes a replay-ready full-lift boson mode with:
  - selector eigenvalue and selector metadata;
  - a persisted `modeVector` of length `576`;
  - explicit limitations for the unavailable secondary/tertiary selector axes.

## Validation

```bash
python3 studies/phase99_selector_eigenvector_full_lift_001/materialize_selector_eigenvector_full_lift.py
jq -e . studies/phase99_selector_eigenvector_full_lift_001/output/selector_eigenvector_full_lift_evidence.json
jq -e . studies/phase99_selector_eigenvector_full_lift_001/output/selector_eigenvector_full_lift_summary.json
```

Result:

- Selected Phase43 selector eigenvector: `[1, 0, 0]`.
- Full lifted vector length: `576`.
- Full lifted vector equals the Phase96 axis-0 source vector.
- Phase84 `4x4` replay builds with the lifted selector mode.
- Replay closure requirements are empty.
- Physical prediction gate blockers are empty.

## Scope

This fixes the concrete selected-mode blocker. It does not claim that the
secondary and tertiary selector-cell axes have full connection-space lifts.
