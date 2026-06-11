# IMPLEMENTATION_P399: Quadratic-Model Coupled Critical-Point Solve

## Scope

Discharges the last partial VO-7 control-branch component (Phase398 ledger:
coupled stationarity) by solving the self-consistent coupled critical point
within the exact quadratic bosonic model.

## Artifacts

- Study: `studies/phase399_quadratic_model_coupled_critical_point_solve_001`
- Project: `Phase399QuadraticModelCoupledCriticalPointSolve.csproj`
- Outputs: `output/quadratic_model_coupled_critical_point_solve.json`
  and `..._summary.json`
- Precursor: the Phase394 working directory (recomputed bosonic spectrum).

## Method and results

Fixed point `d* = -kappa H_B^+ J(psi(d*))` with dense re-eigensolve of
`D + delta_D[d]` each iteration, adiabatic mode-following, and the verified
closed-form per-edge source. kappa in {0.001, 0.003} (perturbative regime;
0.1 diverges - documented in-code).

| Quantity | Value |
| --- | --- |
| Runs (bg x start x kappa) | 8, all converged |
| Projected coupled gradient | <= 9.5e-11 (within <= 9 iterations) |
| Source-formula parity | 4.4e-16 |
| Kernel obstruction per kappa | 0.047 (unrelaxable flat-direction residual) |
| kappa-scaling deviation vs Phase394 | 5.5% (orbit-aware tolerance 10%) |
| Self-consistent correction at kappa=0.003 | ~19% |

Structural conclusions: the coupled critical point exists modulo the 18
flat bosonic directions (the genuine quadratic-model obstruction for
asymmetric occupation); convergence is to a critical orbit within the
degenerate split level; first-order consistency with Phase394 verified.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`quadraticModelCoupledCriticalPointSolve` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `quadratic-model-coupled-critical-point-solve-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase399 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase399 run passes (8/8 runs converged).
- Phase101, Phase202 (checklist 191 -> 192 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase399 included; objective remains
  incomplete by design.
