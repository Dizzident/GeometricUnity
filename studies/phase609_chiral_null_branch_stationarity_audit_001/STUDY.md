# Phase609: constructive chiral null-branch stationarity

Prospective A58 audit. Freeze this proof, full code/helpers/project, fixture
menu, counts, resources, precedence and exact bindings before any scientific
execution. Independent and MAIN reviews plus explicit MAIN approval are
required. Success means
`chiral-null-branch-controls-pass-full-stationarity-conditional`.

## 1. Declared action and limits

Use the fixed oriented flat unit-volume14-torus, positive axes0–6 and
negative axes7–13, epsilon=I, and the real H-anti-Hermitian matrix algebra
u(64,64). Its real nondegenerate bilinear pairing is -ReTr/128 times the
signed exterior metric. This is not a selected positive source norm.
Omega=gamma0...gamma13 has square1, anticommutes with odd Clifford elements,
and commutes with even elements. H=gamma7...gamma13 fixes the same real
form and Clifford conventions as the immutable600 helpers.

Write P=1+hOmega, h=-1,+1. Freeze the matched candidate tensors

    Phi1=P sum_a theta^a gamma_a,
    Phi2=(c-i h Omega) sum_(a<b) theta^ab Gamma_ab,
    C(X,Y)=XY-YX, A(X,Y)=i(XY+YX).

K is the FULL lowered CCA chain

    K(F)=star13(C_Phi1 star2(F)
            -(1/2)star1 C_Phi1 star14 A_Phi2 star2(F)).

The routes have degrees2->12->13->1 and2->12->14->0->1->13->1.
Every coefficient slot retains the appropriate complete route; c is formal,
not fitted from finite c values. The common coefficient1/2 is retained.

For fixed background B, curvature FB=dB+B wedge B, and Q(S)=S wedge S,
the action density being audited is

    ell(S)=<S,KFB> + <S,K D_B S>/2
           +gamma <S,K Q(S)>/3 + kappa <S,S>/2.

Gamma1 and2 are the declared two readings of the printed cubic symbol;
they do not change the geometric curvature definition. Kappa is constant
and nonzero for the primary branch, with -1,+1 finite controls. Kappa0
is tested separately and never divided by. The mass coefficient is exactly
kappa/2 in this action. The compact versus expanded Eq9.4 mass-factor
ambiguity remains unresolved; no author normalization is adjudicated.

The exact-bound primary draft is
`docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`.
Sections8–9 and Eq9.3–9.4 supply the typed tensors/brackets and action
motivation. Eq9.7 is NOT used as the actual-gradient definition: the
independently bound593 audit already shows its shortcut need not be the
variation of this full candidate action away from special states. Eq9.11
does not select the norm used here. Section3.4/Eq3.17 and the bound607
proof prevent identifying a flat frame with the actual induced upstairs
Levi-Civita geometry. This study's B is a curved even spin CONTROL on
that fixed frame, explicitly not the induced Levi-Civita connection of608.

## 2. Full real null space, not a restricted equation

Let A_odd be the real H-anti odd coefficient space and

    L_h = P A_odd,     Pi_h=P/2 on A_odd.

For odd X, XP=(1-hOmega)X, so (PX)(PY)=0 for any odd X,Y.
This is a square-zero subspace, not a claim that L is a two-sided ideal
of the entire Clifford algebra. Left/right multiplication by EVEN elements
preserves the algebraic complex space P Cl_odd. Individual matrix products
need not be H-anti. Their real coefficient-C or i-anticommutator combinations
are H-anti and hence lie in its real part L when the image is odd. In
particular the real trace pairing of any two L elements is zero. Even and
odd coefficients are mutually trace-orthogonal.

The H-adjoint satisfies Omega^sharp=-Omega. For X^sharp=-X odd,
(Omega X)^sharp=X Omega=-Omega X. Thus left Omega preserves the REAL
H-anti odd space, although it does not generally preserve the real even
space. Its square is1, and Pi_h is a genuine real projector on the odd
space. No physical chiral projection is selected by these facts.

