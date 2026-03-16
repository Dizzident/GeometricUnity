# Phase XII Joined Structured Calculation Study

## Purpose

Produce one reproducible run folder where boson spectra, fermion modes, and boson-fermion coupling artifacts are all computed from the same persisted structured background family.

## Checked-in inputs

- Config: `studies/phase12_joined_calculation_001/config/background_study.json`
- Environment tier: `structured`
- Environment: `env-refinement-2x2`
- Branch manifest: `phase8-real-atlas-control`

## Executed run

- Output root: `studies/phase12_joined_calculation_001/output/background_family`
- Atlas: `studies/phase12_joined_calculation_001/output/background_family/atlas.json`
- Admitted backgrounds:
  - `bg-phase12-bg-a-20260315212202`
  - `bg-phase12-bg-b-20260315212202`

## Reproduction commands

```bash
dotnet run --project apps/Gu.Cli -- solve-backgrounds studies/phase12_joined_calculation_001/config/background_study.json --output studies/phase12_joined_calculation_001/output/background_family --lie-algebra su2
dotnet run --project apps/Gu.Cli -- compute-spectrum studies/phase12_joined_calculation_001/output/background_family bg-phase12-bg-a-20260315212202 --num-modes 12
dotnet run --project apps/Gu.Cli -- compute-spectrum studies/phase12_joined_calculation_001/output/background_family bg-phase12-bg-b-20260315212202 --num-modes 12
dotnet run --project apps/Gu.Cli -- track-modes studies/phase12_joined_calculation_001/output/background_family --context continuation
dotnet run --project apps/Gu.Cli -- build-boson-registry studies/phase12_joined_calculation_001/output/background_family
dotnet run --project apps/Gu.Cli -- run-boson-campaign studies/phase12_joined_calculation_001/output/background_family
dotnet run --project apps/Gu.Cli -- export-boson-report studies/phase12_joined_calculation_001/output/background_family
dotnet run --project apps/Gu.Cli -- assemble-dirac studies/phase12_joined_calculation_001/output/background_family --background-id bg-phase12-bg-a-20260315212202
dotnet run --project apps/Gu.Cli -- assemble-dirac studies/phase12_joined_calculation_001/output/background_family --background-id bg-phase12-bg-b-20260315212202
dotnet run --project apps/Gu.Cli -- solve-fermion-modes studies/phase12_joined_calculation_001/output/background_family --dirac studies/phase12_joined_calculation_001/output/background_family/fermions/dirac_bundle_bg-phase12-bg-a-20260315212202.json --count 12
dotnet run --project apps/Gu.Cli -- solve-fermion-modes studies/phase12_joined_calculation_001/output/background_family --dirac studies/phase12_joined_calculation_001/output/background_family/fermions/dirac_bundle_bg-phase12-bg-b-20260315212202.json --count 12
dotnet run --project apps/Gu.Cli -- analyze-chirality studies/phase12_joined_calculation_001/output/background_family --modes studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-a-20260315212202.json
dotnet run --project apps/Gu.Cli -- analyze-chirality studies/phase12_joined_calculation_001/output/background_family --modes studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-b-20260315212202.json
dotnet run --project apps/Gu.Cli -- analyze-conjugation studies/phase12_joined_calculation_001/output/background_family --modes studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-a-20260315212202.json
dotnet run --project apps/Gu.Cli -- analyze-conjugation studies/phase12_joined_calculation_001/output/background_family --modes studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-b-20260315212202.json
dotnet run --project apps/Gu.Cli -- extract-couplings studies/phase12_joined_calculation_001/output/background_family --fermion-modes studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-a-20260315212202.json
dotnet run --project apps/Gu.Cli -- extract-couplings studies/phase12_joined_calculation_001/output/background_family --fermion-modes studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_modes_bg-phase12-bg-b-20260315212202.json
dotnet run --project apps/Gu.Cli -- build-family-clusters studies/phase12_joined_calculation_001/output/background_family
dotnet run --project apps/Gu.Cli -- build-unified-registry studies/phase12_joined_calculation_001/output/background_family
```

## Evidence summary

- Boson side: two structured spectra, twelve tracked mode families, twelve boson candidates, one boson campaign result, one boson atlas report.
- Fermion side: two background-aware Dirac bundles with explicit matrices, two fermion mode bundles, two chirality analyses, two conjugation reports, one fermion family atlas, one family cluster report.
- Coupling side: two coupling atlases, twenty-four persisted finite-difference variation bundles, zero blocked variations in this run.

## Limits carried forward

- Boson comparison is still internal-target-profile only.
- Fermion observation is still proxy-level and both chirality outputs are fully trivial in this run.
- Fermion mode residuals remain large, so the fermion solve is executable but not numerically clean enough for physical comparison claims.
- The structured 2x2 environment is still a low-dimensional control geometry, not a real later-phase physical comparison geometry.
