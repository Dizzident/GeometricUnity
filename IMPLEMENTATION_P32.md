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

## Implementation Steps

1. Add an operator-normalization source inventory diagnostic.
   - Inputs:
     - P31 normalization closure diagnostic;
     - selected W/Z identity readiness or physical mode records;
     - one or more artifact roots containing Dirac/coupling/operator records.
   - Output:
     - `operator_normalization_source_audit.json`.

2. For each candidate source, record:
   - artifact path;
   - artifact kind;
   - source candidate IDs or boson mode IDs referenced;
   - normalization convention;
   - whether the artifact is explicitly marked as proxy-only;
   - whether it contains an operator derivation ID;
   - whether it yields a dimensionless W/Z scale;
   - whether that scale was computed without reading the physical target.

3. Add promotion rules:
   - `promotable` only if the source provides a dimensionless W/Z scale,
     references the selected W/Z sources, has an operator/normalization
     derivation ID, and is not proxy-only.
   - `audit-only` for useful supporting evidence that cannot set scale.
   - `blocked` when no promotable source exists.

4. If a promotable source exists:
   - write a derived `PhysicalCalibrationRecord` with method
     `operator-normalization-closure`;
   - rerun P31 against that derived calibration;
   - rerun the physical W/Z comparison campaign.

5. If no promotable source exists:
   - keep physical W/Z prediction blocked;
   - create explicit requirements for the missing operator normalization
     derivation contract.

## Acceptance Criteria

- The audit can be run from the CLI.
- The generated audit JSON is valid.
- Tests cover:
  - proxy-only coupling artifacts are not promotable;
  - a synthetic operator-derived matching scale is promotable;
  - no-source/no-promotable-source cases remain blocked.
- Full solution tests pass.

## Expected Outcome

Based on the current artifact search, P32 is likely to conclude that existing
Dirac/coupling artifacts are supporting evidence only, not a valid W/Z
normalization closure. If so, the next phase must implement the missing
operator-normalization derivation itself rather than attempting to reuse
proxy-only coupling outputs.
