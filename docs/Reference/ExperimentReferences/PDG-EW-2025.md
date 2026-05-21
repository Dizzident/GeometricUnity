# PDG-EW-2025

## Source

- Title: Review of the Standard Model
- URL: https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf
- Source type: External reference
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

The PDG Standard Model review is the baseline external reference for electroweak
definitions and dependency structure. It is used to keep the implementation's
formulas and terminology aligned with accepted physics references.

## How It Was Used

- Checked the Standard Model relationships among W mass, Z mass, electroweak
  couplings, VEV, and weak mixing.
- Helped separate comparison-target data and Standard Model identities from
  source-lineage evidence.
- Supported audits that require a prediction to derive inputs rather than import
  them.

## Prediction Relevance

The PDG review supplies formulas and context for validation, not a GU geometric
source law. It can define what must be reproduced and how quantities are named,
but it cannot fill GU source contracts.

## Limitation

Using PDG masses, couplings, or VEVs as inputs produces comparison or replay
checks, not a target-independent prediction.

## Follow-Up

- Continue using PDG as the comparison standard for physical targets and formula
  sanity checks.
- Keep source-lineage gates strict so PDG values are not mistaken for derived GU
  outputs.
