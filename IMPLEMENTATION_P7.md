# IMPLEMENTATION_P7.md

## Purpose

This is the implementation handoff for Phase VII, using the actual evidence run at:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase6_evidence_campaign/20260315T022228Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase6_evidence_campaign/20260315T022228Z/campaign_artifacts`
- logs: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase6_evidence_campaign/20260315T022228Z/logs`

as the binding starting state.

The repository no longer needs another pass whose main job is “make the campaign say what
it did.” It now says what it did. Phase VII must improve what the campaign is actually
measuring.

## Binding Decisions

### D-P7-001 Do Not Revert To Hand-Authored Branch/Refinement Tables

The bridge-backed path is now the standard path. Do not move the main campaign back to
hand-authored `branch_quantity_values_table.json`-style inputs.

Any replacement must be:

- persisted upstream artifacts,
- exported through the bridge flow,
- and recorded in the bridge manifest.

### D-P7-002 Do Not Call Synthetic Imported Data “Real Imported Evidence”

The current imported environment contract passes validation but remains synthetic-example
provenance. Phase VII may use it as a control path only until a real dataset lands.

### D-P7-003 Sidecar Heuristics Must Be Replaced, Not Hidden

The current sidecar generator derives evaluated records from registry/observable/environment
inputs. That is acceptable only as a bridge step.

Phase VII must make the standard campaign say explicitly whether a sidecar record is:

- upstream-sourced,
- bridge-derived,
- or heuristic.

Preferred direction:

- remove heuristic records from the primary evidence path once upstream artifacts exist.

### D-P7-004 Quantitative Matching Must Stop Depending On Input Order

The current scorecard chooses the first observable per `observableId`. That is not acceptable
once the campaign intentionally carries multiple environment-specific records for the same
observable.

Phase VII must make the match path explicit about environment and source record identity.

### D-P7-005 Preserve Negative Results As First-Class Outputs

The current campaign now surfaces:

- one failed stronger benchmark,
- one fatal representation-content falsifier,
- and one candidate demotion.

Do not weaken or hide these negatives to improve the headline score.

## Milestones

### P7-M1 Real Atlas Replacement

Replace `background_atlas.json` in the standard study with a solver-generated persisted atlas.

Required behavior:

- bridge export consumes the real atlas,
- branch variant IDs are regenerated from that atlas,
- branch/refinement artifacts remain numerically stable,
- tests cover atlas-to-bridge determinism on the real artifact shape.

Files:

- `studies/phase5_su2_branch_refinement_env_validation/config/background_atlas.json`
- `studies/phase5_su2_branch_refinement_env_validation/config/bridge_manifest.json`
- `src/Gu.Phase5.Reporting/BridgeValueExporter.cs`
- bridge/export tests under `tests/Gu.Phase5.Reporting.Tests/`

### P7-M2 Real Imported Geometry Path

Replace `env_imported_example.json` with a real imported environment record.

Required behavior:

- real `datasetId`, `sourceHash`, `conversionVersion`,
- validation still passes,
- campaign artifacts copy that provenance under `inputs/`,
- dossier/report language says which environment tier supplied which evidence.

Tests:

- imported provenance round-trip tests,
- campaign validator rejects missing provenance,
- end-to-end campaign test confirms copied imported inputs.

### P7-M3 Upstream-Sourced Sidecars

Replace heuristic sidecar derivation in the primary campaign with upstream artifacts.

Required behavior:

- observation-chain records come from persisted observation artifacts,
- environment variance is computed from actual per-environment quantitative outputs,
- representation content is built from explicit mode-family/representation evidence,
- coupling consistency is built from actual coupling proxy artifacts,
- `sidecar_summary.json` labels record origin per channel.

Files:

- `src/Gu.Phase5.Falsification/SidecarGenerator.cs`
- `src/Gu.Phase5.Falsification/SidecarSummary.cs`
- relevant upstream loaders and campaign prep code

Tests:

- origin labeling round-trips,
- reference campaign dossier distinguishes heuristic vs upstream when mixed inputs exist,
- falsifier summary coverage survives the new origin fields.

### P7-M4 Environment-Aware Quantitative Matching

Refactor scorecard generation so matches are tied to the intended environment/run record.

Required behavior:

- no implicit “first observable wins” behavior,
- match records carry enough provenance to identify the contributing environment,
- stronger benchmark targets point to the intended environment tier,
- campaign summary can say which tier passed and which tier failed.

Files:

- `src/Gu.Phase5.QuantitativeValidation/QuantitativeValidationRunner.cs`
- `src/Gu.Phase5.QuantitativeValidation/TargetMatchRecord.cs`
- scorecard/report/dossier serializers and tests

### P7-M5 Shiab Scope Expansion

Add a non-identity Shiab path to the evidence campaign or a linked companion campaign.

Required behavior:

- branch/convergence artifacts include the added scope explicitly,
- final evaluation states whether robustness and convergence conclusions survive it,
- assumptions ledger is updated if the scope remains partial.

## Required Final Evaluation

The next evaluation must distinguish:

- contract-complete evidence,
- real upstream evidence,
- synthetic controls,
- heuristic placeholder evidence,
- and still-open scientific limitations.

If the standard campaign still depends on synthetic atlas records, synthetic imported data,
or heuristic sidecars, those must remain open in Phase VII rather than being waved away as
“already solved.”
