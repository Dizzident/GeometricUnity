# Phase456: Consolidated n=4 Launch (pack-gated)

Team B + A, plan item 9 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`). The
A4 + A5 Stage-A **pre-registration pack** is built and hash-pinned in
`preregistration/`; this phase is the pack **refuse-to-run gate**, the
deterministic production-result consumer, and the standing claim boundary. The
production HMC is implemented as an explicitly selected, environment-clean
mode of the proven phase452 sampler; the ordinary generator path never launches
the long run. Design details and the pack contents are in
`docs/Phases/Implementation/IMPLEMENTATION_P456.md`.

## Terminal taxonomy

- `awaiting-pack` — the pack is absent (interim green).
- `pack-committed-awaiting-gates` — the pack is present and its SHA-256 matches
  the pinned constant; authorization is valid and the separately tracked
  production artifact has not landed yet.
- `pack-hash-mismatch-refuse-to-run` — **BLOCKED**; the MANDATORY refuse-to-run
  condition. Production must never sample against an unpinned/altered pack.
- `production-analysis-invalid` — artifact provenance and mandatory storage can
  be valid while one or more pre-registered rows is non-finite/invalid; the
  entire five-row interpretation then fails closed.
- `production-controls-failed`, `production-underpowered`,
  `t1-quasi-free-compatible-at-probed-volume`,
  `t2-coherent-distinct-irrep-departure-n5-mandatory`, and
  `mixed-row-outcome-no-coherent-distinct-irrep-departure` are the remaining
  closed production terminals.

Before the production artifact lands, zero physics compute; afterward, the
phase consumes workbench-relative lattice measurements only. In every state,
`physicistReviewPending = true` and `promotedPhysicalMassClaimCount = 0`.

## Mandatory gates (pre-registered, non-negotiable)

- **Hash-refuse-to-run gate (MANDATORY).** The pack is hash-pinned in phase456's
  pre-registration directory; the phase refuses to run if the committed pack
  hash does not match. No sampling starts against an unpinned pack.
- **Per-site (un-slice-summed) correlator storage flag (MANDATORY).** The
  dispersion arm at `k_min = 2π/4` requires per-site correlators; the storage
  flag is mandatory and its absence is a refuse-to-run condition.

## Implemented production surface

The A4 pack homes in this phase's pre-registration directory (never phase455):
Stage-A automorphism/irrep enumeration; exact-rational projector kernels;
identity-irrep 2×2 GEVP; `k_min = 2π/4` dispersion; exact free controls;
committed threshold `max(family-wise ≥3σ, per-row 3σ)` (bare 99th percentile
forbidden); a power gate; mechanical AIC window aggregation (no analyst-chosen
window exists). MV-456 fallback is a pack-scoping device, NOT an A4-gate bypass.

The measurement output stores all `4^3 = 64` spatial-momentum correlators for
every Euclidean-time separation with aligned jackknife replicates. This complete
discrete transform is invertible to the per-base-site spatial correlator. An
independent `k_min` projection must reconstruct from the table within `1e-10`.
The A2-like row applies the committed exact face-type kernel before any
slice-only loss. The result consumer calibrates the family-wise threshold from
the aligned null jackknife correlation with a fixed RNG seed and executes the
five-row closed taxonomy fail-closed. All five null rows come from independent
direct samples of the exact block-Gaussian measure passed through the identical
measurement pipeline; the large-beta HMC columns are retained as a distinct
sampler-control gate.

## Hard gates (before production sampling)

phase455 terminal recorded; A4 Stage-A kernels committed; A5 Stage-A verdict
before pack freeze; O4 memo (or explicit user-renewed risk acceptance) before
production sampling — the largest at-risk spend under O4.

## Consumers / limb

T1 would close limb L6 at probed-volume scope with a Gaussian-null Binder
column. The actual production terminal is `production-analysis-invalid`, so L6
and L8 remain open. A coherent ≥3σ departure in ≥2 distinct-irrep channels
would mandate n=5 before ANY claim; no such escalation fires from an invalid
analysis.

## Production result (2026-07-15)

The explicit user risk renewal authorized one environment-clean run under the
committed defaults. It completed in `21846.5987934 s`; the two byte-identical
raw/summary artifacts have SHA-256
`9b7e965a0b8ac906bc1352f908b28b6eb22511579c02ee91562f63beb67ed9cb`.
Pack hash, authorization, committed defaults, complete 64-momentum storage,
per-face-type retention, and the exact independent-Gaussian control shape all
verify. Every correlator family carries 50 aligned jackknife replicates.

The scientific/control result is red. The large-beta free-field gate fails;
SD2 `beta=1` has generic `N_eff=7.1549` and row-specific
`N_eff(k_min)=16.623`; A2 and dispersion mass rows are non-finite under the
committed estimator/control comparison. The any-invalid-row firewall therefore
withholds the family-wise threshold and emits `production-analysis-invalid`.
No G3 motivation, n=5 escalation, spectrum characterization, ledger closure,
or physical-mass claim follows. The measured cost, `0.1137102 CPU-weeks`, is a
valid Phase458/G2 input only.

## Framing

Production quantities remain workbench-relative lattice data. Lattice-unit
quantities stay in lattice units; no GeV/pole/VEV promotion;
`promotedPhysicalMassClaimCount = 0`.
