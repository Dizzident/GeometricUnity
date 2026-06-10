# Phase389: VO-7 Mixed-Linearization Discrete Gauge-Compatibility Identity Probe

## Question

VO-7 in the v29 completion document requires the coupled boson-fermion mixed
linearization blocks together with their gauge-compatibility identities.
Phase371/372 materialized the candidate mixed block `delta_D[b]` and its
reciprocal bilinear structure on the identity-weight control branch, but the
gauge-compatibility identities themselves had never been constructed or
tested. Does the discrete control branch admit exact gauge-compatibility
identities for the candidate mixed block, and what is the exact obstruction?

## Construction

With the persisted Phase12 base Dirac operator and persisted background
connection `omega` (156 edge-major su(2) coefficients), the probe verifies:

1. **Exact linearity / reconstruction**: `D(omega) = D_kin + delta_D[omega]`,
   where `D_kin` must be gauge-diagonal and gauge-replicated.
2. **Exact discrete gauge-compatibility identity**: for every vertex-supported
   and global su(2) gauge parameter `X` (with
   `X_hat = blockdiag_v rho(X_v) (x) I_spinor`, anti-Hermitian):

   ```
   [D(omega), X_hat] = delta_D[v(X)] + R(X)
   v(X)_e = (X_head - X_tail) + [omega_e, (X_head + X_tail)/2]
   R(X)   = sum_e (1/h_e) [E_th (x) Gamma (x) S_e + E_ht (x) Gamma^dagger (x) S_e]
   S_e    = { rho(omega_e), rho(DeltaX_e) } / 2
   ```

   `v(X)` is the discrete covariant differential (midpoint-averaged), and
   `R(X)` is the exact symmetric anticommutator obstruction of this
   discretization. `R` vanishes identically for constant (global) `X`,
   where the identity reduces to exact equivariance
   `[D, X_hat] = delta_D[[omega, X]]`.
3. **Contracted Ward identity**: for every persisted fermion mode,
   `Re<psi, delta_D[v(X)] psi> = Re<psi, [D, X_hat] psi> - Re<psi, R(X) psi>`.

Coverage: 2 backgrounds x (27 vertices x 3 generators + 3 global) = 168 gauge
directions, 2016 Ward contraction rows.

## Result

- Reconstruction residual: exactly `0` on both backgrounds.
- Exact identity residual: exactly `0` for all 168 directions.
- Global equivariance obstruction: exactly `0` for all 6 global directions.
- Obstruction size for vertex-local parameters: up to ~3.6% of the commutator
  Frobenius norm (genuine, exactly characterized by `S_e`).
- Ward contraction consistency: machine precision (max ~6.7e-16).
- **Caveat**: the Ward zero-current statement is NOT sharply tested. The
  persisted Phase12 fermion modes carry large solver residuals
  (`residualNorm ~ 12` recorded in the mode artifacts), so the eigen-residual
  bound is wide; it is reported as diagnostic only.

## Status

Fail-closed. This is the first VO-7 gauge-compatibility artifact, but it is a
discrete identity-weight control-branch result for the candidate mixed block
only. It does not provide a physical `M_psi`-compatible branch, a completed
fermionic action, completed physical mixed blocks, a physical effective-action
Hessian, an observed electroweak namespace map, or any Phase201/Phase256
contract field. `canFillPhase201WzContract=False`. VO-7 remains incomplete and
no physical W/Z/H promotion is possible from this route.

## Reproduce

```bash
dotnet run --project studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/Phase389Vo7MixedLinearizationGaugeCompatibilityIdentityProbe.csproj
```
