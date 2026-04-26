# IMPLEMENTATION_P23.md

## Purpose

Phase XXIII starts the transition from P22 identity-neutral source readiness to
physical W/Z comparison readiness.

P22 produced 12 ready internal vector-boson source candidates. Phase XXIII uses
those ready sources to generate W/Z identity hypotheses and a dimensionless
candidate ratio, while keeping physical identity, mapping, and comparison gates
blocked until independent evidence exists.

## Baseline

Current P22 source layer:

- `studies/phase22_selector_source_spectra_001/source_candidates.json`;
- terminal status: `candidate-source-ready`;
- ready source count: 12;
- all sources remain identity-neutral.

Current physical campaign status:

- Phase19 W/Z physical campaign remains terminal `blocked`;
- P19 candidate modes are placeholders;
- P19 mode-identification evidence is provisional;
- P19 physical observable mapping is provisional/blocked.

## Phase XXIII Goal

Generate reproducible W/Z identity hypotheses from P22 ready sources without
using external physical target values as source data or selection criteria.

The phase succeeds if it emits:

- provisional W/Z candidate mode hypotheses sourced from P22;
- provisional mode-identification evidence with explicit closure requirements;
- a dimensionless source-only candidate W/Z ratio with propagated uncertainty;
- blocked physical observable mapping that explains exactly what evidence is
  still missing.

The phase must not mark physical W/Z predictions as validated.

## Implementation Status

Started 2026-04-26.

- P23-M1 complete: added `VectorBosonIdentityHypothesisGenerator`.
- P23-M2 complete: added CLI command
  `generate-wz-identity-hypotheses`.
- P23-M3 complete: generated
  `studies/phase23_wz_identity_hypotheses_001/candidate_modes.json`.
- P23-M4 complete: generated
  `studies/phase23_wz_identity_hypotheses_001/mode_identification_evidence.json`.
- P23-M5 complete: generated
  `studies/phase23_wz_identity_hypotheses_001/candidate_observables.json`.
- P23-M6 complete: generated blocked
  `studies/phase23_wz_identity_hypotheses_001/physical_observable_mappings.json`.
- P23-M7 complete: focused quantitative validation tests pass.

Current terminal status:

- `identity-blocked`

Current candidate ratio:

- observable id: `candidate-w-z-vector-mode-ratio-from-p22-hypothesis`;
- value: `0.06155803279807537`;
- total uncertainty: `0.0007914705852460068`;
- status: source-only hypothesis, not a physical prediction.

Current blockers:

- derive W vector-mode identity rule independent of target values;
- derive Z vector-mode identity rule independent of target values;
- validate that the selected P22 source pair has electroweak W/Z identity;
- keep physical target values external until identity and mapping gates pass.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- generate-wz-identity-hypotheses \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --out-dir studies/phase23_wz_identity_hypotheses_001
```

Validation completed:

```bash
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet test GeometricUnity.slnx
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json \
  --out-dir study-runs/phase23_wz_physical_check --validate-first
```

Results:

- quantitative validation tests: passed, 88/88;
- Phase V reporting tests: passed, 166/166;
- full solution tests: passed;
- Phase19 physical comparison rerun completed successfully but remains terminal
  `blocked`, with physical boson prediction, physical mapping, observable
  classification, and calibration still blocked.

## Guardrails

- Do not use PDG or external physical target values to select W/Z hypotheses.
- Do not mark mode-identification evidence as validated from ordering alone.
- Do not promote physical observable mappings until mode identity is validated.
- Do not treat the P23 candidate ratio as agreement or disagreement with real
  W/Z values.

## Next Work

The next phase should replace the ordering heuristic with a derived identity
rule, then test that rule across branch, refinement, and environment selectors.
Only after that can physical mapping and real-value comparison be attempted.
