# Phase623: full inverse-kappa fifth-order feedback

Prospective A64 v1, on main after checkpoint
23291a3078d23191df4060da3ad3f57870ecc675. All scientific files, fixtures,
forecasts, census, resources and this proof freeze BEFORE FIRST scientific
execution. Release build checks are permitted. Independent full-pack review,
MAIN full review and explicit MAIN execution approval are required.
There is no pilot, coefficient fitting, interpolation or iterative solve.
Neither code nor lineage uses an unexecuted622 artifact.

Success is
full-inverse-kappa-fifth-order-controls-pass-grade-five-branch-required.

## Full equation, source authority and conventions

Use the declared curved homogeneous geometry already passed608/618:
alpha1,beta-1/2,sigma-1 and y0=diag(-1,1,1,1),
y1=diag(-1,4,9,16). The passed618 FULL frame Nomizu matrices include
the coordinate connection AND the moving-frame derivative. They are not
coordinate Christoffel matrices with vertical derivatives silently deleted.
The two oriented frames have signature(+7,-7).

The diagnostic operator is canonical UNTIED CAA, firstC/outerA/innerA,
with Phi1=Gamma1 and Phi2=Gamma2. Its source choice remains unresolved:
this test does not select the author's missing operator occurrence.
The bound primary3.27 and3.34 explicitly give the full real u(64,64)
adjoint domain, including central iI and both Clifford parities.
We keep the draft's checkerboard caveat and test all16384 masks:
a grade-r blade has real phase1 when (-1)^(r(r+1)/2)=-1,
otherwise phase i. This is the full229376-dimensional one-form fibre,
not a special-unitary or vector/bivector restriction.
Primary12.26 and12.27 give commutator and i-anticommutator; in
particular {gamma0,gamma0}=2iI. The latter central direction is not removed.

The original first-action connection gradient in these conventions is

    F(S)=A+HS+gamma N(S)+kappa S,
    A=K(FB)=-(21/4)PHGamma-(15/4)PEGamma-(21/4)PtGamma,
    H=(K D_B+D_Bdag Kdag)/2,
    N(S)=[K(S wedge S)+DQ_Sdag Kdag S]/3,
    DQ_S[U]=S wedge U+U wedge S.

Here E denotes the nine-dimensional traceless vertical tangent and t its
trace line. H in PH is the four-dimensional horizontal tangent, not the
differential operator H. All couplings and geometry are fixed when taking
field derivatives. We use the real signed pairing
-ReTr(XY)/128 times the exterior metric, never a positive replacement.
The differential formal adjoint follows from compact support or specified
vanishing boundary terms. Invariant fields need not obey pointwise Green
identities on the noncompact homogeneous space. NO pointwise symmetry
<Pair(U,HV)>=<Pair(HU,V)> is assumed or used.

The original potential P(S)=Pair(S,K(S wedge S))/3 is algebraic at a point.
Its gradient is precisely N on the full nondegenerate real fibre.
Its symmetric polarization is

    Bcal(X,Y)=[N(X+Y)-N(X)-N(Y)]/2,
    Bcal(S,S)=N(S).

Unlike differential integration by parts, the algebraic symmetry of the
cubic trilinear form Pair(U,Bcal(X,Y)) is valid pointwise. It follows by
differentiating the ORIGINAL cubic polynomial in all three directions,
not by asserting positivity or replacing the full gradient by a scalar
restriction. This distinction is essential below.

## Independent full covariant implementation

Write Lambda_a for the passed618 frame matrices. The owned implementation
uses the spin generator

    Omega_a=sum_(b<c) sigma_c (Lambda_a)^b_c Gamma_bc/2,

so [Omega_a,gamma_b]=gamma(Lambda_a e_b). For any Clifford-valued p-form,

    (nabla_a T)_(b1...bp)
      =[Omega_a,T_(b1...bp)]
       -sum_i (Lambda_a)^c_bi T_(b1...c...bp).

The primary covector algorithm replaces an entry in an explicit ordered
list of exterior indices, rejects duplicates and counts the permutation
inversions. The Clifford part is the literal full Clifford commutator.
The independent route replaces every exterior and Clifford blade index
using bit masks and shuffle signs. Clifford metric-skew derivations act
on every exterior grade, so this comparison is not a grade truncation.

