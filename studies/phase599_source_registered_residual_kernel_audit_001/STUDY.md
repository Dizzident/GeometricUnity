# Phase599: source and registered residual kernel bridge

Prospective A53 bounded exact audit. Freeze all code, inputs, predictions,
resources and precedence before independent review and explicit coordinator
approval of the first scientific run. This study tests one concrete proposed
pointwise residual identification. It does not select an action or measure.

## Source and actual registered target

The primary draft Eq.9.3 is a fourteen-dimensional degree-raising contraction
from two-forms to thirteen-forms. Eq.9.7 calls its curvature-plus-torsion
residual the first action's Euler derivative; Eq.9.11 separately defines a
squared residual action. Exact-bound text references are lines 2120-2133,
2200-2211, 2230-2244 and 2267-2290 of GU-DRAFT-2021-TEXT.txt.

For fixed epsilon/metric, if r=K F_A+kappa T is taken as a definition, its
square has the chain-rule derivative 2(Dr)^dagger r with
Dr[V]=K D_A V+kappa V. That remains true even when r differs from the actual
first-action gradient. Replacing r by that corrected gradient would define
a different squared action; no replacement is made here.

Actual registered code, EinsteinianShiabFamilySpec.cs:71-80, instead declares
a reduced degree-preserving map on Lambda^2(R^4), acting separately on each
SU(2) coefficient. Lambda2Algebra.MemberEndomorphism at lines 97-106 returns
A(Phi1)(I-c A(Phi2)). For the sampled sd2/id0 c=1/2 member this is

    R_reg=P_+/2=(I+star4)/4.

EinsteinianShiabOperator.cs:140 uses this same API to construct its cell map;
lines 17-28 and 297-350 describe reconstruction, contraction and averaging.
The present study calls the ACTUAL MemberEndomorphism API, not a retyped
substitute. It tests its pointwise represented-two-form map only: no mesh
insertion, inverse geometry or face averaging is performed. The identity
id0/none member and actual HodgeStar API are separate controls.

The full registered objective at EinsteinianShiabOperator.cs:817-847 is
one half of the positive compact-trace residual square with trivial torsion.
The source has relative one-form torsion T=A-B and different field/output
degrees. Dropping a torsion operator is not imposing the field equation T=0.
No dimensional-reduction dictionary is supplied by the name SD2.

## Separate Clifford and Hodge domains

The source uses Cl(7,7), positive axes 0 through 6 and negative axes 7 through
13, gamma_a squared sigma_a, Omega=gamma0...gamma13 and Omega squared I.
The declared diagnostic pairing is -ReTr(XY)/128 with the signed form metric.
It is NOT selected as a positive source norm. The H-Hermitian form is
H=gamma7...gamma13; tested compact Lie generators are H-anti-Hermitian.

The source Hodge star is the literal fourteen-dimensional operation

    star14(thetaI)=shuffle(I,Icomplement)*product_sigma(I)*thetaIcomplement.

In particular star14(top)=-1, and star14 on thirteen-forms inverts star14
on one-forms. Source intermediates retain the full 2->12->13 and
2->12->14->0->1->13 chains. There is no replacement by star4.

The registered star4 is the Euclidean involution on the six-dimensional
ordered basis (01,02,03,12,13,23). Its independent oracle follows exterior
shuffle signs and the four-bit complement. Both stars are tested in their
own domains. All 16384 source Hodge-square identities and 44944 independent
Clifford word products on grades 0,1,2,12,13,14 are known answers.

## Compact Lie embeddings and normalization

On positive axes 0..3 set

    Sigma1=theta01+theta23,
    Sigma2=theta02-theta13,
    Sigma3=theta03+theta12,
    J_i=Gamma(Sigma_i)/2.

Direct Clifford products give [J_i,J_j]=-2 epsilon_ijk J_k and
B(J_i,J_j)=delta_ij/2. For example [J1,J2]=-Gamma03-Gamma12=-2J3;
J_i squared=(-1+Gamma0123)/2, whose scalar trace is -1/2.
The actual core basis has [T_i,T_j]=epsilon_ijk T_k and Gram delta_ij.
Consequently the bracket-preserving embedding is

    T_i maps to E_i=-J_i/2=-Gamma(Sigma_i)/4,
    B(E_i,E_j)=delta_ij/8.

This is an injective Lie embedding, NOT a pairing isometry. Multiplication
of the source diagnostic pairing by eight would match this embedded compact
Gram, but is not a source normalization chosen by this study.

