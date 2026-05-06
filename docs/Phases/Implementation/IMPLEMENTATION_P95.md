# Implementation P95: Target-Blind Refinement Mode Matching

## Objective

Match the selected Phase91 `2x2` fermion pair into the Phase94 exact `2x2` and `4x4`
refinement mode bundles without using external boson targets.

## Changes

- Added `studies/phase95_target_blind_refinement_mode_matching_001/match_refinement_modes.py`.
- The matcher uses only internal Dirac-spectrum structure:
  - same-sign near-null eigenvalue family;
  - nearest absolute eigenvalue scale among exact projected non-null modes;
  - same-length `2x2` subspace overlap as a check where vector dimensions permit it.
- It writes:
  - `output/target_blind_refinement_mode_matching_evidence.json`;
  - `output/target_blind_refinement_mode_matching_summary.json`;
  - replay-ready matched Phase94 `2x2` and `4x4` fermion mode bundles.

## Validation

```bash
python3 studies/phase95_target_blind_refinement_mode_matching_001/match_refinement_modes.py
jq -e . studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_evidence.json
jq -e . studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_summary.json
PHASE84_BACKGROUND_ID=bg-phase12-bg-a-20260315212202 \
PHASE84_BOSON_MODE_ID=bg-phase12-bg-a-20260315212202-mode-3 \
PHASE84_OUTPUT_DIR=studies/phase95_target_blind_refinement_mode_matching_001/output/replay_probe_4x4 \
PHASE84_FERMION_MODES_PATH=studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l1_4x4_refinement_matched_fermion_modes.json \
PHASE84_MODE_I=0 \
PHASE84_MODE_J=2 \
PHASE84_GEOMETRY_ROWS=4 \
PHASE84_GEOMETRY_COLS=4 \
dotnet studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/Phase84FirstBosonPredictionAttempt.dll
dotnet test tests/Gu.Phase4.Dirac.Tests/Gu.Phase4.Dirac.Tests.csproj --no-restore --verbosity minimal
dotnet build studies/phase84_first_boson_prediction_attempt_001/Phase84FirstBosonPredictionAttempt.csproj --no-restore --verbosity minimal
```

Result:

- Phase91 selected pair: modes `002` and `003`, both in the negative near-null family.
- Phase94 `2x2` matched pair: modes `000` and `003`.
- Phase94 `4x4` matched pair: modes `000` and `002`.
- refinement stability score promoted from internal eigenvalue-family continuity: `0.8250944968993068`.
- The `4x4` replay probe selects the matched Phase94 fermion modes and no longer reports fermion vector-length or fermion refinement-stability blockers; its remaining closure requirement is `perturbation vector length must be 576`.
- Focused Dirac tests pass: 94 passed, 0 failed.
- Phase84 replay study build succeeds with 0 warnings and 0 errors.

## Outcome

The target-blind selected-fermion tracking blocker is closed for the available
Phase94 exact refinement bundles.

## Remaining Blockers

- The identity fermion-space lift still needs a derivation against the connection-space gauge quotient.
- A source-backed refinement boson `modeVector` with replay-compatible length `576` is still missing.
