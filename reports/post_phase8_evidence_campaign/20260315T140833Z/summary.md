# Post-Phase-VIII Evidence Campaign Summary

- Timestamp (UTC): `20260315T140833Z`
- Repository: `/home/josh/Documents/GitHub/GeometricUnity`
- Campaign spec: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`
- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z`
- Artifacts root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts`
- Logs root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/logs`
- Linked Shiab companion root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion`

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

1. `dotnet run --no-build --project apps/Gu.Cli -- import-environment --spec studies/phase5_su2_branch_refinement_env_validation/config/env_imported_phase8_spec.json --out studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json`
2. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase8.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_real_atlas --lie-algebra su2`
3. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config`
4. `dotnet run --no-build --project apps/Gu.Cli -- build-phase5-sidecars --registry studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json --observables studies/phase5_su2_branch_refinement_env_validation/config/observables.json --environment-record studies/phase5_su2_branch_refinement_env_validation/config/env_toy_record.json --environment-record studies/phase5_su2_branch_refinement_env_validation/config/env_structured_4x4_record.json --environment-record studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config`
5. `dotnet run --no-build --project apps/Gu.Cli -- validate-phase5-campaign-spec --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --require-reference-sidecars`
6. `dotnet run --no-build --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts --validate-first`

### Paired Shiab Companion

1. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase8_first_order_shiab.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_first_order_shiab_real_atlas --lie-algebra su2`
2. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_first_order_shiab_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase8_first_order_shiab.json --out-dir reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts`
3. Generated `branch_robustness_study.json` and `branch_quantity_values_map.json` from the paired bridge export.
4. `dotnet run --no-build --project apps/Gu.Cli -- branch-robustness --study reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/branch_robustness_study.json --values reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/branch_quantity_values_map.json --out reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/branch_robustness_record.json`
5. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase8_first_order_shiab.json --values reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/refinement_values.json --out reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/refinement_study_result.json`

## Headline Results

- Standard atlas realism: closed the trivial residual-inspection classification gap. The standard atlas now contains nontrivial `objective-solve` records, but only `1/4` attempted backgrounds is admitted; `3/4` symmetric-ansatz seeds fail admissibility.
- Standard branch robustness: `inconclusive`, because only one admitted branch variant remains in the bridge export.
- Standard convergence: `5/5` quantities still classify as convergent.
- Standard quantitative scorecard: `7` passed, `1` failed, overall score `0.875`.
- Benchmark separation: `6` control matches and `2` internal-benchmark matches are labeled explicitly; the only failure remains an internal benchmark, not an external measurement.
- Standard falsifiers: `2` active (`1` fatal representation-content, `1` high quantitative mismatch on the imported repo benchmark).
- Standard sidecar origins: all four channels are now `upstream-sourced`.
- Imported provenance: the campaign now uses a real checked-in imported mesh path with a real dataset id, content hash, and conversion version, but it remains repo-internal benchmark provenance rather than external evidence.
- Candidate escalation: gate records now carry candidate-specific evidence IDs for multi-environment, observation-chain, quantitative-match, and fatal-falsifier joins. Branch and refinement gates fail closed because the registry still lacks candidate-linked branch/background IDs.
- Paired `first-order-curvature` Shiab run: also nontrivial and also `1/4` admitted with `3/4` symmetric-ansatz rejections. Its paired branch study is likewise `inconclusive`, while its paired refinement study remains convergent.

## Key Output Files

- Standard atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_real_atlas/atlas.json`
- Standard branch: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/branch/branch_robustness_record.json`
- Standard convergence: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/convergence/refinement_study_result.json`
- Standard quantitative: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/quantitative/consistency_scorecard.json`
- Standard falsification: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/falsification/falsifier_summary.json`
- Standard sidecars: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/falsification/sidecar_summary.json`
- Standard typed dossier: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/dossiers/phase5_validation_dossier.json`
- Standard report: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/reports/phase5_report.md`
- Imported environment record: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json`
- Paired Shiab atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_first_order_shiab_real_atlas/atlas.json`
- Paired Shiab branch: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/branch_robustness_record.json`
- Paired Shiab convergence: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/refinement_study_result.json`

## Interpretation Boundary

- Phase VIII closed the nontrivial solve-classification gap for the standard atlas.
- Phase VIII closed the remaining sidecar-origin gap: all standard sidecar channels are now upstream-sourced.
- Phase VIII closed benchmark-class labeling and report separation for controls versus internal benchmarks.
- Phase VIII closed the synthetic imported metadata gap only partially: the imported path is now a real imported repository dataset, but it is still not external data.
- Phase VIII closed candidate-specific escalation only partially: the artifacts now show candidate-specific evidence joins, but branch/refinement joins still fail closed because the registry does not carry candidate-linked branch/background provenance.
- Phase VIII did not produce a multi-variant nontrivial standard atlas. Because only one background is admitted, the standard branch study and the paired Shiab branch study are both inconclusive.
- Phase VIII did not turn the imported benchmark or the quantitative miss into real external evidence.
