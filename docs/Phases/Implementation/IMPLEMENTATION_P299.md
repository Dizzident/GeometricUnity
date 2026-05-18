# Phase 299 - Identity-Split Production W/Z Replay Attempt

## Purpose

Phase299 tests whether the Phase27 internal W/Z identity split can be transferred into production analytic replay rows. It replays the identity-selected W candidate and Z candidate through the same source-backed analytic Dirac-variation path used by Phase298.

## Inputs

- Phase27 identity-rule readiness:
  - W: `phase22-phase12-candidate-0` -> `candidate-0`
  - Z: `phase22-phase12-candidate-2` -> `candidate-2`
- Phase12 boson mode vectors:
  - W: `bg-phase12-bg-a-20260315212202-mode-0`, `bg-phase12-bg-b-20260315212202-mode-3`
  - Z: `bg-phase12-bg-a-20260315212202-mode-2`, `bg-phase12-bg-b-20260315212202-mode-0`
- Phase91 promoted fermion modes for the P190 transition `4 -> 6`.
- Phase191 raw-gate diagnostics.
- Phase251, Phase297, and Phase298 blocker state.

## Output

- `studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt.json`
- `studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json`
- Full replay packages under `output/full_replay_packages/`.

## Result

The identity-selected production replay packages build and validate:

- `productionReplayBuiltCount=4`
- `materializationAuditPassedCount=4`
- `evidenceValidatedCount=4`
- `productionInputGapClosedForIdentitySplitCandidates=true`

This still does not complete the W/Z prediction:

- W raw/target ratio: `0.002441034833531895`
- Z raw/target ratio: `0.006344594861823656`
- required raw gate: `0.95`
- `identitySplitRawGatePassed=false`
- `identitySplitStabilityPassed=false`
- `theoremClaimed=false`
- `contractGradeParticleSpecificSourceRowsPresent=false`
- `canEmitWzSourceRows=false`
- `canFillPhase201WzContract=false`

## Decision

Phase299 confirms that the Phase27 identity split can be replayed mechanically as separate source-backed analytic package attempts. It does not make contract-grade W and Z source rows. The remaining requirements are a theorem transferring internal identity labels into direct W/Z source rows, source-derived raw-amplitude closure for both rows, and Phase201/P209 source-lineage fields with gate and stability sidecars.
