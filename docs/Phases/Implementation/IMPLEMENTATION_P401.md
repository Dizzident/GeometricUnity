# IMPLEMENTATION_P401: Full-Quartic-Action Coupled Critical-Point Construction Attempt

## Scope

Attempts the constructive completion left optional by Phase400: solve the
coupled critical point of the full quartic control-branch action with the
kernel obstruction relaxed. The attempt returns a machine-characterized
NEGATIVE structural result: the kernel relaxation is non-perturbative.

## Artifacts

- Study: `studies/phase401_full_quartic_action_coupled_critical_point_construction_001`
- Project: `Phase401FullQuarticActionCoupledCriticalPointConstruction.csproj`
- Outputs: `output/full_quartic_action_coupled_critical_point_construction.json`
  and `..._summary.json`
- Precursors: Phase394 workdir (spectrum/kernel), Phase399 (obstruction),
  Phase400 (all rays lifted).

## Method and results

Exact solver on the quartic objective: GN-preconditioned positive steps +
18-dim exact kernel Newton (closed-form kernel gradient/Hessian via
zero-truncation second differences) + exact quartic line searches (every
step verified monotone descent). Frozen-source runs (J fixed at the base
shell = the Phase399 obstruction object) on kappa {1e-8,1e-7,1e-6}, trust
radius 1.0; adiabatic self-consistent probe at kappa 1e-7, trust radius
0.15.

| Quantity | Value |
| --- | --- |
| kappa=0 baselines | converged, gradient <= 2.7e-17 in 2 iterations |
| Coupled runs | 12/12 exit trust region, none reach stationarity |
| Effective kernel potential at amplitude 1.0 | S_B ~ 1e-13..5e-10 |
| Valley anisotropy ratio | 1.4e8 (softest ray quartic / path-effective quartic) |
| Adiabatic source growth at exit | 5.3x / 7.4x (overlap dips to 0.68) |
| Battery | source parity 4.4e-16, gradient parity 4.7e-10, line-search exactness 3.6e-15 |

Structural conclusion: **no perturbative coupled critical point exists on
the toy control branch.** The positive sector relaxes to absorb nearly all
of Q(d,d), leaving near-null valleys of the quartic form along which the
linear fermionic pull dominates indefinitely; the self-consistent source
additionally strengthens with displacement. The relaxed coupled vacuum is
not a small deformation of the persisted background - welding the VO-7
coupled-stationarity component to the physical gap-ledger item "4D
observed vacuum".

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`fullQuarticActionCoupledCriticalPointConstruction` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `full-quartic-action-coupled-critical-point-construction-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase401 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase401 run passes (battery verified; all runs characterized).
- Phase101, Phase202 (checklist 193 -> 194 passed), claim-integrity verifier
  re-run with Phase401 included; objective remains incomplete by design.
