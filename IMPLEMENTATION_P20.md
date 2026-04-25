# IMPLEMENTATION_P20.md

## Purpose

Phase XX turns the Phase XIX diagnosis into a concrete source-generation
campaign.

Phase XIX proved that the W/Z physical comparison pipeline is now fail-closed:
it can report `blocked`, `predicted`, or `failed`, and it refuses to promote
placeholders, external PDG targets, incomplete uncertainty, provisional
mapping/calibration, or unvalidated mode-identification evidence.

The remaining primary blocker is upstream: no checked-in internal artifact
currently provides individual W and Z vector-boson candidate source modes with
selectors, identity evidence, and uncertainty. Phase XX should create the first
reproducible internal candidate-source campaign that either produces source
records for later W/Z identification or proves, with narrower blockers, why the
current internal artifacts cannot support them.

## Baseline

Current terminal W/Z campaign status:

- `study-runs/phase19_wz_physical_check/reports/phase5_report.json`;
- `physicalPredictionTerminalStatus.status == "blocked"`;
- blockers include no usable internal W/Z source, provisional W/Z evidence,
  unestimated ratio uncertainty, provisional mapping/calibration, and active
  severe falsifiers.

Current source inventory:

- `studies/phase19_dimensionless_wz_candidate_001/internal_mode_source_inventory.json`;
- all possible internal sources are classified as `insufficient` or
  `unrelated`;
- external physical target tables are explicitly excluded.

Most promising raw internal inputs:

- Phase12 bosonic spectra and mode records:
  `studies/phase12_joined_calculation_001/output/background_family/spectra/`;
- Phase12 mode families:
  `studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json`;
- Phase12 boson registry:
  `studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json`;
- Phase5 selector and uncertainty examples:
  `studies/phase5_su2_branch_refinement_env_validation/config/observables.json`;
- Phase5 branch/refinement/environment selector artifacts:
  `studies/phase5_su2_branch_refinement_env_validation/config/`.

## Phase XX Goal

At the end of Phase XX, the repository should have a reproducible internal
candidate-source campaign that emits one of:

- `candidate-source-ready`, if internal artifacts provide at least two
  selector-backed vector-boson candidate source modes suitable for later W/Z
  identity testing;
- `source-blocked`, if the current artifacts cannot support such source modes,
  with machine-readable blockers tied to specific missing fields, selectors, or
  uncertainty components.

Phase XX does not need to validate W and Z identity. It must not label a source
mode as W or Z. Its job is to produce or reject neutral internal source
candidates that a later phase can attempt to identify.

## Work Items

### P20-M1 Define Internal Source Candidate Contract

Status: complete.

Create a schema and C# model for internal vector-boson source candidates.

The record should include:

- source candidate id;
- source artifact paths;
- source mode ids and family ids;
- source origin, fixed to internal computed artifacts;
- mode role, such as `vector-boson-source-candidate`;
- eigenvalue or mass-like value;
- uncertainty components when available;
- branch, environment/background, and refinement selectors;
- stability, ambiguity, gauge-leak, and representation/symmetry fields when
  available;
- status: `candidate-source-ready`, `source-blocked`, or `insufficient`;
- assumptions;
- closure requirements;
- provenance.

Definition of done:

- schema exists;
- C# model exists;
- JSON round-trip tests exist;
- external target paths/origins are rejected by validation tests.

Suggested names:

- `schemas/internal_vector_boson_source_candidate.schema.json`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceCandidate.cs`

### P20-M2 Build Phase12 Source Adapter

Status: complete.

Implement a read-only adapter that extracts neutral source candidates from
Phase12 spectra, mode families, and boson registry artifacts.

Definition of done:

- adapter reads Phase12 boson registry and mode family artifacts;
- each emitted record references the contributing spectrum/mode paths;
- candidates remain particle-identity-neutral;
- candidates with branch fragility, ambiguity, missing branch selectors, missing
  refinement coverage, or missing uncertainty are marked `source-blocked` or
  `insufficient`;
- tests cover the checked-in Phase12 artifacts.

Important rule:

- Do not assign `w-boson` or `z-boson` particle ids in this phase.

Suggested touched areas:

- `src/Gu.Phase5.QuantitativeValidation`
- `tests/Gu.Phase5.QuantitativeValidation.Tests`

### P20-M3 Estimate Source-Level Uncertainty Or Block It

Status: complete for available Phase12 data; terminal result remains blocked.

Derive the uncertainty fields that can honestly be estimated from current
artifacts, and mark the rest blocked.

Required components:

- extraction spread from mass-like envelope or mode-family spread;
- branch variation, only if selector-backed branch variants exist;
- refinement error, only if refinement ladder data exists;
- environment/background sensitivity, only if multiple comparable backgrounds
  exist;
- total uncertainty only if all required components are estimated.

Definition of done:

- source candidate records do not use `-1` silently;
- missing uncertainty components create explicit closure requirements;
- tests show incomplete uncertainty prevents `candidate-source-ready`.

### P20-M4 Generate Checked-In Source Candidate Artifact

Status: complete.

Create a Phase20 study artifact from the adapter output.

Definition of done:

- artifact exists under a new study directory;
- artifact is reproducible from a CLI or documented command;
- all source records reference internal artifacts only;
- if no source is ready, the artifact still records why each candidate is
  blocked.

Suggested output:

- `studies/phase20_internal_vector_boson_sources_001/source_candidates.json`
- `studies/phase20_internal_vector_boson_sources_001/STUDY.md`

### P20-M5 Add Source Candidate CLI

Status: complete.

Add a command that regenerates the Phase20 source candidate artifact from
checked-in Phase12 inputs.

Definition of done:

- command accepts Phase12 registry/family/spectrum root paths and output path;
- command writes deterministic JSON;
- command refuses external target input paths;
- command exits nonzero on malformed input;
- command is documented in the Phase20 study.

Suggested command shape:

```bash
dotnet run --project apps/Gu.Cli -- generate-internal-vector-boson-sources \
  --registry studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json \
  --families studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json \
  --spectra-root studies/phase12_joined_calculation_001/output/background_family/spectra \
  --out studies/phase20_internal_vector_boson_sources_001/source_candidates.json
