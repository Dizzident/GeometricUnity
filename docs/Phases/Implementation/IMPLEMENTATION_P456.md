# Implementation P456 - Consolidated n=4 Launch (Team B+A, Wave-2)

Phase456 hosts the **A4 + A5 Stage-A pre-registration pack** and the pack
**refuse-to-run gate** (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, item 9;
standing items 12 & 14). Registry number 456 per
`docs/Phases/PHASE_NUMBER_REGISTRY.md`.

## What this phase does now

The production path is implemented by an explicit `--phase456-production`
mode on the proven phase452 sampler. It fixes `n=4`, production/control
trajectory counts `16000/10000`, warmup `2000`, twelve leapfrog steps, and
`beta={1,4,400}` in code; it refuses every environment variable whose name
starts with `PHASE`. The ordinary phase452 and phase456 generator paths never
launch the long sampler. Phase456 is the pre-registration pack
**refuse-to-run gate**, deterministic result consumer, and standing claim
boundary. Terminal taxonomy includes:

- `awaiting-pack` — interim green; the pack is absent.
- `pack-committed-awaiting-gates` — the pack is present and its SHA-256 matches
  the pinned constant and the separately tracked production artifact has not
  landed.
- `pack-hash-mismatch-refuse-to-run` — **BLOCKED**; the MANDATORY refuse-to-run
  condition (production must never sample against an unpinned or altered pack).

The hash branches were exercised locally (mismatch → block; absent →
awaiting-pack; restored → pack-committed-awaiting-gates). The production mode's
negative environment control was also exercised: a synthetic `PHASE456_*`
override refuses before mesh construction.

The authorized production run completed on 2026-07-15 with terminal
`production-analysis-invalid`. Pack, authorization, defaults, storage, and
exact-control shape are valid, but the scientific analysis is not: the A2 and
`k_min` mass rows are non-finite under the committed cosh/direct-Gaussian
comparison, the large-beta free-field sampler gate is red, and the SD2
production/control columns fail their autocorrelation-based effective-sample
gates. The consumer therefore withholds the family-wise threshold and
invalidates the complete five-row table. This is a pre-registered fail-closed
outcome, not a reason to change an estimator, threshold, or gate.

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

**Per-site (un-slice-summed) correlator storage flag** — named
`PHASE456_STORE_PER_SITE_CORRELATORS` in the pack — is MANDATORY. Production is
environment-clean, so this is a committed internal true setting rather than an
environment override. Every column stores the complete `4^3=64`
spatial-momentum correlator table at every time separation, including aligned
jackknifes. The transform is invertible to the spatial per-base-site
correlator. A separately accumulated `k_min` projection must reconstruct from
the complete table within `1e-10`; the smoke residual is at most `5.6e-14`.
The exact face-type A2 projector is applied before slice aggregation.

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

The consumer executes five rows: A1 2×2-GEVP gap vs exact-free control, A2 gap
vs exact-free control, lattice-cosh `k_min` dispersion residual, centered
Binder cumulant and susceptibility vs their exact-free controls. The exact
controls are independent samples drawn directly
from the block-diagonal Gaussian measure at `beta=1` and passed through the
same A1/A2/dispersion/Binder/susceptibility pipeline; the large-`beta` HMC
columns remain a separate sampler gate. At `T=4`, the mechanical
informative-window set contains
the single point `t=1`, so its AIC weight is exactly one. The family-wise
threshold is the fixed-seed multivariate-normal max-`|Z|` quantile computed
from the aligned null jackknife correlation, then maxed with the per-row 3σ
floor. The power calculation uses the achieved conservative `N_eff`, the
pack's `N_eff=100` floor as the reference resolution, and the pre-registered
true 3σ effect; both `N_eff≥100` and power ≥0.8 must hold. A1 uses the
conservative existing O1/O2/action estimate, A2 records its own O1/O2 maximum
autocorrelation time, `k_min` records the maximum over all three spatial axes
and real/imaginary components, and Binder/susceptibility use the invariant-ray
autocorrelation time. No row inherits another row's power estimate silently.

## Hard gates (before production sampling)

phase455 terminal recorded; A4 Stage-A kernels committed (this pack); A5
Stage-A verdict committed (this pack); O4 memo (or explicit user-renewed risk
acceptance) before the production HMC. The user supplied that explicit renewal
on 2026-07-15; it is hash-pinned in `production_authorization.json` and states
that O4 remains pending and no physical-mass/GeV claim is permitted. It is not
an O4 ruling.

## Consumers / limb

T1 would close ledger limb L6 at probed-volume scope with a Gaussian-null
Binder column (and L8 only at two-volume strength). The production artifact
does not reach T1: `production-analysis-invalid` closes neither L6 nor L8. A
coherent ≥3σ departure in ≥2 distinct-irrep channels (`|Δ_ch| ≤ 2`) would
mandate n=5 before ANY claim; the invalid-analysis firewall suppresses that
escalation and also suppresses Phase458/G3 motivation.

## Production record (2026-07-15)

- Environment-clean committed-default run: `16000` production trajectories,
  `10000` per control, `2000` warmup; wall time `21846.5987934 s`.
- Raw artifact SHA-256:
  `9b7e965a0b8ac906bc1352f908b28b6eb22511579c02ee91562f63beb67ed9cb`.
- Pack hash verified byte-exact:
  `40fd3c3488f94d18f50961e85d0bb3a3eabd1a31a071b61149875b8cf3d437aa`.
- All five columns, all `64` spatial momenta, per-site/per-face-type retention,
  and `50` aligned jackknife blocks are present. A consumer typo expecting 20
  blocks was corrected to the generator's fixed 50 before adjudication; no
  scientific rule changed.
- SD2 `beta=1`: generic `N_eff=7.1549`; row-specific
  `N_eff(A2)=7601.40`, `N_eff(k_min)=16.623`. SD2 `beta=4` has generic
  `N_eff=5.8589` and `N_eff(k_min)=17.802`.
- The five-row terminal is wholly invalid after the non-finite-row firewall;
  no family-wise threshold is calibrated, no n=5 escalation fires, and G3 is
  not motivated. The measured cost is `0.1137102 CPU-weeks`, so Phase458/G2 is
  available and within its 2.0 CPU-week budget.
- Phase471 remains `closure-not-decidable`: L5/L6/L8 open, L7 withheld.

## Framing

Workbench-relative structure data ONLY (su(2) toy algebra, reduced Spin(4)
slice, lattice units). A4 group-theory content is exact rational structure, not
a physical spectrum. Production measures lattice quantities but fills no
source-lineage contract and promotes nothing;
`promotedPhysicalMassClaimCount = 0`. Lattice-unit quantities stay in lattice
units and are never relabelled.
