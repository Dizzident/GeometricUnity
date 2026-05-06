# Phase 91 - Branch Stability Evidence Promotion

## Goal

Turn the Phase90 target-blind branch-stability scan into auditable mode-level
stability evidence without fabricating refinement evidence.

## Completed

- Added `studies/phase91_branch_stability_evidence_promotion_001/promote_branch_stability_evidence.py`.
- Created `projected_branch_stability_evidence.json` for the Phase90 best replay:
  - candidate: `candidate-3`;
  - fermion mode pair: `[2, 3]`;
  - branch relative spread: `0.026416467062797246`;
  - branch stability score: `0.9735835329372028`;
  - formula: `max(0, min(1, 1 - branchCouplingRelativeSpread))`;
  - external targets used: `false`.
- Wrote promoted A/B projected fermion mode bundles where only the selected
  modes receive the evidence-backed branch stability score.
- Reran candidate-3 replay using the promoted mode bundles.

## Result

The local physical gate no longer reports branch-stability blockers for the
selected candidate-3 replay.

Branch A:

- boson mode: `bg-phase12-bg-a-20260315212202-mode-3`;
- coupling magnitude: `0.0001925404779914997`;
- remaining blockers:
  - `fermion mode I refinement stability 0 is below 0.5`;
  - `fermion mode J refinement stability 0 is below 0.5`.

Branch B:

- boson mode: `bg-phase12-bg-b-20260315212202-mode-1`;
- coupling magnitude: `0.00018752054326654164`;
- remaining blockers:
  - `fermion mode I refinement stability 0 is below 0.5`;
  - `fermion mode J refinement stability 0 is below 0.5`.

Closed blockers:

- `fermion mode I branch stability ... below 0.5`;
- `fermion mode J branch stability ... below 0.5`.

## Physical Prediction Status

Still blocked. This phase intentionally does not promote refinement stability,
because Phase90 varied branches but did not vary refinement level.

Remaining blockers:

- selected projected fermion modes still need refinement stability evidence;
- identity fermion-space lift still needs derivation against the connection-space
  gauge quotient.

## Verification

- `python3 studies/phase91_branch_stability_evidence_promotion_001/promote_branch_stability_evidence.py`
- `jq -e . studies/phase91_branch_stability_evidence_promotion_001/output/projected_branch_stability_evidence.json`
- `jq -e . studies/phase91_branch_stability_evidence_promotion_001/output/branch_stability_promotion_summary.json`

## Next Step

Create a refinement-stability evidence path for the selected candidate-3 replay:

1. find or generate refinement-varied projected fermion artifacts for the same
   candidate/pair;
2. compute source-backed replay spread across refinement levels;
3. promote refinement stability only if the evidence is target-blind and below
   the configured spread threshold;
4. rerun the physical gate and require only the quotient-derivation blocker to
   remain.
