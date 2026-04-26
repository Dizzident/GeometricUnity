# Phase XXIV: W/Z Identity Rule Readiness

This study evaluates whether the ready P22 source families contain enough
internal, non-target identity features to derive W/Z mode identity rules.

## Inputs

- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`
- `studies/phase22_selector_source_spectra_001/mode_families.json`

## Outputs

- `identity_rule_readiness.json`

## Result

- terminal status: `identity-feature-blocked`;
- coverage records: 12;
- derived identity rules: 0.

All 12 P22 source families are selector-stable, but none contains the identity
features required to distinguish charged W-like modes from neutral Z-like modes.
The missing features are electroweak multiplet identifiers, charged/neutral
sector signatures, and current-coupling signatures.

No external physical target values were used.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- evaluate-wz-identity-rule-readiness \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --mode-families studies/phase22_selector_source_spectra_001/mode_families.json \
  --out studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json
```
