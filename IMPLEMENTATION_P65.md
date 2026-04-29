# Phase LXV - Dimensionless Weak-Coupling Amplitude Extractor

## Goal

Produce the first normalized weak-coupling candidate shape from the Phase LXIII generator normalization and Phase LXIV non-proxy matrix-element source.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/DimensionlessWeakCouplingAmplitudeExtractor.cs`
- `tests/Gu.Phase5.Reporting.Tests/DimensionlessWeakCouplingAmplitudeExtractorTests.cs`
- `studies/phase65_dimensionless_weak_coupling_amplitude_001/dimensionless_weak_coupling_amplitude.json`
- `studies/phase65_dimensionless_weak_coupling_amplitude_001/phase61_candidate_audit.json`
- `studies/phase65_dimensionless_weak_coupling_amplitude_001/STUDY.md`

The extractor emits a `NormalizedWeakCouplingCandidateRecord` with:

- source kind `normalized-internal-weak-coupling`;
- normalization convention `physical-weak-coupling-normalization:su2-canonical-trace-half-v1`;
- non-proxy variation method `analytic-dirac-variation-matrix-element:v1`;
- target exclusions for the physical W/Z masses.

## Finding

The first dimensionless weak-coupling candidate shape now exists. It is intentionally not accepted by Phase LXI yet because uncertainty propagation and branch-stability evidence are still missing.

## Next Step

Phase LXVI should implement weak-coupling uncertainty propagation for the extracted amplitude. After that, Phase LXVII should add branch-stability evidence across accepted variants.

## Validation

Completed:

- `jq -e . studies/phase65_dimensionless_weak_coupling_amplitude_001/dimensionless_weak_coupling_amplitude.json`
- `jq -e . studies/phase65_dimensionless_weak_coupling_amplitude_001/phase61_candidate_audit.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 252, Failed: 0, Skipped: 0
- `git diff --check`
