# Phase391: Dense Converged-Shell Shell-Response Replay Audit

## Question

Phase390 proved the persisted Phase12 fermion modes are non-eigen mixtures,
raising the question of whether the Phase378/379 shell-response invariants -
stable positive rank 3, two-axis gauge dominance with suppressed gauge axis 1,
and failed strict background transport - are properties of the discretized
branch or solver artifacts. (Phase378 ran its own iterative weighted solve
with in-study residual checks, so the question is solver fidelity, not the
Phase12 persisted-mode defect.) These invariants are the active blockers
behind the Phase307 W near-pass rejection (Phases 381/383/384) and two of the
Phase388 theorem requirements.

## Construction

Per persisted Phase12 background, the full Phase378/379 pipeline is replayed
on an EXACT dense reference:

1. Dense complex Hermitian Jacobi solve of `B = M^{-1/2} D M^{-1/2}`
   (Phase390 solver), generalized modes `v = M^{-1/2} w`, `D v = lambda M v`.
2. Lowest-nonzero-shell selection with the Phase378 grouping rule
   (`max(1e-12, 1e-8 * |lambda_min|)`); 4-mode shell expected.
3. `G_i = V_shell^dagger deltaK[e_i] V_shell` for all 156 carrier
   coordinates (`DiracVariationComputer.ComputeAnalytical` on unit basis
   vectors, identical to Phase378).
4. Dual feature Gram rank (Phase378 tolerance), 156x156 response Gram
   `Q_ij = Re Tr(G_i^dagger G_j)`, top-rank eigenvectors, gauge-axis
   projector fractions (`coordinate % 3`), inter-background transport
   singular values (Phase379 rules).
5. Quantitative comparison against the persisted Phase378/379 outputs. The
   Gram invariants depend only on the shell SUBSPACE (basis-invariant), so
   this is a sharp solver-independence test.

## Result: CONFIRMED

- Shell eigenvalues match the Phase378 weighted-solver shell to relative
  `1.5e-10`; dense shell eigen-residuals `2.9e-13`.
- Positive rank 3 on both backgrounds (matches Phase378).
- Gauge-axis projector fractions match Phase379 to `1.7e-10`; suppressed
  gauge axis 1 on both backgrounds (bg-a: [0.5429, 0.00166, 0.4555];
  bg-b: [0.5200, 0.00077, 0.4793]).
- Inter-background minimum singular value `0.79970408362` (matches Phase379
  to `2.2e-11`); strict transport still fails.
- Block Hermiticity at machine precision (`6.4e-16`).

Conclusion: the rank-3 carrier image and the suppressed gauge axis are
solver-independent properties of the discretized control branch, NOT
artifacts. The Phase381/383/384 suppressed-axis blockers against the Phase307
W near-pass stand on solver-independent ground, and the Phase374-repaired
weighted solver is incidentally validated to ~1e-10 on this problem.

## Status

Fail-closed. This is a study-defined discrete diagnostic: no physical
effective-action Hessian, no observed electroweak namespace map, no canonical
gauge-axis selector, no W/Z/H source rows, and no Phase201/Phase256 contract
fields. `canFillPhase201WzContract=False`.

## Reproduce

```bash
dotnet run --project studies/phase391_dense_converged_shell_response_replay_audit_001/Phase391DenseConvergedShellResponseReplayAudit.csproj
```
