# Phase XXXV - W/Z Selector Spectrum Independence Audit

## Goal

Phase XXXV follows P34 by checking whether the selected W/Z selector spectra
are independently recomputed operator/eigenvalue results or proxy records that
preserve the upstream ratio by construction.

## Implementation

- Added `WzSelectorSpectrumIndependenceAudit`.
- Added CLI command `audit-wz-selector-spectrum-independence`.
- Added focused tests for:
  - proxy-only invariant selector spectra blocking physical prediction use;
  - solver-backed varying selector spectra completing the audit.
- Generated study artifact:
  - `studies/phase35_wz_selector_spectrum_independence_audit_001/selector_spectrum_independence_audit.json`

## Result

P35 status is `wz-selector-spectrum-independence-blocked`.

Key values:

- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- inspected aligned selector cells: `48`;
- proxy-only selector cells: `48`;
- solver-backed selector cells: `0`;
- selector ratio minimum: `0.8637742965335011`;
- selector ratio maximum: `0.8637742965335012`;
- selector ratio mean: `0.8637742965335007`;
- ratio invariant across selectors: `true`;
- selected source extraction method: `p20-phase12-internal-vector-boson-source-adapter:v1`.

## Interpretation

The current Phase22 selector spectra are not independent selector-specific
operator solves. They are proxy `massLikeValues` records without an operator
bundle, solver method, or mode list. The selected W/Z ratio is invariant across
all selector cells, which is consistent with deterministic rescaling of the
Phase12 source values rather than independent eigenvalue extraction.

This means physical W/Z prediction comparison remains blocked until the
selector spectrum campaign is replaced with real selector-specific
operator/eigenvalue solves.

## Command

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-spectrum-independence \
  --operator-spectrum-path-diagnostic studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --spectra-root studies/phase22_selector_source_spectra_001/spectra \
  --out studies/phase35_wz_selector_spectrum_independence_audit_001/selector_spectrum_independence_audit.json
```

The command returns exit code `1` for the checked-in study because the audit is
fail-closed when selector spectra are proxy-only.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 183/183 tests.
- `jq -e . studies/phase35_wz_selector_spectrum_independence_audit_001/selector_spectrum_independence_audit.json`
  passed.
- `dotnet test GeometricUnity.slnx`
  passed.

## Next Step

Implement a Phase36 selector-backed source spectrum campaign that replaces the
current deterministic offset proxy with selector-specific operator bundle
construction and eigenvalue solving. The new artifacts must include solver
evidence per selector cell before downstream physical W/Z comparison is
unblocked.
