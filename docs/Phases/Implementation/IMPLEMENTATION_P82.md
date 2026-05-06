# Phase 82 - Boson Perturbation Vector Materialization

## Goal

Close the immediate production ingestion blocker for physical W/Z replay inputs: selected Phase12 boson mode artifacts persist their connection-space perturbation as `modeVector`, while the existing Phase4 analytic-variation loader only accepted `eigenvectorCoefficients`.

## Completed

- Added `BosonPerturbationVectorMaterializer` in `Gu.Phase5.Reporting`.
- The materializer accepts the persisted Phase12 `modeVector` field and keeps legacy `eigenvectorCoefficients` fallback support.
- The materializer blocks missing, empty, wrong-length, and non-finite perturbation vectors before they can enter a replay package.
- Patched the Phase4 fermion-family atlas analytic boson loader to read `modeVector` first and then fall back to `eigenvectorCoefficients`.
- Added regression tests covering the Phase12 field shape, legacy fallback, length mismatch, non-finite values, and missing vector fields.
- Recorded the current source artifact evidence in `studies/phase82_boson_perturbation_vector_materializer_001/boson_perturbation_vector_materializer.json`.

## Evidence

- Phase12 source artifact:
  `studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-0.json`
- Source field: `modeVector`
- Vector length: `156`
- Normalization convention: `unit-M-norm`
- Phase4 connection-space expectation: `edgeCount * dimG`; with `dimG = 3`, the discovered vector length corresponds to 52 mesh edges.

## Remaining Blocker

This phase unblocks reading the selected physical boson perturbation vector. It does not yet claim accurate physical W/Z or broad boson predictions. The next phase should wire this materialized vector into the full analytic replay package builder with source-backed fermion eigenvectors and variation matrices, then run the replay against selected W/Z modes without top-coupling summary shortcuts.
