# Implementation P96: Refinement Boson Vector Materialization

## Objective

Clear the immediate `4x4` replay closure blocker requiring a source-backed
refinement boson `modeVector` with length `576`.

## Changes

- Added `studies/phase96_refinement_boson_vector_materialization_001/materialize_refinement_boson_vector.py`.
- Materialized the Phase11 `4x4` Shiab refinement connection state into an explicit boson replay artifact:
  - source state: `bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830_omega.json`;
  - source shape: `[192, 3]`;
  - materialized `modeVector` length: `576`;
  - normalization convention: `raw-connection-coefficients`.
- Replayed Phase84 at `4x4` using:
  - the Phase96 materialized boson vector;
  - the Phase95 target-blind matched `4x4` fermion bundle.

## Validation

```bash
python3 studies/phase96_refinement_boson_vector_materialization_001/materialize_refinement_boson_vector.py
jq -e . studies/phase96_refinement_boson_vector_materialization_001/output/refinement_boson_vector_materialization_evidence.json
jq -e . studies/phase96_refinement_boson_vector_materialization_001/output/refinement_boson_vector_materialization_summary.json
```

Result:

- The materialized boson replay vector has length `576`.
- The Phase84 `4x4` replay probe reports `source-backed-analytic-replay-package-built`.
- `replayClosureRequirements` is empty.
- `physicalPredictionGateBlockers` is empty for the replay validator.

## Scope

This clears the vector-length replay blocker. It does not claim external W/Z
identification: the materialized vector is the source-backed Phase11 refinement
background connection state, not a selector eigenmode.

## Remaining Blockers

- The identity fermion-space lift still needs a derivation against the connection-space gauge quotient.
- The materialized boson vector is not yet tied to a selector eigenmode identification.
