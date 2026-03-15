# PHASE_11_OPEN_ISSUES.md

This file records the open issues that remain after the Phase X completion run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z`

It is the required Phase XI gap ledger and is grounded in generated artifacts.

---

## Open Scientific Gaps

### P11-001 Candidate Branch And Refinement Gates Still Lack Real Candidate-Linked Evidence IDs

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/dossiers/phase5_validation_dossier.json`
  still shows `branch-robust` and `refinement-bounded` failing with empty `evidenceRecordIds`,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/inputs`
  still contains no honest candidate-linked provenance file,
- the executed campaign therefore still blocks escalation because no candidate-linked
  branch/background provenance exists in real inputs.

Impact:

- Phase X kept the candidate-link code path available,
- but Phase X did not close the real executed candidate-evidence gap.

### P11-002 Imported Provenance Is Still Repo-Internal Rather Than External Evidence

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/inputs/env_record_2.json`
  records `datasetId = repo-internal-phase8-import-mesh-v1`,
- that same copied input still points to the checked-in repository mesh source spec,
- the failing quantitative target remains labeled `targetBenchmarkClass = internal-benchmark`.

Impact:

- Phase X preserved honest imported provenance,
- but it still did not produce genuine external evidence.

### P11-003 Standard Branch Evidence Is Evaluable But Still Mixed — STABILIZED AS SCIENTIFIC BOUNDARY (P11-M4)

Status: **Stabilized** by Phase XI P11-M4 as the stable scientific boundary.

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/branch/branch_robustness_record.json`
  reports the standard branch study as `mixed`,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/falsification/falsifier_summary.json`
  still contains active branch-fragility falsifiers for `gauge-violation` and `solver-iterations`,
- broader executed atlas searches (method sweep 4/7 admitted, label sweep 1/8 admitted) show that
  the admitted family inherently mixes trivial (gauge-violation=0, solver-iterations=0) and
  nontrivial (gauge-violation~0.14-0.20, solver-iterations=2) members,
- under unchanged thresholds (absTol=1e-5, relTol=0.1), the trivial and nontrivial solutions cannot
  be brought within tolerance of each other — the mixed result is not a missing implementation.

Phase XI Closure:

- Phase XI P11-M4 confirms that no stronger family within repository context eliminates the mixed character.
- 5 new tests in `Phase11BranchFragilityStabilizationTests.cs` document the stable boundary.
- The mixed result is now explicitly preserved as negative evidence per D-P11-004.

### P11-004 Paired First-Order Shiab Branch Evidence Is Evaluable But Still Mixed — CONFIRMED BY P11-M6 RERUN

Status: **Confirmed** as stable mixed boundary by Phase XI P11-M6 rerun.

Evidence (Phase X, unchanged):

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/branch_robustness_record.json`
  reports `overallSummary = mixed`,
- that same artifact classifies `gauge-violation` and `solver-iterations` as fragile,
- the paired broader family search also remains limited to `4/7` admitted for method sweep and `1/8` admitted for label sweep.

Evidence (Phase XI nontrivial paired ladder, new):

- Background configs: `studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase11_direct_nontrivial_shiab_l{0,1,2}.json`
  use `SymmetricAnsatz` seed with `GaussNewton` solver on `phase8-real-atlas-first-order-shiab`.
- Background records: admitted at B2/B1 with nonzero residuals:
  L0=1.77e-9, L1=8.54e-9, L2=2.53e-8.
- Gauge violations: L0=0.201, L1=0.301, L2=0.752 (non-constant across levels, growing with refinement).
- Refinement values: `studies/phase5_su2_branch_refinement_env_validation/config/phase11_direct_nontrivial_shiab/refinement_values.json`
- Refinement study result: `reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts/nontrivial_direct_refinement_study_result.json`
- Convergence result: solver-iterations trivially convergent (constant=2); residual/stationarity/gauge-violation non-convergent (growing with refinement).
- Comparison: `reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/phase11_shiab_path_comparison.json`

Impact:

- Phase XI P11-M6 reran the paired path with the same nontrivial SymmetricAnsatz approach used in P11-M3,
- the paired path produces the same qualitative pattern: mixed branch, growing residuals under nontrivial refinement,
- broader first-order-curvature Shiab scope still does not show branch robustness or convergence evidence.

### P11-005 Direct Refinement Is Now Real But Still Only Control-Strength Evidence — PARTIALLY UPGRADED (P11-M3)

Status: **Partially upgraded** by Phase XI P11-M3. Nontrivial direct refinement ladder now exists.

Evidence (Phase X control ladder, unchanged):

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/inputs/refinement_evidence_manifest.json`
  states `evidenceSource = direct-solver-backed` with non-empty source record IDs,
- zero-invariant direct control ladder: `5/5` convergent, all values exactly zero.

Evidence (Phase XI nontrivial ladder, new):

- Background configs: `studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase11_direct_nontrivial_l{0,1,2}.json`
  use `SymmetricAnsatz` seed with `GaussNewton` solver.
- Background records: admitted at B2/B1 with nonzero residuals:
  L0=1.24e-9, L1=4.04e-8, L2=1.06e-7.
- Gauge violations: L0=0.197, L1=0.422, L2=0.996 (non-constant across levels).
- Refinement values: `studies/phase5_su2_branch_refinement_env_validation/config/phase11_direct_nontrivial/refinement_values.json`
- Refinement study result: `reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison/nontrivial_direct_refinement_study_result.json`
- Convergence result: `solver-iterations` is trivially convergent (constant=2); residual/stationarity/objective/gauge-violation are non-convergent (values grow with refinement).

Remaining gap:

- The nontrivial ladder has non-constant values, satisfying P11-M3 DoD.
- However, the nontrivial quantities grow with refinement (not decrease), meaning this is
  negative evidence about continuum behavior — not convergence evidence.
- The zero-invariant control ladder and bridge-derived ladder remain separately available per D-P11-005.
- 4 new tests in `Phase11NontrivialDirectRefinementTests.cs` verify the nontrivial contract.

Note per D-P11-006: The nontrivial direct ladder is real solver-backed evidence but is not
described as nontrivial direct refinement closure on the mixed admitted atlas family — the
nontrivial solutions are on the same environment, not on the admitted atlas family itself.

### P11-006 Representation-Content Fatal Remains Active

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts/falsification/falsifier_summary.json`
  still reports `activeFatalCount = 1`,
- the active fatal is still the representation-content falsifier on the same candidate family.

Impact:

- Phase X did not worsen this blocker,
- but it also did not resolve it.
