# Phase 114 - W/Z Route Replayed Matrix-Element Evidence

## Goal

Produce replayed analytic raw matrix-element evidence for the validated W/Z
absolute-mass route rather than the closed candidate-3 route.

## Completed

- Ran Phase84 over the Phase12 W and Z source modes:
  - W route: `bg-phase12-bg-a-20260315212202-mode-0`;
  - Z route: `bg-phase12-bg-a-20260315212202-mode-2`.
- Added `studies/phase114_wz_route_replayed_matrix_element_evidence_001`.
- Updated Phase113 to consume Phase114 when present.
- Updated Phase101 package handoff to point to Phase114.

## Result

Terminal status:

`wz-route-replayed-matrix-elements-built-fermion-quality-blocked`

Raw matrix-element evidence validates for both W/Z source modes:

- W raw matrix-element magnitude: `0.0319080167935132`;
- W normalized weak coupling: `0.022562375048907422`;
- Z raw matrix-element magnitude: `0.06038522986015656`;
- Z normalized weak coupling: `0.0426988055176251`.

The evidence is still not accepted for absolute repair because the replay used
old fermion modes that are not gauge-reduced and have large residuals plus zero
branch/refinement stability scores.

## Verification

- `dotnet build studies/phase114_wz_route_replayed_matrix_element_evidence_001/Phase114WzRouteReplayedMatrixElementEvidence.csproj --verbosity minimal`
- `dotnet run --project studies/phase114_wz_route_replayed_matrix_element_evidence_001/Phase114WzRouteReplayedMatrixElementEvidence.csproj --no-build`
- `dotnet run --project studies/phase113_wz_absolute_repair_attempt_gate_001/Phase113WzAbsoluteRepairAttemptGate.csproj --no-build`
- `dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-build`
