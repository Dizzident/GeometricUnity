# Phase XXXIX W/Z Environment Background Materialization

## Purpose

This study materializes solver-backed background records for Phase XXII
environment selectors that have checked-in local geometry.

## Inputs

- Background study:
  `config/environment_background_study.json`
- Imported repo mesh:
  `../phase5_su2_branch_refinement_env_validation/datasets/phase8_repo_import_mesh.json`
- Branch manifest search path:
  `../phase5_su2_branch_refinement_env_validation/config/manifests`

## Outputs

- Background atlas:
  `backgrounds/atlas.json`
- Background records:
  `backgrounds/background_records/*.json`
- Background states and consumed manifests:
  `backgrounds/background_states/*.json`
- Environment closure audit after materialization:
  `environment_source_closure_after_phase39.json`
- Selector-cell audit after materialization:
  `selector_cell_materialization_after_phase39.json`

## Result

The local-geometry environment axis is partially unblocked:

- `env-toy-2d-trivial`: background-backed;
- `env-structured-4x4`: background-backed;
- `env-imported-repo-benchmark`: background-backed;
- `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1`: still blocked.

The selector-cell audit remains blocked at 0/576 materialized cells because the
current artifacts are axis-level records, not full branch/refinement/environment
cell bundles.
