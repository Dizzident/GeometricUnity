# IMPLEMENTATION_P389: VO-7 Mixed-Linearization Discrete Gauge-Compatibility Identity Probe

## Scope

Phase389 adds the first constructed-and-verified gauge-compatibility artifact
for the VO-7 candidate mixed block. Phase388 left thirteen theorem
requirements unmet; one of the two named components of VO-7 itself
("precise mixed linearization blocks and their gauge-compatibility
identities") had a building block (Phase372) but no identity construction.
Phase389 closes that diagnostic gap at the discrete control-branch level and
fails closed on everything physical.

## Artifacts

- Study: `studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001`
- Project: `Phase389Vo7MixedLinearizationGaugeCompatibilityIdentityProbe.csproj`
- Source: `Program.cs`
- Outputs:
  - `output/vo7_mixed_linearization_gauge_compatibility_identity_probe.json`
  - `output/vo7_mixed_linearization_gauge_compatibility_identity_probe_summary.json`

## Mathematical content

The persisted Phase12 base Dirac assembly is exactly linear in the background
connection: `D(omega) = D_kin + delta_D[omega]`, with `delta_D` the analytic
variation from `Gu.Phase4.Couplings.DiracVariationComputer`. Phase389 verifies
this by subtracting `delta_D[omega]` (built from the persisted
`background_states/{bg}_omega.json` coefficients) from the persisted explicit
Dirac matrix and checking that the remainder is gauge-diagonal and
gauge-replicated (residual exactly 0).

For an infinitesimal gauge parameter `X` acting through the su(2) adjoint
(`X_hat = blockdiag_v rho(X_v) (x) I_spinor`), the probe verifies the exact
discrete identity

```
[D(omega), X_hat] = delta_D[v(X)] + R(X)
v(X)_e = DeltaX_e + [omega_e, Xbar_e]
R(X)   = sum_e (1/h_e) [E_th (x) Gamma (x) S_e + E_ht (x) Gamma^dagger (x) S_e]
S_e    = { rho(omega_e), rho(DeltaX_e) } / 2
```

over all 81 vertex-local and 3 global gauge directions per background
(168 total). Observed residual: exactly 0 in floating point for every
direction. For global parameters `DeltaX = 0`, so `R = 0` and the discrete
branch is exactly equivariant. For vertex-local parameters the obstruction is
genuine (up to ~3.6% of the commutator norm) and exactly characterized by the
symmetric anticommutator `S_e`, which is the midpoint-discretization artifact
of the gauge coupling and vanishes in the continuum limit for smooth fields.

The contracted pure-gauge Ward identity
`Re<psi, delta_D[v(X)] psi> = Re<psi, [D, X_hat] psi> - Re<psi, R(X) psi>`
holds at machine precision for all 2016 (direction, mode) rows. The Ward
zero-current statement (`Re<psi, [D, X_hat] psi> ~ 0` for eigenmodes) is NOT
sharply tested: the persisted Phase12 modes record `residualNorm ~ 12`, so the
eigen-residual bound is wide and reported as diagnostic only
(`wardEigenBoundSharp=false`, `persistedModeEigenResidualsLarge=true`).

## Fail-closed boundary

Phase389 accepts zero Phase201/Phase256 fields and keeps:

- `routeProvidesPhysicalMassPsiCompatibleBranch=false`
- `routeProvidesCompletedFermionicAction=false`
- `routeProvidesCompletedMixedLinearizationBlocks=false`
- `routeProvidesMixedLinearizationGaugeCompatibilityIdentities=false`
  (the physical VO-7 identities; the discrete control-branch identity is
  recorded separately as
  `discreteControlBranchGaugeCompatibilityIdentityMaterialized=true`)
- `routeProvidesPhysicalEffectiveActionHessian=false`
- `routeProvidesObservedElectroweakNamespaceMap=false`
- `canFillPhase201WzContract=false`, `canFillPhase201HiggsContract=false`,
  `canFillPhase256ObservedFieldExtractionContract=false`

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`vo7MixedLinearizationGaugeCompatibilityIdentityProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `vo7-mixed-linearization-gauge-compatibility-identity-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase389 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase389 run passes with all identity checks exact.
- Phase101, Phase202, claim-integrity verifier, and the full generator gate
  re-run with Phase389 included (Phase202 checklist passed count increases by
  one; objective remains incomplete by design).
