# ARCH_P10.md

## Purpose

Phase X starts from the completed Phase IX run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z`

Phase IX removed the single-variant bottleneck. The standard and paired nontrivial
atlases are now broad enough to measure branch dependence, and both measurements are
mixed rather than inconclusive. The next boundary is no longer atlas width. It is
candidate traceability, evidence tier, and directness of the refinement story.

## Verified Starting State

The repository now proves all of the following from generated artifacts:

- the standard atlas is a nontrivial multi-variant family with `4/7` admitted and `3/7` rejected backgrounds,
- the paired `first-order-curvature` Shiab atlas is also a nontrivial multi-variant family with `4/7` admitted and `3/7` rejected backgrounds,
- the standard branch study is evaluable and `mixed`,
- the paired branch study is evaluable and `mixed`,
- standard convergence still reports `5/5` convergent quantities, but now states explicitly that it is bridge-derived from `4` admitted backgrounds,
- all four sidecar channels remain upstream-sourced,
- the quantitative scorecard remains `7` passed and `1` failed, with the only miss still an internal benchmark,
- the imported environment path still points to repo-internal imported provenance rather than external evidence,
- candidate-specific branch/refinement escalation still fails closed in the real dossier because candidate-linked branch/background provenance is absent.

## Architectural Boundary

The remaining boundary is evidence quality and candidate traceability.

The decisive remaining limits are:

- branch studies are no longer inconclusive, but both standard and paired families are still mixed because `gauge-violation` and `solver-iterations` remain fragile,
- the imported benchmark path is still repo-internal and therefore not real external evidence,
- candidate branch and refinement gates still fail with empty evidence IDs in the executed dossier,
- convergence remains bridge-derived rather than direct solver-backed refinement evidence,
- the only quantitative miss is still an internal benchmark, so the evidence story remains internal to the repository.

## Workstreams

### W1. Candidate-Linked Branch And Background Provenance In Real Campaign Inputs

Required outcome:

- real registry candidates or real campaign-side auxiliary inputs carry candidate-linked
  branch variant IDs and background IDs,
- the executed campaign copies those inputs forward,
- dossier branch/refinement gates cite non-empty candidate-linked evidence IDs in the
  actual output artifacts rather than only in unit tests.

### W2. Genuine External Imported Evidence

Required outcome:

- at least one imported environment record references a real external dataset,
- copied campaign inputs preserve that external dataset provenance,
- summaries separate repo-internal imported benchmarks from genuine external evidence.

### W3. Direct Solver-Backed Refinement Evidence

Required outcome:

- at least one refinement family is produced from direct solver-backed runs rather than
  only from bridge-exported atlas values,
- convergence summaries and dossiers can distinguish direct refinement evidence from
  bridge-derived evidence in executed artifacts,
- direct and bridge paths can be compared without blurring them together.

### W4. Branch Fragility Resolution Or Strict Branch-Limit Conclusion

Required outcome:

- determine whether `gauge-violation` and `solver-iterations` fragility can be reduced
  without weakening thresholds,
- if not, preserve the mixed result and promote it to a stable scientific limit instead
  of implying branch robustness.

### W5. Paired Shiab Interpretation Beyond Mere Evaluability

Required outcome:

- keep the paired `first-order-curvature` path executable on the same strict evidence path,
- determine whether its mixed result materially differs from the standard path,
- state plainly whether broader Shiab scope changes any project-level conclusion.

## Exit Criteria

Phase X is complete only when all of the following are true:

- the executed dossier carries candidate-linked branch/refinement evidence IDs where the evidence exists,
- imported-tier evidence includes at least one genuine external dataset,
- at least one executed refinement path is direct solver-backed rather than only bridge-derived,
- branch fragility is either materially improved under the same thresholds or explicitly stabilized as an unresolved scientific limitation,
- the paired non-identity Shiab path remains evaluable and its difference from the standard path is explicitly characterized.
