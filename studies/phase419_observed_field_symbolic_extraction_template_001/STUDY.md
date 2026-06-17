# Phase419 - Observed-Field Symbolic Extraction Template

## Purpose

Materialize the restart prompt's highest-priority post-Phase418 experiment:
assume a scalar doublet exists and write the FMS/dressing-field-style symbolic
map required to extract observed photon, W, Z, and Higgs rows.

This phase is not a prediction route by itself. It clarifies the algebraic
shape of the missing Phase256 artifact and keeps the contract unfilled.

## Result

- The symbolic map covers all 20 Phase256 intake fields.
- Photon, W, Z, and Higgs projection rows are present as template rows.
- The map records the required external pieces: native doublet carrier,
  observed vacuum, dressing field, electroweak embedding, weak-angle/coupling
  lineage, quadratic mass operator, separate W/Z rows, Higgs scalar row, pole
  extraction, stability sidecars, and GeV/unit normalization.
- FMS and dressing-field audits are inherited as external template support, not
  GU source-lineage evidence.
- `sourceDefinedPhase256FieldCount=0`.
- No Phase201 or Phase256 field is filled, and no W/Z/H mass claim is
  promoted.

## Validation

Run:

```bash
dotnet run --project studies/phase419_observed_field_symbolic_extraction_template_001/Phase419ObservedFieldSymbolicExtractionTemplate.csproj
```

Expected summary:

```text
observed-field-symbolic-extraction-template-materialized-contract-unfilled
observedFieldSymbolicExtractionTemplatePassed=True
allPhase256FieldsMappedBySymbolicTemplate=True
sourceDefinedPhase256FieldCount=0
canFillPhase256ObservedFieldExtractionContract=False
```
