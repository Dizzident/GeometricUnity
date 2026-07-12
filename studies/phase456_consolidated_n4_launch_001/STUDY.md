# Phase456: Consolidated n=4 Launch (STEP 0 skeleton)

Team B + A, plan item 9 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`). STEP 0
**skeleton**: emits the pre-registered interim terminal `awaiting-pack` and the
standing claim boundary. The consolidated pack and its sampling are not yet
built. This file pre-registers the design.

## Interim terminal

`awaiting-pack` — skeleton only; zero physics compute; nothing measured or
promoted; `physicistReviewPending = true`.

## Mandatory gates (pre-registered, non-negotiable)

- **Hash-refuse-to-run gate (MANDATORY).** The pack is hash-pinned in phase456's
  pre-registration directory; the phase refuses to run if the committed pack
  hash does not match. No sampling starts against an unpinned pack.
- **Per-site (un-slice-summed) correlator storage flag (MANDATORY).** The
  dispersion arm at `k_min = 2π/4` requires per-site correlators; the storage
  flag is mandatory and its absence is a refuse-to-run condition.

## Pre-registered pack contents (to be implemented)

The A4 pack homes in this phase's pre-registration directory (never phase455):
Stage-A automorphism/irrep enumeration; exact-rational projector kernels;
identity-irrep 2×2 GEVP; `k_min = 2π/4` dispersion; exact free controls;
committed threshold `max(family-wise ≥3σ, per-row 3σ)` (bare 99th percentile
forbidden); a power gate; mechanical AIC window aggregation (no analyst-chosen
window exists). MV-456 fallback is a pack-scoping device, NOT an A4-gate bypass.

## Hard gates (before production sampling)

phase455 terminal recorded; A4 Stage-A kernels committed; A5 Stage-A verdict
before pack freeze; O4 memo (or explicit user-renewed risk acceptance) before
production sampling — the largest at-risk spend under O4.

## Consumers / limb

Closes limb L6 at probed-volume scope on T1 with a Gaussian-null Binder column;
a coherent ≥3σ departure in ≥2 distinct-irrep channels is a mandatory n=5
escalation (contingent consumer of registry block B:471–476) before ANY claim.

## Framing

Zero physics computation at STEP 0. Lattice-unit quantities stay in lattice
units; no GeV/pole/VEV promotion; `promotedPhysicalMassClaimCount = 0`.
