# IMPLEMENTATION_P25.md

## Purpose

Phase XXV computes the internal identity features requested by P24 from
existing Phase12/Phase22 artifacts.

P24 proved that W/Z identity rules cannot be derived from P22 values alone.
The missing inputs were electroweak multiplet identifiers, charged/neutral
sector signatures, and current-coupling signatures. P25 extracts the features
that are actually present in the current internal artifacts and reruns the P24
readiness gate against the enriched family table.

## Phase XXV Goal

Produce a checked-in, reproducible identity-feature table for the P22 source
families without using external physical target values.

The phase succeeds if it emits:

- SU(2)-adjoint multiplet identifiers from mode signatures;
- algebra-basis sector descriptors from mode-signature energy fractions;
- finite-difference current-coupling signatures from coupling atlases;
- explicit blockers for any identity feature that cannot be honestly derived.

## Implementation Status

Started 2026-04-26.

- P25-M1 complete: added `VectorBosonIdentityFeatureExtractor`.
- P25-M2 complete: added CLI command `compute-wz-identity-features`.
- P25-M3 complete: generated
  `studies/phase25_internal_electroweak_features_001/identity_features.json`.
- P25-M4 complete: generated enriched
  `studies/phase25_internal_electroweak_features_001/mode_families_with_identity_features.json`.
- P25-M5 complete: reran P24 readiness against enriched families.
- P25-M6 complete: focused quantitative validation tests pass.

Current feature status:

- terminal status: `identity-features-partial`;
- feature records: 12;
- records with SU(2)-adjoint multiplet id: 12;
- records with finite-difference current-coupling signature: 12;
- records with charged/neutral sector signature: 0.

Readiness after P25 features:

- terminal status: `identity-feature-blocked`;
- derived W/Z rules: 0;
- remaining closure requirements:
  - compute charged/neutral sector signatures for candidate mode families;
  - derive at least one charged-sector vector mode identity candidate for W;
  - derive at least one neutral-sector vector mode identity candidate for Z.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- compute-wz-identity-features \
  --mode-families studies/phase22_selector_source_spectra_001/mode_families.json \
  --phase12-mode-families studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json \
  --mode-signature-root studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures \
  --coupling-atlases studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_bg-phase12-bg-a-20260315212202.json,studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_bg-phase12-bg-b-20260315212202.json \
  --out-dir studies/phase25_internal_electroweak_features_001

dotnet run --project apps/Gu.Cli -- evaluate-wz-identity-rule-readiness \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --mode-families studies/phase25_internal_electroweak_features_001/mode_families_with_identity_features.json \
  --out studies/phase25_internal_electroweak_features_001/identity_rule_readiness_after_features.json
```

Validation completed:

```bash
jq -e . \
  studies/phase25_internal_electroweak_features_001/identity_features.json \
  studies/phase25_internal_electroweak_features_001/mode_families_with_identity_features.json \
  studies/phase25_internal_electroweak_features_001/identity_rule_readiness_after_features.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json \
  --out-dir study-runs/phase25_wz_physical_check --validate-first
dotnet test GeometricUnity.slnx
```

Results:

- Phase25 JSON artifacts: valid;
- quantitative validation tests: passed, 92/92.
- Phase V reporting tests: passed, 166/166;
- Phase19 physical comparison rerun completed successfully but remains terminal
  `blocked`;
- full solution tests: passed.

## Guardrails

- Do not assign charged or neutral sectors from arbitrary SU(2) basis-axis
  labels.
- Do not treat current-coupling profile hashes as physical couplings.
- Do not use PDG or other external physical target values as identity features.
- Do not promote W/Z mode-identification evidence while the readiness gate is
  `identity-feature-blocked`.

## Next Work

The next phase should define or compute an internal electromagnetic/U(1)-mixing
convention that can map SU(2)-adjoint basis sectors to charged and neutral
sectors. Until that convention exists, W/Z identity evidence cannot be
validated and physical predictions must remain blocked.
