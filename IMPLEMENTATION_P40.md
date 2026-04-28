# Phase XL - W/Z Selector Cell Bundle Materialization

## Goal

Phase XXXIX produced solver-backed environment records for the three
local-geometry environment selectors, but the Phase XXXVI selector-cell audit
still saw no full cell-level bundles. Phase XL composes backed branch,
refinement, and environment axes into explicit selector-cell input bundles.

## Implementation

Added `WzSelectorCellBundleMaterializer` and CLI command:

```bash
dotnet run --project apps/Gu.Cli -- materialize-wz-selector-cell-bundles \
  --spec studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json \
  --selector-map studies/phase37_wz_selector_materialization_map_audit_001/selector_materialization_map_audit.json \
  --environment-closure studies/phase39_wz_environment_background_materialization_001/environment_source_closure_after_phase39.json \
  --out-dir studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles
```

The materializer writes one reusable bundle per
branch/refinement/environment selector combination. It does not multiply bundles
by source candidate because source candidates are separate spectrum-campaign
inputs.

The materializer is fail-closed:

- branch selectors must be source-backed by Phase XXXVII;
- refinement selectors must be source-backed by Phase XXXVII;
- environment selectors must be background-backed by Phase XXXIX.

## Result

Bundle manifest:

- `studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles/selector_cell_bundle_manifest.json`

Observed result:

- expected selector-cell bundles: 48;
- written bundles: 36;
- skipped bundles: 12;
- terminal status: `selector-cell-bundles-partial`;
- blocker: `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1` is not background-backed.

## Phase XXXVI Audit After Phase XL

Reran:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-cell-materialization \
  --spec studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json \
  --source-candidates studies/phase21_source_readiness_campaign_001/source_candidates.json \
  --artifact-roots studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles \
  --out studies/phase40_wz_selector_cell_bundle_materialization_001/selector_cell_materialization_after_phase40.json
```

Materialization improved from 0/576 to:

- total selector cells: 576;
- materialized selector cells: 432;
- missing selector cells: 144.

The missing 144 cells are the Zenodo environment slice:

- 12 source candidates;
- 4 branch variants;
- 3 refinement levels;
- 1 unbacked environment.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 189/189 tests.
- `dotnet test GeometricUnity.slnx`
  passed.
- `jq -e . studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles/selector_cell_bundle_manifest.json`
  passed.
- `jq -e . studies/phase40_wz_selector_cell_bundle_materialization_001/selector_cell_materialization_after_phase40.json`
  passed.

## Next Step

Resolve the remaining Zenodo environment blocker. There are two honest paths:

- produce a local GU mesh/field conversion for
  `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1`, then run
  `solve-backgrounds` and regenerate Phase XL bundles; or
- define a reduced selector campaign that excludes Zenodo for solver-backed W/Z
  prediction work, while preserving Zenodo as an external comparison-only
  benchmark.

Physical W/Z predictions should remain blocked for the full four-environment
campaign until the Zenodo selector is backed or explicitly removed from the
solver-backed prediction campaign.
