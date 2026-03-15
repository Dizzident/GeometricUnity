# IMPLEMENTATION_P9.md

## Purpose

This is the implementation handoff for Phase IX, starting from the Phase VIII run:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts`
- Shiab companion: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion`

Phase IX should treat those artifacts as the binding starting state.

## Binding Decisions

### D-P9-001 Do Not Reclassify Inconclusive Branch Studies As Robust

The standard and paired branch studies are inconclusive because each bridge export has
only one admitted variant. Do not summarize that as robustness.

### D-P9-002 Do Not Relabel Repo-Internal Imported Provenance As External

The imported mesh path is now real and hashed, but it still lives inside the repository.
Do not call it external data.

### D-P9-003 Keep Candidate Gates Fail-Closed

Branch and refinement claim gates must continue to fail closed when candidate-linked
branch/background provenance is absent. Do not restore global leakage to recover passes.

### D-P9-004 Preserve Nontrivial Rejections

The three rejected symmetric-ansatz solves in both the standard and paired atlases are
real evidence. Do not tune summaries to hide them.

### D-P9-005 Preserve Benchmark Separation

Controls, internal benchmarks, and external measurements must remain separate classes in
artifacts and summaries.

## Milestones

### P9-M1 Admit A Multi-Variant Nontrivial Standard Atlas

Required behavior:

- at least two nontrivial standard backgrounds are admitted without weakening thresholds,
- rejected nontrivial seeds remain recorded,
- tests cover the replay/continuation or alternative nontrivial seed path used to reach
  that atlas.

### P9-M2 Add Candidate-Linked Branch And Background Provenance

Required behavior:

- registry candidates or auxiliary artifacts expose branch variant ids and background ids,
- dossier branch/refinement gates carry non-empty candidate-linked evidence IDs when the
  evidence exists,
- tests cover mixed populations with and without candidate-linked branch/background provenance.

### P9-M3 Add Genuine External Imported Evidence

Required behavior:

- at least one imported environment record references a real external dataset,
- copied campaign inputs preserve the external dataset provenance fields,
- summaries state plainly which imported records are external and which remain internal.

### P9-M4 Make The Paired Shiab Run Evaluable

Required behavior:

- the `first-order-curvature` paired atlas admits more than one nontrivial background,
- paired branch robustness is no longer inconclusive,
- final evaluation states whether the paired Shiab path changes the project conclusions.

### P9-M5 Strengthen Convergence Evidence

Required behavior:

- convergence artifacts distinguish bridge-derived extrapolation from direct solver-backed
  family evidence,
- tests cover whatever new convergence evidence path is introduced.

## Required Final Evaluation

The next evaluation must distinguish:

- contracts implemented and exercised,
- contracts implemented but not exercised,
- nontrivial upstream evidence,
- real external evidence,
- internal benchmark evidence,
- bridge-derived evidence,
- and remaining scientific limitations.

If the standard atlas still admits only one variant, imported provenance is still only
repo-internal, branch/refinement candidate gates still lack candidate-linked evidence,
or the paired Shiab branch study is still inconclusive, those items remain open.
