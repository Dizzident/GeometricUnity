# Phase 280 - Direct Bridge Analytic Variation Upgrade Audit

Phase280 tests the most direct possible repair for the P190/P191 W/Z direct bridge candidate: recompute the current best P190 bridge matrix elements using the analytic Dirac-variation operator and check whether that changes the raw scale or supplies the missing source row.

The audit intentionally fails closed. It finds that the persisted P190 finite-difference matrix for bg-b matches the registry representative perturbation, not the bg-b branch-local contributing mode. Replaying the correct branch-local analytic perturbation removes the sibling-stability evidence and still leaves the raw-gate failure, missing W/Z theorem, and missing particle-specific source rows.

Current result:

- `terminalStatus=direct-bridge-analytic-variation-upgrade-audit-no-repair`
- `directBridgeAnalyticVariationUpgradeAuditPassed=true`
- `analyticVariationMatchesP190FiniteDifference=false`
- `finiteVariationMatchesRegistryRepresentativeMode=true`
- `p190FiniteVariationUsesRegistryRepresentativeMode=true`
- `branchLocalAnalyticRelativeSpread=0.46107786593154654`
- `branchLocalAnalyticStabilityPassed=false`
- `analyticRawGatePassed=false`
- `theoremClaimed=false`
- `wZParticleSplitPresent=false`
- `canRepairDirectBridgeWithAnalyticVariation=false`

Artifacts:

- `studies/phase280_direct_bridge_analytic_variation_upgrade_audit_001/output/direct_bridge_analytic_variation_upgrade_audit.json`
- `studies/phase280_direct_bridge_analytic_variation_upgrade_audit_001/output/direct_bridge_analytic_variation_upgrade_audit_summary.json`
