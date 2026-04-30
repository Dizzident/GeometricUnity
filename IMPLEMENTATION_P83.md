# Phase 83 - Source-Backed Analytic Replay Package Runner

## Goal

Turn the Phase82 boson-vector materialization unblock into an executable replay path: selected source boson JSON plus fermion eigenvectors now produce a full analytic weak-coupling replay package without using top-coupling summaries or synthetic boson perturbations.

## Completed

- Added `SourceBackedAnalyticReplayPackageRunner`.
- The runner materializes a selected boson perturbation vector from source JSON.
- It validates geometry dimensions, edge arrays, and fermion eigenvector lengths before replay.
- It computes the analytic Dirac variation matrix with `DiracVariationComputer.ComputeAnalytical`.
- It replays the weak coupling through `AnalyticWeakCouplingReplayHarness`.
- It builds the audited Phase81 full replay package with `FullAnalyticWeakCouplingReplayPackageBuilder`.
- Added regression tests for:
  - successful source-backed package construction from a real `modeVector` field shape;
  - pre-replay blocking when the boson vector length does not match `edgeCount * dimG`.

## Validation

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
- Result: 289 passed, 0 failed.

## Prediction Status

This phase gives us the executable package-building path needed before physical boson prediction tests. It still does not claim accurate physical W/Z or broad boson values because the runner has not yet been driven over the actual Phase12 W/Z candidate set with the full production geometry and selected fermion modes.

## Next Step

Build the production Phase12 replay job that:

1. selects W/Z-like boson candidates from the persisted Phase12 spectra,
2. loads the matching Phase12 fermion eigenvectors,
3. derives the production geometry arrays,
4. runs `SourceBackedAnalyticReplayPackageRunner` for the selected boson/fermion triples,
5. writes the resulting prediction-test artifacts for comparison against external reference values.
