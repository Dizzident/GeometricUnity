# Phase LXXV - W/Z Absolute Mass Miss Diagnostic

## Goal

Diagnose the Phase LXXIV absolute W/Z mass target miss without fitting the bridge silently.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WzAbsoluteMassMissDiagnostic.cs`
- `tests/Gu.Phase5.Reporting.Tests/WzAbsoluteMassMissDiagnosticTests.cs`
- `studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json`
- `studies/phase75_wz_absolute_mass_miss_diagnostic_001/STUDY.md`

The diagnostic computes the scale factor each prediction would need to hit its target and checks whether those factors are coherent across W and Z.

## Finding

The miss is coherent:

- W required scale factor: `1.1540428665064741`;
- Z required scale factor: `1.1518612358716782`;
- mean required scale factor: `1.152952051189076`;
- relative W/Z spread: `0.0018922128049869403`.

This points first at the weak-coupling amplitude normalization or scalar-sector relation, not at a W/Z-specific shared-scale failure.

The current weak coupling is `0.5656854249492381`; the target-implied value would be `0.6522081710229882`. That target-implied value is diagnostic only and must not be promoted as a fitted bridge.

## Next Step

Phase LXXVI should audit the Phase LXV weak-coupling amplitude extractor for missing normalization factors that could account for the coherent `1.152952051189076` shortfall.

## Validation

Completed:

- `jq -e . studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 268 passed
- `git diff --check`
