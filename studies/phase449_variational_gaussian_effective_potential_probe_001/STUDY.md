# Phase449: Variational Gaussian (CJT-Hartree) Effective Potential Probe

The 2026-07-04 review board's #1 named experiment (binding record: the
"2026-07-04 Review Board Outcome (post-Phase448)" section of
`docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md` and the matching entry in
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`), after the positive-subspace
saddle `V_eff` was **retired unconditionally** (not stationary in ω,
discontinuous at negative-mode crossings, loop hierarchy inverted, unbounded
below at t→0).

## The question

Tree level has no scale (`S_B = a₂t² + a₄t⁴`, `a₂,a₄ > 0` on invariant rays;
`a₃ = 0` exactly there by the ω→−ω Z₂). No perturbative Coleman-Weinberg scale
exists (443/446/447/448). The **non-perturbative question is open**, and the
30–235 transverse negative modes the retired convention discarded are exactly
where a dynamical scale could hide. The variational Gaussian (CJT-Hartree)
effective potential is the cheapest **rigorous** instrument that keeps them:
for *any* Gaussian ansatz, `V_G` is a Feynman-Jensen **upper bound** on the
constrained free energy, and the Hartree gap equation resums the quartic into
a self-consistent positive mass. A **null** is strong evidence against a scale
at ansatz level; a **positive** is mean-field-only and requires the phase450
constraint-EP HMC for confirmation.

## Construction

At each composite point `(ω = t·u, θ*(t·u))` — verbatim Phase443/446/447
machinery (hardened Newton θ*-solver with the relative-`1e-8`-OR-absolute-
floor-`1e-10` point gate; joint FD Hessian at `HessStep 1e-4`; Jacobi with
eigenvectors; same members `identity` + `sd2-id0/c0.5` + `asd2-id0/c0.5`, same
`RngSeed 20260703`, same 16-point t-grid on (0,3], 2 seeds,
`CreateUniform4D(1)`, nOmega=195, nTheta=48, joint 243):

1. **T4 pair table over ALL modes** above the zero tolerance — positive AND
   negative (unlike Phase447's positive-only figure-eight): `T4[a,a]` via
   `D4Diag`, `T4[a,b]` via `D4MixedPair` on unit eigenvectors at
   `StencilH = 0.1` (the **large-h-by-design** convention with shared axis
   evaluations; the ω sector is exactly quartic so the stencils are
   h-independent there — Phase447's lesson).
2. **Diagonal Gaussian ansatz** in the H-eigenbasis,
   `Σ = Σ_a σ_a v_a v_aᵀ`, `σ_a > 0` (β = 1 recorded convention):

   ```
   V_G(t;σ) = S_B + ½ Σ_a λ_a σ_a + ⅛ Σ_{a,b} T4[a,b] σ_a σ_b − ½ Σ_a log σ_a
   ```

   (additive constant `(n/2)(1+log 2π)` dropped and recorded per point).
3. **Hartree gap equation** (stationarity in σ):
   `1/σ_a = m_a := λ_a + ½ Σ_b T4[a,b] σ_b`, solved by damped fixed-point
   iteration (`α₀ = 0.3`, adaptive halving on divergence) from
   `σ_a = 1/|λ_a|` clipped to `[1e-6, 1e3]`; converged when
   `max_a |σ_a m_a − 1| ≤ 1e-8`. **Fail-closed**: any `m_a ≤ 0` or no
   convergence within 500 iterations records
   `hartreeSelfConsistentSolutionExists=false` for that point — no
   floor-clamping into fake convergence.
4. **Classification** on the raw converged `V_G(t)` curve by the
   strict-local-minimum rule — no fits (the Phase446 lesson). Random
   open-mesh rays ⇒ only `t > 0` probed; the `a₃ = 0` Z₂ (±t degeneracy) is
   noted, not tested.

**Bound caveat (recorded)**: the Feynman-Jensen bound applies to the Gaussian
expectation of the *exact* action. `S_B` is exactly quartic in ω (bound exact
on the ω sector) but transcendental in θ (`Ad = exp(ad_θ)` — the Phase442/447
structure result), so the quartic Taylor truncation in θ is a recorded
workbench convention.

## Batteries (fail-closed)

- **θ*-stationarity gate** — verbatim: per-point relative ≤ 1e-8 OR absolute
  gradient floor 1e-10; identity θ*=0 exact; LinearizeTheta FD ≤ 1e-6.
- **identity exact-quartic h-independence** — T4 battery scalar at h vs h/2
  agree ≤ 1e-6 (exact on a quartic; validates the stencil machinery).
- **Einsteinian Richardson** — h vs h/2 ≤ 5e-2 (honest FD tolerance for the
  transcendental θ-dependence).
- **offset immunity** — `S_B + 1000` leaves T4 unchanged ≤ 1e-5, gated
  **entrywise** on the battery T4 table (the direct reading; impossible to
  fail with clean stencils — Phase447's noise detector). The 1/|λ|²-weighted
  battery scalar over the *softest* modes amplifies benign ~1e-9 absolute FP
  noise on near-zero entries (~4e-5 in the smoke run) and is recorded, not
  gated; the Richardson gates stay on the weighted scalar (there the probed
  error is the physical θ h-truncation of the load-bearing contraction).
- **gap-equation batteries** — converged residual ≤ 1e-8 per point; all
  `m_a > 0` at convergence; damping/iteration counts recorded.
- **solver-plumbing exactness control** — with `T4 := 0` the gap equation must
  reproduce `σ_a λ_a = 1` to 1e-10 on the identity anchor's positive modes and
  must **fail** on a set containing negative modes (Einsteinian anchor) —
  validates both the convergent and the fail-closed paths.
- **bound-direction** — `V_G(converged) ≤ V_G(initial)` (minimization
  descended); per-iteration functional monotonicity recorded.
- **ansatz-rotation honesty check** (recorded, not gated) — at one anchor per
  Einsteinian member the diagonal ansatz is re-solved in a seeded
  pairwise-rotated basis within the span of the 40 softest modes (T4 entries
  touching rotated vectors recomputed by fresh stencils); both converged `V_G`
  values recorded — the basis dependence of the diagonal restriction is
  *measured*.

## Verdict taxonomy (the phase passes on internal consistency regardless)

- `gaussian-bound-null` — no member interior-finite (any seed): the
  rigorous-bound family shows no symmetry breaking along probed rays — strong
  evidence against a scale, per the board.
- `gaussian-mean-field-candidate` — seed-stable interior minima with a clean
  identity control — **mean-field-only**; phase450 CEP required; NOT a
  candidate promotion.
- `gap-equation-breakdown` — self-consistency fails somewhere (recorded
  honestly with per-point failure reasons).
- `gaussian-unstable-or-control-contaminated-recorded` — an interior minimum
  that is not seed-stable, or an identity-control minimum (an added honesty
  kind beyond the four named in the design, following the Phase445/446
  discipline: such an outcome is neither a null nor a candidate).
- `…-blocked` — batteries failed.

Precursors gated: Phase447 (`twoLoopSaturationProbePassed`) and Phase448
(`torusModeVolumeSaturationProbePassed`).

## Outputs

`output/variational_gaussian_effective_potential_probe.json` and
`…_summary.json` (identical content; NaN → null everywhere). Env overrides:
`PHASE449_MESH_REFINEMENT/RAYS/TMAX/GRIDN/OUTPUT_DIR/STENCIL_H`.

## Mandatory framing

Workbench-relative candidate data ONLY (su(2) toy algebra, reduced Spin(4)
slice, lattice units); the diagonal Gaussian ansatz, β = 1, and the θ quartic
truncation are workbench conventions pending physicist review; NO
GeV/pole/VEV promotion either way; no Phase201/Phase256 contract field is
filled; nothing is promoted.
