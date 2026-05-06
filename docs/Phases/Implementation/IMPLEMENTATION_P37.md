# Phase XXXVII - W/Z Selector Materialization Map Audit

## Goal

Phase XXXVII decomposes the Phase36 materialization blocker by selector axis.
P36 showed that no full `(branch, refinement, environment)` selector cell is
materialized. P37 checks which axes already have solver-backed source maps.

## Implementation

- Added `WzSelectorMaterializationMapAudit`.
- Added CLI command `audit-wz-selector-materialization-map`.
- Added focused reporting test coverage.
- Generated:
  - `studies/phase37_wz_selector_materialization_map_audit_001/selector_materialization_map_audit.json`

## Result

P37 status is `selector-materialization-map-blocked`.

Axis coverage:

- branch variants mapped: `4/4`;
- refinement levels mapped: `3/3`;
- environments mapped: `0/4`.

The checked-in data contains:

- branch selector map through
  `studies/phase5_su2_branch_refinement_env_validation/config/bridge_manifest.json`;
- refinement selector map through
  `studies/phase5_su2_branch_refinement_env_validation/config/phase10_direct_refinement_standard/refinement_evidence_manifest.json`;
- environment selector declarations through
  `studies/phase5_su2_branch_refinement_env_validation/config/environment_campaign.json`.

It does not contain solver-backed background records for the environment axis.

## Interpretation

The next blocker is narrower than P36: branch and refinement maps exist, but the
environment selectors are declaration-only. A solver-backed W/Z selector
spectrum campaign cannot materialize full selector cells until each environment
selector is mapped to a persisted background record, branch manifest, and omega
state.

## Command

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-materialization-map \
  --spec studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json \
  --bridge-manifest studies/phase5_su2_branch_refinement_env_validation/config/bridge_manifest.json \
  --refinement-evidence-manifest studies/phase5_su2_branch_refinement_env_validation/config/phase10_direct_refinement_standard/refinement_evidence_manifest.json \
  --environment-campaign studies/phase5_su2_branch_refinement_env_validation/config/environment_campaign.json \
  --out studies/phase37_wz_selector_materialization_map_audit_001/selector_materialization_map_audit.json
```

The command returns exit code `1` for the checked-in study because environment
selector source maps are missing.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 186/186 tests.
- `dotnet test GeometricUnity.slnx`
  passed.
- `jq -e . studies/phase37_wz_selector_materialization_map_audit_001/selector_materialization_map_audit.json`
  passed.

## Next Step

Implement or locate environment-backed background records for:

- `env-toy-2d-trivial`;
- `env-structured-4x4`;
- `env-imported-repo-benchmark`;
- `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1`.

Once those are available, build a full selector-cell materialization table by
combining branch, refinement, and environment source maps.