The anti-selfdual control uses a separately labeled embedding:

    barSigma=(theta01-theta23, theta02+theta13, theta03-theta12),
    barJ_i=Gamma(barSigma_i)/2,
    [barJ_i,barJ_j]=+2 epsilon_ijk barJ_k,
    barE_i=barJ_i/2.

Its Gram factors are again 1/2 and 1/8. There are 18 raw J bracket checks,
18 E bracket checks, 18 raw Gram checks, 18 embedded Gram checks and six
H-anti-Hermitian checks. All 27 structure constants and nine Gram entries
from the actual CreateSu2WithTracePairing factory are checked independently.
No rescaling which preserves the metric but breaks the Lie bracket is hidden.

## Explicit Weyl witness and independent curvature checks

Define pair-antisymmetric, pair-symmetric R by

    R_abcd=Sigma1_ab Sigma1_cd-Sigma2_ab Sigma2_cd.

In the ordered pair matrix its nonzero entries are

    R0101=R2323=1, R0123=R2301=1,
    R0202=R1313=-1, R0213=R1302=1.

The only potentially nontrivial four-distinct-index Bianchi relation is
R0123-R0213+R0312=1-1+0=0. Repeated-index identities follow from pair
symmetry and antisymmetry. Ricci diagonal entries on 0,1,2,3 are sums
1-1 or -1+1, while all off-diagonal and other entries vanish. Thus Ricci
and scalar curvature are zero, but R is nonzero.

Use the frozen Phase591 convention, including the ordered-pair half factor,

    F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d gamma_c gamma_d.

It gives

    F=Sigma1 J1-Sigma2 J2=-2 Sigma1 E1+2 Sigma2 E2.

The anti-selfdual Weyl control replaces Sigma by barSigma. Its diagonal
entries are unchanged; R0123=R0213=-1, and the same Bianchi cancellation
and Ricci zero follow. Its curvature is
barF=barSigma1 barJ1-barSigma2 barJ2=2 barSigma1 barE1-2 barSigma2 barE2.

Together with zero and the full-so4 plane R0101=1 these are exactly four
curvature fixtures. The implementation checks all 14^4 ordered index
quadruples for each fixture: pair antisymmetries, pair interchange and
Bianchi, totaling 153664 component cases. All 784 Ricci entries and four
scalars are checked against the independent displayed predictions. Each
Weyl curvature is reconstructed independently through both J and E, giving
four dictionary checks; the literal curvature assembler is not its own oracle.

## Complete fixed source family and nonzero anchors

Keep formal real a,b,c,d, Phi1=(a+b Omega)gamma and
Phi2=(c+i d Omega)Gamma2. The four tied occurrence choices CCC, CCA, AAC,
AAA form a declared subfamily, not a claim that the source requires tying.
Each three-letter branch lists first, outer and inner bracket occurrences,
with C(X,Y)=XY-YX and A(X,Y)=i(XY+YX). Compute the literal chain

    star1_inverse([Phi1 wedge star14 F]_first
      -(1/2)star14([Phi1 wedge star14([Phi2 wedge star14 F]_inner)]_outer)).

Every coefficient of (a,b,ac,bc,ad,bd) is retained separately. Four fixtures
times four branches times six monomials give exactly 96 formal rows.
This is an exhaustive coefficient identity for this parameterized subfamily,
not a sampled parameter scan. The independent Phase591/592 Ricci/scalar
identities predict all Weyl and zero rows vanish. The program nevertheless
executes the full wedge/Hodge/bracket chain before comparison.

The plane control has Ricci00=Ricci11=1 and scalar2. Write
J=theta0 gamma0+theta1 gamma1 and G=sum_(j=0..13) theta_j gamma_j. Then

    CCC=-a J-b Omega J,
    CCA=-a J-b Omega J-ad Omega G-bd G,
    AAC=0,
    AAA=ac G+bc Omega G.

These give eight nonzero formal rows and 88 zero rows overall. The plane is
deliberately outside the strict selfdual SU(2) carrier. A Riemann curvature
entirely in Lambda+ tensor Lambda+ must have trace-free selfdual block by
Bianchi, hence is Weyl; it cannot supply a nonzero Ricci control there.

