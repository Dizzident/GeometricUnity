# PHASE11_SUMMARY.md

## What Phase X Changed

Phase X closed the direct solver-backed refinement implementation gap.

- The standard campaign now executes against a copied direct refinement evidence manifest
  with `evidenceSource = direct-solver-backed`.
- The standard report now states plainly that convergence is direct solver-backed from
  `3` executed background record(s).
- The paired `first-order-curvature` Shiab path now has its own direct solver-backed
  refinement ladder and executed comparison artifacts.
- The bridge-derived refinement ladder remains available side by side with the new
  direct ladder, so provenance and behavior can be compared instead of conflated.
- The convergence classifier now treats exact zero-delta ladders as converged invariant
  data, and the regenerated falsifier/dossier outputs reflect that executed behavior.

## What Phase X Did Not Change

- Candidate branch/refinement escalation still does not have real candidate-linked
  evidence IDs in the executed dossier.
- The imported benchmark is still repo-internal provenance, not external evidence.
- The only quantitative miss is still an internal benchmark miss.
- The branch story is still not robust: `gauge-violation` and `solver-iterations`
  remain fragile on both standard and paired paths.
- The direct refinement story is now real, but the executed direct ladders are still
  zero-invariant control ladders rather than nontrivial direct refinement evidence.
- The representation-content fatal falsifier still remains active.

## What Now Counts As Stronger Evidence

- A direct solver-backed refinement manifest with non-empty source record IDs is stronger
  than a report that only inherits bridge-derived refinement values.
- A regenerated campaign that preserves the negative branch result after a direct
  refinement upgrade is stronger than a cleaner-looking report that hides the evidence boundary.
- A paired Shiab direct-refinement run that is actually executed is stronger than a
  companion path that only mirrors the old bridge-derived story.
- A classifier that recognizes exact invariance as converged control data is stronger
  than one that mislabels executed zero-delta ladders as failures.

## What Phase XI (P11-M3) Changed

Phase XI added a nontrivial direct solver-backed refinement ladder.

- New background study configs use `SymmetricAnsatz` seed with `GaussNewton` solver on
  the structured fiber-bundle environments (env-refinement-2x2, 4x4, 8x8).
- All three levels produced admitted backgrounds (B2/B1) with nontrivial nonzero metrics.
- Residual norms across levels: L0=1.24e-9, L1=4.04e-8, L2=1.06e-7 (non-constant).
- Gauge violations across levels: L0=0.197, L1=0.422, L2=0.996 (non-constant).
- The nontrivial ladder confirms that SymmetricAnsatz seeds find stable admitted solutions
  on all three mesh sizes, but residuals grow with refinement — this is genuine nontrivial
  solver-backed evidence rather than zero-invariant control evidence.
- The Phase X zero-invariant control ladder remains available for comparison.
- The bridge-derived refinement ladder remains available for comparison.
- Evidence paths: direct-nontrivial, direct-control (zero-invariant), and bridge-derived
  are now all three explicitly present in the repository.
- Output artifacts: `studies/phase5_su2_branch_refinement_env_validation/config/phase11_direct_nontrivial/`
- Background records: `studies/phase5_su2_branch_refinement_env_validation/upstream/phase11_direct_nontrivial_l{0,1,2}/`
- Refinement study result: `reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/nontrivial_direct_refinement_study_result.json`
- 4 new tests in `Phase11NontrivialDirectRefinementTests.cs` verify the nontrivial contract.

## What Phase XI (P11-M4) Changed

Phase XI documented branch fragility as the stable scientific boundary.

- The broader Phase IX family search (method sweeps 4/7 admitted, label sweeps 1/8 admitted)
  demonstrates that the admitted family inherently mixes trivial and nontrivial members.
- The trivial member (zero seed) always produces gauge-violation=0 and solver-iterations=0.
- The nontrivial members (SymmetricAnsatz) produce gauge-violation~0.14-0.20 and solver-iterations=2.
- Under unchanged thresholds (absTol=1e-5, relTol=0.1), gauge-violation and solver-iterations
  remain fragile because the trivial and nontrivial solutions cannot be brought within tolerance.
- No stronger family within repository context eliminates this mixed character.
- 5 new tests in `Phase11BranchFragilityStabilizationTests.cs` document the mixed boundary
  and confirm that a broader family search cannot fix fragility without threshold relaxation.

## What Phase XI (P11-M6) Changed

Phase XI reran the paired `first-order-curvature` Shiab path after the P11-M3 nontrivial
direct refinement upgrade.

- New background study configs use `SymmetricAnsatz` seed with `GaussNewton` solver on
  `phase8-real-atlas-first-order-shiab` manifest (env-refinement-2x2, 4x4, 8x8).
- All three levels produced admitted backgrounds (B2/B1) with nontrivial nonzero metrics.
- Residual norms across levels: L0=1.77e-9, L1=8.54e-9, L2=2.53e-8 (non-constant, growing with refinement).
- Gauge violations across levels: L0=0.201, L1=0.301, L2=0.752 (non-constant, growing with refinement).
- The nontrivial paired ladder confirms the same pattern as the standard path: values grow with
  refinement — genuine nontrivial evidence but not convergence evidence.
- The Phase X zero-invariant control ladder remains available for comparison on the paired path.
- Background records: `studies/phase5_su2_branch_refinement_env_validation/upstream/phase11_direct_nontrivial_shiab_l{0,1,2}/`
- Refinement values: `studies/phase5_su2_branch_refinement_env_validation/config/phase11_direct_nontrivial_shiab/`
- Comparison artifact: `reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/phase11_shiab_path_comparison.json`
- Shiab companion summary updated: `reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/summary.md`

Conclusion: Broader first-order-curvature Shiab scope does not change any project-level
conclusion. Both standard and paired paths produce the same qualitative pattern.

## What Phase XI Did Not Change

- Candidate branch/refinement escalation still does not have real candidate-linked
  evidence IDs in the executed dossier (P11-001).
- The imported benchmark is still repo-internal provenance, not external evidence (P11-002).
- The standard and paired branch studies remain mixed rather than robust (P11-003, P11-004).
- The nontrivial direct ladders (both standard and paired) confirm non-constant values but
  values grow with refinement, not decrease. This is negative evidence about continuum behavior.
- Solver-iterations is trivially convergent (constant=2) on both paths.
- The representation-content fatal falsifier still remains active (P11-006).

## What Still Does Not Count As Real-World Evidence

- The imported benchmark still does not count as external evidence because the copied
  dataset record is still repo-internal.
- The branch study still does not count as robustness evidence because both standard and
  paired paths remain mixed rather than robust.
- The nontrivial direct ladder is genuine solver-backed evidence with non-constant values,
  but the values grow with refinement rather than converge — this is negative evidence
  about the behavior of nontrivial solutions under mesh refinement, not a clean continuum limit.
- Candidate escalation still does not count as fully evidence-linked because the real
  dossier output still lacks candidate-linked branch/background evidence IDs.

## What A Project Lead Can Decide Next

- Treat Phase XI P11-M3 as a real nontrivial direct refinement evidence upgrade.
- Do not describe the nontrivial ladder as showing convergence — the residuals grow with
  refinement, which is genuine nontrivial behavior but not convergence evidence.
- Treat Phase XI P11-M4 as a definitive stabilization of the branch fragility result.
- The mixed branch result is now explicitly documented as the stable scientific boundary
  of the repository, not a missing implementation gap.
- Remaining open items for Phase XII: P11-001 (candidate traceability), P11-002 (external evidence),
  and P11-006 (representation-content fatal).
