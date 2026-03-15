# PHASE_8_OPEN_ISSUES.md

This file records the open issues that remain after the Phase VII completion run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase7_evidence_campaign/20260315T130526Z`

It is the required Phase VIII gap ledger and is grounded in generated artifacts.

---

## Open Scientific Gaps

### P8-001 Standard Atlas Evidence Is Still Trivial Control Evidence

Evidence:

- `studies/phase5_su2_branch_refinement_env_validation/config/background_atlas.json`
  now points to real persisted upstream state artifacts,
- every admitted record still has `runClassification.isTrivialValidationPath = true`,
- the standard branch and convergence artifacts remain all-zero control outputs.

Impact:

- the real-atlas contract is closed,
- but scientific evidence quality is still open.

### P8-002 Imported-Tier Evidence Is Still Synthetic Example Provenance

Evidence:

- `campaign_artifacts/inputs/env_record_2.json` still uses synthetic dataset metadata,
- the copied dossier preserves that same synthetic provenance.

Impact:

- imported-tier contract coverage exists,
- external-data realism does not.

### P8-003 Three Sidecar Channels Are Still Bridge-Derived

Evidence:

- `campaign_artifacts/falsification/sidecar_summary.json` shows:
  - `observation-chain`: bridge-derived
  - `environment-variance`: bridge-derived
  - `representation-content`: bridge-derived
  - `coupling-consistency`: upstream-sourced

Impact:

- the standard run is no longer heuristic-only,
- but upstream evidence coverage is still partial.

### P8-004 Harder Quantitative Benchmarks Are Still Internal

Evidence:

- the only failing harder target in `quantitative/consistency_scorecard.json`
  is `bosonic-mode-2-imported-geometry-benchmark`,
- its provenance is internal benchmark provenance rather than an external measurement source.

Impact:

- Phase VII can separate control success from a harder miss,
- but not yet from a real-world miss.

### P8-005 Claim Escalation Is Still Too Global

Evidence:

- branch, convergence, and quantitative gate outcomes in the dossier are still
  shared broadly across candidates,
- candidate differentiation still depends mainly on direct falsifier hits.

Impact:

- candidate-level evidence ranking remains only partially trustworthy.

### P8-006 Shiab Expansion Is Still Partial And Trivial-Control

Evidence:

- the standard run remains identity-shiab,
- the linked companion run under `shiab_companion` uses `first-order-curvature`,
- that companion run uses the same trivial-control zero-seed atlas conditions.

Impact:

- there is now non-identity Shiab evidence in-repo,
- but Phase VIII must still move it into nontrivial standard evidence.
