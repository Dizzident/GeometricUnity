# Implementation P453 - WHAM Parity-Antisymmetry Error-Model Repair (Team B, Wave-1)

Phase453 executes the first phase of the 2026-07-10 three-team elimination
program (`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md` §2): the WHAM
parity-antisymmetry error-model repair of phase450 - the ONE named gate
standing between phase450's `single-well-everywhere` observation and the
symmetric-phase null. This commit lands **Stage 0 (pre-registration) only**;
the production run executes later, env-clean.

## What this checkpoint commits

The committed env-clean run is `PHASE453_MODE=preregister` (~2 s: hard
batteries + Stage-0 emission, NO production trajectory). It produces
`studies/phase453_wham_parity_error_model_repair_001/output/wham_parity_error_model_repair_summary.json`
with:

- **Item 1 - localization.** The phase450 sd2 per-bin antisymmetry recomputed
  from the committed `cepCurve`: max **5.059 σ at |Φ| = 2.375** (reproduces the
  committed `antisymmetryMaxSigma = 5.0594` exactly), signal monotone toward
  the edges - the WHAM stitch-error signature, while the independent tadpole is
  `-0.04 ± 0.10`.
- **Item 2 - schema.** Per-bin antisymmetry + per-window signed-bin counts in
  the production-arm schema.
- **Item 3 - corrected ladder.** `c = 0/±0.25` sub-ladder, auto-rule spring
  `(2.0/0.25)² = 64`; smoke junction min overlap **0.425 >= 0.15** on the
  pre-registered spring - **0 of 2 budgeted fix-and-reruns used**. Identity
  keeps its own spacing-0.25 ladder.
- **Item 4 - calibration.** 2000 even-CEP synthetic ensembles, matched
  per-window `tauInt`, exact production layout. Baked constants: **σ99 = 3.1597**,
  abs99 = 0.7773, false-flag@5σ = 0 (0/2000), **false-flag@3σ = 1.70%**.
- **Item 5 - S(k) hooks.** Released-column structure-factor schema, ex ante.
- **Verdict taxonomy** T1/T2/T3 baked; `verdictKind = pre-registration-committed`.

`whamParityErrorModelRepairPassed = true`; all hard batteries green
(cov 4e-15, chart ~1e-15, projector 3e-17, WHAM plumbing 5e-2 < 8e-2);
`physicistReviewPending = true` (Wave-0 item 0.3 open, explicit).

## Verdict: PENDING PRODUCTION

## Production verdict (2026-07-10): T1 — SYMMETRIC-PHASE NULL CLAIMED

Three env-clean production runs (the two budgeted fix-and-rerun iterations
were both used, each recorded in `productionIterationLog`):

- **Iteration 1** (MinTraj 1500, sub-ladder on): T3. Both corrected arms
  GREEN on sd2 (A 2.03σ / B 1.18σ vs the calibrated 3.16) — the committed
  phase450 5.06σ class does not reproduce under either corrected estimator —
  but sd2 classified inconclusive-structure and the released column missed
  the N_eff gate. Fix #1: MinTraj 1500 → 3000, released column 3×.
- **Iteration 2** (MinTraj 3000, sub-ladder on): T3, the decisive diagnostic.
  Both arms FLAG sd2 (7.14σ / 6.50σ); the odd residual localizes at the
  |Φ| = 0.25 mixed-stiffness junction (k=64 sub-windows vs k=16 main),
  grows with statistics, tadpole zero (0.91σ, N_eff 110); the uniform-ladder
  identity control is clean — a junction stitching systematic, not physics.
  Fix #2: drop the sub-ladder; sd2 reverts to the exact phase450 uniform
  ladder at the doubled budget.
- **Iteration 3** (uniform ladders, the committed record, 77.5 min): **T1**.
  Arms green on both members (identity 2.49/0.89, sd2 2.30/2.46);
  single-well-at-zero on both; fresh tadpoles zero (0.217±0.195 /
  0.026±0.088); identity clean; all hard batteries and gates green.

The phase450 gate failure is explained (per-bin errors treated as
independent while V(±Φ) share every jointly-solved stitching constant f_i;
the corrected arms propagate that covariance and kill it). THE FRONTIER
UPGRADES: no non-perturbative scale along translation-invariant rays at
n=3 (tree 443 / 1-loop 446 / 2-loop 447 / Hartree 449 / HMC CEP 450+453),
scoped to the audited member family and workbench conventions. LADDER
LESSON: umbrella programs use uniform-stiffness ladders or junction-aware
error models — mixed-stiffness junctions inject a statistics-growing odd
artifact into WHAM reconstructions.

The integrity verifier asserts the production record (mode, T1 verdictKind,
both-arm green booleans, classifications, tadpole bounds, both iteration-log
rows, uniform final ladder).

## Framing

Workbench-relative candidate data only (su(2) toy algebra, reduced Spin(4)
slice, lattice units); `β`, springs, the `Φ` inner product, the HMC kinetic
mass, and the θ-Haar chart are workbench conventions pending physicist review;
NO GeV/pole/VEV; no Phase201/Phase256 contract filled;
`promotedPhysicalMassClaimCount = 0`.