Form D_B T=sum_a theta_a wedge nabla_a T, apply all eight literal CAA
stages and the independent word-product stages, and separately form

    D_Bdag Kdag T=-sum_a sigma_a interior_a nabla_a(Kdag T).

All14 derivatives are retained. Since canonical solder/Hodge tensors are
parallel, an additional complete reverse comparison uses
nabla_a(Kdag T)=Kdag(nabla_a T). The latter calls the literal full
reverse operator on each derivative. Both reverse CAA legs are retained.
No kinetic output is projected before either comparison or averaging.

All14 input matrices are checked for metric skewness and their exact
plane coefficient sums:5/2 on axes0,7,8,9, zero on the other ten.
Both canonical Gamma derivatives vanish in the full slot action.
Each of four nonzero horizontal matrices gives two omission controls:
spin-only and covector-only derivatives of Gamma1 are nonzero, while
their complete sum is zero. These controls prevent freezing the moving
coframe or omitting the Clifford connection.

The complete source, J and N(J) are read from passed617/618/619 and
cross-checked with their independent retained oracles. No scalar
Einstein substitution or isolated grade-five coefficient constructs them.
The619 full tensor has614 entries:14 vector and600 grade-five entries.

## Complete invariant bivector carrier

At the homogeneous point let W=HdirectsumEdirectsumRt, where
E=Sym^2_0(H) is the space of tracefree self-adjoint endomorphisms.
The trace unit is t=-I/2, with norm-1. The horizontal metric is gH=-y.
The nondegenerate metric identifies W* with W.

We need invariants in W* tensor Lambda^2 W, not just an ansatz chosen for
small dimension. Lorentz invariant index contractions use the metric and,
for proper Lorentz transformations, possibly epsilon. Pairs of epsilons
reduce to metric contractions. A single epsilon has four Lorentz slots;
symmetric matrices cannot put both of their indices into that epsilon.

Metric contractions leave exactly three independent slot types:

    H -> H wedge E: gH(u,Av), giving J up to normalization;
    H -> H wedge Rt: gH(u,v), giving B;
    E -> E wedge Rt: tr(AB), giving C.

They are independent because their slot types differ. For E->Lambda^2H,
gH(u,Av) is symmetric in u,v, so antisymmetrization vanishes.
For E->Lambda^2E, tr(ABC) is symmetric in B,C: transpose reverses
the order and cyclicity completes the interchange. Trace factors vanish.
Input t with Lambda^2H or Lambda^2E likewise gives symmetric pairings.
All other types have an odd number of Lorentz indices, require an
invariant vector in H or a tracefree invariant vector in E, and vanish.

No epsilon adds an invariant. In H,H,E it meets both symmetric E indices.
In E,E or E,E,E, after any remaining metric contraction, at least one
symmetric matrix contributes both indices to epsilon. That contraction
vanishes. Odd-index types cannot be repaired by a rank-four epsilon.
Thus the COMPLETE invariant bivector one-form carrier is span{J,B,C}.
The six connected Lorentz generators are checked on all six finite
vector/bivector carrier fields at both points; the contraction proof
establishes completeness independently of that finite check.

Our normalization is

    J_u=-4 Spin(U_u), J_A=J_t=0,
    B_u=-gamma_u wedgeCl gamma_t/2, B_A=B_t=0,
    C_A=gamma_A wedgeCl gamma_t, C_u=C_t=0,
    L=-B+C.

The signed Gram matrix is diag(9,-1,-9), and the three fields are
orthogonal. Together with PHGamma,PEGamma,PtGamma,W5,central i theta0 I,
the eight action directions have norms(-4,-9,-1,9,-1,-9,1,1).
All28 off-diagonal pairings per point vanish. A positive coefficient
pairing gives+4 on PHGamma while the actual signed pairing gives-4:
the planted sign replacement must fail.

## Full kinetic forecasts, without pointwise self-adjointness

