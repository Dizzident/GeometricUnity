# Phase XXXIV W/Z Operator Spectrum Path Diagnostic

This study locates where the normalized W/Z ratio mismatch first appears in the
current calculation chain.

## Inputs

- `studies/phase33_wz_canonical_operator_normalization_001/wz_normalization_closure_with_phase33.json`
- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`
- `studies/phase22_selector_source_spectra_001/source_candidates.json`
- `studies/phase22_selector_source_spectra_001/mode_families.json`
- `studies/phase22_selector_source_spectra_001/spectra`

## Output

- `operator_spectrum_path_diagnostic.json`

## Result

The diagnostic is complete. The selected W/Z ratio is identical across
candidate-mode sources, source candidates, and mode families. The same ratio is
also present across all 48 aligned per-point spectrum entries.

## Interpretation

The W/Z miss is upstream of Phase22 aggregation and Phase28/P33 physical
promotion. The next work should inspect mass-like operator/eigenvalue extraction
and candidate mapping in the upstream spectra.
