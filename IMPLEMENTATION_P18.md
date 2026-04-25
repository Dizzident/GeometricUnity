# IMPLEMENTATION_P18.md

## Purpose

Phase XVIII resolves the remaining blockers between the current physical
prediction scaffolding and a first honest physical-boson comparison.

Phase XVII made the infrastructure fail-closed:

- physical observable mappings exist;
- observable classifications exist;
- physical calibration records exist;
- physical target evidence exists;
- physical prediction projection records exist;
- positive mode-ratio extraction exists;
- identified physical mode records exist.

The outstanding blocker is no longer external data. The project has enough
authoritative target values for a first W/Z dimensionless comparison. The
blocker is internal: the repository does not yet have a validated derivation and
evidence chain proving that any computed internal modes are physical W and Z
modes.

## Baseline

Current physical target data available in checked-in inactive studies:

- W boson mass: `80.3692 +/- 0.0133 GeV`;
- Z boson mass: `91.1880 +/- 0.0020 GeV`;
- W/Z mass ratio: `0.88136 +/- 0.00015`;
- Higgs mass: `125.20 +/- 0.11 GeV`.

Current inactive W/Z candidate scaffold:

- `studies/phase19_dimensionless_wz_candidate_001/candidate_modes.json`;
- `studies/phase19_dimensionless_wz_candidate_001/candidate_observables.json`;
- provisional W and Z mode placeholders;
- provisional mapping to `physical-w-z-mass-ratio`;
- provisional dimensionless identity normalization;
- PDG W/Z target table copied into the isolated study.

Current claim status:

- physical mode records are provisional;
- W/Z mapping is provisional;
- calibration is provisional;
- total physical-mode uncertainty is unestimated;
- severe falsifiers still block physical prediction language.

## Phase XVIII Goal

At the end of this phase, the repository should be able to run a separate
physical W/Z comparison campaign that reports exactly one of:

- `predicted`, if all mode-identification, uncertainty, mapping, calibration,
  target, and falsifier gates pass;
- `failed`, if a validated physical prediction disagrees with the target;
- `blocked`, with explicit reasons, if any bridge element is missing.

It is acceptable for Phase XVIII to end in `blocked`. It is not acceptable to
promote placeholders or PDG-derived target values as theory outputs.

## Work Items

### P18-M1 Add Mode-Identification Evidence Contract

Status: complete for fail-closed infrastructure; provisional for physical
science content.

Create a schema and model for evidence that connects a computed mode to a
physical particle identity.

The evidence record should include:

- evidence id;
- mode id;
- particle id;
- mode kind, such as `vector-boson-mass-mode`;
- source observable ids;
- environment, branch, and refinement selectors;
- derivation or algorithm id;
- validation status: `validated`, `provisional`, or `blocked`;
- assumptions;
- closure requirements;
- provenance.

Definition of done:

- schema file exists: `schemas/mode_identification_evidence.schema.json`;
- C# model exists:
  `src/Gu.Phase5.QuantitativeValidation/ModeIdentificationEvidenceRecord.cs`;
- provisional Phase XIX W and Z evidence records are checked in:
  `studies/phase19_dimensionless_wz_candidate_001/mode_identification_evidence.json`;
- validated physical mode records are rejected for prediction unless matching
  validated evidence exists.

### P18-M2 Enforce Mode Evidence In Ratio Extraction

Status: complete for prediction-gated ratio extraction.

Connect `IdentifiedPhysicalModeRecord` to mode-identification evidence.

Definition of done:

- extractor and validator reject physical mode records with no evidence record;
- extractor and validator reject provisional/blocked evidence for physical
  prediction paths;
- errors are explicit for missing evidence, non-validated evidence, selector
  mismatch, non-validated mode status, invalid uncertainty, and missing source
  observables;
- tests cover validated, missing-evidence, and provisional-evidence paths.

### P18-M3 Replace Placeholder W/Z Modes With Computed Candidate Inputs

Status: blocked and carried forward.

Add a candidate-generation path for W and Z mode records from repository
computed artifacts.

This does not require the modes to be validated yet. It does require them to be
computed independently from PDG target values.

Definition of done:

- candidate W and Z mode records are not yet generated from internal computed
  data;
- existing candidate values remain inactive scaffolding and are not promoted as
  theory outputs;
- provenance points to the computation artifact, branch, and environment;
- total uncertainty is estimated or the records remain blocked;
- target coverage blocker records document the missing computation and
  validation bridge.

### P18-M4 Close Uncertainty For W/Z Ratio

Status: blocked and carried forward.

Estimate enough uncertainty to make a dimensionless W/Z comparison meaningful.

Required components:

- extraction uncertainty for W mode;
- extraction uncertainty for Z mode;
- branch variation;
- refinement error;
- environment sensitivity;
- propagated ratio uncertainty.

Definition of done:

- no physical-mode input with unknown uncertainty is accepted for prediction;
- Phase XIX W/Z mode inputs remain provisional because their uncertainty budget
  is not closed;
- the physical campaign reports a blocker instead of producing a prediction.

