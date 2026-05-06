# Implementation P116

## Goal

Rerun the absolute W/Z projection after Phase115 cleared the fermion-quality
blocker.

## Result

Added `studies/phase116_wz_absolute_projection_rerun_001`.

The phase consumes the accepted Phase115 W/Z-route replay evidence, the Phase69
target-independent W/Z internal mass relation, the Phase54 external
electroweak scale, the Phase18 physical W/Z mass targets, and the Phase110
repair contract.

Terminal status:

`wz-absolute-projection-rerun-target-comparison-failed`

The rerun projects:

- W mass: `0.022607995794646805 GeV`
- Z mass: `0.025699926823525416 GeV`

Both fail the Phase18 target comparison:

- W sigma residual: `6041.0971431727585`
- Z sigma residual: `45581.150036339284`

The repaired W-route raw matrix element is only
`0.0002815678812805859` of the Phase110 target-implied raw magnitude. The
separate repaired W/Z route bridge values also fail common-scale consistency
with relative spread `0.2541741434112769`.

Phase101 now points the next phase at Phase116 with
`absolute-projection-rerun-failed`.

## Validation

- `dotnet build studies/phase116_wz_absolute_projection_rerun_001/Phase116WzAbsoluteProjectionRerun.csproj --verbosity minimal`
- `dotnet run --project studies/phase116_wz_absolute_projection_rerun_001/Phase116WzAbsoluteProjectionRerun.csproj --no-build`
- `dotnet build studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-restore --verbosity minimal`
- `dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj --no-build`

## Next Phase

Do not promote the rerun projection. Investigate the repaired matrix-element
amplitude and the W/Z common-bridge inconsistency before another absolute
projection rerun.
