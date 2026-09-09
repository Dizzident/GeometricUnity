# Phase600: full trace adjoint and periodic actual-gradient norm

## Prospective scope

Amendment A54 authorizes this exact study, building on the independently
passed598 full action variation and599 compact Lie embedding. The primary
source is the bound original draft, particularly Eqs9.3,9.4,9.7 and9.11.
The source9.11 norm remains unspecified for this purpose. The trace/form
pairing used below is explicitly declared; it is not selected as the author's
positive residual norm or as a physical completion.

The geometry is a fixed flat fourteen-dimensional reference of signature(7,7),
with normalized periodic x0 integration, fixed metric, fixed density,
epsilon=I and constant contraction coefficients. All other coordinates are
spectators. No connection sampling, physical decomposition, contour, measure,
source sign, normalization or dimensional reduction is selected.

The task computes the actual action gradient, including a full sparse
trace/exterior adjoint. It does not replace that gradient by K F+κS. Nor does
it fit or transpose a small compressed matrix. The core comparison is the
actual pointwise Lambda2Algebra API applied to the same declared compact Lie
embedding, not a finite mesh execution or an inferred continuum mesh limit.
Phase599's separate pointwise kernel result and its restriction against an
unproved action-modulo-boundary conclusion are unchanged.

## Arithmetic and helper ownership

ExactArithmetic.cs and FourierTensor.cs are byte-identical copies of the
bound598 helpers. Rational numerators and denominators use BigInteger.
Clifford signs have an independent word-insertion oracle. Finite Laurent
coefficients represent exp(i*x0),exp(i*x1); derivatives multiply by i*k and
the real bilinear pairing selects total frequency zero. There is no modewise
Hermitian replacement, numerical integration or finite-difference derivative.

TraceAdjoint.cs is a new general helper, also available to601 after immutable
freezing. Its inputs and outputs are complete sparse Fourier/Clifford forms;
it contains no carrier projection, expected target coefficient or chirality
substitution. The literal forward CCA chain remains the full598 chain.
The scientific program tests both reverse-chain implementations and their
independent forward pairings. All three helpers are exactly bound.

## The real trace and exterior adjoints

Set B(X,Y)=-ReTr(XY)/128 times the signed exterior metric. This is a real
bilinear, indefinite pairing, not a positive Hermitian inner product. For
coefficient operators C_A(X)=[A,X] and A_A(X)=i{A,X}, cyclic trace gives

    C_A^dagger(Y)=[Y,A]=-C_A(Y),
    A_A^dagger(Y)=i{A,Y}=A_A(Y).

The scalar i is retained, not Hermitian-conjugated. For example, with
Phi=theta0 gamma0, X=theta1 gamma0, Y=theta01 iI, both
B(A_Phi X,Y) and B(X,A_Phi^dagger Y) equal2. The wrong conjugation sign gives
-2 and is an explicit nonzero decoy.

For a fixed exterior form I and output form K containing I, put J=K\I.
The transpose of wedge by I carries the factor

    sigma(I) shuffle(I,J).

This follows by dividing the output metric factor sigma(K) by sigma(J),
which leaves sigma(I). The helper combines this factor with the coefficient
trace transpose and normal Fourier convolution. It does not conjugate or
reverse the coefficient frequencies in this bilinear transpose.

The14-dimensional star convention is
star(thetaI)=shuffle(I,Ic) sigma(I) thetaIc. In index7,

    star_p^dagger=(-1)^(p(14-p))star_(14-p)=-star_p^-1,
    d^dagger=-sum_i sigma_i contraction_i partial_i.

The two represented coordinate derivatives are x0,x1; all others are
identically zero. No term has been discarded by an evolution split.

The lowered CCA operator is

    K=star13 (C_Phi1 star2
         -(1/2)star1 C_Phi1 star14 A_Phi2 star2).

