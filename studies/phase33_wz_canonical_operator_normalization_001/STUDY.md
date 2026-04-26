# Phase XXXIII W/Z Canonical Operator Normalization

This study derives the target-independent canonical W/Z normalization scale from
the shared internal mass-like operator unit used by the selected W/Z source
modes.

## Inputs

- `studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json`
- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`

## Outputs

- `canonical_operator_normalization_derivation.json`
- `physical_calibrations.json`
- `operator_normalization_source_audit_with_phase33.json`
- `wz_normalization_closure_with_phase33.json`

## Result

The canonical operator derivation is ready and promotable:

- source candidates: `phase12-candidate-0`, `phase12-candidate-2`;
- normalization convention: `shared-internal-mass-operator-unit`;
- dimensionless W/Z scale: `1`;
- target independent: `true`;
- proxy only: `false`.

P31 remains blocked after applying the derived calibration because the physical
target requires scale `1.0203591418928235`, not the derived canonical scale `1`.

## Interpretation

The normalization-source blocker is closed. The W/Z miss is now a normalized
prediction mismatch, so the next work should inspect the operator/eigenvalue
calculation path rather than adding calibration.
