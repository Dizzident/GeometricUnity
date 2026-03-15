# IMPLEMENTATION_P6.md

## Purpose

This is the Claude-ready implementation handoff for Phase VI.

Use the post-batch evidence set:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/campaign_artifacts`
- logs: `/home/josh/Documents/GitHub/GeometricUnity/reports/phase5_gap_closure_batch/20260315T011924Z/logs`

as the binding starting state.

The current repository already has most of the Phase V code contracts. Phase VI
must focus on making the standard evidence campaign actually use them.

## Binding Decisions

### D-P6-001 Standard Campaign Contract Is No Longer Optional

The checked-in reference campaign must advance to `schemaVersion = "1.1.0"` and
must include:

- `observationChainPath`
- `environmentVariancePath`
- `representationContentPath`
- `couplingConsistencyPath`

For the primary checked-in campaign, omission of these fields is an open defect,
not an acceptable early-run simplification.

### D-P6-002 Coverage Must Be Explicit In Artifacts

Add explicit coverage accounting to the campaign outputs.

At minimum, falsifier outputs must record:

- observation-record count,
- environment-variance-record count,
- representation-record count,
- coupling-record count,
- whether each evidence channel was evaluated, skipped, or absent.

If a gate is skipped because inputs are missing, the dossier must say so.

### D-P6-003 The Main Evidence Path Must Use Bridged Upstream Artifacts

The primary Phase VI campaign must not remain dependent on hand-authored
branch/refinement value tables as its authoritative evidence input.

`BackgroundRecordBranchVariantBridge` is the authoritative bridge for branch
study values in the main evidence path.

### D-P6-004 Multi-Environment Means More Than Toy

The standard campaign must include:

- one toy environment record for continuity,
- one structured environment record,
- and, if real imported data is available, one imported environment record with
  `datasetId`, `sourceHash`, and `conversionVersion`.

Do not claim imported-tier evidence without those provenance fields.

### D-P6-005 Placeholder Targets Move Out Of The Main Claim Path

Phase VI may keep toy-placeholder targets for control studies, but the primary
evidence campaign must explicitly distinguish:

- placeholder targets,
- derived-but-still-synthetic targets,
- stronger evidence-grade targets.

All Phase VI target tables must set `distributionModel` explicitly.

### D-P6-006 Reproduction Metadata Must Be Concrete

The next-phase manifest and dossier outputs must stop at placeholder
reproduction strings only if the inputs are also copied into the campaign
output tree.

Preferred rule:

- copy authoritative inputs under `<out-dir>/inputs/`
- record concrete relative reproduction commands against those copied inputs.

## Milestones

### P6-M1 Campaign Contract Validation

Add a new CLI command:

```text
gu validate-phase5-campaign-spec --spec <campaign.json> [--require-reference-sidecars]
```

Required behavior:

- validate that every declared path exists,
- validate that the reference campaign includes the four sidecar fields when
  `--require-reference-sidecars` is used,
- validate that the campaign includes at least toy + structured environment
  evidence records,
- fail fast with a field-specific error.

Files:

- `apps/Gu.Cli/Program.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignSpec.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignArtifactLoader.cs`
- campaign JSON schema under `schemas/`
- reference study config under `studies/phase5_su2_branch_refinement_env_validation/config/`

Tests:

- validator rejects missing sidecar paths in reference mode,
- validator rejects missing files,
- validator accepts the checked-in reference campaign.

### P6-M2 Bridged Value Export

Add a new CLI command:

```text
gu export-phase5-bridge-values --atlas <background_atlas.json> --refinement-spec <spec.json> --out-dir <dir>
```

Outputs:

- `<dir>/branch_quantity_values.json`
- `<dir>/refinement_values.json`
- `<dir>/bridge_manifest.json`

Rules:

- branch values must be derived through `BackgroundRecordBranchVariantBridge`,
- bridge manifest must record source atlas path, source record IDs, and derived
  variant IDs,
- the reference campaign must consume these generated files instead of the
  hand-maintained tables once the bridge path is available.

Tests:

- deterministic bridge export on a fixed atlas,
- bridge manifest round-trips,
- reference campaign loader accepts the generated files.

### P6-M3 Sidecar Generation And Coverage Accounting

Add a new CLI command:

```text
gu build-phase5-sidecars --registry <registry.json> --observables <observables.json> --environment-record <env.json>... --out-dir <dir>
```

Outputs:

- `<dir>/observation_chain.json`
- `<dir>/environment_variance.json`
- `<dir>/representation_content.json`
- `<dir>/coupling_consistency.json`
- `<dir>/sidecar_summary.json`

Artifact requirements:

- `sidecar_summary.json` must report per-channel input counts and generation
  status,
- `falsifier_summary.json` must embed or reference the same coverage counts,
- `phase5_validation_dossier.json` must include `observationChainSummary` when
  observation records are supplied.

Tests:

- empty-but-present sidecars are distinguishable from omitted sidecars,
- no-trigger falsifier runs still report non-zero evaluated input counts,
- reference campaign dossier includes observation-chain summary when sidecars are present.

### P6-M4 Environment And Target Upgrade

Update the checked-in reference study inputs so that the primary campaign uses:

- toy and structured environments,
- imported environment evidence when dataset provenance is available,
- target tables with explicit `distributionModel`,
- non-Gaussian target examples where the uncertainty model justifies it.

Files:

- `studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`
- sidecar JSON files in the same config directory
- target tables and environment records

Tests:

- imported environment provenance round-trips through campaign loading,
- target matcher regression covers the exact reference-study distribution models,
- campaign validator rejects a primary evidence study that is still toy-only.

### P6-M5 Final Phase VI Reference Campaign

Once P6-M1 through P6-M4 are complete:

- rerun the full batch,
- require the checked-in reference campaign to pass the validator before
  `run-phase5-campaign`,
- verify that the final dossier reports observation coverage and falsifier
  coverage explicitly,
- update the Phase VI open-issues ledger from the actual artifact set.

Required final artifact properties:

- `phase5_validation_dossier.json` contains `observationChainSummary`,
- `falsifier_summary.json` carries evaluation coverage, not just trigger counts,
- the reference campaign no longer relies only on toy environment evidence,
- reproduction metadata points to concrete copied inputs or concrete checked-in inputs.

## Schemas And Artifact Contracts

Phase VI must add or update schemas for:

- `phase5_campaign.schema.json`
- `observation_chain.schema.json`
- `environment_variance.schema.json`
- `representation_content.schema.json`
- `coupling_consistency.schema.json`
- `sidecar_summary.schema.json`

The validator must treat schema mismatch as a hard error for the reference
campaign.

## Non-Goals

Do not spend Phase VI effort on:

- making the campaign live-solver-driven,
- adding new toy-only reports that do not improve evidence quality,
- broad new physics subsystems unrelated to evidence completion,
- claiming physical validation from placeholder targets.
