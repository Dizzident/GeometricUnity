# IMPLEMENTATION_P26.md

## Purpose

Phase XXVI closes the next P25 blocker by making electroweak mixing convention
readiness executable.

P25 extracted SU(2)-adjoint multiplet identifiers and finite-difference
current-coupling signatures for all 12 P22 source families. It could not assign
charged or neutral sectors because the repository did not contain an internal
electromagnetic/U(1)-mixing convention. P26 adds the gate that validates such a
convention before any charged/neutral sector assignment can be used for W/Z
identity evidence.

## Phase XXVI Goal

Evaluate whether a checked-in internal electroweak mixing convention exists and
is sufficient to map SU(2)-adjoint basis sectors to charged and neutral sectors
without external physical target values.

The phase succeeds if it emits one of:

- `mixing-convention-ready`, when a validated convention artifact declares a
  U(1) generator, charge-operator derivation, neutral basis axis, and charged
  basis axes;
- `mixing-convention-blocked`, with concrete requirements for the missing
  convention.

## Implementation Status

Started 2026-04-26.

- P26-M1 complete: added `ElectroweakMixingConventionReadinessEvaluator`.
- P26-M2 complete: added CLI command
  `evaluate-electroweak-mixing-convention`.
- P26-M3 complete: generated
  `studies/phase26_electroweak_mixing_convention_001/mixing_convention_readiness.json`.
- P26-M4 complete: focused quantitative validation tests pass.

Current terminal status:

- `mixing-convention-blocked`

Current result:

- source feature count: 12;
- charge-sector assignments: 0;
- convention artifact: absent.

Current blockers:

- provide a validated internal electroweak mixing convention artifact;
- derive a U(1) generator and charge operator independent of external target
  values;
- declare which SU(2)-adjoint basis axis is neutral and which axes are charged.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- evaluate-electroweak-mixing-convention \
  --identity-features studies/phase25_internal_electroweak_features_001/identity_features.json \
  --out studies/phase26_electroweak_mixing_convention_001/mixing_convention_readiness.json
```

Validation completed:

```bash
jq -e . studies/phase26_electroweak_mixing_convention_001/mixing_convention_readiness.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json \
  --out-dir study-runs/phase26_wz_physical_check --validate-first
dotnet test GeometricUnity.slnx
```

Results:

- Phase26 JSON artifact: valid;
- quantitative validation tests: passed, 95/95.
- Phase V reporting tests: passed, 166/166;
- Phase19 physical comparison rerun completed successfully but remains terminal
  `blocked`;
- full solution tests: passed.

## Guardrails

- Do not assign charged or neutral sectors from arbitrary SU(2) basis-axis
  labels.
- Do not use physical W/Z or photon target values to choose a neutral axis.
- Do not promote W/Z identity evidence until this gate is
  `mixing-convention-ready` and the P24 identity-rule readiness gate is ready.
- Do not open the physical prediction gate from a missing or provisional
  convention.

## Next Work

The next phase should derive or construct a checked-in electroweak mixing
convention artifact. Minimum required fields are a validated U(1) generator, a
charge-operator derivation id, a neutral SU(2)-adjoint basis axis, charged
SU(2)-adjoint basis axes, and an explicit declaration that no external target
values were used.
