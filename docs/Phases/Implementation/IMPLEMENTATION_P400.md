# IMPLEMENTATION_P400: Full-Bosonic-Action Flat-Direction Lift Probe

## Scope

Answers the residual physical question Phase399 recorded for the VO-7
coupled-stationarity component: whether the full non-quadratic bosonic
action lifts the 18 flat Gauss-Newton directions that obstruct the coupled
critical point (the 0.047-per-kappa kernel source component).

## Artifacts

- Study: `studies/phase400_full_bosonic_action_flat_direction_lift_probe_001`
- Project: `Phase400FullBosonicActionFlatDirectionLiftProbe.csproj`
- Outputs: `output/full_bosonic_action_flat_direction_lift_probe.json`
  and `..._summary.json`
- Precursors: Phase394 working directory (recomputed spectrum + kernel),
  Phase399 summary (obstruction present).

## Method and results

The toy bosonic objective is exactly quartic (Upsilon = F - T^aug is
quadratic in omega), so along kernel directions (J k = 0, d^* k = 0) the
expansion terminates: `S(omega0+tk) - S0 = (t^2/2)<U0, M Q(k,k)> +
(t^4/8)||Q(k,k)||_M^2`, with `Q` recovered exactly by symmetric second
differences of the production residual assembly. The GN-dropped curvature
form `B_ij = <U0, M Q(k_i,k_j)>` is assembled by polarization and
diagonalized; eigendirections are classified and gauge-aligned; the
fermionic source kernel component is decomposed onto the classification.

| Quantity | Value |
| --- | --- |
| Kernel directions classified | 36/36 (18 per background) |
| Exactly flat directions | 0 |
| Quartic lift norms | min 1.2e-3, max 5.8e-2 (tol 5.8e-8) |
| Quadratic coefficients | residual-scale, \|beta\| <= 5.5e-11 (Cauchy-Schwarz-bounded, asserted) |
| Max saddle depth | 4.5e-19 (background-residual artifact) |
| Obstruction lifted fraction | 1.000000 (every shell mode, both backgrounds) |
| Phase394 cross-check | kernel norm-fractions 0.3475/0.3655 square to 0.121/0.134 |
| Gauge alignment | kernel not gauge-dominated (fractions <= 0.46, orbit rank 69) |
| Verification battery | orthonormality 4.6e-15, annihilation 2.4e-15, quadratic exactness 7.4e-16, GN parity 3.0e-14, polarization 8.1e-15 |

Structural conclusion: **the full toy bosonic action lifts all 18 flat
directions at quartic order, and the Phase399 flat-direction obstruction is
fully relaxable at higher order** - the coupled critical point exists as a
genuine critical point of the full toy action, not merely modulo flat
directions. This closes the VO-7 coupled-stationarity component at the
full-toy-action control-branch level; the physical gap ledger (8 items) is
unchanged.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`fullBosonicActionFlatDirectionLiftProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `full-bosonic-action-flat-direction-lift-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase400 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase400 run passes (battery fully verified; verdict
  `all-lifted-with-saddle-directions` with saddle depth machine-bounded).
- Phase101, Phase202 (checklist 192 -> 193 passed), claim-integrity verifier
  re-run with Phase400 included; objective remains incomplete by design.
