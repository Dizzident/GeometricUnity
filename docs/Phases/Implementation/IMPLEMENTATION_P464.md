# Implementation P464 - Anchor Adjudication Contract (Team A, Wave-2) (Team A)

Phase464 is the **STEP 0 skeleton** built in the one batched wave wiring
checkpoint (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, item 12). Registry
number 464 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does at STEP 0

Nothing physical. It emits the pre-registered **interim terminal
`awaiting-upstream`** and the standing claim boundary (targetBlindConstruction = true,
no contract fills, no promotions, physicistReviewPending = true, planSection
recorded). It runs in well under a second and is wired into every validation
surface so each pass between this checkpoint and the real implementation
validates green with no silent-promotion path.

The real phase adjudicates LAST over the exact conjunction C1 AND C2 AND C3, with the tripwire denominator pinned ex ante = 31 and pre-committed public sentences for both outcomes. Terminal (i) can NEVER emit with zero physicist rulings. This phase EXHAUSTS the A block.

## Pre-registration

The full pre-registered design that the real phase will implement is recorded
in `studies/phase464_anchor_adjudication_contract_001/STUDY.md`. That design is fixed before any number is
computed.

## Framing

Zero physics computation at STEP 0; nothing is measured, filled, or promoted;
promotedPhysicalMassClaimCount remains 0; physicistReviewPending is carried
explicitly. Lattice-unit quantities stay in lattice units and are never
relabelled.
