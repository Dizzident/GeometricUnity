# Phase XXXV - W/Z Selector Spectrum Independence Audit

This study audits whether the selected Phase22 W/Z selector spectra contain
independent selector-specific solver evidence.

Artifact:

- `selector_spectrum_independence_audit.json`

Result:

- terminal status: `wz-selector-spectrum-independence-blocked`;
- aligned selected W/Z selector cells inspected: `48`;
- proxy-only cells: `48`;
- solver-backed cells: `0`;
- selected W/Z ratio is invariant across selector cells.

Conclusion:

The checked-in selector spectra are proxy `massLikeValues` records, not
independent operator/eigenvalue solves. Physical W/Z prediction comparison
remains blocked until selector-specific operator bundles and spectra are
computed.

Reproduction:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-spectrum-independence \
  --operator-spectrum-path-diagnostic studies/phase34_wz_operator_spectrum_path_diagnostic_001/operator_spectrum_path_diagnostic.json \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --spectra-root studies/phase22_selector_source_spectra_001/spectra \
  --out studies/phase35_wz_selector_spectrum_independence_audit_001/selector_spectrum_independence_audit.json
```
