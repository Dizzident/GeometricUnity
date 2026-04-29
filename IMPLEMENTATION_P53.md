# Phase LIII - Electroweak Scale Source Intake

## Goal

Move from the Phase LII absolute-scale audit to a concrete intake gate for the
scale source needed by absolute W/Z mass prediction.

## Implementation

Added:

- `studies/phase53_electroweak_scale_source_intake_001/scale_source_intake.json`
- `studies/phase53_electroweak_scale_source_intake_001/scale_bridge_validation_contract.json`
- `studies/phase53_electroweak_scale_source_intake_001/STUDY.md`
- `tests/Gu.Phase5.Reporting.Tests/Phase53ElectroweakScaleSourceIntakeTests.cs`

The intake enumerates three lanes:

- GU-derived internal absolute scale: blocked until a GeV bridge observable is
  derived;
- external disjoint electroweak input scale: blocked until a sourced input and
  internal bridge are materialized;
- W/Z target-fit scale: rejected.

## Finding

Absolute W/Z mass prediction is now blocked on a narrower item: there is no
selected target-independent scale source satisfying the Phase LII projection
contract. Adding an external constant alone is still insufficient unless the
repo also validates a bridge from that input to `internal-mass-unit`.

## Next Step

Phase LIV should implement one scale-source lane:

- either derive a GU internal absolute scale with GeV units; or
- ingest a sourced disjoint electroweak scale input and build the internal bridge
  evidence needed to convert it into GeV per internal mass unit.

After that, emit absolute W/Z observables and compare them to targets only after
projection.

## Validation

Completed:

- `jq -e . studies/phase53_electroweak_scale_source_intake_001/scale_source_intake.json`
- `jq -e . studies/phase53_electroweak_scale_source_intake_001/scale_bridge_validation_contract.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 215, Failed: 0, Skipped: 0
- `git diff --check`
