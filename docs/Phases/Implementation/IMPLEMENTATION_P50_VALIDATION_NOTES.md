# Phase L Validation/Reproducibility Notes - W/Z-Only Severe-Falsifier Closure

## Purpose

Phase L validation must prove two things at the same time:

- a W/Z-only Phase V campaign has no active global fatal/high falsifiers;
- the W/Z physical comparison is still reported honestly, with the computed
  `physical-w-z-mass-ratio`, target comparison, prediction records, and
  terminal status all machine-checkable.

This track is validation-only. It should not weaken source-code gates or hide
sidecars. The final Phase50 W/Z-only campaign at
`study-runs/phase50_wz_only_global_clear_check` clears active fatal/high
falsifiers and reaches unrestricted `predicted` status for the W/Z-only
campaign scope.

## Required Campaign Shape

Use a Phase50 W/Z-only campaign spec, expected at:

```bash
studies/phase50_wz_only_campaign_integration_001/config/campaign.json
```

The spec must keep the W/Z physical inputs from Phase XLVI/XLIX:

- `observablesPath` points to W/Z observable records containing
  `physical-w-z-mass-ratio`;
- physical mappings, classifications, calibrations, physical mode records, and
  mode-identification evidence are present and validated;
- `externalTargetTablePath` points to the PDG W/Z ratio target table;
- if target-scoped reporting is still needed, the spec wires
  `physicalClaimFalsifierRelevanceAuditPath`.

Current concurrent Phase50 inputs provide W/Z-only branch/refinement value
tables at:

```bash
studies/phase50_wz_branch_refinement_inputs_001/config/branch_quantity_values.json
studies/phase50_wz_branch_refinement_inputs_001/config/refinement_values.json
```

Those tables currently contain only `physical-w-z-mass-ratio`. Before the
campaign can run, the integration spec must either request only that quantity
or the handoff tables must also provide every requested mode quantity. Do not
run the campaign while the spec and tables disagree.

Current concurrent Phase50 scoped sidecars are available at:

```bash
studies/phase50_wz_scoped_falsification_sidecars_001/observation_chain.json
studies/phase50_wz_scoped_falsification_sidecars_001/environment_variance.json
studies/phase50_wz_scoped_falsification_sidecars_001/representation_content.json
studies/phase50_wz_scoped_falsification_sidecars_001/coupling_consistency.json
studies/phase50_wz_scoped_falsification_sidecars_001/config/sidecar_summary.json
studies/phase50_wz_scoped_falsification_sidecars_001/wz_scoped_registry.json
```

The scoped sidecar study's own `config/campaign.json` is useful as an input
wiring reference, but it still uses the global Phase5 branch/refinement target
track. The final global-severe-clear campaign should be the integration spec
after it is wired to both the W/Z-only branch/refinement inputs and the scoped
sidecars.

To clear global severe falsifiers, the spec must not include unrelated global
sidecar inputs that trigger non-W/Z severe falsifiers, such as the Phase46
`gauge-violation`, `solver-iterations`, or Phase IV toy fermion-registry
representation-content blockers. If those artifacts are still present and
active, the run has not cleared global severe falsifiers.

## Recommended Commands

Run from the repository root:

```bash
export P50_SPEC=studies/phase50_wz_only_campaign_integration_001/config/campaign.json
export P50_OUT=study-runs/phase50_wz_only_global_clear_check

dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj \
  --filter Phase50WzBranchRefinementInputTests

jq -e '
  [.branchFamilySpec.targetQuantityIds[]] as $requested |
  (["physical-w-z-mass-ratio"] | sort) as $available |
  ($requested | sort) == $available
' "$P50_SPEC"

jq -e '
  .integrationStatus != "draft-blocked-awaiting-wz-only-inputs"
' "$P50_SPEC"

jq -e '
  (.branchFamilySpec.targetQuantityIds | index("gauge-violation") | not) and
  (.branchFamilySpec.targetQuantityIds | index("solver-iterations") | not) and
  (.refinementSpec.targetQuantities | index("gauge-violation") | not) and
  (.refinementSpec.targetQuantities | index("solver-iterations") | not)
' "$P50_SPEC"

jq -e '
  (.registryPath | contains("phase50_wz_scoped_falsification_sidecars_001") or contains("wz_scoped_registry")) and
  (.observationChainPath | contains("phase50_wz_scoped_falsification_sidecars_001") or contains("wz_sidecars")) and
  (.environmentVariancePath | contains("phase50_wz_scoped_falsification_sidecars_001") or contains("wz_sidecars")) and
  (.representationContentPath | contains("phase50_wz_scoped_falsification_sidecars_001") or contains("wz_sidecars")) and
  (.couplingConsistencyPath | contains("phase50_wz_scoped_falsification_sidecars_001") or contains("wz_sidecars"))
' "$P50_SPEC"

dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec "$P50_SPEC" \
  --out-dir "$P50_OUT" \
  --validate-first

dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
```

If the integration spec intentionally keeps multiple requested W/Z quantities,
replace the first preflight `jq` check with a coverage check against the actual
handoff tables:

```bash
jq -n --argfile spec "$P50_SPEC" \
      --argfile branch studies/phase50_wz_branch_refinement_inputs_001/config/branch_quantity_values.json \
      --argfile refinement studies/phase50_wz_branch_refinement_inputs_001/config/refinement_values.json '
  ($spec.branchFamilySpec.targetQuantityIds | sort) as $branchRequested |
  ($spec.refinementSpec.targetQuantities | sort) as $refinementRequested |
  ([$branch.levels[].quantities | keys[]] | unique | sort) as $branchAvailable |
  ([$refinement.levels[].quantities | keys[]] | unique | sort) as $refinementAvailable |
  ($branchRequested - $branchAvailable | length) == 0 and
  ($refinementRequested - $refinementAvailable | length) == 0
'
```

