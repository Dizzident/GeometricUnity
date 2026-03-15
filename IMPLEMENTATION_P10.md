# IMPLEMENTATION_P10.md

## Purpose

This is the implementation handoff for Phase X, starting from the Phase IX run:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts`
- Shiab companion: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion`

Phase X should treat those artifacts as the binding starting state.

## Binding Decisions

### D-P10-001 Do Not Relabel Mixed Branch Studies As Robust

The standard and paired branch studies are now evaluable, but both are `mixed`.
Do not summarize that as robustness.

### D-P10-002 Do Not Relabel Repo-Internal Imported Provenance As External

The copied imported environment record still uses `datasetId = repo-internal-phase8-import-mesh-v1`.
Do not call that external data.

### D-P10-003 Keep Candidate Gates Fail-Closed Until Real Candidate Links Exist

The optional candidate-link ingestion path now exists in code, but the executed Phase IX
campaign still had no honest candidate-linked branch/background input. Do not infer those
links globally or synthesize them from campaign-wide results.

### D-P10-004 Preserve Branch Fragility As Negative Evidence

`gauge-violation` and `solver-iterations` are active branch-fragility evidence in the
standard path. Do not smooth that away in summaries or scorecards.

### D-P10-005 Preserve The Distinction Between Bridge-Derived And Direct Refinement Evidence

Phase IX improved the bridge provenance story, not the directness of the refinement story.
Do not describe the current ladder as a direct refinement family.

## Milestones

### P10-M1 Exercise Candidate-Linked Branch And Background Provenance In A Real Campaign

Required behavior:

- add real candidate-linked branch/background provenance to the registry or a real auxiliary input,
- ensure the Phase X campaign spec copies that input forward,
- confirm that executed dossier `branch-robust` and `refinement-bounded` gates carry
  non-empty `evidenceRecordIds` for candidates whose evidence exists,
- keep mixed populations supported: candidates with links and candidates without links
  must still be distinguished correctly.

### P10-M2 Add Genuine External Imported Evidence

Required behavior:

- integrate at least one imported environment record backed by an external dataset,
- preserve the external dataset provenance in copied inputs and summaries,
- keep internal benchmark imported records distinct from external imported records.

### P10-M3 Add A Direct Solver-Backed Refinement Path

Required behavior:

- produce at least one refinement ladder from direct solver-backed runs,
- keep the existing bridge-derived ladder available for comparison,
- update reports and dossiers so the executed artifacts state clearly which refinement
  path is direct and which remains bridge-derived,
- add tests for the new direct refinement contract.

### P10-M4 Resolve Or Stabilize Branch Fragility

Required behavior:

- investigate whether the mixed branch result can be improved under unchanged thresholds,
- if a stronger family exists, demonstrate it with executed artifacts,
- if no stronger family exists in repository context, document that the mixed result is
  the stable scientific boundary and not a missing implementation.

### P10-M5 Keep The Paired Shiab Path Scientifically Comparable

Required behavior:

- rerun the paired `first-order-curvature` path whenever Phase X changes affect branch,
  refinement, or imported evidence interpretation,
- preserve direct comparability between the standard and paired paths,
- state explicitly whether broader Shiab scope changes any conclusion or only reproduces
  the same mixed branch story.

## Required Final Evaluation

The next evaluation must distinguish:

- contracts implemented and exercised,
- contracts implemented but not exercised,
- nontrivial upstream evidence,
- real external evidence,
- internal benchmark evidence,
- bridge-derived evidence,
- direct solver-backed refinement evidence,
- and remaining scientific limitations.

If candidate-linked branch/refinement evidence IDs are still absent in the executed
dossier, imported provenance is still only repo-internal, refinement evidence is still
only bridge-derived, or the standard/paired branch studies remain mixed without stronger
family evidence, those items remain open.
