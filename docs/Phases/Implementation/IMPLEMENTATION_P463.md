# Implementation P463 - Transport Structure Theorems (Team A, Wave-2)

Phase463 executes Team A plan item 3 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`):
the Stage-A transport-structure ladder, T4-first. Registry number 463 per
`docs/Phases/PHASE_NUMBER_REGISTRY.md`. Exact / structural elimination theorems
over the committed phase454 block spectra; deterministic (no RNG), target-blind,
fail-closed; **zero HMC in Stage A**; runs in well under a second.

## What this phase computes

The committed phase454 sector data (joint `(omega, theta)` Hessian block spectra
of the su(2) reduced Spin(4) slice, n=3 torus) is read and gated on the
structural invariants the theorems depend on: the trivial (stationary vacuum)
background is all-nonnegative on the gauge complement; the 2016 named negatives
live only at the NON-STATIONARY ray backgrounds; `armB minA2 > 0` exactly.

A hash-pinned dim-2 transport operator menu (6 audited operators from the
phase460 pinned-row corpus operators plus the standard tr F^2-conjugate dim-2
candidates, and 1 synthetic wrong-sign control) drives the ladder:

- **T4 (first)** - conformal-sector kill: a dim-2 operator is degree-2
  homogeneous along the conformal (dilation) direction; a PSD operator added to
  the committed PSD stationary-vacuum form with `minA2 > 0` cannot inject an
  unbounded conformal direction. All 6 audited operators are killed; only the
  synthetic control injects (the overturn battery has teeth).
- **T1** - the rank-one abelian cone (Cartan direction; exact BigInteger rank 1)
  is provably nonempty; 0 stationary-vacuum witnesses; the named negatives are
  non-stationary transverse witnesses.
- **T2** - the Motzkin-type counterexample `f = (y - x^2)(y - 2 x^2)` is bounded
  on every ray yet unbounded below along `y = (3/2) x^2` (both verified exactly
  in BigInteger). Bounded-on-every-ray does NOT imply bounded below; the survive
  terminal is renamed `no-ray-instability-found-at-audited-menu` and authorizes
  nothing.
- **T3** - the coercivity certificate obligation (the only Stage-B HMC gate);
  no off-ray certificate is obtainable in Stage A.

Batteries: exactness cross-checks vs the phase454 spectra, menu-completeness
hash, determinism, synthetic-overturn, abelian-cone nonemptiness, Motzkin
exactness.

## Committed verdict

`transport-structure-theorems-transport-killed-at-audited-menu` with
`transportStructureTheoremsPassed = true`. This is the strongest form of the
phase464 conjunct C3, cancelling item-16 Stage-B at the audited menu with two
named reopening conditions (`operator-outside-audited-menu`;
`coercivity-proof-for-negative-c2-window`) - NEVER "cancelled permanently".

## Framing

All eigenvalues/coefficients are lattice-unit workbench-relative structure data;
no GeV; nothing measured, filled, or promoted; `promotedPhysicalMassClaimCount = 0`;
`physicistReviewPending = true`.
