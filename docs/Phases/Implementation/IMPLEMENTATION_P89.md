# Phase 89 - Phase12 Projected Fermion Dirac Replay

## Goal

Apply the Phase88 projected-Dirac path to the persisted Phase12 A/B fermion
bundles and rerun the source-backed boson replay with gauge-reduced fermion-mode
metadata.

## Completed

- Added `studies/phase89_phase12_fermion_projected_dirac_001/project_phase12_dirac.py`.
- Materialized projected Dirac artifacts for both Phase12 branch backgrounds:
  - `bg-phase12-bg-a-20260315212202`;
  - `bg-phase12-bg-b-20260315212202`.
- Wrote compact identity fermion-space projector artifacts for both branches.
- Wrote projected Dirac bundle metadata and projected matrix artifacts.
- Exact-solved the projected matrices into non-null fermion mode bundles with:
  - `gaugeReductionApplied: true`;
  - projected residuals at machine precision.
- Replayed the selected candidate-0 A/B boson branches against the projected
  exact fermion modes.

## Result

The replay no longer blocks on `fermion mode was not gauge-reduced`.

Branch A:

- boson mode: `bg-phase12-bg-a-20260315212202-mode-0`;
- coupling magnitude: `0.0001371411566683907`;
- remaining blockers: branch/refinement stability only.

Branch B:

- boson mode: `bg-phase12-bg-b-20260315212202-mode-3`;
- coupling magnitude: `0.0002980616976126623`;
- remaining blockers: branch/refinement stability only.

Branch spread remains high:

- mean: `0.0002176014271405265`;
- absolute spread: `0.0001609205409442716`;
- relative spread: `0.7395196946036089`.

## Physical Prediction Status

Still blocked. This phase closes the executable gauge-reduction metadata blocker
for replay inputs, but it uses the identity fermion-space lift. That is useful
for artifact plumbing and for proving the replay gate can consume reduced
metadata, but it does not prove a nontrivial connection-gauge quotient on
fermion states.

The current blockers are now narrower:

- identity fermion-space lift needs a derivation against the connection-space
  gauge quotient;
- `candidate-0` remains branch fragile;
- branch coupling relative spread is still too large;
- selected fermion modes have no branch/refinement stability evidence.

## Verification

- `python3 studies/phase89_phase12_fermion_projected_dirac_001/project_phase12_dirac.py`
- `dotnet build studies/phase84_first_boson_prediction_attempt_001/Phase84FirstBosonPredictionAttempt.csproj --no-restore --verbosity minimal`
- ran Phase84 replay executable for A and B projected mode inputs.

`dotnet run` restore for the Phase84 study project failed without surfacing an
error, but the project builds with `--no-restore` and the built executable runs
successfully.

## Next Step

Replace the identity lift with a justified stability-producing selection path:

1. compute coupling values across the available Phase12 candidate modes and
   fermion mode pairs;
2. rank candidates by branch spread using source-backed replay only;
3. select the most branch-stable non-target-tuned candidate/pair;
4. require the branch/refinement gate to use this stability evidence instead of
   zero default scores.
