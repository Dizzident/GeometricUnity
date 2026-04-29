# Phase LXIII - Canonical SU(2) Generator Normalization

## Goal

Close the first missing Phase LXII source by deriving a replayable canonical SU(2) generator normalization convention for weak-coupling amplitudes.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/Su2GeneratorNormalizationDeriver.cs`
- `tests/Gu.Phase5.Reporting.Tests/Su2GeneratorNormalizationDeriverTests.cs`
- `studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json`
- `studies/phase63_su2_generator_normalization_001/STUDY.md`

The derivation validates a trace-paired canonical SU(2) algebra with epsilon structure constants, then declares:

- convention id `physical-weak-coupling-normalization:su2-canonical-trace-half-v1`;
- commutator convention `[T_a, T_b] = epsilon_abc T_c`;
- physical trace normalization `tr(t_a t_b) = 1/2 delta_ab`;
- internal-to-physical generator scale `sqrt(1/2)` for the current unit trace-paired internal basis.

## Finding

Canonical generator normalization is now a concrete artifact. This does not yet produce a weak-coupling value; it supplies the normalization convention required before a non-proxy matrix element can be converted into a dimensionless weak-coupling candidate.

## Next Step

Phase LXIV should implement the non-proxy fermion-current matrix element source, using this normalization convention and explicitly avoiding finite-difference coupling proxy magnitudes.

## Validation

Completed:

- `jq -e . studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 247, Failed: 0, Skipped: 0
- `git diff --check`
