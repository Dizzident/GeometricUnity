# Phase XIII External Evidence Study

This study is the staging area for Phase XIII runs that compare computed GU
observables against external references. Generated artifacts should be written to:

```bash
study-runs/phase13_external_evidence_001/
```

The `study-runs/` tree and each `studies/**/output/` directory are ignored by
git so evidence runs can produce large or transient files without entering the
repository.

## Implemented Gate

Phase XIII validation is fail-closed: every declared external target must either
have a matching computed observable or appear as a failed target coverage record.
Campaign spec validation also rejects target tables whose selectors cannot match
the computed observable artifact set.

## Reproduction Commands

```bash
dotnet build
dotnet test --no-build
dotnet run --project apps/Gu.Cli -- validate-quantitative \
  --observables <observables.json> \
  --targets <external_targets.json> \
  --fail-closed-target-coverage \
  --out study-runs/phase13_external_evidence_001/scorecard.json
```

For boson external analogy campaigns, provide descriptors explicitly:

```bash
dotnet run --project apps/Gu.Cli -- run-boson-campaign <runFolder> \
  --campaign <campaignSpec.json> \
  --external-descriptors <external_descriptors.json>
```

Descriptor files may be either a JSON array of `ExternalAnalogyDescriptor`
objects or an object with a `descriptors` array.

## Current Blockers

- External physical references still need a provenance-preserving adapter before
  they can be treated as evidence-grade targets.
- Candidate-to-observable links must be present before the project can make
  particle-level support claims rather than aggregate score claims.
- Existing synthetic/toy targets remain useful controls, not physical
  predictions.
