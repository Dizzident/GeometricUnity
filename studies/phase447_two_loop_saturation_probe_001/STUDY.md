# Phase447: Two-Loop Saturation Probe

The named beyond-one-loop follow-up after Phase446 resolved the Phase445 RG
candidate as a fit-normalization artifact. Design source:
`studies/phase446_rg_scheme_dependence_resolution_probe_001/PHASE447_TWO_LOOP_DESIGN.md`.

## The question

Phase443 showed the one-loop joint effective potential has no interior minimum on
the minimal mesh; Phase446 closed the RG-improvement-by-potential-fit shortcut. The
remaining no-platform lever is **genuine two-loop structure**: the vacuum diagrams
built from the action's third and fourth derivative tensors (nontrivial by the
Phase442 degree-lift). Does the two-loop correction bend the effective potential
into a finite interior minimum?

## Structure result

With `S_B = ½‖M·Ad(θ)·F(ω)‖²`, `F = dω + ½[ω,ω]`: `S_B` is an **exact quartic in ω**
at fixed θ (identity-control stencils exact to roundoff — the anchor battery) and
**transcendental in θ** via `Ad = exp(ad_θ)` — resolving Phase442's "degree > 2" as
θ-transcendentality. Einsteinian vertices therefore need honest FD stencils.

## Construction

0. **Recompute** the `V_eff(t)` rays with the verbatim Phase443/446 hardened
   machinery (same members/seeds/rays as Phase445/446, same `RngSeed`; Phase445
   16-point grid), with the joint FD Hessian now eigen-decomposed **with
   eigenvectors** (Jacobi rotations accumulated).
1. **Two-loop terms** at each composite point on the **positive-subspace
   propagator** `G⁺ = Σ_{λ>zeroTol} v vᵀ/λ` (the recorded convention extending the
   one-loop positive-mode rule; the backgrounds are saddles with ~31–59 negative
   modes — the substantive convention question, `physicistReviewPending`):
   - figure-eight `= (1/8) Σ_{ij} T4[v_i,v_i,v_j,v_j]/(λ_i λ_j)` — deterministic
     over **all** positive pairs (mixed 4th-derivative stencils, shared axis evals);
   - sunset `= −(1/12) Σ_{ijk} T3[v_i,v_j,v_k]²/(λ_i λ_j λ_k)` — **soft-mode
     truncated** to the K=40 softest positive modes (the `1/λ³` weight is
     IR-dominated), with a convergence-in-K sweep `{10,20,30,40}` and a **full
     deterministic** evaluation at an anchor point per Einsteinian member.
   - **Propagator soft-mode floor** (recorded convention): modes enter only above
     `floorRel × maxAbsEig` (default `1e-4`), swept over `{1e-5, 1e-4, 1e-3}` —
     the softest modes sit near the zero tolerance and an unfloored `1/λ³` sum is
     numerically and physically uncontrolled. The floor/K sweeps are assembled
     **free** from cached stencil tables.
   - **Stencil step `h = 0.1` — large by design**: `S_B` is an exact quartic in
     ω, so the stencils are h-independent on the ω sector, and a large h lifts
     the numerators far above the double-precision cancellation floor (the first
     smoke run *proved* `h = 5e-3` is noise-dominated: the offset-immunity
     battery, impossible to fail with clean stencils, failed at 135%). The
     transcendental θ truncation is controlled by the h-vs-h/2 Richardson battery.
2. **Classification** on the raw `V = S_B + V_1loop + V_2loop` curve by the
   strict-local-minimum rule — **no fits** (the Phase446 lesson).
3. **Alternative-convention arm**: one full Einsteinian ray recomputed with the
   absolute-value propagator over **all** nonzero modes; a verdict that flips under
   the convention is recorded as convention-dependent, **not** a candidate
   (Phase445/446 discipline).

## Batteries

- **theta\*-stationarity gate** — verbatim hardened solver; per-point relative
  `≤ 1e-8` OR absolute-gradient floor `1e-10` (the Phase446 small-t convention).
- **LinearizeTheta FD-vs-analytic** — `≤ 1e-6`.
- **identity exact-quartic stencil battery** — h vs h/2 agreement `≤ 1e-6`
  (stencils are exact on a quartic; validates the stencil machinery itself).
- **Einsteinian Richardson battery** — h vs h/2 agreement `≤ 2e-2` (honest FD
  tolerance for the transcendental θ-dependence; load-bearing).
- **offset immunity** — `S_B + 1000` leaves T3/T4 unchanged (stencils annihilate
  constants; `≤ 1e-9`).
