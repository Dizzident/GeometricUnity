# Phase603: homogeneous invariant stationary family

Prospective Amendment A55 audit. This proof, program, polynomial helper,
project, complete fixture menu and exact input closure must be frozen and
independently reviewed before the first scientific run. Build-only checks
are permitted beforehand. No output is used to choose a coefficient,
tolerance, fixture or terminal. All equalities below are exact predictions.

## Declared action and domain

Use the bound primary draft Eqs9.3/9.4/9.7, the already declared CCA
interpretation, flat fixed metric/reference, epsilon identity, constant
tensors and normalized constant density. This is the first action, not the
separate incompletely normalized squared-residual action of Eq9.11.
Its constant connection restriction is

    I(S) = gamma B(S,K(S wedge S))/3 + kappa B(S,S)/2.

Here B is minus real normalized matrix trace, -ReTr/128, contracted with
the signed exterior-form metric. It is a nondegenerate indefinite real
pairing on the full H-anti-Hermitian connection carrier. It is not a
selected positive source norm. All quantities are constant, so exterior
derivative terms vanish on these inputs; no nonconstant operator terms
are dropped in a claim about the general action. Constant output gradients
give the full local connection Euler derivative, also valid under periodic
integration or compatible compact-support variations at fixed geometry.
Metric derivatives, boundary laws and physical time are not tested.

Set signature(7,7), Omega=gamma0...gamma13, Omega squared=1, h=-1,+1,
P=1+h Omega, Gamma1=sum theta_i gamma_i and
Gamma2=sum_(i<j)theta_ij gamma_i gamma_j. Fix

    Phi1=P Gamma1, Phi2=(c-i h Omega)Gamma2.

c remains a formal real variable throughout both Hodge legs of K and both
reverse legs of its true trace adjoint. The inner A bracket is i times
the matrix anticommutator, not the anticommutator without i. The outer
star and the second-leg factor -1/2 are retained. The immutable600 helper
implements this entire typed chain and two independently arranged reverse
chains. The program compares both reverse chains on every requested input.

## Full symbolic gradient before any invariant restriction

Write S=a Gamma1+b Omega Gamma1 and r=a-hb, s=a+hb. Omega anticommutes
with odd blades and commutes with even blades. Consequently the cross
terms in S wedge S cancel and

    Q=2(a squared-b squared)Gamma2=2rs Gamma2,
    K Gamma2=156P Gamma1, KQ=312rsP Gamma1.

For the full adjoint, the constant part of K-adjoint(theta_i gamma_i)
is -2 theta_ab Gamma_ab for every pair not containing i and zero on pairs
containing i. Each pair occurs for twelve i, giving -24 Gamma2 after
summing i. Its c part is -2ihc theta_ab Omega Gamma_ab for every pair,
including those containing i. Summing fourteen i gives -28ihc Omega Gamma2.
The Omega Gamma1 input reverses the first coefficient by -h and the
second by -h as well. Thus

    K-adjoint S = -24r Gamma2 -28ihcr Omega Gamma2.

The full derivative DQ_S[V]=S wedge V+V wedge S is not half this expression.
Its adjoint satisfies DQ_S-adjoint Gamma2=-26S and
DQ_S-adjoint(Omega Gamma2)=0. For the latter, the only surviving form
contractions share an index, and [gamma_i,Omega Gamma_ij]=0 and
[Omega gamma_i,Omega Gamma_ij]=0. This removes a NONZERO intermediate
c-adjoint term, not an assumed absence of the second source leg. Hence

    DQ_S-adjoint K-adjoint S=624rS,
    G=gamma(KQ+DQ_S-adjoint K-adjoint S)/3+kappa S
     =104gamma rsP Gamma1+(208gamma r+kappa)S.

The executable constructs Q, KQ, K-adjoint S, DQ-adjoint K-adjoint S and G
as complete Clifford/form-valued polynomials. Equality with these formulas
is checked on every coefficient before projection. It also checks the
H-anti real form and form degrees. No anticipated gradient replaces the
direct source-chain output. All five variables a,b,c,gamma,kappa are formal
and all coefficients use arbitrary-precision rational complex arithmetic.

## Independent action derivatives and completeness of invariant closure

The signed trace Gram in the (Gamma1,Omega Gamma1) basis is diag(-14,+14).
It has determinant -196; the two invariant directions are not null even
though their chiral sums are. Therefore

    B(S,S)=-14rs, B(S,KQ)=-4368r squared s,
    I=-1456gamma r squared s-7kappa rs.

The program independently constructs the scalar action from forward K and
literal Q, then differentiates its polynomial coefficients. Its two first
derivatives must equal B(Gamma1,G) and B(Omega Gamma1,G). These are
covectors; the raised gradient requires the inverse signed Gram, not an
ordinary Euclidean identification. The r,s Gram is [[0,-7],[-7,0]], also
nondegenerate. The raised gradient coordinates are

    g_r=r(208gamma r+kappa), g_s=s(416gamma r+kappa).

There is a separate general closure proof: all pointwise contractions used
here are Spin-equivariant at fixed metric and the tensors are invariant.
The full gradient of a constant invariant input is therefore invariant.
The exact-bound595 proof establishes that the FULL real invariant one-form
space in u(H), not merely a chosen low-grade subspace, has dimension two
and is spanned by Gamma1 and Omega Gamma1. Nondegeneracy of its Gram then
makes vanishing invariant covectors sufficient for full stationarity.
This is an independent written proof supporting the direct full-output
test, not an additional executed representation census. Complexification
and finite sign-character upper bounds are justified in595. No claim is
made that arbitrary real Spin transformations preserve a torus lattice.

