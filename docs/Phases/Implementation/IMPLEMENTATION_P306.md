# Implementation P306: Decoupled Charged-Ladder W/Z Row Source Audit

Phase306 tests a narrower question left open by Phase305: whether W and Z
must use the same charged-ladder source definition and fermion transition.
Phase305 found no same-definition W/Z construction that clears the gate stack.
Phase306 keeps the same target-independent source definitions, then allows the
particle-specific W row and Z row to be paired after materialization.

Targets are not used to construct the row candidates. Target values are only
used after the W and Z rows exist, for the raw, stability, and common-scale
diagnostics.

## Result

Terminal status:

`decoupled-charged-ladder-wz-row-source-audit-scaled-near-pass-not-promotable`

The audit reproduced the Phase305 base search:

- `definitionCount=125`;
- `pairCount=132`;
- `assessmentCount=16500`;
- `allRowsRawPassingAssessmentCount=0`;
- `p302ScaledAllRowsRawPassingAssessmentCount=72`;
- `stableAssessmentCount=6`;
- `stableRawCommonAssessmentCount=0`;
- `p302ScaledStableRawCommonAssessmentCount=0`.

Then it decoupled W and Z particle rows:

- `wStableP302ScaledRawRowCount=40`;
- `zStableP302ScaledRawRowCount=12`;
- `decoupledP302ScaledCommonPassingAssessmentCount=96`;
- `decoupledRawCommonPassingAssessmentCount=0`;
- `canFillPhase201WzContract=false`.

The strongest decoupled Phase302-scaled near pass is:

- W row: `charged-ladder-all-axis-neutral-rss-plus:0->6`;
- Z row: `charged-ladder-all-axis-neutral-rss-minus:4->6`;
- `bestDecoupledP302ScaledCommonSpread=0.028734581907060696`;
- `bestDecoupledMinP302ScaledRawToTargetRatio=1.2924117589977038`.

This is the first Phase305-family result where particle-specific W and Z rows
can pass the Phase302-scaled raw, branch-stability, and common-scale numerical
gates when paired independently. It is still not promotable. The unscaled raw
gate remains blocked, and no theorem currently derives the decoupled row
selection or promotes the Phase302 scale as a contract-grade W/Z source law.

## Outputs

- `studies/phase306_decoupled_charged_ladder_wz_row_source_audit_001/output/decoupled_charged_ladder_wz_row_source_audit.json`
- `studies/phase306_decoupled_charged_ladder_wz_row_source_audit_001/output/decoupled_charged_ladder_wz_row_source_audit_summary.json`
