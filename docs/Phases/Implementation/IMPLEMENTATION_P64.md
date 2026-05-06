# Phase LXIV - Non-Proxy Fermion-Current Matrix Element Source

## Goal

Close the Phase LXII non-proxy fermion-current matrix-element source without promoting finite-difference coupling proxy magnitudes.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/NonProxyFermionCurrentMatrixElementDeriver.cs`
- `tests/Gu.Phase5.Reporting.Tests/NonProxyFermionCurrentMatrixElementDeriverTests.cs`
- `studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json`
- `studies/phase64_non_proxy_fermion_current_matrix_element_001/STUDY.md`

The derivation records the replayable operator matrix element:

`<phi_i, delta_D[b_k] phi_j>`

with `delta_D[b_k]` sourced from `Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical`, under the Phase LXIII SU(2) generator normalization convention.

## Finding

The non-proxy matrix-element source is now explicit and rejects finite-difference coupling proxy usage.

This phase still does not produce a dimensionless weak-coupling value. It provides the replayable matrix element needed by the next amplitude-extraction phase.

## Next Step

Phase LXV should implement the dimensionless coupling amplitude extractor. It should consume:

- Phase LXIII generator normalization;
- Phase LXIV non-proxy matrix-element source;
- selected W/Z source modes;
- target exclusions.

The output should be a candidate `NormalizedWeakCouplingCandidateRecord` that can be submitted to the Phase LXI audit. It will still need uncertainty propagation and branch stability before being accepted.

## Validation

Completed:

- `jq -e . studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 249, Failed: 0, Skipped: 0
- `git diff --check`
