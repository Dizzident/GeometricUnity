# Phase XLVI - Electroweak-Term W/Z Source Spectra

## Goal

Phase XLV showed that the Phase XLIII selector-eigen spectra were solver-backed
but still lacked electroweak, mixing, charge-sector, normalization-scale, or
nontrivial mass-operator term evidence. Phase XLVI adds a target-independent
electroweak feature operator term to the selector-cell source spectra and reruns
the W/Z physical comparison.

## Implementation

Updated:

- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceSpectrum.cs`
- `src/Gu.Phase5.QuantitativeValidation/InternalVectorBosonSourceMatrixCampaign.cs`
- `apps/Gu.Cli/Program.cs`
- `src/Gu.Phase5.Reporting/WzSelectorEigenOperatorTermAudit.cs`

The source-spectrum campaign spec now accepts optional `identityFeaturePath`.
When supplied, the selector-cell solve derives a target-independent
`electroweak-feature-charge-anisotropy:v1` operator term from Phase XXV identity
features:

- charge sector;
- dominant SU(2) basis energy fraction;
- isotropic-basis anisotropy;
- current-coupling mean magnitude;
- electroweak multiplet and coupling signature provenance.

The term contributes to selected spectra as `operatorTermEvidence` and emits
mode block participation in `connection` and `electroweak-mixing`. The physical
target table is not read during source extraction.

## Source Spectrum Campaign

Added:

- `studies/phase46_electroweak_term_wz_source_spectra_001/config/source_spectrum_campaign_electroweak_term.json`

Command:

```bash
dotnet run --project apps/Gu.Cli -- run-internal-vector-boson-source-spectrum-campaign \
  --spec studies/phase46_electroweak_term_wz_source_spectra_001/config/source_spectrum_campaign_electroweak_term.json \
  --out-dir studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra
```

Result:

- spectra: 432;
- mode families: 12;
- terminal status: `candidate-source-ready`;
- ready candidates: 12;
- selector-cell bundles: Phase XL manifest;
- identity features: Phase XXV table.

Selected promoted family values:

- W source `phase12-candidate-0`: `1.1158059937692792E-15`;
- Z source `phase12-candidate-2`: `1.268406657962647E-15`.

Selected W operator term:

- term ID: `electroweak-feature-charge-anisotropy:v1`;
- charge sector: `charged`;
- coupling mean magnitude: `0.057993823115516055`;
- dominant basis energy: `0.6636395176414301`;
- basis anisotropy: `0.33030618430809683`;
- relative mass shift: `0.019155718426724814`;
- electroweak-mixing block participation: `0.38830000742361287`.

## Physical Comparison

Command:

```bash
dotnet run --project apps/Gu.Cli -- promote-wz-physical-prediction-artifacts \
  --identity-readiness studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json \
  --candidate-mode-sources studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/candidate_mode_sources.json \
  --mode-families studies/phase46_electroweak_term_wz_source_spectra_001/source_spectra/mode_families.json \
  --out-dir studies/phase46_electroweak_term_wz_physical_prediction_001

dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase46_electroweak_term_wz_physical_prediction_001/config/campaign.json \
  --out-dir study-runs/phase46_electroweak_term_wz_physical_prediction_check \
  --validate-first
```

Result:

- campaign spec validation: passed;
- matched physical targets: 1;
- passed physical targets: 1;
- failed physical targets: 0;
- computed W/Z ratio: `0.8796910570948282`;
- computed uncertainty: `0.001526619561417894`;
- target ratio: `0.88136`;
- target uncertainty: `0.00015`;
- pull: `1.0879885044906925`;
- target comparison passed: true;
- physical prediction terminal status: `blocked`.

Phase XLVI therefore clears the W/Z numerical target comparison, but does not
yet clear the physical claim gate because active fatal/high falsifiers remain.

## Diagnostics

Ratio diagnostic:

- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- selected pull: `-1.0879885044906925`;
- selected pair passes sigma-5: true;
- selected pair remains the best charged/neutral diagnostic pair;
- required scale to land exactly on target: `1.0018971920786413`.

Selector variation diagnostic:

- aligned selector points: 36;
- selector points passing sigma-5: 36;
- ratio envelope: `0.8782652373172205` to `0.882051476946298`;
- target lies inside the observed selector envelope;
- closest point ratio: `0.8815002461700707`;
- closest point pull: `0.13197757094469217`.

Operator-term audit:

- terminal status: `wz-selector-eigen-operator-term-ready`;
- inspected selected W/Z spectra: 72;
- solver-backed spectra: 72;
- nontrivial operator-term evidence count: 72;
- observed emitted mode blocks: `connection`, `electroweak-mixing`;
- closure requirements: none.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj`
  passed: 99/99.
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed.
- `jq -e .` over Phase XLVI source, promotion, campaign, and diagnostic JSON
  artifacts passed.
- Phase XLVI campaign ran with `--validate-first` and passed spec validation.

## Next Step

The next blocker is the physical claim gate, not the W/Z numerical comparison.
The next phase should inspect the active fatal/high falsifiers in the Phase XLVI
report and either resolve, demote, or explicitly document why each still blocks
physical boson prediction claims.
