# Phase471: B2 Closure Theorem-of-Record Ledger

Team B, plan item 14 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`). Unlike the
other STEP 0 items, this phase is **implemented for real** at STEP 0: it is the
eight-limb closure ledger. It reads committed limb outcomes and emits a
machine-attested program-state record, upgrading mechanically as limbs land.

## The eight limbs

| Limb | Phases | Status now |
|------|--------|-----------|
| L1 | phase443 | CLOSED (Wave 1) |
| L2 | phase446, phase447 | CLOSED (Wave 1) |
| L3 | phase449, phase450 | CLOSED (Wave 1) |
| L4 | phase453, phase454 | CLOSED (Wave 1) |
| L5 | phase455 | OPEN (skeleton) |
| L6 | phase456 | OPEN (skeleton) |
| L7 | phase457, phase466 | WITHHELD (hold) |
| L8 | phase458 | OPEN (skeleton) |

L1–L4 are read from their committed summary outputs and marked CLOSED with their
committed terminal strings recorded for provenance. L5/L6/L8 read the skeleton
outputs (OPEN); L7 reads the portal/schema skeletons (WITHHELD).

## Current verdict

`closure-not-decidable`, naming the open limbs (L5, L6, L8) and the withheld
limb (L7). This emission is itself a citable machine-attested program-state
record.

## The closure decision (STATUS-keyed, never string-keyed)

- any CLOSED limb with a positive outcome ⇒ `closed-with-positive-inclusion`;
- else any non-withheld not-closed limb ⇒ `closure-not-decidable`;
- else any withheld limb (all others closed-negative) ⇒
  `closed-negative-except-portal-unprobed` (explicit carve-out);
- else (all closed, all clean-negative, none withheld) ⇒ `closed-negative`.

## Structural-incapability battery (proved by construction, in-phase)

The strong negative `closed-negative` is UNREACHABLE while any limb is
open/held/withheld. Five synthetic scenarios prove it:

1. the real limb set ⇒ `closure-not-decidable`;
2. a **forged near-miss** — open/withheld limbs relabelled with
   negative-looking terminal strings but left not-closed — STILL ⇒
   `closure-not-decidable` (proves the decision keys on STATUS, not on the
   content of a terminal string);
3. all-closed, all clean-negative, none withheld ⇒ `closed-negative` (the ONLY
   route to the strong negative);
4. all-closed-negative except one withheld ⇒
   `closed-negative-except-portal-unprobed` (carve-out only, never the bare
   negative);
5. one positive inclusion ⇒ `closed-with-positive-inclusion`.

`verdict-withheld` may never feed the strong negative.

## Fail-closed reads

Any unreadable committed input demotes its limb to `unreadable` (which counts as
not-closed), so a missing artifact can never silently close a limb.

## Upgrade paths (recorded)

Fastest full-negative pattern E1: phase455-T1 + phase456-T1-with-Gaussian-null-
Binder + phase457 post-unlock nonexistence/null (AND/OR phase458
NO-GO-theorem-closed). Any committed T2 anywhere flips to
`closed-with-positive-inclusion` — the strongest legal positive claim in the
program.

## Framing

Zero physics computation: this ledger reads committed verdicts and emits a
program-state record. Nothing is measured, filled, or promoted;
`physicistReviewPending = true`; `promotedPhysicalMassClaimCount = 0`.
