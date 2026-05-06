# Phase XXXIV - W/Z Operator Spectrum Path Diagnostic

## Goal

Phase XXXIV follows P33 by locating where the normalized W/Z mismatch first
appears. Since the canonical operator normalization scale is now derived as
`1`, the remaining failure must come from the operator spectrum/eigenvalue path
or from missing higher-order/continuum corrections.

P34 compares the selected W/Z ratio across:

- candidate-mode source bridge records;
- source-candidate records;
- selector-aware mode-family records;
- aligned per-point spectrum files.

## Implementation

- Added `WzOperatorSpectrumPathDiagnostic`.
- Added CLI command `diagnose-wz-operator-spectrum-path`.
- Added focused tests for:
  - all layers agreeing and pointing upstream to per-point spectra;
  - missing spectra roots blocking the diagnostic.
- Generated study artifact:
  - `studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json`

## Result

P34 status is `wz-operator-spectrum-path-diagnostic-complete`.

Key values:

- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- target value: `0.88136`;
- required scale to target: `1.0203591418928235`;
- candidate-mode-source ratio: `0.8637742965335007`;
- source-candidate ratio: `0.8637742965335007`;
- mode-family ratio: `0.8637742965335007`;
- aligned spectrum point count: `48`;
- spectrum ratio minimum: `0.8637742965335011`;
- spectrum ratio maximum: `0.8637742965335012`;
- spectrum ratio mean: `0.8637742965335007`;
- first mismatch layer: none.

## Interpretation

The mismatch is already present in the per-point spectrum/mode eigenvalues,
before Phase22 aggregation, physical promotion, or normalization. The Phase22
adapter and family aggregation preserve the upstream ratio rather than creating
the miss.

The next phase should inspect the upstream mass-like operator/eigenvalue
extraction itself: operator definition, eigenvalue selector, candidate mode
mapping, missing correction term, or continuum/higher-order correction.

## Command

```bash
dotnet run --project apps/Gu.Cli -- diagnose-wz-operator-spectrum-path \
  --normalization-closure studies/phase33_wz_canonical_operator_normalization_001/wz_normalization_closure_with_phase33.json \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --source-candidates studies/phase22_selector_source_spectra_001/source_candidates.json \
  --mode-families studies/phase22_selector_source_spectra_001/mode_families.json \
  --spectra-root studies/phase22_selector_source_spectra_001/spectra \
  --out studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json
```

## Validation

Completed:

- `jq -e . studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json`
  passed.
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 181/181 tests.
- `dotnet test GeometricUnity.slnx`
  passed.
