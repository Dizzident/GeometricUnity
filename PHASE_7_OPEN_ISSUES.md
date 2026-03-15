# PHASE_7_OPEN_ISSUES.md

This file records the open issues that remain after the Phase VII evidence run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z`

It is based on generated artifacts from the standard campaign and the linked
`first-order-curvature` Shiab companion run, not on planning prose.

---

## Scientific Limitations

### P7-001 The Standard Branch/Convergence Path Now Uses A Real Persisted Atlas, But It Is Still A Trivial Toy-Control Atlas

Evidence:

- `config/background_atlas.json` now comes from `upstream/phase7_real_atlas/atlas.json`,
- `bridge_manifest.json` records real `sourceRecordIds` and `sourceStateArtifactRefs`,
- every admitted atlas record in `background_atlas.json` has `runClassification.isTrivialValidationPath = true`,
- the standard branch and convergence artifacts remain all-zero control quantities.

Impact:

- the synthetic checked-in atlas boundary is closed,
- but the evidence is still not a nontrivial scientific background validation result.

### P7-002 Imported Environment Tier Is Still Synthetic Example Provenance

Evidence:

- the copied input `campaign_artifacts/inputs/env_record_2.json` still declares `environmentId = env-imported-synthetic-example`,
- it still uses `datasetId = synthetic-example-dataset-su2-2d-001`,
- its admissibility note still says it is a fabricated imported-tier example.

Impact:

- imported-tier provenance is implemented and exercised,
- but the campaign still does not use a genuine imported dataset.

### P7-003 The Harder Quantitative Failure Is Now Environment-Aware, But It Is Still An Internal Benchmark

Evidence:

- `quantitative/consistency_scorecard.json` now records both `computedEnvironmentId` and `targetEnvironmentId`,
- the failing record is `bosonic-mode-2-imported-geometry-benchmark`,
- that failed target still traces to `targetProvenance = imported-geometry-benchmark-v1`, not to an external experiment.

Impact:

- the campaign can now say which environment tier supplied the miss,
- but it is still a miss against an internal benchmark, not against real-world data.

### P7-004 Shiab Scope Expansion Is Only Partial

Evidence:

- the standard campaign still uses `V1-identity-shiab-trivial-torsion-simple-a0-omega`,
- a linked companion run now exists under `reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion`,
- that companion uses `first-order-curvature` and reproduces robust branch and convergent refinement results on the same toy-control zero-path atlas.

Impact:

- Phase VII no longer has zero evidence outside identity-shiab,
- but the main campaign is still identity-led and the companion run does not yet test a nontrivial Shiab-sensitive solve.

---

## Evidence Pipeline Limitations

### P7-005 Sidecars Are No Longer Heuristic In The Standard Run, But Most Are Still Bridge-Derived Rather Than Upstream-Sourced

Evidence:

- `falsification/sidecar_summary.json` shows no heuristic channel counts,
- `coupling-consistency` is `upstream-sourced`,
- `observation-chain`, `environment-variance`, and `representation-content` are still `bridge-derived`.

Impact:

- Phase VII closed the pure-heuristic placeholder problem in the standard run,
- but only one of the four sidecar channels is a true upstream evidence product.

### P7-006 Claim Escalation Gates Remain Mostly Global Rather Than Candidate-Specific

Evidence:

- `dossiers/phase5_validation_dossier.json` still applies the same branch, refinement, and quantitative gate outcomes broadly across candidates,
- only direct falsifier hits materially separate candidates in the current escalation path.

Impact:

- escalation is exercised and preserves negative results,
- but it still over-assigns global campaign success to individual candidates.

---

## Closed In This Run

The following Phase VII contracts are now closed in both implementation and execution:

- the standard campaign consumes a real persisted upstream atlas rather than a synthetic checked-in reference atlas,
- bridge manifests now record persisted `sourceStateArtifactRefs`,
- quantitative matching is environment-aware and no longer depends on first-record-wins behavior,
- scorecard and falsifier artifacts now state which environment supplied each quantitative match,
- sidecar outputs label origin by channel and the standard run contains no heuristic sidecar channel records,
- coupling-consistency sidecars are now upstream-sourced from persisted coupling artifacts,
- a linked `first-order-curvature` Shiab companion run was executed and recorded.
