# Implementation P464 - Anchor Adjudication Contract (Team A, Wave-2)

Phase464 is the **A6 adjudication contract** (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`,
item 12). It adjudicates LAST and **EXHAUSTS the A block**. Registry number 464
per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does

It mechanically evaluates the exact conjunction **C1 AND C2 AND C3** over the
three committed Team-A inputs plus the B1 rulings ledger, fail-closed. Zero
physics is computed; it reads committed upstream verdicts and adjudicates.

- **C1 (blocking-set closure)** <- phase462. Satisfied ONLY by
  `closure-resolved(-with-excision)`. phase462 is `pinning-insufficient`, so
  **C1 = BLOCKED**.
- **C2 (reading-menu disposal)** <- phase461. Satisfied by
  `declared-comparison-consistency-only` / `part12d-anchor-readings-all-miss`
  (no import-clean anchor survives). phase461 is
  `declared-comparison-consistency-only`, so **C2 = satisfied**.
- **C3 (transport)** <- phase463. Satisfied by
  `transport-killed-at-audited-menu` OR, per the labeled item-20 amendment
  (`WAVE2_AMENDMENTS_2026-07-12.md` A1), `vacuous-given-zero-sanctioned-anchors`.
  phase463 is `transport-killed-at-audited-menu`, so **C3 = satisfied**
  (mode = `transport-killed-at-audited-menu`).

## Terminals (pre-registered consts)

- **(i) `anchor-adjudication-convention-only`** — requires C1 AND C2 AND C3 AND
  **>= 1 physicist ruling** parsed from `docs/Phases/Adjudication/B1_RULINGS_LEDGER_P462.md`.
  Can NEVER emit with zero rulings (the ledger is parsed and ruling rows counted).
- **(ii) `anchor-adjudication-reopened-live-candidate`** — any input forwards a
  live anchor candidate (precedence over the blocked record).
- **(iii) `anchor-adjudication-blocked-upstream-ambiguous`** — C1 blocked; emits
  the plan §3 pre-committed sentence VERBATIM with `N_pending` substituted, plus
  the named reopening conditions. **The reachable terminal now.**
- **(iv) `anchor-adjudication-no-claim`** — partial state (all conjuncts
  satisfied but zero rulings, or a conjunct unmet); forbids citing partial
  closure.

Fail-closed guards additionally emit `review-required-inputs-missing` /
`review-required-battery-failed`.

## Result (real inputs)

**Terminal: `anchor-adjudication-blocked-upstream-ambiguous`.** C1 is blocked
(phase462 `pinning-insufficient`); C2 and C3 hold but a blocked C1 is decisive.
`N_pending = 31` of `31` (read from phase462 `pendingAfterStage1` /
`blockingSetCount`; tripwire denominator pinned ex ante = 31 and drift-checked).
Rulings on record = 0, so terminal (i) cannot emit on two independent grounds.

The emitted verbatim §3 sentence:

> "The audited corpus neither supplies nor forbids a dimensionful anchor at
> theorem grade: 31 of 31 prose-only statements remain unadjudicated under the
> committed 33-symbol grading; no reading-menu anchor survives (phase461); every
> dimensionful anchor in use is a labeled convention adoption pending
> adjudication; no anchor theorem is claimed and none may be cited."

## Machine-monitorable reopening fields

Read from the phase461 / phase463 outputs: `sanctionedAnchorPresent = false`
(phase461 forwarded zero anchors) and `coercivityCertified = false` (phase463
obtained no off-ray certificate). A flip of either from `false` reopens the
record. The four named reopening conditions are recorded in the output:
human-authored per-item draft locators; a later RELATION ruling; a corpus/menu
update; and the machine-monitorable field flip.

## In-phase input-flip battery (synthetic; asserted every pass)

A single pre-registered `Adjudicate(...)` function is the source of truth; the
battery calls it with synthetic inputs:

- **(a)** phase462 flipped to `closure-resolved`, zero rulings => terminal (i)
  must STILL NOT emit (emits `no-claim`). The rulings gate holds.
- **(b)** phase462 `closure-resolved` + one synthetic ruling => (i) emits
  (`convention-only`).
- **(c)** phase461 flipped to forward an anchor => (ii) emits
  (`reopened-live-candidate`).
- **(d)** real inputs => (iii) emits (`blocked-upstream-ambiguous`).

All four pass; any failure fails the phase closed
(`review-required-battery-failed`).

## Objections carried honestly (plan §4 objections 3-4)

- **Objection 3 (post-hoc admissibility):** the corpus-admissibility predicate
  (readings breaking the two pinned corpus relations p335-r2 / p339-r1 are
  inadmissible) was formulated after observing those breaks; defended as
  verdict-monotone (disposal only narrows scope); the terminal (i) closure
  sentence must record the post-hoc fact — carried, never eliminated.
- **Objection 4 (33-symbol scope):** any B1 closure theorem is scoped to the
  committed 33-symbol grading; a dimensionful quantity absent from the symbol
  table is invisible to it.

The terminal (i) closure contract skeleton records the five mandatory scope
fields (named disposed readings; post-hoc admissibility; 33-symbol scope; C3
mode; O4-discharge clause); it is emitted only when (i) is reached.

## Framing

Zero physics computation; nothing is measured, filled, or promoted;
`promotedPhysicalMassClaimCount` remains 0; `physicistReviewPending` is carried
explicitly. Lattice-unit quantities stay in lattice units and are never
relabelled. Runtime < 1 s.
