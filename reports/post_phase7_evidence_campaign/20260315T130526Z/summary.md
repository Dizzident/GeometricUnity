# Post-Phase-VII Evidence Campaign Summary

- Timestamp (UTC): `20260315T130526Z`
- Repository: `/home/josh/Documents/GitHub/GeometricUnity`
- Campaign spec: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`
- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z`
- Artifacts root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts`
- Logs root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/logs`
- Linked Shiab companion root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion`

## Execution

1. `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj --no-restore`
2. `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj --no-restore --filter FullyQualifiedName~Phase5CampaignSpecValidatorTests`
3. `dotnet test tests/Gu.Phase5.Falsification.Tests/Gu.Phase5.Falsification.Tests.csproj --no-restore --filter FullyQualifiedName~SidecarGeneratorTests`
4. `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj --no-restore --filter FullyQualifiedName~BridgeValueExporterTests`
5. `dotnet build GeometricUnity.slnx -nologo`
6. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase7.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase7_real_atlas --lie-algebra su2`
7. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase7_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config`
8. `dotnet run --no-build --project apps/Gu.Cli -- build-phase5-sidecars --registry studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json --observables studies/phase5_su2_branch_refinement_env_validation/config/observables.json --environments studies/phase5_su2_branch_refinement_env_validation/config/env_toy_record.json studies/phase5_su2_branch_refinement_env_validation/config/env_structured_4x4_record.json studies/phase5_su2_branch_refinement_env_validation/config/env_imported_example.json --output-dir studies/phase5_su2_branch_refinement_env_validation/config`
9. `dotnet run --no-build --project apps/Gu.Cli -- validate-phase5-campaign-spec --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --require-reference-sidecars`
10. `dotnet run --no-build --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts --validate-first`
11. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase7_first_order_shiab.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase7_first_order_shiab_real_atlas --lie-algebra su2`
12. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase7_first_order_shiab_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase7_first_order_shiab.json --out-dir reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts`
13. `dotnet run --no-build --project apps/Gu.Cli -- branch-robustness --study reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/branch_robustness_study.json --values reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/branch_quantity_values_map.json --out reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/branch_robustness_record.json`
14. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase7_first_order_shiab.json --values reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/refinement_values.json --out reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/refinement_study_result.json`

## Headline Results

- Standard branch robustness: `5/5` quantities classified robust or invariant.
- Standard convergence: `5/5` quantities classified convergent.
- Standard quantitative scorecard: `7` passed, `1` failed, overall score `0.875`.
- Standard falsifiers: `2` active (`1` fatal representation-content, `1` high quantitative mismatch).
- Standard sidecar coverage: all four channels evaluated.
- Standard sidecar origins: `3` bridge-derived observation-chain, `5` bridge-derived environment-variance, `3` bridge-derived representation-content, `3` upstream-sourced coupling-consistency.
- Linked Shiab companion branch robustness: `5/5` quantities robust or invariant.
- Linked Shiab companion convergence: `5/5` quantities convergent with no failure records.

## Key Output Files

- Standard branch: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/branch/branch_robustness_record.json`
- Standard convergence: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/convergence/refinement_study_result.json`
- Standard quantitative: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/quantitative/consistency_scorecard.json`
- Standard falsification: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/falsification/falsifier_summary.json`
- Standard sidecar coverage: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/falsification/sidecar_summary.json`
- Standard typed dossier: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/dossiers/phase5_validation_dossier.json`
- Standard report: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts/reports/phase5_report.md`
- Shiab companion branch: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/branch_robustness_record.json`
- Shiab companion convergence: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion/artifacts/refinement_study_result.json`

## Interpretation Boundary

- Phase VII closed the synthetic-atlas dependency in the standard bridge path.
- Phase VII closed environment-aware quantitative matching and artifact-level environment provenance.
- Phase VII closed heuristic sidecar channels in the standard run, but only coupling-consistency is fully upstream-sourced.
- The standard atlas is still a toy-control residual-inspection run, not a nontrivial scientific solve.
- The imported environment is still synthetic-example provenance.
- The harder benchmark is still an internal benchmark, not an external experiment.
- The linked `first-order-curvature` Shiab companion broadens scope, but only on the same trivial-control atlas path.
