# Implementation P456 - Consolidated n=4 Launch (Team B+A, Wave-2) (Team B+A)

Phase456 is the **STEP 0 skeleton** built in the one batched wave wiring
checkpoint (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, item 9). Registry
number 456 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does at STEP 0

Nothing physical. It emits the pre-registered **interim terminal
`awaiting-pack`** and the standing claim boundary (targetBlindConstruction = true,
no contract fills, no promotions, physicistReviewPending = true, planSection
recorded). It runs in well under a second and is wired into every validation
surface so each pass between this checkpoint and the real implementation
validates green with no silent-promotion path.

The real phase carries the hash-pinned consolidated pack with the MANDATORY hash-refuse-to-run gate and the MANDATORY per-site (un-slice-summed) correlator storage flag for the dispersion arm. It closes ledger limb L6 at probed-volume scope on T1 with a Gaussian-null Binder column.

## Pre-registration

The full pre-registered design that the real phase will implement is recorded
in `studies/phase456_consolidated_n4_launch_001/STUDY.md`. That design is fixed before any number is
computed.

## Framing

Zero physics computation at STEP 0; nothing is measured, filled, or promoted;
promotedPhysicalMassClaimCount remains 0; physicistReviewPending is carried
explicitly. Lattice-unit quantities stay in lattice units and are never
relabelled.
