# Phase LXXVIII Replayed Raw Weak-Coupling Matrix-Element Builder

## Purpose

Create the bridge from Phase IV analytic coupling records into the Phase LXXVII raw weak-coupling evidence gate.

## Result

The builder now exists and is tested. It computes raw matrix-element magnitude from replayed analytic coupling real/imaginary components, validates provenance, and rejects finite-difference variation records.

Existing persisted coupling atlases found under `studies/` are finite-difference based, so they cannot be promoted as physical weak-coupling evidence.

## Closure

The next phase must produce an actual analytic replay artifact from `DiracVariationComputer.ComputeAnalytical` and selected fermion modes. Until that exists, the boson absolute mass path remains blocked from making usable physical predictions.
