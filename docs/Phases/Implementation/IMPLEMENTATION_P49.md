# Phase XLIX - W/Z Target-Scoped Report Integration

## Goal

Phase XLVIII added target-scoped physical claim gate support, but the Phase V
campaign path still needed to consume the Phase XLVII relevance audit and emit
the target-scoped status in regenerated W/Z reports.

## Implementation

Updated:

- `src/Gu.Phase5.Reporting/Phase5CampaignSpec.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignArtifactLoader.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignRunner.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignSpecValidator.cs`
- `apps/Gu.Cli/Program.cs`
- `schemas/phase5_campaign.schema.json`
- `tests/Gu.Phase5.Reporting.Tests/Phase5CampaignArtifactLoaderTests.cs`
- `studies/phase46_electroweak_term_wz_physical_prediction_001/config/campaign.json`

Added optional campaign spec field:

- `physicalClaimFalsifierRelevanceAuditPath`

The loader now deserializes the Phase XLVII
`WzPhysicalClaimFalsifierRelevanceAuditResult`; the campaign runner passes it to
`Phase5ReportGenerator`; the CLI copies it into run inputs and renders it in the
markdown report.

## Regenerated Report Check

Command:

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase46_electroweak_term_wz_physical_prediction_001/config/campaign.json \
  --out-dir study-runs/phase49_wz_target_scoped_physical_report_check \
  --validate-first
```

Result:

- campaign spec validation: passed;
- report JSON includes `physicalClaimFalsifierRelevanceAudit`;
- `physicalBosonPredictionAllowed`: false;
- `targetScopedPhysicalComparisonAllowed`: true;
- `targetScopedObservableId`: `physical-w-z-mass-ratio`;
- `targetRelevantSevereFalsifierCount`: 0;
- `globalSidecarSevereFalsifierCount`: 3;
- physical prediction terminal status: `target-scoped`.

Markdown report now includes:

- `Physical boson prediction: blocked.`;
- `Target-scoped physical comparison: allowed for physical-w-z-mass-ratio`;
- `Physical Claim Falsifier Relevance`;
- `Target-relevant severe falsifiers: 0`;
- `Global sidecar severe falsifiers: 3`;
- per-falsifier relevance lines for `falsifier-0001`, `falsifier-0002`, and
  `falsifier-0003`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed: 198/198.
- `dotnet run --project apps/Gu.Cli -- run-phase5-campaign ... --validate-first`
  passed for the Phase XLVI W/Z campaign with the Phase XLVII audit wired in.
- `jq -e . studies/phase46_electroweak_term_wz_physical_prediction_001/config/campaign.json`
  passed.

## Current Status

We can now make the precise statement:

- unrestricted campaign-level physical boson prediction remains blocked by
  global sidecar severe falsifiers;
- W/Z target-scoped physical comparison is allowed by the current gate because
  the numerical target passed, selector variation passed, and no severe
  falsifier directly targets the W/Z ratio or selected W/Z modes.

## Next Step

The remaining blocker for unrestricted physical prediction language is not the
W/Z value. It is the global sidecar policy:

- either resolve the global sidecars (`gauge-violation`, `solver-iterations`,
  and the Phase IV toy fermion registry representation-content blocker);
- or keep them as disclosed global blockers while using target-scoped language
  only for the W/Z physical comparison.
