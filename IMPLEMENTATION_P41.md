# Phase XLI - Reduced Solver-Backed W/Z Prediction Campaign

## Goal

Phase XL materialized 432/576 selector cells. The remaining 144 cells are the
Zenodo environment slice, which is comparison-only until a local GU mesh/field
conversion exists. Phase XLI creates a reduced solver-backed campaign that
excludes Zenodo so the solver-backed slice can advance independently.

## Campaign

Added:

- `studies/phase41_solver_backed_wz_prediction_campaign_001/config/source_spectrum_campaign_solver_backed.json`

It keeps:

- 12 source candidates;
- 4 branch variants;
- 3 refinement levels;
- 3 solver-backed environments:
  - `env-toy-2d-trivial`;
  - `env-structured-4x4`;
  - `env-imported-repo-benchmark`.

It excludes:

- `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1`.

Zenodo remains an external-comparison-only benchmark until its local GU
mesh/field conversion exists.

## Materialization Audit

Command:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-cell-materialization \
  --spec studies/phase41_solver_backed_wz_prediction_campaign_001/config/source_spectrum_campaign_solver_backed.json \
  --source-candidates studies/phase21_source_readiness_campaign_001/source_candidates.json \
  --artifact-roots studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles \
  --out studies/phase41_solver_backed_wz_prediction_campaign_001/selector_cell_materialization_solver_backed.json
```

Result:

- terminal status: `selector-cells-materialized`;
- total cells: 432;
- materialized cells: 432;
- missing cells: 0.

This is the first fully materialized W/Z selector campaign in the repo.

## Source Spectrum Campaign

Command:

```bash
dotnet run --project apps/Gu.Cli -- run-internal-vector-boson-source-spectrum-campaign \
  --spec studies/phase41_solver_backed_wz_prediction_campaign_001/config/source_spectrum_campaign_solver_backed.json \
  --out-dir studies/phase41_solver_backed_wz_prediction_campaign_001/source_spectra
```

Result:

- spectra: 432;
- mode families: 12;
- terminal status: `candidate-source-ready`;
- ready candidates: 12.

## Spectrum Independence Audit

Command:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-spectrum-independence \
  --operator-spectrum-path-diagnostic studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json \
  --candidate-mode-sources studies/phase41_solver_backed_wz_prediction_campaign_001/source_spectra/candidate_mode_sources.json \
  --spectra-root studies/phase41_solver_backed_wz_prediction_campaign_001/source_spectra/spectra \
  --out studies/phase41_solver_backed_wz_prediction_campaign_001/selector_spectrum_independence_solver_backed.json
```

Result:

- terminal status: `wz-selector-spectrum-independence-blocked`;
- inspected aligned cells: 36;
- proxy-only cells: 36;
- solver-backed cells: 0.

Interpretation: the reduced campaign is fully materialized at the selector-cell
input layer, but the source-spectrum runner still does not consume those
materialized bundles. It continues to emit proxy/synthetic selector spectra.

## Validation

Completed:

- `jq -e . studies/phase41_solver_backed_wz_prediction_campaign_001/config/source_spectrum_campaign_solver_backed.json`
  passed.
- `jq -e . studies/phase41_solver_backed_wz_prediction_campaign_001/selector_cell_materialization_solver_backed.json`
  passed.
- `jq -e . studies/phase41_solver_backed_wz_prediction_campaign_001/source_spectra/spectra_manifest.json`
  passed.
- `jq -e . studies/phase41_solver_backed_wz_prediction_campaign_001/selector_spectrum_independence_solver_backed.json`
  passed.
- `dotnet test GeometricUnity.slnx` passed.

## Next Step

Update the source-spectrum campaign runner to require materialized selector-cell
bundles and derive source spectra from those bundle records. Physical W/Z
prediction remains blocked until the generated spectra are tied to solver-backed
bundle inputs rather than proxy selector offsets.
