# Phase620: curved canonical CAA action descent and actual Ward variations

## Prospective status and scope

A63 allocates this deterministic local-jet audit. This document, all code,
project, complete fixture JSON, exact counts, resource bounds and lineage
must be frozen and independently reviewed before MAIN authorizes FIRST
scientific execution. A Release build is not scientific execution.
No result is claimed here.

The motivating primary is the bound April1,2021 draft, especially3.27,
3.34 and9.3-9.4. Sections3.27/3.34 support full real u(64,64), dimension16384,
including central iI and both Clifford parities. The finite fixtures below
do not restrict the operator or allowed variation domain. Uniform tensor
conjugation and the consistent plus lift are declared conventions, not
resolution of source tuple-sign or operator ambiguities.

This extends598's executed flat tied-CCA controls to actual nonzero
induced reference curvature and canonical UNTIED CAA. It does not reuse
598's flatness formula or silently change its operator. All14 authority
flags remainfalse, O4/external reviewpending, Phase561closed, physical
massclaims0. No sibling621 artifact is an input.

## Exact algebra, pairing and immutable geometry

Compile immutable600 ExactArithmetic/FourierTensor/TraceAdjoint,
607 VerticalGeometry,610 SpinGeometry and611 CaaOperator. The local helper
implements independently supplied first/outer/inner Phi tensors and exact
dual-number chain products. It never calls600's tied CCA Chain as CAA.

Signature is sevenpositive/sevennegative. Gamma blades obey the bound
word-product and H-adjoint conventions. The indefinite real bilinear
pairing is -ReTr/128 with the signed exterior metric. Pairing invariance
uses cyclic trace, not positivity or a Hilbert-space norm.

Read BOTH complete passed608 beta=-1/2 curvature arrays. Transform all
four lowered slots with610's oriented orthonormal frame and independently
transform the curvature endomorphism. Check every76832 transported
component and all2548 spin action entries. Lift the entire curvature
using610's immutable normalization, and require complete equality with
the passed610 spin-curvature arrays. Each has822 coefficients.
Ricci/Einstein identities are forecasts only; no Einstein oracle replaces
the literal full spin-curvature input.

Canonical PhiFirst=Gamma1, PhiOuter=Gamma1, PhiInner=Gamma2. With
C the coefficient commutator and A=i times the coefficient
anticommutator, the upper map is

    U(F)=[PhiFirst wedge starF]_C
         -star[PhiOuter wedge star[PhiInner wedge starF]_A]_A/2,
    K(F)=star U(F).

The last star is the degree13 inverse of star1. Retain all stages with
degrees2,12,13,14,0,1,13,1; each value and variation is checked typed.
Each occurrence is conjugated by its own literal call, even though the
two canonical Phi1 values coincide. No low-grade projection occurs.

## Consistent local connection and finite observer jets

At each point choose normal reference gauge, with local coordinate
directions identified with the oriented frame at that point:

    A0_a(p)=0, partial_a A0_b(p)=F0_ab/2.

Then partial_a A0_b-partial_b A0_a=F0_ab. This realizes the prescribed
point curvature. It does NOT assert the reference is linear on an open
set, constant curvature, a globally constant normal coframe, or a
covariantly constant curvature. Higher jets can match the actual local
reference; they are not needed for this pointwise first-action audit.

Use598's H-anti-Hermitian nilpotents

    N=gamma2+Gamma12, M=gamma2-Gamma12,
    N2=M2=0, NM=2(1+gamma1), MN=2(1-gamma1),
    [N,M]=4gamma1, MNM=4M.

Thus epsilon=(1+fN)(1+gM) has EXACT inverse
(1-gM)(1-fN) and is H-unitary for real f,g. No exponential approximation,
Fourier sampling, angle selection, or quadrature is involved.
Mode0 is f=g=0 with zero derivatives. Mode1 is the germ
f=1+x0,g=1+x1 at p: f=g=1, df=theta0,dg=theta1.
Both inverse orders and both H-unitarity orders are checked.

First derivatives are computed from these finite products. All196 ordered
ordinary Hessian entries are retained for each epsilon/point context:

    partial01 epsilon=partial10 epsilon=NM,
    partial01 inverse=partial10 inverse=MN,

with all other ordinary Hessian entries zero. Both inverse product
Hessians vanish by the four-term second product rule. Covariant Hessians
add [partial_a A0_b,epsilon] (and similarly for inverse). Their full
antisymmetrization is compared to a second implementation that separately
expands the two connection-jet commutators before collecting terms.
Only afterward compare to D0²epsilon=[F0,epsilon]. Ordinary partial
Hessians commute; covariant second derivatives do not.

Set b=epsilon^-1 D0epsilon, the difference from A0. Compute

    D0b=(D0 inverse) wedge D0epsilon+inverse D0²epsilon,
    FB=F0+D0b+b wedge b.

Never use Ad_inverse(F0) as this computation's input. Compare the actual
result with that separately constructed tensor. Mode1 must retain nonzero
D0b, b², D0²epsilon and an FB different from unrotated F0.

