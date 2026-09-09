# Phase606: general-companion stationary Fourier closure

Prospective Amendment A57 audit. Freeze ALL new scientific files, fixtures,
coefficients/counts/tolerances/resources/precedence and unique exact input
bindings before the first scientific execution. Independent reviewer and
MAIN must read the whole pack; only MAIN authorizes the first run. Release
build-only checks are allowed beforehand. Every new text file has exactly
one final newline before freeze. No output selects a fixture or threshold.

## Declared domain, source and affine normalization

Use the bound primary draft's declared CCA interpretation, full outer Hodge
star, both source legs, inner i-anticommutator and second factor-1/2. Hold
the flat(7,7) metric, reference, epsilon identity, tensors and normalized
periodic density fixed. B=-ReTr/128 with signed form metric is nondegenerate
and indefinite, not a chosen source9.11 positive norm. Gamma1=sum theta_i
gamma_i, Gamma2=sum_(i<j)theta_ij Gammaij, Omega squared=1, h=-1,+1,
P=1+hOmega, Pbar=1-hOmega, Phi1=P Gamma1, Phi2=(c-ihOmega)Gamma2.

The first action and its actual connection Hessian at the bound603 branch are

    I(T)=B(T,KdT)/2+gamma B(T,K(T wedge T))/3+kappa B(T,T)/2,
    S*=-kappa Pbar Gamma1/(416gamma), gamma nonzero,
    H_S* V=H0 V+(gamma/3)(K DQ_S* V
             +DQ_V-adjoint K-adjoint S*+DQ_S*-adjoint K-adjoint V)+kappa V,
    H0=(Kd+d-adjoint K-adjoint)/2,
    DQ_S V=S wedge V+V wedge S.

All three potential legs are linear in S*, so gamma*S*=-kappa Pbar
Gamma1/416 removes gamma algebraically. The executable's gamma1 normalization
is an exact computational control, not a fit or selected coupling. Compute
the three legs A,Bleg,C at Sunit=Pbar Gamma1, scale their sum by-kappa/1248
and add mass kappa V. K, its true adjoint, H0 and the potential are affine
in c by construction. There is no expansion truncation: all c0/c1 slots
are the entire operator, not a weak-c approximation.

The unchanged600 arithmetic/Fourier/adjoint helpers,601 matrix helper and605
SourceHessian class are compiled with complete exact bindings. SourceHessian
does not know the expected carrier or matrix; it computes full tensors and
compares literal/simplified reverse chains. New GeneralClosure supplies an
independent raw-leg table, an independent combined-column table and a
forward-only original-action oracle. No frozen upstream byte is changed.

This is a fixed-geometry connection test. The flat control is not thereby
shown to belong to the source-induced upstairs metric family or solve its
downstairs metric Euler equations. No physical time, boundary prescription,
vacuum, field extraction, norm, unit scale or physical spectrum is selected.

## Basis, reality and full signed Gram

For nonzero periodic integer n retain605's modes, with no n in amplitudes:

    u=theta0 gamma2 cos(nx0), b=theta2 gamma0 cos(nx0),
    e=sum_(j!=0,2)theta_j Gamma2j sin(nx0),
    e0=theta0 Gamma20 sin(nx0), q=theta2 Omega sin(nx0),
    Z=sum_(j!=0,2)theta_j Gamma0j gamma2 cos(nx0),
    z=iZ, oz=Omega z, j=i theta2 I sin(nx0), T=iOmega e,T0=iOmega e0.

The order is(u,b,Omega u,Omega b,z,oz,e,e0,q,j,T,T0). All coefficients are
in the real H-anti form; the i phases on grades0,3,11,12 are essential.
The diagonal trace Gram is

    (-1/2,-1/2,1/2,1/2,-6,6,6,1/2,-1/2,1/2,-6,-1/2),

with determinant81/16 and inertia6+/6-. Form/blade orthogonality makes all
cross terms zero. Write a=u-b, oa=Omega a, x=u+b, E=e+e0,U=T+T0.
There are six odd and six even directions. The earlier10D proposal fails
because H0z contains T and T0; no potential on odd z can cancel even leakage.

## Complete three-leg oracle at the unit background

The following tables fix every c slot before execution. Every entry denotes
the complete tensor, not a projection or selected Clifford grade. z already
contains i. P and Pbar act on the Clifford factor on the left.

