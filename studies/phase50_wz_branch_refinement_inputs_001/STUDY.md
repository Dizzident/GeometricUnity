# Phase50 W/Z Branch Refinement Inputs

This study provides W/Z-specific branch and refinement quantity value inputs for the next physical W/Z campaign.

The values are derived from `studies/phase46_electroweak_term_wz_physical_prediction_001/wz_selector_variation_diagnostic.json`, using the selected W/Z source pair:

- W source: `phase12-candidate-0`
- Z source: `phase12-candidate-2`
- Observable: `physical-w-z-mass-ratio`
- Aligned selector points: 36

Branch quantity values average the Phase46 point ratios by `branchVariantId` across all refinement levels and environments. Refinement quantity values average the same point ratios by `refinementLevel` across all branch variants and environments.

These inputs intentionally replace the global `gauge-violation` and `solver-iterations` target track with a W/Z-only `physical-w-z-mass-ratio` track.
