# IMPLEMENTATION_P30.md

## Purpose

Phase XXX tests whether the Phase28 W/Z physical-ratio failure is hidden by
branch, refinement, or environment variation in the underlying selected mode
records.

P29 showed that no charged/neutral source pair passes the sigma-5 target gate
using family-level values. P30 keeps the P27 identity-selected W/Z pair fixed
and recomputes its ratio at every aligned Phase22 branch/refinement/environment
point.

## Phase XXX Goal

Produce a selector-variation diagnostic that:

- loads the P27 identity-rule-selected W and Z source candidates;
- aligns their Phase22 per-mode records by branch, refinement, and environment;
- computes the W/Z ratio and extraction-only pull at every aligned point;
- reports whether the physical target lies inside the observed selector
  variation envelope;
- reports whether any aligned point passes sigma-5.

## Implementation Status

Started 2026-04-26.

- P30-M1 complete: added `WzSelectorVariationDiagnostic`.
- P30-M2 complete: added CLI command `diagnose-wz-selector-variation`.
- P30-M3 complete: generated
  `studies/phase30_wz_selector_variation_diagnostic_001/wz_selector_variation_diagnostic.json`.
- P30-M4 complete: focused Phase V reporting tests pass.

Diagnostic terminal status:

- `selector-variation-diagnostic-complete`

Key result:

- W source: `phase12-candidate-0`;
- Z source: `phase12-candidate-2`;
- aligned selector points: 48;
- ratio minimum: `0.8637742965335011`;
- ratio maximum: `0.8637742965335012`;
- ratio mean: `0.8637742965335007`;
- ratio standard deviation: `5.009764024549043E-16`;
- target inside observed envelope: false;
- passing selector points: 0.

Closest aligned point:

- `bg-variant-d840fea6e6d36748::L0-2x2::env-toy-2d-trivial`;
- ratio: `0.8637742965335011`;
- extraction-only pull: `-16.684911359583424`.

Interpretation:

- The W/Z ratio is effectively invariant across the current branch,
  refinement, and environment selector grid.
- The physical target is outside that observed envelope.
- Selector variation does not explain the Phase28 miss.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- diagnose-wz-selector-variation \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --modes-root studies/phase22_selector_source_spectra_001/modes \
  --target-table studies/phase19_dimensionless_wz_candidate_001/physical_targets.json \
  --out studies/phase30_wz_selector_variation_diagnostic_001/wz_selector_variation_diagnostic.json
```

Validation completed:

```bash
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
jq -e . studies/phase30_wz_selector_variation_diagnostic_001/wz_selector_variation_diagnostic.json
dotnet test GeometricUnity.slnx
```

Results:

- Phase30 JSON artifact: valid;
- Phase V reporting tests: passed, 172/172;
- full solution tests: passed.

## Guardrails

- Do not change identity selection based on target proximity.
- Do not treat extraction-only pulls as the full Phase28 uncertainty model.
- Do not claim that selector invariance validates the physical prediction; it
  only rules out branch/refinement/environment selector drift as the cause of
  the current miss.

## Next Work

With selector variation ruled out, the next phase should investigate
derivation-backed normalization or operator-level causes for the stable
approximately 2.036% ratio deficit. If no internal mechanism exists, the
current canonical Cartan convention/source pipeline should remain treated as
physically falsified for the W/Z mass-ratio target.
