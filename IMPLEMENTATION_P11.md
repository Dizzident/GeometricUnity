# IMPLEMENTATION_P11.md

## Purpose

This is the implementation handoff for Phase XI, starting from the Phase X run:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts`
- bridge comparison: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison`
- Shiab companion: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion`

Phase XI should treat those artifacts as the binding starting state.

## Binding Decisions

### D-P11-001 Do Not Relabel Mixed Branch Studies As Robust

The standard and paired branch studies are still `mixed`.
Do not summarize that as robustness.

### D-P11-002 Do Not Relabel Repo-Internal Imported Provenance As External

The copied imported environment record still uses `datasetId = repo-internal-phase8-import-mesh-v1`.
Do not call that external data.

### D-P11-003 Keep Candidate Gates Fail-Closed Until Real Candidate Links Exist

The executed Phase X dossier still has empty `evidenceRecordIds` for `branch-robust`
and `refinement-bounded`. Do not infer those links globally or synthesize them from
campaign-wide outputs.

### D-P11-004 Preserve Branch Fragility And Representation Fatal As Negative Evidence

`gauge-violation` and `solver-iterations` remain active branch-fragility evidence,
and representation-content still contributes an active fatal falsifier.
Do not smooth those away in summaries or scorecards.

### D-P11-005 Preserve The Distinction Between Direct And Bridge Refinement Evidence

Phase X now has both forms of refinement evidence. Keep direct solver-backed and
bridge-derived ladders separately visible in reports, dossiers, and copied inputs.

### D-P11-006 Do Not Overstate The Current Direct Refinement Family

The executed direct ladders are real solver-backed evidence, but they are exact
zero-invariant control ladders. Do not describe them as a nontrivial direct refinement
closure on the mixed admitted atlas family.

## Milestones

### P11-M1 Exercise Candidate-Linked Branch And Background Provenance In A Real Campaign

Required behavior:

- add real candidate-linked branch/background provenance to the registry or a real auxiliary input,
- ensure the Phase XI campaign spec copies that input forward,
- confirm that executed dossier `branch-robust` and `refinement-bounded` gates carry
  non-empty `evidenceRecordIds` for candidates whose evidence exists,
- keep mixed populations supported: candidates with links and candidates without links
  must still be distinguished correctly.

### P11-M2 Add Genuine External Imported Evidence

Required behavior:

- integrate at least one imported environment record backed by an external dataset,
- preserve the external dataset provenance in copied inputs and summaries,
- keep internal benchmark imported records distinct from external imported records.

### P11-M3 Add A Nontrivial Direct Solver-Backed Refinement Path

Required behavior:

- produce at least one nontrivial refinement ladder from direct solver-backed runs,
- keep the existing bridge-derived ladder and the current zero-invariant direct ladder available for comparison,
- update reports and dossiers so the executed artifacts state clearly which refinement
  path is direct nontrivial evidence, which is direct control evidence, and which remains bridge-derived,
- add tests for any new direct refinement contract.

### P11-M4 Resolve Or Stabilize Branch Fragility

Required behavior:

- investigate whether the mixed branch result can be improved under unchanged thresholds,
- if a stronger family exists, demonstrate it with executed artifacts,
- if no stronger family exists in repository context, document that the mixed result is
  the stable scientific boundary and not a missing implementation.

### P11-M5 Resolve Or Stabilize The Representation-Content Fatal

Required behavior:

- determine whether stronger candidate content can close the active fatal representation-content falsifier,
- if not, preserve it as an explicit blocker in the executed Phase XI artifacts and handoff.

### P11-M6 Keep The Paired Shiab Path Scientifically Comparable

Required behavior:

- rerun the paired `first-order-curvature` path whenever Phase XI changes affect branch,
  refinement, imported evidence, or candidate-evidence interpretation,
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
- direct solver-backed control evidence,
- direct solver-backed nontrivial refinement evidence,
- and remaining scientific limitations.

If candidate-linked branch/refinement evidence IDs are still absent in the executed
dossier, imported provenance is still only repo-internal, direct refinement is still
only zero-invariant control evidence, or the standard/paired branch studies remain
mixed without stronger family evidence, those items remain open.
