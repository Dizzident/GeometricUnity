# Phase XLI Reduced Solver-Backed W/Z Prediction Campaign

## Purpose

This study defines and validates the solver-backed W/Z selector campaign that
excludes the unbacked Zenodo comparison environment.

## Inputs

- Reduced campaign:
  `config/source_spectrum_campaign_solver_backed.json`
- Source candidates:
  `studies/phase21_source_readiness_campaign_001/source_candidates.json`
- Materialized selector-cell bundles:
  `studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles`

## Outputs

- Full materialization audit:
  `selector_cell_materialization_solver_backed.json`
- Source spectra:
  `source_spectra/`
- Spectrum independence audit:
  `selector_spectrum_independence_solver_backed.json`

## Result

The reduced campaign is fully materialized at the selector-cell input layer:
432/432 cells have the required background record, manifest, omega, A0, geometry,
and environment references.

The subsequent spectra are still proxy-only because the current source-spectrum
runner does not consume the materialized bundles. That is the next blocker before
physical W/Z predictions can be computed from solver-backed selector spectra.
