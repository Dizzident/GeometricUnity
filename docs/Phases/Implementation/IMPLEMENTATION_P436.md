# Implementation P436 - Exact-Hessian Saturation No-Go Probe

Phase436 ties the workbench bosonic determinant (Phases 430/431/435) to the
TRUE control-branch Hessian, using the Phase405 su(3) machinery
(CpuSolverBackend/EvaluateDerived; exact objective S_B = (1/2)||Upsilon||^2
with Upsilon = (1/2)[omega wedge omega] at A0 = 0).

## Exact Results

- The exact Hessian at background t*u decomposes as
  H(t) = A0 + t*A1 + t^2*A2 with the third t-difference EXACTLY ZERO
  (the action is exactly quartic). The odd term vanishes for the
  Cartan-type direction (lambda_8) and is nonzero for the root-type
  direction (lambda_4, |A1|/|A0| ~ 0.15) - recorded honestly (it does not
  affect asymptotics).
- CONSEQUENCE (the headline): exact Hessian masses grow exactly as t^2,
  the bosonic one-loop is exactly logarithmic at large t, and NO
  LOG-SATURATION CAN ARISE FROM THE EXACT CONTROL-BRANCH HESSIAN AT ONE
  LOOP: `scaleGapPinnedBeyondControlBranch=True`. The Phase435 scale gap
  is a property of the control branch itself, not of the workbench model.
- Growing-mode counts match the workbench structure exactly:
  lambda_8 -> 64 = adRank 4 x geometric multiplicity 16;
  lambda_4 -> 96 = adRank 6 x 16
  (`phase430SlopeCountsConfirmedByExactHessian=True`). Mass VALUES differ
  from the workbench model (max 0.271 vs 1.0) - recorded honestly; only
  counts entered the Phase430 verdicts.

## Scientific Boundary

The scale question is now pinned at theorem grade: a finite dynamical
scale requires structure beyond the exactly-quartic control branch - the
physical VO-6/VO-7 completion's additional terms or a source anchor. No
contract field is filled; nothing is promoted. Runtime ~0.3 s.
