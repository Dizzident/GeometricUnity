# Phase602: a stationary odd background and its full constant Hessian

This is a prospective, deterministic exact-arithmetic audit, authorized by A55.
The complete scientific pack must be frozen and independently reviewed before
its first scientific execution. A Release build is not an execution. The
implementation note is unbound and may record results afterwards; this proof,
Program, project, linked helpers and preregistration are not then edited.

## 1. Question and scope

Does the actual first-action connection gradient vanish at the declared odd
background, and does its differentiated FULL output close on the proposed
26-dimensional constant carrier? If it does, what exact Jordan structure does
that closed operator have? This is not the compression tested in Phase601.
Nor does zero curvature alone establish stationarity: a deliberately unaligned
zero-curvature background is a negative control.

We keep the fixed flat reference, metric, orientation and normalized density,
epsilon equal to the identity, constant Clifford-valued differential forms,
and kappa=0. We vary all connection coefficients before restricting the
resulting gradient or Hessian. No metric, source-density, second-action or
physical mode decomposition is chosen. The constant fields can be placed on
the normalized flat periodic control carrier; all exterior derivatives and
their formal-adjoint terms vanish here. This does not discard those terms
from the general first-action Euler equation on nonconstant fields.

## 2. Literal operator, real pairing and immutable lineage

Indices are 0 through13, with seven positive followed by seven negative
directions. Gamma_a Gamma_b+Gamma_b Gamma_a=2 sigma_a delta_ab. Ordered
blade masks and coframe masks are separate. Omega is the ordered14-volume
blade, Omega^2=1, anticommutes with odd Clifford elements and commutes with
even ones. Every one-form basis coefficient used below is H-anti-Hermitian;
Omega Gamma_i is also H-anti-Hermitian. Coframe and Clifford signatures must
both be retained: the resulting diagonal one-form Gram is not positive.

For h=-1,+1 let P=1+h Omega, Phi1=P Gamma1, and
Phi2=(c-i h Omega)Gamma2. Here Gamma1=sum theta_a Gamma_a and
Gamma2=sum_(a<b)theta_ab Gamma_ab. The real c is FORMAL. We calculate its
constant and linear coefficients, never estimate them at sampled values.
This is the already declared matched CCA family, not a selection of the
author's missing final tensors, normalization or bracket convention.

The linked Phase600 Chain implements the printed source9.3 form chain:

K(F)=star13(C_Phi1(star2 F)
       -1/2 star1(C_Phi1(star14(A_Phi2(star2 F))))).

C and A are respectively coefficient commutator and i times coefficient
anticommutator together with exterior wedge. The intermediate form degrees
are 2,12,13 and, on the second leg,14,0,1,13,1. Both Hodge legs and the
one-half factor are retained. All degree checks remain active, including
zero tensors. The metric index7 Hodge signs are not Euclidean signs.

The declared pairing is B(X,Y)=-ReTr(XY)/128 times the signed exterior
metric, with normalized zero Fourier mode. This is a real BILINEAR pairing,
not a Hermitian-positive replacement. Real trace transposes are C_A^dag=-C_A
and A_A^dag=A_A; the latter retains i without conjugating it. Exterior
adjoints include signature and shuffle factors. The literal reverse chain
and independently simplified reverse chain from600 are compared on every
new unit input to which K^dag is applied in this audit. The full derivative
DQ_S[V]=S wedge V+V wedge S is the coefficient-commutator wedge map;
its linked DQAdjoint has no extra factor2.

Primary source sections8/9 are exact-bound. Passed600 binds full adjoint
controls, and passed601 binds the subsequent Hessian/closure audit and its
exact rational matrix helper. All four externally compiled helper files
are individually exact-bound here. No frozen upstream file is rewritten.
The source's printed tau/sign issue and unspecified norm in9.11 are not
resolved by choosing this declared control family or pairing.