| Input | A0 | B0 | C0 |
| --- | --- | --- | --- |
| u | -48Pu | -96b | -48Pbar u |
| b | -48Pb | -96u | -48Pbar b |
| Omega u | -48P Omega u | -96Omega b | -48Pbar Omega u |
| Omega b | -48P Omega b | -96Omega u | -48Pbar Omega b |
| z | 432Pz | 1056z | 432Pbar z |
| oz | 432P oz | 1056oz | 432Pbar oz |
| e | 8(12e0+11e) | 96(12e0+11e) | 96e0+88e+1152h q |
| e0 | 8e | 96e | 8e+96h q |
| q | -96hE | 0 | 0 |
| j | 0 | 0 | 0 |
| T | -184T-96T0 | 1056T+1152T0 | -184T-96T0 |
| T0 | -8T-96T0 | 96T | -8T-96T0 |

| Input | A1 | B1 | C1 |
| --- | --- | --- | --- |
| u | 4Pz | -112h oz | -44Pbar z |
| b | -4Pz | 112h oz | 44Pbar z |
| Omega u | 4hPz | -112h z | 44hPbar z |
| Omega b | -4hPz | 112h z | -44hPbar z |
| z | -528Pa | 1344h oa | 48Pbar a |
| oz | -528hPa | 1344h a | -48hPbar a |
| e | 0 | 112h(11T+12T0) | -1152hU |
| e0 | 0 | 112hT | -96hU |
| q | 0 | 0 | -96U |
| j | 0 | 0 | 0 |
| T | 1152hE-1152q | -112h(11e+12e0) | 0 |
| T0 | 96hE-96q | -112h e | 0 |

Here A=K DQ_Sunit, Bleg=DQ_V-adjoint K-adjoint Sunit and
C=DQ_Sunit-adjoint K-adjoint V. These are separately tested before summing.
Some useful complete algebra behind the tables follows; no nonzero adjoint
term is silently discarded.

K-adjoint Sunit=-48Gamma2-56ihc Omega Gamma2. For u, its 0j adjoint part
is2Gamma2j, while disjoint pairs carry-2hOmega Gammaij Gamma02 at c0 and
-2iGammaij Gamma02 at c1. There is also c1 coefficient2iI on02. The c0
disjoint terms commute with the contracted background coefficient; the c1
ones give eleven-4Pbar z_j contractions. The source curvature's already
scaled2Gamma2j pieces each yield-4Pu. The full source Omega-curvature
contribution cancels between legs. This reproduces605 without its former
prefreeze proof omission. For companions, DQ_Sunit(Omega v)=h DQ_Sunit v,
K-adjoint(Omega v)=-h K-adjoint v on odd v, and Bleg(Omega v)=Omega Bleg(v).

For z, let J be the twelve axes other than0,2 and A_j=Gamma0j gamma2.
DQ_Sunit z has disjoint-J coefficients-4iGamma0ab2, 0j coefficients
2ihOmega Gamma2j and2j coefficients-2ihOmega Gamma0j. The complete c0
adjoint of z has J-pair coefficients20iGamma0ab2, 0j coefficients
-2ihOmega Gamma2j,2j coefficients2ihOmega Gamma0j and02 coefficient-24iI.
Forward contraction has only Pz support; reverse background contraction
has only Pbar z support. In B(z,A),66 J-pairs contribute-80 each and the
two sets of twelve shared pairs contribute+4 each; the cosine square1/2
gives-2592, fixing A=432Pz and C=432Pbar z. Each remaining index supplies
96z_j to Bleg, for1056z. In the c slot the contracted forward scalar is
528iGamma02; Bleg has twelve112h contributions at each of outputs0,2;
K_c-adjoint z has02=24hOmega and disjoint-24hOmega Gammaab Gamma02.
The latter vanish on repeated-index contraction, leaving C1=48Pbar a.

For E_i=theta_i Gamma2i sin, i!=2, write T_i=iOmega E_i and sum over13
indices. The per-index old-even c1 legs are(0,112h(U-T_i),-96hU).
The new-even c0 legs are A=C=-96T_i-8(U-T_i), Bleg=96(U-T_i).
Their c1 legs are(96hE-96q,-112h(E-E_i),0). Sum twelve i in J for e,T
or take i0 for e0,T0. The key adjoint of T_i is first-leg only:
(K-adjoint T_i)_ai=-2ihP gamma_a Gamma2i for a!=2,i. Its contracted outer
commutator is zero, so K_c-adjoint T_i=0. These formulas explain the
separate nonzero legs, not merely their combined cancellations. All j
potential legs vanish because it is central and K-adjoint j=0.

## Full24 source columns

Write H=H^(0)+c H^(1). Derivative and potential use actual source fields;
the following independent combined oracle is NOT generated by adding the
raw-leg table in the implementation.

