# Phase XXXVI - Selector-Backed W/Z Spectrum Campaign

This study records the implementation plan needed to replace the Phase22
selector proxy spectra with solver-backed selector spectra.

Artifact:

- `selector_backed_campaign_plan.json`

Current blocker:

- Phase22 selector cells are labels only.
- The existing solver path requires materialized background records, manifests,
  omega/A0 states, geometry, and environment inputs.
- P35 confirmed the selected W/Z cells are proxy-only and have no solver-backed
  spectrum evidence.

Next implementation work:

- add selector-cell materialization;
- build real operator bundles per selector cell;
- solve eigenvalues per selector cell;
- regenerate W/Z source candidates from solver-backed artifacts;
- rerun P34/P35 before attempting physical comparison.
