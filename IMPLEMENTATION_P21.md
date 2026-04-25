# IMPLEMENTATION_P21.md

## Purpose

Phase XXI upgrades the Phase XX internal source-candidate layer from a
read-only diagnosis of old Phase12 artifacts into a reproducible
branch/refinement-aware source-readiness campaign.

Phase XX produced 12 identity-neutral internal vector-boson source candidates,
but every candidate remained `source-blocked` because the checked-in Phase12
artifacts lack:

- branch selectors;
- unambiguous stable mode-family tracking;
- strong source claim class;
- refinement coverage;
- complete source uncertainty.

Phase XXI should attack those blockers directly. It still must not assign W or
Z identity. The output is a stronger internal source layer that later phases can
try to identify, not a physical prediction.

## Baseline

Current source-candidate artifact:

- `studies/phase20_internal_vector_boson_sources_001/source_candidates.json`;
- terminal status: `source-blocked`;
- candidate count: 12;
- source origin: internal Phase12 computed artifacts only;
- all candidates are particle-identity-neutral.

Current recurring blockers:

- `branch selectors are missing from the Phase12 candidate`;
- `mode family matching is ambiguous`;
- `mode family is not marked stable`;
- `candidate claim class is not strong enough for a ready source candidate`;
- `source uncertainty budget is incomplete`.

Current physical W/Z status:

- Phase19/P20 W/Z campaign remains terminal `blocked`;
- no physical claim gate should open during Phase XXI;
- no PDG or external physical target value may be used as source data.

## Phase XXI Goal

At the end of Phase XXI, the repository should have a reproducible source
readiness campaign that emits one of:

- `candidate-source-ready`, if at least two identity-neutral vector-boson source
  candidates have branch selectors, stable tracking, refinement coverage, and a
  complete uncertainty budget;
- `source-blocked`, with narrower blockers tied to the exact missing campaign
  dimension if ready candidates still cannot be produced.

The phase succeeds even if the terminal status remains `source-blocked`,
provided it proves which missing computation or selector dimension blocks
readiness.

## Work Items

### P21-M1 Define Source Readiness Campaign Spec

Create a campaign spec for generating source candidates across branch variants,
refinement levels, and environments/backgrounds.

Definition of done:

- spec lists branch variant ids;
- spec lists refinement levels;
- spec lists environment/background ids;
- spec lists source quantities to extract from spectra and mode records;
- spec declares readiness thresholds for stability, ambiguity, and uncertainty;
- spec explicitly states that particle identity is out of scope.

Suggested output:

- `studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json`
- optional schema:
  `schemas/internal_vector_boson_source_readiness_campaign.schema.json`

### P21-M2 Generate Or Normalize Spectra Across The Campaign Matrix

Create or normalize spectrum inputs so each candidate source can be tied to
branch, refinement, and environment selectors.

Definition of done:

- every source spectrum record has branch id, refinement level, and environment
  or background id;
- existing Phase12 spectra are either adapted with explicit selector blockers or
  replaced by regenerated campaign spectra;
- missing spectra are reported as source-readiness blockers;
- generated artifacts are deterministic and reproducible;
- Phase12 source artifacts are not mutated.

Suggested output:

- `studies/phase21_source_readiness_campaign_001/spectra_manifest.json`

### P21-M3 Implement Branch/Refinement-Aware Mode Tracking

Track mode families across the source-readiness matrix and quantify ambiguity.

Definition of done:

- mode-family records include branch selectors;
- mode-family records include refinement levels;
- mode-family records include environment/background selectors;
- matching ambiguity is recorded per family;
- unstable or ambiguous families remain blocked;
- tests prove ambiguous matching cannot yield `candidate-source-ready`.

Suggested touched areas:

- `src/Gu.Phase3.ModeTracking`
- `src/Gu.Phase5.QuantitativeValidation`
- `tests/Gu.Phase5.QuantitativeValidation.Tests`

### P21-M4 Define Source Claim-Class Promotion Rules

Replace ad hoc source readiness decisions with explicit promotion rules.

Minimum requirements for `candidate-source-ready`:

- nonempty branch selectors;
- nonempty refinement coverage;
- nonempty environment/background selectors;
- stable mode family;
- ambiguity count of zero;
- claim class at or above the configured readiness threshold;
- no unverified GPU-only provenance;
- complete uncertainty budget.

Definition of done:

- promotion rules are implemented as a validator;
- blocked records include precise closure requirements;
- tests cover every failed promotion condition;
- no physical particle id is introduced by the validator.

Suggested output:

- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceReadinessValidator.cs`

### P21-M5 Close Source-Level Uncertainty

Compute uncertainty components from the source-readiness campaign matrix.

Required components:

- extraction error from mode solve or mass-like envelope;
- branch variation across branch variants;
- refinement error across refinement levels;
- environment/background sensitivity across environments;
- total uncertainty in quadrature only when all components are estimated.

Definition of done:

- ready candidates have `QuantitativeUncertainty.IsFullyEstimated == true`;
- blocked candidates explain each missing uncertainty component;
- uncertainty is not widened to force readiness;
- tests cover complete and incomplete uncertainty budgets.

### P21-M6 Generate Phase XXI Source Candidate Artifact

Generate a new source candidate table from the source-readiness campaign.

Definition of done:

- artifact lives under `studies/phase21_source_readiness_campaign_001`;
- terminal status is `candidate-source-ready` or `source-blocked`;
- records remain identity-neutral;
- records reference only internal generated or checked-in source artifacts;
- records include branch/refinement/environment selectors;
- records include full blockers if not ready.

Suggested output:

- `studies/phase21_source_readiness_campaign_001/source_candidates.json`
- `studies/phase21_source_readiness_campaign_001/STUDY.md`

### P21-M7 Bridge Ready Sources Without Physical Identity

If any Phase XXI sources are ready, bridge them into the P19 candidate-mode
source contract as identity-neutral inputs only.

Definition of done:

- ready sources can produce `CandidateModeSourceRecord` values;
- blocked sources still produce blocked extraction records;
- bridge output does not use `w-boson` or `z-boson` particle ids;
- P19 W/Z physical campaign remains blocked until a separate phase supplies
  validated W/Z identity evidence.

Suggested output:

- `studies/phase21_source_readiness_campaign_001/candidate_mode_sources.json`

### P21-M8 Re-run Physical And Regression Campaigns

Verify that source readiness work does not accidentally open physical claims.

Required commands:

```bash
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase21_wz_physical_check --validate-first
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase21_reference_regression_check --validate-first
dotnet test GeometricUnity.slnx
```

Definition of done:

- all focused and full tests pass;
- Phase19 W/Z campaign remains terminal `blocked` unless downstream W/Z identity
  evidence, mapping, calibration, uncertainty, and falsifier gates all pass;
- source-readiness artifact terminal status is explicit;
- report text contains no physical prediction language from source candidates.

## Recommended Execution Order

1. P21-M1: define the source-readiness campaign spec.
2. P21-M2: generate or normalize spectra across the campaign matrix.
3. P21-M3: implement branch/refinement-aware mode tracking.
4. P21-M4: implement source readiness promotion rules.
5. P21-M5: close or block source-level uncertainty.
6. P21-M6: generate the Phase XXI source candidate artifact.
7. P21-M7: bridge ready sources as identity-neutral inputs only.
8. P21-M8: run focused tests, physical campaign, reference campaign, and full
   solution tests.

## Guardrails

- Do not assign W or Z identity in Phase XXI.
- Do not use PDG or other external physical target values as source values.
- Do not use target agreement as a source readiness criterion.
- Do not mark a candidate ready with missing branch selectors.
- Do not mark a candidate ready with ambiguous or unstable mode-family tracking.
- Do not mark a candidate ready with incomplete uncertainty.
- Do not promote physical mapping, calibration, or claim language from source
  readiness alone.
- Do not mutate Phase12 artifacts; generate Phase XXI derivatives.

## Plain-English Success Criteria

Phase XXI succeeds if it either produces identity-neutral internal source
candidates that are genuinely ready for later W/Z identity testing, or it proves
which specific branch, refinement, tracking, or uncertainty computation is still
missing.

Phase XXI fails if it calls source candidates W/Z, copies target values into
source records, or opens the physical prediction gate from source-readiness
evidence alone.

## Implementation Status

Started 2026-04-25.

- P21-M1 complete: added
  `studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json`
  and `schemas/internal_vector_boson_source_readiness_campaign.schema.json`.
- P21-M2 complete as a selector-blocked normalization pass: added
  `studies/phase21_source_readiness_campaign_001/spectra_manifest.json`. The
  manifest documents that checked-in Phase12 spectra cannot satisfy readiness
  without branch and refinement selectors.
- P21-M3 complete for the readiness gate: mode-family ambiguity is carried into
  the validator and blocks promotion. No regenerated branch/refinement-aware
  spectra were available in this phase.
- P21-M4 complete: added explicit source promotion rules in
  `InternalVectorBosonSourceReadinessValidator`.
- P21-M5 complete as blocked uncertainty closure: incomplete source uncertainty
  budgets are detected and prevent readiness.
- P21-M6 complete: generated
  `studies/phase21_source_readiness_campaign_001/source_candidates.json`.
  Terminal status is `source-blocked`; candidate count is 12; ready count is 0.
- P21-M7 complete as a blocked bridge: added
  `studies/phase21_source_readiness_campaign_001/candidate_mode_sources.json`
  with no promoted `CandidateModeSourceRecord` values because no source is
  ready.
- P21-M8 complete: JSON validation, focused tests, physical campaign,
  reference regression campaign, and full solution tests pass. The Phase19 W/Z
  physical campaign remains terminal `blocked`.

Current narrowed blockers:

- branch selectors are missing;
- refinement coverage is missing;
- mode-family ambiguity exceeds the configured threshold;
- branch stability is below the configured threshold;
- source claim class is below `C2_BranchStableCandidate`;
- source uncertainty components and total uncertainty are incomplete.

Validation completed:

```bash
jq -e . studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json studies/phase21_source_readiness_campaign_001/spectra_manifest.json studies/phase21_source_readiness_campaign_001/source_candidates.json studies/phase21_source_readiness_campaign_001/candidate_mode_sources.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json --out-dir study-runs/phase21_wz_physical_check --validate-first
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir study-runs/phase21_reference_regression_check --validate-first
dotnet test GeometricUnity.slnx
```
