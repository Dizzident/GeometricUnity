# Phase596: fixed curvature operator Helmholtz audit

Prospective A51 exact, zero-sampling audit. No scientific execution is permitted
until this complete pack is frozen, reviewed independently and approved by the
main coordinator. This is a conditional classification of variationally
compatible fixed operators, not a choice of the missing source operator.

## Question and hypotheses

Fix a nondegenerate real trace/form pairing, background and volume density.
Allow unrestricted bosonic connection variations and their independent local
first jets. Let K be zero-order, linear in curvature and independent of the
varied connection A. Ask when the curvature-only covector E(A)=K(F_A),
F_A=dA+A wedge A, can be an Euler derivative. All displayed L coefficients
below are LOWERED using the pairing; a raised output K requires the inverse
Gram matrix. This distinction matters for indefinite u(H).

The finite audit uses three positive spatial axes and the full real u(1,1)
carrier, not a restricted three-coordinate pullback. There are 12 independent
connection components and 12 independent curvature components. A separate
written theorem concerns u(64,64) in fourteen dimensions, and additionally
requires a Spin-invariant trace/form pairing and a genuinely Spin-intertwining
K. The computation is not a coefficient census of u(64,64).

Compactly supported local variations or periodic variations remove boundary
terms. With boundaries, boundary conditions and boundary action terms must
be specified separately; they do not alter a failed interior necessary
identity. Fermionic signs, constrained fields, A-dependent or differential
operators, and additional background tensors are outside this classification.

## Independent principal-symbol proof

Write, with L antisymmetric in its last two spatial indices,

    E_ia = sum_(j<k,b) L_ia,jkb F_jk^b
         = (1/2) sum_(j,k,b) L_ia,jkb F_jk^b.

The derivative part of DE is M^j_ia,kb partial_j, M^j_ia,kb=L_ia,jkb.
Formal self-adjointness of this first-order operator requires M^j=-(M^j)^T,
because integration by parts reverses the derivative sign. Thus

    L_ia,jkb = -L_kb,jia = L_kb,ija.

Apply this spatial cyclic relation three times. Each application exchanges
the two Lie labels; three applications return the spatial indices and exchange
a,b. Hence L_ia,jkb=L_ib,jka. The Lie pair is symmetric, and then spatial
cyclicity together with j,k antisymmetry makes all three spatial slots totally
antisymmetric. Conversely this tensor symmetry gives exactly M^j=-(M^j)^T.
The pointwise principal kernel is therefore

    Lambda^3(V*) tensor Sym^2(g*).

In dimension three L_ia,jkb=epsilon_ijk h_ab, h symmetric. The complete 144
variables have 10 independent solutions, so principal rank is 134. This is
an analytic dimension proof before elimination, not an inference from an
observed rank. The 234 programmed rows are three derivative axes times the
78 upper-triangular entries of a symmetric 12-by-12 matrix, including its
diagonal. All ten symmetric h basis lifts must annihilate those rows and be
independent; every computed kernel vector must reconstruct from its h slice.

## Cubic proof, matrix basis and independently predicted ranks

In u(1,1), H=diag(1,-1), choose

    e0=i I, e1=sigma1, e2=sigma2, e3=i sigma3,
    b(X,Y)=-ReTr(XY)/2, G=diag(1,-1,-1,1).

Literal Gaussian-rational 2-by-2 matrix multiplication independently checks
all 16 ordered brackets, 16 Gram entries, four H-anti-Hermitian conditions
and all 64 ordered Jacobi triples. The nonzero independent brackets are

    [e1,e2]=2e3, [e2,e3]=-2e1, [e3,e1]=-2e2.

Once the principal condition holds, symmetry of the derivative of the
quadratic term K(A wedge A), for independent values of A on any three
distinct spatial axes, requires and is implied by

    h([Z,X],Y)+h(X,[Z,Y])=0.

For example the output-i derivative in direction A_j tests
h(X,[Y,Z]); exchanging output and variation gives the same condition after
using antisymmetry in the spatial slots. These conditions say that h is an
ad-invariant symmetric bilinear form. Conversely invariance makes
h(X,[Y,Z]) fully alternating. Contracting it with the alternating spatial
three-tensor gives a completely symmetric cubic coefficient in the combined
connection slots, so its one-third Euler contraction is a potential.

