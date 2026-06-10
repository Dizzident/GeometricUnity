# IMPLEMENTATION_P396: Gauge-Invariant Neutral/Charged Sector Separation Probe

## Scope

Phase396 materializes the first gauge-invariant observed-field-extraction
skeleton (per the Phase395 requirement) and verifies an exact structural
result: every su(2) triplet of the recomputed bosonic Gauss-Newton spectrum
splits as one neutral plus one charged pair relative to the background
invariant axis.

## Artifacts

- Study: `studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001`
- Project: `Phase396GaugeInvariantNeutralChargedSectorSeparationProbe.csproj`
- Outputs: `output/gauge_invariant_neutral_charged_sector_separation_probe.json`
  and `..._summary.json`
- Precursor: reads the Phase394 working directory (run Phase394 first; the
  generator orders them correctly).

## Results

| Quantity | bg-a | bg-b |
| --- | --- | --- |
| Triplet clusters | 34 | 34 |
| Clusters with neutral content ~ 1.0 | 34 | 34 |
| Max neutral-content deviation | <= 1.9e-7 | <= 1.9e-7 |
| Kernel neutral content (18-dim) | 6.000000000 | 6.000000000 |
| Extraction invariance residual | 7.8e-16 | 7.8e-16 |

Combined with Phase394's exact triplet clustering, the full bosonic spectrum
decomposes exactly into residual-U(1) multiplets {1 neutral, 2 charged} at
every level - the discrete skeleton of {Z-like, W-pair-like} classification,
built from gauge invariants only.

## Fail-closed boundary

The probe audits all 20 Phase256 fields with per-field reasons (recorded in
`phase256FieldAudit`): no hypercharge, no photon/Z mixing, no weak angle, no
scalar sector, no physical vacuum, no scale/pole/GeV lineage, and the
construction is study-defined rather than theorem-backed. Zero fields
filled; all promotion flags false.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks,
  after Phase395 and Phase394)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`gaugeInvariantNeutralChargedSectorSeparationProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `gauge-invariant-neutral-charged-sector-separation-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase396 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase396 run passes.
- Phase101, Phase202 (checklist 188 -> 189 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase396 included; objective remains
  incomplete by design.
