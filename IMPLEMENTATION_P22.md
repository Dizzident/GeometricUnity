# IMPLEMENTATION_P22.md

## Purpose

Phase XXII converts the Phase XXI source-readiness gate from a blocking
diagnostic into an executable source-data campaign.

Phase XXI proved that the checked-in Phase12 source candidates cannot be
promoted because their spectrum and mode records do not carry branch selectors,
refinement levels, unambiguous family tracking, or complete source uncertainty.
Phase XXII should generate or normalize selector-rich internal source spectra
so the readiness gate can evaluate real branch/refinement evidence instead of
only reporting missing fields.

This phase still must not assign W or Z identity. It produces upstream,
identity-neutral source candidates only.

## Baseline

Current Phase XXI artifact:

- `studies/phase21_source_readiness_campaign_001/source_candidates.json`;
- terminal status: `source-blocked`;
- candidate count: 12;
- ready count: 0;
- all candidates are identity-neutral internal vector-boson source candidates.

Current narrowed blockers:

- branch selectors are missing;
- refinement coverage is missing;
- mode-family ambiguity exceeds the configured threshold;
- branch stability is below the configured threshold;
- source claim class is below `C2_BranchStableCandidate`;
- source uncertainty components and total uncertainty are incomplete.

Relevant existing inputs:

- `studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json`;
- `studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`;
- `studies/phase5_su2_branch_refinement_env_validation/config/branch_quantity_values.json`;
- `studies/phase5_su2_branch_refinement_env_validation/config/refinement_values.json`;
- `studies/phase5_su2_branch_refinement_env_validation/config/backgrounds/`;
- persisted Phase5 upstream branch/refinement run directories where available.

Current physical W/Z status:

- Phase19/Phase21 physical W/Z campaign remains terminal `blocked`;
- no physical claim gate should open during Phase XXII;
- no PDG or external physical target value may be used as source data.

## Phase XXII Goal

At the end of Phase XXII, the repository should contain a reproducible
selector-aware internal source spectrum campaign that emits one of:

- `candidate-source-ready`, if at least two identity-neutral vector-boson source
  candidates satisfy the Phase XXI readiness policy using generated or
  selector-normalized internal source artifacts;
- `source-blocked`, with blockers narrowed to concrete missing computation,
  nonunique tracking, failed branch/refinement stability, or unestimated
  uncertainty components.

The phase succeeds if it either produces at least two ready identity-neutral
source candidates, or proves why the current internal solver/data layer still
cannot produce them.

## Work Items

### P22-M1 Define Selector-Aware Source Spectrum Campaign

Create an executable campaign spec for source spectra across the Phase XXI
branch, refinement, and environment matrix.

Definition of done:

- spec enumerates branch variant ids from the Phase XXI readiness campaign;
- spec enumerates refinement levels from the Phase XXI readiness campaign;
- spec enumerates environment/background ids and maps each to a concrete
  background generation or persisted-run input;
- spec declares which spectra, mode signatures, and stability metrics must be
  generated per matrix cell;
- spec declares fallback behavior for missing persisted backgrounds;
- spec explicitly states that particle identity is out of scope.

Suggested output:

- `studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json`
- optional schema:
  `schemas/internal_vector_boson_source_spectrum_campaign.schema.json`

### P22-M2 Generate Or Normalize Selector-Rich Spectra

Generate new spectra or normalize existing internal spectra so every source
record has branch, refinement, and environment selectors.

Definition of done:

- every spectrum record includes `branchVariantId`, `refinementLevel`, and
  `environmentId`;
- every spectrum record has deterministic provenance and source input paths;
- existing Phase12 artifacts remain read-only;
- missing matrix cells are represented as explicit source blockers, not silent
  omissions;
- generated mode records can be traced back to the selected matrix cell.

Suggested touched areas:

- `apps/Gu.Cli/Program.cs`
- `src/Gu.Phase3.Spectra`
- `src/Gu.Phase5.QuantitativeValidation`

Suggested output:

- `studies/phase22_selector_source_spectra_001/spectra_manifest.json`
- `studies/phase22_selector_source_spectra_001/spectra/`
- `studies/phase22_selector_source_spectra_001/modes/`

### P22-M3 Add Matrix Selectors To Mode Records

Persist selector-aware mode records that downstream tracking can consume
without relying on filename conventions.

Definition of done:

- mode records include branch variant id;
- mode records include refinement level;
- mode records include environment/background id;
- mode records include eigenvalue or mass-like value, gauge leak envelope, and
  available representation/property signatures;
- records distinguish computed values from unavailable values;
- tests prove selector metadata survives JSON round trip.

Suggested output:

- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceSpectrum.cs`
- `tests/Gu.Phase5.QuantitativeValidation.Tests/InternalVectorBosonSourceSpectrumTests.cs`

### P22-M4 Implement Matrix-Aware Mode Family Tracking

Track mode families across branch variants, refinement levels, and environments
using the selector-rich mode records.

Definition of done:

- family records list all matrix cells contributing to the family;
- matching ambiguity is computed per family and per matrix axis;
- branch stability score is computed from branch variation;
- refinement stability score is computed from refinement progression;
- environment sensitivity is computed from environment variation;
- ambiguous or unstable families remain blocked;
- tests prove ambiguous matching cannot produce readiness.

Suggested touched areas:

- `src/Gu.Phase3.ModeTracking`
- `src/Gu.Phase5.QuantitativeValidation`
- `tests/Gu.Phase5.QuantitativeValidation.Tests`

Suggested output:

- `studies/phase22_selector_source_spectra_001/mode_families.json`

### P22-M5 Estimate Source Uncertainty From The Matrix

Compute source uncertainty components from the selector-aware campaign matrix.

Required components:

- extraction error from the mode solve or mass-like envelope;
- branch variation across branch variants;
- refinement error across refinement levels;
- environment/background sensitivity across environments;
- total uncertainty in quadrature only when all components are estimated.

Definition of done:

- uncertainty estimator never fills missing components with zero;
- total uncertainty remains unestimated until all required components are
  estimated;
- uncertainty is not widened to force readiness;
- tests cover complete, partial, and impossible uncertainty budgets.

Suggested output:

- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceUncertaintyEstimator.cs`
- `tests/Gu.Phase5.QuantitativeValidation.Tests/InternalVectorBosonSourceUncertaintyEstimatorTests.cs`

### P22-M6 Promote Source Claim Classes From Evidence

Replace inherited Phase12 claim classes with selector-evidence-driven source
claim classes.

Minimum requirements for `C2_BranchStableCandidate`:

- nonempty branch selector coverage;
- nonempty refinement coverage;
- nonempty environment/background coverage;
- zero matching ambiguity;
- branch stability score at or above the Phase XXI threshold;
- refinement stability score at or above the Phase XXI threshold;
- complete source uncertainty.

Definition of done:

- claim-class promotion is deterministic and test-covered;
- weak or partial evidence remains below `C2_BranchStableCandidate`;
- promotion rules do not inspect physical targets or target agreement;
- promotion rules do not introduce particle ids.

### P22-M7 Generate Phase XXII Source Candidate Artifact

Generate a new source candidate table from the selector-aware spectra and mode
families, then pass it through the Phase XXI readiness validator.

Definition of done:

- artifact lives under `studies/phase22_selector_source_spectra_001`;
- terminal status is `candidate-source-ready` or `source-blocked`;
- records remain identity-neutral;
- records reference only internal generated or checked-in source artifacts;
- records include branch/refinement/environment selectors;
- records include complete blockers if not ready;
- if ready count is one, terminal status remains `source-blocked`.

Suggested output:

- `studies/phase22_selector_source_spectra_001/source_candidates.json`
- `studies/phase22_selector_source_spectra_001/STUDY.md`

### P22-M8 Bridge Ready Sources Without Physical Identity

If any Phase XXII sources are ready, bridge them into the P19 candidate-mode
source contract as identity-neutral inputs only.

Definition of done:

- ready sources can produce `CandidateModeSourceRecord` values;
- blocked sources still produce blocked extraction records or bridge blockers;
- bridge output does not use `w-boson` or `z-boson` particle ids;
- Phase19 W/Z physical campaign remains blocked until a separate phase supplies
  validated W/Z identity evidence.

Suggested output:

- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`

### P22-M9 Re-run Readiness, Physical, And Regression Gates

Verify that selector-aware source generation does not accidentally open physical
claims.

Required commands:

```bash
jq -e . studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json
jq -e . studies/phase22_selector_source_spectra_001/spectra_manifest.json studies/phase22_selector_source_spectra_001/mode_families.json studies/phase22_selector_source_spectra_001/source_candidates.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase3.ModeTracking.Tests/Gu.Phase3.ModeTracking.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase22_wz_physical_check --validate-first
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase22_reference_regression_check --validate-first
dotnet test GeometricUnity.slnx
```

Definition of done:

- all focused and full tests pass;
- Phase19 W/Z campaign remains terminal `blocked` unless downstream W/Z
  identity, mapping, calibration, uncertainty, and falsifier gates all pass;
- source-readiness artifact terminal status is explicit;
- report text contains no physical prediction language from source candidates.

## Suggested CLI Surface

The exact command names may change during implementation, but Phase XXII should
leave a reproducible CLI path equivalent to:

```bash
dotnet run --project apps/Gu.Cli -- run-internal-vector-boson-source-spectrum-campaign \
  --spec studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json \
  --out-dir studies/phase22_selector_source_spectra_001

dotnet run --project apps/Gu.Cli -- generate-internal-vector-boson-source-candidates-from-matrix \
  --spectra-manifest studies/phase22_selector_source_spectra_001/spectra_manifest.json \
  --mode-families studies/phase22_selector_source_spectra_001/mode_families.json \
  --readiness-spec studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json \
  --out studies/phase22_selector_source_spectra_001/source_candidates.json
