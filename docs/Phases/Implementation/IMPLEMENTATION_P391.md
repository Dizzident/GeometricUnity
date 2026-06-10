# IMPLEMENTATION_P391: Dense Converged-Shell Shell-Response Replay Audit

## Scope

Phase391 settles the question Phase390 raised: are the Phase378/379
shell-response invariants (rank-3 carrier image, suppressed gauge axis 1,
failed strict transport) solver artifacts? It replays the entire
Phase378/379 pipeline on the exact dense generalized eigensolve and compares
the invariants quantitatively. Verdict: **confirmed** - the invariants are
solver-independent properties of the discretized control branch.

## Artifacts

- Study: `studies/phase391_dense_converged_shell_response_replay_audit_001`
- Project: `Phase391DenseConvergedShellResponseReplayAudit.csproj`
- Outputs:
  - `output/dense_converged_shell_response_replay_audit.json`
  - `output/dense_converged_shell_response_replay_audit_summary.json`

## Method

Identical pipeline to Phase378/379, with the iterative weighted solver
replaced by the Phase390 dense complex Hermitian Jacobi solve of
`B = M^{-1/2} D M^{-1/2}`:

- Shell: lowest-nonzero |lambda| group, Phase378 grouping tolerance; 4-mode
  shell per background, M-orthonormal `v = M^{-1/2} w`.
- Blocks: `G_i = V_shell^dagger deltaK[e_i] V_shell`, all 156 coordinates,
  `deltaK` from `DiracVariationComputer.ComputeAnalytical` on unit vectors.
- Rank: eigenvalues of the 32x32 dual feature Gram above
  `max(1e-14, 1e-10 * max|eig|)`.
- Axes: squared components of the top-rank eigenvectors of the 156x156
  response Gram grouped by `coordinate % 3`, normalized by rank.
- Transport: singular values of the 3x3 overlap between background positive
  eigenspaces.

The Gram invariants are invariant under any M-orthonormal change of shell
basis, so agreement tests the shell subspace itself.

## Results

| Quantity | Dense replay | vs persisted Phase378/379 |
| --- | --- | --- |
| Shell eigenvalues | 4-mode shell | relative delta <= 1.5e-10 |
| Positive rank | 3 (both backgrounds) | match |
| Suppressed axis | 1 (both backgrounds) | match |
| Axis fractions | bg-a [0.5429, 0.00166, 0.4555]; bg-b [0.5200, 0.00077, 0.4793] | abs delta <= 1.7e-10 |
| Min transport SV | 0.79970408362 | delta <= 2.2e-11 |
| Strict transport | fails (>= 0.99 required) | match |

Diagnostic consequences recorded in the journal: the suppressed-axis
blockers (Phases 381/383/384) and the Phase388 theorem requirements
`w-row-source-theorem-explains-suppressed-axis` and
`phase307-selector-escape-from-suppressed-axis` rest on solver-independent
ground; the Phase374-repaired weighted solver is validated to ~1e-10 here.

## Fail-closed boundary

Zero Phase201/Phase256 fields accepted; all physical route flags false
(`routeProvidesPhysicalEffectiveActionHessian`,
`routeProvidesObservedElectroweakNamespaceMap`,
`routeProvidesCanonicalGaugeAxisSelector`, promotion flags, contract fills).

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`denseConvergedShellResponseReplayAudit` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `dense-converged-shell-response-replay-audit-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase391 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase391 run passes with verdict `confirmed`.
- Phase101, Phase202 (checklist 183 -> 184 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase391 included; objective remains
  incomplete by design.
