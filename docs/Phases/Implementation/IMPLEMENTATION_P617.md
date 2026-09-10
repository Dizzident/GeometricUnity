# Phase617 implementation record

A61,2026-09-10 UTC, completed on main as requested. FIRST frozen Release
execution PASSED unchanged after complete independent and MAIN reviews of
code, compiled helpers, proof, project, fixture, exact counts, resources,
precedence and all34 unique bindings. Full live726 sorted core/hash/tree
closure,14 false flags and single EOF were checked before execution.
Release builds had zero warnings/errors. MAIN explicitly approved and ran
the first scientific execution; this builder did not execute science.

Contract SHA256:
61479fae80b4a57df1850e10633efd270ac0fce6d96ca125c993fb6cbc1f6df2.
Identical full/summary JSON SHA256:
b41c563cc72b6d38cd1a8956122ae9b0b84b7ccbf527e68f480653e945d747cb.

First-run wall time reported by MAIN: 1.814s.
Actual resource counters: 108980 tracked matrix products,42350 tracked coefficient products,
largest tensor822; frequency predicate passed.
All38 prospective count fields matched exactly:

```json
{
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "contexts": 2,
  "frameControls": 2,
  "projectorControls": 2,
  "connectionEntries": 5488,
  "coordinateDerivativeEntries": 5488,
  "derivativeTraceRows": 28,
  "projectorDerivativeRows": 28,
  "wrongParallelHorizontalRows": 8,
  "omittedPartialVerticalRows": 20,
  "frameDerivativeEntries": 5488,
  "transportDerivativeEntries": 2744,
  "divergenceEntries": 28,
  "horizontalAnchorRows": 2,
  "kineticStageComparisons": 16,
  "reverseDerivativeRows": 28,
  "reverseSimplifiedRows": 28,
  "kineticLegRows": 4,
  "kineticGradientRows": 2,
  "kineticWitnessRows": 2,
  "omittedKineticLegRows": 4,
  "transportKineticRows": 1,
  "spinInputRows": 2,
  "sourceStageComparisons": 16,
  "sourceRows": 2,
  "algebraicAdjointRows": 4,
  "forwardAdjointProbes": 364,
  "adjointReconstructions": 4,
  "quadraticSourceCoefficients": 6,
  "quadraticAdjointCoefficients": 6,
  "quadraticGradientCoefficients": 6,
  "originalActionVariations": 1176,
  "parameterRows": 162,
  "equalWeightRows": 54,
  "unequalWeightRows": 108,
  "falseStationaryRows": 2
}
```

All14 complete projector derivatives agree with the independently typed
formulas after coordinate covariant differentiation and frame transport.
PT has trace9 and divergence0 but is not parallel. Both complete kinetic
legs equal the retained full J tensor; their combined theta0 Gamma01
coefficient is -(a-b)/2. All three full polynomial algebraic coefficients
and1176 original-action variations pass. The planted gamma1,kappa11,
a3/4,b-3/4 control has zero algebraic residual but differential coefficient
-3/4. This rejects the constant two-weight ansatz only: nonconstant
weights, additional Clifford grades and the coupled metric equation remain
outside the result.

There were no post-run changes to bound scientific inputs. The resource
figures are actual instrumented counters; CPU/memory estimates are not
measurements. No physical spectrum, source operator/norm or vacuum is
selected. All14 authority flags remain false, O4/external review pending,
Phase561 closed, WZ15/H14 source-field deficits and physical mass claims0.

Independent serialized-output and shared-verifier reviews passed.
Ordered101/202/integrity PASSED397/3,coverage31/31,overturn94/94;
incremental tooling unit tests50/50passed. Final incremental must follow
ALL edits before checkpoint; record its report in commit/handoff without
post-pass edits. MAIN owns final validation, scoped commit and push on main.
This implementation note is unbound and may record subsequent validation.
