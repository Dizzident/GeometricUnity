# Phase XLII Bundle-Backed W/Z Source Spectra

## Purpose

This study reruns the reduced W/Z source-spectrum campaign with Phase XL
materialized selector-cell bundles as required inputs.

## Inputs

- Campaign spec:
  `studies/phase42_bundle_backed_wz_source_spectra_001/config/source_spectrum_campaign_bundle_backed.json`
- Selector-cell bundles:
  `studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles/selector_cell_bundle_manifest.json`

## Outputs

- Bundle-backed source spectra:
  `studies/phase42_bundle_backed_wz_source_spectra_001/source_spectra`
- Spectrum independence audit:
  `studies/phase42_bundle_backed_wz_source_spectra_001/selector_spectrum_independence_bundle_backed.json`

## Result

The reduced materialized selector slice generated 432 spectra and 12 ready
source candidates. The independence audit now reports 0 proxy-only cells and 36
solver-backed aligned W/Z cells.

Physical W/Z prediction remains blocked because the selected W/Z ratio is still
invariant across selector cells. The next required step is independent
selector-specific eigenvalue extraction from each materialized selector-cell
bundle.
