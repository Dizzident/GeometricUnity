# Phase465: Anomaly Consistency Variety Kernel (C-KERNEL v3)

Team C, plan item 4 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`; C-KERNEL).
Implemented. See `docs/Phases/Implementation/IMPLEMENTATION_P465.md` for the full
record.

## Construction

Re-executes the committed Phase404 construction to derive the signed slot space
(six chirality-projected blocks of the 16, with exact hypercharge slots and
re-derived weak/color multiplicities), then treats the six hypercharges as free
exact-rational variables and imposes the standard chiral anomaly set:

- linear forms `[SU(2)]^2 U(1)`, `[SU(3)]^2 U(1)`, `[grav]^2 U(1)`;
- cubic form `[U(1)]^3`.

Coefficients are derived from block quantum numbers. The variety is classified by
exact reduced row echelon over Q (rank + nullspace) plus the projective
hypersurface-dimension theorem, cross-checked by an explicit basis-independent
vector-like family lying wholly on the variety.

## Committed rules

- **Phase404 hash pin** `ed68a8f3...` (drift => `anomaly-variety-phase404-hash-drift`).
- **SM-pattern tripwire committed BEFORE the census**: the Phase404 signed-slot
  hypercharge signature is a const; it fires as a flag (never a promotion) iff
  the SM pattern lands on the variety.
- **O7 labeled-import cap**: anomaly freedom is a labeled theoretical-consistency
  import; the fermionic action is UNDEFINED, so
  `anomalyRouteProvidesLowEnergyHyperchargeSource = false` can never flip. Even an
  isolated point would be conditional only (direction, not normalization; the
  phase451 tension stands).

## Terminals

`anomaly-variety-isolated-point-conditional` (one direction-only conditional row)
/ `anomaly-variety-positive-dimensional-route-closed` / `anomaly-variety-empty-consistency-alarm`,
plus blocking `anomaly-variety-phase404-hash-drift`,
`anomaly-variety-exact-arithmetic-nonconvergent`, `anomaly-variety-battery-failed`.

## Actual result

`linearRank = 3`, `nullity = 3`, cubic not identically zero on the nullspace ->
**positive-dimensional** (projective dim 1, affine dim 2; scale-invariant cone).
Terminal **`anomaly-variety-positive-dimensional-route-closed`**: anomaly freedom
does not isolate the SM hypercharge, so the route closes as a rule-out. The SM
tripwire fired as a membership flag but is not an isolated selection. All three
batteries pass. `kernelConstructionHash = 51ee4558...`.

## Framing

Nothing promoted; `promotedPhysicalMassClaimCount = 0`; lattice-unit quantities
stay in lattice units.