A real H-anti blade representative is gamma_m if
(-1)^(r(r+1)/2)=-1 and i gamma_m otherwise, r=degree(m).
These16384 representatives have diagonal, nonzero real trace Gram entries.
The odd subspace has dimension8192. Left Omega pairs each odd mask with
its complement, without fixed masks. Choosing4096 unordered complement
pairs gives two4096-dimensional eigenspaces. If X represents one pair,

    <PX,PX>=0, <Pbar X,Pbar X>=0,
    <PX,Pbar X>=2<X,X> != 0.

Different unordered blade/complement pairs are orthogonal by Clifford
trace support. Therefore L is maximal isotropic within the odd space,
and its annihilator within that nondegenerate odd space is itself.
This proof, not a quadratic norm test on the sparse fixture, establishes
the dimension and annihilator claims.

## 3. Literal K and full-transpose image theorem

Phi1 is P-odd, Phi2 is even. For an even input F, the first C product
is P times odd coefficients; the inner A product is even and the outer
C product again lies in L. Thus K(even) is in L. For odd F the output
is even by parity. For F in L, each product with even Phi2 remains in
P Cl_odd; the real inner A combination is in L. Each outer product of
P-odd with that image is zero. Both routes vanish:

    K(L-valued two-forms)=0.

An Omega factor cannot be moved across an odd coefficient without a minus
sign. For example {Omega Gamma12,Pgamma3}=P Omega[Gamma12,gamma3]=0,
whereas {Gamma12,Pgamma3}=2P Gamma123. The proof uses preservation of
P Cl_odd under even multiplication and real closure of the C/A combinations,
not an invalid identical factorization of both orders through odd matrices.

Kdagger is the real bilinear pointwise transpose between the full one-
and two-form carriers. It is not a positive-dagger operator. Nondegeneracy
and the preceding image facts give

    Kdagger(L)=0,
    Kdagger(even) in L,     Kdagger(odd) even.

Indeed pairing any L one-form with K(even) vanishes by nullness and with
K(odd) by parity, proving Kdagger(L)=0 on the FULL input space. For even
Y, KdaggerY is odd and annihilates L because K(L)=0; maximal isotropy
places it in L. This fixes adjoint chirality without guessing a transpose
sign. The immutable600 helper implements both literal reversed typed
composition and independently simplified reversed composition, retaining
the real bilinear i-anticommutator transpose and all signed Hodge factors.

The theorem assumes B preserves parity and Omega, as an even spin connection
does in this frame. It does not claim D_B L is contained in a fixed L for
an arbitrary full non-Spin B. A gauge-conjugated description must transport
Omega, L, Phi1/Phi2 and B together; holding Omega fixed while conjugating
only B is not the same assertion. No separate gauge-orbit study runs here.

## 4. Actual full gradient and pointwise stationarity

The exact derivative DQ_S[V]=S wedge V+V wedge S is the full derivative,
not half of it. With real formal adjoints, the actual gradient is

    G=KFB + (K D_B S + D_Bdagger Kdagger S)/2
       +gamma (KQ(S)+DQ_Sdagger Kdagger S)/3 + kappa S.

D_Bdagger differentiates its entire argument, including any position
dependence of Kdagger. Nothing assumes K commutes with D_B. Our constant
fixture uses Ddagger plus the full coefficient-C exterior transpose for
the B term. A constant noncentral control and a central Fourier derivative
control independently check that covariant transpose with nonzero pairings.

For an even spin curvature FB and nonzero kappa, define

    S*=-K(FB)/kappa.

Then S* is real H-anti and in L. Since B preserves L, D_B S* lies in L,
and Q(S*)=0 by the coefficient square-zero law. Thus K D_B S*=0 and
Kdagger S*=0, and the FULL gradient reduces to KFB+kappa S*=0.
This does not merely test directions within L and does not require S*
to be parallel. Both displayed source-curvature residual coefficients also
vanish: K(FB+D_B S*+beta Q(S*))+kappa S*=0 for any beta, because Q=0.
That common zero does not repair the general off-branch variational mismatch
of593 or select its printed normalization.

