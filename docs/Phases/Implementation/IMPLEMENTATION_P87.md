# Phase 87 - Fermion Gauge-Reduction Readiness Audit

## Goal

Close the next blocker for a correct boson prediction by making the remaining fermion gauge-reduction gap explicit and testable.

## Completed

- Added `FermionGaugeReductionReadinessAuditor`.
- Added tests for ready and blocked gauge-reduction states.
- Audited the Phase12 A/B Dirac and fermion-mode artifacts used by Phase86.
- Recorded that the current solver configuration requests gauge reduction, but the produced Dirac bundles and fermion modes are still unreduced.

## Finding

The codebase has Phase3 connection-space gauge-reduction machinery, but the Phase4 fermion Dirac assembly path does not apply it:

- `FermionSpectralConfig.GaugeReduction` defaults to `true`;
- `CpuDiracOperatorAssembler` always emits `GaugeReductionApplied = false`;
- `FermionSpectralSolver` emits `GaugeReductionApplied = false`;
- no persisted Phase12 fermion-compatible gauge projector or projected Dirac operator artifact exists.

## Physical Prediction Status

Still blocked. The replay path and exact residual repair are working, but the missing projected/gauge-reduced fermion operator means the current values remain internal replay diagnostics, not correct physical boson predictions.

## Next Step

Implement a fermion-compatible gauge-reduction materialization path:

1. construct the Phase12 connection-space gauge projector for the 52-edge SU(2) geometry;
2. define how that projector acts on fermion Dirac degrees of freedom, or add a justified fermion-specific redundancy projector;
3. assemble and persist a projected/gauge-reduced Dirac operator with `GaugeReductionApplied = true`;
4. exact-solve the projected operator;
5. rerun Phase86 branch-pair replay and require the gauge-reduction audit to pass before external comparison.
