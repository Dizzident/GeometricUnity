# Implementation P456 - Consolidated n=4 Launch (Team B+A, Wave-2)

Phase456 hosts the **A4 + A5 Stage-A pre-registration pack** and the pack
**refuse-to-run gate** (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, item 9;
standing items 12 & 14). Registry number 456 per
`docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does now

No physics compute and no sampling. The `~6-16 h` production HMC is
deliberately **not implemented yet**. The program is the pre-registration pack
**refuse-to-run gate** plus the standing claim boundary. Terminal taxonomy
(pre-registered, all reachable):

- `awaiting-pack` — interim green; the pack is absent.
- `pack-committed-awaiting-gates` — the pack is present and its SHA-256 matches
  the pinned constant; the phase awaits the remaining hard gates before
  production.
- `pack-hash-mismatch-refuse-to-run` — **BLOCKED**; the MANDATORY refuse-to-run
  condition (production must never sample against an unpinned or altered pack).

All three branches were exercised locally (mismatch → block; absent →
awaiting-pack; restored → pack-committed-awaiting-gates).

`promotedPhysicalMassClaimCount` remains 0; `physicistReviewPending` is carried
explicitly; the claim boundary holds in every branch.

## The pre-registration pack (`studies/phase456_consolidated_n4_launch_001/preregistration/`)

Fixed **before** any physics number is computed. Files (hash-pinned):

- `a4_symmetry_irrep_projectors.json` — A4 Stage-A.
- `a5_gaussian_domination_stage_a.json` — A5 Stage-A verdict.
- `pack_manifest.json` — thresholds, power gate, AIC rule, MV-456, storage flag.
- `pack_hash.json` — the committed SHA-256 (also pinned as `PinnedPackSha256`
  in `Program.cs`; the hash is over the byte-exact concatenation of the first
  three files in listed order).

The pack is produced by the standalone, deterministic generator
`preregistration/a4a5_pack_generator/` (no HMC, no sampling). The generator is
**not** a wired phase, **not** in the traversal, and **not** in the `.slnx`;
phase456 production only ever reads the committed pack. When this pack is wired
into a real checkpoint, the generator (non-phase tooling that writes files in
the repo) and the pack files must be registered in the scanner exclusion /
known-file lists per the CLAUDE.md whole-repo-scanner rule.

### A4 Stage-A (exact + analytic; verified two independent ways)

Realized spacetime symmetries of the committed n=4 lattice-canonical
(Coxeter–Freudenthal–Kuhn) torus action, decided by **both** a combinatorial
simplicial-automorphism test (edge/face/cell sets preserved) **and** a numeric
`S_B(pushforward) == S_B` battery carrying the phase448/452 `Faces[f][0]` /
edge-orientation gauge. The two agree (worst residual 1.5e-14):

- Translations `(Z_4)^4` (order 256) — realized (phase448 established exact
  n=4 covariance); momentum labels `k ∈ (Z_4)^4`.
- Realized point group **order 48** = `S_4 × {±I}` (the signed permutations
  preserving the Kuhn body-diagonal (1,1,1,1) up to sign).
- Rest-frame little group (elements fixing the time axis forward)
  **H_s = S_3** (order 6; irreps `{1, 1, 2}`; 3 conjugacy classes,
  abelianization order 2).

**Parity ruling:** no realized improper element implements spatial inversion
(the Kuhn body-diagonal is broken by any spatial reflection), so **P is not a
good quantum number** — pseudoscalar `0-+-like` labels are **BANNED**; channels
carry realized-irrep labels only. (Axis-swap elements have 3D spatial
determinant −1 but do not implement spatial inversion; they are cubic/
permutation irreps, not a parity operation.)

**Irrep channel table + exact-rational projector kernels** over the 50-dim
face-type interpolator space:
- Identity irrep (A1, scalar 0++-analogue): face-type multiplicity 15;
  the 2×2 GEVP basis `{O1=Tr(F²), O2=Tr(Upsilon²)}` uses the trivial all-ones
  face-type weight (exact rational `1/50`), i.e. the covariant slice sum.
- Realized non-identity channel (A2, the S_3 sign character; realized, **not**
  a parity label): face-type multiplicity 3; exact-rational projector
  `P_χ = (1/|H_s|) Σ_g χ(g) ρ_facetype(g)`, integer numerators over `|H_s|=6`.
  It needs per-face-type (⇒ per-site) resolution, not the plain slice sum.
- `k_min = 2π/4` dispersion-row spec (phases are 4th roots of unity): needs
  per-site correlators.

**Per-site (un-slice-summed) correlator storage flag** —
`PHASE456_STORE_PER_SITE_CORRELATORS` — is MANDATORY and pinned in the pack; the
non-identity channel and the dispersion row both require it (committed phase452
outputs are slice-summed, so retroactive extraction is impossible).

### A5 Stage-A (Gaussian-domination pre-theorem attempt)

Target `m(β) ≥ m_free` on the even (`ω → −ω`) sector of the exactly-quartic
`S_B`. **Verdict: not provable at Stage A** —
`a5-stage-a-gaussian-domination-not-provable-obstruction-recorded`. The
obstruction is the deliverable:

1. **Compositeness** — field-level Gaussian-domination/infrared bounds control
   `⟨ωω⟩`, not the connected 4-`ω` composite correlator that defines the 0++
   pole; no elementary inequality transports the field bound to the composite
   channel for the matrix-valued su(2) quartic.
2. **Reflection positivity** — Gaussian domination presupposes reflection
   positivity across the axis-0 time reflection; the Kuhn body-diagonal is not
   preserved by that reflection (A4), so link-reflection positivity is not
   established for this simplicial `S_B`.
3. Non-locality — the quartic coupling is plaquette/face-local, not single-site,
   so the single-site Gaussian-domination measure hypothesis does not apply.

This does **not** cancel the β ladders (WS4/Binder sampling is not analytically
closed by A5); it feeds phase458 gate G1 as the A5 terminal string.

### Pack manifest (analysis pre-registration)

- **Threshold:** `max(calibrated family-wise ≥3σ, per-row 3σ)`; a bare 99th
  percentile is **forbidden**.
- **Power gate:** each row must reach pre-registered power ≥ 0.8 to detect a
  true 3σ effect; underpowered rows emit `underpowered` (no claim either way).
- **AIC window aggregation:** MECHANICAL — enumerate every contiguous cosh-fit
  window in the informative range, AICc-weight (Akaike weights) across them; no
  analyst-chosen window exists anywhere in the pipeline.
- **MV-456 fallback:** a pack-**scoping** device (n=4 free-field gate + identity
  2×2 GEVP zero-momentum gap only), **not** an A4-gate bypass — it never relaxes
  a gate and the A4 kernels stay committed ex ante.

## Hard gates (before production sampling)

phase455 terminal recorded; A4 Stage-A kernels committed (this pack); A5
Stage-A verdict committed (this pack); O4 memo (or explicit user-renewed risk
acceptance) before the production HMC — the largest at-risk spend under O4.

## Consumers / limb

Closes ledger limb L6 at probed-volume scope on T1 with a Gaussian-null Binder
column (and L8 at two-volume strength). A coherent ≥3σ departure in ≥2
distinct-irrep channels (`|Δ_ch| ≤ 2`) is a mandatory n=5 escalation (contingent
consumer of registry block B:471–476) before ANY claim.

## Framing

Workbench-relative structure data ONLY (su(2) toy algebra, reduced Spin(4)
slice, lattice units). A4 group-theory content is exact rational structure, not
a physical spectrum. Zero physics computation; nothing measured, filled, or
promoted; `promotedPhysicalMassClaimCount = 0`; lattice-unit quantities stay in
lattice units and are never relabelled.
