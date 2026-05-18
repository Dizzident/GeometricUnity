# Phase 250 - Phase46 Electroweak Feature Source-Lineage Audit

Purpose: audit the existing Phase46 electroweak-feature W/Z spectra as a concrete local lead for the missing W/Z absolute bridge-source law.

Result: Phase46 contains SU(2) adjoint-triplet electroweak-feature labels and separate W/Z internal mode records, but its promoted physical calibration is explicitly a dimensionless W/Z mass-ratio normalization. It does not provide source-lineage contract fields (`sourceLineageId`, `sourceRowId`, `theoremOrDerivationId`, raw-amplitude gates), does not provide GeV-scale absolute mass observables, and does not supply the theorem needed to apply the P221/P249 adjoint RMS factor to the Phase64 fermion-current trace-half source.

Outputs:

- `studies/phase250_phase46_electroweak_feature_source_lineage_audit_001/output/phase46_electroweak_feature_source_lineage_audit.json`
- `studies/phase250_phase46_electroweak_feature_source_lineage_audit_001/output/phase46_electroweak_feature_source_lineage_audit_summary.json`

Promotion boundary: Phase46 remains useful for W/Z ratio/internal electroweak-feature diagnostics only. It does not fill the P245/P247 W/Z absolute-scale unlock.
