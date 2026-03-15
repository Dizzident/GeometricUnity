# Post-Phase-IX Evidence Campaign Summary

- Timestamp (UTC): `20260315T144315Z`
- Repository: `/home/josh/Documents/GitHub/GeometricUnity`
- Campaign spec: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`
- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z`
- Artifacts root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts`
- Linked Shiab companion root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion`

## Execution

### Tests

1. `dotnet test tests/Gu.Phase3.Backgrounds.Tests/Gu.Phase3.Backgrounds.Tests.csproj --no-restore`
2. `dotnet test tests/Gu.Phase5.Environments.Tests/Gu.Phase5.Environments.Tests.csproj --no-restore`
3. `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj --no-restore`
4. `dotnet test tests/Gu.Phase5.Falsification.Tests/Gu.Phase5.Falsification.Tests.csproj --no-restore`
5. `dotnet test tests/Gu.Phase5.Dossiers.Tests/Gu.Phase5.Dossiers.Tests.csproj --no-restore`
6. `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj --no-restore`
7. `dotnet build apps/Gu.Cli/Gu.Cli.csproj -nologo`

### Standard Campaign

1. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase9.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_real_atlas --lie-algebra su2`
2. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config`
3. `dotnet run --no-build --project apps/Gu.Cli -- import-environment --spec studies/phase5_su2_branch_refinement_env_validation/config/env_imported_phase8_spec.json --out studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json`
4. `dotnet run --no-build --project apps/Gu.Cli -- build-phase5-sidecars --registry studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json --observables studies/phase5_su2_branch_refinement_env_validation/config/observables.json --environment-record studies/phase5_su2_branch_refinement_env_validation/config/env_toy_record.json --environment-record studies/phase5_su2_branch_refinement_env_validation/config/env_structured_4x4_record.json --environment-record studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config`
5. `dotnet run --no-build --project apps/Gu.Cli -- validate-phase5-campaign-spec --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --require-reference-sidecars`
6. `dotnet run --no-build --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts --validate-first`

### Paired Shiab Companion

1. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase9_first_order_shiab.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas --lie-algebra su2`
2. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase9_first_order_shiab.json --out-dir reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts`
3. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase9_first_order_shiab.json --values reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/refinement_values.json --out reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/refinement_study_result.json`
4. `dotnet run --no-build --project apps/Gu.Cli -- branch-robustness --study studies/phase5_su2_branch_refinement_env_validation/config/branch_robustness_study_phase9_first_order_shiab.json --values reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/branch_quantity_values.json --out reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/branch_robustness_record.json`

## Headline Results

- Standard nontrivial atlas: closed. `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_real_atlas/atlas.json` now admits `4/7` backgrounds and rejects `3/7`. The admitted set is still nontrivial: one zero-seed solve and three symmetric-ansatz objective solves survive the original thresholds.
- Standard branch study: evaluable and not robust. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/branch/branch_robustness_record.json` reports `overall summary = mixed`, with `3` invariant/robust quantities and `2` fragile quantities.
- Standard convergence: still `5/5` convergent, but the report now states that the ladder is bridge-derived from `4` admitted backgrounds and is not a direct solver-backed refinement family.
- Standard quantitative scorecard: unchanged at `7` passed, `1` failed, overall score `0.875`.
- Standard harder miss: still the imported repo benchmark only. The failed match remains `targetBenchmarkClass = internal-benchmark`, not real external evidence.
- Standard falsifiers: `4` active. `1` fatal representation-content falsifier remains, and there are now `3` active high falsifiers: `2` branch-fragility targets (`gauge-violation`, `solver-iterations`) plus the imported benchmark mismatch.
- Standard sidecar origins: all four channels remain `upstream-sourced`.
- Imported provenance: still not external. The copied imported environment record remains `datasetId = repo-internal-phase8-import-mesh-v1` with the same in-repo source hash and conversion version.
- Candidate-linked provenance in the real campaign: still open. The dossier still shows `branch-robust` and `refinement-bounded` failing with empty `evidenceRecordIds`, and there is no `candidate_provenance_links.json` in the campaign config directory.
- Paired `first-order-curvature` Shiab run: closed from inconclusive to evaluable. `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json` also admits `4/7` and rejects `3/7`, and the paired branch study now reports `overallSummary = mixed` instead of `inconclusive`.
- Paired convergence: `5/5` continuum estimates classify as convergent, but they remain bridge-derived from the paired admitted atlas rather than direct solver-backed refinement solves.

## Contract Status

### Implemented And Exercised

- Multi-variant nontrivial standard atlas admission without weakening thresholds.
- Multi-variant nontrivial paired `first-order-curvature` Shiab atlas admission.
- Standard and paired branch studies are now evaluable on admitted multi-variant families.
- Convergence summaries now distinguish bridge-derived multi-background evidence from direct solver-backed refinement evidence.
- `branch-robustness` now accepts the bridge exporter’s table-shaped `branch_quantity_values.json` directly, and that path was exercised on the paired artifact.

### Implemented But Not Exercised

- Optional candidate-linked provenance enrichment via `candidate_provenance_links.json` was added and tested, but the real Phase IX campaign had no honest candidate-linked file to ingest and no candidate-linked branch/background IDs in the registry.

### Still Open

- Real external imported evidence was not added, because the executed campaign still copies only the repo-internal imported benchmark record.
- Candidate-specific branch/refinement gates still fail closed in the real dossier output.
- Refinement evidence is still bridge-derived rather than direct solver-backed family evidence.
- Standard and paired branch conclusions are now evaluable, but both remain mixed rather than robust.

## Key Output Files

- Standard atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_real_atlas/atlas.json`
- Standard bridge manifest: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/inputs/bridge_manifest.json`
- Standard branch: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/branch/branch_robustness_record.json`
- Standard convergence: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/convergence/refinement_study_result.json`
- Standard quantitative: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/quantitative/consistency_scorecard.json`
- Standard falsification: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/falsification/falsifier_summary.json`
- Standard typed dossier: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/dossiers/phase5_validation_dossier.json`
- Standard report: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/reports/phase5_report.md`
- Paired atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json`
- Paired bridge manifest: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/bridge_manifest.json`
- Paired branch: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/branch_robustness_record.json`
- Paired convergence: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/refinement_study_result.json`

## Interpretation Boundary

- Phase IX truly closed the Phase VIII one-variant bottleneck for both the standard and paired non-identity Shiab paths.
- Phase IX did not make either branch story robust. It made both of them measurable, and both measurements are mixed.
- Phase IX did not upgrade the imported benchmark into real external evidence.
- Phase IX did not produce candidate-linked branch/background provenance in the real executed campaign artifacts.
- Phase IX improved convergence provenance honesty, but not convergence realism all the way to direct refinement solves.
