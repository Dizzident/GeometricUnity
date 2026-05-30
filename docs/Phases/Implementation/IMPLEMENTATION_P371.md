# Phase371 Discrete Connection-to-Dirac First-Variation Coverage Audit

## Purpose

Phase371 materializes the strongest currently implementable precursor to the
unfinished `VO-7` coupled boson-fermion mixed-linearization program.

It replays the existing discrete SU(2) connection-to-Dirac first variation

```text
delta_D[b_k] = d D_h(omega)[b_k]
```

against every persisted Phase12 finite-difference variation matrix, checks
Hermiticity, and persists target-blind response diagnostics

```text
delta_D[b_k] phi_j
<phi_i, delta_D[b_k] phi_j>
```

for the minimum-index branch-local fermion mode on each background.

## Inputs

- `studies/phase12_joined_calculation_001/output/background_family/`
- `studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency_summary.json`
- `studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output/boson_fermion_coupling_proxy_source_audit_summary.json`
- `studies/phase370_completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_001/output/completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_summary.json`

## Outputs

- `studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/discrete_connection_dirac_first_variation_coverage_audit.json`
- `studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/discrete_connection_dirac_first_variation_coverage_audit_summary.json`
- `studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/responses/`

## Result

The bounded precursor passes:

- `variationCount=24`
- `analyticParityPassedCount=24`
- `hermiticityPassedCount=24`
- `responseArtifactCount=24`
- `responseParityPassedCount=24`
- `maxUnitScaleRelativeResidual=1.858480441423612E-12`
- `maxResponseRelativeResidual=2.157933267014728E-12`
- `maxSelectedProjectionRelativeResidual=4.1117298383234736E-11`

## Boundary

Phase371 is one implemented `VO-7` building block. It is not a completed
coupled action Hessian and does not fill any Phase201 or Phase256 prediction
field.

The repository still lacks a fixed GU fermionic operator branch, explicit
Yukawa functional, solved coupling map, coupled residual, reciprocal
fermion-to-boson backreaction block, remaining mixed blocks, gauge-compatibility
identities, scalar projection theorem, observed-field extraction, pole
extraction, and GeV normalization.

The persisted Phase12 sidecars use `epsilon=1e-5`. Phase371 records an
epsilon-ladder convergence rerun as a remaining diagnostic extension.
