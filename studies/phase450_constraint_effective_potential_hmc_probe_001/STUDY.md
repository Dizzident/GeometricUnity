# Phase450: Constraint Effective Potential by Umbrella-Sampled HMC

The 2026-07-04 review board's named **ansatz-free** non-perturbative
experiment (binding record: the "2026-07-04 Review Board Outcome" section of
`docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md` and the matching journal
entry), run after phase449's recorded `gap-equation-breakdown` — the
diagonal-Gaussian family could not hold the Einsteinian negative-mode
structure, and the scale question passed here intact. The preserved
feasibility prototype is
`studies/phase448_torus_mode_volume_saturation_probe_001/cep_hmc_prototype/`
(theta=0 sampler demo; explicitly **not** reusable as-is).

## The question

The true effective potential is **convex** — "interior minimum" was the wrong
signature all along. SSB manifests as a **flat Maxwell bottom / degenerate
±t\* minima** of the finite-volume constraint effective potential
`U(Φ) = −(1/β) ln P(Φ)`. `S_B ≥ 0` real ⇒ `e^{−βS_B}` has **no sign problem**.
This phase measures the FULL non-perturbative constrained free energy on
lattice-canonical tori (`CreateUniform4DPeriodic(n, latticeCanonical: true)`,
n=3 primary), members `identity` (control) + `sd2-id0/c0.5` (+`asd2` via env),
integrating the 30–235 transverse negative modes every retired perturbative
convention discarded. The `a3 = 0` parity on invariant rays makes ±t exact
partners — the ± symmetry of the reconstruction is itself a check.

## The four binding conditions (each fail-closed)

1. **Gauge-invariant collective coordinate** — `Φ = ⟨u_inv, ω⟩` with `u_inv`
   the phase448 translation-invariant per-type ray (`Random(20260703)`,
   16 types × 3 coefficients, types 0..14 on edges) in the **oSign
   base→tip lattice gauge**, with the 3 global-su(2) zero-mode directions
   `z_l = [e_l, u]` Gram-Schmidt-projected out and the **projector recorded**
   (on the invariant metric `⟨u,[c,u]⟩ = 0` identically by bracket
   antisymmetry, so the recorded overlaps are machine-zero — asserted, and
   the projection applied regardless). Random rays leak ker-d gauge motion —
   forbidden; a translation-invariant `dλ` requires uniform `λ`, i.e.
   `dλ = 0`, so the invariant ray cannot leak them.
2. **Theta-Haar measure** — θ enters `S_B` only via `Ad = exp(ad_θ)` in SO(3)
   per vertex (su(2) `f = ε` ⇒ rotation by `|θ|` about `θ`); the `R^n`
   θ-integral diverges. The sampler maintains per-vertex rotations as **unit
   quaternions** updated by **symmetric Haar-invariant Metropolis** moves
   (small left-multiplied rotations alternating with single-vertex
   uniform-Haar independence proposals), charted to the axis-angle
   **fundamental domain `|θ| ≤ π`** (w ≥ 0 quaternion representative) at the
   operator boundary. Hard chart batteries: uniform-θ shift leaves `S_B`
   exactly constant; a `|θ| > π` config and its wrapped image give identical
   `S_B`; a single-vertex 2π rotation is the identity.
3. **Ergodicity control** — `τ_int(S_B)` and `τ_int(Φ)` recorded per window;
   **gate `N_eff = N/(2τ_int) ≥ 100`** per window; adaptive trajectory count
   (chunks until the gate is met or the fail-closed cap records the failure).
4. **θ=0 is never a verdict** — the physics run is the joint
   (ω-HMC, θ-Haar-Metropolis) ensemble. The identity control is exactly
   θ-independent (`GradTheta == 0` asserted), so its ω marginal *is* its
   joint ensemble.

## Design

- Umbrella windows over `Φ ∈ [−t_max, +t_max]` (default `t_max = 2.5`),
  symmetric centers including 0, planned **per member** on its own tree
  curvature (the identity control is ~12× stiffer than sd2); harmonic bias
  `(k/2)(Φ−c)²` added to `β·S_B` (bias **not** multiplied by β — recorded
  convention). Auto rule: `spacing = min(0.5, 2.2/√(6β·κ_tree_max))`,
  `k = (2.2/spacing)²` — self-consistently `k ≥ 6β·κ_tree ≥ 6β·κ_F`, pinning
  window means within ~15% of centers while the biased width stays
  ≥ spacing/2.35 for neighbor overlap.
- Per-window leapfrog HMC on ω against the platform's analytic
  `ComputeJointGradient` with the bias force added (flat kinetic mass —
  recorded convention; ε auto-tuned in warmup to acceptance [0.90, 0.98] —
  the first smoke run showed acceptance ~0.85 already drags `⟨e^{−dH}⟩` to
  0.92), interleaved θ-Haar sweeps between trajectories. Windows are
  independent chains and run in parallel (per-window operator/mass instances;
  seeds pre-drawn sequentially — deterministic for a given `PHASE450_SEED`).
