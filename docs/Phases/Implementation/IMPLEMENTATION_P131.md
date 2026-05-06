# Implementation P131: Sector-Label Candidate Coverage Repair

## Status

Implemented `studies/phase131_sector_label_candidate_coverage_repair_001`.

## Purpose

P130 materialized the sector-label table but found two blockers: incomplete candidate coverage and missing physical sector labels. P131 resolves the structural coverage blocker by creating a target-blind successor candidate that covers every repaired quality family.

## Result

Terminal status:

`sector-label-candidate-coverage-repaired-labels-blocked`

Candidate coverage is repaired for both quality rows. Physical labels remain unassigned, by design, because no source artifact currently derives target-blind fermion `chargeSector`, `weakSector`, or `quantumNumbers`.

## Next Work

Derive or materialize physical sector labels for every coverage-repaired row:

- `chargeSector`
- `weakSector` or `quantumNumbers`
- `derivationSource`

Only after those labels exist should the corrected W/Z transition sweep be rerun.