One adjoint reverses every typed factor literally. Independently simplifying
star13 star1=I before transposition gives the second implementation

    K^dagger Y=-star C_Phi1^dagger star Y
         -(1/2)star A_Phi2^dagger star C_Phi1^dagger Y.

The first occurrence of C_Phi1^dagger takes degree13 to12; the second takes
degree1 to0. Both literal and simplified routes check all required form
degrees. Omitting the outer Hodge, its adjoint sign or either Phi1 occurrence
would change these operations.

Seven nonzero block controls are frozen: C pairing2; A pairing2 and wrong-i
-2; a theta7/negative-metric and shuffle C pairing2; star1 and star2 pairings
-1; the periodic d/d^dagger pairing1/2; and the full DQ derivative pairing2
against the erroneous half pairing1. Additional known answers check256
Clifford word products, all16384 Hodge-square masks,
mean(sin^2)=mean(cos^2)=1/2, mean(sin^4)=3/8 and mean(cos*sin^2)=0.

## Matched family and compact periodic carrier

Use positive axes0..6 and negative axes7..13, with gamma_a squared sigma_a,
Omega=gamma0...gamma13 and Omega squared1. The H real form satisfies
gamma_a^dagger H=-H gamma_a. A grade-r blade has H-adjoint sign
(-1)^(r(r+1)/2). The two matched rows are

    h=±1, P=1+h Omega,
    Phi1=P sum_j theta_j gamma_j,
    Phi2=(c-i h Omega)sum_(a<b)theta_ab Gamma_ab.

The formal c slots are the constant and linear coefficients. No c values
are sampled or fitted. The mass term belongs only to the constant slot.
The chosen coefficient family and tied CCA brackets remain conditional;
these choices are not asserted to be the author's final operator.

Retain the599 bracket-preserving compact embedding

    E1=-(Gamma01+Gamma23)/4,
    S=theta2 E1 sin(x0), V=theta0 gamma3 cos(x0).

The source diagnostic Gram of E1 is1/8, whereas the actual core SU(2) basis
Gram is1. The source field has B(S,S)=1/16. This embedding is not an
unannounced pairing isometry. The field is even in Clifford parity,
Q(S)=S wedge S=0, and F=dS=theta02 E1 cos(x0).

The full literal forward operator gives

    K(F)=P(theta2 gamma1-theta0 gamma3)cos(x0)/2.

The second leg is zero on F because Gamma02 shares one index with each of
Gamma01 and Gamma23 in the inner anticommutator. Its ADJOINT is nevertheless
nonzero on S. Explicitly,

    d^dagger K_second^dagger S=
      -P theta3 gamma0 cos(x0)/2
      -(ic/2)P sum_(j!=0,3)theta_j gamma0 gamma_j gamma3 cos(x0).

All12 c-dependent directions and the constant companion direction are kept.
The program requires forward second-leg zero, nonzero adjoint derivative,
and exact equality to the independently derived coefficients. Thus this
control cannot pass by dropping an identically zero operator.

## Complete adjoint support and forward-pairing verification

The source one-form S has exterior index2 and coefficient blades01 and23.
For each curvature form ab, structural scalar-trace routing through either
operator leg allows only the following four input Clifford masks:

    7 xor ab, 8 xor ab, Omega xor7 xor ab, Omega xor8 xor ab.

Here7 is the012 mask and8 the gamma3 mask. In the first leg, form ab must
contain2 and the remaining Phi1 index produces a subset of these masks.
In the second leg the outer Phi1 index must be2 and inner Phi2 form ab;
XOR of its coefficient masks with01/23 gives the stated set. This routing
ignores coefficients and cancellations, so it is an upper support proof,
not a derivation from the desired adjoint output. The four masks are
distinct: their differences are neither0 nor the full14-dimensional mask.

The program separately generates the structural set from the actual S,
Phi1 and Phi2 supports and compares it to this independent four-mask set.
There are364 candidate form/blade positions and1,490,580 excluded positions
out of91*16384. Those excluded positions cannot contribute any scalar trace;
the report does not claim that their forward chains were individually run.

