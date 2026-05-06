# Phase XXXVIII - W/Z Environment Source Closure Audit

## Goal

Phase XXXVII showed that branch and refinement selectors have source background
maps, but environment selectors remain declaration-only. Phase XXXVIII makes that
blocker mechanically auditable by separating three kinds of environment support:

- environment record presence;
- computed observable support;
- persisted solver-backed background record support.

Physical W/Z predictions remain blocked unless the third category is present.
Environment records and observables are useful evidence, but they do not
materialize the selector cells needed by the W/Z operator/eigenvalue path.

## Implementation

Added `WzEnvironmentSourceClosureAudit` with CLI command:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-environment-source-closure \
  --spec studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json \
  --environment-records studies/phase5_su2_branch_refinement_env_validation/config/env_toy_record.json,studies/phase5_su2_branch_refinement_env_validation/config/env_structured_4x4_record.json,studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json,studies/phase5_su2_branch_refinement_env_validation/config/env_zenodo_su2_plaquette_chain.json \
  --observables studies/phase5_su2_branch_refinement_env_validation/config/observables.json \
  --background-roots studies/phase5_su2_branch_refinement_env_validation/config/backgrounds,studies/phase5_su2_branch_refinement_env_validation/config/background_atlas.json,studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase7.json,studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase8.json,studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase9.json,studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l0.json,studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l1.json,studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l2.json \
  --out studies/phase38_wz_environment_source_closure_audit_001/environment_source_closure_audit.json
```

The command intentionally returns exit code `1` for blocked closure.

## Result

Artifact:

- `studies/phase38_wz_environment_source_closure_audit_001/environment_source_closure_audit.json`

Observed status:

- terminal status: `environment-source-closure-blocked`;
- environment records: 4/4;
- observable-backed environments: 4/4;
- background-backed environments: 0/4.

The missing records are:

- `env-toy-2d-trivial`;
- `env-structured-4x4`;
- `env-imported-repo-benchmark`;
- `env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 188/188 tests.
- `dotnet test GeometricUnity.slnx`
  passed.
- `jq -e . studies/phase38_wz_environment_source_closure_audit_001/environment_source_closure_audit.json`
  passed.

## Next Step

Create or locate persisted solver-backed background records for the four
environment selectors, then rerun Phase XXXVIII until `backgroundBackedCount` is
4/4. Only after that can Phase XXXVI selector-cell materialization be advanced
from missing full-cell inputs toward solver-backed W/Z spectra.
