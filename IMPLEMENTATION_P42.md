# Phase XLII - Bundle-Backed W/Z Source Spectra

## Goal

Phase XLI proved that the reduced W/Z selector campaign is fully materialized
at the selector-cell input layer, but the source-spectrum runner still emitted
proxy-only `massLikeValues` spectra. Phase XLII wires the source-spectrum runner
to the Phase XL materialized selector-cell bundle manifest.

## Implementation

Updated:

- `apps/Gu.Cli/Program.cs`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceMatrixCampaign.cs`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceSpectrum.cs`
- `src/Gu.Phase5.Reporting/WzSelectorSpectrumIndependenceAudit.cs`
- `tests/Gu.Phase5.QuantitativeValidation.Tests/InternalVectorBosonSourceSpectrumTests.cs`

The source-spectrum campaign now accepts:

```bash
--cell-bundles <selector_cell_bundle_manifest.json|dir>
```

The campaign spec can also provide:

```json
"selectorCellBundleManifestPath": "..."
```

When selector-cell bundles are supplied, the runner:

- loads only written materialized selector-cell bundle records;
- fails closed if any requested branch/refinement/environment cell is missing;
- adds selector-cell bundle IDs and paths to manifest entries and mode records;
- emits `operatorBundleId`, `solverMethod`, `operatorType`, `eigenvalues`, and
  `modeRecords` in each spectrum record so downstream audits can distinguish
  bundle-backed spectra from proxy-only spectra.

The generated numeric values remain selector-deterministic and refinement
invariant. They are now bundle-backed campaign artifacts, but they are not yet
independent physical W/Z eigenvalue predictions.

## Campaign

Added:

- `studies/phase42_bundle_backed_wz_source_spectra_001/config/source_spectrum_campaign_bundle_backed.json`

The campaign keeps the reduced Phase XLI selector scope:

- 12 source candidates;
- 4 branch variants;
- 3 refinement levels;
- 3 background-backed environments.

It consumes:

- `studies/phase40_wz_selector_cell_bundle_materialization_001/cell_bundles/selector_cell_bundle_manifest.json`

## Source Spectrum Result

Command:

```bash
dotnet run --project apps/Gu.Cli -- run-internal-vector-boson-source-spectrum-campaign \
  --spec studies/phase42_bundle_backed_wz_source_spectra_001/config/source_spectrum_campaign_bundle_backed.json \
  --out-dir studies/phase42_bundle_backed_wz_source_spectra_001/source_spectra
```

Result:

- spectra: 432;
- mode families: 12;
- terminal status: `candidate-source-ready`;
- ready candidates: 12;
- selector-cell bundles: Phase XL bundle manifest.

## Spectrum Independence Audit

Command:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-spectrum-independence \
  --operator-spectrum-path-diagnostic studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json \
  --candidate-mode-sources studies/phase42_bundle_backed_wz_source_spectra_001/source_spectra/candidate_mode_sources.json \
  --spectra-root studies/phase42_bundle_backed_wz_source_spectra_001/source_spectra/spectra \
  --out studies/phase42_bundle_backed_wz_source_spectra_001/selector_spectrum_independence_bundle_backed.json
```

Result:

- terminal status: `wz-selector-spectrum-independence-blocked`;
- inspected aligned cells: 36;
- proxy-only cells: 0;
- solver-backed cells: 36;
- remaining blocker: selected W/Z ratio is invariant across selector cells.

Interpretation: Phase XLII clears the proxy-only blocker for the reduced,
materialized selector slice. The remaining blocker is independence: the selected
W/Z ratio is still invariant, consistent with deterministic rescaling rather
than independent selector-specific eigenvalue extraction.

## Validation

Completed:

- `jq -e . studies/phase42_bundle_backed_wz_source_spectra_001/config/source_spectrum_campaign_bundle_backed.json`
  passed.
- `jq -e . studies/phase42_bundle_backed_wz_source_spectra_001/source_spectra/spectra_manifest.json`
  passed.
- `jq -e . studies/phase42_bundle_backed_wz_source_spectra_001/selector_spectrum_independence_bundle_backed.json`
  passed.
- `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj`
  passed: 99/99.
- `dotnet test GeometricUnity.slnx` passed.

## Next Step

Replace the bundle-backed deterministic source-matrix rescaling with a real
selector-specific eigenvalue extraction over the materialized cell bundle inputs.
The next phase should define and implement an operator assembly/solve path that
uses each selector-cell bundle as input and produces independently varying W and
Z spectra suitable for physical comparison.
