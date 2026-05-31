# Phase 372 Discrete Fermionic Bilinear Reciprocal Mixed-Block Audit

## Purpose

Materialize and audit the local candidate

`S_F^candidate(omega, psi) = Re<psi, D_h(omega) psi>`

with branch-local source components

`J_k(psi) = Re<psi, delta_D[b_k] psi>`.

The study depends on the passed, nonpromotional Phase371 summary and reuses the
persisted Phase12 backgrounds, fermion modes, boson variation directions, base
Dirac matrices, and finite-difference variation matrices.

For each of the 24 persisted variation directions, the source spinor `psi` is
selected target-blind as the branch-local persisted fermion mode with minimum
`modeIndex`. Every one of the 12 branch-local fermion modes is evaluated as a
direction `chi_i`. Each variation sidecar records analytical and persisted
matrix parity, reciprocal response pairings, two-term current derivatives,
an explicit epsilon ladder for central finite differences of `J_k`, and
Hermitian adjoint identity residuals.

The required passing audit is explicitly the Phase12 identity-weight control
branch, built with `MassPsiWeightsBuilder.BuildIdentity(...)`. Phase12 modes
were solved with the default identity `M_psi`; this control is not presented as
physical. A separate diagnostic uses `MassPsiWeightsBuilder.BuildFromMesh(...)`
and records mesh-volume M-adjoint and central-derivative metrics. A failure of
that diagnostic remains an actionable requirement to rebuild an
`M_psi`-compatible Dirac branch and solve matching modes.

Each row also separates the independent `psi-bar` and `psi` source derivatives
from the Hermitian real-form shortcut `2 Re<chi_i, A psi>`.

## Run

```bash
dotnet run --project studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001
```

## Outputs

- Summary: `output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json`
- Full audit: `output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit.json`
- Per-variation evidence: `output/variations/*.json`

## Boundary

This is a reciprocal discrete bilinear source-block candidate and a VO-7
building block. It is not completed VO-7 and does not provide a completed GU
fermionic action, a fixed GU fermionic branch, an explicit Yukawa term, a
solved coupling map, a coupled residual, completed mixed blocks, gauge
compatibility identities, a W/Z bridge law, a Higgs scalar operator, a scalar
projection theorem, GeV normalization, physical predictions, or fills for
Phase201 or Phase256.
