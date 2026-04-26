# Phase XXV: Internal Electroweak Identity Features

This study extracts identity features from the current internal Phase12 and P22
artifacts and reruns the W/Z identity-rule readiness gate.

## Inputs

- `studies/phase22_selector_source_spectra_001/mode_families.json`
- `studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json`
- `studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures/`
- `studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_bg-phase12-bg-a-20260315212202.json`
- `studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_bg-phase12-bg-b-20260315212202.json`

## Outputs

- `identity_features.json`
- `mode_families_with_identity_features.json`
- `identity_rule_readiness_after_features.json`

## Result

- feature extraction terminal status: `identity-features-partial`;
- readiness terminal status after enriched features: `identity-feature-blocked`;
- 12/12 records have SU(2)-adjoint multiplet identifiers;
- 12/12 records have finite-difference current-coupling signatures;
- 0/12 records have charged/neutral sector signatures.

P25 narrows the blocker. The current remaining blocker is no longer generic
identity-feature absence; it is specifically the lack of an internal
electromagnetic or U(1)-mixing convention that can distinguish charged from
neutral sectors.

No external physical target values were used.
