# Phase LXXX - Production Analytic Replay Input Materialization Audit

## Goal

Complete the next production replay step by checking whether existing artifacts are sufficient to run the Phase LXXIX analytic weak-coupling replay harness on real W/Z inputs.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/ProductionAnalyticReplayInputMaterializationAuditor.cs`
- `tests/Gu.Phase5.Reporting.Tests/ProductionAnalyticReplayInputMaterializationAuditorTests.cs`
- `studies/phase80_production_analytic_replay_input_materialization_audit_001/production_analytic_replay_input_materialization_audit.json`
- `studies/phase80_production_analytic_replay_input_materialization_audit_001/STUDY.md`

The audit requires:

- selected physical W/Z boson mode source;
- persisted boson perturbation vector;
- persisted analytic variation matrix;
- selected fermion mode eigenvectors;
- full replayed coupling record with real and imaginary components;
- `analytic-dirac-variation-matrix-element:v1`;
- `unit-modes`;
- variation evidence id;
- provenance id.

## Finding

The existing Phase IV study code does compute analytic Dirac variations, but the persisted `studies/phase4_fermion_family_atlas_001/output/coupling_atlas.json` is a top-coupling summary. Its top entries are synthetic boson perturbations and only persist magnitudes. It does not persist the selected W/Z boson perturbation vector, analytic variation matrix, selected fermion eigenvectors in the replay package, or full real/imaginary coupling record.

Therefore it cannot be promoted as physical weak-coupling evidence.

## Next Step

Phase LXXXI should update the replay-producing path to persist a full production replay package:

- selected W/Z boson perturbation vector from physical mode artifacts;
- analytic variation matrix from `DiracVariationComputer.ComputeAnalytical`;
- selected fermion current modes with eigenvectors;
- full `BosonFermionCouplingRecord` including real, imaginary, magnitude, method, evidence id, and provenance.

Then the package should pass `ProductionAnalyticReplayInputMaterializationAuditor`, `AnalyticWeakCouplingReplayHarness`, and `DimensionlessWeakCouplingAmplitudeExtractor.ExtractFromEvidence`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 280 passed
- `git diff --check`