The reduced code imposes four generators times ten symmetric Lie pairs,
40 rows on the ten h coefficients. Directly from the brackets, the eight
independent equations are

    h01=h02=h03=h12=h13=h23=0,
    h11+h33=0, h22+h33=0.

To see necessity without elimination: invariance with a central slot and
the three nonzero brackets kills h01,h02,h03. Invariance with a repeated
noncentral slot kills h12,h13,h23. The triples (e1,e2,e3) and (e2,e1,e3)
give the two diagonal relations. Substitution into every bracket verifies
sufficiency. Thus rank 8 and nullity 2 follow independently. The two allowed
bilinears are diag(1,0,0,0) and diag(0,-1,-1,1).

For an independent unreduced construction, first compute the full matrix
curvature polynomial Q(A)=A wedge A. The raw degree-two monomials are t_i*t_j,
0<=i<=j<12; the off-diagonal symmetric tensor is e_i tensor e_j+e_j tensor
e_i, WITHOUT a half. The 12-by-78 map has 936 entries. Its three central
curvature rows vanish; for each spatial pair the three noncentral outputs
are spanned independently by the nonzero brackets using monomials on that
pair. Different spatial pairs have disjoint monomial supports. Hence rank
9 and kernel dimension 69, independently of row reduction.

Differentiate L Q without substituting any principal solution. For all 66
distinct output pairs, extract all 12 linear coefficients of
partial_J E_I-partial_I E_J. These are 792 rows on 144 unknowns. Combined
with the principal 234 rows there are 1026 rows. The analytic principal and
invariance equivalences prove nullity exactly 2, rank exactly 142. Both
known surviving L tensors must annihilate the combined matrix, be independent,
and reconstruct every computed kernel vector. No rank is assumed for the
792 cubic rows considered alone.

## Independent exact linear-algebra checks

The rational solver performs deterministic full row reduction. Its separate
kernel/RREF consistency check verifies original-matrix annihilation, pivot
columns and vanishing trailing rows. This check ALONE is not a standalone
rank certificate linking transformed rows to the original matrix.

A second algorithm performs forward elimination on the original matrices
over the fixed prime 1000003. Trial division by every integer 2 through 1000
proves primality, since the square root is between 1000 and 1001. All rational
denominators must be nonzero modulo that prime; inversion uses the Fermat
power p-2. Fixed known answers include a rank-one rational matrix with
denominators 2 and 3, a full-rank signed rational diagonal matrix, a zero
matrix, and mandatory rejection of denominator p. This algorithm neither
uses the rational pivot sequence nor transforms its RREF. Its ranks 134,
8, 142 and 9 give independent rational rank LOWER bounds, since any minor
nonzero after reduction modulo p is nonzero over Q. The displayed analytic
kernel dimensions and independently checked explicit kernel vectors supply
the upper bounds. No random prime or post-run prime change is allowed.

Outputs contain matrix and RREF SHA256 digests, rational kernel vectors,
pivots, independent modular ranks, the complete q map and the reduced
eight-equation oracle. The frozen code and enumeration conventions fully
specify the original equation tables. Rational/Gaussian arithmetic and finite
polynomial/Fourier coefficients are exact. There is no floating tolerance.

## Nonvacuous positives and fixed adversarial controls

The raised curl operator Kcurl(F)=(F12,F20,F01), with lowered h=G, is allowed.
For constant A0=x e1, A1=y e2, A2=z e3, literal matrix products give

    F01=2xy e3, F02=2xz e2, F12=-2yz e1,
    I=(1/3)b(A,Kcurl Q)=2xyz,
    dI=(2yz,2xz,2xy).

Check all three derivatives directly from the scalar polynomial against
independently computed trace-pairing force components, and all nine Hessian
entries. This is a positive Chern-Simons-type algebra control, not a new
source action. The central surviving bilinear is separately exercised by
the Fourier derivative control below; it is not declared valid merely
because central brackets vanish.

The first negative operator has only L_(0,0),(01,0)=1. Its principal maximum
defect is exactly 2. A second decoy h=diag(1,1,1,1) passes every principal
row but fails ad-invariance and cubic reciprocity, each maximum defect 4.
Thus a symmetric internal bilinear alone is insufficient.

