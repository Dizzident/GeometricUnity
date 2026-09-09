# Phase604: squared-action stationarity is not equation equivalence

This is the prospective A56 audit. All scientific code, helpers, project,
proof, fixtures, exact file bindings, resource ceilings and terminal rules
must be frozen before the first scientific execution. A successful Release
build does not execute the study. MAIN and an independent reviewer must
approve the complete frozen pack before science. Failed first outputs are
retained; any scientific repair requires a versioned, newly reviewed pack.

## 1. Distinct declared equations and actions

Keep the fixed flat reference, metric, orientation, normalized density and
epsilon=I of600-603. The fields in this audit are constant. All connection
coefficient variations are allowed before the resulting gradient is compared
with the invariant carrier. Derivative-adjoint terms vanish on these constant
fields and residuals because the metric and tensors are fixed and constant;
they are not discarded from the general nonconstant Euler equation.

Use signature(7,7), H-anti-Hermitian connection coefficients, both h=-1,+1,
P=1+h Omega, Phi1=P Gamma1, Phi2=(c-i h Omega)Gamma2. The real c is formal.
The complete typed CCA source9.3 operator K retains both Hodge legs and its
printed factor1/2. It is exactly the linked600 implementation. No final
author tensor, normalization or bracket convention is selected here.

The declared B is real bilinear -ReTr/128 times the signed exterior metric,
with normalized constant integral. It is indefinite, not a chosen positive
source9.11 norm. Set Q=S wedge S and retain three different objects:

R=beta KQ+kappa S,
G=gamma(KQ+DQ_S^dag K^dag S)/3+kappa S,
J_R=B(R,R)/2, J_G=B(G,G)/2.

G is the actual first-action gradient. R is the explicitly parameterized
curvature residual; beta and gamma are independent formal variables, not
identified or fitted. J_R and J_G are two distinct declared square actions.
The source9.11 expression without the half factor doubles their gradients
and Hessians. That normalization change cannot establish equation
equivalence, and neither declared square chooses the source's missing norm.

## 2. Exact polynomial and carrier conventions

Use six equivalent formal variables (r,s,c,beta,gamma,kappa), with
r=a-hb,s=a+hb, and the invertible bridge a=(r+s)/2,b=h(s-r)/2.
Let Er=(1-h Omega)Gamma1/2, Es=(1+h Omega)Gamma1/2. Then S=r Er+s Es.
The full basis Gram is [[0,-7],[-7,0]], nondegenerate despite each basis
vector being null. Raising a scalar derivative therefore gives
(-partial_s J/7,-partial_r J/7), not the coordinate Euclidean gradient.

The owned SquaredPolynomial helper extends the exact-bound603 five-variable
helper to six exponent slots. It provides BigInteger rational arithmetic,
sparse coefficient multiplication, exact differentiation and simultaneous
polynomial substitution. No substitution is recursive: the surviving r in
a branch substitution is a fresh amplitude t, not the old r to be replaced
again. No floating-point root fitting, sampling, finite difference or
truncation is used. Exact zero coefficients alone are removed.

Tensor coefficients remain unrestricted14-dimensional form/Clifford masks.
The literal source chain and full real transposes act on every coefficient;
no result is projected into Er,Es before equality with its full tensor is
checked. The real transpose of a coefficient commutator reverses its sign;
the i-anticommutator retains i. Exterior signature and shuffle factors and
all Hodge adjoints are inherited unchanged from600. DQ_S[V]=S wedge V+
V wedge S is the FULL derivative, so DQAdjoint has no additional factor2.

The first known-answer menu checks44944 independent blade/word products:
grades0,1,2,12,13,14 contain212 masks. All16384 Hodge squares and eight
six-variable polynomial identities are checked. Both chiralities have their
four Gram entries independently computed. The helper extension and every
compiled upstream source file are exact-bound; no upstream file is edited.

## 3. Full source identities and nonzero c intermediates

Define A=208gamma and B0=624beta (B0 is a scalar, not the pairing). Direct
source operations must reproduce the following complete tensor identities:

Q=2rs Gamma2; KQ=312rs P Gamma1;
Y=K^dag S=-24r Gamma2-28ihcr OmegaGamma2;
DQ_S^dag Y=624rS;
G=r(Ar+kappa)Er+s(2Ar+kappa)Es;
R=kappa r Er+s(B0r+kappa)Es.

K^dag(P Gamma1)=0 is calculated explicitly, including both c slots. Thus
K^dag R=kappa Y. Also K^dag G is the same full Y formula with r replaced
by g_r=r(Ar+kappa), not with S replaced by a scalar norm. The nonzero c
coefficients in Y,K^dag R,K^dag G are checked BEFORE cancellation. The final
G and both square gradients must have no c coefficients. Literal and
independently simplified full reverse chains are compared on all12 new
polynomial inputs across the two h.

## 4. True square transposes, not original-equation shortcuts

The full differential DR[V]=beta K DQ_S[V]+kappa V gives

grad J_R=beta DQ_S^dag K^dag R+kappa R
        =kappa[(B0r+kappa)S+312beta rs P Gamma1].

This is not R itself. For J_G one needs the derivative of the ACTUAL
first gradient, rather than treating the rejected KQ shortcut as its Euler
derivative. Put M_Y[V]=DQ_V^dag Y. The actual constant first Hessian is

H_I[V]=gamma/3(K DQ_S[V]+M_Y[V]+DQ_S^dag K^dag V)+kappa V.

M_Y is real self-adjoint: B(U,M_Y[V])=B(DQ_V[U],Y) and
DQ_V[U]=DQ_U[V]. Transposing H_I swaps the first and third terms and
leaves the middle term and mass term fixed. Hence
grad J_G=(DG)^dag G=H_I G. The program constructs all three full terms;
the separately written reversed composition is an algebraic transpose
check, not advertised as an independent numerical implementation.

For a general invariant V=v_r Er+v_s Es, independent source hand formulas
for the three unweighted terms are

K DQ_S[V]=624(s v_r+r v_s)Es,
M_Y[V]=624rV,
DQ_S^dag K^dag V=624v_rS.

They hold as complete tensors for formal c. They are checked separately
on both invariant basis vectors and on G; no term is inferred by subtracting
the other terms from an expected answer. Full first-Hessian columns must
also equal exact derivatives of the previously computed full G polynomial.

For each basis vector the program independently pairs the middle transpose,
DR transpose and DG transpose in their forward and reverse orders. The
original first scalar action I=gamma B(S,KQ)/3+kappa B(S,S)/2 is separately
differentiated and compared with G. Thus the same code path does not merely
declare itself the true action derivative.

## 5. Independent scalar squares and full invariant Hessians

The program forms B(R,R)/2 and B(G,G)/2 from the complete source tensors,
then differentiates those literal polynomial coefficients independently of
the adjoint computations. The scalar predictions are

I=-7rs(Ar+kappa),
J_R=-7kappa rs(B0r+kappa),
J_G=-7s f(r), f(r)=r(Ar+kappa)(2Ar+kappa).