Use this baseline check if Phase50 is expected to preserve target-scoped
language while global sidecars remain disclosed:

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase46_electroweak_term_wz_physical_prediction_001/config/campaign.json \
  --out-dir study-runs/phase50_phase49_target_scoped_regression_check \
  --validate-first
```

## Required jq Checks

Validate JSON shape:

```bash
jq -e . "$P50_OUT/falsification/falsifier_summary.json" >/dev/null
jq -e . "$P50_OUT/quantitative/consistency_scorecard.json" >/dev/null
jq -e . "$P50_OUT/reports/phase5_report.json" >/dev/null
```

Global severe falsifiers must be cleared for unrestricted Phase50 success:

```bash
jq -e '
  .activeFatalCount == 0 and
  .activeHighCount == 0 and
  ([.falsifiers[]? |
    select(.active == true and (.severity == "fatal" or .severity == "high" or .severity == "Fatal" or .severity == "High"))
  ] | length) == 0
' "$P50_OUT/falsification/falsifier_summary.json"
```

The report gate must show unrestricted physical prediction allowed:

```bash
jq -e '
  .physicalClaimGate.physicalBosonPredictionAllowed == true and
  .physicalPredictionTerminalStatus.status == "predicted"
' "$P50_OUT/reports/phase5_report.json"
```

If target-scoped mode is intentionally retained instead of clearing global
sidecars, use this alternate check and do not claim unrestricted prediction:

```bash
jq -e '
  .physicalClaimGate.physicalBosonPredictionAllowed == false and
  .physicalClaimGate.targetScopedPhysicalComparisonAllowed == true and
  .physicalClaimGate.targetScopedObservableId == "physical-w-z-mass-ratio" and
  .physicalClaimGate.targetRelevantSevereFalsifierCount == 0 and
  .physicalPredictionTerminalStatus.status == "target-scoped"
' "$P50_OUT/reports/phase5_report.json"
```

The W/Z target comparison must be present, matched, and passing:

```bash
jq -e '
  .matchedTargetCount == 1 and
  .missingTargetCount == 0 and
  ([.targetCoverage[]? |
    select(.observableId == "physical-w-z-mass-ratio" and .status == "matched")
  ] | length) == 1 and
  ([.matches[]? |
    select(
      .observableId == "physical-w-z-mass-ratio" and
      .targetLabel == "pdg-w-z-mass-ratio" and
      .passed == true and
      .targetEvidenceTier == "physical-prediction" and
      .targetBenchmarkClass == "physical-observable"
    )
  ] | length) == 1
' "$P50_OUT/quantitative/consistency_scorecard.json"
```

The reported physical prediction must be the W/Z dimensionless ratio and must
not be blocked:

```bash
jq -e '
  ([.physicalPredictions[]? |
    select(
      .status == "predicted" and
      .sourceComputedObservableId == "physical-w-z-mass-ratio" and
      .targetPhysicalObservableId == "physical-w-z-mass-ratio" and
      .particleId == "electroweak-sector" and
      .physicalObservableType == "mass-ratio" and
      .unitFamily == "dimensionless" and
      .unit == "dimensionless" and
      (.value | type == "number") and
      (.uncertainty | type == "number") and
      .uncertainty >= 0 and
      (.blockReasons | length) == 0
    )
  ] | length) == 1
' "$P50_OUT/reports/phase5_report.json"
```

Cross-check that the report prediction value equals the scorecard computed W/Z
ratio:

```bash
jq -n --argfile report "$P50_OUT/reports/phase5_report.json" \
      --argfile score "$P50_OUT/quantitative/consistency_scorecard.json" '
  ($report.physicalPredictions[] |
    select(.targetPhysicalObservableId == "physical-w-z-mass-ratio" and .status == "predicted") |
    .value) as $predicted |
  ($score.matches[] |
    select(.observableId == "physical-w-z-mass-ratio" and .targetLabel == "pdg-w-z-mass-ratio") |
    .computedValue) as $computed |
  (($predicted - $computed) | fabs) < 1e-12
'
```

## Acceptance Criteria

Unrestricted Phase50 success requires:

- Phase50 handoff tests pass;
- the campaign spec is no longer marked
  `draft-blocked-awaiting-wz-only-inputs`;
- branch/refinement requested quantity ids are fully covered by the Phase50
  W/Z-only handoff tables;
- `run-phase5-campaign --validate-first` exits zero for the Phase50 W/Z-only
  spec;
- reporting tests pass;
- active fatal/high falsifier count is zero in
  `falsification/falsifier_summary.json`;
- `physicalClaimGate.physicalBosonPredictionAllowed` is `true`;
- `physicalPredictionTerminalStatus.status` is `predicted`;
- exactly one W/Z physical target comparison is matched and passing;
- the report contains exactly one unblocked predicted W/Z ratio record;
- the report prediction value equals the scorecard computed W/Z value;
- markdown/JSON still disclose the physical target comparison rather than
  replacing it with an internal benchmark claim.

Target-scoped-only acceptance is different:

- global severe falsifiers may remain nonzero only if they are disclosed;
- `targetRelevantSevereFalsifierCount` must be zero;
- `targetScopedPhysicalComparisonAllowed` must be true;
- terminal status must be `target-scoped`, not `predicted`;
- final language must say unrestricted physical prediction remains blocked.
