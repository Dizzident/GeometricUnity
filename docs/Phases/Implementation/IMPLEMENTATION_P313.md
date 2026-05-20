# Phase 313: Official Draft Electroweak Projection Map Audit

## Purpose

Phase313 checks a narrow remaining W/Z loophole: whether the official draft's
weak-isospin / weak-hypercharge placement language, combined with the
repository's internal Cartan convention, can be treated as a physical
photon/Z/W projection map.

## Inputs

- `studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json`
- `studies/phase46_electroweak_term_wz_physical_prediction_001/promotion_result.json`
- `studies/phase46_electroweak_term_wz_physical_prediction_001/selector_eigen_operator_term_audit.json`
- `studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json`
- `studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json`
- `studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json`
- `studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json`
- `studies/phase311_completion_observed_sector_wz_row_selector_audit_001/output/completion_observed_sector_wz_row_selector_audit_summary.json`
- `studies/phase312_current_public_gu_rvg_revision_delta_audit_001/output/current_public_gu_rvg_revision_delta_audit_summary.json`

## Result

Phase313 is a negative audit. It preserves that:

- Phase27 has a target-independent internal Cartan charged/neutral convention.
- Phase46 has a defensible dimensionless W/Z ratio lane.
- The official draft has electroweak placement leads.

It does not promote W/Z or Higgs masses because the checked material still lacks
a physical photon/Z Weinberg rotation, unbroken electromagnetic generator,
weak-mixing/coupling source, neutral mass-matrix diagonalization, and
branch-stable observed W/Z/photon projection rows.

## Outputs

- `studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit.json`
- `studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json`

## Integration

Phase313 is wired into:

- `scripts/generate_validated_boson_predictions.sh`
- `studies/phase101_boson_prediction_package_001`
- `studies/phase202_boson_objective_completion_audit_001`
- `scripts/verify_boson_claim_integrity.sh`
- source-lineage, observed-field, GU-RVG, and Higgs-source scanner exclusions
