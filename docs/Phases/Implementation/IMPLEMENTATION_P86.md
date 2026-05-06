# Phase 86 - Branch-Pair Boson Replay

## Goal

Push the first boson prediction attempt beyond a single background by replaying both branch representatives of Phase12 `candidate-0`.

## Completed

- Parameterized the Phase84 runner by background and boson mode.
- Extended Phase85 exact residual repair to produce exact non-null fermion modes for both Phase12 backgrounds:
  - `bg-phase12-bg-a-20260315212202`
  - `bg-phase12-bg-b-20260315212202`
- Replayed `candidate-0` branch representatives:
  - background A mode `bg-phase12-bg-a-20260315212202-mode-0`
  - background B mode `bg-phase12-bg-b-20260315212202-mode-3`
- Wrote `studies/phase86_branch_pair_boson_replay_001/output/branch_pair_boson_replay_summary.json`.

## Result

Both branch replays build source-backed analytic packages:

- A coupling magnitude: `0.0001371411566683907`
- B coupling magnitude: `0.0002980616976126623`
- branch mean: `0.0002176014271405265`
- branch relative spread: `0.7395196946036089`

## Physical Prediction Status

Still blocked. This phase makes the block clearer:

- replayed matrix-element machinery works on both branch representatives;
- exact fermion residuals are repaired on both branches;
- branch spread remains large;
- the candidate registry already marks `candidate-0` as `C0_NumericalMode` with `BranchFragility`;
- both underlying Phase12 Dirac bundles still have `gaugeReductionApplied: false`.

## Next Step

The next required work is to generate a gauge-reduced Phase12 branch family and repeat the branch-pair replay. Without gauge-reduced Dirac bundles and a stable candidate claim class, any numerical value remains a blocked internal replay value, not a correct physical boson prediction.
