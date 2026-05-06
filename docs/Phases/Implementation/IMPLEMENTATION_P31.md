# Phase XXXI - W/Z Normalization Closure Diagnostic

## Goal

Phase XXXI closes the next blocker after P30 showed that selector
branch/refinement/environment variation does not explain the physical W/Z mass
ratio miss. P31 determines whether the scale required to hit the target is
already supported by an internal normalization/operator derivation, or whether
applying it would be target-fitted calibration.

## Implementation

- Added `WzNormalizationClosureDiagnostic`.
- Added CLI command `diagnose-wz-normalization-closure`.
- Added focused reporting tests for:
  - current identity normalization being blocked;
  - an operator-derived matching scale being recognized as a scale closure even
    when selector coverage remains separately blocked.
- Generated study artifact:
  - `studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json`

## Result

P31 status is `wz-normalization-closure-blocked`.

Key values:

- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- computed ratio: `0.8637742965335007`;
- physical target: `0.88136`;
- required scale to target: `1.0203591418928235`;
- declared Phase28 scale factor: `1`;
- declared scale delta: `-0.020359141892823507`;
- selector variation explains miss: `false`;
- derivation-backed scale available: `false`;
- normalization change allowed: `false`.

The current calibration is
`phase28-dimensionless-wz-ratio-identity-normalization`. It is a valid
dimensionless identity normalization, but it is not an operator-derived
normalization closure for the required 2.0359% scale shift.

## Commands

```bash
dotnet run --project apps/Gu.Cli -- diagnose-wz-normalization-closure \
  --ratio-diagnostic studies/phase29_wz_ratio_failure_diagnostic_001/wz_ratio_failure_diagnostic.json \
  --selector-diagnostic studies/phase30_wz_selector_variation_diagnostic_001/wz_selector_variation_diagnostic.json \
  --physical-calibrations studies/phase28_wz_physical_prediction_promotion_001/physical_calibrations.json \
  --out studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json
```

## Validation

Completed:

- `jq -e . studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json`
  passed.
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 174/174 tests.
- `dotnet test GeometricUnity.slnx` passed.

## Remaining Blocker

Physical W/Z prediction remains blocked because the only scale that would bring
the computed ratio to the target is not internally derived. The next phase
should build or audit an internal normalization/operator derivation source for
the W/Z mass operator ratio. If no such derivation exists, the current physical
claim must remain failed rather than applying the required scale from the
target.