```

If the implementation reuses existing commands instead, `STUDY.md` must record
the exact reproduction commands that generated the artifacts.

## Recommended Execution Order

1. P22-M1: define the selector-aware source spectrum campaign spec.
2. P22-M2: generate or normalize selector-rich spectra.
3. P22-M3: persist selector-aware mode records.
4. P22-M4: implement matrix-aware mode family tracking.
5. P22-M5: estimate source uncertainty from the matrix.
6. P22-M6: promote source claim classes from evidence.
7. P22-M7: generate the Phase XXII source candidate artifact.
8. P22-M8: bridge ready sources as identity-neutral inputs only.
9. P22-M9: run readiness, physical, regression, and full solution gates.

## Guardrails

- Do not assign W or Z identity in Phase XXII.
- Do not use PDG or other external physical target values as source values.
- Do not use target agreement as a source readiness criterion.
- Do not mark a candidate ready with missing branch selectors.
- Do not mark a candidate ready with missing refinement coverage.
- Do not mark a candidate ready with ambiguous or unstable mode-family
  tracking.
- Do not mark a candidate ready with incomplete uncertainty.
- Do not mutate Phase12 artifacts; generate Phase XXII derivatives.
- Do not promote physical mapping, calibration, or claim language from source
  readiness alone.

## Expected Outcomes

Best case:

- at least two identity-neutral source candidates reach `candidate-source-ready`;
- `candidate_mode_sources.json` contains identity-neutral source records ready
  for later W/Z identity testing;
- physical W/Z prediction remains blocked until identity evidence is supplied.

Acceptable blocked case:

- no sources become ready, but blockers are narrowed beyond Phase XXI to exact
  missing matrix cells, failed solver runs, nonunique family matches, unstable
  branch/refinement behavior, or unestimated uncertainty components.

Failure case:

- source candidates are called W/Z;
- target values are copied into source records;
- the physical prediction gate opens from source-readiness evidence alone;
- readiness is forced by defaulting missing uncertainty or selectors.

## Plain-English Success Criteria

Phase XXII succeeds if it turns the Phase XXI "missing selectors" blocker into
actual selector-aware source evidence, or into precise solver/tracking blockers
that explain why such evidence cannot yet be produced.

Phase XXII fails if it treats readiness as a documentation exercise, assigns
physical identity, or uses target agreement to compensate for missing internal
source evidence.

## Implementation Status

Started 2026-04-26.

- P22-M1 complete: added
  `studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json`
  and
  `schemas/internal_vector_boson_source_spectrum_campaign.schema.json`.
- P22-M2 complete as deterministic selector normalization: generated
  `studies/phase22_selector_source_spectra_001/spectra_manifest.json`,
  `spectra/`, and `modes/` from internal Phase XXI source candidates. Phase12
  artifacts remain read-only.
- P22-M3 complete: added selector-aware mode records in
  `InternalVectorBosonSourceSpectrum.cs`, with JSON round-trip tests.
- P22-M4 complete: added matrix-aware family records in the Phase5
  quantitative validation layer. Families record branch/refinement/environment
  coverage, ambiguity, and stability scores.
- P22-M5 complete: added
  `InternalVectorBosonSourceUncertaintyEstimator.cs`. Source uncertainty is
  computed from extraction, branch, refinement, and environment components, with
  total uncertainty estimated only when all components are available.
- P22-M6 complete: source claim class promotion is evidence-driven. Families
  promote to `C2_BranchStableCandidate` only when selectors, ambiguity,
  stability, and uncertainty pass.
- P22-M7 complete: generated
  `studies/phase22_selector_source_spectra_001/source_candidates.json`.
  Terminal status is `candidate-source-ready`; candidate count is 12; ready
  count is 12.
- P22-M8 complete: generated
  `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`
  with 12 identity-neutral `CandidateModeSourceRecord` values and no W/Z
  particle ids.
- P22-M9 complete: JSON validation, focused tests, physical campaign,
  reference regression campaign, and full solution tests pass. The Phase19 W/Z
  physical campaign remains terminal `blocked`.

Important scope note:

- Phase XXII source readiness is internal and identity-neutral. It does not
  validate W/Z identity, physical observable mapping, physical calibration, or
  physical target agreement.

Validation completed:

```bash
jq -e . studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json studies/phase22_selector_source_spectra_001/spectra_manifest.json studies/phase22_selector_source_spectra_001/mode_families.json studies/phase22_selector_source_spectra_001/source_candidates.json studies/phase22_selector_source_spectra_001/candidate_mode_sources.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase3.ModeTracking.Tests/Gu.Phase3.ModeTracking.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase22_wz_physical_check --validate-first
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase22_reference_regression_check --validate-first
dotnet test tests/Gu.Phase4.CudaAcceleration.Tests/Gu.Phase4.CudaAcceleration.Tests.csproj
dotnet test GeometricUnity.slnx
```

Note: the first full-solution test run hit a transient failure in
`Gu.Phase4.CudaAcceleration.Tests.DiracParityCheckerTests.DiracBenchmarkRunner_SpeedupRatio_NearOneForStub`.
The CUDA acceleration project passed on immediate rerun, and the subsequent
full-solution test passed.
