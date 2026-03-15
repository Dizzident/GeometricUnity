# ARCH_P6.md

## Purpose

Phase VI is the first post-Phase-V phase whose main job is not to add more
validation scaffolding. Its job is to make the checked-in validation campaign
scientifically informative enough that a "clean" Phase V artifact set means more
than "the toy campaign ran."

The Phase V gap-closure batch succeeded on the current tree and produced a
verified artifact set under:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z`

That run confirms:

- branch robustness artifact present and reports `5/5` invariant quantities,
- convergence artifact present and reports `5/5` convergent quantities,
- quantitative scorecard present and reports `5/5` passing target matches,
- dual dossier emission works,
- the campaign runner is reproducible and batch-verifiable.

It also confirms the real boundary of the current evidence:

- the reference campaign still uses only toy-tier quantitative targets,
- the checked-in campaign still runs only a toy environment,
- `phase5_validation_dossier.json` omits `observationChainSummary`,
- `falsifier_summary.json` reports `0` falsifiers with no coverage accounting,
- claim escalation remains empty,
- the reference study does not yet show Phase III background-bridged evidence.

Phase VI therefore targets evidence completeness, not feature-count growth.

## Verified Starting State

The following are inherited prerequisites, not new work:

- `run-phase5-campaign` exists and emits the required Phase V output tree.
- The batch runner now executes reliably through the script path by delegating
  to a Node runner.
- The ten Phase V/Phase III batch test projects pass on the current tree.
- `Phase5CampaignArtifactLoader` can load optional sidecars.
- `ObservationChainRecord`, deferred falsifier sidecar record types,
  non-Gaussian target fields, imported-environment provenance fields,
  `BackgroundRecordBranchVariantBridge`, and `ShiabVariantId` already exist in
  code and tests.

This means Phase VI should not re-litigate whether those APIs exist. It should
use them in the standard evidence path.

## Architectural Goal

Phase VI should convert the current state:

- contract-complete tooling,
- toy-tier reference evidence,
- optional but mostly unused sidecar evidence,

into:

- a standard reference campaign whose inputs satisfy the full checked-in
  contract,
- a dossier that proves which evidence channels were actually evaluated,
- a branch/refinement/environment study backed by persisted upstream artifacts,
- a quantitative comparison path that is explicit about target realism and
  uncertainty model.

## Workstreams

### W1. Reference Campaign Contract Hardening

The checked-in standard campaign must become the authoritative evidence bundle,
not just a smoke-test config.

Required architectural change:

- `campaign.json` advances to the authoritative Phase V campaign contract and
  includes the sidecar paths needed by the current codebase.
- the reference campaign must declare at least toy plus structured environment
  evidence inputs,
- the campaign contract must be machine-validatable before a batch run starts.

### W2. Coverage-Visible Evidence Evaluation

Current artifacts cannot distinguish:

- "no falsifier inputs were supplied"
- from
- "inputs were supplied and all checks passed."

Phase VI must make evaluation coverage explicit in artifacts and dossiers.

Required architectural change:

- falsifier outputs must carry input counts and evaluation coverage,
- observation-chain outputs must be explicitly present when the reference
  campaign claims the observation gate was evaluated,
- the final dossier must tell the reader which gates were skipped, blocked,
  clean, or triggered.

### W3. Background-Backed Phase V Inputs

The reference campaign still relies on pre-authored values and analytic branch
inputs. Phase VI must move the main evidence path onto persisted Phase III/IV
artifacts.

Required architectural change:

- `BackgroundRecordBranchVariantBridge` becomes part of the standard campaign
  preparation flow,
- branch-family values, refinement values, and registry overlays should be
  generated from persisted upstream artifacts rather than hand-maintained
  reference tables.

### W4. Environment and Target Realism

The successful Phase V run is clean, but it is still a toy-only clean run.

Required architectural change:

- the primary reference campaign must run more than one environment tier,
- imported-environment provenance fields must appear in a real campaign input,
- target tables must explicitly carry distribution-model choices,
- toy-placeholder targets must move to smoke-test or control-study status once
  stronger targets exist.

### W5. Evidence-Bearing Phase VI Reference Campaign

All previous workstreams converge into one deliverable: a rerunnable campaign
whose artifacts support branch, convergence, environment, observation,
falsification, and quantitative claims with explicit coverage accounting.

The Phase VI reference campaign should remain analysis-first and artifact-driven.
It should not reintroduce live-solver ambiguity into the campaign command.

## Dependency Graph

`W1 -> W2 -> W5`

`W3 -> W5`

`W4 -> W5`

`W1` is first because the reference campaign contract must be stable before
coverage, bridge, or realism work can be trusted. `W2` is next because artifact
interpretability matters before new evidence is collected. `W3` and `W4` then
upgrade the actual evidence inputs consumed by the campaign.

## Exit Criteria

Phase VI should be considered complete only when all of the following are true:

- the standard checked-in campaign includes and validates its sidecar evidence
  inputs,
- the primary dossier includes explicit observation and falsifier coverage
  accounting,
- the reference campaign uses persisted upstream artifacts for its branch or
  refinement evidence path,
- the environment ladder is no longer toy-only,
- quantitative evidence clearly distinguishes placeholder versus evidence-grade
  targets,
- the Phase VI batch can rerun from the current tree and produce a dossier that
  is interpretable without reading source code.
