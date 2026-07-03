# Phase442: Joint (omega, theta) Hessian-Degree Probe

The ratified first 4D physics study (design `FOUR_D_PLATFORM_DESIGN.md` §3.9). It
asks the one structural question the M1-M3 platform was built to answer: **does the
draft-canonical Einsteinian Shiab -- dim-4 base, spinor-realized reduced Lambda^2
slice, epsilon-conjugation -- break the exact-quadraticity of the JOINT (omega, theta)
Hessian that Phases 436/441 proved forbids a dynamical bosonic scale on the toy
family?**

## The no-go it tests against (physics-decisions §0, the "one physics trap")

On the control branch `Upsilon = S(F) - T` with `S = F` (identity Shiab) is exactly
degree-2 in `omega`, so the objective `S_B = (1/2)||Upsilon||^2` is degree-4, the
exact Hessian along a background `t*u` is degree-2 in `t`, masses^2 grow exactly as
`t^2`, the one-loop potential grows exactly as `log(t)`, and **no log-saturation ->
no dynamical scale**. Moving to a 4D base does not by itself break this: a linear
eq. 9.3 with trivial gauge dressing (`epsilon = 1`) reproduces the same degree-2
Hessian on a bigger mesh. What breaks quarticity is the `omega`/`theta`-dependence
the control sets to trivial: the **epsilon-conjugation** `Ad_eps = exp(ad_theta)`,
all-orders nonlinear in `theta`.

## The pinned epsilon realization (design §3.5, CO-SIGNED & RATIFIED, `dc7ddd36`)

`theta` is a **genuine INDEPENDENT `H`-valued DOF on VERTICES**
(`VertexCount x dimG`), NOT a function of `omega`. The Hessian is over the **JOINT
`(omega, theta)`**. The per-face conjugator `Ad_eps = exp(ad_theta)` is evaluated at
the face's representative vertex; `theta = 0 <=> eps = 1`. The slaved Wilson
`eps(omega)` ansatz is demoted to a labelled NON-GATING smoke-test.
`epsilonRealization = "independent-theta-dof"`, `hessianOverJointOmegaTheta = true`.

## Setup

`CreateUniform4D(1)` (committed default; `V=16, E=65, F=110, C=24`, so
`nOmega = 195`, `nTheta = 48`, joint DOF `243`), `su(2)` with the positive-definite
trace pairing. `Upsilon = S_h - T` with `T = TrivialTorsion = 0`, so
`S_B = (1/2)||S_h(omega, theta)||^2`. Joint rays `t*(u_omega, u_theta)` with random
unit directions (four seeds each). **Degree measurement:** the objective's fifth
`t`-difference (degree-4 exact => 0) and the Hessian's third `t`-difference (degree-2
=> 0), the Phase436 finite-difference machinery reused on the enlarged DOF vector.
`CreateUniform4D(2)` is confirmed opportunistically (`PHASE442_MESH_REFINEMENT=2`).

## The mechanism (verified, not assumed)

`S_h(F)_face = M(Ad_theta(F))_face`, where `M` is the per-cell Lambda^2 contraction
(acting on the spatial/face index) and `Ad_theta` acts on the ad/Lie index.

- **Identity-equivalent member** (`Phi1 = id0`, `Phi2 = none`): `R = identity` on
  Lambda^2, so the per-cell face map `M - I = W^T(R - I)Q = 0` -- **no face mixing**,
  `S_h_face = Ad_theta(F_face)`. Because the trace pairing is Ad-invariant
  (`||Ad(F)|| = ||F||`), `theta` cancels from the objective: the theta-block is
  degenerate and the degree stays 2.
- **Genuinely non-scalar `R`** (`sd2 = P_+`, `asd2 = P_-`): the face map mixes faces
  carrying DIFFERENT per-face `Ad` rotations, so `theta` survives into `S_B` and the
  all-orders `exp(ad_theta)` non-polynomiality lifts the joint-Hessian degree above 2.

So any degree-lift is attributable to the Shiab contraction (`R != I`), not to
inserting the `theta` DOF -- which is exactly what the isolation battery certifies.

## Results (arms, all recorded)