The canonical plane CCA at (a,b,c,d)=(1,0,1,0) is -J, with two coefficients
and diagnostic self-pairing -2. The two matched rows (1,h,1,-h), h=+/-1,
are (1+h Omega) sum_(j=2..13)theta_j gamma_j, with 24 coefficients each.
Both are NONZERO but self-pair to zero. Indeed Omega anticommutes with odd
Clifford elements, so P_h X_odd P_h Y_odd=0 for P_h=(1+h Omega)/2.
This also explains why a matched Riemann contraction can be null throughout
its nonzero image. No zero-norm test is used to certify an operator kernel.
The source's norm notation in Eq.9.11 does not settle which positive or
indefinite pairing a physical completion uses.

## Registered residual predictions and robust mismatch

The actual registered matrix is compared entrywise with (I+star4)/4; the
actual identity control with I; and the actual star with the independent
exterior oracle. These are 108 exact matrix-entry checks. All numbers are
small dyadic rationals, and all used core operations are additions and
multiplications exactly representable in binary64. The tolerance is ZERO;
there is no quadrature, exponential or inversion in this API path.

On the core coordinates the positive Weyl fixture has four coefficients of
magnitude two, norm squared16. Its registered residual is F/2, with norm
squared4 and pointwise half-square2 in the CORE identity pairing. The
anti-selfdual fixture has input norm squared16 and registered residual zero.
Zero remains zero. Identity contraction must return every input unchanged.
Three inputs times two operators times 18 coefficients give 108 residual
entry checks, plus three norm/action rows. These pointwise numbers are not
the finite mesh or induced continuum action of Phase588; no factor from a
face census is silently omitted from a claimed mesh result.

At both zero and the positive Weyl input the source residual equals zero
in every declared formal coefficient slot, but the registered residuals
are distinct. Thus no zero-preserving map of SOURCE RESIDUAL OUTPUT ALONE
can reconstruct the registered output there. Conversely an injective map
cannot send those two distinct registered residuals to the same source
residual. This rejects the proposed literal residual identification under
the declared input embedding, independently of residual pairings, overall
normalization and chiral null self-pairings. A map depending separately on
the original input is not a residual-output identification of this kind.

Noninjective projection, altered input dictionaries, other source operators
and physical dimensional reduction remain open. This is not a theorem that
the word selfdual can never appear in a valid source reduction.

## Local realization and action/measure boundary

Any prescribed Lie-valued curvature F at a point can be realized on a flat
reference patch by A_mu(x)=-(1/2)sum_nu F_mu,nu x^nu. At the origin A=T=0,
dA=F and A wedge A=0. Thus a pointwise comparison with T=0 and nonzero F
is consistent. Away from that point nonabelian A wedge A generally makes
curvature nonconstant. T IDENTICALLY zero on a flat reference patch instead
forces A=B and F=0. No global constant-curvature connection, periodic lift,
Levi-Civita realization, finite-lattice connection, or quantum carrier is
constructed by this audit. The local realization is a written algebraic
observation, not an executed field-lift test.

Pointwise residual inequivalence does not alone exclude action equivalence
modulo total derivatives: selfdual curvature squares can contribute a
Pontryagin density. A global Fourier or compact-support Hessian comparison
would need a declared field/pairing dictionary and separate preregistration.
Even action equivalence would not select section density, Jacobians, residual
symmetries or a probability measure. Neither source first-action cubic
unboundedness nor this kernel mismatch is a theory-wide no-go.

## Freeze, lineage, resources and terminals

Exactly thirteen unique bindings cover Program, project, this STUDY, the
unchanged compiled Phase592 ExactAlgebra helper, primary source, Phase588,
591 and 592 summaries/contracts, Directory.Build.props and the complete
726-file live core source manifest. The core manifest binds the actually
referenced core projects and Lambda2 API; no helper dependency is inferred
merely from an old output. Upstream success terminals and control flags must
match, and the entire standalone fixture object must equal Program's menu.

Estimated resource use is five CPU seconds and 128 MiB, with prospective
ceiling estimates 30 seconds and 256 MiB. No sampling, profile search,
parameter fitting or threshold adjustment. Input drift dominates known-answer
failure, embedding/curvature failure, literal-source failure, registered
failure, kernel-witness failure, then success
`source-registered-residual-kernel-mismatch-scoped`.

Full and summary JSON are identical deterministic bytes. Any first-run
failure is preserved, and scientific repair requires a separately reviewed
versioned freeze. No scientific execution precedes full coordinator and
independent review plus explicit approval. Only the unbound implementation
record may acquire results afterward. All fourteen authority flags stay
false, O4 and external source review remain pending, Phase561 stays closed,
and promoted physical mass claims remain zero.
