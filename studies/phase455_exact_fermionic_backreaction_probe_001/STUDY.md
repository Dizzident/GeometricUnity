# Phase455: Exact Fermionic Backreaction Probe

Team B, plan item 1 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`). The full
pre-registered T0-T3 design is now IMPLEMENTED (superseding the STEP 0 skeleton).
Everything below the "Pre-registered design" heading was fixed before any number
was computed; the "Result" section records what the exact computation returned.

## Pre-registered design (implemented verbatim)

Exact backreaction `V(t) = S_B(t*u) + N_f*V_f(t)` along constant rank-1 rays
`omega = t*u`, on the COMMITTED phase428 closed-form ray spectrum
`lambda_i^2(t) = (s1 + t*u_c)^2 + (s2 + t*u_c)^2` (multiplicity 4 per
momentum/gauge-eigenvalue), pinned by battery g0.

- **Exact arithmetic** over `Q(sqrt 3)`: a small `Rational` (BigInteger) field, a
  `Q3 = a + b*sqrt3` field, and dense `Poly` arithmetic. The linear-in-t cross
  term is real; per-mode evenness is FALSE and only holds SET-WISE over the
  +/-k-summed `V_f` (battery g3, never assumed).
- **Committed conventions** (read from the committed record, cited, realized
  exactly, `physicistReviewPending`):
  - `S_B(t*u)` := the committed phase430 bosonic one-loop determinant
    `W_B(t) = sum log(eps_k^2 + t^2 m^2)` (phase430 Program.cs 230-244). A
    recorded workbench model; identifying it as `S_B` is a pre-registered
    modeling choice.
  - `V_f(t)` := the committed one-loop fermionic functional
    `W_F(t) = -(1/2) sum log lambda^2` (phase428 line 334, phase430 line 225);
    sign negative.
  - Per-mode coefficients after the committed prefactors: bosonic `+1` per
    (momentum, nonzero adjoint `m^2`); fermionic `-2*N_f` per (momentum, nonzero
    gauge eigenvalue).
- **Zero-mode convention axis** (each pre-registered, `physicistReviewPending`):
  Z-a (symmetric `k=0` exclusion: drop `eps^2 = s1^2 + s2^2 = 0` momenta) PRIMARY;
  Z-b (keep `k=0`, phase447 soft floor `floorRel` 1e-4 swept {1e-5,1e-4,1e-3});
  Z-c (keep `k=0`, exclude only exact zero modes, `lambda^2 <= 1e-18`). The three
  give different stationarity polynomials (the `k=0` modes add t-dependent
  factors).
- **Stationarity**: `dV/dt` denominator-cleared to ONE polynomial with exact
  `Q(sqrt3)` coefficients; Sturm-certified root isolation; the depth sign at each
  isolated root is read from the certified sign flip of `dV/dt` (min = `- -> +`).
- **`t_max`**: a pre-registered rational strictly below the first exact fermionic
  zero-crossing (the `V_f` log divergence) per (rep, axis).
- **g1 perturbativity**: `|N_f*V_f| <= 2*S_B` pinned at `C=2` on `(0, t*]`, with a
  mandatory marginality label when within 10% at `t*`. Gates over the DERIVED
  verdict rows only.
- **Derived verdict content**: the Phase404 blind single family decomposes into 4
  su(3)-active fundamentals (the 4 color singlets decouple) => `N_f = 4`
  fundamental (the ONLY strictly-derived count). Phase433's `n_f=3` (`N_f=12`) is
  an IMPORTED structural parameter, recorded NON-GATING. The occupancy sweep
  (`N_f=1..16`, adjoint) is robustness-only. The critical-coupling column appears
  in NO terminal conjunct.
- **Batteries g0-g6**: g0 committed-spectra hash pin; g1 perturbativity; g2
  exact-vs-float; g3 set-wise evenness; g4 synthetic positive control (a spectrum
  that DOES produce a below-origin well, proving T2 detectable); g5 Sturm battery
  on known polynomials; g6 convention-flip matrix. Any battery red => T0.

## Pre-registered verdict taxonomy (T0-T3)

- **T0** `batteries-failed-no-verdict` — any battery red.
- **T1** `fermionic-backreaction-null` — no derived-content row has a stationary
  point of certified NEGATIVE DEPTH (a local min with `V(t*) < V(0) = 0`) on
  `(0, t_max]`. Closes limb L5; reopening = a source-defined fermionic action.
- **T2** `radiative-well-candidate` — a derived row has a certified below-origin
  well; `t*` forwarded to the phase456 pack (anchor-free ratios only, never a
  mass claim).
- **T3** `convention-fragile` — the below-origin-well verdict flips across
  Z-a/Z-b/Z-c; the flip axis routes to the O4 queue.

## Result (exact computation)

**Terminal: T3 `convention-fragile`.** All batteries g0-g6 are green.

- Z-a PRIMARY (k=0 excluded), derived `N_f=4`: perturbatively controlled
  (g1 max ratio ~0.23) and NO below-origin well on any axis (the only interior
  minima sit ABOVE the origin: T/D at `t~1.619`, `V~+1.92`; S at `t~1.418`,
  `V~+9.15`). The primary arm alone reads T1.
- Z-b / Z-c (k=0 included): the T/D axes develop a genuine below-origin well
  (`t~1.758`, `V~-18.4`); the S axis does not. The verdict therefore FLIPS with
  the zero-momentum-mode convention => T3, routing that convention to O4.
- Imported 3-family (`N_f=12`, NON-GATING): non-perturbative (g1 ratio ~1.44) with
  below-origin wells on all axes; outside the derived content and the controlled
  regime, so it decides nothing.

L5 is NOT closed by this phase (only T1 closes it).

## Consumers

t* -> phase456 pack freeze (only on T2; none forwarded here); phase458 gates
G3/G5; the phase471 ledger limb L5.

## Framing

Every `t` and every `V, S_B, V_f` is a workbench-relative structure of the
reduced su(3) slice in lattice units — never a physical mass; no GeV/pole/VEV
promotion; no contract field filled; `promotedPhysicalMassClaimCount = 0`;
`physicistReviewPending = true`.
