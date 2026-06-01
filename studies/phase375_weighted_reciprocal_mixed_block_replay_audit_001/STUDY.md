# Phase 375 Weighted Reciprocal Mixed Block Replay Audit

## Purpose

This discrete-only audit follows the Phase373 stiffness/operator convention
candidate and the Phase374 shared weighted solver repair. It replays the
Phase372 reciprocal fermionic source-block candidate with shared weighted
modes:

```text
A = M_psi^-1 K
deltaA = M_psi^-1 deltaK
K psi = lambda M_psi psi
J_k(psi) = Re<psi, deltaA_k psi>_M
```

For both persisted Phase12 backgrounds, it solves the persisted stiffness
matrix `K` through `FermionSpectralSolver` with nonuniform mesh-volume weights
from `MassPsiWeightsBuilder.BuildFromMesh(...)`. It verifies generalized
residuals, `M_psi` norms, and `M_psi` orthonormality before selecting the
minimum weighted `modeIndex` as the target-blind source.

For all 24 persisted and analytic `deltaK` matrices, it evaluates all 12
weighted direction modes. The replay verifies the weighted-operator to
stiffness pairing identity, reciprocal real derivative, justified Hermitian
shortcut, central finite-difference ladder, and analytic/persisted matrix and
directional parity.

This is a fixed-mesh connection-space replay: `delta M_psi = 0` while each
connection perturbation `b_k` varies `K` and therefore
`deltaA = M_psi^-1 deltaK`.

## Run

```bash
dotnet run --project studies/phase375_weighted_reciprocal_mixed_block_replay_audit_001
```

## Outputs

- Summary: `output/weighted_reciprocal_mixed_block_replay_audit_summary.json`
- Full audit: `output/weighted_reciprocal_mixed_block_replay_audit.json`
- Per-background evidence: `output/backgrounds/*.json`
- Per-variation evidence: `output/variations/*.json`

## Boundary

The selected Phase12 weighted modes are kernel modes. This audit reports
nonzero current and derivative counts but is not a nonzero-spectrum eigenmode
proof. It does not establish a physical GU branch, a canonical physical
`M_psi`, a completed fermionic action, fixed branch, Yukawa sector, coupled
residual, completed mixed blocks, gauge identities, W/Z bridge law, Higgs
scalar operator, scalar projection theorem, GeV normalization, prediction
promotion, or Phase201/Phase256 fill.
