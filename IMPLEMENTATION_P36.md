# Phase XXXVI - Selector-Backed W/Z Spectrum Campaign

## Goal

Phase XXXVI replaces the Phase22 selector proxy matrix with a solver-backed
selector spectrum campaign. P35 showed that the current selector spectra are
`massLikeValues` projections with no operator bundle, solver method, or mode
list, so physical W/Z comparison must remain blocked until selector cells are
materialized as real operator/eigenvalue solves.

## Current Blocker

The repository has a real spectrum path in `gu compute-spectrum`, but it
requires a materialized run folder/background tuple:

- background record;
- persisted branch manifest;
- persisted omega state;
- persisted A0 state or explicit fallback;
- persisted or derivable geometry context;
- resolvable environment;
- operator bundle construction;
- eigensolver output.

The Phase22 campaign only carries selector labels:

- `branchVariantIds`;
- `refinementLevels`;
- `environmentIds`;
- source candidate ids.

Those labels are not enough to call the existing solver without first creating
or locating selector-specific background states.

## Required Implementation

1. Add a selector-cell materialization layer.

   For each `(sourceCandidateId, branchVariantId, refinementLevel,
   environmentId)` cell, resolve or generate:

   - a background id;
   - a background record;
   - branch manifest;
   - omega state;
   - A0 state;
   - geometry context;
   - environment record.

2. Add a solver-backed source spectrum campaign path.

   Replace `InternalVectorBosonSourceMatrixCampaign.CreateModeRecord` for the
   physical-prediction path with a path that:

   - builds a real `LinearizedOperatorBundle`;
   - runs `EigensolverPipeline`;
   - selects the candidate-linked mode from the solved spectrum;
   - writes spectrum artifacts with `operatorBundleId`, `solverMethod`,
     `operatorType`, and mode records.

3. Preserve fail-closed behavior.

   Physical promotion must remain blocked if any selected W/Z cell lacks
   solver-backed evidence or if the campaign falls back to deterministic
   offsets.

4. Re-run the W/Z path.

   After solver-backed selector spectra exist, regenerate:

   - selector source candidates;
   - candidate mode sources;
   - identity readiness;
   - physical promotion artifacts;
   - P31/P34/P35 diagnostics.

## Acceptance Criteria

- The selected W/Z selector spectra have `solverBackedCellCount > 0` in the P35
  audit.
- No selected W/Z selector spectrum cell is proxy-only.
- P35 terminal status becomes
  `wz-selector-spectrum-independent-evidence-present`.
- P34 no longer reports that the ratio is only preserved through proxy spectra.
- The physical W/Z comparison is attempted only after the above conditions pass.

## Deliverables

- New selector-cell materialization code or a documented blocker artifact if
  required selector background inputs are absent.
- New solver-backed source spectrum campaign command or an extension to
  `run-internal-vector-boson-source-spectrum-campaign`.
- Updated tests proving proxy spectra fail and solver-backed spectra pass.
- Regenerated Phase22-derived W/Z artifacts under a new study directory, not
  overwriting the existing proxy study.
