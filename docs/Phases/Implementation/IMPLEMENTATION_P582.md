# Phase582: relative two-connection transport observable

Executed successfully on the first frozen v1 Release run, 2026-09-08, under
Amendment A44. Terminal:
`local-relative-observable-controls-pass-registered-bridge-open`.

## Result

The Phase581 follow-up has a constructive answer at the declared compact-link
level. Let U_A and U_B be transports on the same edge, with a common endpoint
gauge law. Their ratio R=U_A U_B^-1 transforms by conjugation. The observable
q=(3-Tr_Ad R)/4 is locally gauge-invariant and independent of SU(2) lift signs.
This is an algebraic statement for all endpoint transformations. The numerical
controls test its implementation, not an extrapolation from a rotation menu.

The primary GU draft's Eq. 12.6 motivates this through its two-connection
difference T=A-B. The discrete construction is our derivation, not something
claimed to be explicitly supplied by the draft. It is not yet an observable
of the registered action. The scope is the common compact gauge transformation
of the pair, not full inhomogeneous GU symmetry or observer-space covariance.

## Executed controls

| Test | Result |
| --- | --- |
| Local invariant error | 2.22e-16 |
| Reference-transport covariance error | 2.78e-16 |
| Independent complex-matrix observable error | 4.44e-16 |
| SU(2) lift-sign error | exactly zero |
| Fundamental-trace lift decoy | changes by 1.5463; rejected |
| A=B pure-gauge relative observable | exactly zero |
| Bare-link pure-gauge decoy | reaches 0.22695 |
| Transform-U-only / stale-V decoy | changes by 0.38011 |
| Constant-profile refinement ratios | 3.9917, 3.9979, 3.9995 |
| Smooth-profile refinement ratios | 3.9899, 3.9975, 3.9994 |
| Conditional Haar moment error | 3.33e-15 |

In t_a=-i sigma_a/2 normalization, 4q/a^2 tends to the directional coefficient
norm |A_mu-B_mu|^2. The frozen target is 1.73. At spacing 0.025, the constant
estimate is 1.7298340 and the smooth estimate 1.7298178; doubling the smooth
midpoint subdivisions changes the latter by 2.98e-9. This is one-edge
classical consistency, not a quantum or full-action continuum-limit theorem.
The metric contraction and physical scalar interpretation remain to be derived.

The conditional Haar model has q-density (2/pi)*sqrt(q/(1-q)), with moments
1,3/4,5/8. It enhances large q without any interaction. Neither a flat dq
measure nor a displaced histogram peak may be used as a breaking certificate.
This control does not replace the registered noncompact coefficient measure.

## What the result changes

There is now a concrete locally invariant candidate to investigate, rather
than another fixed-ray search. It also exposes the importance of the second
connection and of center descent. It does not fill the missing registered
variable map, reproduce the intended Shiab action, establish its symmetry,
identify an electroweak operator, or produce a mass.

The next scientific task is to derive the two-connection action/variable
dictionary and compare it with the registered curvature-only, trivial-torsion
restriction. Track the reference connection and epsilon endpoints explicitly;
do not assume that omega is already the torsion difference. A consistent
discretization can be derived internally from specified continuum content;
requiring the original author to prescribe every lattice choice is unnecessary.
What is necessary is the demonstrated bridge, including the interacting
measure. Only then should connected correlators and dimensionless pole ratios
be considered. A physical unit anchor remains a separate subsequent issue.

All earlier results and gates are preserved. In particular Phase559/560's
registered bridge remains incomplete, Phase561 stays closed, external review
is pending, and no action or production authorization changes. This local
mathematical construction is neither a full GU repair nor a physical prediction.

## Validation and lineage

The targeted Release run passed with no revised scientific contract. The
generator, traversal, nine source scanners, Phase101 mirror, Phase202 checklist,
and exact-byte integrity verifier include Phase582. Integrated validation uses
`./scripts/run_boson_phases_incremental.sh --incremental`; its timestamped report
records the executed/skipped steps and exit status. The new checklist state
required by the verifier is 362 passed / 3 standing physical-completion failures.
No promotion-relevant full pass or commit is implied by the targeted result.

Program SHA256: `73e56b4cd8b52ad7009e8e2c851642412b4f32d857e1ff90a63736a7a9791266`.
Contract SHA256: `a7520956d9c08ed8c3984bfb8f1787935649a026f447057772e3fe6473ea84ac`.
Full/summary SHA256: `946de50d95c96bc9bb41af89adaa8168e4023bc4e83fdb7ccd90e0da94b1cc27`.

Derivation and limitations: [study](../../../studies/phase582_relative_transport_observable_control_001/STUDY.md).
Sources: [reference note](../../Reference/ExperimentReferences/RELATIVE-TRANSPORT-OBSERVABLE-20260908.md).