There is a stronger local statement. For arbitrary H-anti value variation V
and arbitrary first derivative jet dV, the direct density variation is

    <V,KFB> + (<V,K D_B S>+<S,K D_B V>)/2
    +gamma (<V,KQ>+<S,K DQ_S[V]>)/3 + kappa<V,S>.

At S*, K D_B S=0, Q=0, and every derivative/interaction term with S
on the left is zero POINTWISE by Kdagger S=0. The remaining algebraic
term is <V,KFB+kappa S>=0. No integration by parts or boundary cancellation
is used in this statement; unrestricted value and derivative jets vanish.

## 5. Sparse curved spin fixture and independent scalar oracles

All indices in this fixture are positive. Set

    B=theta0 Gamma01+theta1 Gamma12+theta2 Gamma23.

It is real H-anti and even, but is not the LC connection of the flat frame.
Direct Clifford commutators give

    FB=2theta01 Gamma02+2theta12 Gamma13,
    KFB=-4P(theta1 gamma2+theta2 gamma3),
    S*=4P(theta1 gamma2+theta2 gamma3)/kappa,
    D_B S*=8theta12 Pgamma3/kappa != 0.

The KFB formula follows independently from the bound593 exact full-chain
anchor and an index relabeling; both curvature inner A contractions are
zero because the two bivectors share exactly one index. This formula is
independent of c, but c is NOT discarded from the operator or its adjoint.
On the nonzero D_B S*, the inner A top-form coefficient is

    c0: 0,     c1: 16i P Gamma123 volume/kappa.

Its two nonzero Clifford coefficients are retained and checked before the
outer C product vanishes. This catches an implementation that simply
drops the inner leg or assumes a parallel field.

For V=theta1 gamma2, separate first-variation pieces are

    curvature +4, kinetic 0, cubic 0, mass -4.

An independent forward action builds Q, D_B S, literal ordered-word K,
pairing and a formal t polynomial at S*+tV. It gives

    ell(S*+tV)=-kappa t^2/2.

The cubic polynomial is zero by coefficient/trace support, not because
Q(S*+tV) vanishes: its linear coefficient is
(8/kappa)theta12 Gamma23. K maps that to a theta1 Pgamma3 term, trace-
orthogonal to all theta1 gamma2 coefficients in S*+tV. The kinetic
polynomial vanishes by odd/even trace parity. Curvature gives4t; mass
gives-4t-kappa t^2/2. Gamma and c thus drop out for proved reasons.

For the wrong sign -S*, the scalar action value is still0, but the FULL
gradient is2KFB, pairing8 with V. The independent polynomial is
8t-kappa t^2/2. Its trace gradient-square is zero despite a nonzero field
equation. The positive coefficient diagnostic of that gradient is256,
versus64 for KFB and64/kappa^2 for S*. These are not physical norm choices.

At kappa0 use S=0, not a divided expression. G=KFB, its V pairing is4,
and the independent action polynomial is4t. No nonzero-kappa branch is
silently continued through a singular division.

## 6. Operator/support controls and independently predicted counts

Real-basis rows cover all16384 blades;8192 odd rows additionally check
Omega reality/involution and the sign reversal of trace square. Hodge
square controls cover all16384 masks. The old44944 word comparisons cover
all ordered pairs of grades0,1,2,12,13,14. New independent ordered-word
tests cover every blade times every one of14 generators in BOTH orders:
16384*14*2=458752 comparisons. Omega both orders adds32768 comparisons,
counted separately rather than double-counted inside the real-basis rows.
The bit-mask shuffle/sign formula and ordered-word sorting/reduction are
independent implementations; their Clifford-generator law extends through
word composition. No new grade is justified only by an image assertion.

The bounded operator menu has all cyclic contiguous masks of lengths1..13
on14 axes plus0/full:184 unique masks,98 odd. It uses K input planes01,
07,78 and Kdagger output axes0,7,13, both h and both c slots. Therefore
each operator has2208 generic parity/image checks and1176 L-null checks.
This is not a full91-form-times16384-blade enumeration. The general proof
above establishes its stated universal scope; the battery challenges each
grade, both signature signs and exterior placements independently.

