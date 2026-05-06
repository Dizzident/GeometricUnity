# Implementation P125: Source Join Family Metadata Materialization

## Status

Implemented `studies/phase125_source_join_family_metadata_materialization_001`.

## Purpose

Phase124 materialized target-blind source-mode joins for the quality repaired Phase95 modes. Phase125 maps those source modes into the existing Phase12 fermion family table using source background plus mode index.

## Result

Terminal status:

`source-family-metadata-materialized-sector-labels-blocked`

The two quality repaired modes now have `familyId` and `sourceFamilyJoin` metadata in:

`studies/phase125_source_join_family_metadata_materialization_001/output/phase95_l0_source_family_enriched_fermion_modes.json`

This closes the family-join part of the blocker. The remaining blocker is still fermion sector identity: no target-blind `chargeSector`, weak-sector/quantum-number labels, nontrivial chirality, or conjugation pairing are available.

## Next Work

Derive or materialize target-blind fermion charge/weak-sector labels, or create a separate sector identity observable. Only after that should the corrected-operator W/Z transition sweep be rerun under a physical transition rule.