## Original first action and two nonzero field jets

At fixed reference, metric, tensors and density use

    T=varpi-b, S=epsilon T inverse,
    I=<T,K_e FB>+<T,K_e D_B T>/2
      +gamma<T,K_e(T wedge T)>/3+kappa<T,T>/2.

The total connection is A=A0+varpi and B=A0+b. Directly construct
T=inverse S epsilon and D0T by the exterior product rule, including its
minus sign on the final derivative of epsilon. Then varpi=b+T and
D0varpi=D0b+D0T are actual coordinate jets. Evaluate D_BT=D0T+bT+Tb.
Uniform conjugation respects every product, fixed star and real trace,
so the predicted action is the corresponding base first action I0(S).
This identity is a comparison, never a replacement for the evaluated
transformed chains or product derivatives.

Field C has S=Gamma1 and D0S=0 at p. Field K has S=theta0gamma1 and
D0S=theta02Gamma12, realized for example by partial2 S0=-Gamma12.
These are legitimate local first jets, not assertions that their
constant-value expressions solve a global field equation.

In piece order(source,kinetic-with-half,raw-cubic,mass-with-half):

    C action=(60,0,-4368,-7),
    C scaling derivative=(60,0,-13104,-14),
    K action=(0,1,0,-1/2),
    K scaling derivative=(0,2,0,-1).

The source vector has coefficients-15/4 on the nine traceless vertical
axes and-21/4 on the otherfive, hence Pair(Gamma1,source)=60.
Q(Gamma1)=2Gamma2 and literal KQ=312Gamma1, while
Pair(Gamma1,Gamma1)=-14, proving the C cubic/mass anchors.
For K, literal K(theta02Gamma12)=-2theta0gamma1,
Pair(S,S)=-1, and Q(S)=0. These prove all K anchors.
Every complete base action is nonzero for gamma1,2 and kappa0,1.

## Actual variations, nonzero epsilon-only control and decoys

Use first-order dual numbers, differentiating epsilon, inverse, both
their first covariant derivatives, b, D0b, FB, T, D0T, D_BT, Q, all
three independent Phi occurrences, every chain stage and each pairing.
Check differentiated inverse identities at value and first-jet order.
The normal-reference second-derivative construction acts independently
on epsilon's value and variation. It does not substitute a desired force.

For right deltaepsilon=epsilon eta,

    delta inverse=-eta inverse,
    deltaD0epsilon=(D0epsilon)eta+epsilon D0eta.

The three directions are fixed epsilon with deltaS=S,D0deltaS=D0S;
correct plus Ward delta varpi=D_Aeta; and epsilon-only delta varpi=0.
For the lasttwo use eta=Gamma01,D0eta=theta0gamma1.
Compute D0(D_Aeta) from

    [F0,eta]+[D0varpi,eta]-[varpi,D0eta]_graded.

The graded last bracket is the sum of the two one-form products.
This retains precisely the curved second jet D0²eta=[F0,eta].

Separately reconstruct the pullback field/derivative jets and compare
them with deltaS=Ad_epsilon(delta varpi-D_Aeta) and its actual derivative.
Then evaluate the independent BASE first-variation formula

    Pair(V,KF0)
    +(Pair(V,KD0S)+Pair(S,KD0V))/2
    +gamma(Pair(V,KQ)+Pair(S,K(VS+SV)))/3
    +kappa Pair(S,V).

The plus Ward direction has zero full V and D0V, hence every action-piece
derivative vanishes. This is not a declaration of printed9.6/9.7.

Identity-epsilon K provides a nonzero epsilon-only anchor:
deltaS=-S+2theta0gamma0,
D0deltaS=-[F0,Gamma01]+2theta02Gamma02.
Its derivative pieces are(21/2,-21/16,0,1), total147/16+kappa.

Three independent K-field decoys at identity epsilon,kappa0 are:

1. Wrong minus lift with eta(p)=0,D0eta=S:
   deltaS=-2S,D0deltaS=0; pieces(0,-2,0,2), total-2.
2. Correct lift with eta=Gamma01,D0eta=0 but frozen FB variation:
   pieces(-5/8,0,0,0), total-5/8.
3. Correct lift, same eta, but freezing ALL three Phi variations:
   pieces(-79/8,0,0,0), total-79/8.

For the lasttwo, retained F01,01=-1/32 and Kfirst(F0)_0=-1/4 give
K([F0,Gamma01])_(theta0,gamma1)=2(-1/4)+4(-1/32)=-5/8.
The corresponding [K(F0),Gamma01] coefficient is-21/2.
Pair(theta0gamma1,theta0gamma1)=-1 fixes the two residual signs.
The scalar inner variation vanishes in this pairing; grade5 terms are
retained, not discarded from tensors. In the kinetic decoy both
K([D0S,eta]) and [K(D0S),eta] have no theta0gamma1 coefficient.
Q=0 and mass invariance remove the remaining pieces. The epsilon-only
kinetic derivative has pairings-2 and-5/8, dividedby2: -21/16.
These forecasts received separate reviewer and MAIN hand checks before
implementation, not fits to scientific output.

