# Phase 317 - Electroweak Mass Matrix Bridge Source Audit

Phase317 audits the tempting shortcut of treating the Standard Model
electroweak mass matrix as the missing W/Z/H bridge-source law.

The PDG 2025 electroweak review records the low-energy dependency map:
SU(2)xU(1), a Higgs doublet VEV, charged W combinations, photon/Z Weinberg
rotation, and tree-level W/Z/H mass formulas. That is the correct external
physics structure, but it is not a GU source-lineage theorem.

Current result:

- `terminalStatus=electroweak-mass-matrix-bridge-source-audit-dependency-map-not-gu-source`
- `electroweakMassMatrixBridgeSourceAuditPassed=true`
- `smMassMatrixProvidesExternalDependencyMap=true`
- `smMassMatrixProvidesGuLocalWzTheorem=false`
- `smMassMatrixProvidesGuObservedFieldExtraction=false`
- `smMassMatrixProvidesGuVevSource=false`
- `smMassMatrixProvidesGuWeakCouplingSource=false`
- `smMassMatrixProvidesGuHiggsScalarSourceOperator=false`
- `smMassMatrixJustifiesWOnlyCasimirMultiplier=false`
- `smMassMatrixJustifiesZUnitMultiplier=false`
- `smMassMatrixPromotesWzMasses=false`
- `smMassMatrixPromotesHiggsMass=false`
- `canFillPhase201WzContract=false`
- `canFillPhase201HiggsContract=false`
- `canFillPhase256ObservedFieldExtractionContract=false`

The audit is deliberately non-promotional. The Standard Model mass matrix
explains why the Phase302/307 Casimir shortcut is insufficient: W/Z splitting
requires a neutral-sector mixing and electroweak-parameter source, not a
W-only Casimir multiplier with an unexplained Z unit multiplier.

Artifacts:

- `studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit.json`
- `studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json`
