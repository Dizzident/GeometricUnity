# IMPLEMENTATION_P393: Coupled Stationarity Fermionic Source Residual Probe

## Scope

Phase393 is the first installment of the coupled-critical-point program
("Best Next Work" branch 1 after Phase392). It characterizes the coupled
stationarity residual at the persisted backgrounds and settles whether
first-order backreaction is constructible from persisted artifacts.

## Artifacts

- Study: `studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001`
- Project: `Phase393CoupledStationarityFermionicSourceResidualProbe.csproj`
- Outputs:
  - `output/coupled_stationarity_fermionic_source_residual_probe.json`
  - `output/coupled_stationarity_fermionic_source_residual_probe_summary.json`

## Method

The coupled action candidate is `S = S_B(omega) + kappa S_F(omega, psi)`,
`S_F = Re<psi, D(omega) psi>`. At the persisted bosonic critical point the
coupled omega-gradient is `kappa J`, `J_k = Re<psi_s, delta_D[e_k] psi_s>`,
evaluated on the converged lowest-nonzero shell (dense Jacobi path from
Phase390/392). The probe computes per-mode and aggregated `J`, projects them
onto (a) the orthonormalized persisted bosonic Gauss-Newton mode span,
(b) the orthonormalized span of the 84 Phase389 covariant differentials,
(c) the top-3 Gram eigenvectors recomputed on the converged shell, and
diagonalizes the 4x4 degenerate-perturbation matrix for the unit source
direction.

## Results

| Quantity | bg-a | bg-b |
| --- | --- | --- |
| Per-mode source norms | 0.11294 (x4, identical) | 0.12116 (x4, identical) |
| Aggregate cancellation ratio | 3.9e-11 | 5.4e-12 |
| Gram-image fraction (per mode) | 1.0 | 1.0 |
| Pure-gauge fraction (per mode) | 0.679 | 0.608 |
| Bosonic-kernel fraction (per mode) | 0.083 | 0.112 |
| Unit-source shell splitting | +-6.487e-3 (x2) | +-7.223e-3 (x2) |
| Max persisted bosonic eigenvalue | 1.5e-15 | 1.2e-15 |

Key consequences recorded in the journal:

1. The exact plus/minus cancellation means symmetric shell occupation leaves
   the background first-order coupled-stationary; the leading backreaction
   is second order - exactly the Phase392 response operator. The
   "non-coupled-critical-point" caveat on Phase392 is therefore softened for
   symmetric occupation at first order.
2. The per-mode sources lie identically in the rank-3 Gram image, tying the
   Phase378 carrier image to the diagonal coupled sources exactly.
3. The persisted bosonic spectrum is numerical-kernel-only (~1e-15), so
   `delta_omega = -kappa H_B^+ J` is not constructible from persisted
   artifacts; the positive Gauss-Newton spectrum at these backgrounds must
   be recomputed to proceed beyond symmetric occupation.

## Fail-closed boundary

No coupled critical point, no physical coupling, all route flags false, zero
Phase201/Phase256 fields, target-blind construction hash persisted.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`coupledStationarityFermionicSourceResidualProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `coupled-stationarity-fermionic-source-residual-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase393 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase393 run passes.
- Phase101, Phase202 (checklist 185 -> 186 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase393 included; objective remains
  incomplete by design.
