# Phase XXXVIII W/Z Environment Source Closure Audit

## Purpose

This study audits whether Phase XXII environment selectors are backed by
persisted solver background records, not only by environment records or computed
observable tables.

## Inputs

- Selector campaign:
  `studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json`
- Environment records:
  - `studies/phase5_su2_branch_refinement_env_validation/config/env_toy_record.json`
  - `studies/phase5_su2_branch_refinement_env_validation/config/env_structured_4x4_record.json`
  - `studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json`
  - `studies/phase5_su2_branch_refinement_env_validation/config/env_zenodo_su2_plaquette_chain.json`
- Observables:
  `studies/phase5_su2_branch_refinement_env_validation/config/observables.json`
- Background roots:
  `studies/phase5_su2_branch_refinement_env_validation/config/backgrounds`
  plus the checked-in Phase 7, Phase 8, Phase 9, and Phase 10 background study
  JSON files.

## Output

- `environment_source_closure_audit.json`

## Result

The audit is blocked: 4/4 environment selectors have environment records and
observable support, but 0/4 have persisted solver-backed background records.