- **K-convergence** — sunset tail change across the K-sweep recorded.
- **anchor cross-checks** — full-vs-truncated residual recorded.
- **floor-sweep stability** — the two-loop classification must agree across the
  propagator soft-floor sweep (else the verdict is not admissible).
- **perturbativity admissibility** — a saturation verdict (either way) is only
  admissible where `max |V_2loop|/|V_1loop| ≤ 0.5`; beyond that the loop
  expansion is out of control and the honest outcome is that the lever is
  **non-perturbative at this scope**.
- **control (recorded + candidate-gating)** — an identity two-loop "minimum" in
  the admissible regime blocks any candidate claim; in the non-perturbative
  regime it is supporting evidence of the breakdown (identity monotonicity is
  NOT a theorem at two loops).

## Verdict taxonomy

- **non-perturbative-or-convention-bound** — perturbativity or floor-sweep
  stability (or the identity control) fails: the two-loop lever is CLOSED as
  non-perturbative/convention-bound at the minimal-mesh scope (decisive frontier
  statement; deciding it beyond this scope requires the platform unlock projects
  or a source anchor).
- **two-loop-candidate** — admissible + interior minimum + seed-stable + both
  convention axes + clean control.
- **convention-or-seed-unstable-recorded** — admissible interior minimum that
  fails seed/propagator-arm stability.
- **no-two-loop-saturation** — admissible regime, no minimum: two loops are also
  insufficient (frontier-sharpened).

The phase **passes on internal consistency regardless of which way it falls**.

## Results (filled from the committed run)

Terminal status:
`two-loop-saturation-probe-passed-two-loop-lever-non-perturbative-convention-bound-at-minimal-scope-frontier-sharpened`
(runtime 42 min, 48.4M action evaluations).

- **Numerics all green**: theta gate 1.4e-9 (0 points via the absolute floor);
  identity exact-quartic stencil h-independence `1.3e-7` (the ω-quartic anchor
  works); Einsteinian Richardson `7.6e-4`; offset immunity `2.9e-7`.
- **Physics admissibility all fails**: `max |V_2loop|/|V_1loop| = 1.6e3`
  (median 155) — the loop expansion is out of control at these backgrounds;
  `floorSweepStable = false` (IR-domination: the classification is bound to the
  soft-floor convention); `seedStable = false`; the propagator-convention arm
  flips (`runaway` vs `interior-finite`); the identity control acquires a
  spurious "minimum" (evidence of the same breakdown, not an estimator defect).
- **Verdict**: `resolutionKind = non-perturbative-or-convention-bound`,
  `twoLoopVerdictAdmissible = false`, `twoLoopCandidate = false`.

**The two-loop lever is closed as non-perturbative at the minimal-mesh scope.**
With Phase443 (one loop: no saturation) and Phase446 (RG-fit shortcut: artifact),
the internal no-platform program at the minimal mesh is exhausted through two
loops. Deciding the scale question beyond this scope requires changing the IR
spectrum (the Phase444 unlock projects) or a source anchor.

## Mandatory framing (verbatim intent)

All scales are **workbench-relative candidate data ONLY** — `su(2)` toy algebra on
the reduced Spin(4) slice, lattice units; the positive-subspace two-loop about a
saddle is a **workbench convention pending physicist review**
(`twoLoopConventionIsWorkbenchConvention = true`, `physicistReviewPending = true`).
**NO GeV/pole/VEV promotion either way.**

## Fail-closed

Target-blind construction (`targetBlindConstructionHash` recorded);
reduced-spin4-slice only (six verbatim recorded-boundary keys). No scale/pole/GeV
lineage; no Phase201/Phase256 contract field filled; nothing promoted either way.

Precursors: Phase443 (`jointEffectivePotentialSaturationProbePassed &&
einsteinianLogSaturationObserved === false`) and Phase446
(`rgSchemeDependenceResolutionProbePassed &&
phase445MinimaResolvedAsFitNormalizationArtifact &&
einsteinianRgSaturationObserved === false`).

## Run

```bash
dotnet run -c Release --project studies/phase447_two_loop_saturation_probe_001/Phase447TwoLoopSaturationProbe.csproj
```

Ends `Passed=True`, 0 warnings. Outputs `output/two_loop_saturation_probe.json` and
`_summary.json` (including per-point figure-eight/sunset values and the raw ray
data). Runtime ~60–90 min (anchor full-sunset evaluations dominate;
`PHASE447_FULL_ANCHOR_CAP` exists for smoke runs only).