For hand derivation decompose Lambda_u=U_u+R_u. Put

    C0(u,v)=-(u tensor vflat+v tensor uflat)/4+gH(u,v)I/8,
    U_u v=C0(u,v), U_u A=Au/2, U_u t=0,
    R_u v=-gH(u,v)t/4, R_u t=-u/4, R_u A=0.

Every vertical Nomizu input is zero. The tracefree symmetric completeness
relations are

    sum_A sigma_A A^2=9I/4,
    sum_u sigma_u C0(u,u)=0,
    sum_u sigma_u C0(u,v)u=-9v/8.

Define the vector-action matrix rho(X) of a Clifford bivector X by
[X,gamma(v)]=gamma(rho(X)v). Inserting the completeness identities in
the FULL slot derivative gives the matrices

    rho((D_BJ)_uv)=-8[U_u,U_v],
    rho((D_BJ)_uA)=2U_(Au), rho((D_BJ)_ut)=-U_u.

These are matrix identities, not literal equalities between Clifford
elements and matrices. Separately, the vector-valued adjoint is

    (KdagJ)(u,A)=-2gamma(Au), all other pair types zero.

The first CAA contraction gives horizontal9/4, traceless-1, trace0.
Its total trace is4*(9/4)-9=0, so the scalar second contribution is zero.
The cyclic argument below kills the complete grade-five contribution.
Separately evaluating the three terms of the codifferential,

    -sum_u sigma_u [Lambda_u Y(u,b)
                  -Y(Lambda_u u,b)-Y(u,Lambda_u b)],

gives the same three weights. This is a direct derivative computation.

For clarity, the corresponding independently hand-derived full leg table
is included; it is not an input matrix used by the code:

    field     forward(H,E,t)       reverse(H,E,t)      average(H,E,t)
    J         (9/4,-1,0)           (9/4,-1,0)          (9/4,-1,0)
    B         (-9/4,-4,-3)         (3/4,0,1)           (-3/4,-2,-1)
    C         (27/2,16,18)         (0,-2,0)            (27/4,7,9)
    L=-B+C    (63/4,20,21)         (-3/4,-2,-1)        (15/2,9,10)

One obtains the B,C rows by the same displayed full slot derivative and
codifferential, using B_u=-4Spin(R_u) and C_A=gamma_A wedge gamma_t.
Their forward/reverse inequality is retained, not averaged away by an
assumed pointwise transpose identity. In particular Pair(PHGamma,HB)=3
whereas Pair(H(PHGamma),B)=-1. The difference is a Green divergence.

For diagonal V=a PHGamma+b PEGamma+c PtGamma, the full response is

    H(V)=(b-a)J+(c-a)L.

The trace contribution is essential: div(Pt)=t, and the full formula is
-2sum_a sigma_a gamma_a wedge gamma((nabla_a V)b)
-gamma(div V) wedge gamma_b. It yields H(PtGamma)=L, not just-B.
These identities imply HJ=(9/4,-1,0), HL=(15/2,9,10).
They hand-derive the six H forecasts used by the recursion. The executable
constructs each entire covariant result from618 matrices before comparison;
it does not call this table as an operator implementation.

## Full mixed polarization and the potential-only reflection

For diagonal V and cyclic bivector J, the full expansion is

    2 Bcal(V,J)=[K(V wedge J+J wedge V)
                +DQ_Vdag KdagJ+DQ_Jdag KdagV]/3.

The first input is vector-valued because [vector,bivector] is vector.
K maps vector two-forms purely to bivector one-forms: firstC has grade2,
innerA has grade3 and outerA returns grade2. The full diagonal KdagV is
bivector-valued. The passed619 identity

    W3(J)=sum_b sigma_b gamma_b wedgeCl J_b=0

makes the entire outer-A transpose2iW3(J) zero, and KdagJ purely vector.
The two remaining transpose terms are respectively vector/vector and
bivector/bivector commutators, hence bivector. This is a FULL grade proof
before invoking the complete invariant carrier.

