# Phase XLIII - Selector-Specific Eigen W/Z Source Spectra

## Goal

Phase XLII cleared the proxy-only blocker but still produced an invariant W/Z
ratio across selector cells. Phase XLIII replaces deterministic bundle-backed
rescaling with a selector/candidate-specific generalized eigenvalue solve.

## Implementation

Updated:

- `src/Gu.Phase5.QuantitativeValidation/Gu.Phase5.QuantitativeValidation.csproj`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceMatrixCampaign.cs`
- `src/Gu.Phase5.Reporting/WzSelectorSpectrumIndependenceAudit.cs`
- `tests/Gu.Phase5.QuantitativeValidation.Tests/InternalVectorBosonSourceSpectrumTests.cs`

The source-spectrum runner now uses the existing Phase III spectral stack when
selector-cell bundles are supplied:

- builds a small selector/candidate-specific `LinearizedOperatorBundle`;
- solves it through `EigensolverPipeline` with the explicit dense generalized
  eigensolver;
- emits the resulting `SpectrumBundle`, eigenvalues, modes, operator bundle ID,
  solver method, and operator type into each spectrum artifact.

This is still an internal source extraction path, not a final physical W/Z mass
prediction. The important Phase XLIII advance is that selected W/Z values now
come from selector-specific eigenvalue solves and are no longer invariant across
selector cells.

## Campaign

Added:

- `studies/phase43_selector_eigen_wz_source_spectra_001/config/source_spectrum_campaign_selector_eigen.json`

The campaign consumes the Phase XL materialized selector-cell bundle manifest
and keeps the reduced Phase XLI/XLII scope:

- 12 source candidates;
- 4 branch variants;
- 3 refinement levels;
- 3 background-backed environments.

## Source Spectrum Result

Command:

```bash
dotnet run --project apps/Gu.Cli -- run-internal-vector-boson-source-spectrum-campaign \
  --spec studies/phase43_selector_eigen_wz_source_spectra_001/config/source_spectrum_campaign_selector_eigen.json \
  --out-dir studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra
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
  --candidate-mode-sources studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/candidate_mode_sources.json \
  --spectra-root studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/spectra \
  --out studies/phase43_selector_eigen_wz_source_spectra_001/selector_spectrum_independence_selector_eigen.json
```

Result:

- terminal status: `wz-selector-spectrum-independent-evidence-present`;
- inspected aligned cells: 36;
- proxy-only cells: 0;
- solver-backed cells: 36;
- ratio invariant across selectors: false;
- ratio min: 0.8617415263399019;
- ratio max: 0.8654919922785442;
- ratio mean: 0.8631579492922258;
- closure requirements: none.

Interpretation: Phase XLIII clears the selector-spectrum independence blocker
for the reduced materialized slice. The next blocker is no longer proxy-only or
invariant selector spectra; it is physical calibration and comparison of the
selector-eigen W/Z candidates against real W/Z target values.

## Validation

Completed:

- `jq -e . studies/phase43_selector_eigen_wz_source_spectra_001/config/source_spectrum_campaign_selector_eigen.json`
  passed.
- `jq -e . studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/spectra_manifest.json`
  passed.
- `jq -e . studies/phase43_selector_eigen_wz_source_spectra_001/selector_spectrum_independence_selector_eigen.json`
  passed.
- `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj`
  passed: 99/99.
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed: 189/189.
- `dotnet test GeometricUnity.slnx`
  was attempted and hit the Phase IV CUDA stub timing assertion
  `DiracBenchmarkRunner_SpeedupRatio_NearOneForStub`; the isolated rerun passed:
  `dotnet test tests/Gu.Phase4.CudaAcceleration.Tests/Gu.Phase4.CudaAcceleration.Tests.csproj --filter DiracBenchmarkRunner_SpeedupRatio_NearOneForStub`.

## Next Step

Run the downstream W/Z identity, normalization, prediction promotion, and
target-comparison pipeline using the Phase XLIII selector-eigen source spectra.
That should show whether the newly independent source spectra can be converted
into physical W/Z predictions or whether calibration/identity selection remains
blocked.