### P18-M5 Validate Mapping And Calibration For Dimensionless W/Z Ratio

Status: blocked and enforced fail-closed.

Promote the W/Z mapping and normalization only if P18-M1 through P18-M4 pass.

Definition of done:

- mapping status remains `provisional`;
- calibration status remains `provisional`;
- campaign validation and reporting keep the physical claim blocked when
  mapping, classification, calibration, or evidence are not validated;
- provisional records remain acceptable in inactive studies but cannot activate
  a physical claim.

### P18-M6 Create First Physical W/Z Campaign

Status: complete as a blocked physical comparison campaign.

Create a separate campaign for the W/Z mass-ratio target.

Definition of done:

- the reference benchmark campaign remains unchanged and still validates;
- the physical campaign lives at
  `studies/phase19_dimensionless_wz_candidate_001/config/campaign.json`;
- the physical campaign references the W/Z physical target table and explicit
  target coverage blockers;
- campaign output includes physical prediction records;
- the campaign reports `blocked` without ambiguous
  benchmark language.

### P18-M7 Resolve Or Carry Severe Falsifiers

Status: carried as explicit blockers.

Physical claims remain blocked while fatal/high falsifiers are active.

Definition of done:

- active fatal/high falsifiers are copied into the physical campaign through the
  Phase V sidecar summary;
- the physical report carries them as blockers;
- the physical claim gate cannot pass while unresolved severe falsifiers remain.

### P18-M8 External Data Refresh Gate

Status: complete for gating; data refresh itself is not the current blocker.

External target data is not the current blocker, but it should be refreshable
before any physical campaign is published.

Definition of done:

- PDG source URLs and retrieval dates remain checked into the physical target
  table;
- W, Z, Higgs, and W/Z target values are updated if sources changed;
- source and retrieval metadata remain required for physical targets;
- tests prevent physical targets without citations or retrieval dates.

## Recommended Execution Order

1. P18-M1: define mode-identification evidence.
2. P18-M2: enforce evidence for physical modes.
3. P18-M3: generate candidate W/Z mode inputs from internal artifacts.
4. P18-M4: estimate uncertainty and ratio uncertainty.
5. P18-M5: promote mapping/calibration only if evidence supports promotion.
6. P18-M6: create the separate physical W/Z campaign.
7. P18-M7: resolve or explicitly carry severe falsifiers.
8. P18-M8: refresh external target data before publishing results.

## Guardrails

- Do not copy PDG target values into computed observable records.
- Do not mark a mode as `validated` without a matching evidence record.
- Do not activate physical targets in the reference benchmark campaign.
- Do not widen uncertainties just to pass a target.
- Do not bypass fatal/high falsifiers.
- Do not claim W/Z prediction if the result is only an internal benchmark ratio.

## Phase XVIII Completion Notes

Implemented files:

- `src/Gu.Phase5.QuantitativeValidation/ModeIdentificationEvidenceRecord.cs`
- `src/Gu.Phase5.QuantitativeValidation/PhysicalModeEvidenceValidator.cs`
- `src/Gu.Phase5.QuantitativeValidation/SpectrumObservableExtractor.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignSpec.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignArtifactLoader.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignSpecValidator.cs`
- `apps/Gu.Cli/Program.cs`
- `schemas/mode_identification_evidence.schema.json`
- `schemas/identified_physical_mode.schema.json`
- `schemas/phase5_campaign.schema.json`
- `studies/phase19_dimensionless_wz_candidate_001/config/campaign.json`
- `studies/phase19_dimensionless_wz_candidate_001/config/sidecar_summary.json`
- `studies/phase19_dimensionless_wz_candidate_001/mode_identification_evidence.json`
- `studies/phase19_dimensionless_wz_candidate_001/target_coverage_blockers.json`

Validation runs:

- `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase18_wz_physical_blocked_check --validate-first`
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase18_reference_regression_check --validate-first`
- `dotnet test GeometricUnity.slnx --no-build`

Observed physical W/Z campaign result:

- campaign spec validation: OK;
- physical boson prediction: blocked;
- physical mapping: blocked because no validated physical observable mapping is
  present;
- observable classification: blocked because no computed observable is
  classified as a physical observable;
- calibration: blocked because no physical unit or scale calibration is
  present;
- falsifiers: blocked by active fatal/high falsifiers.

Remaining blockers:

- derive W and Z candidate modes from internal computed artifacts rather than
  placeholders;
- validate mode-identification evidence for those modes;
- close the extraction, branch, refinement, environment, and propagated ratio
  uncertainty budget;
- validate the dimensionless W/Z mapping and identity calibration;
- resolve or explicitly demote the severe falsifiers with evidence.

## Plain-English Success Criteria

Phase XVIII succeeds if the repository can run a W/Z physical comparison that is
scientifically honest: either it produces a validated physical prediction, it
fails against the target, or it stays blocked with precise missing evidence.

The phase fails if it gets a green report by treating placeholders, target
values, or benchmark eigenvalue ratios as physical W/Z mode predictions.
