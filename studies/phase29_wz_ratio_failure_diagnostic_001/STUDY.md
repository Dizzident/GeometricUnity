# Phase XXIX: W/Z Ratio Failure Diagnostic

This study diagnoses the Phase28 physical W/Z ratio failure without changing
the identity rules or using the target to select modes.

## Inputs

- `studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json`
- `studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json`
- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`
- `studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json`
- `studies/phase19_dimensionless_wz_candidate_001/physical_targets.json`

## Outputs

- `wz_ratio_failure_diagnostic.json`

## Result

- terminal status: `wz-ratio-diagnostic-complete`;
- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- selected ratio: `0.8637742965335007`;
- selected pull: `-14.867815514417117`;
- selected pair is closest charged/neutral diagnostic pair to target: yes;
- charged/neutral pairs scanned: 27;
- charged/neutral pairs passing sigma-5: 0;
- scale factor required to land selected pair on target:
  `1.0203591418928235`;
- total computed uncertainty required for selected pair sigma-5 compatibility:
  `0.0035139406165252546`;
- uncertainty inflation factor required for selected pair sigma-5 compatibility:
  `2.9950392916898507`.

No target value was used to change identity selection or calibration. The target
is used only to diagnose the already-produced Phase28 comparison failure.