| Input | H^(0) | H^(1) |
| --- | --- | --- |
| u | ne+kappa(14u+b)/13 | nj+kappa(5z+8h oz)/156 |
| b | ne+kappa(u+14b)/13 | -nj-kappa(5z+8h oz)/156 |
| Omega u | -hne+kappa(14Omega u+Omega b)/13 | -hnj+kappa(8hz+5oz)/156 |
| Omega b | -hne+kappa(Omega u+14Omega b)/13 | hnj-kappa(8hz+5oz)/156 |
| z | -12nj-2hn(T+6T0)-7kappa z/13 | 12nE+kappa(5a-8h oa)/13 |
| oz | 12hnj+2n(T+6T0)-7kappa oz/13 | -12hnE+kappa(-8h a+5oa)/13 |
| e | -12nPx+kappa(e/78-14e0/13-12h q/13) | -12n(z+h oz)+hkappa(-5T-12T0)/78 |
| e0 | kappa(e0-7e/78-hq/13) | -n(z+h oz)+hkappa(-T+6T0)/78 |
| q | kappa(q+hE/13) | kappa U/13 |
| j | n(z+h oz)+kappa j | -n(a+h oa) |
| T | -2hn(z+h oz)+kappa(35T-60T0)/78 | hkappa(5e+12e0)/78+12kappa q/13 |
| T0 | -hn(z+h oz)+kappa(-5T+90T0)/78 | hkappa(e-6e0)/78+kappa q/13 |

For example Kdz=-2hnT-24hnT0 and d-adjoint K-adjoint z=-2hnT-24nj,
giving the new c0 differential column. In the c slot K_c dz=24n(E-hq)
and d-adjoint K_c-adjoint z=24hnq. For T_i, K_c dT_i vanishes by a
shared-index anticommutator and K_c-adjoint T_i=0. The c0 forward/reverse
pieces give H0T=-2hnPz and H0T0=-hnPz. These identities reproduce601's
H0 squared u=-12n squared Px-11cn squared Pz+c squared n squared P(b-u).

All full slots are checked for form degree, H-anti reality, exact combined
coefficient and zero tensor residual after Gram reconstruction. Affine
combination of these FULL slots is exact for all c,kappa, not interpolation.

## Independent original-action bilinears

The scalar coefficient of st in I(S*+sV+tW) is computed independently
through forward K only. Its quadratic coefficient is
[B(V,KdW)+B(W,KdV)]/2. Its cubic coefficient contains six ordered placements
of Sunit,V,W in B(T,K(T wedge T)), multiplied by-kappa/1248; the mass term
is kappa B(V,W). Group each exchanged wedge-word pair BEFORE the real
H-anti trace pairing, since individual words need not have real trace.
No adjoint, expected Hessian, inverse Gram or source column is used by
this scalar oracle. Both c slots and both derivative/potential coefficients
are checked separately; exact affine substitution then checks every row.
Gram-lowered Hessians must be symmetric, not necessarily raised matrices.

## Symbolic chiral flag and characteristic factorization for all c

Define Xplus=Px,Aplus=Pa,Zplus=Pz and Xminus=Pbar x/2,
Aminus=Pbar a/2,Zminus=Pbar z/2. Use the explicit invertible basis order
(odd-plus3,even6,odd-minus3). The potential preserves all three diagonal
blocks. The derivative kills odd-plus, maps even into odd-plus and
odd-minus into even. The program transforms ACTUAL formal-c source matrices
and checks the forbidden blocks vanish identically. Thus n appears only
above the block diagonal; characteristic factors cannot depend on n.

Both X directions have eigenalpha=15kappa/13. The plus(A,Z) block is
[[kappa,-3ckappa/13],[ckappa/6,-7kappa/13]], and the minus block is
[[kappa,ckappa],[-ckappa/26,-7kappa/13]]. They have the same

    Qodd=lambda squared-6kappa lambda/13+kappa squared(c squared-14)/26.

In the even block use(E=e+e0,q,U=T+T0), central j, and
(F=e-12e0,V=T-12T0). The trace3 potential is

    (kappa/13)[[-1,h,hc],[-13h,13,13c],[-hc,c,5]].

Its second row is13h times its first. Direct trace/minor calculation gives
characteristic lambda Qeven, where
Qeven=lambda squared-17kappa lambda/13+12kappa squared(5-c squared)/169.
Central j has eigenkappa. The hook2 potential is

    (kappa/78)[[85,-7hc],[7hc,95]],

with Qhook=lambda squared-30kappa lambda/13+
kappa squared(8075+49c squared)/6084. These are diagonal QUOTIENT blocks
of a full invariant flag, not independently closed full-H even subspaces.
Their factors nevertheless give the exact full characteristic

    (lambda-alpha)^2 Qodd^2 (lambda-kappa)lambda Qeven Qhook.

