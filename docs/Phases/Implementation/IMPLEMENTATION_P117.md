# Implementation P117

## Goal

Determine whether Phase116's amplitude and common-bridge failure can be fixed by
selecting a different repaired exact fermion pair.

## Result

Added `studies/phase117_wz_repaired_pair_sweep_001`.

The phase reruns Phase84 for all 66 unordered pairs of the Phase95 repaired
exact fermion modes against the W and Z source modes, then aggregates the replay
outputs.

Terminal status:

`wz-repaired-pair-sweep-no-pair-repair`

Key findings:

- pair count: `66`
- admissible pair count: `1`
- common-bridge pair count: `0`
- strongest admissible pair: `pair-0-3`
- strongest pair W raw-to-target ratio: `0.0002815678812805859`
- strongest pair Z raw-to-target ratio: `0.0002478942158199359`
- strongest pair common-bridge relative spread: `0.2541741434112769`

This means changing only the repaired fermion pair does not repair the W/Z
absolute projection. The next blocker is upstream of pair choice: analytic
matrix-element normalization, boson vector/source normalization, or a missing
operator-scale factor.

Phase101 now points the next phase at Phase117 with `pair-sweep-exhausted`.

## Validation

- `dotnet build studies/phase117_wz_repaired_pair_sweep_001/Phase117WzRepairedPairSweep.csproj --verbosity minimal`
- `dotnet run --project studies/phase117_wz_repaired_pair_sweep_001/Phase117WzRepairedPairSweep.csproj --no-build`
- `dotnet build studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-restore --verbosity minimal`
- `dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-build`

## Next Phase

Investigate analytic matrix-element normalization, boson vector/source
normalization, and missing operator-scale factors upstream of the repaired
fermion pair choice.
