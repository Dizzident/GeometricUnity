# Phase XXIII W/Z Identity Hypotheses

## Purpose

This study screens the P22 ready identity-neutral source candidates for a
provisional W/Z hypothesis pair and computes a source-only dimensionless ratio.

The output is not a physical W/Z prediction. It is an identity-hypothesis layer
that documents the remaining evidence needed before any comparison to real
values is allowed.

## Inputs

- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`

External physical target tables are not inputs to this study.

## Outputs

- `candidate_modes.json`
- `mode_identification_evidence.json`
- `candidate_observables.json`
- `physical_observable_mappings.json`
- `identity_hypothesis_result.json`

Current terminal status:

- `identity-blocked`

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- generate-wz-identity-hypotheses \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --out-dir studies/phase23_wz_identity_hypotheses_001
```

## Interpretation

The generated W/Z candidate modes are provisional. The lower/higher source
ordering is a screening heuristic, not a derivation of physical identity. The
candidate ratio is therefore a source-only internal value and must not be used
as target agreement evidence.

## Guardrails

- No external physical target value is used as source data.
- No mode-identification evidence is marked validated.
- No physical observable mapping is marked validated.
- Real-value comparison remains blocked until independent identity evidence is
  derived and tested.
