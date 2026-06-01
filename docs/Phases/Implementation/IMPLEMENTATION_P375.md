# Phase375 Weighted Reciprocal Mixed-Block Replay Audit

## Purpose

Phase375 replays the Phase372 reciprocal discrete fermionic source-block
candidate using the repaired Phase374 weighted modes and the Phase373
representation contract:

```text
K = persisted Euclidean-Hermitian stiffness matrix
A = M_psi^-1 K
deltaA = M_psi^-1 deltaK
K psi = lambda M_psi psi
J_k(psi) = Re<psi, deltaA_k psi>_M
```

The replay is restricted to fixed-mesh connection-space perturbations, so
`delta M_psi=0`.

## Inputs

- `studies/phase12_joined_calculation_001/output/background_family/`
- `studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/`
- `studies/phase374_shared_weighted_fermion_spectral_solver_repair_audit_001/output/`

## Result

The discrete weighted reciprocal replay passes:

- `weightedReciprocalMixedBlockReplayAuditPassed=true`
- `backgroundCount=2`
- `variationPassedCount=24/24`
- `directionalIdentityPassedCount=288/288`
- `pairingIdentityPassedCount=288/288`
- `reciprocalDerivativeEqualityPassedCount=288/288`
- `centralDerivativeLadderPassedCount=288/288`

The selected Phase12 source modes are exact kernel modes:

- `phase12SelectedWeightedNonzeroModeCount=0/24`
- `weightedSourceModesAreKernelOnly=true`
- `nonzeroPersistedWeightedCurrentCount=0`
- `nonzeroPersistedWeightedReciprocalDerivativeCount=0`

## Boundary

This is a discrete-only fixed-mesh zero-mode replay. It does not establish a
physical GU fermionic action, canonical `M_psi`, nonzero-spectrum mixed-block
proof, completed coupled Hessian, corrected-gauge identities, W/Z bridge law,
Higgs scalar source, pole extraction, GeV normalization, or boson prediction.
