# IMPLEMENTATION_P28.md

## Purpose

Phase XXVIII wires the P27 identity-rule-ready W/Z modes into the Phase V
physical comparison campaign inputs.

P27 made W/Z identity rules ready under an internal SU(2) Cartan convention.
P28 promotes those identity rules into typed campaign artifacts: identified
physical mode records, mode-identification evidence, a computed W/Z mass-ratio
observable, a physical observable mapping, an observable classification, and a
dimensionless identity calibration.

## Phase XXVIII Goal

Replace the Phase19 placeholder/provisional physical comparison inputs with
P27-derived inputs that can be validated by the campaign loader and compared to
the external W/Z mass-ratio target.

The phase succeeds if:

- promotion emits `physical-prediction-artifacts-ready`;
- campaign spec validation passes;
- the scorecard contains a real matched target for `physical-w-z-mass-ratio`;
- the report records the physical prediction value and its uncertainty.

## Implementation Status

Started 2026-04-26.

- P28-M1 complete: added `WzPhysicalPredictionArtifactPromoter`.
- P28-M2 complete: added CLI command
  `promote-wz-physical-prediction-artifacts`.
- P28-M3 complete: generated
  `studies/phase28_wz_physical_prediction_promotion_001/*` artifacts.
- P28-M4 complete: added a Phase28 campaign spec that consumes the promoted
  artifacts.
- P28-M5 complete: ran the Phase28 campaign with `--validate-first`.

Promotion terminal status:

- `physical-prediction-artifacts-ready`

Physical comparison result:

- observable: `physical-w-z-mass-ratio`;
- computed value: `0.8637742965335007`;
- computed uncertainty: `0.001173253595128173`;
- target value: `0.88136`;
- target uncertainty: `0.00015`;
- pull: `14.867815514417117`;
- target matched: yes;
- match passed: no.

Report terminal status:

- `blocked`

The terminal status remains blocked because the Phase V physical claim gate is
blocked by active fatal/high falsifiers after the failed physical target match.
This is no longer blocked by missing W/Z identity rules, mapping, classification,
or calibration.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- promote-wz-physical-prediction-artifacts \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --mode-families studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json \
  --out-dir studies/phase28_wz_physical_prediction_promotion_001

dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase28_wz_physical_prediction_promotion_001/config/campaign.json \
  --out-dir study-runs/phase28_wz_physical_prediction_check --validate-first
```

Validation completed:

```bash
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
jq -e . studies/phase28_wz_physical_prediction_promotion_001/*.json \
  studies/phase28_wz_physical_prediction_promotion_001/config/*.json
dotnet test GeometricUnity.slnx
```

Results:

- Phase28 JSON artifacts: valid;
- Phase V reporting tests: passed, 168/168;
- Phase28 campaign spec validation: passed;
- Phase28 campaign completed with one matched physical target and one failed
  match;
- full solution tests: passed.

## Guardrails

- Do not tune the W/Z source choice to the external target.
- Keep the target table external to promotion; promotion consumes only internal
  identity readiness, candidate sources, and uncertainty-bearing mode families.
- Treat the failed physical comparison as scientific signal, not as a reason to
  retune the convention or source selection.
- Keep the physical claim gate blocked while active fatal/high falsifiers remain.

## Next Work

Investigate the failed W/Z ratio comparison. The current identity-rule-selected
ratio is about 14.87 sigma from the target under the Phase28 uncertainty budget.
The next phase should inspect whether the failure is due to source selection,
branch/refinement/environment uncertainty underestimation, missing electroweak
mixing normalization, or a genuine falsification of this branch.
