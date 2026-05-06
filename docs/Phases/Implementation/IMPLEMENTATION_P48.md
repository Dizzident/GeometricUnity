# Phase XLVIII - Target-Scoped Physical Claim Gate

## Goal

Phase XLVII showed that the remaining Phase XLVI severe falsifiers do not
directly target the W/Z physical ratio claim. Phase XLVIII updates the report
policy so W/Z target-scoped physical comparison language can be allowed while
unrestricted campaign-level physical prediction language remains blocked by
global sidecar falsifiers.

## Implementation

Updated:

- `src/Gu.Phase5.Reporting/PhysicalClaimGate.cs`
- `src/Gu.Phase5.Reporting/PhysicalPredictionTerminalStatus.cs`
- `src/Gu.Phase5.Reporting/Phase5Report.cs`
- `src/Gu.Phase5.Reporting/Phase5ReportGenerator.cs`
- `tests/Gu.Phase5.Reporting.Tests/PhysicalObservableContractTests.cs`

The physical claim gate now preserves the original unrestricted gate:

- `physicalBosonPredictionAllowed` still requires no active fatal/high
  falsifiers at campaign scope.

It also adds a separate target-scoped result:

- `targetScopedPhysicalComparisonAllowed`;
- `targetScopedObservableId`;
- `targetRelevantSevereFalsifierCount`;
- `globalSidecarSevereFalsifierCount`.

Target-scoped comparison is allowed only when:

- validated physical mappings are present;
- a physical-observable classification is present;
- calibration and physical target evidence are present;
- the Phase XLVII relevance audit says the W/Z target comparison passed;
- the Phase XLVII relevance audit says selector variation passed;
- the Phase XLVII relevance audit has zero target-relevant severe falsifiers.

Global sidecar severe falsifiers are still disclosed in the gate summary and do
not unlock unrestricted physical prediction language.

## Report Behavior

Reports can now include `physicalClaimFalsifierRelevanceAudit`. Markdown emits a
dedicated "Physical Claim Falsifier Relevance" section with:

- target observable;
- target comparison status;
- selector variation status;
- target-relevant severe falsifier count;
- global sidecar severe falsifier count;
- per-falsifier relevance classification.

When unrestricted prediction remains blocked but target-scoped comparison is
allowed, `PhysicalPredictionTerminalStatus` returns status `target-scoped`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed: 196/196.

## Next Step

Wire the Phase XLVII audit artifact into the Phase XLVI-style campaign/report
generation path, then regenerate a W/Z physical prediction report that displays
both statuses:

- unrestricted physical boson prediction: blocked by global sidecars;
- W/Z target-scoped physical comparison: allowed.
