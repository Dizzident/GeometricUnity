# IMPLEMENTATION_P8.md

## Purpose

This is the implementation handoff for Phase VIII, starting from the Phase VII run:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/campaign_artifacts`
- Shiab companion: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z/shiab_companion`

Phase VIII should treat those artifacts as the binding starting state.

## Binding Decisions

### D-P8-001 Do Not Downgrade The Real Persisted Atlas Back To A Synthetic Reference

The standard bridge path must continue to consume persisted upstream atlas records.
The remaining issue is atlas quality, not whether the atlas is real.

### D-P8-002 Do Not Call Trivial-Control Atlas Outputs Scientific Validation

If the atlas still reports `isTrivialValidationPath = true`, branch and convergence
results remain control evidence only.

### D-P8-003 Do Not Call Synthetic Imported Provenance External Evidence

The imported environment path remains a control path until a real dataset lands.

### D-P8-004 Preserve Origin Labels And Negative Results

Do not collapse `upstream-sourced`, `bridge-derived`, and internal benchmark labels
into a generic "evidence" bucket. Do not hide failing or demoting outputs.

### D-P8-005 Candidate Promotions Must Be Candidate-Evidence Joins

Phase VIII must not keep using global campaign success as a proxy for candidate-level
evidence sufficiency.

## Milestones

### P8-M1 Nontrivial Standard Atlas

Replace the current toy-control standard atlas with a persisted atlas produced from
nontrivial seeds or replayed states.

Required behavior:

- standard campaign `background_atlas.json` comes from a nontrivial atlas output,
- admitted backgrounds are not classified as trivial residual-inspection runs,
- branch and convergence artifacts are regenerated from that atlas,
- tests cover manifest/state replay and atlas-to-bridge determinism on the nontrivial path.

### P8-M2 Genuine Imported Geometry Path

Replace the synthetic imported environment record with a real imported dataset.

Required behavior:

- real `datasetId`, `sourceHash`, and `conversionVersion`,
- campaign copied inputs preserve that provenance,
- dossier and report language distinguish imported evidence from synthetic controls,
- tests cover provenance round-trip and validator enforcement.

### P8-M3 Upstream Observation And Representation Sidecars

Replace remaining bridge-derived sidecar channels with upstream artifacts.

Required behavior:

- observation-chain records load persisted upstream observation products,
- representation-content records load explicit upstream representation evidence,
- environment-variance records derive from actual per-environment quantitative runs,
- `sidecar_summary.json` shows the upgraded origin counts.

### P8-M4 Candidate-Specific Escalation Gates

Refactor claim escalation so each gate references the evidence that belongs to the
candidate under evaluation.

Required behavior:

- gate records reference candidate-specific evidence IDs,
- dossier escalation traces are candidate-specific,
- tests cover mixed pass/fail candidate populations without global leakage.

### P8-M5 Nontrivial Shiab Scope In The Standard Evidence Path

Promote Shiab scope expansion from linked trivial-control companion evidence into the
main evidence path or a standard paired campaign.

Required behavior:

- non-identity Shiab is run on nontrivial inputs,
- branch and convergence artifacts are emitted for that path,
- final evaluation states whether the broader scope changes the conclusions.

### P8-M6 Benchmark Separation

Distinguish internal harder benchmarks from genuine external evidence in the scoring
and reporting path.

Required behavior:

- scorecard outputs label benchmark class explicitly,
- final summaries state which failures are internal and which are external,
- tests cover serialization and dossier/report wording.

## Required Final Evaluation

The next evaluation must distinguish:

- contracts implemented and exercised,
- contracts implemented but not exercised,
- nontrivial upstream evidence,
- real external evidence,
- internal benchmark evidence,
- bridge-derived evidence,
- and remaining scientific limitations.

If imported provenance is still synthetic, sidecars are still bridge-derived, or
the Shiab expansion still relies on trivial-control solves, those items remain open.