Each candidate is probed with sin and cos, using phase1 for a real
H-anti-Hermitian blade and phase i otherwise. These span the real H-anti
frequency±1 inputs. Other Fourier frequencies pair to zero by orthogonality,
because the untransformed K coefficients are constant. All2912 forward
pairings B(S,KF_probe) are compared to B(K^dagger S,F_probe).

The constant K^dagger S has14 exterior positions and two chiral coefficient
blades at each, hence28 static positions/56 Fourier terms. They are02,12,
03,13 and3j for j4..13; the23 contribution cancels between adjoint legs.
The c slot has all78 two-form positions excluding axis3, with two chiral
blades each:156 positions/312 Fourier terms. Thus368 sin probes are nonzero
over both chiralities; every cos probe is zero. These are exact predictions.

Only after those complete coefficients and support pass does the program
use Y=K^dagger S for the full curvature derivative

    DQ_S[V]=S wedge V+V wedge S.

For one-forms this equals the coefficient commutator-wedge map, so its
adjoint is C_S^dagger with NO further factor2. Reverse routing through the
actual checked Y and the two E1 coefficient blades gives8 candidate
positions in the constant slot and48 in the c slot. Probe each with
1,cos(2x0),sin(2x0), covering all real output frequencies0,±2. The336
independent forward checks directly form both wedges in DQ, not a half
polarization. The actual result has four static supported positions in
each slot; constant and cos2 probes are nonzero, sin2 probes zero, yielding
32 nonzero pairings. No predicted support is used to truncate the result.

The conservative raw support bounds1560 for K-adjoint and6240 for DQ-adjoint
are unmeasured bounds, not targets. In particular S has two blades and two
Fourier modes, so four raw candidates per incoming Y term are allowed.

## Full actual gradient and independent coefficient predictions

On the fixed flat background, the action is

    I(S)=B(S,K dS)/2+gamma B(S,K Q(S))/3+kappa B(S,S)/2.

For the FULL derivative DQ, direct variation gives

    G=(K dS+d^dagger K^dagger S)/2
      +gamma(KQ+(DQ_S)^dagger K^dagger S)/3+kappa S.

This formula must not acquire the extra2 appropriate to a different
half-bilinear convention. Although Q(S)=0, its adjoint variation need not
vanish. Write G=L+gamma N/3+kappa S. The hand-derived tensors are

    L=P[-theta0 gamma3/4+theta2 gamma1/2-theta3 gamma0/4
        -(ic/4)sum_(j!=0,3)theta_j gamma0 gamma_j gamma3]cos(x0),
    N=P[(theta0 gamma0+theta1 gamma1)/4
        +(ic/4)(theta0 Gamma123-theta1 Gamma023)]sin^2(x0).

L has12 and48 Fourier terms in its two c slots; N has12 in each.
At kappa0, G has24/60 terms; at kappa1 it has28/60. The extra mass terms
are even and cannot cancel odd gradient terms. Gamma=1,2 and kappa=0,1
give16 gradient rows. Each is compared to the full computed coefficients,
H real form, chirality and an independent direct action variation.

For the closed odd direction V, dV=0 and B(V,KF)=1/4. The cubic variation
pairs to zero because K(theta02 gamma2) has no theta2 component. Therefore
delta I[V]=B(V,G)=1/8, independent of gamma and kappa. This nonzero exact
periodic integral is not an unintegrated boundary term. The shortcut
pairing1/4 is separately rejected in both chiralities; it is not used to
define the actual gradient.

## Null trace square versus positive diagnostic

This requires a proof about the actual gradient, not only about the shortcut.
For even input, the literal K lands in P*Cl_odd. Separately, cyclic trace
transposition gives [S,P gamma]=P[S,gamma] for even S, and the transpose
i-anticommutator with even Phi2 preserves P*Cl_odd. Hodge adjoints act on
forms only. Thus K^dagger S also lies in P*Cl_odd. The fixed derivative
and DQ adjoints with even S preserve this subspace, so L and N do too.

