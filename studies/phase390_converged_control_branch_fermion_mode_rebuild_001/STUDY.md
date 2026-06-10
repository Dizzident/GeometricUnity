# Phase390: Converged Control-Branch Fermion Mode Rebuild and Sharp Pure-Gauge Ward Probe

## Question

Phase389 left two boundary defects open: the persisted Phase12 fermion modes
are not tight eigenmodes of the persisted base Dirac matrix (mode artifacts
record `residualNorm ~ 12`), so the pure-gauge Ward zero-current statement was
not sharply testable; and the mesh-volume `M_psi` branch carried the Phase372
actionable obstruction ("Rebuild an M_psi-compatible Dirac branch and solve
matching M_psi-compatible fermion modes"). Can both defects be removed at the
discrete control-branch level?

## Construction

For each persisted Phase12 background:

1. **Identity-weight branch**: eigensolve the persisted explicit base Dirac
   matrix `D` directly with an in-study complex Hermitian Jacobi solver.
2. **Mesh-volume `M_psi` branch**: eigensolve the symmetrized generalized
   pencil `B = M^{-1/2} D M^{-1/2}` (`M_psi` from
   `MassPsiWeightsBuilder.BuildFromMesh`, always positive) and reconstruct
   M-orthonormal generalized modes `v = M^{-1/2} w` with `D v = lambda M v`.
3. **Kernel accounting**: the toy mesh has 4 isolated vertices whose Dirac
   rows are identically zero, giving an exact 48-dimensional structural kernel
   per branch (4 vertices x 12 dofs). The target-blind selection rule excludes
   the near-zero kernel and takes the 12 smallest-|lambda| NONZERO eigenpairs
   (selecting kernel modes would make the Ward test vacuous).
4. **Structural commutation**: `[M_psi, X_hat] = 0` exactly, because `M_psi`
   is scalar per vertex block while gauge parameters act within vertex blocks.
   This conjugates the Phase389 gauge-compatibility identity exactly onto the
   M_psi branch: `[B, X_hat] = M^{-1/2} (delta_D[v(X)] + R(X)) M^{-1/2}`, and
   makes the generalized Ward statement `Re<v, [D, X_hat] v> ~ 0` valid for
   `D v = lambda M v` modes.
5. **Sharp Ward probe**: for all 84 gauge directions x 12 rebuilt modes x
   both branches x both backgrounds (4032 rows),
   `|Re<psi, [D, X_hat] psi>| <= 10 * 2 ||r|| ||X_hat psi|| + 1e-9` with `r`
   now at solver precision.
6. **Persisted-branch characterization**: per persisted mode, relative eigen
   residual and best overlap with the converged eigenbasis.

## Result

- Jacobi converged in 10-11 sweeps per solve; eigen-residuals <= 3.1e-13 and
  orthonormality residuals at solver precision on both branches.
- Zero-mode count exactly 48 per branch per background (isolated-vertex
  structural kernel), spectra symmetric under `lambda -> -lambda`.
- `[M_psi, X_hat] = 0` exactly (residual 0).
- Sharp Ward zero-current verified: max |current| `1.7e-14` (identity branch)
  and `9.3e-13` (mesh-volume branch) across all 4032 rows. This resolves the
  Phase389 sharpness caveat.
- Persisted Phase12 mode branch confirmed unconverged: relative eigen
  residuals `12.1 - 18.5`, and best overlap of any persisted mode with any
  single converged eigenvector only `0.376`. The persisted modes are
  non-eigen mixtures, not approximate eigenmodes.

## Status

Fail-closed. The Phase372 actionable obstruction is closed at the
control-branch level and the M_psi-compatible generalized control branch is
materialized, but the mesh, backgrounds, and `M_psi` remain toy/study-defined
objects. No physical `M_psi`-compatible GU branch, completed fermionic action,
completed physical mixed blocks, physical effective-action Hessian, observed
electroweak namespace map, or Phase201/Phase256 contract field is provided.
`canFillPhase201WzContract=False`. VO-7 remains incomplete.

## Reproduce

```bash
dotnet run --project studies/phase390_converged_control_branch_fermion_mode_rebuild_001/Phase390ConvergedControlBranchFermionModeRebuild.csproj
```
