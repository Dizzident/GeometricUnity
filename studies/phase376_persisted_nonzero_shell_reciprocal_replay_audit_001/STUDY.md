# Phase 376 Persisted Nonzero Shell Reciprocal Replay Audit

## Purpose

This bounded discrete-only audit follows the Phase375 weighted reciprocal
replay. It solves both persisted Phase12 stiffness matrices with the shared
`FermionSpectralSolver` and mesh-volume `M_psi`, requesting modes `0..52`.

The explicit kernel filter is:

```text
abs(lambda) <= 1e-12
```

The target-blind source construction selects the complete lowest nonzero
spectral shell using:

```text
groupingTolerance = max(1e-12, 1e-8 * abs(lambda_min_nonzero))
```

The required shell is indices `48..51`, with sentinel index `52` outside the
shell. For all 24 persisted and analytic `deltaK` variations, the audit builds:

```text
G_k = Psi_shell^dagger deltaK Psi_shell
    = <psi_i, deltaA psi_j>_M
deltaA = M_psi^-1 deltaK
delta M_psi = 0
```

It validates persisted/analytic block parity, the weighted pairing identity,
nonzero block Frobenius norms, Hermiticity, basis-invariant block metrics, and
unchanged geometry and mass-weight hashes throughout replay.

## Structural Disclosure

The ambient mesh contains isolated vertices `[19, 20, 23, 26]`. They induce
`48` exact zero-row/zero-column complex fermion DOFs and use the shared
mesh-volume builder's isolated-vertex fallback weight `1.0`. The filtered
kernel is therefore a topology artifact at this boundary.

The persisted metadata supports `connection-1form` vectors of length `156`
from `52` ambient edges times gauge dimension `3`. It does not persist
geometry or `M_psi` hashes. This study records that metadata limitation and
uses deterministic study-derived hashes for replay immutability checks.

## Run

```bash
dotnet run --project studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001
```

## Outputs

- Summary: `output/persisted_nonzero_shell_reciprocal_replay_audit_summary.json`
- Full audit: `output/persisted_nonzero_shell_reciprocal_replay_audit.json`
- Per-background evidence: `output/backgrounds/*.json`
- Per-variation evidence: `output/variations/*.json`

## Boundary

This is a nonphysical fixed-mesh, connection-only, discrete replay. It does
not establish canonical physical `M_psi`, a fixed GU branch, a completed
fermionic action, completed mixed blocks, corrected-gauge identities, a
direct W/Z bridge law, a Higgs row, GeV normalization, predictions, or
contract fills.
