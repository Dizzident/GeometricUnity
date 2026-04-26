# IMPLEMENTATION_P27.md

## Purpose

Phase XXVII closes the P26 convention blocker by checking in an internal
SU(2)-Cartan/U(1) branch convention and applying its charged/neutral sector
assignments back into the P25 identity-feature artifacts.

This phase does not use W, Z, photon, PDG, or other external target values. It
only makes the internal convention explicit so P24 can evaluate whether the
existing source families are identity-rule ready.

## Phase XXVII Goal

Produce a reproducible charge-sector convention application that:

- validates a canonical internal SU(2) Cartan convention artifact;
- assigns charged/neutral sectors to all P25 feature records;
- writes updated identity-feature and mode-family tables;
- reruns the P24 W/Z identity-rule readiness gate.

## Implementation Status

Started 2026-04-26.

- P27-M1 complete: added
  `ElectroweakChargeSectorAssignmentApplier`.
- P27-M2 complete: added CLI command `apply-electroweak-charge-sectors`.
- P27-M3 complete: checked in
  `studies/phase27_charge_sector_convention_001/electroweak_mixing_convention.json`.
- P27-M4 complete: reran P26 convention readiness with the Phase27 convention.
- P27-M5 complete: applied charge-sector assignments to P25 identity features
  and mode families.
- P27-M6 complete: reran P24 identity-rule readiness against the
  charge-sector-enriched families.

Current terminal statuses:

- P26 convention readiness: `mixing-convention-ready`;
- P27 charge-sector application: `charge-sectors-applied`;
- P24 identity-rule readiness after charge sectors: `identity-rule-ready`.

Current result:

- charge-sector assignments: 12;
- charged assignments: 9;
- neutral assignments: 3;
- unassigned assignments: 0;
- derived identity rules: 2.

Derived internal identity rules:

- `w-boson`: `phase22-phase12-candidate-0`;
- `z-boson`: `phase22-phase12-candidate-2`.

These are internal identity-rule mappings, not yet physical boson mass
predictions.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- evaluate-electroweak-mixing-convention \
  --identity-features studies/phase25_internal_electroweak_features_001/identity_features.json \
  --mixing-convention studies/phase27_charge_sector_convention_001/electroweak_mixing_convention.json \
  --out studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json

dotnet run --project apps/Gu.Cli -- apply-electroweak-charge-sectors \
  --identity-features studies/phase25_internal_electroweak_features_001/identity_features.json \
  --mode-families studies/phase25_internal_electroweak_features_001/mode_families_with_identity_features.json \
  --mixing-readiness studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json \
  --out-dir studies/phase27_charge_sector_convention_001

dotnet run --project apps/Gu.Cli -- evaluate-wz-identity-rule-readiness \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --mode-families studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json \
  --out studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json
```

Validation completed:

```bash
jq -e . studies/phase27_charge_sector_convention_001/*.json
dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase19_dimensionless_wz_candidate_001/config/campaign.json \
  --out-dir study-runs/phase27_wz_physical_check --validate-first
dotnet test GeometricUnity.slnx
```

Results:

- Phase27 JSON artifacts: valid;
- quantitative validation tests: passed, 97/97;
- Phase V reporting tests: passed, 166/166;
- Phase19 physical comparison rerun completed successfully but remains wired to
  the prior blocked/provisional physical mapping campaign;
- full solution tests: passed.

## Guardrails

- Treat the Phase27 convention as an internal branch convention, not empirical
  evidence.
- Do not claim physical W/Z predictions from identity-rule readiness alone.
- Do not use external target values to select or tune the neutral axis.
- Keep the physical prediction gate closed until the campaign consumes the
  identity rules and emits dimensionless observables that can be compared to
  real targets.

## Next Work

Wire the now-ready internal W/Z identity rules into the physical prediction
campaign so it can select the charged and neutral source observables directly
instead of relying on provisional source-only hypotheses.
