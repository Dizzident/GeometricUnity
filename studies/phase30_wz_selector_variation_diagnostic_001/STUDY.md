# Phase XXX: W/Z Selector Variation Diagnostic

This study tests whether the Phase28 W/Z ratio failure is explained by branch,
refinement, or environment variation in the P27-selected W/Z mode records.

## Inputs

- `studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json`
- `studies/phase22_selector_source_spectra_001/modes/`
- `studies/phase19_dimensionless_wz_candidate_001/physical_targets.json`

## Outputs

- `wz_selector_variation_diagnostic.json`

## Result

- terminal status: `selector-variation-diagnostic-complete`;
- aligned selector points: 48;
- ratio minimum: `0.8637742965335011`;
- ratio maximum: `0.8637742965335012`;
- ratio mean: `0.8637742965335007`;
- ratio standard deviation: `5.009764024549043E-16`;
- target inside observed selector envelope: false;
- selector points passing sigma-5: 0.

The W/Z ratio is effectively invariant across the current branch, refinement,
and environment selector grid. Selector variation does not explain the Phase28
physical target miss.
