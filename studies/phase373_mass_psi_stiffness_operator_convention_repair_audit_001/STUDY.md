# Phase 373 Mass-Psi Stiffness/Operator Convention Repair Audit

## Purpose

This branch-local numerical experiment follows Phase372. It treats each
persisted Euclidean-Hermitian assembled Dirac matrix as a stiffness/action
matrix `K`, then derives

`A = M_psi^-1 K`

and

`B = M_psi^-1/2 K M_psi^-1/2`

using `MassPsiWeightsBuilder.BuildFromMesh(...)`.

The executable checks both persisted Phase12 backgrounds, all 24 persisted
Phase12 `deltaK` matrices, analytical `deltaK` matrices from
`DiracVariationComputer.ComputeAnalytical`, and all 288 selected-source
direction checks. It also runs a diagnostic mesh-weighted fermion-mode replay
by solving synthetic explicit `B` bundles with the existing
`FermionSpectralSolver`, then transforming `u` modes back to
`psi = M_psi^-1/2 u`.

## Run

```bash
dotnet run --project studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001
```

## Outputs

- Summary: `output/mass_psi_stiffness_operator_convention_repair_audit_summary.json`
- Full audit: `output/mass_psi_stiffness_operator_convention_repair_audit.json`
- Per-background evidence: `output/backgrounds/*.json`
- Per-variation evidence: `output/variations/*.json`

## Boundary

This is a branch-local convention candidate only. It does not change shared
solver code, prove a GU/local physical `M_psi` contract, complete a fermionic
action or mixed linearization, provide Yukawa or scalar-source laws, normalize
GeV units, or promote boson predictions.