Define a real Clifford automorphism that negates ONLY gamma_t and fixes
the other generators, and simultaneously negate theta_t on forms.
It preserves Clifford relations, dagger phases, normalized scalar trace
and the signed pairing. No Pin lift, physical gauge transformation or
symmetry of the differential geometry is assumed.
Gamma1 and Gamma2 are invariant under this combined map.
Because the isometry reverses orientation, star r=-r star. The first
CAA leg contains two Hodge factors and the second contains four, so both
complete legs, their algebraic transposes and the potential N are equivariant.
V,J are even, while B,C are odd. Thus the FULL mixed output lies along J.

The full cubic trilinear symmetry and619 vector weights(3,22/3,6) now give

    9*coefficient=Pair(V,N(J))=-12a-66b-6c,
    Bcal(V,J)=-(4a+22b+2c)J/3.

An independent literal-leg derivation supplies stricter prospective checks.
Writing Y=KdagJ, one has Y(u,A)=-2gamma(Au), Pair(Y,Y)=-36 and

    V wedge J+J wedge V=bY, KY=-4J,
    DQ_Vdag Y=-4bJ,
    DQ_Jdag KdagV=-4(2a+9b+c)J.

Thus the three basis rows have half-Q=bY/2, Khalf-Q=-2bJ and the two
displayed unhalved transpose contributions, not just a fitted final sum.
Their final half-polarization coefficients are(-4/3,-22/3,-2/3).
Doubling those coefficients is a nonzero normalization decoy in every row.
Reflection is checked separately on the full forward output and both
full field adjoints; it is NOT asserted for H.

For diagonal vectors X,Y the complete independently derived formula is

    Bcal(X,Y)_i=2[(tauX-x_i)(tauY-y_i)
                 -(sum_j x_j y_j-x_i y_i)].

It follows from matching form/Clifford pairs in Q, the full adjoint
KdagV_ij=2(v_i+v_j-tr V)Gamma_ij, and DQ_Vdag KdagV=2KQ(V).
No higher-grade term is silently discarded.

## Full S1 through S5 and the original equation residual

Let lambda=1/kappa and S=sum_(n>=1)lambda^n S_n, at fixed finite gamma.
The scaled equation is S+lambda A+lambda HS+lambda gamma N(S)=0.
Using the HALF polarization gives

    S1=-A,
    S2=-HS1,
    S3=-HS2-gamma N(S1),
    S4=-HS3-2gamma Bcal(S1,S2),
    S5=-HS4-gamma[2Bcal(S1,S3)+N(S2)].

The gamma coefficients are stored separately, never interpolated from
sampled coupling values. The complete forecasts, with vector triples
ordered(H,E,t), are

    S1=(21/4,15/4,21/4),
    S2=(3/2)J,
    S3=(-27/8,3/2,0)-gamma(11043/2,11655/2,11043/2),
    S4=(-39/8+420gamma)J-(27/8)L,
    S5_vector=(1161/32,51/2,135/4)
       +gamma(-1701,1275/2,-189/2)
       +gamma^2(15072318,15422130,15072318),
    S5_grade5=-(9gamma/4)N(J)_grade5.

Independent intermediate arithmetic is

    N(A)=(11043/2,11655/2,11043/2), HN(A)=306J,
    Bcal(A,J)=38J,
    X=-H^2A=(-27/8,3/2,0), tauX=0, sum_i A_i X_i=81/4,
    Bcal(A,X)=(-2997/8,117,-81/2),
    Bcal(A,N(A))=(-7536159,-7711065,-7536159).

Accordingly the actual recursion feedback rows N11,B12,B130,B131,N22
are N(A),-57J,(2997/8,-117,81/2),
(-7536159,-7711065,-7536159),(9/4)N(J).
Each entire tensor, half-Q, both field adjoints, both composite terms
and all literal/independent CAA stages is retained.

The grade-five conclusion needs ALL feedback, not just N(J).
J,B,C are cyclic: J by619; B,C have a repeated exterior/Clifford direction.
Covariantly differentiating W3(T)=0 with the parallel solder gives
W4(D_BT)=0. Therefore K D_BT has no grade5; KdagT is vector and
D_Bdag preserves Clifford grade. Thus HS4 is entirely vector-valued.
S1,S3 are diagonal vectors, so their full polarization is entirely vector.
Only N(S2) contributes at grade5. The executable still computes every
component of all three terms before making this grade comparison.

