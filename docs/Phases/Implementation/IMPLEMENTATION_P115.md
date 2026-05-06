# Implementation P115

## Goal

Clear the Phase114 fermion-quality blocker for the W/Z absolute-mass route.

## Result

Added `studies/phase115_wz_route_fermion_quality_replay_001`, which aggregates
Phase84 W and Z replay outputs produced with the Phase95 target-blind matched
Phase94 exact projected fermion modes.

The repaired W/Z replay now reports:

- `rawEvidenceValidated = true`
- `replayReady = true`
- `fermionQualityPassed = true`
- `repairAccepted = true`
- terminal status `wz-route-fermion-quality-replay-ready-absolute-projection`

Selected fermion modes:

- index `0`: gauge-reduced, residual `2.2956191058930723E-14`,
  branch stability `0.9735835329372028`, refinement stability
  `0.8250944968993068`
- index `3`: gauge-reduced, residual `2.3377053323282423E-14`,
  branch stability `0.9735835329372028`, refinement stability
  `0.8250944968993068`

The downstream Phase113 gate now prefers Phase115 evidence when present and
reports `wz-absolute-repair-evidence-ready` with `projectionRerunAllowed = true`.
Phase101 now points the next phase at the Phase115 artifact with
`absolute-projection-rerun-ready`.

## Validation

- `dotnet build studies/phase115_wz_route_fermion_quality_replay_001/Phase115WzRouteFermionQualityReplay.csproj --verbosity minimal`
- `dotnet run --project studies/phase115_wz_route_fermion_quality_replay_001/Phase115WzRouteFermionQualityReplay.csproj --no-build`
- `dotnet build studies/phase113_wz_absolute_repair_attempt_gate_001/Phase113WzAbsoluteRepairAttemptGate.csproj --no-restore --verbosity minimal`
- `dotnet run --project studies/phase113_wz_absolute_repair_attempt_gate_001/Phase113WzAbsoluteRepairAttemptGate.csproj --no-build`
- `dotnet build studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-restore --verbosity minimal`
- `dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-build`

## Next Phase

Rerun the absolute W/Z projection using the repaired W/Z-route replay evidence
and compare the result against the Phase110 target-independent repair contract.
