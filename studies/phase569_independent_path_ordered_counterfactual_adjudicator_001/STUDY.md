# Phase 569: independent path-ordered counterfactual adjudicator

## Status

Implementation complete and preregistration frozen after the Phase568 v3
negative result became known. The contract discloses that timing, exact-binds
the full predecessor lineage and this phase's Program, and has not yet been
executed.

## Question

Does an implementation independent of Phases567 and 568 reproduce the
registered-versus-composable boundary-order counterfactual on all six preserved
Phase548 checkpoint positions, and does it agree with Phase568's downstream
classification?

The phase can detect a non-null action/gradient counterfactual. It cannot infer
that the alternate target would mix or converge: the preserved positions were
generated under the frozen Phase548 target. A non-null result therefore leaves
sampler causality unresolved and can at most motivate a separately
preregistered sampling pack.

## Method

- Verify exact hashes and both the outer and payload checksums of all six
  checkpoints. RNG-state words are never read into the computation.
- Before parsing any audited numeric output or checkpoint position, run planted
  checksum, commuting/noncommuting order, wrong-order, contraction-adjoint,
  objective-polynomial, and gradient-direction batteries.
- Reconstruct both curvatures directly from mesh incidence. The registered
  order is `[0,1,2]`; the composable order is `[0,2,1]`; their quadratic
  difference is independently required to equal `[x2,x1]`.
- Independently reconstruct the default theta-zero Shiab contraction, its
  transpose, the trace-pairing objective, and both full gradients.
- Compare curvature, action, and gradient on the origin and all six checkpoint
  positions. Adjudicate Phase568's recorded spectra; do not repeat its dense
  eigensolves.

## Boundaries

No Phase567 or Phase568 project is referenced and no implementation is shared.
This phase performs no sampling, HMC transition, replay, RNG draw, quotient,
gauge fixing, or measure normalization. It changes neither Phase548 nor its
terminal. It grants no Phase561, O4, Phase458, Phase481, production, physical-
unit, or GeV authority. `promotedPhysicalMassClaimCount=0`.
