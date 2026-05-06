# Phase LXXXI - Full Analytic Weak-Coupling Replay Package Builder

## Goal

Provide the concrete artifact shape that a production replay job must emit before the weak-coupling value can be used for W/Z absolute mass predictions.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/FullAnalyticWeakCouplingReplayPackageBuilder.cs`
- `tests/Gu.Phase5.Reporting.Tests/FullAnalyticWeakCouplingReplayPackageBuilderTests.cs`
- `studies/phase81_full_analytic_weak_coupling_replay_package_001/full_analytic_replay_package_contract.json`
- `studies/phase81_full_analytic_weak_coupling_replay_package_001/STUDY.md`

The package includes:

- selected boson mode id and source kind;
- boson perturbation vector;
- analytic variation matrix real and imaginary parts;
- selected fermion mode snapshots with eigenvector coefficients;
- full `BosonFermionCouplingRecord` with real, imaginary, and magnitude;
- Phase LXXVIII raw evidence build;
- Phase LXXX materialization audit.

## Finding

The code can now build and serialize a full analytic replay package when provided physical W/Z replay inputs. A synthetic test package validates the route, while synthetic boson sources remain blocked by the materialization audit.

This phase still does not produce a usable physical W/Z prediction because the selected physical W/Z perturbation vector and selected target-independent fermion current modes have not been materialized together in production artifacts.

## Next Step

Phase LXXXII should implement the production replay job that fills this package from real artifacts:

1. select the accepted W/Z mode source;
2. load or materialize its perturbation vector;
3. compute `DiracVariationComputer.ComputeAnalytical`;
4. load selected fermion current modes with eigenvectors;
5. run `AnalyticWeakCouplingReplayHarness`;
6. emit `FullAnalyticWeakCouplingReplayPackage`;
7. feed validated evidence into `DimensionlessWeakCouplingAmplitudeExtractor.ExtractFromEvidence`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj` - 282 passed
- `git diff --check`
