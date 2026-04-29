# Phase LI - Broad Boson Prediction Readiness

## Goal

Phase L produced a validated W/Z-only physical prediction campaign. Phase LI
defines the next scaling boundary: what must be built before claims can extend
from the W/Z ratio to broad boson predictions.

## Implementation

Added:

- `studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json`
- `studies/phase51_broad_boson_prediction_readiness_001/STUDY.md`
- `tests/Gu.Phase5.Reporting.Tests/Phase51BroadBosonReadinessTests.cs`

The readiness matrix covers:

- W/Z mass ratio;
- W absolute mass;
- Z absolute mass;
- Higgs mass;
- photon masslessness;
- gluon masslessness.

## Finding

Current validated prediction coverage is narrow:

- `physical-w-z-mass-ratio` is predicted;
- all other broad-boson prediction records are blocked.

Main blockers:

- no target-independent absolute mass-energy scale calibration for W/Z absolute
  masses;
- no scalar-sector Higgs source, identity rule, sidecars, mapping, or
  calibration;
- no photon/gluon masslessness target contracts, identity rules, computed
  observables, mappings, or sidecars.

## Next Step

Phase LII should target absolute W/Z masses first because the internal W and Z
modes are already validated. The phase must derive a target-independent
mass-energy calibration and emit:

- `physical-w-boson-mass-gev`;
- `physical-z-boson-mass-gev`;
- absolute-mass branch/refinement tables;
- W/Z absolute-mass sidecars;
- a campaign that compares both absolute masses without fitting either target.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 208, Failed: 0, Skipped: 0
- `jq -e . studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json`
- `git diff --check`
