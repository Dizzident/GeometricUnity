# Phase XXVIII: W/Z Physical Prediction Promotion

This study promotes the P27 identity-rule-ready W/Z modes into physical
comparison campaign inputs and runs a Phase V campaign against the external
W/Z mass-ratio target.

## Inputs

- `studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json`
- `studies/phase22_selector_source_spectra_001/candidate_mode_sources.json`
- `studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json`
- `studies/phase19_dimensionless_wz_candidate_001/physical_targets.json`

## Outputs

- `promotion_result.json`
- `physical_mode_records.json`
- `mode_identification_evidence.json`
- `observables.json`
- `physical_observable_mappings.json`
- `observable_classifications.json`
- `physical_calibrations.json`
- `config/campaign.json`

## Result

- promotion terminal status: `physical-prediction-artifacts-ready`;
- campaign spec validation: passed;
- scorecard matched target count: 1;
- scorecard missing target count: 0;
- physical prediction record: present;
- physical prediction terminal status: `blocked`;
- physical claim gate: blocked by active fatal/high falsifiers.

Physical comparison:

- computed `physical-w-z-mass-ratio`: `0.8637742965335007`;
- computed uncertainty: `0.001173253595128173`;
- target `physical-w-z-mass-ratio`: `0.88136`;
- target uncertainty: `0.00015`;
- pull: `14.867815514417117`;
- passed: false.

No external physical target values were used to construct the promoted
artifacts. The target table is used only by the Phase V comparison campaign.
