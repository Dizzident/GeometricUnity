# PHASE_9_OPEN_ISSUES.md

This file records the open issues that remain after the Phase VIII completion run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z`

It is the required Phase IX gap ledger and is grounded in generated artifacts.

---

## Open Scientific Gaps

### P9-001 Standard Nontrivial Atlas Is Too Thin For A Real Branch Study

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_real_atlas/atlas.json`
  shows `1` admitted background and `3` rejected symmetric-ansatz solves,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/branch/branch_robustness_record.json`
  reports the study as `inconclusive`,
- the branch dossier note explicitly says fewer than two branch variants survived.

Impact:

- Phase VIII closed the trivial-control misclassification gap,
- but it did not deliver a multi-variant nontrivial branch family.

### P9-002 Imported Provenance Is Real In-Repo Provenance, Not External Evidence

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json`
  records a real dataset id, source hash, and conversion version,
- that same record points to a checked-in repository mesh source path,
- the failing harder quantitative target in the scorecard is labeled
  `targetBenchmarkClass = internal-benchmark`, not external measurement evidence.

Impact:

- Phase VIII removed fake imported metadata,
- but the imported tier is still an internal benchmark path.

### P9-003 Candidate Branch And Refinement Gates Still Lack Candidate-Linked Provenance

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/campaign_artifacts/dossiers/phase5_validation_dossier.json`
  now includes candidate-specific `evidenceRecordIds`,
- the same artifact still shows `branch-robust` and `refinement-bounded` failing with
  empty `evidenceRecordIds` and explanations that no candidate-linked branch/background ids exist.

Impact:

- Phase VIII closed the global leakage problem,
- but candidate-specific branch/refinement promotion remains blocked by missing provenance.

### P9-004 Paired First-Order Shiab Branch Evidence Is Still Inconclusive

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_first_order_shiab_real_atlas/atlas.json`
  also shows `1` admitted background and `3` rejected symmetric-ansatz solves,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/branch_robustness_record.json`
  is `inconclusive` for the same one-variant reason.

Impact:

- Phase VIII moved the non-identity Shiab path onto a nontrivial solve,
- but it still cannot support a real paired branch comparison.

### P9-005 Convergence Evidence Still Comes From A Bridge-Derived Ladder Seeded By One Admitted Variant

Evidence:

- the standard bridge export was generated from the single admitted background in
  `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_real_atlas/atlas.json`,
- the resulting refinement study remains cleanly convergent, but it is not backed by a
  richer admitted nontrivial family.

Impact:

- convergence coverage exists and passes,
- but its realism ceiling remains lower than the now-upgraded sidecar and classification paths.
