# IMPLEMENTATION_P50.md

## Purpose

Phase50 defines and validates the W/Z-only campaign integration surface that
separates the physical W/Z ratio claim from global Phase46 sidecar blockers.

The campaign must consume W/Z-specific branch/refinement values,
W/Z-specific sidecars, and a W/Z-only falsifier relevance audit. It must not
modify Phase46 artifacts or source code.

## Scope

Owned files for this phase:

- `studies/phase50_wz_only_campaign_integration_001/**`
- `IMPLEMENTATION_P50.md`

Out of scope:

- source code changes;
- edits to existing Phase46 studies;
- edits to Phase47 audit output;
- replacing unresolved W/Z-specific inputs with Phase5 global sidecars.

## Inputs

Already available:

- Phase50 W/Z-only relevance audit:
  `studies/phase50_wz_only_campaign_integration_001/report/physical_claim_falsifier_relevance_audit.json`
- Phase46 promoted W/Z physical prediction artifacts:
  `studies/phase46_electroweak_term_wz_physical_prediction_001/observables.json`
  `studies/phase46_electroweak_term_wz_physical_prediction_001/physical_observable_mappings.json`
  `studies/phase46_electroweak_term_wz_physical_prediction_001/observable_classifications.json`
  `studies/phase46_electroweak_term_wz_physical_prediction_001/physical_calibrations.json`
  `studies/phase46_electroweak_term_wz_physical_prediction_001/physical_mode_records.json`
  `studies/phase46_electroweak_term_wz_physical_prediction_001/mode_identification_evidence.json`

The W/Z-only branch/refinement values are available from the parallel handoff
study:

- `studies/phase50_wz_branch_refinement_inputs_001/config/branch_quantity_values.json`
- `studies/phase50_wz_branch_refinement_inputs_001/config/refinement_values.json`

The W/Z-scoped sidecars are available from the parallel handoff study:

- `studies/phase50_wz_scoped_falsification_sidecars_001/observation_chain.json`
- `studies/phase50_wz_scoped_falsification_sidecars_001/environment_variance.json`
- `studies/phase50_wz_scoped_falsification_sidecars_001/representation_content.json`
- `studies/phase50_wz_scoped_falsification_sidecars_001/coupling_consistency.json`
- `studies/phase50_wz_scoped_falsification_sidecars_001/wz_scoped_registry.json`
- `studies/phase50_wz_scoped_falsification_sidecars_001/config/sidecar_summary.json`
- `studies/phase50_wz_scoped_falsification_sidecars_001/sidecar_scope.json`

## Integration Contract

The Phase50 campaign config is W/Z-only if all of the following are true:

- branch/refinement target quantity ids are restricted to
  `physical-w-z-mass-ratio`;
- sidecar paths point to W/Z-specific sidecars under the Phase50 study, not the
  global Phase5 or Phase46 sidecars;
- the Phase50 W/Z-only audit path is included and its current status is treated
  as an input to physical-claim language;
- global severe falsifiers remain visible through the audit, but they do not
  masquerade as target-relevant W/Z blockers.

## Validated Run Command

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase50_wz_only_campaign_integration_001/config/campaign.json \
  --out-dir study-runs/phase50_wz_only_global_clear_check \
  --validate-first
```

## Current Status

`validated-predicted`

The campaign file is wired to W/Z-only branch/refinement paths and W/Z-scoped
sidecars. No unresolved placeholder artifact paths remain.

Validation result:

- campaign spec validation: passed;
- active fatal falsifiers: 0;
- active high falsifiers: 0;
- physical claim gate: passed;
- physical prediction terminal status: `predicted`;
- W/Z target comparison: passed;
- computed W/Z ratio: `0.8796910570948282`;
- target W/Z ratio: `0.88136`;
- pull: `1.0879885044906925`.

The Phase50 W/Z-only relevance audit reports:

- active severe falsifiers: 0;
- target-relevant severe falsifiers: 0;
- global sidecar severe falsifiers: 0;
- terminal status: `wz-physical-claim-falsifier-clear`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed: 204/204.
- `dotnet run --project apps/Gu.Cli -- validate-phase5-campaign-spec --spec studies/phase50_wz_only_campaign_integration_001/config/campaign.json --require-reference-sidecars`
  passed.
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase50_wz_only_campaign_integration_001/config/campaign.json --out-dir study-runs/phase50_wz_only_global_clear_check --validate-first`
  passed.
- jq acceptance checks passed for zero active fatal/high falsifiers, predicted
  terminal status, passing W/Z target comparison, and report/scorecard W/Z value
  agreement.
