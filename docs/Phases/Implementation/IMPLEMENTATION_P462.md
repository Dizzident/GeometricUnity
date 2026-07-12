# Implementation P462 - Blocking Set Resolution (Team A, Wave-2) (Team A)

Phase462 is the **STEP 0 skeleton** built in the one batched wave wiring
checkpoint (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, item 2). Registry
number 462 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does at STEP 0

Nothing physical. It emits the pre-registered **interim terminal
`awaiting-adjudication`** and the standing claim boundary (targetBlindConstruction = true,
no contract fills, no promotions, physicistReviewPending = true, planSection
recorded). It runs in well under a second and is wired into every validation
surface so each pass between this checkpoint and the real implementation
validates green with no silent-promotion path.

The real phase runs the staged resolution (Stage P quote-pinning, Stage 0 exact SNF certificates, Stage 1 conjunction-gated excision, Stage 2 physicist adjudication with >=40 paraphrase decoys, sealed redo, second signer). Audit-authored strings are labelled AUDIT-AUTHORED-NOT-CORPUS and never gradable; an automated extractor/NLP route is deliberately discarded. Interim k = 31 pending; delivers phase464 conjunct C1.

## Pre-registration

The full pre-registered design that the real phase will implement is recorded
in `studies/phase462_blocking_set_resolution_001/STUDY.md`. That design is fixed before any number is
computed.

## Framing

Zero physics computation at STEP 0; nothing is measured, filled, or promoted;
promotedPhysicalMassClaimCount remains 0; physicistReviewPending is carried
explicitly. Lattice-unit quantities stay in lattice units and are never
relabelled.
