# Phase LXXVII - Raw Weak-Coupling Matrix-Element Evidence Gate

## Goal

Prevent the Phase LXV scalar `rawMatrixElementMagnitude` input from being mistaken for a physically usable prediction input.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/RawWeakCouplingMatrixElementEvidenceValidator.cs`
- `tests/Gu.Phase5.Reporting.Tests/RawWeakCouplingMatrixElementEvidenceValidatorTests.cs`
- `studies/phase77_raw_weak_coupling_matrix_element_evidence_gate_001/raw_matrix_element_evidence_gate.json`
- `studies/phase77_raw_weak_coupling_matrix_element_evidence_gate_001/STUDY.md`

Extended `DimensionlessWeakCouplingAmplitudeExtractor` with `ExtractFromEvidence`, which only emits a weak-coupling candidate when the raw matrix element has passed a replayed analytic matrix-element evidence gate.

The accepted raw evidence contract requires:

- source kind `replayed-analytic-dirac-variation-matrix-element`;
- variation method `analytic-dirac-variation-matrix-element:v1`;
- normalization convention `unit-modes`;
- finite positive raw matrix-element magnitude;
- no finite-difference proxy usage;
- no coupling-proxy magnitude usage;
- replayed coupling record id;
- variation evidence id;
- provenance id.

## Finding

The existing Phase LXV scalar input `0.8` is blocked by this gate because it is a scalar study input, not replayed evidence. This is intentional: it prevents target-fitting or unsupported constants from entering the W/Z absolute mass path.

## Next Step

Phase LXXVIII should build a concrete replay artifact from the existing Phase IV analytic Dirac variation and coupling machinery, then feed that replayed evidence into `ExtractFromEvidence`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 273 passed
- `git diff --check`
