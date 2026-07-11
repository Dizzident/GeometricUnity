# Implementation P461 - Dimensional-Transmutation Reading Menu (Team A, Wave-1, A2)

Phase461 executes Team A rank-3 of the 2026-07-10 three-team elimination
program (`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md`, Wave-1
item 3), including the unresolved-objection O6 reconstruction gate.
Registry number 461 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase computes

Exact rational arithmetic on every decision, sub-second. The finite menu of
power readings `M(a,b) = c * (gravityAnchor^a * ccScale^b)^(1/(a+b))` of the
snapshot's coefficient-free CC-as-a-VEV slot statement (routed here by
phase460) is tested against a consistency-window list pre-registered
COMPLETE with per-window imports-observed flags BEFORE any value is
computed:

- 17 readings (9 exponent pairs x {full, reduced} gravity-anchor
  conventions; the degenerate (0,b) reading registered once);
- 7 live test-only observed-comparison windows + the BANNED 246-GeV
  electroweak VEV window (circular and gauge-variant per the recorded
  task-force ruling; role=referee-reconstruction-only; contributes nothing
  to live verdicts) => 119 live rows;
- look-elsewhere control: primary band c in [1/(4*pi), 4*pi] (fixed
  rational endpoints), secondary wide band demoted to a labeled sweep,
  p1 = 2*ln(missFactor)/ln(anchorRange) under the log-uniform null,
  Bonferroni-corrected over 119 rows, survival threshold 0.05
  (pre-registered);
- THE RECONSTRUCTION GATE (O6): fail-close as menu-incomplete (NO verdict)
  unless >= 1 row against the referee-reconstruction window reproduces the
  task-force referee's ~460x kill within the pre-registered band [300, 700]
  (conventions: linear, cc-plane-squared).

Band membership and kill factors are decided by BigInteger rational
cross-multiplication of k-th powers (no floating point in any decision);
doubles are reporting-layer only. Batteries: exact-rational battery,
menu-completeness battery, banned-import battery, routed-slot-statement
evidence.

## Committed verdict

`dimensional-transmutation-reading-menu-declared-comparison-consistency-only`
with `dimensionalTransmutationReadingMenuPassed = true`. Key numbers:

- RECONSTRUCTION GATE GREEN, exactly one candidate: reading `M(1,1)-full`
  against the banned VEV window under the `cc-plane-squared` convention,
  kill factor **451.105** inside [300, 700] - the referee's quoted
  "misses by ~460x, referee-confirmed" kill, reconstructed exactly
  (ccScale x gravityAnchorFull / vev^2) and committed as the reconstructed
  referee reading.
- Live rows: 4 primary-band hits, ALL import-laden - M(1,2)-full/reduced vs
  qcdScaleNf5 (miss 5.33 / 9.12), M(1,3)-full/reduced vs massElectron
  (miss 4.72 / 7.07); none survives the trials correction (minimum
  Bonferroni-corrected p = 1.0 over 119 rows); 11 rows land only in the
  labeled secondary band; `importCleanTrialsSurvivingHitCount = 0`.

## Elimination meaning

The part-12d anchor readings supply NO import-clean, trials-surviving
dimensionful anchor: the cc/dark-energy anchor remains killed for anchor
purposes and the relation survives only as declared-comparison numerology.
A6 consumes this as the A2 rule-out of anchor-grade content (second
conjunct; not all-miss, so the reading menu itself is honestly recorded as
consistency-compatible with two import-laden windows under look-elsewhere
demotion).

## Framing

`targetBlindConstruction = false` - this is a DECLARED-COMPARISON phase
(phase429/451-style separation): every GeV number is a labeled test-only
import, never a prediction. `physicistReviewPending = true`; no
Phase201/Phase256 contract filled; nothing promoted;
`promotedPhysicalMassClaimCount = 0`.

## Wiring

See `docs/Phases/Implementation/WIRING_P461.md` for the exact snippets for
every main-checkout validation surface.
