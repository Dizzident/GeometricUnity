# Phase LXXIX Analytic Weak-Coupling Replay Harness

## Purpose

Provide an executable path from analytic variation matrices and selected fermion modes to validated raw weak-coupling matrix-element evidence.

## Result

The harness now computes a unit-mode matrix element using `CouplingProxyEngine.ComputeCoupling`, labels it with `analytic-dirac-variation-matrix-element:v1`, and runs it through the Phase LXXVIII evidence builder.

The validated test path is synthetic. It proves the code route works, but it is not a physical prediction.

## Closure

The next blocker is production materialization: selected W/Z analytic variation matrices and target-independent fermion current modes with eigenvectors must be persisted and replayed through this harness.
