# Phase401: Full-Quartic-Action Coupled Critical-Point Construction Attempt

## Question

Phase399 solved the coupled critical point in the quadratic model (modulo
the 18 flat directions); Phase400 proved the full quartic toy action lifts
every flat RAY. Can the relaxed coupled critical point - with the kernel
obstruction actually relaxed - be constructed in the full quartic action?

## Construction

Full coupled objective `S_total(d) = S_B(omega0+d) + kappa <psi, D(omega0+d) psi>`
with the production residual assembly, penalty referenced at omega0 (so the
GN quadratic form equals the compute-spectrum operator), and the gradient
`g = grad S_B + kappa J` (Hellmann-Feynman; D exactly linear). Solver:
GN-preconditioned positive-subspace steps and an 18-dim EXACT kernel Newton
(the kernel-restricted gradient and Hessian have closed forms because
Upsilon is quadratic; second differences have zero truncation error), each
followed by an exact quartic line search, so every step is verified
monotone descent. Two source treatments: FROZEN (J fixed at the base shell
mode - exactly the Phase399 obstruction object; no eigensolves) on a kappa
ladder {1e-8, 1e-7, 1e-6} with trust radius 1.0, and ADIABATIC
(self-consistent psi re-solve, kappa = 1e-7) with trust radius 0.15.
Battery: source parity 4.4e-16, analytic-vs-fit gradient parity 4.7e-10,
line-search exactness 3.6e-15, kappa = 0 baseline converged to 2.7e-17 in
2 iterations.

## Results

- **NO PERTURBATIVE COUPLED CRITICAL POINT EXISTS.** Every coupled run (12
  of 12 across 2 backgrounds, 3 kappas, 2 start modes, both source
  treatments) descends monotonically out of its trust region without
  reaching stationarity; the kappa = 0 baselines converge immediately, so
  the machinery is sound and the descent is real.
- **Mechanism - near-null valleys of the quartic form:** Phase400's lift is
  real on every ray, but the positive sector relaxes to absorb almost all
  of Q(d,d): the measured effective kernel potential along the exact-Newton
  descent path is `S_B ~ 1e-13..5e-10` even at kernel amplitude 1.0 -
  **1.4e8 times softer** than the softest per-direction quartic
  (valleyAnisotropyRatio). Along these positive-relaxed valleys the linear
  fermionic pull (kappa |J_ker| ~ 1e-9..1e-7) dominates indefinitely.
- The adiabatic probe adds a second driver: the followed-mode source
  STRENGTHENS with displacement (growth 5.3x / 7.4x at exit, adiabatic
  overlap dipping to 0.68) - the self-consistent relaxation is even more
  strongly non-perturbative.
- The diagonal cube-root warm-start prediction underestimates the
  relaxation by orders of magnitude (recorded, not gated) - direct evidence
  that the per-direction picture misses the valley structure.

## Interpretation

Combined closure of the coupled-stationarity question on the toy control
branch: the quadratic-model critical point exists modulo flat directions
(Phase399), every flat ray is quartically lifted (Phase400), but the
relaxed coupled vacuum is NOT a small deformation of the persisted
background (Phase401) - the kernel relaxation is non-perturbative under
both source treatments. This welds the VO-7 coupled-stationarity component
to the physical gap-ledger item "4D observed vacuum": a trustworthy coupled
vacuum requires the physical derivation chain, not toy-branch relaxation.

## Status

Fail-closed. The probe gates on the verified measurement battery and the
terminate-or-exit characterization of every run - the negative verdict is
data, not a gate. Toy Mode-B objective only; kappa non-physical; nothing
promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj
dotnet run --project studies/phase400_full_bosonic_action_flat_direction_lift_probe_001/Phase400FullBosonicActionFlatDirectionLiftProbe.csproj
dotnet run --project studies/phase401_full_quartic_action_coupled_critical_point_construction_001/Phase401FullQuarticActionCoupledCriticalPointConstruction.csproj
```
