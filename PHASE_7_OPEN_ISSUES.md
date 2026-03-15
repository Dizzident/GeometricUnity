# PHASE_7_OPEN_ISSUES.md

This file records the open issues that remain after the evidence-bearing campaign run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase6_evidence_campaign/20260315T022228Z`

It is based on the generated artifacts, not on planning prose.

---

## Scientific Limitations

### P7-001 Bridge-Backed Branch/Convergence Evidence Still Uses A Synthetic Reference Atlas

Evidence:

- `branch_quantity_values.json` and `refinement_values.json` are now bridge-generated,
- `bridge_manifest.json` points to `config/background_atlas.json`,
- that atlas is a checked-in synthetic reference atlas rather than a solver-generated upstream atlas.

Impact:

- branch and convergence evidence are now contract-faithful and reproducible,
- they still do not count as solver-grounded scientific evidence.

### P7-002 Imported Environment Tier Is Still Synthetic Example Provenance

Evidence:

- `environmentSummary` shows an imported-tier record,
- `env_imported_example.json` still uses `datasetId=synthetic-example-dataset-su2-2d-001`.

Impact:

- the imported-environment contract is implemented and exercised,
- the campaign still does not use a real imported geometry dataset.

### P7-003 Stronger Quantitative Benchmark Exists, But It Is Still An Internal Benchmark

Evidence:

- `consistency_scorecard.json` now contains `bosonic-mode-2-imported-geometry-benchmark`,
- that target is labeled `evidenceTier="evidence-grade"` and fails with pull `6.243670828476197`,
- the target notes still describe it as an imported-geometry benchmark rather than an experimental measurement.

Impact:

- the campaign can now distinguish “control passed” from “stronger benchmark failed,”
- but the failure is still against an internal benchmark, not against real-world data.

### P7-004 Identity-Only Shiab Still Dominates The Main Evidence Path

Evidence:

- campaign provenance and all standard observables still use `V1-identity-shiab-...` as the quantitative branch,
- no non-identity Shiab branch appears in the standard evidence path.

Impact:

- current conclusions remain local to the identity-only Shiab slice.

---

## Evidence Pipeline Limitations

### P7-005 Sidecar Records Are Evaluated But Still Heuristic

Evidence:

- `sidecar_summary.json` reports all four channels as `status="evaluated"`,
- observation/representation/coupling records are derived from registry and observable shapes rather than from dedicated upstream observation/coupling artifacts.

Impact:

- coverage is now explicit and exercised,
- the standard campaign still lacks upstream-sourced sidecar evidence products.

### P7-006 Quantitative Matching Is Still Input-Order Dependent Across Environments

Evidence:

- `observables.json` now includes multiple records per `observableId` across toy/structured/imported environments,
- `QuantitativeValidationRunner` still indexes the first observable per `observableId`,
- the scorecard therefore reflects the first listed environment rather than an explicit environment-selected match policy.

Impact:

- the scorecard is informative,
- but the environment supplying each quantitative match is still encoded by file order rather than by an explicit contract.

### P7-007 Claim Escalation Gates Remain Mostly Global Rather Than Candidate-Specific

Evidence:

- `claimEscalations` show the same branch/refinement/quantitative gate outcomes for all three candidates except where a fatal falsifier directly targets one candidate,
- current branch/refinement/quantitative gate helpers do not join to candidate-specific evidence IDs.

Impact:

- the escalation machinery is now exercised,
- but it still over-shares global campaign success across candidates.

---

## Closed In This Run

The following Phase VI-era gaps are now closed in both implementation and execution:

- strict campaign validation before the standard run,
- explicit sidecar paths in the standard campaign,
- evaluated sidecar coverage recorded in `falsifier_summary.json`,
- `observationChainSummary` present in the typed dossier,
- `sidecarCoverage` present in the typed dossier,
- bridge-backed branch/refinement export used by the standard campaign,
- multi-environment record ladder present in the standard campaign,
- non-empty registry and claim escalation exercised in the standard campaign.
