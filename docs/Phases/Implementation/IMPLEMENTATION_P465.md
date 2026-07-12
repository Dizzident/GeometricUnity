# Implementation P465 - Anomaly Consistency Variety Kernel (Team C, Wave-2) (Team C)

Phase465 is the **C-KERNEL v3** implementation of `docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`,
item 4. Registry number 465 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does

It re-executes the committed Phase404 construction to derive the signed slot
space (the six chirality-projected blocks of the 16 with their exact
hypercharge slots and re-derived weak/color multiplicities), then treats the six
hypercharges as **free exact-rational variables** and imposes the standard
chiral anomaly-consistency conditions as an exact-rational polynomial system:

- three **linear** forms: `[SU(2)]^2 U(1)`, `[SU(3)]^2 U(1)`, and the mixed
  gravitational-gauge `[grav]^2 U(1)` form;
- one **cubic** form: `[U(1)]^3`.

All coefficients are derived from each block's quantum numbers (weak doublet /
color triplet multiplicities and the block dimension), not hardcoded. The
solution **variety** is classified by exact linear algebra (reduced row echelon
over Q -> rank and nullspace) plus the projective hypersurface-dimension
theorem, and positive-dimensionality is cross-checked by an explicit
basis-independent 2-parameter vector-like family that lies wholly on the
variety.

## Committed pins and tripwire

- **Phase404 pin (drift => blocking).** The committed Phase404
  `targetBlindConstructionHash`
  `ed68a8f3e0d98f25f3c476e940103a4275a79f550346338d08c3c6319160a481` is read from
  the committed summary and asserted; the six blocks (dimensions 6/2/1/3/1/3) are
  reconstructed and matched. Any mismatch emits
  `anomaly-variety-phase404-hash-drift`.
- **SM-pattern tripwire committed BEFORE the census.** The Phase404
  signed-slot hypercharge signature (`-1/6, 1/2, -1, -1/3, 0, 2/3` in the
  `|Y|=1/2` normalization) is recorded as an exact-rational const before any
  variety computation runs. It fires as a **flag** (never a promotion) iff the SM
  pattern lands on the variety.

## Actual result

- `linearRank = 3`, `nullity = 3`; the `[U(1)]^3` cubic is **not** identically
  zero on the nullspace.
- **Classification: positive-dimensional** (projective dimension 1, affine
  dimension 2; a scale-invariant cone). The explicit vector-like plane
  `y = (0,0,-c,d,c,-d)` lies wholly on the variety for all rational `c, d`,
  certifying affine dimension >= 2 independently of any basis choice.
- **Terminal: `anomaly-variety-positive-dimensional-route-closed`.** Anomaly
  freedom does NOT isolate the SM hypercharge assignment (neither normalization
  nor even direction is selected), so this route **closes as a rule-out**.
- **SM-pattern tripwire fired** (the SM signature is anomaly-consistent, a
  membership flag) but `smPatternIsIsolatedSelection = false` - it is one ray
  among an infinite anomaly-free cone. The discriminator (a synthetic anomalous
  all-ones pattern) does **not** fire.
- Batteries: Phase404 reconstruction match, exact-arithmetic determinism, and
  the tripwire battery all pass.
- `kernelConstructionHash = 51ee4558001a50c42af7c003d92d2c4b4c83b8608a019b215816240e13ee8915`.

## O7 cap (labeled theoretical import; can never flip)

Anomaly freedom is a **labeled theoretical-consistency import**: the GU
fermionic action is recorded UNDEFINED, so phase326's cap
`anomalyRouteProvidesLowEnergyHyperchargeSource = false` can never flip. Even had
the variety been an isolated point, the row would have been **conditional only**
(direction, not normalization) with the phase451 two-loop tension standing.

## Framing

No physics is promoted; nothing is measured or normalized;
`promotedPhysicalMassClaimCount` remains 0; `physicistReviewPending` is carried
explicitly. Lattice-unit quantities stay in lattice units.