For a support-complete transpose reconstruction, take Y=theta1 gamma2.
Write C_Y=sum_a sigma_a[Pgamma_a,Y_a]=2Gamma12. Every possible Kdagger
coefficient is on a two-form ab, with blade mask ab XOR bit1 XOR bit2,
or its Omega complement. First-route support with a form containing1 is
a subset of this182-position set. The second route is obtained from C_Y
and Phi2 and has exactly the same mask rule. This structurally excludes
ALL other positions before any expected coefficient is used.

The first constant adjoint route has12 ordinary bivectors, on forms1j
with j!=1,2, plus -2hOmega on form12. Its second constant route is
-h{Omega Gamma_ab,Gamma12}:66 disjoint pairs give nonzero Omega-grade4
outputs and form12 gives+2hOmega, cancelling the first overlap. Exactly
78 positions remain. The c route is -i{Gamma_ab,Gamma12}:66 disjoint
i-grade4 outputs and form12's+2iI, for67 positions. Their real H-anti
phases are consistent: grades2/10 real, grades0/4 multiplied by i.

For all182 candidates, independently apply ordered-word `NaiveChain` to
the real input basis X, pair with Y, and reconstruct the coefficient by
dividing by the known nonzero real basis norm. All728 probes over both h
and slots equal the full literal-adjoint pairings and reconstruct the
entire adjoint. Exactly290 probes are nonzero. Coefficient square diagnostics
are312 and268, from78 and67 entries of absolute coefficient2. The structural
support proof is essential; probes on a guessed support alone would not
certify that coefficients outside it are absent.

The covariant transpose controls have nonzero pairings:
U=theta1 gamma2,Y=theta12 gamma3 gives D_B U=2Y and D_BdaggerY=2U,
with both pairings-2. For U=i theta1 I sinx0,Y=i theta01 I cosx0,
centrality removes B commutators and both derivative pairings are1/2.
Only this explicitly periodic positive control uses a period average.
All pointwise-density/metric/branch rows have constant Fourier support,
and the local-jet routine asserts form degree, H-anti reality and zero
frequencies before taking a pairing.

There are8 primary contexts h±,kappa±1,gamma1/2;8 wrong-sign contexts;
4 massless contexts. All actual6-gradient legs, independent4-variation
legs, original-action polynomials and intermediate c slots are checked.
Four independently declared value/derivative jets per context give64
coefficient-slot tests, including even/odd derivative jets not generated
by the variation's value. Arbitrary such local jets are realizable at a
point; they are not asserted to be global constant derivatives on a torus.
The exact full smaller counts are in the executable fixture and contract.

## 7. Metric/background identification: conditional density theorem

Fix a smooth local family of compatible metric/spin-field identifications
in an orientation/signature component. In an orthonormal spin trivialization
the Clifford matrices, H, Omega and P are fixed, while coframes, Hodge maps,
canonical Phi tensors and density vary consistently. For any even B preserving
the transported Omega and ANY L-valued S, the entire declared density is
identically zero: KFB lies in L, K D_B S=0, Q=0, and <S,S>=0.

In that identification its partial metric variation at fixed L-valued
coordinate S is zero. A general smooth identification J_g changes the
partial metric variation by the field variation induced by delta J_g.
At S*, section4 proves that induced field variation vanishes pointwise
for unrestricted value and derivative jets, so no field-chain term remains.
Equivalently, differentiating the identically zero branch density along
S*(g)=-K_g F_B(g)/kappa gives a zero partial metric variation after the
already pointwise-zero field term is subtracted.

An even delta B contributes delta FB=D_B delta B (even), and
delta(D_B S)=[delta B,S] (in L); its density variation also vanishes
pointwise by the same image/null laws. This accounts for the fact that an
induced B(g) may contain metric derivatives. No claim here is based solely
on a zero integrated action or on variations restricted to L. No integration
by parts is required for this LOCAL DENSITY theorem, including its field
identification term. If only an integrated Euler equation had been known,
boundary terms would instead require separate treatment.

