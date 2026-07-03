# Phase443: Joint Effective-Potential Saturation Probe

The named follow-up to Phase442 (design `FOUR_D_PLATFORM_DESIGN.md` §3.9 "next
study"; physics-decisions `FOUR_D_PLATFORM_PHYSICS_DECISIONS.md` §6 epsilon
taxonomy **mode 3, `variational`** -- the deferred one, now built). It asks the
sufficiency question that Phase442's necessary-condition result opened.

## The question

Phase442 proved the draft-canonical Einsteinian Shiab lifts the joint
`(omega, theta)` Hessian **above degree 2** -- the NECESSARY condition for one-loop
log-saturation, which Phases 435/436/440/441 proved impossible on the 2D toy
(there the objective is exactly degree-4, the Hessian exactly degree-2, masses^2
`~ t^2`, the one-loop potential `~ log t`: log-runaway, no interior minimum, no
dynamical scale). **Necessary is not sufficient.** Does the lifted structure
actually PRODUCE log-saturation -- a finite interior minimum of the one-loop
effective potential -- the first internally generated dynamical-scale CANDIDATE?

## Mode 3 (variational), now built

For a given `omega` background we solve the theta-stationarity `dS_B/dtheta = 0`
for `theta*(omega)` (damped **Levenberg-Marquardt** on the least-squares
`S_B = (1/2)||S_h||^2_M`, using the analytic theta-Jacobian `LinearizeTheta`;
FD-gradient descent for the incident-average robustness variant; an FD-vs-analytic
battery on `LinearizeTheta` at composite points). For the identity control
`theta*(omega) = 0` exactly (theta is absent from `Upsilon`). We then integrate
`theta` out and read the one-loop effective potential along `omega`-rays `t*u`:

```
V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i( H_joint(t*u, theta*) )
```

over the POSITIVE eigenvalues of the joint `(omega, theta)` Hessian at the
composite point. `H_joint` is a forward-difference FD Hessian (Phase442 machinery,
forward stencil for cost); eigenvalues via a Jacobi sweep (the physics-study
convention). Zero/negative-mode counts are recorded honestly; the IR convention
(exact zero modes excluded at `zeroTol = max(absFloor, relTol*maxAbsEig)`,
Phase435-style continuity check) is recorded.

## Setup

`CreateUniform4D(1)` (`V=16, E=65, F=110, C=24`, so `nOmega = 195`, `nTheta = 48`,
joint DOF `243`), `su(2)` with the positive-definite trace pairing.
`Upsilon = S_h - T` with `T = TrivialTorsion = 0`, so
`S_B = (1/2)||S_h(omega, theta)||^2`. Members: identity control `{id0, none,
trivial}`; the Einsteinian set `sd2-id0` at `c in {0, 0.5, 1}` and `asd2-id0/c0.5`
(independent-theta). Two random `omega`-ray seeds (seed-stability). Interior-minimum
search: linear grid over `t in (0, t_max]` plus verified-descent (golden-section)
refinement; large-`t` behaviour via octave log-slopes `s(t) = [V(2t)-V(t)]/log 2`
(Phase437: the DECISIVE evidence is the octave slopes, not the ill-conditioned CW
polynomial-fit signs) and the recorded `{1, t^2, t^2 log t, t^4, t^4 log t}` fit.

## Control discipline

The identity-equivalent member has a pure degree-4 objective (theta absent,
`theta*(omega) = 0` exactly), so its `V_eff` reproduces the known no-saturation
structure: log-runaway, **no interior minimum**. This anchors that any saturation
seen for Einsteinian members is caused by the degree-lift, not by the machinery.

## Results (filled from the committed run)

<!-- RESULTS_PLACEHOLDER -->

## MANDATORY FRAMING (physicist; carried verbatim in intent)

**Any scale found is a WORKBENCH-RELATIVE CANDIDATE ONLY** -- `su(2)` toy algebra on
the reduced Spin(4) slice, lattice units, one-loop; **NO GeV/pole/VEV promotion**
(`scaleIsWorkbenchRelativeCandidateOnly = true`, `noGevPromotion = true`). A
no-saturation result is a legitimate frontier-sharpening outcome. The phase PASSES
on internal consistency (precursor Phase442 + the variational theta-stationarity
solve + control discipline + honesty batteries) **regardless of how the saturation
verdict falls** -- it reports what IS.

## Fail-closed

Target-blind construction (`targetBlindConstructionHash` recorded); reduced spin-4
slice only (the six verbatim recorded-boundary keys: `definition81Scope =
reduced-spin4-slice`, `ambientSevenSevenRealized = internalGaugeContentRealized =
weldRealized = false`, `canFillPhase201WzContract = canFillPhase256Contract =
false`). No scale/pole/GeV lineage; no Phase201 or Phase256 contract field is
filled; `sourceContractApplicationAllowed = phase201TemplateMutated = false`;
`acceptedContractFieldCount = 0`. Nothing is promoted either way.

Precursor: Phase442 (`jointOmegaThetaHessianDegreeProbePassed &&
einsteinianJointHessianDegreeExceedsTwo && isolationBatteryPassed`).

## Run

```bash
dotnet run -c Release --project studies/phase443_joint_effective_potential_saturation_probe_001/Phase443JointEffectivePotentialSaturationProbe.csproj
```