- **Log-space WHAM** reconstruction of `U(Φ)` (bin width = spacing/4);
  per-bin errors = quadrature sum of (a) local counting (total bin counts
  deflated by the window-mean `2τ_int(Φ)`), (b) **stitching accumulation**
  (per-boundary variance `1/(β²·neff_overlap)` accumulated from the central
  window out to the bin — the `f_w` random walk that is the textbook dominant
  umbrella-chain error; the smoke runs measured it as a spurious 6-7-"σ" tilt
  under the naive model while the independent unconstrained tadpole was
  zero), and (c) a robust high-frequency roughness floor from second
  differences (`1.4826·MAD(|U_{i−1}−2U_i+U_{i+1}|)/√6`); all components
  recorded, model recorded as approximate.
- **Unconstrained tadpole diagnostic** per arm: one extra k=0 window
  measuring `⟨Φ⟩` of the raw joint ensemble directly (never enters WHAM) —
  attributes any ±Φ asymmetry of the reconstruction independently.
- Tree curves `S_B(t·u_inv, 0)` per member with the exact-quartic fit
  `a2 t² + a3 t³ + a4 t⁴` recorded (a3 ≈ 0 is the review-board Z2).

## Gates

**Hard batteries** (fail ⇒ terminal `…-blocked`): precursors (phase448
`torusModeVolumeSaturationProbePassed`, phase449
`variationalGaussianEffectivePotentialProbePassed`); orbit map; S_B
translation covariance ≤ 1e-9; objective-path consistency ≤ 1e-9;
analytic-vs-FD joint gradient ≤ 1e-4; the three θ-chart batteries ≤ 1e-9;
identity θ-independence ≤ 1e-12; projector overlaps ≤ 1e-12; WHAM plumbing on
a synthetic exactly-sampled Gaussian umbrella set (reconstruction ≤ 0.08).

**Sampling gates** (fail ⇒ verdict `inconclusive-gates-failed`, recorded per
window/arm, never silent): per-window `⟨e^{−dH}⟩ = 1` within error
(measure preservation); equipartition virial
`⟨Σ_i ω_i ∂U_tot/∂ω_i⟩ = nOmega` within error (bias included); `N_eff ≥ 100`;
neighbor histogram overlap ≥ 15%; WHAM residual ≤ 1e-8; coverage half-width
≥ 0.5·t_max; **±Φ antisymmetry** `|U(Φ)−U(−Φ)|` ≤ 4σ (the a3=0 Z2 is exact
only on the invariant sector — off-sector a3 ~ 1e-4 real — and the per-bin
error model is an approximate lower bound, hence 4σ with the raw max
recorded); the **large-β control column** (β=8 default, first
Einsteinian member) must classify in the single-well family with
Pearson ≥ 0.9 against the tree curve.

## Classification (pre-registered)

On bins with ≥ 30 counts:

- `single-well-at-zero` — minimum within 1.5 bins of Φ=0 (or an off-center
  minimum whose depth below the center bin is ≤ 3σ — consistent with a single
  well plus noise), monotone rise both sides within 2 median errors ⇒
  **symmetric-phase null** for that member.
- `single-well-offset` — a UNIQUE minimum at Φ\*≠0 (depth > 3σ, no degenerate
  partner) with monotone rise both sides — still the **null family** for the
  SSB question (no ± pair) but with a recorded parity tilt. The joint
  ensemble has no exact Φ→−Φ symmetry (the a3=0 Z2 is exact only on the
  invariant ray — machine-verified in the tree fit; the action's cubic terms
  can generate a fluctuation tadpole and the Kuhn triangulation is not
  reflection-symmetric), so this outcome is recorded first-class, pending
  physicist review.
- `flat-bottom-degenerate-minima` — **SSB candidate (workbench-relative
  only)**: EITHER an off-center minimum pair at ±t\* (depth > 3σ, partner
  within 2 bins and 3 median errors) OR a flat Maxwell bottom of half-width
  ≥ 1.0 lattice units with rising edges; t\* reported.
- `inconclusive-structure` — anything else, recorded as structure data.

Verdict kinds: `symmetric-phase-null` / `flat-bottom-ssb-candidate` /
`control-contaminated-recorded` / `inconclusive-gates-failed` /
`tilted-single-well-parity-asymmetry-recorded` (added honesty kind per the
phase445/446/449 discipline: every curve is in the single-well family but a
tilt is present — either the only failing gates are antisymmetry gates with
a significant tadpole diagnostic, or the gates pass with a `single-well-offset`
classification) / `inconclusive-structure-recorded`. The phase passes on
internal consistency regardless of the verdict.

## Outputs

`output/constraint_effective_potential_hmc_probe.json` and `…_summary.json`
(identical content; NaN → null everywhere). Env overrides:
`PHASE450_TORI/MEMBERS/TMAX/WINDOWS/TRAJ/MAXTRAJ/MINTRAJ/WARM/NEFF/BETA/`
`BETA_LARGE/KUMB/NLEAP/EPS/THETA_PROPS/PARALLEL/SEED/OUTPUT_DIR`.

## Mandatory framing

Workbench-relative candidate data ONLY (su(2) toy algebra, reduced Spin(4)
slice, lattice units, finite volume); β, the umbrella springs, the Euclidean
(non-mass-weighted) Φ inner product, the flat HMC kinetic mass, and the
θ-Haar chart are workbench conventions pending physicist review; **a
flat-bottom signature is a workbench-relative SSB CANDIDATE only**; NO
GeV/pole/VEV promotion either way; cross-member normalization comparisons are
barred by the recorded Faddeev-Popov caveat; no Phase201/Phase256 contract
field is filled; nothing is promoted.
