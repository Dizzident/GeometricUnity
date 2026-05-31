# Phase372 Discrete Fermionic-Bilinear Reciprocal Mixed-Block Audit

## Purpose

Phase372 extends the Phase371 connection-to-Dirac first-variation precursor
with a reciprocal discrete source-block candidate for the local control action

```text
S_F^candidate(omega, psi) = Re<psi, D_h(omega) psi>
J_k(psi) = Re<psi, delta_D[b_k] psi>
```

For each persisted Phase12 bosonic variation `b_k`, it selects the
minimum-index branch-local fermion mode `psi` without target data and evaluates
every persisted branch-local fermion direction `chi_i`.

## Evidence

The official GU draft supplies the research direction, not a completed
discretization:

- Section 9.3 equations `9.16-9.20` describe a Dirac-like spinorial operator
  and compile bosonic and fermionic variations into a mixed `Upsilon`.
- Equation `10.5` places the spinorial terms in the deformation-complex
  equation.
- The diagram following equation `10.10` is explicitly caveated as carried
  over from an older version and potentially inconsistent until stabilized.

The local v29 completion document keeps the fermionic action and `VO-7`
coupled deformation complex as typed obligations. Phase372 therefore tests a
local control candidate only.

## Inputs

- `studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/discrete_connection_dirac_first_variation_coverage_audit_summary.json`
- `studies/phase12_joined_calculation_001/output/background_family/`
- `docs/Architecture/ARCH_P4.md`

## Outputs

- `studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit.json`
- `studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json`
- `studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/variations/`

## Result

The identity-weight Phase12 control branch passes:

- `variationCount=24`
- `backgroundCount=2`
- `directionalCheckCount=288`
- `responsePairingParityPassedCount=288`
- `currentDirectionalDerivativeParityPassedCount=288`
- `centralFiniteDifferenceConvergencePassedCount=288`
- `hermitianAdjointIdentityPassedCount=288`
- `maxMatrixRelativeResidual=1.858480441423612E-12`
- `maxDirectionalAnalyticVsPersistedScaleAwareResidual=9.71445146547012E-11`
- `maxCentralDerivativeScaleAwareResidual=7.176240504613851E-11`

This branch is explicitly a control branch. Phase12 modes were solved with the
default identity `M_psi`.

The architecture-preferred mesh-volume diagnostic is materialized separately
and exposes a convention obstruction:

- `meshVolumeWeightBranchCompatible=false`
- `maxMeshVolumeWeightBaseDiracMAdjointRelativeResidual=0.34982671637306856`
- `maxMeshVolumeWeightVariationMAdjointRelativeResidual=0.7139201615144989`

## Boundary

Phase372 is a reciprocal discrete bilinear source-block candidate and one
`VO-7` building block. It is not a completed GU fermionic action, fixed GU
fermionic branch, coupled residual, completed mixed Hessian, corrected-gauge
identity package, W/Z bridge law, Higgs scalar-source operator, physical
observed-field extraction, pole extraction, GeV normalization, or boson
prediction.

The next actionable experiment is to test the stiffness/operator convention:
interpret the persisted Euclidean-Hermitian matrix as `K`, derive
`A = M_psi^-1 K`, and test the symmetric representative
`B = M_psi^-1/2 K M_psi^-1/2` before modifying shared solver code.