## 3. Actual gradient and stationary versus unaligned backgrounds

For constant S, the original action is
I(S)=gamma B(S,K(S wedge S))/3. Its exact real directional derivative is

dI_S[V]=gamma/3 [B(V,KQ)+B(S,K(DQ_S[V]))].

Nondegeneracy of the full declared trace/form pairing gives the ambient
constant gradient G(S)=gamma(KQ+DQ_S^dag K^dag S)/3. It is NOT KQ and is not
the rejected source9.7 shortcut. Differentiating this expression gives
the three Hessian terms tested below, with no extra factor2.

Let S0=lambda theta0 Gamma0. Its Q vanishes, but we additionally compute
the complete K^dag image at unit lambda:

Y0=-2 sum_(a<b,a,b!=0) theta_ab Gamma_ab,
Yc=-2 i h sum_(a<b) theta_ab Omega Gamma_ab.

These have exactly78 and91 nonzero mask coefficients. In DQ_S0^dag only
the0j form components can contribute. Y0 has none; Gamma0 commutes with
Omega Gamma0j in Yc. Consequently the FULL gradient is zero for all c,h,
lambda,gamma. The computation compares both reverse chains with these
complete tensors, retains the nonzero c intermediate, and only then tests
the gradient. Its vanishing is not inferred from a projection or norm.

The fixed negative control U=lambda theta0 Gamma2 also has Q=0. Direct
full-adjoint contraction instead gives

G(U)=-(4 gamma lambda^2/3) sum_(j!=0,2)theta_j Gamma_j.

The c coefficient vanishes and the unit constant coefficient has12 terms,
independently of h. V=theta1 Gamma1 pairs with this full gradient to4/3 at
unit lambda=gamma=1. The program separately differentiates the original
cubic action in this direction using forward K only and obtains the same
nonzero value. Thus this control rejects both Q=0-implies-stationary and
the curvature-only shortcut. Constant-field integration introduces no
boundary term which could change that directional coefficient.

## 4. Full differentiated columns and formal-c cancellations

For fixed tensors and couplings, the actual constant Hessian is

H_S0[V]=(gamma/3)(A[V]+B[V]+C[V]),
A=K(DQ_S0[V]), B=DQ_V^dag K^dag S0, C=DQ_S0^dag K^dag V.

Its outputs are assembled in unrestricted sparse Clifford/form masks.
Only afterwards are their pairings with the candidate basis used to
reconstruct and test exact equality with the full image.

Set V_i=theta_i Gamma_i and W_i=theta_i Omega Gamma_i, i=1..13; order the
basis V1..V13,W1..W13. At unit lambda the independent hand coefficient
table, before multiplying by gamma/3, is:

| Input | A | B | C |
| --- | --- | --- | --- |
| V_i | 4 sum_(j!=0,i)(V_j+h W_j) | 4 sum_(j!=0,i)V_j | 4 sum_(j!=0,i)V_j |
| W_i | 0 | 4 sum_(j!=0,i)W_j | -4h sum_(j!=0,i)V_j |

All three c coefficients vanish separately. This is not a numerical
c=0 restriction. To see the cancellations, DQ_S0 V_i=2 theta0i Gamma0i
and DQ_S0 W_i=0. In A_c, the inner anticommutator on Gamma0i gives a
scalar, killed by the outer commutator. In B_c, contraction with the
nonzero Yc leaves commutators of Gamma_i or Omega Gamma_i with
Omega Gamma_ij; these vanish because the shared-index anticommutator
{Gamma_i,Gamma_ij} is zero. In C_c, the diagonal input contraction in
K_c^dag V_i is independent of i and proportional to the same Yc tensor;
for W_i it is -h times that tensor. Its0j components again commute with
Gamma0. These arguments include the possible output form0, rather than
silently excluding it in a projection.

