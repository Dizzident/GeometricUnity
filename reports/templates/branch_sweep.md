# Branch Sweep Report

**Environment ID:** {{environmentId}}
**Sweep Completed:** {{sweepCompleted}}
**Total Branches:** {{totalBranches}}

---

## Branch Family

| VariantId | A0Variant | TorsionVariant | ShiabVariant |
|-----------|-----------|----------------|--------------|
{{#variants}}
| {{variantId}} | {{a0Variant}} | {{torsionVariant}} | {{shiabVariant}} |
{{/variants}}

---

## Per-Branch Results

| VariantId | Converged | TerminationReason | FinalObjective | FinalResidualNorm | Iterations | SolveMode | ObservedState |
|-----------|-----------|-------------------|----------------|-------------------|------------|-----------|---------------|
{{#runRecords}}
| {{variantId}} | {{converged}} | {{terminationReason}} | {{finalObjective}} | {{finalResidualNorm}} | {{iterations}} | {{solveMode}} | {{observedState}} |
{{/runRecords}}

---

## Pairwise Distance Matrices

### D_obs (Observed Output Distances)

| | {{#branchIds}}{{.}} | {{/branchIds}}
|-|{{#branchIds}}------|{{/branchIds}}
{{#dObsRows}}
| {{rowId}} | {{#values}}{{.}} | {{/values}}
{{/dObsRows}}

### D_dyn (Dynamical Diagnostic Distances)

| | {{#branchIds}}{{.}} | {{/branchIds}}
|-|{{#branchIds}}------|{{/branchIds}}
{{#dDynRows}}
| {{rowId}} | {{#values}}{{.}} | {{/values}}
{{/dDynRows}}

### D_stab (Stability Distances)

| | {{#branchIds}}{{.}} | {{/branchIds}}
|-|{{#branchIds}}------|{{/branchIds}}
{{#dStabRows}}
| {{rowId}} | {{#values}}{{.}} | {{/values}}
{{/dStabRows}}

---

## Canonicity Evidence Summary

**EvidenceId:** {{evidenceId}}
**StudyId:** {{studyId}}
**Verdict:** {{verdict}}
**Max Observed Deviation:** {{maxObservedDeviation}}
**Tolerance:** {{tolerance}}

---

_Generated from Phase2BranchSweepResult and CanonicityAnalyzer output._