## Differential closedness is a separate condition

When coefficients depend on position, the formal adjoint also differentiates
them. Pointwise principal and cubic identities do not remove those terms.
In differential-form notation the allowed spatial three-tensor corresponds,
using the fixed density/metric, to an (n-3)-form coefficient beta. The
curvature-only equation from beta times Chern-Simons requires d beta=0;
otherwise the actual Euler derivative has the additional coefficient-
derivative term. In dimension three beta=lambda is a scalar: d lambda=0
is the missing condition. More general backgrounds require the corresponding
covariant density/connection version, not the assertion that arbitrary
coordinate components are constant.

The exact Fourier menu uses theta=x0, period 2pi, NORMALIZED average
(1/2pi) integral_0^(2pi). Central fields have only A1,A2 components, and

    I_lambda=(1/2) average lambda (A2 A1' - A1 A2'),
    f_desired=(-lambda A2',lambda A1'),
    f_actual=(-[lambda A2'+(lambda A2)']/2,
                [lambda A1'+(lambda A1)']/2).

Finite Gaussian-rational Fourier convolution computes these expressions
exactly; averaging extracts mode zero. An independent amplitude-polynomial
derivative of I must equal the projected adjoint expression. Cosine/sine
derivatives, averages of squares and cross products, and a product derivative
are fixed known answers.

Positive: lambda=1, A1=p cos(theta)e0, A2=q sin(theta)e0 gives I=-pq/2 and
both derivatives (-q/2,-p/2). Decoy: lambda=cos(theta), A1=p sin(theta)e0,
A2=q e0 gives I=pq/4, actual derivative (q/4,p/4), desired force (0,p/2).
The desired cross derivatives differ by 1/2 even though lambda times an
allowed constant operator satisfies all pointwise principal/cubic identities.
The actual Hessian is symmetric. The missing contribution is one half of
d lambda wedge A, with the displayed curl sign convention. This explicitly
separates pointwise algebraic classification from differential sufficiency.

## Conditional fourteen-dimensional statement: independent proof

Do not extrapolate u(1,1) ranks to u(64,64). Here is the separate argument.
For g=u(64,64), the real Lie algebra splits into its center R iI and the
simple ideal su(64,64). A symmetric invariant form has zero center/simple
cross term: the simple ideal is perfect, so h(z,[x,y])=-h([x,z],y)=0.
On the simple ideal, use the nondegenerate real trace form to identify any
invariant bilinear with an endomorphism commuting with every adjoint action.
Complexification is sl(128,C). This is simple: commuting a nonzero ideal
with diagonal matrices isolates matrix-unit weight spaces; any off-diagonal
matrix unit generates the other units and diagonal differences by brackets.
If the ideal initially has only a nonzero diagonal element, its bracket
with a suitable matrix unit supplies an off-diagonal one. Thus its adjoint
module is irreducible. The elementary complex Schur argument says that a
commuting endomorphism has an eigenvalue, its eigenspace is an invariant
nonzero subspace, and hence the endomorphism is scalar. Preservation of the
real form forces that scalar real. The center has one free square coefficient.
Consequently there are exactly two real invariant symmetric bilinears,
equivalently combinations of ReTr(XY) and Tr(X)Tr(Y).

Both are individually Spin-trivial under the spin representation acting by
matrix conjugation. If K is a Spin intertwiner AND its lowering trace/form
pairing is Spin-invariant, the principal/cubic classification therefore
leaves a Spin-invariant spatial three-form multiplying each of these two
bilinears. There is no such three-form on R^(7,7). Already the subgroup
SO(7) times SO(7) contains every same-block coordinate pi rotation, which
flips two positive or two negative axes. Every degree-three coordinate mask
occupies at least one but fewer than seven axes in some block. Choose an
occupied and an unoccupied axis in that block: the corresponding flip
negates that coefficient. As these actions are diagonal, invariance forces
every coefficient to vanish.

The finite control tests all 364 three-masks against all 42 flips, 15288
characters, predicting zero survivors. The four masks 0,127,16256,16383
provide 168 positive subgroup character checks. In particular the degree-
seven masks are ONLY invariants of this tested subgroup, NOT asserted
invariants of full Spin(7,7). This is a sign/representation control for the
written proof, not a substitute for the full-gauge bilinear argument.

