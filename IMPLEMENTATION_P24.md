# IMPLEMENTATION_P24.md

## Purpose

Phase XXIV closes the next P23 blocker by replacing the informal W/Z ordering
heuristic with an executable identity-rule readiness gate.

P23 produced provisional W/Z hypotheses from the lowest and highest ready P22
source values. That ordering is not a physical identity derivation. P24 tests
whether the current internal source-family records contain enough non-target
identity features to derive W and Z mode identities.

## Phase XXIV Goal

Evaluate the exact prerequisites for deriving W/Z identity rules without using
external physical target values.

The phase succeeds if it emits one of:

- `identity-rule-ready`, when the source families contain selector-stable
  charged and neutral electroweak identity features;
- `identity-feature-blocked`, with concrete missing feature requirements.

## Implementation Status

Started 2026-04-26.

- P24-M1 complete: added `VectorBosonIdentityRuleReadinessEvaluator`.
- P24-M2 complete: added CLI command
  `evaluate-wz-identity-rule-readiness`.
- P24-M3 complete: generated
  `studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json`.
- P24-M4 complete: focused quantitative validation tests pass.

Current terminal status:

- `identity-feature-blocked`

Current result:

- coverage records: 12;
- selector-stable records: 12;
- identity-rule eligible records: 0;
- derived W/Z rules: 0.

Current blockers:

- compute electroweak multiplet identifiers for candidate mode families;
- compute charged/neutral sector signatures for candidate mode families;
- compute current-coupling signatures independent of physical target values;
- derive at least one charged-sector vector mode identity candidate for W;
- derive at least one neutral-sector vector mode identity candidate for Z.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- evaluate-wz-identity-rule-readiness \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --mode-families studies/phase22_selector_source_spectra_001/mode_families.json \
  --out studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json
```

Validation completed:

```bash
jq -e . studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet test GeometricUnity.slnx
```

Results:

- identity-rule readiness JSON: valid;
- quantitative validation tests: passed, 90/90.
- Phase V reporting tests: passed, 166/166;
- full solution tests: passed.

## Guardrails

- Do not derive W/Z identity from mass ordering alone.
- Do not use PDG or external physical target values as identity features.
- Do not promote P23 mode-identification evidence while P24 is
  `identity-feature-blocked`.
- Do not open the physical prediction gate until identity evidence, mapping,
  classification, calibration, and falsifier gates all pass.

## Next Work

The next phase should compute internal electroweak identity features for the
P22 source families. Minimum required features are an electroweak multiplet
identifier, a charged/neutral sector signature, and a current-coupling
signature. Once those features exist, rerun the P24 gate and only then promote
mode-identification evidence.
