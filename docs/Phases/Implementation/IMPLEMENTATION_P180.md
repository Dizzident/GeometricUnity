# Implementation P180: Prediction Blocker Root-Cause Procedure

P180 replaces repeated speculative unblocker attempts with an executable root-cause procedure.

## Procedure

1. Inventory the current prediction rows, route selection, execution package, scientific boundary, W/Z bridge experiments, and massless-sector experiments.
2. Build a gate matrix for every route: diagnostic, target contract, identity, source, stability, attempt, and promotion.
3. Check for contradictions between comparison rows, validated rows, route status, execution package status, scientific boundary status, and massless benchmark contracts.
4. Classify every blocked row as missing evidence, failed validation, negative local experiment, or pipeline inconsistency.
5. Permit another prediction attempt only when at least one route has an open attempt/promotion gate and all integrity checks pass.
6. Otherwise fail closed with the root cause.

## Outputs

- `studies/phase180_prediction_blocker_root_cause_procedure_001/output/prediction_blocker_root_cause_procedure.json`
- `studies/phase180_prediction_blocker_root_cause_procedure_001/output/prediction_blocker_root_cause_procedure_summary.json`
- `studies/phase180_prediction_blocker_root_cause_procedure_001/output/prediction_blocker_root_cause_procedure.md`

## Interpretation

The phase can validate the prediction package and still refuse to promote new values. That is the intended behavior when the blocker is scientific evidence rather than implementation mechanics.