The complete600-term grade-five tensor is compared with the full passed619
tensor scaled by-9/4 in the gamma1 coefficient. Its allowed witness
W5=theta3 Gamma02347 has signed norm+1 and coefficient-3/4.
That witness alone is not used as a substitute for the other599 entries.

Separately, ordered convolution constructs every Q and DQ-adjoint term
for lambda orders2,3,4 and the available gamma orders. There are eight
ordered summands per point. These are constructed directly from the S_n
arrays, not by summing cached B results as the implementation.
They agree with the symmetric recursion, and all original-equation
lambda coefficients0..4, each at gamma powers0..2, must be the zero FULL
tensor. No HS5, S6 or order-lambda5 residual is claimed.

The diagnostic menu gamma=-2,-1,0,1,2 evaluates the exact polynomials
after their coefficientwise construction. It does not fit them.
For each nonzero gamma, discarding grade5 from S5 leaves a full600-term
original lambda4 residual with witness+3gamma/4. The gamma0 specialization
does not support a nonlinear-grade-five conclusion.

## Original cubic-action derivative controls

For every one of the eight feedback pairs X,Y per point and all eight
declared real H-anti directions U, independently construct the coefficient
of r*s*t in the ORIGINAL potential P(rX+sY+tU):

    [Pair(U,K(X wedge Y+Y wedge X))
     +Pair(X,K(U wedge Y+Y wedge U))
     +Pair(Y,K(U wedge X+X wedge U))]/3.

It must equal2 Pair(U,Bcal(X,Y)). The product rule varies every occurrence
of the field, including both inputs of Q. The two new cross-Q inputs
are also rebuilt with the independent word kernel before all eight CAA
stages are compared and retained. Every scalar contribution is retained.
The same coefficient divided by2 is the symmetric cubic trilinear form.
The finite directions supplement the full tensor/grade/support proofs;
they do not span the original229376-dimensional field space.
In particular the W5 direction on N22 gives the nonzero nonlinear
derivative, while the central and signed-metric controls retain the actual
source domain and indefinite pairing.

## Conditional analytic-branch consequence

The following is a new analytic deduction, NOT retroactively executed618
scope. Let V be the FULL finite-dimensional isotropy-fixed field space.
618 proves H:V->V, N:V->V, with auxiliary coefficient-l1 bounds
a=||A||=60, ||H||<=51520 and
||Bcal(U,V)||<=2576||U||||V||. This norm is only an existence norm.

Fix any finite real gamma. Complexify V and its polynomial maps.
The same bounds hold with absolute complex coefficients. Choose delta>0
with delta h<=1/4 and |gamma|ca delta^2<=1/16.
On the fixed complex ball of radius r=2a delta, for every |lambda|<=delta,
T_lambda(S)=-lambda A-lambda HS-lambda gamma N(S)
maps the ball into itself with image/r<=7/8 and Lipschitz constant<=1/2.
Starting from0, the polynomial iterates are holomorphic in lambda and
converge uniformly on the disk by the same geometric estimate. Their
limit is holomorphic in its interior, solves the full equation, and has
the real618 fixed point for real lambda by conjugation and uniqueness.
Equivalently the derivative of the scaled equation with respect to S at
(lambda,S)=(0,0) is the identity. No invertibility of the signed invariant
Gram matrix or pointwise differential transpose is needed.

Therefore the displayed formal coefficients are the actual Taylor
coefficients of that conditional branch. For fixed gamma!=0, its W5
component is -(3gamma/4)lambda^5+O(lambda^6). Dividing by lambda^5 has
nonzero limit, so this component is nonzero for every sufficiently small
real nonzero lambda. This does NOT supply a numerically optimized radius
or a uniform nonvanishing radius as gamma approaches0.
No numerical field solve is performed here.

The conclusion concerns this full invariant connection branch under the
declared local geometry/operator. It does not select a physical background,
source coupling, observed-field projection, global spinor descent,
unsubtracted total action, momentum-space pole, mass, units or stability.
The local native joint-stationarity synthesis from620/621 is separate;
no metric/epsilon Hessian or physical spectrum is calculated in this audit.

