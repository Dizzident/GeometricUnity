# Implementation P441 - Toy-Branch Family Universality Sweep

Phase441 extends the scale-gap no-go to the ENTIRE realizable toy branch
family - the 36-member Cartesian product of Shiab {identity, first-order,
metric-scaled(lambda=2)} x Torsion {trivial, augmented, local-algebraic}
x A0 {0, lambda_8 amp 0.5, lambda_8 amp 1.0, mixed lambda_8+lambda_4} on
the su(3) 2x2 mesh.

## Universality Results (all measured, not assumed)

- Upsilon is exactly degree <= 2 in omega for every member (max third
  field-difference 1.5e-15); the Hessian decomposition H(t) = H0 + t H1 +
  t^2 H2 is exact for every member (max third t-difference 2.8e-14) - THE
  NO-SATURATION THEOREM EXTENDS TO THE ENTIRE TOY FAMILY.
- Structural insight (verified): along single-direction rank-1 rays,
  [u,u] = 0 makes Upsilon affine in t for every member, and A0 cancels
  from J2 = J(u) - J(0), so the growing-mode operator is A0-independent.
- Growing-mode counts are invariant family-wide (64 lambda_8-type / 96
  lambda_4-type); only mass VALUES rescale (metric-scale by lambda^2;
  local-algebraic by a distinct J2). The Phase440 balance therefore holds
  for every member: net derived slope 2*64 - 768 = -640 < 0 - THE RUNAWAY
  VERDICT IS UNIVERSAL.
- Inhomogeneity ledger: trivial torsion exactly 0; augmented torsion
  nonzero at A0 != 0 (the -d_A0(A0) term: 2.449/4.899/6.928);
  local-algebraic 0 even at constant nonzero A0 ([A0,A0]=0) - a genuine
  recorded distinction.
- The draft's CANONICAL physical Shiab is NOT REALIZABLE on the toy
  (dimX >= 4 and the Cl(7,7)/128 spinor-realized invariant basis are both
  required and both absent; MetricScaledShiabOperator.cs 15-18, draft v29
  Section 32.2).

## Terminal Frontier Statement

`scaleGapRequiresDimFourSpinorShiabOrSourceAnchor=True`: no completion
realizable in the toy can produce a dynamical scale at one loop; the
requirement is exactly the dimX >= 4 spinor-realized Shiab structure (the
physical VO-6/VO-7 completion) or a source anchor. Control member
reproduces Phase436 exactly (counts 64/96; root-direction odd term
0.153). Toy-family-only, polarization-convention, and no-promotion
boundaries recorded; no contract fill. Runtime ~21 s.
