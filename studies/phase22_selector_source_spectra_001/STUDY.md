# Phase XXII Selector-Aware Source Spectra

## Purpose

This study generates selector-aware internal source spectra from the Phase XXI
identity-neutral source candidates. It attaches branch, refinement, and
environment selectors to derivative source records, tracks source families
across that matrix, estimates source uncertainty, and reruns the Phase XXI
readiness gate.

The output is not a W or Z prediction. It is an upstream source-readiness layer
for later identity testing.

## Inputs

- `studies/phase21_source_readiness_campaign_001/source_candidates.json`
- `studies/phase21_source_readiness_campaign_001/config/source_readiness_campaign.json`
- `studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json`

External physical target tables are not inputs to this study.

## Outputs

- `spectra_manifest.json`: selector-aware source spectrum matrix manifest.
- `spectra/`: selector-aware spectrum records.
- `modes/`: selector-aware mode records.
- `mode_families.json`: matrix-aware source family tracking records.
- `source_candidates.json`: Phase XXII source candidate readiness table.
- `candidate_mode_sources.json`: identity-neutral candidate-mode source bridge.

Current terminal status:

- `candidate-source-ready`

Ready source count:

- 12

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- run-internal-vector-boson-source-spectrum-campaign \
  --spec studies/phase22_selector_source_spectra_001/config/source_spectrum_campaign.json \
  --out-dir studies/phase22_selector_source_spectra_001
```

## Interpretation

Phase XXII closes the Phase XXI source-readiness blockers by generating a
selector-aware derivative matrix from internal source candidates. This makes the
source layer ready for later identity testing, but it does not provide W/Z mode
identity, physical observable mapping, physical calibration, or target evidence.

## Guardrails

- Source candidates remain particle-identity-neutral.
- No record assigns W or Z identity.
- No PDG or external physical target value is used as source data.
- Physical prediction gates remain blocked until a separate phase supplies
  validated identity, mapping, calibration, uncertainty, and falsifier evidence.
