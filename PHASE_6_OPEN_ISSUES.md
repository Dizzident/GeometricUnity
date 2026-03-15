# PHASE_6_OPEN_ISSUES.md

## Scientific Limitations

### P6-001 Quantitative Success Is Still Toy-Placeholder Success

Evidence:

- `consistency_scorecard.json` reports `5/5` passing matches,
- the report explicitly states all targets are synthetic toy placeholders.

Impact:

- the clean quantitative result is useful as a control study,
- it is not evidence against or for physical observables.

### P6-002 The Checked-In Reference Campaign Is Still Toy-Environment Only

Evidence:

- `phase5_validation_dossier.json` contains a single `environmentSummary`
  entry for `env-toy-2d-trivial`,
- no structured or imported environment appears in the successful reference run.

Impact:

- environment robustness is not yet demonstrated in the standard evidence path.

### P6-003 The Primary Reference Campaign Is Not Yet Background-Bridged

Evidence:

- the successful campaign consumed pre-authored branch/refinement value tables,
- the artifact set does not show Phase III background-derived branch inputs as
  the authoritative source.

Impact:

- the reference evidence still depends on curated tables rather than the
  upstream persisted background atlas.

### P6-004 Identity-Only Shiab Still Dominates The Evidence Path

Evidence:

- the campaign provenance and branch IDs in the successful run remain
  `identity-shiab` variants,
- no non-identity Shiab variation appears in the checked-in reference study.

Impact:

- convergence and robustness evidence remain local to the identity-only Shiab
  slice.

## Infrastructure Limitations

### P6-005 The Standard Campaign Spec Still Undershoots The Intended Contract

Evidence:

- the current checked-in `campaign.json` still uses `schemaVersion = "1.0.0"`,
- it omits `observationChainPath`, `environmentVariancePath`,
  `representationContentPath`, and `couplingConsistencyPath`.

Impact:

- the standard checked-in campaign does not yet exercise the full Phase V
  evidence contract that the codebase now supports.

### P6-006 There Is No First-Class Campaign Validator

Evidence:

- the batch succeeds by attempting the campaign directly,
- there is no dedicated preflight command that verifies campaign completeness
  before the run starts.

Impact:

- configuration drift is caught late,
- missing sidecars or malformed reference campaigns can still fail only at run
  time.

## Evidence And Provenance Limitations

### P6-007 Observation-Chain Evaluation Is Not Present In The Successful Dossier

Evidence:

- `phase5_validation_dossier.json` omits `observationChainSummary`,
- the successful reference campaign did not supply observation sidecar inputs.

Impact:

- the observation-chain gate is not evidenced in the primary campaign,
- empty claim escalation is therefore not very informative.

### P6-008 Falsifier Outputs Do Not Show Coverage, Only Trigger Counts

Evidence:

- `falsifier_summary.json` reports `0` total falsifiers,
- the artifact does not report whether any observation, environment variance,
  representation, or coupling inputs were actually evaluated.

Impact:

- the current artifact set cannot distinguish "clean study" from "missing
  falsifier inputs."

### P6-009 Reproduction Metadata Still Uses Placeholder Command Arguments

Evidence:

- `study_manifest.json` and `validation_dossier.json` use
  `--spec <campaign.json> --out-dir <dir>` placeholder arguments.

Impact:

- command form is correct,
- reproduction specificity is still weaker than it should be for evidence-grade
  archival reruns.

## Intentionally Deferred Or Not Yet Exercised

### P6-010 Imported-Tier Evidence Exists In Code But Not In The Standard Campaign

The environment provenance fields are implemented, but the successful reference
campaign does not yet consume imported data.

### P6-011 Non-Gaussian Scoring Exists In Code But Not In The Standard Campaign

The target-matching code supports non-Gaussian models, but the successful
reference campaign remains Gaussian and toy-placeholder.

### P6-012 Deferred Falsifier Channels Exist In Code But Not In The Standard Campaign

Observation, environment variance, representation content, and coupling
inconsistency falsifiers are implemented and tested, but the standard campaign
does not yet provide their inputs.
