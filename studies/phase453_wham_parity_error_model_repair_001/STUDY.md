# Phase453: WHAM Parity-Antisymmetry Error-Model Repair

Team B's Wave-1 item 1 of the committed three-team elimination program
(`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md` §2). The ONE named
item standing between phase450 and the symmetric-phase null.

## The question

Phase450 found `single-well-at-zero` everywhere under all four binding
conditions but is `inconclusive-gates-failed` on exactly one gate: the sd2
parity-antisymmetry gate trips at **5.06 σ** (committed
`antisymmetryMaxSigma = 5.0594`) while the INDEPENDENT unconstrained tadpole
is zero (`-0.04 ± 0.10`, 0.44 σ). The exact on-ray Z2 (`S_B` even in `Φ`;
`a3 ≡ 0`) FORBIDS a true odd constraint-effective-potential, so the
discrepancy is either

1. a **WHAM error-model artifact** — the phase450 per-bin errors are treated
   as independent while `U(+Φ)` and `U(-Φ)` share every jointly-solved
   stitching constant `f_i` (tadpole-window `tauIntPhi = 40.35` sd2 / `72.9`
   identity, `N_eff ~ 105 / 62`); or
2. **genuine Z2 breaking** — the very signature the probe exists to find.

Phase453 discriminates with **two independent estimator arms on fresh
ensembles**, both propagating the `f_i` covariance the phase450 lower-bound
error model neglects.

## Stage 0 (pre-registration — this committed checkpoint)

Everything below is committed BEFORE any production trajectory. The committed
env-clean run is `PHASE453_MODE=preregister` (fast: ~2 s; hard batteries +
Stage-0 emission, NO production sampling).

1. **Localization (item 1).** The phase450 sd2 per-bin antisymmetry is
   recomputed from the committed `cepCurve` (only max fields are committed
   there). Result: the signal grows monotonically toward the edges and the
   max (5.06 σ) sits at the far edge `|Φ| = 2.375` — the WHAM stitch-error
   signature (edge windows accumulate the most `f_i` random-walk variance),
   while the independent tadpole is zero. Recorded as committed provenance.
2. **Schema (item 2).** The output records per-bin antisymmetry AND per-window
   signed-bin counts — never just the max.
3. **Corrected ladder (item 3).** The `c = 0 / ±0.25` soft-spring sub-ladder
   straddling the origin, spring from the phase450 auto-spring rule at the
   recorded 2.0 softening factor: `k = (2.0/0.25)² = 64`. Smoke-tested at the
   junction to the phase450 `±0.5` ladder: min neighbor overlap **0.425**
   (spring 64) vs the 0.15 gate — PASS on the pre-registered spring, **0
   fix-and-reruns** of the 2 budgeted (a softened spring 16 gives 0.588,
   recorded as the wider alternative). The stiffer identity control keeps its
   own phase450 spacing-0.25 ladder.
4. **Calibration (item 4).** 2000 synthetic control ensembles from the fitted
   EVEN CEP of the committed phase450 sd2 record, with matched per-window
   `tauInt` (`N_eff = N/(2τ)` deflation) and the exact production window
   layout. Baked 99th-percentile decision boundaries: max-antisymmetry
   **σ99 = 3.16**, abs99 = 0.777; false-flag at 5 σ = 0 (0/2000), at 3 σ =
   **1.7%** (the few-percent correlated-max false-flag the charge anticipates).
5. **S(k) hooks (item 5).** Structure-factor measurement hooks on the
   unconstrained/released (`k = 0`) columns, pre-registered ex ante
   (`k = 2π m/n` on the `(Z_n)^4` reciprocal lattice; released-ensemble
   `S(k)`); emitted in the output schema, filled in production.

## Production (runs LATER, env-clean — NOT this checkpoint)

`PHASE453_MODE=production` (enabled by flipping the committed `DefaultMode`).
Fresh identity-control + sd2 runs at `n=3` under all four phase450 binding
conditions with the corrected ladder. Two analysis arms:

- **Arm A** — moving-block bootstrap of trajectories within each window, block
  length `L_w = ceil(2 τ_Φ,w)` from that window's fresh chain, with a FULL
  WHAM re-solve on every one of 400 replicates (propagates the `f_i`
  stitching-constant covariance into the antisymmetry errors).
- **Arm B** — within-window antisymmetrized `U_odd` estimator; bins enter only
  with `N_eff ≥ 25` per sign bin (raw counts scaled by `1/(2 τ_Φ,w)`).

## Verdict taxonomy (pre-registered, fail-closed)

- **T1 symmetric-phase-null** — both arms within the calibrated thresholds AND
  single-well-at-zero on both members AND fresh tadpole zero AND control clean
  ⇒ the null IS claimed; frontier upgrades to "no non-perturbative scale along
  translation-invariant rays at `n=3`".
- **T2 z2-breaking-candidate** — antisymmetry survives BOTH arms above the
  thresholds AND the fresh tadpole moves off zero at `≥5 σ` with coherent sign
  on the same member ⇒ program-level alarm; escalate to a volume scan at `Φ*`.
- **T3 estimator-discordance-or-new-defect** — arms disagree or gates fail in
  a new pattern ⇒ no physics claim; commit the discordance pattern.

Any phase450 hard-battery failure ⇒ `inconclusive-gates-failed` regardless of
the arms.

## Modes / env knobs

`PHASE453_MODE` = `preregister` (committed default) | `calibrate` | `smoke` |
`production`. `calibrate`/`smoke` and the reduced `PHASE453_*` knobs are dev
tools; the incremental env-knob fingerprint flags them (correct). Production
runs env-clean once `DefaultMode` is flipped.

## Outputs

`output/wham_parity_error_model_repair.json` and `…_summary.json` (identical;
NaN → null everywhere).

## Mandatory framing

Workbench-relative candidate data ONLY (su(2) toy algebra, reduced Spin(4)
slice, lattice units, finite volume); `β`, the umbrella springs, the Euclidean
`Φ` inner product, the flat HMC kinetic mass, and the θ-Haar chart are
WORKBENCH CONVENTIONS pending physicist review (`physicistReviewPending=true`,
Wave-0 item 0.3 OPEN); NO GeV/pole/VEV promotion either way; no Phase201/
Phase256 contract field is filled; nothing is promoted. The phase passes on
internal consistency (pre-registration completeness + hard batteries)
regardless of the eventual production verdict.
