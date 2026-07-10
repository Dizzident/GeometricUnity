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

No physics verdict (T1/T2/T3) is emitted before the fresh production run. The
Stage-0 pre-registration is complete and internally consistent.

**Production launch (later, env-clean):** flip the committed `DefaultMode` in
`Program.cs` from `"preregister"` to `"production"`, rebuild, then

```
dotnet run --no-build -c Release --project \
  studies/phase453_wham_parity_error_model_repair_001/Phase453WhamParityErrorModelRepair.csproj
```

Expected wall time ~2.5-4.5 h from the phase450 per-window rates (identity
constrained 215-462 ms/traj, sd2 constrained 537-542). Two analysis arms
(moving-block bootstrap + full WHAM re-solve; within-window antisymmetrized
`U_odd`) decide the antisymmetry against the calibrated σ99 = 3.16 boundary.

## Asserts to finalize post-production

`scripts/verify_boson_claim_integrity.sh` currently asserts the STAGE-0
pre-registration fields (`mode === "preregister"`,
`verdictKind === "pre-registration-committed"`, localization reproduces,
calibration/smoke complete, junction overlap clears the gate, boundaries). After
the production run these are replaced/augmented with the production asserts:
`mode === "production"`, the T1/T2/T3 `verdictKind`, both-arm agreement
booleans, fresh-tadpole significance, and the phase101 block.

## Framing

Workbench-relative candidate data only (su(2) toy algebra, reduced Spin(4)
slice, lattice units); `β`, springs, the `Φ` inner product, the HMC kinetic
mass, and the θ-Haar chart are workbench conventions pending physicist review;
NO GeV/pole/VEV; no Phase201/Phase256 contract filled;
`promotedPhysicalMassClaimCount = 0`.
