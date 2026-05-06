# Phase LXXVIII - Replayed Raw Weak-Coupling Matrix-Element Builder

## Goal

Build the code path that converts a replayed Phase IV analytic coupling record into Phase LXXVII raw matrix-element evidence.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/ReplayedRawWeakCouplingMatrixElementEvidenceBuilder.cs`
- `tests/Gu.Phase5.Reporting.Tests/ReplayedRawWeakCouplingMatrixElementEvidenceBuilderTests.cs`
- `studies/phase78_replayed_raw_weak_coupling_matrix_element_builder_001/replayed_raw_matrix_element_builder.json`
- `studies/phase78_replayed_raw_weak_coupling_matrix_element_builder_001/STUDY.md`

Updated:

- `src/Gu.Phase5.Reporting/Gu.Phase5.Reporting.csproj`

The builder consumes `BosonFermionCouplingRecord`, computes the raw matrix-element magnitude from the real and imaginary matrix-element components, and forwards the result through the Phase LXXVII evidence validator.

It accepts only:

- variation method `analytic-dirac-variation-matrix-element:v1`;
- normalization convention `unit-modes`;
- finite, positive, internally consistent real/imaginary/magnitude components;
- a variation evidence id;
- provenance with branch and revision.

## Finding

The production study search found persisted unit-mode coupling atlases, but they are finite-difference records. They remain blocked for physical weak-coupling promotion.

This phase adds the replay builder, but it does not yet produce a new physical prediction because no persisted production analytic matrix-element replay artifact is available in `studies/`.

## Next Step

Phase LXXIX should produce a real analytic replay artifact by running or adding a small replay harness around:

- `Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical`;
- `Gu.Phase4.Couplings.CouplingProxyEngine.ComputeCoupling`;
- the selected W/Z boson mode and target-independent fermion current modes.

That artifact should then pass through `ReplayedRawWeakCouplingMatrixElementEvidenceBuilder` and `DimensionlessWeakCouplingAmplitudeExtractor.ExtractFromEvidence`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 276 passed
- `git diff --check`
