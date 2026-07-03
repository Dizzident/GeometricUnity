# Implementation P442 - Joint (omega,theta) Hessian-Degree Probe

Phase442 is the first physics study on the dimension-four platform - the
experiment the platform build (M1-M3b) was made for. It decides whether
the draft-canonical Einsteinian Shiab (4D base, reduced Spin(4) slice,
epsilon-conjugation with theta an independent H-valued vertex field)
breaks the exact-quadraticity of the joint (omega,theta) Hessian that
Phases 436/441 proved forbids a dynamical bosonic scale on the 2D toy
family.

## Verdict: IT DOES (necessary-not-sufficient; no scale produced)

- CONTROL (identity-equivalent member, trivial mode): reproduces the
  Phase436/441 no-go on the 4D mesh EXACTLY - relative fifth
  t-difference 2.6e-15 (degree-4 objective, degree-2 Hessian).
- ISOLATION (identity member, independent-theta): the theta-block of
  the joint Hessian is BIT-EXACTLY ZERO (theta absent from Upsilon),
  while the Einsteinian theta-block is 6.6e-2 - any degree-lift is
  provably Shiab-caused, not an inserted-DOF artifact.
- TREATMENT: all nine Einsteinian members (sd2/asd2/sd2-none x
  EinsteinCoefficient {0, 0.5, 1}) exceed degree-2 in the joint Hessian
  (D5rel 1.4e-6 to 5.2e-4 against a noise-relative threshold with the
  control floor three-plus orders below), under BOTH vertex-to-face
  rules (lowest-index pinned; incident-average attenuates ~11.5x but
  agrees on every verdict - robust).
- HONESTY SWEEP: the third t-difference vanishes as the theta amplitude
  goes to zero (1.3e-13) and grows monotonically with it.
- The slaved-Wilson row is present as labeled non-gating smoke-test only.

## Mandatory Framing (physicist-pinned, carried in the study)

Degree > 2 is the NECESSARY condition for log-saturation, NOT
sufficient, and NOT a scale. No scale, pole, VEV, or GeV value is
produced; canFill*/routePromotes* = false; nothing is promoted. The
NAMED NEXT STUDY is the joint (omega,theta) effective potential with the
variational eps*(omega) mode and the Coleman-Weinberg/gap-equation
saturation analysis (the Phase435/438 machinery on the lifted Hessian).
Recorded boundary verbatim: definition81Scope=reduced-spin4-slice;
ambientSevenSevenRealized=false; internalGaugeContentRealized=false;
weldRealized=false; canFillPhase201WzContract=false;
canFillPhase256Contract=false. Runtime ~0.7 s.
