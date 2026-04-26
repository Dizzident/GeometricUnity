# Phase XXVII: Charge Sector Convention Application

This study declares a canonical internal SU(2) Cartan convention, validates it
with the P26 readiness gate, applies its charge-sector assignments to P25
identity features, and reruns the P24 W/Z identity-rule readiness gate.

## Inputs

- `studies/phase25_internal_electroweak_features_001/identity_features.json`
- `studies/phase25_internal_electroweak_features_001/mode_families_with_identity_features.json`
- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`
- `electroweak_mixing_convention.json`

## Outputs

- `mixing_convention_readiness.json`
- `charge_sector_application.json`
- `identity_features_with_charge_sectors.json`
- `mode_families_with_charge_sectors.json`
- `identity_rule_readiness_after_charge_sectors.json`

## Result

- convention readiness terminal status: `mixing-convention-ready`;
- charge-sector application terminal status: `charge-sectors-applied`;
- identity-rule readiness terminal status: `identity-rule-ready`;
- charge-sector assignments: 12;
- charged assignments: 9;
- neutral assignments: 3;
- unassigned assignments: 0;
- derived identity rules: 2.

The derived internal identity rules map:

- `w-boson` to `phase22-phase12-candidate-0`;
- `z-boson` to `phase22-phase12-candidate-2`.

No external physical target values were used. This study establishes internal
identity-rule readiness only; physical boson prediction comparison remains a
separate downstream campaign step.
