# Implementation P132: Fermion Sector-Label Derivation Source Gate

## Status

Implemented `studies/phase132_fermion_sector_label_derivation_source_gate_001`.

## Purpose

P131 repaired candidate coverage but left physical sector labels unassigned. P132 audits whether existing target-blind electroweak charge-sector artifacts can serve as the derivation source for those labels.

## Result

Terminal status:

`fermion-sector-label-derivation-source-blocked`

Phase27 has a ready internal mixing convention, but its charge-sector assignments and identity features are keyed to `phase12-candidate-*` boson/vector-source records. They do not match the P131 coverage-repaired fermion candidate or family IDs. Phase46 source spectra are also vector-boson source artifacts and cannot be transferred to fermion labels.

## Next Work

Implement a fermion-specific, target-blind identity feature extractor keyed to the P131 coverage-repaired rows. It must emit:

- `chargeSector`
- `weakSector` or `quantumNumbers`
- `derivationId`
- `externalTargetValuesUsed=false`

After those records exist, rerun the sector-label table gate and then the corrected W/Z transition sweep.
