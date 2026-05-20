# Phase 314: Dimension/Casimir W/Z Source-Law Audit

## Purpose

Phase314 checks the narrowest remaining interpretation of the Phase302/307 W/Z
near-pass: whether `source-mode-vector-length=156` and the W multiplier `8/3`
can be promoted as a target-independent geometric source law.

## Inputs

- `studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json`
- `studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json`
- `studies/phase82_boson_perturbation_vector_materializer_001/boson_perturbation_vector_materializer.json`
- `studies/phase84_first_boson_prediction_attempt_001/output/first_boson_prediction_attempt.json`
- `studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json`
- `studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants_summary.json`
- `studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json`
- `studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json`
- `studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json`
- `studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit_summary.json`
- `studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit_summary.json`
- `studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json`

## Result

Phase314 is a negative audit. It records that:

- `156` is already explained locally as the Phase12 discrete connection-vector
  coordinate count: `52` mesh edges times `dimG=3`, with unit-M-norm mode
  normalization.
- `2 * dim so(13) = 156` is only arithmetic coincidence in the current repo
  state; no source artifact derives the Phase12 vector scale from Spin(13) or
  SO(13).
- `8/3` is real SU(2) adjoint/fundamental Casimir arithmetic, but existing
  source evidence still uses the Phase63/64 trace-half fermion-current
  convention.
- No checked artifact justifies applying `8/3` only to W rows while assigning
  multiplier `1` to the Z row. That would still require a physical neutral
  projection and W/Z source-row theorem.

## Outputs

- `studies/phase314_dimension_casimir_wz_source_law_audit_001/output/dimension_casimir_wz_source_law_audit.json`
- `studies/phase314_dimension_casimir_wz_source_law_audit_001/output/dimension_casimir_wz_source_law_audit_summary.json`

## Integration

Phase314 is wired into:

- `scripts/generate_validated_boson_predictions.sh`
- `studies/phase101_boson_prediction_package_001`
- `studies/phase202_boson_objective_completion_audit_001`
- `scripts/verify_boson_claim_integrity.sh`
- source-lineage, observed-field, GU-RVG, and Higgs-source scanner exclusions
