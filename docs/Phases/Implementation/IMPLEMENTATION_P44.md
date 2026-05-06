# Phase XLIV - Selector-Eigen W/Z Physical Prediction Check

## Goal

Phase XLIII produced selector-specific eigenvalue-backed W/Z source spectra and
cleared the selector-spectrum independence blocker. Phase XLIV promotes those
Phase XLIII spectra through the existing W/Z identity-rule physical prediction
path and runs the physical W/Z mass-ratio comparison.

## Implementation

No production code changes were required. The existing Phase XXVIII promotion
path accepted the Phase XLIII source artifacts because the promoted source IDs
remain aligned with the Phase XXVII identity-rule records.

Generated:

- `studies/phase44_selector_eigen_wz_physical_prediction_001/promotion_result.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/physical_mode_records.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/mode_identification_evidence.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/observables.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/physical_observable_mappings.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/observable_classifications.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/physical_calibrations.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/config/campaign.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/config/sidecar_summary.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/wz_ratio_failure_diagnostic.json`
- `studies/phase44_selector_eigen_wz_physical_prediction_001/wz_selector_variation_diagnostic.json`

## Promotion Result

Command:

```bash
dotnet run --project apps/Gu.Cli -- promote-wz-physical-prediction-artifacts \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --candidate-mode-sources studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/candidate_mode_sources.json \
  --mode-families studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/mode_families.json \
  --out-dir studies/phase44_selector_eigen_wz_physical_prediction_001
```

Result:

- terminal status: `physical-prediction-artifacts-ready`;
- physical mode records: 2;
- observables: 1;
- promoted W/Z ratio: `0.8631570381342449`;
- promoted uncertainty: `0.0015118933497614763`.

## Physical Campaign Result

Command:

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase44_selector_eigen_wz_physical_prediction_001/config/campaign.json \
  --out-dir study-runs/phase44_selector_eigen_wz_physical_prediction_check \
  --validate-first
```

Result:

- campaign spec validation: passed;
- matched physical targets: 1;
- failed physical targets: 1;
- computed W/Z ratio: `0.8631570381342449`;
- computed uncertainty: `0.0015118933497614763`;
- target ratio: `0.88136`;
- target uncertainty: `0.00015`;
- pull: `11.981023246920753`;
- physical prediction terminal status: `blocked`.

The physical claim gate remains blocked by active fatal/high falsifiers and the
failed physical target match. Phase XLIV therefore produces a real physical
comparison, but not a successful physical boson prediction.

## Diagnostics

Ratio diagnostic command:

```bash
dotnet run --project apps/Gu.Cli -- diagnose-wz-ratio-failure \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --mixing-readiness studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json \
  --candidate-mode-sources studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/candidate_mode_sources.json \
  --mode-families studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/mode_families.json \
  --target-table studies/phase19_dimensionless_wz_candidate_001/physical_targets.json \
  --out studies/phase44_selector_eigen_wz_physical_prediction_001/wz_ratio_failure_diagnostic.json
```

Key result:

- terminal status: `wz-ratio-diagnostic-complete`;
- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- selected pull: `-11.981023246920753`;
- best diagnostic pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- required scale to target: `1.021088818212155`;
- uncertainty inflation needed for sigma-5 compatibility: `2.4059242701808032`;
- charged/neutral diagnostic pairs passing sigma-5: 0.

Selector variation diagnostic command:

```bash
dotnet run --project apps/Gu.Cli -- diagnose-wz-selector-variation \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --modes-root studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/modes \
  --target-table studies/phase19_dimensionless_wz_candidate_001/physical_targets.json \
  --out studies/phase44_selector_eigen_wz_physical_prediction_001/wz_selector_variation_diagnostic.json
```

Key result:

- terminal status: `selector-variation-diagnostic-complete`;
- aligned selector points: 36;
- selector points passing sigma-5: 0;
- ratio envelope: `0.8617415263399019` to `0.8654919922785442`;
- closest point: `bg-variant-d84f5d66fd98b842::L0-2x2::env-toy-2d-trivial`;
- closest ratio: `0.8654919922785442`;
- closest pull: `-15.040886122052667`.

Interpretation: Phase XLIII fixed the proxy/invariant-source blocker, but the
selector-eigen W/Z ratios still sit below the physical target. The target is
outside the observed selector ratio envelope, and the identity-selected pair is
already the best charged/neutral diagnostic pair in the current source set.

## Validation

Completed:

- `jq -e . studies/phase44_selector_eigen_wz_physical_prediction_001/*.json`
- `jq -e . studies/phase44_selector_eigen_wz_physical_prediction_001/config/*.json`
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase44_selector_eigen_wz_physical_prediction_001/config/campaign.json --out-dir study-runs/phase44_selector_eigen_wz_physical_prediction_check --validate-first`

## Next Step

The next blocker is not missing physical-comparison plumbing. It is the
remaining target miss after selector-specific eigen extraction. The next phase
should inspect whether the Phase XLIII selector-cell operator construction is
missing a derivation-backed electroweak normalization or mass-operator term that
can shift the W/Z ratio by about `2.1088818212155%` without using the external
target as a tuning input.