The constant entries follow directly by substituting the Clifford shared-
index commutator identities in the typed chain/reverse chain; the paired
form and Clifford sigma_i factors cancel. The table is frozen independently
of its source calculation. The program compares312 individual legs and104
full images. Each constant full image has24 terms; every c image is zero.
The676 Gram entries are separately computed, giving diag(-I13,+I13).

There are130 nonzero individual-leg omission controls: per h, each of13
V inputs has three nonzero legs and each of13 W inputs has two. Omitting
the already-zero A[W] is not counted as a rejection. Halving B+C instead
of using the full DQ adjoint fails on all52 constant columns. Restricting
the output to pure V fails on all26 V-input columns across the two h.

## 5. Independent polarization of the original action

For every ordered pair A,B of the26 basis vectors, independently extract
the coefficient st in I(S0+sA+tB). Its coefficient is the sum of six
ordered cubic words with outer/inner argument orders
(S0,A,B),(S0,B,A),(A,S0,B),(A,B,S0),(B,S0,A),(B,A,S0), times gamma/3.
Each word uses forward wedge products and the complete forward K; no
adjoint, Hessian function, projected matrix or expected coefficient enters
this oracle. The two exchanged words with the same outer argument are
added BEFORE taking the real pairing. Individual ordered wedge words
need not themselves belong to the H-anti real carrier and can have
imaginary trace parts which cancel only in the exchanged sum. This
regrouping is exact coefficient extraction, not discarding imaginary terms.

All2704 ordered bilinears (26^2 times two h and two c slots) must equal
B(A,H[B]) and the independently predicted entry. Both symmetry of the
polarized matrix and its equality to Gram times the full-output matrix
are checked. No finite difference, parameter fit or target spectrum is
used to derive these coefficients.

## 6. Exact closure, similarity, minimal polynomial and inertia

In paired index/internal notation the matrix is
H=(4 gamma lambda/3)(J13-I13) tensor M, M=[[3,-h],[h,1]].
The actual program representation is V-major, and the full matrix entries
are checked in that ordering. Let e=(1,h),g=(1,0). Then
(M-2I)e=0,(M-2I)g=e: the nilpotent part is NONZERO, not merely square-zero.

Use the mutually orthogonal index basis z0=(1,...,1) and, for a=1..12,
za=(1 repeated a,-a,0 repeated12-a). They have squared lengths13 and
a(a+1), are nonzero, and diagonalize J13-I13 with eigenvalues12 and-1.
The26-column similarity uses za tensor e,za tensor g, in that order for
each sector. Exact rational inversion and both similarity products are
checked against the independently specified13 upper-triangular2x2 blocks
(4 mu/3)[[2,1],[0,2]] at unit gamma=lambda=1.

The characteristic polynomial is therefore (x-32)^2(x+8/3)^24. It is
derived from the checked FULL similarity, not a factorial permutation
expansion or a compressed minor. The minimal polynomial is
(x-32)^2(x+8/3)^2. Exact ranks in the frozen order are
26,25,24,14,2,0,1,12 for H, H-32I, its square, H+8I/3, its square,
their squared-factor product, and the products with respectively the
uniform or traceless exponent lowered by one. The final two nonzero
ranks certify that neither squared factor can be reduced. All26 chiral
sector vectors across both h are constructed from full source columns;
they have Omega eigenvalue h, null trace norm, and the asserted nonzero
H eigenvalue. Nullness does not imply they are zero vectors.

The lowered Hessian is symmetric under the declared indefinite Gram.
Congruence by the same orthogonal-index/internal basis gives13 real
symmetric2x2 blocks, with all off-block entries zero. In each block its
determinant is -4(4 mu/3)^2 ||za||^4<0. The program checks all26 block
determinants across h, so the real constant-action Hessian has13 positive
and13 negative directions at nonzero gamma*lambda. This stationary point
is a constant-action SADDLE, not a Euclidean minimum. No dynamical
instability, physical pole, mass or source vacuum-selection claim follows
without the missing time split, constraints and physical interpretation.