Thus a nonzero curvature-only Euler covector cannot have ALL the stated
fixed-operator, unrestricted-gauge and Spin-intertwining hypotheses. This
does not prohibit Chern-Simons operators with additional geometric tensors,
restricted variation sectors, or actual action derivatives involving adjoints.

## Source scope, epsilon and constructive continuation

The primary source is exactly bound, with text anchors checked in Program.
Its Eq.8.1 (text line 2030) gives possible bracket/Hodge contractions,
Eq.8.7 (2076) supplies invariant tensors, and the discussion at 2088-2100
explicitly says the original preferred operator notes are missing. Eq.9.3
(2127) displays one Ricci/scalar-like family. Eq.9.4 (2162) is the mixed
torsion/contraction action; Eq.9.7 (2232) prints the residual variation claim.
The source is not silently completed by this audit.

For operators built only from the displayed fixed invariant tensors, fixed
Hodge operations, scalar coefficients and uniform epsilon conjugation, all
wedge products and either matrix bracket commute with simultaneous
conjugation. Hence pointwise

    K_epsilon = Ad_(epsilon^-1) K_identity Ad_epsilon.

For fixed epsilon the trace pairing and unrestricted variation carrier are
preserved by this invertible conjugation. Therefore pointwise principal and
cubic compatibility is equivalent to compatibility at epsilon=I; epsilon
does not rescue a failure of those identities in THIS displayed family.
If epsilon varies spatially, its derivatives and background terms must also
be tracked when asserting full differential sufficiency. The pointwise
argument never drops those terms from an action.

Frame covariance alone does not establish pointwise Spin invariance. An
additional fixed tensor, background curvature, reduced structure group, or
more general epsilon-dependent operator may transform covariantly but have
a smaller stabilizer. Those possibilities are outside the conditional zero
statement. Nor do invariant dimensions of Phi1/Phi2 alone classify every
possible operator. No claim is made that missing author intent must satisfy
this bounded completion or that the whole source theory has been ruled out.

The constructive consequence is to compute the actual action-derived
adjoint/cyclic Euler operator, or explicitly declare additional structures
and restrictions before searching a broader class. Merely adding a fixed
symmetric mass term cannot repair an antisymmetric connection-connection
Hessian block. Adding independently varied fields also does not repair that
principal block on variations with those other fields held fixed; constraints
which remove those variations would change the hypotheses and require a
separate specification. No physical projection or mass result follows here.

## Frozen pack, execution and interpretation

The project explicitly compiles unchanged Phase592 ExactAlgebra.cs and
Phase594 PolynomialTensor.cs, each separately bound. Their earlier lineage
does not replace direct binding. Exactly eleven unique input IDs and paths
bind own Program, ExactControls, project, this STUDY, primary text, Phase594
summary and contract, both compiled helpers, Directory.Build.props and the
complete live core manifest. The manifest must match all 726 src C#/project
paths and hashes, excluding bin/obj; it is not an abbreviated path sample.
Phase594 must have passed its exact terminal and controls under its bound
contract. Standalone contract fixtures must deep-equal the complete Program
menu, including counts, thresholds, resources and terminal precedence.

Resource estimate: 20 CPU seconds and 128 MiB; prospective ceiling estimates
60 CPU seconds and 256 MiB. No sampling, diagonalization, optimization, search
over profiles or threshold tuning. All tolerances are exactly zero. The
success terminal is
`fixed-operator-helmholtz-controls-pass-conditional-classification-only`.
Input drift dominates known-answer failure, then principal-rank failure,
cubic-classification failure, positive/decoy failure, conditional-Spin failure,
then success. Any failed first output is preserved; no frozen scientific
edit or unapproved rerun is allowed. Repairs require separately reviewed
versioned preregistration. Full and summary JSON bytes are identical and
deterministic; elapsed time is recorded only in the unbound implementation
record after approval and execution.

All fourteen standard authority firewalls remain false. External review is
pending in the source/physical sense even after independent technical pack
review. O4 remains pending, promoted physical mass claims remain zero, and
Phase561, production, sampling and GeV claims remain unopened.
