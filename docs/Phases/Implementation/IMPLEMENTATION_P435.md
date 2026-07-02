# Implementation P435 - Two-Condensate Scale-Gap Probe

Phase435 decides the self-consistent background question on the
Phase428/430/431 workbench: the two-condensate (lambda_8, lambda_4)
landscape with the nonzero tree quartic plus both one-loop determinants.

## What It Computes

- IR-continuous relative potential: the zero-dispersion doubler momentum
  sector is excluded from both determinants (recorded convention), making
  V continuous with V(0,0) = 0 exactly (`potentialContinuousAtOrigin`
  battery); the momentum-block fermionic functional is verified against a
  dense 192-dim construction (residual 1.7e-12).
- Grid landscape, interior local minima, verified-descent refinement
  (backtracking line search - every accepted step strictly decreases V),
  finite-difference Hessian classification, near-origin onset scan, and
  the axis-runaway comparison, for the fundamental and derived contents.

## Exact Results

- Fundamental: no interior minima; no condensation onset
  (`fundamentalShowsNoCondensationOnset=True`).
- Derived: one interior LOCAL minimum (gradient 5e-4, Hessian
  positive-definite) at (a,b) ~ (3.72, 3.36), V = +276 - undercut by the
  pure-lambda_8 runaway (V = -814 at a = 20):
  `derivedAxisRunawayUndercutsInterior=True`.
- `finiteSelfConsistentScaleExists=False`;
  `scaleRequiresLogSaturationBeyondWorkbench=True`;
  `logSaturationStructureSourceDefined=False`.

## Scientific Boundary

The interior stationary point and its amplitude ratio are candidate-only
workbench structure. The scale gap of the Coleman-Weinberg program is now
quantified: direction selection (430), background-induced mass law (431),
and source-pinned breaking structure (432) all exist, but a finite
dynamical scale requires saturation structure absent from every reviewed
source. No contract field is filled; nothing is promoted.