Their raised square gradients are
g_R=(kappa r(B0r+kappa), kappa s(2B0r+kappa)),
g_G=(f,s f').

Both full tensors must equal these coordinates times Er,Es. The complete
full-gradient polynomial identities are differentiated to construct the
square Hessian columns, then compared with independent second derivatives
of the literal scalar squares. Their raised matrices are

H_R=[[kappa(2B0r+kappa),0],[2B0 kappa s,kappa(2B0r+kappa)]],
H_G=[[f',0],[s f'',f']].

All eight square first derivatives, eight full columns and16 second
bilinears across h must agree. An additional24 checks explicitly cover
doubling all eight first and16 second derivatives when the half-square
normalization is removed. They do not alter the chosen beta or gamma.

## 6. Complete stationary branches, including degeneracies

For gamma*kappa nonzero, the factors of f give exactly three distinct
roots0,-kappa/A,-kappa/(2A). At these roots f' equals kappa^2,kappa^2,
-kappa^2/2 respectively, all nonzero. Thus s f'=0 requires s=0. No other
stationary points exist in this invariant family under these assumptions.
The corresponding invariant H_G is f'I2. The first two roots are first-
action stationary points; the third is not a zero of the original G.

For gamma=0,kappa nonzero, f=kappa^2r and only the origin is stationary.
For gamma nonzero,kappa=0, f=2A^2r^3, giving the entire r=0 line; its
INVARIANT square Hessian is zero even though the original first-action
Hessian on that line can be nonzero nilpotent. If both couplings vanish,
every invariant constant S is stationary. None of these zero invariant
Hessians is a claim that the full ambient Hessian is zero.

For beta*kappa nonzero, g_R vanishes only at(r,s)=(0,0) and
(-kappa/B0,0); the other diagonal factor is nonzero at both roots. Their
invariant Hessians are+kappa^2I2 and-kappa^2I2. For beta=0,kappa nonzero,
only the origin remains. If kappa=0 the FULL constant grad J_R vanishes
on every invariant S, for arbitrary beta; R need not vanish. The both-zero
case is retained explicitly as a separate diagnostic branch row.

The frozen program contains six J_G and five J_R symbolic rows per h,
22 in total. Nonzero roots are substituted without division: r=-t,s=0,
kappa=208gamma t,416gamma t,624beta t for the relevant branch. Generic
rows mean the named coupling and t are nonzero; their10 nonzero root-
derivative certificates and exact factorization prove completeness, not a
scan of amplitude values. Degenerate rows leave free r,s where specified.

Full constant stationarity is not inferred solely from a restricted scalar
extremum. The full adjoint gradients are already equal to invariant tensor
polynomials, and those entire tensors vanish under the substitutions.
Separately, local Spin equivariance and the exact-bound595 complete
two-dimensional invariant one-form classification force these constant
gradients into Er,Es, whose restricted Gram is nondegenerate. This is local
tensor algebra, not a statement that arbitrary Spin rotations preserve a
torus lattice, nor that metric/reference Euler derivatives vanish.

## 7. Nonzero original equations and exceptional relations

Five explicit polynomial decoys per h prevent interpreting a zero square
or its stationarity as a zero of the original equation.

1. At the extra J_G root r=-kappa/(416gamma),s=0,
   G_r=-kappa^2/(832gamma), G_s=0. The full G is nonzero and null, while
   H_I G=0 and grad J_G=0. The independent first-action variation along
   Es is7kappa^2/(832gamma), nonzero. Under the denominator-free branch
   substitution G_r=-208gamma t^2. Its raw coefficient norm is7 G_r^2,
   nonzero; it is only a diagnostic of nonzero coefficients, not a newly
   selected positive action. H_I vanishes on the invariant two-dimensional
   carrier at this point, not necessarily on its full ambient domain.

2. At the nonzero J_R root, R_r=-kappa^2/(624beta),R_s=0. Its full tensor
   and pairing B(R,Es)=7kappa^2/(624beta) are nonzero, although grad J_R
   and the trace square vanish. The independent coefficient norm7 R_r^2
   is retained as the same nonzero diagnostic.

3. At kappa=0,r=s=1, R=624beta Es and B(R,Er)=-4368beta, while grad J_R=0.
   At the same field grad J_G=(2A^2,6A^2) is nonzero when gamma is nonzero.
   This distinguishes two declared square actions with all couplings kept
   independent; it is not a positive-norm variation experiment.

4. At the first-action branch r=-kappa/(208gamma),s=0,
   dJ_R[Es]=7kappa^3/(208gamma)(1-3beta/gamma), whereas dJ_G[Es]=0.
   In the polynomial normalization the first expression is
   302848gamma(gamma-3beta)t^3. It is not the zero polynomial. The invariant
   Hessians there are kappa^2(1-6beta/gamma)I2 and kappa^2I2.

5. Impose gamma=3beta as an EXPLICIT exceptional-control substitution,
   not an inferred law. Both gradients then vanish at the common branch,
   but their invariant Hessians are respectively-kappa^2I2 and+kappa^2I2.
   Thus even a deliberately coincident stationary point does not identify
   the two equations or their Hessians.

The lowered invariant Hessians use the indefinite Gram, so scalar raised
eigenvalues here do not label Euclidean minima or dynamical stability.
No physical vacuum, original-equation equivalence, particle, pole or mass
follows from any of these declared squared-action stationary points.

## 8. Frozen counts, degree/support bounds and resources

The full fixture object is duplicated literally in Program and contract;
complete structural equality is required, not merely a fixture hash.
Each h has the following independently predicted nonzero coefficient
counts after like monomials/masks are combined:

| Tensor/scalar | Count |
| --- | ---: |
| S,Q,KQ |56,91,28|
| Y,DQ^dag Y |182,56|
| G,R |112,84|
| K^dag R,K^dag G |182,364|
| grad J_R,grad J_G |112,168|
| I,J_R,J_G scalar monomials |2,2,3|
| H_R columns |84,56|
| H_G columns |140,84|

These counts follow because Er,Es each have28 blade/form coefficients;
Gamma2 has91, and each independent listed polynomial monomial multiplies
one such tensor. The complete field square has one rs Gamma2 monomial,
not two separate unsimplified a^2,b^2 monomials. Y_G has four monomials
with91 coefficients each, including the two nonzero c-adjoint monomials.

Across both h the frozen counts are44944 word cases,16384 Hodge cases,
8 polynomial controls,8 Gram entries,2 chirality rows,30 complete source
identities,6 scalar identities,6 retained c-adjoint checks,12 adjoint calls,
4 first-action derivatives,4 first-Hessian columns,12 individual first-
Hessian legs,4 middle-transpose,4 residual-transpose and4 gradient-transpose
pairings,8 square derivatives,8 square columns,16 square Hessian bilinears,
24 no-half normalization controls,22 stationary rows,10 generic root
certificates and10 nonzero-equation decoys. Every count is fail-closed.

Exact zero tolerance is used everywhere. Six-variable exponents are never
truncated. The scalar squares have total degree at most6; full unsimplified
source compositions can introduce c before its checked cancellation, so
the frozen tracked-output degree ceiling8 is conservative. The largest
predicted named tensor has364 coefficients; a2048 final-coefficient ceiling
allows the checked branch and derivative constructions without selecting
or truncating their supports. The scalar-monomial ceiling is256.

Every coefficient remains at zero Fourier frequency. Each source stage is
a finite full mask product with exterior-overlap pruning; no source spectrum
or projected carrier is used to accelerate it. A single new-input adjoint
coefficient can occupy at most91 two-form positions with four routed
Clifford masks per form. Products of the two28-term invariant basis tensors
and these finite adjoints give a generous temporary ceiling65536 terms.
The fixed12 polynomial adjoint calls and the small source/transpose menu
give a conservative20000000 coefficient-product ceiling. These ceilings
are checked, not learned from a trial execution. CPU estimate15s/maximum
estimate90s and memory estimate128MiB/maximum estimate512MiB are prospective
resource estimates, not mathematical or physical assumptions.

## 9. Integrity and claim firewall

Twenty unique exact file bindings include own Program/project/proof/helper,
all three externally compiled600 helpers, passed600 and603 summaries,
contracts and programs, the original603 helper, passed595 summary/contract/
proof, primary source text, the complete core manifest and Buildprops.
No shared integration, core or frozen historical file is edited. Program
checks every exact hash and unique ID/path, all upstream success/firewall
conditions, and the full live726-file src .cs/.csproj tree against its
exact-bound manifest, excluding bin/obj and recomputing its tree digest.

Terminal precedence is invalid/drifted input, known-answer failure, full
square-transpose failure, independent derivative failure, stationary-branch
failure, nonzero-equation/count/resource failure, then success:
squared-action-stationary-controls-pass-equations-not-equivalent.
Both knownAnswerPassed and controlsPassed are explicit evidence. Full and
summary are serialized from the same deterministic object into identical
bytes. Contracts have one final newline, not an extra trailing blank line.

All14 authority flags remain false, external review/O4 remain pending,
Phase561 stays closed and promoted physical mass claims remain0. These
results concern fixed-geometry constant connection variations only. They
do not select source intent, source9.11 norm, measure, contour, coupling,
vacuum, physical time split, production model, scale or GeV calibration.