The executable compares all formal-c entries of the actual potential in
this factor basis, then independently computes its symbolic characteristic.
Exact Faddeev-LeVerrier uses B0=I, ck=-Tr(A B_(k-1))/k,
Bk=A B_(k-1)+ck I, valid over characteristic-zero rational polynomial
rings. It is checked against a small independent permutation determinant
and a formal diagonal example. This avoids factorial12 determinants.
Homogeneity restores arbitrary kappa, while the checked derivative flag
proves arbitrary n independence. Finite controls alone are not the proof.

## Certified minimal and Jordan strata only

At kappa0, the full derivative has H0 cubed=0. In the stated half-minus
to plus basis, the actual symbolic H0 squared block is

    n squared[[-24,0,-144c],[0,-2c squared,12c],[-24c,2c,4-156c squared]].

Its determinant is192c squared(4-3c squared)n^6; the upper-left2 minor
is48c squared n^4. The program checks these polynomial identities from
full source matrices. For c nonzero, each parity block of H0 has rank3
(select e,j,T for the plus image and Xminus,Aminus,Zminus for the minus
image); for c0 each has rank2. H0 squared has rank3 except c0 or
c squared=4/3, where it has rank2. Hence the full12 Jordan types are
3J3+3J1 generically,2J3+2J2+2J1 at c squared=4/3, and2J3+6J1 at c0.
The irrational exceptional stratum is an exact determinant/minor PROOF,
not a rounded finite parameter row. Every real c has seed-u dimension3:
H0 squared u has fixed nonzero -12n squared Xplus coefficient and
B(H0u,H0u)=n squared(12+c squared)/2>0.

At c0,kappa*n nonzero, full characteristic has alpha and beta=-7kappa/13
each repeated twice, kappa repeated three times and the other five roots
simple. Alpha has the605 nonzero Schur coefficient-208n squared/(5kappa).
For beta in the present HALF-minus convention, its central-j contribution
is39n squared/(5kappa) and its(T,T0) contribution is
-1833n squared/(137kappa), totaling-3822n squared/(685kappa), nonzero.
Thus both have Jordan2. The three kappa eigenvectors are semisimple:
at c0 no derivative couples the antisymmetric odd kappa modes, and the
central-j derivative points to the distinct beta block. The full minimal is

    (lambda-alpha)^2(lambda+7kappa/13)^2(lambda-kappa)lambda
    (lambda-12kappa/13)(lambda-85kappa/78)
    (lambda-5kappa/13)(lambda-95kappa/78),

degree10, NOT the full characteristic and NOT the seed-u minimal of degree6.
The executable checks full polynomial annihilation, alpha/beta shift ranks
11,10,10, kappa shift ranks9,9,9, and actual full-source seed iterates.
The seed remains the605 closed nondegenerate cyclic6 at c0.

No nonzero-c/kappa full minimal, generic cyclic12 or full collision Jordan
claim is made. Generic perturbative reachability is not substituted for an
exact preregistered certificate. The48 such finite Fourier rows have full
closure/characteristic/action checks but explicitly no minimal assertion.

## Separate actual constant six-dimensional carrier

At n0 the six sine fields vanish; use the actual first six cosine fields
with cos0=1, Gram(-1,-1,1,1,-12,12), determinant-144. The characteristic is
(lambda-alpha)^2 Qodd^2. For real c and kappa nonzero, alpha is distinct
from Qodd because Qodd(alpha)=kappa squared(44/169+c squared/26)>0.
The full minimal is(lambda-alpha)Qodd, degree3 even at c0. If Qodd itself
has a repeated real root, its nonzero-c off-diagonal entries still give
a degree-two minimal; no diagonalizability assumption is made there.

For c nonzero the seed-u span is(u,b,w=5z+8h oz), with Gram(-1,-1,468).
In that ordered basis, det[u,Hu,H squared u] is
-kappa cubed c(44/169+c squared/26)/312, nonzero for kappa c nonzero.
Thus the seed dimension is3 and its span is nondegenerate. At c0 it is only(u,b), dimension2,
although the FULL constant6 minimal still has degree3. At kappa0 the
constant operator is zero, full minimal lambda and seed dimension1.
No twelve-mode sine Gram is specialized formally to n0.

## Diagnostic nonuniqueness, not physical instability