## Actual closed invariant Hessian

For each of the two basis directions V, construct the full tensor

    H_S V=gamma[ K(DQ_S V)+DQ_V-adjoint K-adjoint S
                  +DQ_S-adjoint K-adjoint V ]/3+kappa V.

Compare all its coefficients with the derivative of the actual symbolic
G and with the independently declared formula. Only then recover the
two-by-two raised matrix using the signed Gram, and reconstruct both FULL
tensor columns from it. Independently differentiate the scalar action
twice and compare all four lowered Hessian entries. This tests closure,
the adjoint, and actual action reciprocity separately.

In r,s coordinates, obtained using an explicit basis transformation,

    J=[[416gamma r+kappa,0],[416gamma s,416gamma r+kappa]].

Its characteristic polynomial is (t-416gamma r-kappa) squared. Subtracting
its scalar diagonal gives a nonzero formal shear whose square is zero.
The ordinary raised matrix is generally not symmetric, whereas its
Gram-lowered matrix is symmetric. Confusing the two is a rejected decoy.
These eigenvalues refer ONLY to this genuinely closed constant invariant
connection carrier; no full nonconstant or metric-plus-connection spectrum
or physical mass is inferred.

## Complete real coupling degeneracies and exact branch controls

Solving r(208gamma r+kappa)=0 and s(416gamma r+kappa)=0 gives:

- gamma and kappa both nonzero: origin, or r=-kappa/(208gamma), s=0.
- gamma nonzero, kappa=0: the whole r=0 line, including the origin.
- gamma=0, kappa nonzero: only the origin.
- gamma=kappa=0: every constant invariant S.

For the first case the nonzero point is

    S*=-kappa(1-h Omega)Gamma1/(416gamma).

There is no division by a possibly zero coupling in the executable. The
opposite-chiral branch is tested with a=-t,b=ht,kappa=416gamma t, where
the surviving formal variable a denotes the fresh t under simultaneous
polynomial substitution. This parametrizes every nonzero branch point
when gamma*t is nonzero; degeneracies are tested separately. The chiral
line uses a=t,b=ht,kappa=0. Six rows per h separate the origin from the
nonzero line/point where needed. Symbolic nonzero means a polynomial is
not identically zero; the stated nonzero-coupling hypotheses are necessary.

At the origin J=kappa I. At S*, J=-kappa I. Its lowered Hessian in the
original a,b basis is diag(14kappa,-14kappa), so for real nonzero kappa
the first action has a saddle on this real constant invariant carrier.
This rules out calling it a local minimum on that unconstrained carrier,
not a dynamical instability theorem without a time/constraint prescription.
On the massless chiral line J=[[0,0],[832gamma t,0]], rank one and a
size-two nilpotent block for gamma*t nonzero; at its origin J=0.

All stationary rows have I=0. At S*, Q=0 and B(S*,S*)=0, but neither the
mass gradient nor cubic adjoint gradient vanishes separately; they cancel.
Three negative controls per h explicitly show that:

- S=t(1-h Omega)Gamma1 at kappa0 is flat/null but has
  G=416gamma t squared (1-h Omega)Gamma1, generally nonzero.
- The chiral massless family with kappa restored has G=kappa S.
- At S* the curvature-only expression KQ+kappa S equals nonzero kappa S,
  while the actual full first-action gradient vanishes.

No global first-action prefactor convention repairs the last shortcut on
Q=0. It says nothing by itself about stationarity of the separately defined
second action. The ratio kappa/gamma is an existing symbolic coupling ratio,
not a selected physical scale, vacuum amplitude or unit calibration.

## Frozen exact menu, resources and failure precedence

Counts:44944 independent Clifford word cases on grades0,1,2,12,13,14;
16384 Hodge-square cases;5 polynomial known answers;2 chirality rows;
16 complete symbolic identities;4 first-action derivative checks;4 full
Hessian columns;8 scalar-action Hessian bilinears;12 stationary rows;6
negative rows. Per h the exact nonzero tensor coefficient counts are
Q182,KQ56,K-adjoint S364,DQ-adjoint term56,cubic G84,mass G28.
The scalar action has six nonzero monomials. In particular no c sampling,
interpolation, root finder or floating rank tolerance appears.

Every final tensor polynomial has at most364 nonzero tensor coefficients,
and scalar/tensor total polynomial degree is at most four. Clifford/form
support stays within the declared constant grades; transient source
products are finite combinations of at most91 form components and28
Phi1 components. Products are grouped by form and pruned exactly. A
conservative prospective estimate is10 CPU seconds and128MiB, estimate
ceilings60 CPU seconds/256MiB, not measured results or enforced timeouts.

Sixteen unique bindings cover own program, polynomial helper, project,
proof; primary source;600 summary, contract, program and all three compiled
helpers;595 summary, contract and complete classification proof; the
726-file core manifest; and Directory.Build.props. Verify every live core
path/hash and sorted tree digest excluding bin/obj. Complete fixture JSON,
terminal precedence, passed upstream summaries and fourteen exact authority
names must match. Full and summary outputs are deterministic and identical.

Precedence: invalid-or-drifted-input; known-answer-control-failed;
full-symbolic-gradient-control-failed; invariant-hessian-control-failed;
stationary-family-control-failed;
`homogeneous-invariant-stationary-family-controls-pass-scale-unselected`.
Only the last terminal passes. Preserve any failed frozen output unchanged;
scientific repairs require a versioned pack and renewed independent review.
All fourteen authority flags stay false, externalReviewPending=true,
promotedPhysicalMassClaimCount=0. No sampling, core or historical edits,
source/norm/measure selection, physical vacuum or scale claim is authorized.
