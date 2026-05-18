# Implementation P304: Phase27 Sector Aggregate W/Z Source Audit

## Purpose

Phase304 tests the strongest remaining local repair suggested by Phase303: Phase27 may not identify a single W representative, but instead a charged SU(2) sector. The audit checks whether replacing the Phase27 singleton W/Z rows with source-defined charged/neutral sector aggregates can repair the direct W/Z bridge-source path.

## Inputs

- Phase27 W/Z identity-rule readiness.
- Phase27 mode families with charge sectors and dominant SU(2) basis axes.
- Phase282 branch-local variation matrices over all boson candidates.
- Phase299 identity-split W/Z replay.
- Phase302 particle-normalization near-pass.
- Phase303 branch/source normalization audit.
- Phase213 source-lineage blocker matrix.

## Behavior

The audit uses target-independent Phase27 sector definitions:

- Phase27 singleton W/Z identities.
- all Phase27 charged candidates versus all Phase27 neutral candidates.
- charged-plane candidates with dominant axes 0/1 versus neutral axis-2 candidates.
- charged axis-0 candidates versus neutral axis-2 candidates.
- charged axis-1 candidates versus neutral axis-2 candidates.

For each promoted fermion transition and each background, it computes root-sum-square matrix-element norms over the candidate groups, then evaluates raw gates, branch stability, common W/Z scale consistency, and the Phase302 scaled near-pass gate stack. Target values are used only after the sector aggregate is constructed.

## Result

Phase304 is expected to remain non-promotional. The strongest sector aggregate is a real near miss: the all-charged/all-neutral aggregate on the same `4->6` transition clears the Phase302-scaled per-row raw floor, but it still fails the existing branch-stability tolerance and common W/Z scale consistency. No sector aggregate produces stable raw/common W/Z source rows.

## Output

- `studies/phase304_phase27_sector_aggregate_wz_source_audit_001/output/phase27_sector_aggregate_wz_source_audit.json`
- `studies/phase304_phase27_sector_aggregate_wz_source_audit_001/output/phase27_sector_aggregate_wz_source_audit_summary.json`
