# PDG-TARGETS-2025

## Source

- Title: PDG W, Z, and Higgs boson listings
- W boson listing: https://pdg.lbl.gov/2025/listings/rpp2025-list-w-boson.pdf
- Z boson listing: https://pdg.lbl.gov/2025/listings/rpp2025-list-z-boson.pdf
- Higgs boson listing: https://pdg.lbl.gov/2025/listings/rpp2025-list-higgs-boson.pdf
- Source type: External target reference
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

These PDG listings define the measured target values used when checking whether
the pipeline's promoted physical predictions match current accepted W, Z, and
Higgs masses.

## How It Was Used

- Anchored comparison targets in validation phases.
- Helped distinguish target matching from target-independent derivation.
- Supported claim-integrity checks that prevent a target value from being reused
  as its own source.

## Prediction Relevance

These listings are target references only. They are necessary for evaluating
accuracy but irrelevant to filling geometric source-lineage contracts.

## Limitation

Target data cannot be used as prediction input. Any pipeline that imports these
values as source evidence fails target independence.

## Follow-Up

- Update target references when the project intentionally moves to a newer PDG
  release.
- Keep these references outside the source-law promotion path.