## Finite census and complete expanded evidence

The full fixture is independently duplicated in Program and contract,
with exact deep equality and tolerance0. There are49 counter fields.
Known controls:4 rational,507904 word signs(31 per16384 blades),
16384 Hodge squares,16384 full-domain masks,16384 orientation-reversing
Hodge checks and2 central controls.

Two points give6 complete input checks,5488 metric entries,28 matrix
support/plane-sum rows,56 canonical parallel checks and16 omission decoys.
The eight directions give16 norms and56 orthogonality checks.
Six carrier directions times six Lorentz generators times two points give72
isotropy checks;12 carrier and4 canonical reflection checks follow.
Two cyclic rows each test J,B,C;2 signed-metric decoys distinguish the
positive coefficient pairing.

Six H calls per point give12 kinetic rows,168 derivative comparisons,
168 complete reverse comparisons,96 stage comparisons,12 simplified
adjoint checks and12 full-type aggregate checks.
Eight feedback calls per point give16 product/oracle rows,32 adjoint
comparisons,128 stage comparisons,16 forward and32 adjoint reflection
comparisons. Three mixed basis rows per point give24 separate literal-leg
controls and6 nonzero half-polarization decoys.

All five S orders times three gamma powers at two points give30 full
coefficient and30 grade rows. Four ordered-convolution rows per point
give8 rows,16 ordered summands and64 stage comparisons. The original
equation gives30 full zero residual coefficients. There are2 complete
grade-five rows,50 exact coupling/order evaluations,8 projected-S5
nonzero decoys and15 transported coefficient comparisons.

Eight feedback pairs times eight action directions times two points give
128 original cubic derivative and128 trilinear rows. Two cross-Q inputs
per row give256 independent word-product checks. Two eight-stage chains
per row give2048 primary/independent stage comparisons. BOTH sets of
stage coefficients remain present, not only comparison booleans.

Exactly two deterministic point shards hold complete expanded tensors,
one per point. Each point has14 matrix,8 direction,6 isotropy,6 kinetic,
8 feedback,15 coefficient,4 convolution,15 residual,25 evaluation,
4 projection and64 original-action rows. Full and summary are identical
small manifests containing both paths, SHA256 values, byte counts, totals
and all49 counts. There is no DAG, omitted grade, fitted coefficient,
external reconstruction service or unexecuted sibling dependency.

## Resource bounds and preserved first failures

All coefficients are exact arbitrary-precision Gaussian rationals.
Scalar dimensions are14; no nonzero Fourier frequency is generated.
The tensor ceiling65536 includes partial W-product accumulations.
The matrices have ten nonzero metric-skew planes per horizontal input.
Vector/bivector H inputs remain within14 vector and49 bivector terms.
Their full derivatives lie within196 vector or1274 bivector one-form slots.
Their exterior derivatives lie within1274 vector or8281 bivector two-form
slots. Full K on the latter has grades1/5, at most14*(14+2002)=28224
one-form positions. Full reverse cyclic adjoints are vector two-forms;
diagonal vector adjoints are matching bivector two-forms.

Raw W products are bounded BEFORE commutator cancellation: the largest
finite recursive factor has36 terms, so an individual product has at most
1296 candidates and a cross sum at most2592. Ordered convolution adds
at most2*14^2+36^2=1688 candidates. These bounds cover temporary
scalar/grade4 terms of the separate W products, not just final grades.

For action W5 controls, a cross product with a vector has grade6 and
with a bivector has grade5. The grade6 input can produce inner grades4/8
and outer grades5/9; the grade5 input can produce inner grades3/7 and
outer grades2/6. These higher action-probe grades are INCLUDED in the
following sparse candidate bound. The finite input has at most72 candidates.
For a fixed two-form input term, firstC has at most two external choices,
innerA exactly one, and outerA at most14. Thus even the generous sparse
first-plus-outer support bound16*72=1152 is below65536.
All other action directions return the preceding vector/bivector bounds.
C/A primitives combine their signed Clifford factors before insertion.

