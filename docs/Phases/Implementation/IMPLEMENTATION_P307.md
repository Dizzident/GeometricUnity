# Implementation P307: Target-Independent Decoupled W/Z Row Selection Law Audit

Phase307 tests whether the Phase306 decoupled charged-ladder near-pass can be
selected by predeclared source-side rules, rather than only discovered by
post-hoc target comparison.

The audit keeps the Phase305/Phase306 charged-ladder row materialization:

- `T+ = (axis0 + i axis1) / sqrt(2)`;
- `T- = (axis0 - i axis1) / sqrt(2)`;
- singleton, coherent, and root-sum-square charged/neutral source definitions.

Targets are not used to select rows. The selectors use only branch stability,
source magnitudes, source magnitude common-spread, and optionally the Phase302
W/Z particle scales. Target values are used only after selection to evaluate
raw, stability, and common-scale gates.

## Result

Terminal status:

`target-independent-decoupled-wz-row-selection-law-audit-scaled-selector-not-promotable`

The audit materialized eight target-independent selectors:

- `selectionLawCount=8`;
- `rawStableCommonSelectionLawCount=0`;
- `p302ScaledStableCommonSelectionLawCount=1`;
- `selectionLawCanFillPhase201WzContractCount=0`.

One predeclared selector can choose a Phase302-scaled numerical near-pass:

- `bestP302ScaledStableCommonSelectionLaw=p302-scaled-max-min-magnitude`;
- the selected row pair clears the Phase302-scaled numerical gate after
  selection.

This still cannot be promoted. No selector clears the unscaled raw/common gate,
and the only selector-side repair depends on Phase302 particle scales, which
still lack a theorem-backed source-lineage contract.

## Outputs

- `studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit.json`
- `studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json`
