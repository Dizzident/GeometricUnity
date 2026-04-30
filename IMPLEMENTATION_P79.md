# Phase LXXIX - Analytic Weak-Coupling Replay Harness

## Goal

Make the analytic weak-coupling replay step executable from an analytic variation matrix and selected fermion modes.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/AnalyticWeakCouplingReplayHarness.cs`
- `tests/Gu.Phase5.Reporting.Tests/AnalyticWeakCouplingReplayHarnessTests.cs`
- `studies/phase79_analytic_weak_coupling_replay_harness_001/analytic_weak_coupling_replay_harness.json`
- `studies/phase79_analytic_weak_coupling_replay_harness_001/STUDY.md`

Updated:

- `src/Gu.Phase5.Reporting/Gu.Phase5.Reporting.csproj`

The harness:

1. accepts two `FermionModeRecord` inputs with eigenvectors;
2. accepts an analytic variation matrix for the selected boson mode;
3. computes the unit-mode matrix element using `CouplingProxyEngine.ComputeCoupling`;
4. stamps the coupling with `analytic-dirac-variation-matrix-element:v1`;
5. runs the result through `ReplayedRawWeakCouplingMatrixElementEvidenceBuilder`.

## Finding

The code path from analytic variation matrix to validated Phase LXXVII raw matrix-element evidence is now executable and tested.

This still does not produce a new production W/Z mass prediction because the persisted production analytic variation matrix and selected target-independent fermion current modes have not yet been materialized together.

## Next Step

Phase LXXX should materialize the production replay inputs:

- selected W/Z boson mode perturbation as an analytic `delta_D[b_k]` variation matrix;
- selected target-independent fermion current modes with eigenvectors;
- a persisted variation evidence id.

Then call `AnalyticWeakCouplingReplayHarness.ReplayFromAnalyticVariation` and feed the validated evidence into the weak-coupling extraction pipeline.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 278 passed
- `git diff --check`