For coefficient size, the universal a60,h51520,c2576 recurrence bounds
all coefficients and original cubic scalar contributions in this finite
menu by10^22 in magnitude, including the exact integer gamma menu.
Source/nomizu denominators divide powers of2 (quarters/eighths);
each H adds at most six factors2, and each B adds at most two factors2
and one factor3. Four linear recursions and at most two nonlinear factors
give a conservative denominator bound2^30*3^6, also covering retained
frame/isotropy inputs. Hence numerator magnitude is below10^34 and a
canonical rational string, including sign and slash, fits64 characters.
The serializer enforces this cap on all rational strings and verifies
canonical reduced tensor records, sorted masks, uniqueness and zero omission.

The instrumented grouped-product ceiling is500M, and the explicitly
instrumented slot-multiplication ceiling100M. These are acceptance limits,
not counts of every rational/word operation or interrupting watchdogs.
The CPU180s/maximum estimate600s and peak512MiB/maximum estimate2GiB
are engineering budgets, not measured results or executable RSS/time gates.

For storage, full primary AND independent evidence is intentionally
duplicated. A dense carrier-support/text envelope is looser than the
64MiB per-shard operational limit; that limit is not advertised as a
mathematical guarantee that every possible partial result fits.
The code checks the actual serialized byte size BEFORE writing:
each point<=64MiB, aggregate<=128MiB. It also checks all rational lengths.
The immutable sparse passed inputs and finite49-field census bound the
number of tensor arrays and their candidate supports as above; no
unbounded iteration or enlarged field basis can occur. Full serialized
allocation precedes the byte test, so that test is a disk-writing guard,
not a memory watchdog. Estimates are explicitly distinguished from
enforced dimensions, tensor counters and write ceilings.

A resource rejection retains previously written shards and emits their
partial manifest/counts together with every observed scientific control
boolean. If a scientific comparison has already failed, the earliest
scientific failure has precedence over the resource terminal.
Incomplete counts are NOT themselves declared a census failure.
No oversized shard is partially written; no output is deleted or truncated
to turn failure into success. There is no post-run retuning.

The read-only --verify-evidence mode performs frozen preflight before
reading outputs, and its Emit guard forbids writes on every preflight
failure. It checks identical manifests, full metadata/firewalls/bindings,
exact two-file path set(no missing/extra/duplicate), SHA/bytes/single EOF,
all point/derivative/stage array counts, canonical complete tensor records,
all49 counts and aggregate/max byte totals. Since all tensors are expanded,
it is a validation of retained bytes, not a second scientific computation
or a DAG replay. The normal success path also rejects extra shard files.

## Frozen provenance and final claim firewall

There are38 unique exact bindings: own Program/helper/project/STUDY4;
600 three compiled helpers plus Program/STUDY/contract/passed summary7;
611,617,618,619 each helper/Program/project/STUDY/contract/passed summary6;
primary draft/core manifest/Directory.Build.props3.
All seven compiled sources(two owned plus five explicit includes) must
be present exactly once in those bindings. The program enumerates actual
SDK-owned .cs files excluding bin/obj and resolves every explicit Compile
Include, rejecting added or unbound compiled files. It does not merely
assume the project file's visible list is complete.

All upstream terminals, actual contract hashes, pass booleans,14 false
authority flags, pending external review and zero physical claims are
checked. Live core closure compares the exact726 sorted paths, every
SHA256 and the aggregate tree digest. Contract/schema/fixture/binding/
compiled closure and primary source anchors must pass before arithmetic.

Precedence is invalid/drifted input; known answer/domain; passed input/
carrier; full covariant kinetic; full polarized feedback; full fifth-order
recursion; original cubic action; nonzero omission controls; resource/census;
success. All required booleans and counts must pass.

Every text file has exactly one final newline. Bound files cannot be
repaired after FIRST execution; a failed first artifact is preserved.
Only the unbound implementation note may receive authorized result updates.
All14flags remain false, externalReviewPending=true,
promotedPhysicalMassClaimCount=0. O4 remains pending, Phase561 closed,
source deficits WZ15/H14 unchanged. No registered action, physical norm,
physical operator, source intent, scale or spectrum is selected.
