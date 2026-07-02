# Phase430: Net One-Loop Direction-Selection Probe

The named non-constant / full-one-loop successor to the Phase428 fermion-loop
no-go. Phase428 proved the FERMION loop alone, on constant rank-1 rays
`omega = t*u`, is an su(3) class function that falls like `-N log t` in every
direction and cannot select a block. Phase430 adds the BOSONIC one-loop
contribution and studies the NET functional `W(t) = W_B(t) + W_F(t)` along the
same rays.

## Workbench (recorded conventions, matching Phase428)

- 4x4 periodic lattice (V=16), 4-dim Euclidean spinors, `gamma_1 = sigma_x (x) I2`,
  `gamma_2 = sigma_y (x) I2`, naive central-difference Hermitian Dirac
  `hop_mu = i(T_mu - T_mu^T)/2`.
- su(3) Gell-Mann generators `T_a = lambda_a/2`; block menu `T={1,2,3}`,
  `D={4,5,6,7}`, `S={8}`.
- FERMIONIC one-loop: `W_F(t) = -1/2 sum log(lambda^2)` with the exact closed
  form `lambda^2 = (s1 + t*u_c)^2 + (s2 + t*u_c)^2` (`s_mu = sin(2 pi n/4)`,
  gauge eigenvalue `u_c`, multiplicity 4). Verified against a dense 192-dim
  solve (residual 1.6e-13).
- BOSONIC one-loop (RECORDED WORKBENCH MODEL of the transverse fluctuation
  determinant): `W_B(t) = +1/2 sum` over 16 momenta, 2 polarizations, 8 adjoint
  directions of `log(eps_k^2 + t^2 m_i^2(u))`, `eps_k^2 = sin^2 k1 + sin^2 k2`,
  `m_i^2(u) =` eigenvalues of `-ad_u^2` on su(3). Only the mode-count arithmetic
  is exact; tying the masses to the actual control-branch Hessian is named
  future work.

## Large-t slope convention

`W ~ N log t` at large `t`; the slope is `(W(t2)-W(t1))/log(t2/t1)`. The raw
`t=40 -> t=80` octave slope carries an `O(1/t^2)` lattice correction that scales
by `1/4` per octave (the fermion side cancels its `O(1/t)` term by momentum
symmetry; the boson side does not). Richardson extrapolation across the
`t=40/80/160` octaves removes it exactly for a pure `1/t^2` tail:
`N = (4*slope[80,160] - slope[40,80])/3`. Both the Richardson and raw octave
slopes are recorded; the batteries test the Richardson slope against the
integer targets at tolerance 0.05.

## Result (machine-verified)

Bosonic and fermionic log slopes have OPPOSITE signs, so the net slope is a
genuine competition whose winner depends on the fermionic matter content:

| content | bosonic | fermionic | NET |
|---|---|---|---|
| — (bosonic) | T/D +192, S +128 | — | — |
| fundamental-3 | | T/D -128, S -192 | **T/D +64 (CONFINED), S -64 (hypercharge)** |
| adjoint-8 | | T/D -384, S -256 | T/D -192, S -128 (selection FLIPS to T/D) |
| derived (4x fund) | | T/D -512, S -768 | T/D -320, S -640 (hypercharge steepest) |

- Bosonic targets: T/D `+192 = 2*16*6` (dim ker ad_u = 2 on regular T/D axes),
  S `+128 = 2*16*4` (ker ad(lambda_8) = 4).
- DERIVED CONTENT: per Phase404's blind one-family pattern the 16 contains 4
  triplet-type reps (2x 3 + 2x 3bar, `|eigenvalue|`-identical so loop-equivalent)
  and 4 su(3) singlets, i.e. fermionic content = 4 copies of the fundamental.
- T/D exact degeneracy per content (class function): residual 8.5e-14.
- Mixed-configuration teaser (`omega_x = a U`, `omega_y = b V`, pair
  `(lambda_1, lambda_4)`, dense fundamental eigensolve): the fermion loop alone
  plus the tree bosonic quartic `kappa*(ab)^2*||[u,v]||^2` leaves the minimum on
  an axis (boundary escape along a ray) — the motivation for the net analysis.

## Verdict

`netOneLoopDirectionSelective=True`; `fundamentalContentConfinesTdRays=True`;
`fundamentalContentSelectsHyperchargeAxis=True`;
`selectionIsMatterContentDependent=True` (adjoint flips);
`derivedContentSelectsHyperchargeAxis=True`;
`su3ToSu2U1BreakingDirectionDynamicallyPreferred=True`.

Honest caveat: every net slope is a log runaway — there is NO finite minimum on
any ray (`noFiniteMinimumOnRays=True`) and hence NO scale law. For the derived
content BOTH directions are unbounded, so "selected" means the steepest-descent
direction, not a stabilized minimum. The bosonic side is a recorded workbench
model. No target value is consulted; no Phase201 or Phase256 field is filled;
nothing is promoted.

## Run (Release, sub-second)

```bash
dotnet run -c Release --project studies/phase430_net_one_loop_direction_selection_probe_001/Phase430NetOneLoopDirectionSelectionProbe.csproj
```

Precursors gated: Phase404 (`guEmbeddingChainCouplingRatioEnumerationPassed`),
Phase410 (`curvatureCoupledVevSelectionProbePassed`), Phase428
(`fermionLoopBlockSelectionNoGoProbePassed` +
`fermionLoopBlockSelectionMechanismClosed`).
