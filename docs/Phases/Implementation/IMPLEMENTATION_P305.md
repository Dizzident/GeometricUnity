# Implementation P305: Phase27 Charged-Ladder Operator W/Z Source Audit

Phase305 tests the canonical charged-current repair left open by Phase304.
Phase304 used sector norms; Phase305 instead evaluates charged ladder
operators on the same branch-local matrix elements:

- `T+ = (axis0 + i axis1) / sqrt(2)`;
- `T- = (axis0 - i axis1) / sqrt(2)`;
- neutral axis-2 coherent sums, neutral axis-2 root-sum-square norms, and
  singleton neutral candidates.

The phase keeps target masses out of source construction. Targets enter only
after each source definition and fermion transition are materialized, for the
same raw-scale, branch-stability, and common W/Z scale gates used by the
previous W/Z source-law audits.

## Result

Terminal status:

`phase27-charged-ladder-operator-wz-source-audit-no-stable-common-source`

The audit evaluated:

- 5 charged axis-0 candidates;
- 4 charged axis-1 candidates;
- 3 neutral axis-2 candidates;
- 125 charged-ladder source definitions;
- 132 promoted ordered fermion transitions;
- 16,500 total assessments.

It found no promotable W/Z source construction:

- `allRowsRawPassingAssessmentCount=0`;
- `p302ScaledAllRowsRawPassingAssessmentCount=72`;
- `stableAssessmentCount=6`;
- `stableRawCommonAssessmentCount=0`;
- `p302ScaledStableRawCommonAssessmentCount=0`;
- `canFillPhase201WzContract=false`.

The most branch-stable definition is
`charged-ladder-all-axis-neutral-coherent-plus:1->9` with
`bestMaxParticleRelativeSpread=0.02448213567533975`, but it fails raw and
common-scale gates. The strongest Phase302-scaled raw near miss is
`charged-ladder-pair-candidate-10-plus-candidate-7-z-candidate-2:4->6` with
`bestP302ScaledRawAssessmentMaxSpread=0.051076179232408876`, just above the
`0.05` stability tolerance, and it still fails common W/Z scale consistency.

Phase305 therefore closes the direct local repair route of replacing Phase304
sector norms with canonical charged-current ladder combinations. The remaining
artifact is still a theorem-backed W/Z direct bridge-source law with
target-independent source rows and normalization.

## Outputs

- `studies/phase305_phase27_charged_ladder_operator_wz_source_audit_001/output/phase27_charged_ladder_operator_wz_source_audit.json`
- `studies/phase305_phase27_charged_ladder_operator_wz_source_audit_001/output/phase27_charged_ladder_operator_wz_source_audit_summary.json`