The exact-bound592 matching condition fixes a1,bh,d-h but leaves c free.
All c here obey that same DECLARED conditional Riemann matching. The hook
factor has roots kappa(90 plus/minus sqrt(25-49c squared))/78: c0 gives
85kappa/78,95kappa/78, while c1 gives kappa(90 plus/minus i sqrt24)/78.
The code checks the actual quotient block and its discriminant
kappa squared(25-49c squared)/1521, including the exact c0/c1 difference.
At c=plus/minus5/7 and nonzero kappa its quotient nilpotent part has rank1
and square zero. This is NOT an assertion about the full collision Jordan
structure, where alpha also occurs in the odd blocks. None of these facts
identifies physical time, poles, stability, particles or a source norm.
They reject uniqueness of THIS diagnostic spectrum from matching alone.

## Frozen menu, exact counts and resource estimates

Menu: both h, n=0,1,2, kappa=-1,0,1, c=0,+/-1,+/-5/7,+/-2. There are
42 actual constant6 rows and84 Fourier12 rows,126 total. The symbolic
affine source proof, not interpolation across this menu, covers all c.

Counts:44944 independent word cases on grades0,1,2,12,13,14, plus84630
selected new-grade word cases. The added menu includes all2730 blades of
grades3,4,10,11, each multiplied by all14 singleton generators on each
side, by Omega on each side, and by itself:31 products per blade.
This covers the new grade3/11 inputs and grade4/10 adjoint intermediates
with a bounded generator/Omega/square battery, not an exhaustive pair
census or an assertion that no other intermediate grades occur. The
general Clifford word/inversion derivation supplies the all-grade rule.
There are16384 Hodge
cases;4 arithmetic known answers;120 formal source columns;360 separate
potential legs;120 origin slots and120 affine-slot checks. The six base
carriers have sum of squared dimensions648; two slots and two action
coefficients give2592 independent original-action coefficient checks.
Six symbolic flag/factor rows and four symbolic nilpotency rows follow.
Finite rows give1260 full tensor column reconstructions,13608 Gram entries
and13608 action bilinears,126 characteristic and126 power-rank rows.
The nonzero-frequency, nonzero-kappa rank prediction11,11,11 applies to
THIS finite c menu: c squared is never5 or14, so Qeven(0) and Qodd(0)
are nonzero, Qhook(0) is positive, and the characteristic has exactly one
zero root. The zero eigenvalue is therefore simple. This finite-menu
rank assertion is not extrapolated to all real c.

The certified union has78 rows:42 constant,28 Fourier kappa0 and8 Fourier
c0/nonzero-kappa. Remaining48 have no minimal assertion. Actual source
Krylov applications total238:14 one-step constant-zero rows,28 three-step
constant-nonzero rows,28 three-step Fourier-zero-kappa rows and8 seven-step
Fourier c0/nonzero-kappa rows. Including each seed gives316 prefix rank
checks. There are8 c0 Jordan rows,84 hook quotient rows,16 nonzero hook
collision rows and8 c0/c1 diagnostic comparisons. All tolerances are zero.

Full Fourier tensors/iterates have at most112 terms:56 odd and56 even;
actual constant tensors have at most28. Supports are checked. Background
and tensors are constant, so operator application does not create new
frequencies. Exact rational arithmetic has unbounded integer precision.
The largest characteristic degree is12 and the longest actual Krylov loop
seven applications. Prospective estimate60 CPU seconds/256MiB; conservative
estimate ceilings180 CPU seconds/512MiB, not measured outcomes or runtime
timeouts. No large eigensolver, sampling, interpolation or target fit occurs.

Twenty-one unique exact bindings cover own program/helper/project/proof;
primary source;600 summary/contract/program and its three compiled helpers;
601 summary/contract and compiled matrix helper;605 summary/contract and
compiled source-Hessian helper;592 summary/contract; full core manifest;
Directory.Build.props. Verify every live726 source/project path and hash
excluding bin/obj, plus sorted tree hash. Complete fixtures, precedence,
upstream passed terminals and all fourteen exact false authority names
are mandatory. New-file newline discipline is checked before freeze.

Precedence: invalid-or-drifted-input; known-answer-control-failed;
formal-source-column-control-failed; original-action-control-failed;
symbolic-factor-control-failed; certified-stratum-control-failed;
diagnostic-comparison-control-failed;
`general-companion-fourier-closure-controls-pass-diagnostic-spectrum-nonunique`.
Only the final terminal passes. Full and summary JSON are deterministic and
identical. Preserve any failed frozen first output; repairs require a new
version and renewed complete review. All fourteen flags remain false,
externalReviewPending=true and promotedPhysicalMassClaimCount=0. No core or
historical rewrite, source/norm/measure choice, physical vacuum, time,
instability, dispersion, mass or unit-calibration claim is authorized.
