# Phase463: Transport Structure Theorems (Stage-A ladder, T4-first)

Team A, plan item 3 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`). Exact /
structural elimination theorems over the committed phase454 block spectra;
**zero HMC in Stage A**. Deterministic (no RNG), target-blind, fail-closed.

## Committed input

The phase454 `beyond-ray-quadratic-certificate-probe` output: the joint
`(omega, theta)` Hessian block spectra of the su(2) reduced Spin(4) slice
(n=3 lattice torus). Load-bearing committed facts: the **trivial (stationary
vacuum) background is all-nonnegative** on the gauge complement for all 3
members (0 negative directions); the **2016 named negatives** live only at the
NON-STATIONARY ray backgrounds; and the on-ray quadratic coefficient
`armB minA2 > 0` exactly.

## Dim-2 transport operator menu (committed ex ante)

Six audited dim-2 operators derived from the phase460 pinned-row corpus
operators that are dim-2 mass terms (explicit vector mass, condensate mass,
two-form topological mass, inverse-radius mode mass) plus the standard
tr F^2-conjugate dim-2 candidates on this workbench (the quadratic curvature
form itself, and a non-tachyonic conformal coupling). Each carries a committed
quadratic-form sign class. A seventh SYNTHETIC CONTROL (wrong-sign dim-2 form,
NOT audited) exercises the overturn battery. The menu is hash-pinned.

## Theorems

- **T4 - conformal-sector likely-kill (FIRST).** A dim-2 operator is degree-2
  homogeneous along the conformal (uniform dilation) direction, the same order
  as the leading `a2`. It injects an unbounded conformal-factor direction only
  via a negative-definite quadratic form in the k=0 conformal sector overcoming
  the committed nonnegative margin. Since (i) the stationary vacuum Hessian is
  PSD on the complement and (ii) `armB minA2 > 0` exactly, every PSD audited
  operator added to the committed form stays PSD and does NOT inject; only the
  synthetic wrong-sign control injects. Exact per-operator verdict.
- **T1 - abelian-cone spectral-instability witnesses.** The rank-one abelian
  cone (su(2) Cartan direction; exact BigInteger rank 1) is provably nonempty.
  It carries 0 witnesses at the stationary vacuum; the 2016 named negatives are
  NON-STATIONARY transverse witnesses (S_B >= 0 pins the global minimum at 0),
  not vacuum instabilities.
- **T2 - curve-escape repair.** The Motzkin-type counterexample
  `f = (y - x^2)(y - 2 x^2)` is bounded on every ray through the origin yet
  unbounded below along `y = (3/2) x^2` (both verified EXACTLY in BigInteger).
  Bounded-on-every-ray does NOT imply bounded below, so ray/cone analysis
  authorizes no boundedness claim; the survive terminal is renamed
  `no-ray-instability-found-at-audited-menu` and AUTHORIZES NOTHING.
- **T3 - coercivity certificate obligation** (the ONLY Stage-B HMC gate). A
  global lower bound / off-ray SOS certificate for the transport-augmented
  potential. `S_B >= 0` certifies the pure gauge action, but the augmented
  off-ray coercivity is NOT certifiable in Stage A (the T2 obstruction). No
  certificate is obtained.

## Terminals (pre-registered)

`transport-killed-at-audited-menu`, `transport-coercivity-certified`,
`no-ray-instability-found-at-audited-menu`, `no-certificate-obtainable`, plus
`review-required-inputs-missing` / `review-required-battery-failed`.

## Committed result

`transport-structure-theorems-transport-killed-at-audited-menu`
(`transportStructureTheoremsPassed = true`). T4 kills the conformal route for
all 6 audited operators (synthetic control injects - the overturn battery has
teeth); T1 finds no stationary-vacuum instability; T2 verified exactly; T3 no
certificate. This is the strongest form of the phase464 conjunct C3, cancelling
item-16 Stage-B at the audited menu with **two named reopening conditions**
(`operator-outside-audited-menu`; `coercivity-proof-for-negative-c2-window`) -
NEVER "cancelled permanently".

Batteries (all green): exactness cross-checks vs the phase454 spectra,
menu-completeness hash, determinism, synthetic-overturn, abelian-cone
nonemptiness, Motzkin exactness. Lattice-unit workbench-relative structure data
only; `promotedPhysicalMassClaimCount = 0`; `physicistReviewPending = true`.

## Env knob

`PHASE463_OUTPUT_DIR`.
