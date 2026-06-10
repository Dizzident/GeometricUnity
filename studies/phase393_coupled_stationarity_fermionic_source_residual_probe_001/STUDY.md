# Phase393: Coupled Stationarity Fermionic Source Residual Probe

## Question

The remaining constructive VO-7 step is a coupled-critical-point
construction. Since the persisted background `omega` is a bosonic critical
point, the coupled gradient there is exactly the fermionic source current
`J_k(psi_s) = Re<psi_s, delta_D[e_k] psi_s>` on the converged shell. What is
the structure of this coupled residual, and is first-order backreaction
constructible from persisted artifacts?

## Construction

Per persisted Phase12 background, on the converged lowest-nonzero shell
(Phase390/392 dense path):

1. Per-mode and shell-aggregated source currents `J` (156-vectors).
2. Projections of `J` onto three orthonormalized subspaces: the persisted
   bosonic Gauss-Newton mode span, the span of the 84 Phase389 discrete
   covariant differentials (pure gauge), and the top-3 eigenvectors of the
   Phase378-rule response Gram recomputed on the converged shell.
3. The degenerate shell-splitting pattern under the unit source direction:
   eigenvalues of the 4x4 matrices `<psi_a, delta_D[J-hat] psi_b>`.

## Results (three structural findings)

1. **Exact plus/minus source cancellation.** All four per-mode source norms
   are identical (0.1129 / 0.1212), and the shell-aggregated source vanishes
   to ratio ~4e-11: plus/minus eigenvalue partners carry exactly opposite
   currents. Under symmetric shell occupation the persisted background is
   ALREADY first-order stationary in the coupled sense; backreaction starts
   at second order, where the Phase392 response operator is the leading
   object.
2. **Per-mode sources live exactly in the rank-3 Gram image** (fraction
   1.0 to 7e-16): the diagonal coupled sources inhabit the Phase378 carrier
   image identically. About two-thirds of each current is pure-gauge
   (0.61 - 0.68) and about a tenth lies in the persisted bosonic numerical
   kernel (0.08 - 0.11).
3. **Backreaction is not constructible from persisted artifacts.** All 12
   persisted bosonic Gauss-Newton eigenvalues per background are ~1e-15
   (numerical kernel only). The kernel component of any asymmetric source is
   a first-order obstruction no bosonic relaxation can absorb, and the
   complement requires the positive Gauss-Newton spectrum, which was never
   persisted. Completing the coupled-critical-point step beyond symmetric
   occupation requires recomputing the positive bosonic spectrum.

The unit-source shell splitting is a clean doubly degenerate plus/minus pair
pattern (+-6.49e-3 for bg-a, +-7.22e-3 for bg-b, per unit coupling).

## Status

Fail-closed. No coupled critical point constructed, no physical coupling,
no physical Hessian, no observed namespace map, no Phase201/Phase256 fields.
`canFillPhase201WzContract=False`.

## Reproduce

```bash
dotnet run --project studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/Phase393CoupledStationarityFermionicSourceResidualProbe.csproj
```
