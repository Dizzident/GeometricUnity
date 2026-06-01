# Phase376 Persisted Nonzero-Shell Reciprocal Replay Audit

## Purpose

Phase376 extends the Phase375 zero-mode control to the complete target-blind
lowest nonzero weighted shell of both persisted Phase12 stiffness matrices:

```text
K psi = lambda M_psi psi
G_k = Psi_shell^dagger deltaK_k Psi_shell
    = <psi_i, deltaA_k psi_j>_M
deltaA_k = M_psi^-1 deltaK_k
```

The replay is restricted to fixed-mesh connection-space perturbations, so
`delta M_psi=0`.

## Inputs

- `studies/phase12_joined_calculation_001/output/background_family/`
- `studies/phase375_weighted_reciprocal_mixed_block_replay_audit_001/output/`
- `src/Gu.Phase4.Dirac/FermionSpectralSolver.cs`

## Result

The bounded discrete nonzero-shell replay passes:

- `persistedNonzeroShellReciprocalReplayAuditPassed=true`
- `weightedModeCount=106/106`
- `filteredKernelModeCount=96/96`
- `shellModeCount=8/8`
- `sentinelOutsideShellPassedCount=2/2`
- `variationPassedCount=24/24`
- `persistedAnalyticBlockParityPassedCount=24/24`
- `projectedPairingIdentityPassedCount=24/24`
- `nonzeroProjectedBlockFrobeniusPassedCount=24/24`
- `minPersistedProjectedBlockFrobeniusNorm=7.020152634073169E-4`

Each background has a four-mode first nonzero shell at indexes `48..51`.
Index `52` is persisted as an outside-shell sentinel.

## Topology Boundary

The filtered `48`-mode kernel per background is a mesh artifact. The ambient
toy mesh has isolated vertices `[19,20,23,26]`, which induce `48` exact
zero-row/zero-column complex fermion DOFs and use fallback `M_psi` weight
`1.0`.

Phase12 does not persist geometry or `M_psi` hashes. Phase376 records that
limitation and verifies deterministic study-derived hashes remain unchanged
throughout replay.

## Boundary

This is a nonphysical fixed-mesh, connection-only, discrete projected-shell
audit. It does not establish canonical physical `M_psi`, a fixed GU branch,
a completed fermionic action, completed mixed blocks, corrected-gauge
identities, a direct W/Z bridge law, a Higgs row, GeV normalization,
predictions, or contract fills.