The theorem is conditional on smooth admissible identifications, the full
declared action and real carrier, nonzero kappa and even/transported B.
Global spin/field admissibility, a variational functional domain on possibly
noncompact fibers, allowed source metric variations, integrability and
physical stability are NOT established by a local identity. A fixed-frame
even non-LC control does not itself instantiate the source's metric-induced
branch. Nor is an unselected Eq9.11 norm or a physical propagator supplied.

Bounded metric tests hold coordinate B and FB fixed. Use coframe jets
theta'^a=(1+t m_a)theta^a, with either every m_a=1 (homothety) or m_0=1
only. Clifford matrices/P remain fixed. On a coordinate p-form component I,

    delta star_p = (sum_a m_a-2 sum_(a in I)m_a) star_p,
    delta Phi = (sum_(a in form)m_a) Phi.

The coordinate pairing density has the same volume/inverse-form multiplier.
The helper differentiates EACH star in the typed chain, both Phi occurrences,
both pairing inputs and that density factor; it does not add density twice.
Independent nonzero anchors are

    homothety: deltaKFB=-KFB, deltaS*=-S*,
    axis0: deltaKFB=4theta1 Pgamma2,
           deltaS*=-4theta1 Pgamma2/kappa.

Each metric row evaluates the literal density derivative both with S fixed
and with the recomputed candidate deltaS*=-deltaKFB/kappa. All four action
pieces vanish separately. There are16 rows/32 c-slot checks, with16
nonzero branch derivatives, so a zero metric-derivative routine fails.

A separate even background jet deltaB=B gives deltaFB=2FB, partial
delta(D_B S*)=D_B S*, and recomputed deltaS*=2S*. It is tested with
the original B curvature and action products, in8 rows/16 slots. Finally,
the identification variation [gamma2,S*] is the nonzero EVEN field
-8h theta1 Omega/kappa+8theta2 Gamma23/kappa, outside L. All of its
first-variation pieces vanish in8 rows/16 slots. These finite controls
challenge the general jet proof; they do not replace that proof with an
empirical claim about all metric directions.

## 8. Freeze, resources and authority

Twenty unique file bindings cover own Program/project/proof/control helper;
three explicitly compiled immutable600 arithmetic/Fourier/adjoint helpers;
600 passed summary/contract/Program;593 summary/contract/Program/proof;
607 summary/contract/proof; primary text; complete core manifest; and
Directory.Build.props. No608 result is required, no upstream source is
modified, and no shared/core file is edited. The exact-bound manifest is
independently compared with all726 live sorted src .cs/.csproj paths and
hashes excluding bin/obj, and its combined tree hash.

Before arithmetic, full fixture JSON equality, terminal precedence, every
binding ID/path/hash, upstream pass evidence/contract hashes and all14
authority flags are checked. Both artifacts are deterministic byte-identical
JSON. Tolerance is exactly0; BigInteger rationals have no coefficient
overflow. No floating-point parameter fitting, sampling or preliminary
scientific run is authorized.

Estimated CPU15 seconds, planning ceiling90 seconds; expected peak128MiB,
planning allowance512MiB. The operation/image/word loops are explicitly
bounded above. `CoefficientProducts` counts optimized `Product` coefficient
products ONLY, not `NaiveProduct`, ordered-word tests or all scalar work;
its enforced ceiling is50000000. The sparse tensor ceiling is65536; no
fixture changes support adaptively. The forward original-action polynomial
has degree at most3 by construction. Resource estimates are not measured
wall-time or memory gates. All-grade word controls are included in the CPU
estimate but not mislabeled as instrumented coefficient products.

Precedence is input drift, known-answer failure, null-image failure,
full-adjoint failure, full-stationarity failure, pointwise-variation failure,
metric-density failure, decoy/census/resource failure, then success. Preserve
any failed output. A scientific repair requires a new version and review;
never rewrite frozen inputs silently. Only the unbound implementation note
may receive factual approved results after execution.

All14 authority flags remain false, O4/external review pending, Phase561
closed and physical mass claims0. No Hessian calculation, propagator,
physical vacuum, global functional domain, source normalization or source
second-action norm is claimed by this audit.
