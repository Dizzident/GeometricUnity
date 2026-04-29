# Phase XLVII - W/Z Physical Claim Falsifier Relevance

## Purpose

Phase XLVI passed the W/Z numerical physical target comparison but left the
physical claim gate blocked by active fatal/high falsifiers. This study audits
whether those severe falsifiers directly target the W/Z physical ratio claim or
remain global campaign sidecars.

## Inputs

- `study-runs/phase46_electroweak_term_wz_physical_prediction_check/falsification/falsifier_summary.json`
- `study-runs/phase46_electroweak_term_wz_physical_prediction_check/quantitative/consistency_scorecard.json`
- `studies/phase46_electroweak_term_wz_physical_prediction_001/wz_selector_variation_diagnostic.json`
- `studies/phase46_electroweak_term_wz_physical_prediction_001/physical_mode_records.json`

## Result

Artifact:

- `physical_claim_falsifier_relevance_audit.json`

Summary:

- terminal status: `wz-physical-claim-target-clear-global-sidecars-blocked`;
- W/Z target comparison passed: true;
- W/Z selector variation passed: true;
- active severe falsifiers: 3;
- target-relevant severe falsifiers: 0;
- global-sidecar severe falsifiers: 3.

The active severe falsifiers are:

- `falsifier-0001`: branch fragility on `gauge-violation`;
- `falsifier-0002`: branch fragility on `solver-iterations`;
- `falsifier-0003`: representation content on `fermion-registry-phase4-toy-v1-0000`.

None directly target `physical-w-z-mass-ratio`, the selected W mode
`phase22-phase12-candidate-0`, the selected Z mode
`phase22-phase12-candidate-2`, or their Phase XII source candidate ids.

## Interpretation

The W/Z numerical claim is target-clear under the Phase XLVII relevance audit.
The global physical claim gate remains blocked because the current gate treats
all active fatal/high falsifiers as campaign-wide blockers. The remaining
closure requirement is to resolve those global sidecars or add a documented
target-scoped physical-claim policy before allowing unrestricted physical boson
prediction language.
