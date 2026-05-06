# Phase 105 - Candidate-3 Physical Derivation Prerequisites

## Goal

Complete the setup work needed before the next physical-derivation phase can
attempt candidate-3 mapping, calibration, uncertainty propagation, and target
comparison.

## Completed

- Added `studies/phase105_candidate3_physical_derivation_prerequisites_001`.
- Consumed:
  - Phase101 prediction package;
  - Phase104 blocked mapping attempt;
  - Phase75 absolute W/Z miss diagnostic;
  - Phase76 weak-coupling normalization audit;
  - Phase72 calibration;
  - Phase74 failed absolute comparison.
- Updated the Phase101 package to reference Phase102, Phase103, Phase104, and
  Phase105 follow-on evidence.

## Result

Phase105 confirms the existing absolute W/Z mass pipeline is not plug-compatible
with the candidate-3 source value:

- candidate-3 source value is an internal coupling-proxy magnitude;
- existing absolute W/Z projection consumes validated mass-energy W/Z modes plus
  a GeV-per-internal-mass calibration;
- direct use of the candidate-3 proxy as a weak coupling would require a
  multiplier of `6108.115548050812` to reach the Phase75 target-implied weak
  coupling, which is a diagnostic only, not a calibration.

The next required artifacts are now explicit:

- candidate-3 observable identity derivation;
- target-independent normalization;
- uncertainty propagation;
- calibrated target comparison.

## Verification

- `dotnet build studies/phase105_candidate3_physical_derivation_prerequisites_001/Phase105Candidate3PhysicalDerivationPrerequisites.csproj --verbosity minimal`
- `dotnet run --project studies/phase105_candidate3_physical_derivation_prerequisites_001/Phase105Candidate3PhysicalDerivationPrerequisites.csproj --no-build`
- `dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj`
