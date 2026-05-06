# Phase XXXIX - W/Z Environment Background Materialization

## Goal

Phase XXXVIII proved that environment selector records and observables existed,
but no persisted solver-backed background records existed for the Phase XXII W/Z
environment selectors. Phase XXXIX materializes the subset that can be generated
honestly from checked-in local geometry.

## Implementation

Extended `solve-backgrounds` environment resolution for:

- `env-toy-2d-trivial`;
- `env-structured-4x4`;
- `env-imported-repo-benchmark`.

The first two use generated local 2D geometry. The imported repo benchmark uses
the checked-in native mesh:

- `studies/phase5_su2_branch_refinement_env_validation/datasets/phase8_repo_import_mesh.json`.

The Zenodo selector is intentionally not mapped because its checked-in record is
an external spectrum benchmark, not a local mesh/field conversion suitable for a
GU background solve.

## Generated Study

Study spec:

- `studies/phase39_wz_environment_background_materialization_001/config/environment_background_study.json`

Command:

```bash
dotnet run --project apps/Gu.Cli -- solve-backgrounds \
  studies/phase39_wz_environment_background_materialization_001/config/environment_background_study.json \
  --output studies/phase39_wz_environment_background_materialization_001/backgrounds \
  --lie-algebra su2
```

Result:

- total attempts: 3;
- admitted backgrounds: 3;
- rejected backgrounds: 0;
- all three admitted records reached B2 with zero residual and stationarity.

Generated records:

- `bg-phase39-env-toy-2d-trivial-bg-0-20260428190014`;
- `bg-phase39-env-structured-4x4-bg-0-20260428190014`;
- `bg-phase39-env-imported-repo-benchmark-bg-0-20260428190014`.

## Closure Audit

Reran the Phase XXXVIII closure audit with the Phase XXXIX background root:

- artifact:
  `studies/phase39_wz_environment_background_materialization_001/environment_source_closure_after_phase39.json`;
- environment records: 4/4;
- observable-backed environments: 4/4;
- background-backed environments: 3/4;
- remaining blocker:
  `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1`.

The audit still returns blocked because the Zenodo selector has no persisted
solver-backed background record.

## Selector-Cell Audit

Reran the Phase XXXVI selector-cell materialization audit with the Phase XXXIX
background root:

- artifact:
  `studies/phase39_wz_environment_background_materialization_001/selector_cell_materialization_after_phase39.json`;
- total selector cells: 576;
- materialized selector cells: 0;
- missing selector cells: 576.

Phase XXXIX closes most of the environment-axis background gap, but it does not
create full Cartesian selector-cell inputs that bind branch variant, refinement
level, environment, branch manifest, and omega state together. That remains the
next blocker before physical W/Z predictions can be computed from solver-backed
selector spectra.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 188/188 tests.
- `dotnet test GeometricUnity.slnx`
  passed.
- `jq -e . studies/phase39_wz_environment_background_materialization_001/environment_source_closure_after_phase39.json`
  passed.
- `jq -e . studies/phase39_wz_environment_background_materialization_001/selector_cell_materialization_after_phase39.json`
  passed.

## Next Step

Build a full selector-cell materialization layer. It should compose the existing
branch source records, refinement source records, and Phase XXXIX environment
background records into per-cell bundles with explicit background record,
branch manifest, geometry, A0, and omega state references. The Zenodo
environment must remain excluded or fail-closed until a local mesh/field
conversion is available.