For odd X,Y, XP=(1-h Omega)X and therefore
(PX)(PY)=P(1-h Omega)XY=0. Hence every massless actual-gradient trace
self/cross pairing vanishes pointwise, despite the nonzero variation1/8.
The even mass term is trace-orthogonal to the odd gradient. The exact formal
polynomial in c is consequently

    B1(G,G)=kappa^2/16,
    B13(star G,star G)=-kappa^2/16.

The latter sign follows from index7; one must not silently identify the
standard upper13-form metric with a lowered pullback convention.

For comparison only, the positive Fourier/Clifford coefficient diagnostic is

    3/8+gamma^2/96+c^2(3/4+gamma^2/96)+kappa^2/16.

Linear cos and quadratic sin^2 terms are orthogonal. Constant and c slots
occupy different Clifford grades, and the even mass part is orthogonal to
both. The program computes all three polynomial coefficients1,c,c^2 using
full tensors, not a selected numerical c. Eight rows check the trace,
upper13 and positive diagnostic polynomials. This positive diagnostic is
not invariant under arbitrary H-unitary changes and is not thereby an
admissible or source-selected residual norm.

## Actual registered API comparison

Call the actual CreateSu2WithTracePairing factory and check all9 Gram and27
structure entries against the declared compact basis. Call the actual
MemberEndomorphism for sd2/id0 coefficient1/2 and id0/none, and the actual
HodgeStar API. Check all108 matrix entries against independent4-dimensional
exterior shuffle signs and identity. Source star14 is never replaced by star4.

On the core input theta02 T1 cos(x0), the registered map (I+star4)/4 gives
coefficients+cos/4 at02 and-cos/4 at13. Its normalized positive half-square
is1/32; the identity control is1/4. All36 residual coefficients and both
norm rows are checked with exact dyadic binary64 operations and tolerance0.
No face averaging or mesh interpolation is performed or inferred.

At kappa0 the declared actual-gradient trace square is zero while that
registered pointwise-kernel functional is positive on this periodic input.
At kappa1, half the lowered trace square happens to be1/32. This isolated
mass-only coincidence does not identify the operators, choose kappa, fix
the source norm or establish an action bridge. In particular, normalizing
a null pairing cannot turn its zero into a positive kinetic contribution.

## Frozen execution contract and boundaries

The complete fixture JSON is structurally repeated in Program and contract.
The counts frozen above also include4 adjoint contexts,4 nonzero omitted-
adjoint controls,2 shortcut rejections and8 norm rows. All exact checks and
actual dyadic core comparisons use tolerance0. CPU estimate15seconds,
upper estimate90seconds; memory estimate128MiB, upper estimate256MiB.
These are prospective resource estimates, not scientific acceptance fits.

Precedence is invalid/drifted input; known-answer adjoint failure; full
adjoint/support failure; actual-gradient failure; declared-norm failure;
registered/count failure; then
`full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch`.
Failed frozen outputs must be retained; scientific repairs require versioning
and fresh review. No scientific execution precedes independent and MAIN
full-pack review and explicit MAIN approval.

All17 unique exact bindings include own Program/project/proof/three helpers,
primary source,598 program/contract/summary and copied-helper lineage,
599 program/contract/summary, the full726-file core manifest and build props.
Live core paths, every hash and the sorted tree digest are checked; an
unchanged manifest file alone is insufficient. Full/summary JSON are
deterministic identical. No core or historical frozen files are modified.

All fourteen authority flags remain false, O4 and external review remain
pending, and promotedPhysicalMassClaimCount=0. Correcting the first action's
gradient and choosing a squared action are distinct operations. No source
norm, transformation, measure, physical spectrum, stability, hyperbolicity,
physical field, pole mass or unit normalization is selected by this audit.
