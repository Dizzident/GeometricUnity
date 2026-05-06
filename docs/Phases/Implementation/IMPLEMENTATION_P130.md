# Implementation P130: Fermion Sector Label Table Gate

## Status

Implemented `studies/phase130_fermion_sector_label_table_gate_001`.

## Purpose

P129 showed that the repaired quality families map to a cluster and candidate, but that no candidate-level artifact provides physical fermion sector labels. P130 converts that blocker into a concrete sector-label table with validation rules.

## Result

Terminal status:

`fermion-sector-label-table-incomplete-blocked`

The table has rows for the repaired quality families, but it is not promotable. Physical labels are unassigned, and the matched registry candidate does not cover every repaired quality family. In particular, `ferm-family-0003` appears in the matched cluster but not in the registry candidate's contributing source IDs.

## Next Work

Repair or derive the fermion sector-label table:

- ensure the matched candidate covers every repaired quality family, or materialize a successor candidate that does
- assign explicit target-blind `chargeSector`
- assign explicit `weakSector` or `quantumNumbers`
- attach a derivation source for each physical label

Only then should the corrected W/Z transition sweep be rerun.
