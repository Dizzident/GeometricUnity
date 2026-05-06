# Phase XXXII - W/Z Operator Normalization Source Audit

## Goal

Phase XXXII should resolve the blocker left by P31: physical W/Z prediction
cannot advance until the required ratio scale, `1.0203591418928235`, is derived
from internal operator/normalization artifacts rather than copied from the
physical target.

P32 must audit the existing Dirac, coupling, mass-like, and electroweak feature
artifacts and decide whether any can legitimately supply a W/Z normalization
scale.

## Current Facts

- P29 selected W/Z pair:
  `phase22-phase12-candidate-0/phase22-phase12-candidate-2`.
- Computed W/Z ratio: `0.8637742965335007`.
- Physical target: `0.88136`.
- Required scale to target: `1.0203591418928235`.
- P30 selector sweep found the ratio effectively invariant across the current
  branch/refinement/environment grid.
- P31 found the declared Phase28 calibration is identity scale `1`, with no
  operator-derived normalization closure for the required scale.

## Candidate Internal Sources

Audit these artifacts first:

- `studies/phase12_joined_calculation_001/output/background_family/fermions/dirac_bundle_*.json`
- `studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_*.json`
- `studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations/*.json`
- `studies/phase25_internal_electroweak_features_001/identity_features.json`
- `studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json`
- `studies/phase4_fermion_family_atlas_001/output/coupling_atlas.json`
- `studies/phase4_fermion_family_atlas_001/output/dirac_bundle.json`

Important constraint: existing coupling code and artifacts describe coupling
values as proxies that depend on normalization convention. P32 must not promote
one of these proxy values into a physical normalization scale unless the
artifact itself includes an explicit, target-independent derivation contract.

## Implementation

Completed:

- Added `WzOperatorNormalizationSourceAudit`.
- Added CLI command `audit-wz-operator-normalization-sources`.
- Added focused reporting tests for:
  - proxy-only coupling artifacts not being promotable;
  - synthetic operator-derived scale artifacts being promotable;
  - empty/no-source roots remaining blocked.
- Generated study artifact:
  - `studies/phase32_wz_operator_normalization_source_audit_001/operator_normalization_source_audit.json`

The diagnostic records, for each candidate source:

   - artifact path;
   - artifact kind;
   - source candidate IDs or boson mode IDs referenced;
   - normalization convention;
   - whether the artifact is explicitly marked as proxy-only;
   - whether it contains an operator derivation ID;
   - whether it yields a dimensionless W/Z scale;
   - whether that scale was computed without reading the physical target.

Promotion rules:

   - `promotable` only if the source provides a dimensionless W/Z scale,
     references the selected W/Z sources, has an operator/normalization
     derivation ID, and is not proxy-only.
   - `audit-only` for useful supporting evidence that cannot set scale.
   - `blocked` when no promotable source exists.

## Result

P32 status is `wz-operator-normalization-source-blocked`.

Key values:

- audited sources: `69`;
- promotable sources: `0`;
- audit-only sources: `30`;
- blocked sources: `39`;
- proxy-only sources: `28`;
- required P31 scale: `1.0203591418928235`.

Artifact kind counts:

- coupling atlas: `2`;
- coupling JSON/matrix artifacts: `25`;
- Dirac operator bundles: `3`;
- Dirac variation bundles: `24`;
- electroweak feature table: `1`;
- other JSON/array artifacts: `14`.

The audited coupling atlases and electroweak feature records reference the
selected W/Z source candidates, but they do not provide a dimensionless W/Z
normalization scale, do not include an operator-normalization derivation ID, and
are inferred as proxy-only evidence. They therefore cannot be promoted into a
physical calibration.

## Command

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-operator-normalization-sources \
  --p31-diagnostic studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json \
  --artifact-roots studies/phase12_joined_calculation_001/output/background_family/fermions,studies/phase25_internal_electroweak_features_001/identity_features.json,studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json,studies/phase4_fermion_family_atlas_001/output/coupling_atlas.json,studies/phase4_fermion_family_atlas_001/output/dirac_bundle.json \
  --out studies/phase32_wz_operator_normalization_source_audit_001/operator_normalization_source_audit.json
```

## Acceptance Criteria

- The audit can be run from the CLI. Complete.
- The generated audit JSON is valid. Complete:
  `jq -e . studies/phase32_wz_operator_normalization_source_audit_001/operator_normalization_source_audit.json`
  passed.
- Tests cover:
  - proxy-only coupling artifacts are not promotable;
  - a synthetic operator-derived matching scale is promotable;
  - no-source/no-promotable-source cases remain blocked.
- Focused reporting tests pass. Complete:
  `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed with 177/177 tests.
- Full solution tests pass. Complete:
  `dotnet test GeometricUnity.slnx` passed.

## Expected Outcome

P32 concludes that existing Dirac/coupling artifacts are supporting evidence
only, not a valid W/Z normalization closure. The next phase must implement the
missing operator-normalization derivation itself rather than attempting to reuse
proxy-only coupling outputs.
