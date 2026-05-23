# Phase352 Higgs-Top-Z NNLO Matching Source Audit

Phase352 records the 2026 Higgs-top-Z NNLO matching update as a focused
negative boson-source audit. The source is relevant because it sharpens the
existing Phase262 empirical relation `M_H^2 ~= M_Z M_t` using updated
electroweak inputs and the ATLAS-CMS top-mass combination.

The audit is intentionally non-promotional. The pole-level ratio remains close,
but the source uses measured top, Z, W, and Higgs inputs for the test. Its NNLO
running-coupling boundary is incompatible with the measured point and would
need a finite pole-threshold/matching factor or new custodial/top-Higgs
mechanism that this repository does not derive.

## Outputs

- `studies/phase352_higgs_top_z_nnlo_matching_source_audit_001/output/higgs_top_z_nnlo_matching_source_audit.json`
- `studies/phase352_higgs_top_z_nnlo_matching_source_audit_001/output/higgs_top_z_nnlo_matching_source_audit_summary.json`

## Validation Role

Phase352 is wired into:

- `scripts/generate_validated_boson_predictions.sh`
- `studies/phase101_boson_prediction_package_001`
- `studies/phase202_boson_objective_completion_audit_001`
- `scripts/verify_boson_claim_integrity.sh`

The expected terminal status is
`higgs-top-z-nnlo-matching-source-audit-pole-coincidence-running-boundary-fails`.
