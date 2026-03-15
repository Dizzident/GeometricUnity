# PHASE_10_OPEN_ISSUES.md

This file records the open issues that remain after the Phase IX completion run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z`

It is the required Phase X gap ledger and is grounded in generated artifacts.

---

## Open Scientific Gaps

### P10-001 Candidate Branch And Refinement Gates Still Lack Real Candidate-Linked Evidence IDs

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/dossiers/phase5_validation_dossier.json`
  still shows `branch-robust` and `refinement-bounded` failing with empty `evidenceRecordIds`,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/inputs`
  contains no `candidate_provenance_links.json`,
- the executed campaign therefore still blocks escalation because no candidate-linked
  branch/background provenance exists in real inputs.

Impact:

- Phase IX added optional code support for candidate-linked provenance,
- but Phase IX did not close the real executed candidate-evidence gap.

### P10-002 Imported Provenance Is Still Repo-Internal Rather Than External Evidence

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/inputs/env_record_2.json`
  records `datasetId = repo-internal-phase8-import-mesh-v1`,
- that same copied input still points to the checked-in repository mesh source spec,
- the failing quantitative target remains labeled `targetBenchmarkClass = internal-benchmark`.

Impact:

- Phase IX preserved honest imported provenance,
- but it did not produce genuine external evidence.

### P10-003 Standard Branch Evidence Is Evaluable But Still Mixed

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/branch/branch_robustness_record.json`
  reports the standard branch study as `mixed`,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/falsification/falsifier_summary.json`
  contains active branch-fragility falsifiers for `gauge-violation` and `solver-iterations`.

Impact:

- Phase IX closed the old inconclusive one-variant limit,
- but it did not establish branch robustness for the standard path.

### P10-004 Paired First-Order Shiab Branch Evidence Is Evaluable But Still Mixed

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/branch_robustness_record.json`
  reports `overallSummary = mixed`,
- that same artifact classifies `gauge-violation` and `solver-iterations` as fragile.

Impact:

- Phase IX closed the paired path’s inconclusive state,
- but broader Shiab scope still does not show branch robustness.

### P10-005 Convergence Evidence Is Still Bridge-Derived Rather Than Direct Solver-Backed

Evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/reports/phase5_report.md`
  states `Evidence source: bridge-derived from 4 admitted background record(s).`,
- the same report states `Direct solver-backed refinement family: no.`,
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/campaign_artifacts/inputs/bridge_manifest.json`
  shows the refinement ladder is seeded from the admitted atlas export.

Impact:

- Phase IX strengthened convergence provenance honesty and breadth,
- but it did not close the direct-refinement evidence gap.
