# Phase 90 - Target-Blind Projected Branch Stability Scan

## Goal

Move beyond the single candidate-0 replay by scanning available Phase12 boson
candidate branch pairings and projected Phase89 fermion mode pairs for
target-blind branch-stable replay values.

## Completed

- Added `studies/phase90_branch_stability_scan_001/scan_projected_branch_replay.py`.
- Reused the existing Phase84 source-backed replay executable as the computation
  engine.
- Scanned:
  - 12 Phase12 boson candidate branch pairings;
  - six projected fermion mode pairs from the first four exact non-null modes;
  - 72 target-blind candidate/pair replay records.
- Wrote:
  - `studies/phase90_branch_stability_scan_001/output/branch_stability_scan_summary.json`;
  - `studies/phase90_branch_stability_scan_001/output/branch_stability_scan_records.json`.

## Result

The scan found a target-blind branch-stable replay candidate under a 10% relative
spread threshold.

Best record:

- candidate: `candidate-3`;
- branch A boson mode: `bg-phase12-bg-a-20260315212202-mode-3`;
- branch B boson mode: `bg-phase12-bg-b-20260315212202-mode-1`;
- fermion pair indices: `[2, 3]`;
- branch A coupling magnitude: `0.0001925404779914997`;
- branch B coupling magnitude: `0.00018752054326654164`;
- mean: `0.00019003051062902067`;
- absolute spread: `0.0000050199347249580664`;
- relative spread: `0.026416467062797246`.

The scan did not use external boson target values.

## Physical Prediction Status

Still blocked, but the branch-spread blocker has changed shape.

Previously, the only replayed branch pair had relative spread
`0.7395196946036089`. Phase90 found a target-blind candidate/pair with relative
spread `0.026416467062797246`, so there is now concrete stability evidence to
ingest.

Remaining blockers:

- identity fermion-space lift still needs a derivation against the
  connection-space gauge quotient;
- projected fermion mode records still carry zero branch/refinement stability
  scores;
- best candidate registry claim class remains `C0_NumericalMode`;
- no external boson value comparison should be made until the stability evidence
  is promoted into the physical gate and the quotient derivation is resolved.

## Verification

- `python3 studies/phase90_branch_stability_scan_001/scan_projected_branch_replay.py`
- `jq -e . studies/phase90_branch_stability_scan_001/output/branch_stability_scan_summary.json`
- `jq -e . studies/phase90_branch_stability_scan_001/output/branch_stability_scan_records.json`

## Next Step

Turn the Phase90 scan result into auditable stability evidence:

1. define a `ProjectedBranchReplayStabilityEvidence` artifact;
2. compute branch stability scores from target-blind relative spread;
3. attach the evidence to the selected projected fermion mode records and
   candidate replay record;
4. update the physical gate to accept this evidence instead of zero default
   mode stability scores;
5. rerun the best candidate-3 replay and require branch/refinement stability
   blockers to clear only from evidence-backed scores.
