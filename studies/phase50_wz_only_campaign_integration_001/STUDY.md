# Phase L W/Z-Only Campaign Integration

This study provides the W/Z-only campaign configuration that replaces the
Phase46 campaign's global branch/refinement and sidecar inputs for the physical
W/Z mass-ratio claim.

Status: `validated-predicted`

The campaign intentionally consumes:

- W/Z-specific branch quantity values from
  `studies/phase50_wz_branch_refinement_inputs_001/config/branch_quantity_values.json`;
- W/Z-specific refinement values from
  `studies/phase50_wz_branch_refinement_inputs_001/config/refinement_values.json`;
- W/Z-specific sidecars from
  `studies/phase50_wz_scoped_falsification_sidecars_001`;
- the Phase50 W/Z-only physical-claim falsifier relevance audit.

The W/Z numerical artifacts still come from the existing Phase46 promoted
physical prediction outputs. The resolved input wiring is listed in
`report/integration_report.json`.

Validated run command:

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase50_wz_only_campaign_integration_001/config/campaign.json --out-dir study-runs/phase50_wz_only_global_clear_check --validate-first
```

Result:

- active fatal falsifiers: 0;
- active high falsifiers: 0;
- physical claim gate: passed;
- physical prediction terminal status: `predicted`;
- W/Z target comparison: passed;
- computed W/Z ratio: `0.8796910570948282`;
- target W/Z ratio: `0.88136`;
- pull: `1.0879885044906925`.
