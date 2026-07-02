# Phase435: Two-Condensate Scale-Gap Probe

The self-consistent background iteration named after the Phase430-434 team
wave. On the workbench, the two-condensate landscape (lambda_8 amplitude
`a` on the x-links, doublet lambda_4 amplitude `b` on the y-links; tree
quartic `(ab)^2 * 3/4` nonzero) is characterized with an IR-continuous
relative potential (the zero-dispersion doubler sector is excluded from
both determinants, a recorded convention making V(0,0) = 0 exactly).

Result:

- FUNDAMENTAL content: no interior stationary structure and no
  condensation onset near the origin - the workbench vacuum is the origin.
- DERIVED content: a genuine interior LOCAL minimum exists (gradient norm
  5e-4, positive-definite Hessian) at (a,b) ~ (3.72, 3.36) - a metastable
  two-condensate state, recorded candidate-only - but the pure-lambda_8
  log-runaway undercuts it (-814 at a=20 vs +276), so NO FINITE GLOBAL
  MINIMUM exists.
- Verdict: `finiteSelfConsistentScaleExists=False`,
  `scaleRequiresLogSaturationBeyondWorkbench=True`,
  `logSaturationStructureSourceDefined=False`. The scale gap is now a
  recorded quantity: a finite dynamical scale requires higher-order,
  compactness, or UV saturation structure that no reviewed source defines.

No Phase201 or Phase256 field is filled; nothing is promoted.

Run:

```bash
dotnet run -c Release --project studies/phase435_two_condensate_scale_gap_probe_001/Phase435TwoCondensateScaleGapProbe.csproj
```
