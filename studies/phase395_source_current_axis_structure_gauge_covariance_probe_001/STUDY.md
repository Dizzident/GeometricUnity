# Phase395: Source-Current Axis Structure and Global Gauge Covariance Probe

## Question

WHY do the fermionic source currents, the Phase378/379 Gram image, and the
Phase394 backreaction direction avoid gauge axis 1? (The sharpest open
internal question after Phase394.)

## Construction

1. **Basis-invariant block Gram**: per edge,
   `T_e[a,b] = Re sum conj(B_(e,a)) B_(e,b)` on the converged shell
   (invariant under shell basis changes); aggregate `T = sum_e T_e` (3x3).
2. **Omega invariants**: second-moment matrix `C = sum_e omega_e omega_e^T`,
   its dominant axis and fraction, and the angle to the symmetric direction
   `(1,1,1)/sqrt(3)`.
3. **Exact global gauge covariance**: for rotations `R = exp(theta rho(n))`
   the rotated operator is constructible from persisted artifacts by exact
   linearity (Phase389): `D' = D - delta_D[omega] + delta_D[R omega]`. Verify
   the rotated shell spectrum is identical and `T' = R T R^T`.

## Results: the axis question is answered structurally

1. **The background is the symmetric ansatz**: `C` is nearly rank-1 along
   `n_omega ~ (1,1,1)/sqrt(3)` (dominant fraction 0.969 / 0.971, angle to the
   symmetric direction 0.61 / 0.75 degrees).
2. **The block Gram is effectively RANK-ONE** (dominant fraction >= 0.9992;
   eigenvalues e.g. {2.4e-5, 4.7e-5, 8.8e-2}). There is no "suppressed axis"
   - there is a single DOMINANT direction `d`, and the near-null 2-plane's
   internal eigenvectors are unstable (nearly degenerate tiny eigenvalues),
   which is why naive minimum-direction readings differ between backgrounds.
3. **The Phase379 axis fractions are the coordinate shadow of `d`**: the
   dominant direction's squared components are [0.5433, 0.0009, 0.4558]
   (bg-a) and [0.5197, 0.0002, 0.4802] (bg-b), reproducing the persisted
   Phase379 Gram fractions. "Suppressed coordinate axis 1" simply means `d`
   has a tiny e_1 component.
4. **`d` lies in the charged plane orthogonal to the omega invariant axis**
   (angles 87.05 / 89.09 degrees to `n_omega`): the shell responses select a
   single direction perpendicular to the background's symmetric-ansatz
   direction.
5. **Exact global gauge covariance verified** (spectrum invariance and
   `T' = R T R^T` residuals <= 9.5e-11 for a quarter turn and a generic
   rotation): rotating the background presentation rotates `d`. The
   direction is GAUGE-COVARIANT, NOT CANONICAL.

## Consequence for the namespace program

No target-blind canonical photon/W/Z/H namespace map can be built from raw
carrier coordinate axes: the only gauge-invariant data are the geometry
relative to `n_omega` (the invariant-axis component vs the charged plane)
and rotation-invariant magnitudes. This strengthens the Phase385
missing-namespace-map boundary with a constructive reason, and it reframes
the Phase307/381/383 suppressed-axis blockers as gauge-frame statements.

## Status

Fail-closed. No namespace map, no canonical axis selector, no W/Z/H source
rows, no Phase201/Phase256 fields. `canFillPhase201WzContract=False`.

## Reproduce

```bash
dotnet run --project studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/Phase395SourceCurrentAxisStructureGaugeCovarianceProbe.csproj
```
