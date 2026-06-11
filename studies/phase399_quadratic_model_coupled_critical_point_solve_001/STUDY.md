# Phase399: Quadratic-Model Self-Consistent Coupled Critical-Point Solve

## Question

The Phase398 ledger left one VO-7 control-branch component partial: coupled
stationarity had only the first-order picture. Can the self-consistent
coupled critical point be solved, and what is its structure?

## Construction

Within the exact quadratic model of S_B at the persisted background (H_B
from the Phase394 recomputed positive spectrum; 18-dim kernel flat) and the
exactly linear D (Phase389), iterate the fixed point
`d* = -kappa H_B^+ J(psi(d*))` with adiabatic (max-overlap) mode-following
of a target-blind shell eigenmode and the closed-form per-edge source
`J_(e,a) = (2/h_e) Re sum eps_abc W_e[b,c]` (verified against the variation
matrices at 4.4e-16). kappa ladder {0.001, 0.003} - the perturbative regime
of the shell (kappa = 0.1 was observed to diverge: the induced shift
~6.5e-3 kappa must stay below the shell scale ~8.4e-4).

## Results

- **Fixed point converged on every run** (2 backgrounds x 2 start modes x
  2 kappas): projected coupled gradient <= 9.5e-11 within <= 9 iterations.
  The convergence criterion is gradient-based: the solve lands on a critical
  ORBIT (the followed mode can rotate within the remaining 2-fold-degenerate
  split level without changing the gradient).
- **Flat-direction obstruction quantified**: the kernel component of the
  source (0.047 per unit kappa) cannot be relaxed within the quadratic
  model - the coupled critical point exists only MODULO the 18 flat bosonic
  directions. Whether the full non-quadratic action lifts them is beyond the
  persisted artifacts.
- **kappa scaling consistent with Phase394**: ||d*||/kappa matches the
  first-order backreaction within 5.5% at kappa = 0.001 (orbit-selection
  effect documented); the finite-kappa self-consistent correction reaches
  ~19% at kappa = 0.003.

This discharges the Phase398 partial VO-7 component at the QUADRATIC-MODEL
control-branch level.

## Status

Fail-closed. kappa is not physical; the full non-quadratic bosonic action
was not used; nothing promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj
dotnet run --project studies/phase399_quadratic_model_coupled_critical_point_solve_001/Phase399QuadraticModelCoupledCriticalPointSolve.csproj
```
