# Phase XX Internal Vector-Boson Source Candidates

## Purpose

This study generates identity-neutral internal vector-boson source candidates
from checked-in Phase12 bosonic artifacts.

The output is not a W or Z prediction. It is an upstream source-candidate layer
that later phases may use only after separate particle-identity evidence,
uncertainty closure, mapping, calibration, and falsifier gates pass.

## Inputs

- `studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json`
- `studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json`
- `studies/phase12_joined_calculation_001/output/background_family/spectra/`

External physical target tables are not inputs to this study.

## Output

- `source_candidates.json`: generated Phase20 source-candidate table.

Current terminal status:

- `source-blocked`

The checked-in Phase12 candidates are internal computed artifacts, but they are
not ready source candidates because they lack branch selectors, refinement
coverage, unambiguous stable family tracking, and complete source uncertainty.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- generate-internal-vector-boson-sources \
  --registry studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json \
  --families studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json \
  --spectra-root studies/phase12_joined_calculation_001/output/background_family/spectra \
  --out studies/phase20_internal_vector_boson_sources_001/source_candidates.json
```

## Guardrails

- Source candidates remain particle-identity-neutral.
- No record assigns W or Z identity.
- No external target value is used as a source value.
- Blocked source candidates cannot be promoted through the P19 candidate-mode
  extractor because their uncertainty remains unestimated.
