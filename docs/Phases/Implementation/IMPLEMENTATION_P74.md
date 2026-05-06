# Phase LXXIV - Absolute W/Z Target Comparison

## Goal

Compare the projected W/Z absolute masses to the physical target values after projection.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WzAbsoluteMassTargetComparator.cs`
- `tests/Gu.Phase5.Reporting.Tests/WzAbsoluteMassTargetComparatorTests.cs`
- `studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json`
- `studies/phase74_wz_absolute_mass_target_comparison_001/STUDY.md`

The comparator computes residuals against PDG target values using combined prediction and target uncertainty.

## Finding

The predictions are no longer blocked, but they fail physical target comparison:

- W residual: `29.135235899380998 sigma`;
- Z residual: `28.692791899768647 sigma`.

## Next Step

The next phase should diagnose why the bridge-derived weak coupling/mass relation undershoots both absolute masses by about 13%, then decide whether the blocker is weak-coupling amplitude normalization, scalar-sector relation, or internal W/Z mode scale.

## Validation

Completed:

- `jq -e . studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 267, Failed: 0, Skipped: 0
- `git diff --check`
