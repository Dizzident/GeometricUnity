# Phase437: Four-Dimensional Transmutation Scaling Probe

Phases 430/435/436 found log-runaways with no finite minimum on a 2D 4x4
workbench, and Phase436 proved the exact control-branch Hessian gives bosonic
masses^2 ~ t^2 exactly (one-loop term ~ log t). This phase decides the open
question: is the no-minimum verdict a small-2D-lattice artifact, or does it
survive the genuine Coleman-Weinberg regime? In continuum D=4 the one-loop term
scales as t^4 log t (same order as a tree quartic) - the dimensional-
transmutation regime that CAN produce a bounded minimum.

Workbench: Euclidean L^4 lattice, L in {4, 6, 8, 12}, naive Dirac in the
momentum-block form; pure ray `omega_mu = t*U` on ALL four directions (tree = 0
because `[U,U] = 0`); su(3) Gell-Mann with `U = lambda_8/2` (hypercharge/Cartan)
and `U = lambda_1/2` (T-type reference); fundamental (x1) and derived (x4
fundamental copies) contents. Per-unit-volume net one-loop functional with the
Phase435 IR convention (zero-dispersion doubler momenta excluded from both
determinants so `V(0) = 0`).

Result: the no-minimum runaway is STRUCTURAL, not a 2D artifact.

- LOG-DOMINATED at every L. The large-t octave log slope
  `s(t) = [W(2t)-W(t)]/log 2` is FLAT (growth ratio ~1.09, not the ~16x/octave
  a genuine `t^4 log t` term would give). Its Richardson value equals the
  analytic integer count exactly.
- 2D-vs-4D consistency (the exact anchor). The 4D per-volume slope reproduces
  the 2D-per-site slope to <1e-2 and reproduces Phase430's slope integers / 16
  exactly: derived hypercharge -40, derived T -20, fundamental hypercharge -4,
  fundamental T +4. The dimension does not change the verdict.
- CW regime unreachable. Bounded lattice dispersion (`|sin k| <= 1`) caps the
  phase space, so the continuum `t^4 log t` regime never turns on. The CW
  polynomial fit yields only tiny, ill-conditioned `t^4`-family coefficients
  (`B ~ 1e-3`, individual fitted terms exceed `|W|` by large cancelling
  factors). The fit formally implies a turning point
  `t* = exp(-C/B - 1/4) ~ 40` for the derived hypercharge content, but `t*`
  lies OUTSIDE the analysed window `[2,20]` and is NEVER realised: the direct
  scan finds no interior minimum at any L (derived contents run away to the
  t=20 boundary; the fundamental T-axis rises monotonically from the origin).

Verdicts: `landscapeLogDominatedAtEveryL=True`,
`fourDimensionalCwRegimeReached=False`, `transmutationMinimumExists=False`,
`runawayStructuralAtOneLoopInFourD=True`, `twoDimVsFourDConsistent=True`,
`noScalePromoted=True`.

Batteries (all pass): gamma anticommutation to 1e-12; L=2 dense 192-dim Dirac
reproduces the momentum-block functional to <1e-8; 2D anchor reproduces
Phase430's slope integers to <1e-3.

Everything is recorded blind as candidate-only workbench structure. No scale,
pole, or GeV lineage; no Phase201 or Phase256 field is filled; nothing is
promoted.

Run:

```bash
dotnet run -c Release --project studies/phase437_four_dimensional_transmutation_scaling_probe_001/Phase437FourDimensionalTransmutationScalingProbe.csproj
```
