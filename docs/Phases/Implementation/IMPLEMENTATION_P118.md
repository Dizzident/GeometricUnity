# Implementation P118

## Goal

Resolve Phase117's upstream blocker into a concrete normalization/operator-scale
diagnostic.

## Result

Added `studies/phase118_wz_matrix_element_normalization_diagnostic_001`.

Terminal status:

`wz-matrix-element-normalization-diagnostic-upstream-scale-blocked`

Key findings:

- repaired W raw required scale: `3551.541445181695`
- repaired Z raw required scale: `4033.9787545764066`
- repaired raw required scale mean: `3792.760099879051`
- repaired raw required scale relative spread: `0.12719953192138259`
- canonical generator scale is `0.7071067811865476`, while the repaired replay
  would require generator scales of `2511.319039553048` and
  `2852.453732523441`

Ruled out:

- persisted W/Z boson source vectors are already unit-norm
- selected repaired fermion eigenvectors are already unit-norm
- canonical SU(2) trace-half generator normalization cannot explain the missing
  factor
- restoring the invalid ungauged replay amplitude still misses the target by
  more than an order of magnitude

Remaining plausible blocker:

- a target-independent connection-mode-to-Dirac-variation operator/source scale
  or dimensional lift convention is missing

Phase101 now points the next phase at Phase118 with
`operator-source-scale-blocked`.

## Validation

- `dotnet build studies/phase118_wz_matrix_element_normalization_diagnostic_001/Phase118WzMatrixElementNormalizationDiagnostic.csproj --verbosity minimal`
- `dotnet run --project studies/phase118_wz_matrix_element_normalization_diagnostic_001/Phase118WzMatrixElementNormalizationDiagnostic.csproj --no-build`
- `dotnet build studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-restore --verbosity minimal`
- `dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-build`

## Next Phase

Derive or audit the target-independent connection-mode-to-Dirac-variation
operator/source scale, then replay Phase115 and Phase116 only if that scale is
materialized as evidence.
