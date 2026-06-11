# Phase400: Full-Bosonic-Action Flat-Direction Lift Probe

## Question

Phase399 solved the coupled critical point within the exact quadratic model
and left one residual physical question for the VO-7 coupled-stationarity
component: the kernel component of the fermionic source (0.047 per unit
kappa) is unrelaxable within the quadratic model, so the coupled critical
point exists only MODULO the 18 flat bosonic directions. Does the full
non-quadratic bosonic action lift them?

## Construction

On the toy control branch the full bosonic objective
`S(omega) = (1/2)<Upsilon, M_R Upsilon> + (lambda/2)||d^* omega||^2` is
QUARTIC in omega (Upsilon = F - T^aug is exactly quadratic), so for a
Gauss-Newton kernel direction k (J k = 0 and d^* k = 0, both verified) the
expansion terminates exactly:

```
S(omega0 + t k) - S0 = (t^2/2) <Upsilon0, M Q(k,k)> + (t^4/8) <Q(k,k), M Q(k,k)>
Q(k,k) = (Upsilon(omega0 + t k) + Upsilon(omega0 - t k) - 2 Upsilon0) / t^2   (no truncation error)
```

using the repo's own production residual assembly (`CpuResidualAssembler`)
at the Phase394 recomputed backgrounds. The probe assembles the GN-dropped
curvature form `B_ij = <Upsilon0, M Q(k_i,k_j)>` on the 18-dim kernel by
polarization, diagonalizes it, classifies every eigendirection
(quadratically lifted / saddle / quartically lifted / exactly flat),
measures gauge alignment against the infinitesimal gauge-orbit map, and
decomposes the Phase393/394/399 fermionic source kernel component onto the
classified directions. Verification battery: kernel orthonormality
(4.6e-15), J- and d^*-annihilation (2.4e-15), exact quadraticity of the
second differences (7.4e-16, t = 0.5 vs 0.25), GN operator parity on
positive modes (3.0e-14), polarization consistency (8.1e-15), and the
Cauchy-Schwarz bound |beta| <= ||Upsilon0|| ||Q|| (asserted).

## Results

- **ALL 18 flat directions are LIFTED by the full action in both
  backgrounds** (36/36): every kernel eigendirection has a nonzero quartic
  residual norm (min 1.2e-3, max 5.8e-2, tolerance 5.8e-8). ZERO exactly
  flat directions.
- The quadratic coefficients are residual-scale (|beta| <= 5.5e-11, bounded
  by the converged background residual ||Upsilon0|| <= 1.8e-9 via
  Cauchy-Schwarz); the nominal "saddle" directions have maximum depth
  4.5e-19 - an artifact of the not-exactly-zero background residual, not
  action-scale structure. Physically the lift is quartic.
- **The Phase399 flat-direction obstruction is fully relaxable at higher
  order**: the fermionic source kernel component lies entirely (fraction
  1.000000) in lifted directions, for every shell mode in both backgrounds.
  The coupled critical point therefore exists as a genuine critical point of
  the full toy bosonic action, not merely modulo flat directions.
- Cross-check: the source kernel norm-fractions 0.3475 / 0.3655 square to
  0.1208 / 0.1336, matching the Phase394 energy-fractions 0.121 / 0.134.
- The kernel is NOT gauge-dominated: gauge fractions <= 0.46 per direction
  (gauge-orbit rank 69); the flat directions are physical-moduli-like at
  quadratic level and all lifted at quartic order.

## Status

Fail-closed. The probed objective is the toy control-branch Mode-B
objective (quartic in omega), not a physical GU bosonic action; no physical
coupling; nothing promoted; zero contract fields. The probe gates only on
the verified measurement battery - the lift verdict is data, not a gate.

## Reproduce

```bash
dotnet run --project studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj
dotnet run --project studies/phase400_full_bosonic_action_flat_direction_lift_probe_001/Phase400FullBosonicActionFlatDirectionLiftProbe.csproj
```
