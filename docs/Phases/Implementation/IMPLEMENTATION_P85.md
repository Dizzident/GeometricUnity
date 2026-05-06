# Phase 85 - Exact Fermion Residual Repair

## Goal

Remove the numeric fermion residual blocker from the first boson replay attempt, without pretending to fix the remaining physical-gate requirements.

## Completed

- Added a Phase85 residual-repair study script using `numpy.linalg.eigh`.
- Loaded the persisted Phase12 explicit Hermitian Dirac matrix for `bg-phase12-bg-a-20260315212202`.
- Generated exact dense eigenmodes from the matrix.
- Wrote two repaired fermion-mode bundles:
  - lowest absolute eigenvalue modes, including the null space;
  - first non-null modes with `|eigenvalue| >= 1e-10`.
- Parameterized the Phase84 replay runner with:
  - `PHASE84_FERMION_MODES_PATH`
  - `PHASE84_OUTPUT_DIR`
  - `PHASE84_MODE_I`
  - `PHASE84_MODE_J`
- Reran the source-backed boson replay with the exact non-null modes.

## Result

The non-null exact modes remove the residual blocker:

- max repaired residual: `2.9195578230195064e-14`
- replay status: `source-backed-analytic-replay-package-built`
- coupling magnitude: `0.0001371411566683907`

## Still Blocked

This is still not a correct physical boson prediction. The remaining blockers are real:

- the underlying Phase12 Dirac bundle has `gaugeReductionApplied: false`;
- branch/refinement stability evidence remains absent;
- the selected boson candidate registry still marks the relevant candidates as `C0_NumericalMode`.

## Next Step

The next real blocker is not the replay or residual calculation anymore. We need a gauge-reduced, branch/refinement-backed Phase12 fermion solve for the same 27-vertex, 52-edge geometry. Once that exists, Phase84 can rerun against those modes and the physical gate can become eligible to pass.
