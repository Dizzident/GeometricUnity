# Phase XXXIII - Canonical W/Z Operator Normalization Derivation

## Goal

Phase XXXIII addresses the P32 blocker by deriving the canonical W/Z
normalization scale from the shared internal mass-like operator unit, independent
of physical target values and without using coupling proxy magnitudes.

This phase is intentionally not a fit. If the target-required scale is not
derived by the shared operator convention, physical W/Z prediction must remain
failed.

## Implementation

- Added `WzCanonicalOperatorNormalizationDeriver`.
- Added CLI command `derive-wz-canonical-operator-normalization`.
- Added focused tests for:
  - shared W/Z operator unit deriving target-independent scale `1`;
  - mismatched source units blocking derivation.
- Tightened P32 audit rules so explicit `targetIndependent: true` and
  `proxyOnly: false` derivation artifacts are respected, instead of being
  blocked by negated provenance text.
- Generated study artifacts:
  - `canonical_operator_normalization_derivation.json`;
  - `physical_calibrations.json`;
  - `operator_normalization_source_audit_with_phase33.json`;
  - `wz_normalization_closure_with_phase33.json`.

## Result

P33 successfully derives a canonical shared-operator scale:

- terminal status: `wz-canonical-operator-normalization-derived`;
- selected W/Z sources:
  `phase12-candidate-0`, `phase12-candidate-2`;
- normalization convention: `shared-internal-mass-operator-unit`;
- dimensionless W/Z scale: `1`;
- target independent: `true`;
- proxy only: `false`.

Rerunning the P32 source audit with the P33 artifact now gives:

- status: `wz-operator-normalization-source-ready`;
- source count: `70`;
- promotable source count: `1`;
- best promotable source:
  `canonical_operator_normalization_derivation`;
- best source scale: `1`.

Rerunning P31 with the P33 calibration gives:

- status: `wz-normalization-closure-blocked`;
- derivation-backed scale available: `true`;
- required scale to target: `1.0203591418928235`;
- declared derived scale: `1`;
- normalization change allowed: `false`.

## Interpretation

The previous “missing operator normalization derivation” blocker is resolved.
The remaining W/Z blocker is now a normalized prediction mismatch: the
target-independent canonical operator derivation says the dimensionless ratio
scale is `1`, while agreement with the physical target would require
`1.0203591418928235`.

The next phase should stop looking for a normalization escape hatch and instead
diagnose the operator spectrum itself: mass-like operator definition, bosonic
mode eigenvalue extraction, or missing higher-order/continuum correction.

## Commands

```bash
dotnet run --project apps/Gu.Cli -- derive-wz-canonical-operator-normalization \
  --p31-diagnostic studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --out-dir studies/phase33_wz_canonical_operator_normalization_001

dotnet run --project apps/Gu.Cli -- audit-wz-operator-normalization-sources \
  --p31-diagnostic studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json \
  --artifact-roots studies/phase33_wz_canonical_operator_normalization_001/canonical_operator_normalization_derivation.json,studies/phase12_joined_calculation_001/output/background_family/fermions,studies/phase25_internal_electroweak_features_001/identity_features.json,studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json,studies/phase4_fermion_family_atlas_001/output/coupling_atlas.json,studies/phase4_fermion_family_atlas_001/output/dirac_bundle.json \
  --out studies/phase33_wz_canonical_operator_normalization_001/operator_normalization_source_audit_with_phase33.json

dotnet run --project apps/Gu.Cli -- diagnose-wz-normalization-closure \
  --ratio-diagnostic studies/phase29_wz_ratio_failure_diagnostic_001/wz_ratio_failure_diagnostic.json \
  --selector-diagnostic studies/phase30_wz_selector_variation_diagnostic_001/wz_selector_variation_diagnostic.json \
  --physical-calibrations studies/phase33_wz_canonical_operator_normalization_001/physical_calibrations.json \
  --out studies/phase33_wz_canonical_operator_normalization_001/wz_normalization_closure_with_phase33.json
```

## Validation

Completed:

- `jq -e . studies/phase33_wz_canonical_operator_normalization_001/canonical_operator_normalization_derivation.json`
  passed.
- `jq -e . studies/phase33_wz_canonical_operator_normalization_001/physical_calibrations.json`
  passed.
- `jq -e . studies/phase33_wz_canonical_operator_normalization_001/operator_normalization_source_audit_with_phase33.json`
  passed.
- `jq -e . studies/phase33_wz_canonical_operator_normalization_001/wz_normalization_closure_with_phase33.json`
  passed.
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 179/179 tests.
- `dotnet test GeometricUnity.slnx` passed.