## 7. Couplings, controls and bounded resources

Linearity of K and K^dag and bilinearity of wedge and its adjoint prove
symbolically that G(S0)=0 and H_(lambda S0)=gamma*lambda H_unit. This proof
holds for arbitrary real couplings; it is not interpolated from a grid.
The explicitly frozen known-answer rows (lambda,gamma) are
(-1,1),(1,1),(1,2),(0,1),(1,0),(0,0), for each h. In each row both c slots
of the actual background gradient and all26 full Hessian images are
recomputed using the scaled background. There are624 scaled columns and
24 additional stationarity checks. Nonzero product gives rank26 and
eigenvalues32 gamma lambda and-8 gamma lambda/3; zero product gives the
zero operator, rank0, characteristic x^26 and minimal polynomial x.
No row selects a coupling, amplitude, vacuum scale or unit calibration.

All fixture counts, term counts, formulas, exact zero tolerance and
terminal precedence are duplicated in the Program's literal FixtureJson
and the frozen contract, with full JSON structural equality required.
The complete counts are:256 word cases,16384 Hodge masks,676 Gram entries,
4 contexts,4 stationary adjoint oracles,28 stationarity checks,4 unaligned
gradient and4 independent variation checks,104 full columns,312 legs,
2704 matrix entries,2704 action bilinears,112 new-input adjoint comparisons,
2 Jordan matrices,16 rank certificates,26 chiral vectors,26 inertia blocks,
130 omitted-leg rejections,52 half-DQ rejections,26 discarded-W rejections,
12 scaling rows and624 scaled full-column checks.

All coefficients use unbounded BigInteger rationals and real/imaginary
rational pairs; constant inputs retain zero Fourier frequency. Products
are sparse with exterior overlap pruning, not a preselected carrier
truncation. Every forward two-form source in polarization has at most
one blade before exchange, and a full forward chain has at most28
one-form terms per such input. Reverse images may occupy91 forms and
at most four routed Clifford masks per form; a coarse raw candidate bound
is1560 terms per linked reverse chain. DQ contractions introduce at most
two input blades, never a new Fourier frequency. Thus the frozen maximum
65536 temporary terms is conservative, not inferred by a trial run.
The finite loop menu has fewer than17000 forward chains, fewer than1000
reverse chains and fewer than3000 small DQ contractions; the grouped
exterior product loop and these bounds give a conservative100000000
coefficient-product limit, which is checked in the emitted evidence.
Exact26x26 matrix elimination is polynomial-time; the linked helper's
factorial determinant/characteristic method is deliberately NOT called.
Estimated CPU30s, maximum estimate180s, estimated memory128MiB and
maximum estimate512MiB are prospective estimates, not physical limits.

## 8. Freeze, evidence and authority firewall

Sixteen unique exact file bindings cover own Program/project/proof, four
immutable compiled helpers, the primary text, passed600 and601 summaries,
contracts and programs, the full core manifest, and Directory.Build.props.
The manifest binds all726 live src .cs/.csproj files excluding bin/obj;
Program independently compares the live sorted path list, every file hash
and the sorted path/hash tree digest. Upstream terminal, contract hash,
known-answer/controls booleans and fourteen-false firewalls are checked.

Invalid/drifted inputs take precedence, then known-answer, stationarity,
full-closure, action-polarization and Jordan/scaling failures, then success:
stationary-odd-background-full-closure-controls-pass-no-physical-spectrum.
Known-answer and controls booleans are emitted explicitly. Full and summary
are serialized from the same deterministic object into identical bytes.
Output is evidence, not a new authority to alter registered physics.

All fourteen authority flags remain false, external review/O4 remain
pending, Phase561 stays closed, and promoted physical mass claims remain0.
No core edits, sampling, fitting, author choice, source normalization,
positive norm, measure/contour, physical spectrum or GeV claim is made.
