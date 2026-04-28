# Phase XL W/Z Selector Cell Bundle Materialization

## Purpose

This study materializes selector-cell input bundles from the backed branch,
refinement, and environment axes.

## Inputs

- Selector campaign:
  `studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json`
- Selector axis map:
  `studies/phase37_wz_selector_materialization_map_audit_001/selector_materialization_map_audit.json`
- Environment closure after Phase XXXIX:
  `studies/phase39_wz_environment_background_materialization_001/environment_source_closure_after_phase39.json`

## Outputs

- Cell bundles:
  `cell_bundles/`
- Bundle manifest:
  `cell_bundles/selector_cell_bundle_manifest.json`
- Selector-cell materialization audit:
  `selector_cell_materialization_after_phase40.json`

## Result

Phase XL writes 36/48 selector bundles and materializes 432/576 source-candidate
selector cells. The remaining 144 cells are the Zenodo environment slice, which
still lacks a local solver-backed background record.