## Exact menus, counts and prospective resources

Two points times two epsilon jets times two fields give8 contexts.
Each has four gamma/kappa combinations,32 action rows. Three full
variation contexts give24 product-rule evaluations and32 rows each for
fixed-direction,Ward,epsilon-only. Three inputs per context give24
complete operator comparisons. Three decoy evaluations per point, each
at two gamma values, give12 retained decoy rows.

There are38 calls to Evaluate:8 baselines+24 variations+6 decoys.
Each has3 chains with8 stages, exactly912 dual stage visits.
NormalSquare has106 underlying scalar calls, each visiting91planes:
8 Coordinates calls,76 Evaluate value/delta calls,8 explicit epsilon/
inverse checks,8 eta calls in main contexts and6 decoy eta calls,
hence9646 connection-jet entries. Empty variations are still visited.
The complete36-field count map in Program/contract is prospective and
checked field-for-field; ordinary Hessian rows total4*196=784.
There are no acceptance tolerances or post-run count retunings.

Finite epsilon/inverse lie in span{I,gamma1,gamma2,Gamma12}; each blade
conjugates into at most4 blades. The added tangent generator uses only
axes0,1,2, so first variations have at most8 blade coset components.
Initially transformed Phi1<=56 and Phi2<=364, and curvature<=3288.
All arithmetic dictionaries retain fixed exterior degree and zero
frequency. Consequently the absolute full-domain capacity of any
degree0,1,2,12,13,14 intermediate is at most
C(14,2)*16384=1490944 terms, including partial accumulation BEFORE
cancellation. Degree1/13 and degree0/14 bounds are229376 and16384.
No sparse bound is assumed to prove algebraic closure.

The product-count ceiling20,000,000,000 is a deliberately conservative
pre-execution guard, not an observed count or prediction of optimal cost.
Form grouping makes an inner top-form contraction match exactly one
input form; each conjugated Phi coefficient has<=8 blade components.
First contractions allow only two Phi1 exterior positions per degree2
input. Outer products have at most14*8 coefficient terms against a
top-form scalar. Combining these with the above degree capacities bounds
each dual chain below120million coefficient products, including the three
product-rule products at each multiplication;912 stage visits represent
114 chains, below13.68billion at these conservative capacities. There are
146 base-only Caa calls (120 directional,24 comparisons,2 source), each
below5million products with unrotated one-blade-per-form Phi coefficients.
Separate the small explicitly looped geometry controls: NormalSquare
performs five primitive products for each of9646 plane visits, not one
product per visit. Each slice has at most91 bivectors and its zero-form
argument at most8 terms; four coefficient products plus exterior insertion
give at most3640 tracked coefficient products perplane, below36million
overall. The784 ordinary-Hessian rows contain fewer than32 primitive
products each; scalar factors have at most8 terms, and connection-jet
slices have at most91. A bound32*91*8 perrow gives below19million.
After excluding those loops and the already bounded chain calls, the
remaining finite jet construction has fewer than10000 primitive products.
In this finite menu one-form jets have at most512 terms (the
fourteen canonical coefficient cosets contribute at most112 and the
additional connection/variation slots remain in the same eight-blade
algebra). Degree2 connection jets have at most8192 terms, allowing the
6576 curvature-coset terms plus differentiated field products. Products
with degree2 inputs have a zero-form factor of at most8 terms; other
products pair at most512 terms with512. Thus this construction contributes
less than10000*512*512, and adding the explicit36million+19million
controls is still below2.68billion, within the remaining5.59billion margin.
The bound does not grant a large
sampled field carrier.
Matrix operations reconstruct only two14-dimensional geometry rows;
10million tracked matrix products is a separate conservative bound.

Prospective CPU estimate30seconds, maximum estimate300seconds; peak
estimate128MiB, upper estimate1GiB. These are estimates, not scientific
results or measured execution times. Full exact tensors and variations
are serialized; no frequency or grade projection is used to save space.

## What the result could establish

The all-domain analytical identity, supported by these finite curved
controls, gives conditional Ward redundancy: with compact-supported
variations and the FULL action-derived connection gradient g, the
right-trivialized epsilon Euler derivative is -D_A^dagger g. Thus g=0
implies epsilon stationarity in this declared fixed-metric construction.
Derivatives of K belong inside that full gradient. No integration by
parts or finite total action is asserted by the local-jet fixture itself.

This can support the conditional homogeneous branch argument, but does
not replace its full gradient by a scalar action restricted to invariants.
It neither selects the physical coupling nor computes full metric Euler
equations, global spinor descent, a finite-action vacuum, observer-field
extraction, a Hessian spectrum, poles or GeV units. All such boundaries
remain explicit. No claim is made that local scalar nonzero densities
alone establish an integrated physical observable.
