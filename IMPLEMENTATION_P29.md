# IMPLEMENTATION_P29.md

## Purpose

Phase XXIX diagnoses the Phase28 W/Z physical-ratio failure without retuning
the identity convention, W/Z source selection, or target comparison.

P28 produced the first real physical target comparison for
`physical-w-z-mass-ratio`, but the identity-rule-selected ratio failed by about
14.87 sigma. P29 turns that miss into a reproducible diagnostic artifact that
scans all charged/neutral internal pairs and quantifies whether the failure is
specific to the P27 selected pair or broader across the current source set.

## Phase XXIX Goal

Produce a diagnostic that:

- preserves P27 identity-rule selection as the selected pair;
- scans all charged/neutral source pairs as diagnostic-only alternatives;
- computes ratio, uncertainty, pull, required target scale, and uncertainty
  inflation needed for sigma-5 compatibility;
- reports whether any charged/neutral pair in the current internal set passes
  the physical target gate.

## Implementation Status

Started 2026-04-26.

- P29-M1 complete: added `WzRatioFailureDiagnostic`.
- P29-M2 complete: added CLI command `diagnose-wz-ratio-failure`.
- P29-M3 complete: generated
  `studies/phase29_wz_ratio_failure_diagnostic_001/wz_ratio_failure_diagnostic.json`.
- P29-M4 complete: focused Phase V reporting tests pass.

Diagnostic terminal status:

- `wz-ratio-diagnostic-complete`

Key result:

- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- selected ratio: `0.8637742965335007`;
- selected total uncertainty: `0.001173253595128173`;
- target ratio: `0.88136`;
- selected pull: `-14.867815514417117`;
- required scale factor to land exactly on target: `1.0203591418928235`;
- required total computed uncertainty for sigma-5 compatibility:
  `0.0035139406165252546`;
- uncertainty inflation factor for sigma-5 compatibility:
  `2.9950392916898507`;
- charged/neutral diagnostic pairs scanned: 27;
- charged/neutral pairs passing sigma-5: 0.

The P27 selected pair is also the closest charged/neutral diagnostic pair to
the target. This means the miss is not explained by P24 choosing an obviously
wrong charged/neutral pair among the current internal source set.

## Reproduction

```bash
dotnet run --project apps/Gu.Cli -- diagnose-wz-ratio-failure \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --mixing-readiness studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json \
  --candidate-mode-sources studies/phase22_selector_source_spectra_001/candidate_mode_sources.json \
  --mode-families studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json \
  --target-table studies/phase19_dimensionless_wz_candidate_001/physical_targets.json \
  --out studies/phase29_wz_ratio_failure_diagnostic_001/wz_ratio_failure_diagnostic.json
```

Validation completed:

```bash
dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj
jq -e . studies/phase29_wz_ratio_failure_diagnostic_001/wz_ratio_failure_diagnostic.json
dotnet test GeometricUnity.slnx
```

Results:

- Phase29 JSON artifact: valid;
- Phase V reporting tests: passed, 170/170;
- full solution tests: passed.

## Guardrails

- Do not use the diagnostic best pair to retune W/Z identity selection.
- Do not apply the required scale factor as a calibration unless a separate
  internal derivation exists.
- Do not inflate uncertainty to pass the target gate without a source-backed
  uncertainty mechanism.
- Treat the failure as real evidence against the current branch/convention/source
  pipeline until a derivation-backed correction is available.

## Next Work

Investigate why no charged/neutral pair passes the sigma-5 target gate. The
next phase should test derivation-backed explanations: missing electroweak
normalization, incomplete uncertainty budget, branch/environment dependence, or
a genuine falsification of the current canonical Cartan convention pipeline.
