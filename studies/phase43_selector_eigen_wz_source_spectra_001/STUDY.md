# Phase XLIII Selector-Specific Eigen W/Z Source Spectra

## Purpose

This study reruns the reduced W/Z selector-source campaign with materialized
selector-cell bundles and selector/candidate-specific generalized eigenvalue
solves.

## Inputs

- Campaign spec:
  `studies/phase43_selector_eigen_wz_source_spectra_001/config/source_spectrum_campaign_selector_eigen.json`
- Selector-cell bundles:
  `studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles/selector_cell_bundle_manifest.json`

## Outputs

- Selector-eigen source spectra:
  `studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra`
- Spectrum independence audit:
  `studies/phase43_selector_eigen_wz_source_spectra_001/selector_spectrum_independence_selector_eigen.json`

## Result

The reduced materialized selector slice generated 432 spectra and 12 ready
source candidates. The independence audit reports 36 solver-backed aligned W/Z
cells, 0 proxy-only cells, and a non-invariant selected W/Z ratio.

Physical W/Z prediction can now advance to calibration and target comparison.
