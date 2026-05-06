# Implementation P93: Parameterized Refinement Replay Geometry

## Objective

Remove the hardcoded `2x2` geometry assumption from the Phase84 source-backed boson replay runner so the same replay path can be used to probe refinement-level geometries.

This phase does not claim a physical boson prediction. It exposes the exact refinement-shaped inputs still missing before the refinement stability gate can be cleared.

## Changes

- Updated `studies/phase84_first_boson_prediction_attempt_001/Program.cs`.
- Added environment-controlled geometry dimensions:
  - `PHASE84_GEOMETRY_ROWS`
  - `PHASE84_GEOMETRY_COLS`
- Added optional explicit boson mode path:
  - `PHASE84_BOSON_MODE_PATH`
- Defaults remain `2x2`, preserving the prior Phase84/Phase91 replay behavior.
- Added `rows` and `cols` to the emitted `geometry` summary.
- Added `bosonModeSourcePath` to the emitted prediction attempt artifact.
- Added positive-integer validation for the geometry variables.

## Validation

Built the replay study without restore:

```bash
dotnet build studies/phase84_first_boson_prediction_attempt_001/Phase84FirstBosonPredictionAttempt.csproj --no-restore --verbosity minimal
```

Result: succeeded with 0 warnings and 0 errors.

Rebuilt after adding `PHASE84_BOSON_MODE_PATH` support:

```bash
dotnet build studies/phase84_first_boson_prediction_attempt_001/Phase84FirstBosonPredictionAttempt.csproj --no-restore --verbosity minimal
```

Result: succeeded with 0 warnings and 0 errors.

Replayed the best target-blind Phase90/Phase91 branch candidate with default geometry:

```bash
PHASE84_BACKGROUND_ID=bg-phase12-bg-a-20260315212202 \
PHASE84_BOSON_MODE_ID=bg-phase12-bg-a-20260315212202-mode-3 \
PHASE84_OUTPUT_DIR=studies/phase93_refinement_geometry_parameterized_replay_001/output/default_2x2 \
PHASE84_FERMION_MODES_PATH=studies/phase91_branch_stability_evidence_promotion_001/output/bg-phase12-bg-a-20260315212202/branch_stability_promoted_fermion_modes.json \
PHASE84_MODE_I=2 \
PHASE84_MODE_J=3 \
dotnet studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/Phase84FirstBosonPredictionAttempt.dll
```

Result:

- `replayTerminalStatus`: `source-backed-analytic-replay-package-built`
- `couplingMagnitude`: `0.0001925404779914997`
- `expectedBosonVectorLength`: `156`
- `expectedFermionEigenvectorLength`: `648`
- Remaining blockers:
  - `fermion mode I refinement stability 0 is below 0.5`
  - `fermion mode J refinement stability 0 is below 0.5`

Probed `4x4` geometry through the same replay path:

```bash
PHASE84_BACKGROUND_ID=bg-phase12-bg-a-20260315212202 \
PHASE84_BOSON_MODE_ID=bg-phase12-bg-a-20260315212202-mode-3 \
PHASE84_OUTPUT_DIR=studies/phase93_refinement_geometry_parameterized_replay_001/output/probe_4x4 \
PHASE84_FERMION_MODES_PATH=studies/phase91_branch_stability_evidence_promotion_001/output/bg-phase12-bg-a-20260315212202/branch_stability_promoted_fermion_modes.json \
PHASE84_MODE_I=2 \
PHASE84_MODE_J=3 \
PHASE84_GEOMETRY_ROWS=4 \
PHASE84_GEOMETRY_COLS=4 \
dotnet studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/Phase84FirstBosonPredictionAttempt.dll
```

Result:

- `replayTerminalStatus`: `source-backed-analytic-replay-package-blocked`
- `expectedBosonVectorLength`: `576`
- `expectedFermionEigenvectorLength`: `1800`
- Closure requirements:
  - `perturbation vector length must be 576`
  - `fermion mode I eigenvector length must be 1800`
  - `fermion mode J eigenvector length must be 1800`

## Outcome

The replay path is now refinement-geometry-aware. The next blocker is no longer hardcoded replay geometry. It is the absence of compatible refinement-level physical inputs:

1. A `4x4` boson perturbation vector with length `576` for the selected candidate.
2. Two `4x4` projected fermion eigenvectors with length `1800`.
3. The same shape-compatible materialization for additional refinement levels so a measured refinement stability score can be promoted without guessing.

## Next Phase

Implement refinement-level projected Dirac materialization for at least `2x2` and `4x4` geometries, using source-backed bosonic states with the exact vector lengths required by the Phase84 replay validator. Do not promote refinement stability until the selected fermion modes are tracked across these refinement-shaped projected Dirac bundles.
