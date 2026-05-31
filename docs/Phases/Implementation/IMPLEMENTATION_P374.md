# Phase374 Shared Weighted Fermion Spectral Solver Repair Audit

## Purpose

Phase374 repairs and audits the shared weighted fermion spectral solver after
Phase373 distinguished:

```text
K = persisted Euclidean-Hermitian stiffness matrix
A = M_psi^-1 K
B = M_psi^-1/2 K M_psi^-1/2
```

The repaired path solves `B u=lambda u`, back-transforms
`psi=M_psi^-1/2 u`, and reports residuals for
`K psi=lambda M_psi psi`.

## Inputs

- `studies/phase12_joined_calculation_001/output/background_family/`
- `studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/`
- `src/Gu.Phase4.Dirac/FermionSpectralSolver.cs`

## Result

The discrete shared-solver audit passes:

- `sharedWeightedFermionSpectralSolverRepairAuditPassed=true`
- `phase373SyntheticBReplayQualityPassed=true`
- `backgroundCount=2`
- `weightedGeneralizedResidualPassedCount=24/24`
- `weightedMNormalizationPassedCount=24/24`
- `weightedMOrthonormalityPassedBackgroundCount=2/2`
- `identityRegressionPassedCount=2/2`

The selected persisted Phase12 modes are exact kernel modes:

- `phase12SelectedWeightedNonzeroModeCount=0/24`
- `phase12SelectedWeightedModesAreKernelOnly=true`

To prevent a kernel-only replay from overstating the repair, the audit requires
an independent nonuniform-`M_psi` nonzero-spectrum solve:

- `syntheticNonzeroWeightedBenchmarkPassed=true`
- `syntheticWeightedNonzeroModeCount=4/4`
- `maxSyntheticWeightedGeneralizedRelativeResidual=8.173154784325226E-15`

## Boundary

This is a bounded dense discrete reference-solver repair. It does not prove a
physical GU fermionic branch, canonical `M_psi`, completed coupled action,
mixed-block gauge identities, W/Z bridge law, Higgs scalar source, pole
extraction, GeV normalization, or boson prediction.
