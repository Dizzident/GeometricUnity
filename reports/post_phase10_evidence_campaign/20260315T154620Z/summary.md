# Post-Phase-X Evidence Campaign Summary

- Timestamp (UTC): `20260315T154620Z`
- Repository: `/home/josh/Documents/GitHub/GeometricUnity`
- Campaign spec: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`
- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z`
- Artifacts root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts`
- Bridge comparison root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison`
- Linked Shiab companion root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion`

## Execution

### Tests

1. `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj --no-restore`
2. `dotnet test tests/Gu.Phase3.Backgrounds.Tests/Gu.Phase3.Backgrounds.Tests.csproj --no-restore`
3. `dotnet test tests/Gu.Phase5.Convergence.Tests/Gu.Phase5.Convergence.Tests.csproj --no-restore`
4. `dotnet test tests/Gu.Phase5.Falsification.Tests/Gu.Phase5.Falsification.Tests.csproj --no-restore`
5. `dotnet test tests/Gu.Phase5.Dossiers.Tests/Gu.Phase5.Dossiers.Tests.csproj --no-restore`
6. `dotnet build apps/Gu.Cli/Gu.Cli.csproj -nologo`

### Standard Direct Refinement And Campaign

1. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l0.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_standard_l0 --lie-algebra su2`
2. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l1.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_standard_l1 --lie-algebra su2`
3. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l2.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_standard_l2 --lie-algebra su2`
4. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-direct-refinement-values --level-spec studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l0.json --record studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_standard_l0/background_records/bg-phase10-direct-standard-l0-20260315154018.json --level-spec studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l1.json --record studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_standard_l1/background_records/bg-phase10-direct-standard-l1-20260315153944.json --level-spec studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l2.json --record studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_standard_l2/background_records/bg-phase10-direct-standard-l2-20260315153945.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config/phase10_direct_refinement_standard`
5. `dotnet run --no-build --project apps/Gu.Cli -- validate-phase5-campaign-spec --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --require-reference-sidecars`
6. `dotnet run --no-build --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts --validate-first`
7. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --out-dir reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison`
8. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --values reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/refinement_values.json --out reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/standard_bridge_refinement_study_result.json`
9. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study.json --values studies/phase5_su2_branch_refinement_env_validation/config/phase10_direct_refinement_standard/refinement_values.json --out reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/standard_direct_refinement_study_result.json`

### Paired Shiab Companion

1. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l0.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_first_order_l0 --lie-algebra su2`
2. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l1.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_first_order_l1 --lie-algebra su2`
3. `dotnet run --no-build --project apps/Gu.Cli -- solve-backgrounds studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l2.json --output studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_first_order_l2 --lie-algebra su2`
4. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-direct-refinement-values --level-spec studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l0.json --record studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_first_order_l0/background_records/bg-phase10-direct-first-order-l0-20260315153944.json --level-spec studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l1.json --record studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_first_order_l1/background_records/bg-phase10-direct-first-order-l1-20260315153944.json --level-spec studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l2.json --record studies/phase5_su2_branch_refinement_env_validation/upstream/phase10_direct_refinement_first_order_l2/background_records/bg-phase10-direct-first-order-l2-20260315153945.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase9_first_order_shiab.json --out-dir studies/phase5_su2_branch_refinement_env_validation/config/phase10_direct_refinement_first_order`
5. `dotnet run --no-build --project apps/Gu.Cli -- export-phase5-bridge-values --atlas studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json --refinement-spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase9_first_order_shiab.json --out-dir reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts`
6. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase9_first_order_shiab.json --values reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/refinement_values.json --out reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/bridge_refinement_study_result.json`
7. `dotnet run --no-build --project apps/Gu.Cli -- refinement-study --spec studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase9_first_order_shiab.json --values studies/phase5_su2_branch_refinement_env_validation/config/phase10_direct_refinement_first_order/refinement_values.json --out reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/direct_refinement_study_result.json`
8. `dotnet run --no-build --project apps/Gu.Cli -- branch-robustness --study studies/phase5_su2_branch_refinement_env_validation/config/branch_robustness_study_phase9_first_order_shiab.json --values reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/branch_quantity_values.json --out reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/branch_robustness_record.json`

## Headline Results

- Standard direct refinement is now executed and solver-backed. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/inputs/refinement_evidence_manifest.json` records `evidenceSource = direct-solver-backed` with three executed background record IDs.
- Standard convergence now remains `5/5` convergent after the direct path is exercised. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/reports/phase5_report.md` states `Evidence source: direct solver-backed from 3 executed background record(s).`
- Standard bridge comparison stays available. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/standard_bridge_refinement_study_result.json` still gives a nontrivial bridge-derived `5/5` convergent ladder, while `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/standard_direct_refinement_study_result.json` gives a direct solver-backed `5/5` convergent zero-invariant ladder.
- Standard branch evidence remains mixed, not robust. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/branch/branch_robustness_record.json` still reports `3` invariant quantities and `2` fragile quantities.
- Standard quantitative scorecard remains `7` passed, `1` failed, overall score `0.875`. The only miss is still the imported repo benchmark labeled `targetBenchmarkClass = internal-benchmark`.
- Standard falsifiers drop back to `4` active after the classifier fix. There is still `1` active fatal representation-content falsifier and `3` active high falsifiers: `2` branch-fragility targets plus the imported benchmark mismatch.
- Candidate-linked branch/refinement gates remain open in the executed dossier. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/dossiers/phase5_validation_dossier.json` still shows `branch-robust` and `refinement-bounded` with empty `evidenceRecordIds`.
- Imported provenance remains repo-internal rather than external. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/inputs/env_record_2.json` still records `datasetId = repo-internal-phase8-import-mesh-v1`.
- Paired `first-order-curvature` Shiab direct refinement is now also exercised. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/direct_refinement_study_result.json` records `5/5` convergent direct solver-backed quantities.
- Paired branch evidence remains mixed. `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/branch_robustness_record.json` still shows `gauge-violation` and `solver-iterations` as fragile.
- Broader family search still does not produce a stronger branch story in repository context. The executed Phase IX experiment atlases remain `4/7` admitted for the standard method sweep, `1/8` admitted for the standard label sweep, `4/7` admitted for the paired method sweep, and `1/8` admitted for the paired label sweep.

## Contract Status

### Implemented And Exercised

- Direct solver-backed refinement export with explicit refinement evidence manifests.
- Campaign input copying and reporting of direct-vs-bridge refinement provenance.
- Standard direct solver-backed refinement ladder, exercised end to end in the main Phase X campaign.
- Paired `first-order-curvature` direct solver-backed refinement ladder, exercised in the companion artifacts.
- Convergence classification of exact zero-delta ladders as converged invariant data, with downstream falsification and dossier paths exercised on regenerated artifacts.

### Implemented But Not Exercised

- Candidate-linked provenance ingestion remains code-capable but still unexercised in a real campaign because no honest candidate-linked input exists in repository context.

### Still Open

- Candidate-specific branch/refinement escalation still fails closed in the executed dossier.
- Imported evidence is still repo-internal and does not qualify as real external evidence.
- Standard and paired branch studies remain mixed rather than robust.
- The direct solver-backed refinement family now exists, but the executed direct ladders are exact zero-invariant control ladders rather than nontrivial direct refinements on the admitted mixed branch family.
- The active fatal representation-content falsifier remains unresolved.

## Interpretation Boundary

- Phase X truly closes the direct-refinement implementation gap.
- Phase X does not close candidate traceability, external evidence, or branch robustness.
- Phase X improves provenance honesty and execution coverage without changing the negative branch result.
