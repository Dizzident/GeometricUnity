# Phase373 Mass-Psi Stiffness/Operator Convention Repair Audit

## Purpose

Phase373 follows the Phase372 mesh-volume `M_psi` obstruction. It tests a
branch-local representation contract without modifying shared solver code:

```text
K = persisted Euclidean-Hermitian assembled Dirac matrix
A = M_psi^-1 K
B = M_psi^-1/2 K M_psi^-1/2
```

The intended identities are:

```text
A^dagger M_psi = M_psi A = K
B = M_psi^1/2 A M_psi^-1/2
<chi, A psi>_M = chi^dagger K psi
```

## Inputs

- `studies/phase12_joined_calculation_001/output/background_family/`
- `studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/`
- `studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/`
- `docs/Architecture/ARCH_P4.md`

## Outputs

- `studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/mass_psi_stiffness_operator_convention_repair_audit.json`
- `studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/mass_psi_stiffness_operator_convention_repair_audit_summary.json`
- `studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/backgrounds/`
- `studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/variations/`

## Result

The branch-local algebraic convention candidate passes:

- `massPsiStiffnessOperatorConventionRepairAuditPassed=true`
- `phase372MeshVolumeWeightObstructionPresent=true`
- `transformedBaseBackgroundCount=2`
- `transformedVariationIdentityPassedCount=24`
- `transformedDirectionalIdentityPassedCount=288`
- `transformedAnalyticPersistedParityPassedCount=24`
- `maxBaseAWeightedAdjointRelativeResidual=3.521639958898684E-17`
- `maxBaseBHermiticityRelativeResidual=0`
- `maxDirectionalPairingIdentityScaleAwareResidual=8.441528768080324E-17`
- `maxDirectionalCentralDerivativeScaleAwareResidual=8.210085389315225E-11`

The matching synthetic-`B` replay is separately materialized and fails the
strict shared-solver quality gate:

- `matchingWeightedModeReplayMaterialized=true`
- `matchingWeightedModeReplayQualityPassed=false`
- `maxModeReplayBRelativeResidual=1.1279684115339121`
- `maxModeReplayGeneralizedRelativeResidual=1.3606147345417028`
- `maxModeReplayMOrthonormalityResidual=2.643431958340854E-11`

## Boundary

Phase373 is a branch-local discrete representation candidate only. It does not
prove that the current mesh-volume lumping is physically canonical, repair the
shared weighted spectral solver, establish a fixed GU fermionic action or
Yukawa map, complete mixed-linearization blocks, provide gauge identities,
derive a W/Z bridge law or Higgs scalar row, normalize GeV units, or promote a
boson prediction.

The next implementation step is a shared-solver correctness repair benchmarked
against `B`, with generalized `K psi = lambda M_psi psi` residuals and
`M_psi` orthonormality required before production mesh weighting is enabled.
