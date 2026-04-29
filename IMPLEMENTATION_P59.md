# Phase LIX - W/Z Absolute Mass Observable Projector

## Goal

Add the code that turns validated W/Z internal mass modes plus a validated
absolute scale calibration build into physical GeV observables.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WzAbsoluteMassObservableProjector.cs`
- `tests/Gu.Phase5.Reporting.Tests/WzAbsoluteMassObservableProjectorTests.cs`
- `studies/phase59_wz_absolute_mass_observable_projector_001/observable_projector_contract.json`
- `studies/phase59_wz_absolute_mass_observable_projector_001/STUDY.md`

The projector emits:

- `physical-w-boson-mass-gev`;
- `physical-z-boson-mass-gev`.

It propagates uncertainty as:

`sqrt((internalModeUncertainty * scale)^2 + (internalModeValue * scaleUncertainty)^2)`.

## Finding

The absolute W/Z prediction software path is now mostly assembled:

1. bridge validation;
2. scale calibration construction;
3. absolute observable projection;
4. physical prediction gating.

The remaining blocker is still the missing real electroweak bridge derivation.

## Next Step

Phase LX should focus directly on deriving or failing a real bridge record from
available internal mass-generation evidence. If no evidence exists, the phase
should produce executable diagnostics that identify the exact missing internal
operator, coupling, or scalar-sector artifact.

## Validation

Completed:

- `jq -e . studies/phase59_wz_absolute_mass_observable_projector_001/observable_projector_contract.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 236, Failed: 0, Skipped: 0
- `git diff --check`