```

### P20-M6 Connect Source Candidates To P19 Extraction Contract

Status: complete for fail-closed bridge behavior.

Bridge source candidates to `CandidateModeSourceRecord` only when the source is
ready.

Definition of done:

- ready source candidates can be converted into candidate-mode source records;
- blocked/insufficient source candidates cannot be converted into physical mode
  candidates;
- tests verify blocked source candidates remain blocked by
  `CandidateModeExtractor`;
- P19 W/Z physical campaign remains blocked unless downstream W/Z identity
  evidence is separately validated.

### P20-M7 Re-run Physical And Regression Campaigns

Status: complete.

Validate that the new source-candidate layer does not accidentally open the
physical claim gate.

Required commands:

```bash
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase20_wz_physical_check --validate-first
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase20_reference_regression_check --validate-first
dotnet test GeometricUnity.slnx
```

Definition of done:

- all tests pass;
- Phase19 W/Z campaign remains terminal `blocked` unless all downstream W/Z
  gates are actually satisfied;
- Phase20 source candidate artifact has terminal status
  `candidate-source-ready` or `source-blocked`;
- report text contains no physical prediction language unless the physical
  claim gate passes.

## Recommended Execution Order

1. P20-M1: define source candidate schema/model.
2. P20-M2: implement Phase12 adapter.
3. P20-M3: implement uncertainty/blocker accounting.
4. P20-M4: generate checked-in Phase20 artifact.
5. P20-M5: add reproducible CLI command.
6. P20-M6: bridge ready sources into the P19 extraction contract.
7. P20-M7: run focused tests, campaigns, and full solution tests.

## Phase XX Completion Notes

Implemented files and artifacts:

- `schemas/internal_vector_boson_source_candidate.schema.json`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceCandidate.cs`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceCandidateAdapter.cs`
- `tests/Gu.Phase5.QuantitativeValidation.Tests/InternalVectorBosonSourceCandidateTests.cs`
- `apps/Gu.Cli/Program.cs`
- `studies/phase20_internal_vector_boson_sources_001/STUDY.md`
- `studies/phase20_internal_vector_boson_sources_001/source_candidates.json`

Generated artifact:

- `source_candidates.json` contains 12 Phase12 internal vector-boson source
  candidates;
- terminal status is `source-blocked`;
- all candidates are particle-identity-neutral;
- all candidates reference internal Phase12 artifacts only;
- all candidates remain blocked by missing branch selectors, ambiguous/unstable
  mode-family matching, weak claim class, and incomplete uncertainty.

Reproduction command:

```bash
dotnet run --project apps/Gu.Cli -- generate-internal-vector-boson-sources \
  --registry studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json \
  --families studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json \
  --spectra-root studies/phase12_joined_calculation_001/output/background_family/spectra \
  --out studies/phase20_internal_vector_boson_sources_001/source_candidates.json
```

Observed Phase XX result:

- Phase12 contains internal bosonic source material, but not a ready W/Z source
  layer;
- source extraction is now reproducible and fail-closed;
- downstream P19 physical prediction remains blocked unless a future phase
  supplies ready source candidates plus independent W/Z identity evidence.

Validation runs:

- `jq -e . studies/phase20_internal_vector_boson_sources_001/source_candidates.json`
- `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase20_wz_physical_check --validate-first`
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase20_reference_regression_check --validate-first`
- `dotnet test GeometricUnity.slnx`

## Guardrails

- Do not use external physical target values as source values.
- Do not assign W or Z identity in Phase XX.
- Do not mark a source `candidate-source-ready` with missing branch,
  environment, refinement, or uncertainty requirements unless the missing field
  is explicitly declared not applicable with evidence.
- Do not hide branch fragility or ambiguity.
- Do not promote mapping, calibration, or physical claim language from source
  candidates alone.
- Do not mutate Phase12 source artifacts; generate Phase20 derivatives.

## Plain-English Success Criteria

Phase XX succeeds if the project can regenerate an honest internal vector-boson
source-candidate artifact from checked-in computed data. The artifact may say
`source-blocked`. That is still progress if the blockers are specific enough to
drive the next computation.

Phase XX fails if it labels weak internal evidence as W/Z identity, copies PDG
targets into source candidates, or opens the physical prediction gate without
validated downstream evidence.