| arm | measurement | result (n=1) |
|---|---|---|
| **A CONTROL** `{id0, none, trivial}` | objective 5th / Hessian 3rd `t`-diff | degree-4/degree-2 **exact**: `2.6e-15` / `1.1e-13` (reproduces the Phase436/441 no-go on the 4D mesh); FAILS the richness certificate (expected) |
| **B ISOLATION** identity-equivalent, `theta` varied | theta-block Frobenius of the joint Hessian | **bit-exact `0.0`** (theta absent from Upsilon); the sd2 contrast couples theta (`6.6e-2 > 0`) |
| **C TREATMENT** `{sd2-id0, asd2-id0, sd2-none}` x `c in {0, 0.5, 1}`, independent-theta | joint-Hessian degree (5th-obj / 3rd-Hess) | every genuinely-rich member **lifts above degree 2** (`3e-6` .. `3e-5` relative, ~1e9x the machine floor) |
| **D HONESTY** (design battery #8) | 3rd `t`-diff vs theta amplitude | `[1.3e-13, 8.5e-7, 6.6e-6, 6.4e-5]` at amplitude `[0, 0.25, 0.5, 1.0]`: vanishes at 0, grows monotonically |
| **E ROBUSTNESS** both vertex->face rules | degree verdict under each | verdict **agrees** (both lift); incident-average attenuates the MAGNITUDE ~11x (recorded, not papered over) |
| **F SMOKE** slaved-wilson, `kappa = 1` | non-gating wiring check | `|D3| = 5.6e-1`; clearly labelled NON-GATING, NOT the study instrument |

Cross-check: the study-side per-face `Ad` machinery (used for the incident-average
rule) reproduces the shipped `EvaluateWithTheta` bit-for-bit (`maxDiff = 0.0`).

### Honest sub-findings (recorded, not hidden)

- **The lift tracks `R != identity`, not `R` non-scalar.** The `c = 1` `*-id0`
  members have `R = (1 - c) P = 0`, so they FAIL the richness certificate (`R` is the
  zero scalar) yet still lift, because `R - I = -I != 0` still produces face-mixing.
  The physically meaningful control -- and the unique no-lift case -- is `R = identity`
  (the identity-equivalent member of arms A/B). The genuine non-scalar contractions
  (`sd2`, `asd2`) are the headline; the `c = 1` annihilated members are recorded as
  scalar-but-non-identity lifters.
- **Vertex->face rule (arm E) is a MAGNITUDE difference, not a verdict flip.** The
  incident-average rule smooths `theta` over a face's three vertices and attenuates the
  lift ~10-11x, but the lift stays ~1e6-1e7x the machine floor under both rules, so the
  degree verdict is robust. The degree threshold is therefore defined RELATIVE to the
  same-mesh machine-exact control floor (a fixed relative tolerance is mesh-dependent,
  because the relative 5th-difference dilutes as the objective sums over more faces --
  which spuriously flipped the n=2 incident-average verdict under a fixed `1e-7`). The
  noise-relative threshold self-calibrates per mesh; both `n=1` and `n=2` are robust.

## MANDATORY FRAMING (physicist; carried verbatim in intent)

**A degree > 2 verdict is the NECESSARY condition for one-loop log-saturation, NOT
sufficient, and is NOT a scale.** No scale, pole, VEV, or GeV is produced. The
isolation battery (arm B) is front-and-center in the decision logic: it certifies
that any lift is caused by the Einsteinian contraction's epsilon-dependence, not by
merely inserting the `theta` degree of freedom (identity Shiab => bit-exact-zero
theta-block -- the check the slaved-Wilson form structurally cannot run). A degree-2
Einsteinian result would have been a legitimate frontier-sharpening outcome; the
observed degree > 2 establishes the necessary condition on a draft-canonical operator
for the first time -- a candidate MECHANISM, not a scale, not a promotion.

**The named NEXT study** (recorded): the joint effective potential + variational
`eps*(omega)` (solve `dS/deps = 0` and integrate epsilon out) + Coleman-Weinberg /
gap-equation saturation analysis -- the Phase435/438 machinery applied to the lifted
joint Hessian. That is where an eventual dynamical-scale EXTRACTION would live; it is
deferred and is not attempted here.

## Fail-closed

Target-blind construction (`targetBlindConstructionHash` recorded); reduced spin-4
slice only (`definition81Scope = reduced-spin4-slice`, `ambientSevenSevenRealized =
internalGaugeContentRealized = weldRealized = false`, `baseSignature =
Cl(4,0)-euclidean-slice`, `shiabOutputDegree = 2`, `draftOperatorIsDegreeRaising =
true`, `reducedRealizationCapturesRicciWeylAlgebraNotFormDegree = true`). No
scales/poles/GeV lineage; `noScaleProduced = noGevPromotion = true`. No Phase201 or
Phase256 contract field is filled (`canFillPhase201WzContract =
canFillPhase256Contract = false` unconditionally); `sourceContractApplicationAllowed
= phase201TemplateMutated = false`; `acceptedContractFieldCount = 0`. Nothing is
promoted either way.

`jointOmegaThetaHessianDegreeProbePassed` gates on internal consistency only
(precursors Phase436 + Phase441; control-arm reproduction; isolation battery; honesty
sweep; cross-check; carrier match) and **passes regardless of how the treatment
degree verdict falls** -- it reports what IS. Precursors: Phase436
(`exactHessianSaturationNoGoProbePassed && scaleGapPinnedBeyondControlBranch`) and
Phase441 (`toyBranchFamilyUniversalitySweepPassed`). Runtime ~0.7 s (n=1), ~2.4 s
(n=2).

## Terminal statement

On the faithful 4D Einsteinian Shiab with the independent-`theta` epsilon sector
switched on, the JOINT `(omega, theta)` Hessian exceeds degree 2 for every genuinely
non-scalar member, under both vertex->face rules, while the identity-equivalent
control reproduces the Phase436/441 degree-2 no-go exactly and the isolation battery
is bit-exact. This is the NECESSARY (not sufficient) condition for a dynamical scale,
met on a draft-canonical operator for the first time -- a candidate mechanism, not a
scale, and nothing is promoted. The scale question passes to the deferred variational
effective-potential study.
