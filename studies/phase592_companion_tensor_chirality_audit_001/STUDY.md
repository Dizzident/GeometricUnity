# Phase592: companion tensors, formal matching and chirality

Prospective Amendment A49 exact algebra. Freeze and review this proof, all
source files, the complete fixture menu and exact bindings before the first
scientific execution. Release build-only checks are permitted beforehand.
This tests a declared invariant subfamily, not a source operator selection.

## Source scope and fixed conventions

The bound primary source is `docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`,
SHA256 `062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Eq8.1 and its following text (lines 2026-2034) provide commutator and
i-anticommutator tools; Eqs8.5-8.7 (2060-2078) discuss invariant tensors.
Eq9.3 (2126) gives the displayed Hodge construction, with outer star and
coefficient 1/2 retained as visually checked in the A46 source work.
The source acknowledges its missing final operator (2150-2153).
The mixed torsion/contraction pairing in Eq9.4 (2162-2183) is not a residual
self-square. No unspecified source choice is filled by this audit.

Use the Phase590/591 conventions: sigma_a=+1 on a=0,...,6 and -1 on 7,...,13;
gamma_a gamma_b+gamma_b gamma_a=2 sigma_a delta_ab; increasing blade order;
positive exterior volume theta^0 wedge ... wedge theta^13. Write

    Omega=gamma_0 ... gamma_13,  Omega^2=1,
    G=sum_a theta^a gamma_a,  Gamma2=sum_(a<b) theta^ab gamma_a gamma_b,
    Phi1=(a+b Omega)G,  Phi2=(c+i d Omega)Gamma2.

The coefficients a,b,c,d are formal real scalars. Phase590 certifies the
canonical and companion directions, their real form and Spin invariance.
It proves at least two invariant directions at each degree, not that these
exhaust all invariant tensors. Omega commutes with even and anticommutes
with odd Clifford elements. Its sign is fixed, not silently interchanged
with the raw core builder's different chirality convention.

For an increasing exterior mask I, use

    star(theta^I)=shuffle(I,Ic) product_(i in I) sigma_i theta^Ic.

Thus star squared in degree r is (-1)^(r(14-r)+7), star(top)=-1, and the
inverse star on degree one is star on degree thirteen. The curvature is

    F=sum_(a<b) theta^ab F_ab,
    F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d gamma_c gamma_d.

The half-factor is part of the fixed ordered-pair convention. For bracket
C(X,Y)=XY-YX or A(X,Y)=i(XY+YX), the literal tested chain at epsilon=I is

    K(F)=star1_inverse([Phi1 wedge star(F)]_first
          -(1/2) star([Phi1 wedge star([Phi2 wedge star(F)]_inner)]_outer)).

Bracket order is first/outer/inner. We restrict to CCC,CCA,AAC,AAA, tying the
two Phi1 occurrences. This is our conservative family restriction, not a
theorem that the author requires tying. No independent-occurrence branch,
physical chirality projection, source action, sampling or dimensional
reduction is introduced here.

## Full algebraic Riemann space and independent oracle

R is antisymmetric within each pair, pair-exchange symmetric and Bianchi.
Its symmetric 91x91 pair-coordinate space has 4186 independent coordinates.
The 1001 distinct quadruples each impose the independent relation
t1-t2+t3=0 on their disjoint coordinate sector, where
t1=R_abcd,t2=R_acbd,t3=R_adbc for a<b<c<d. Repeated-index Bianchi relations
follow from the pair symmetries. Hence the Riemann dimension is 3185.

The basis consists of 91 diagonal entries, 1092 shared-index symmetric
off-diagonal entries, and two rows (1,1,0),(0,1,1) per quadruple: 2002 rows.
Each has a unique unit pivot: its diagonal/shared coordinate, or t1/t3 in
the two quadruple rows. All other selected pivot coordinates are zero.
Independence plus the dimension count proves spanning. The executable
checks this pivot property, pair symmetry and every quadruple Bianchi
relation on every row. This proof does not generalize to arbitrary
adjoint-valued two-forms; Phase591's bound off-Riemann controls delimit it.

The independent four-index oracle is

    Ric_bd=sum_a sigma_a R_ab ad,
    Rscalar=sum_b sigma_b Ric_bb,
    J=sum_(j,d) theta^j Ric_jd sigma_d gamma_d.

Below R denotes Rscalar, not the rank-four tensor. Both the literal chain
and the oracle are linear in algebraic curvature. Comparing every formal
coefficient on the full basis proves the identities on that whole space.
No parameter grid or selected curvature sample is promoted to universality.

## Independent companion-branch derivation

The elementary signed Hodge contraction is

    star13(theta^a wedge star2(theta^bc))
       =-sigma_a(delta_ab theta^c-delta_ac theta^b).

It yields first C=-J for canonical G, with the spin-curvature half canceled
by the factor two in the vector/bivector commutator. Canonical first A is
zero by Bianchi. Since Omega commutes with F's even coefficients, the
companion first terms are their left Omega multiples. With P=a+b Omega,
the first C term is therefore -PJ, and first A remains zero.

The canonical inner C is zero by pair-exchange symmetry. The canonical
inner A is (iR/2)I: its diagonal Clifford product gives the scalar and its
grade-four coefficient is proportional to t1-t2+t3. Multiplication by
c+i d Omega commutes with these even products, so the full inner A is

    S=(i c R/2)I-(d R/2)Omega.

This uses the top-star sign -1. For every odd vector gamma,

    [P gamma,Omega]=-2 P Omega gamma=-2(b+a Omega)gamma,
    {P gamma,Omega}=0.

The outer C applied to S is dR(b+a Omega)G. The outer A applied to S is
-cRPG. The consecutive outer star/inverse-star return the one-form without
an additional sign. Multiplying by -1/2 proves the full tied formulas:

| Branch | Exact formal result |
| --- | --- |
| CCC | -PJ |
| CCA | -PJ-(dR/2)(b+a Omega)G |
| AAC | 0 |
| AAA | (cR/2)PG |

The executable does not substitute these formulas into the literal side.
It computes the first contractions of G and Omega G, the inner contractions
of Gamma2 and i Omega Gamma2, and all corresponding outer contractions.
The six coefficient slots a,b,ac,bc,ad,bd are compared separately. This
prevents a special parameter cancellation from concealing a wrong branch.
There are 3185*4=12740 branch cases and 3185*4*6=76440 coefficient cases.

## Universal matching: necessity, sufficiency and the trivial sector

Matching CCA to -P(J-RG/2) for every algebraic Riemann tensor is equivalent
to the exact residual identity

    K_CCA+P(J-RG/2)=-(R/2)[(a+bd)G+(b+ad)Omega G].

The positive-plane fixture has R=2. G and Omega G have nonzero, independent
Clifford grades one and thirteen. Thus necessity is precisely

    E1=a+bd=0,  E2=b+ad=0.

These equations plainly suffice on every algebraic curvature input. The
program constructs the positive-plane residual as an actual polynomial
tensor from the literal coefficients and checks it against -E1 G-E2 Omega G.
Its exact polynomial ring also checks the elimination certificates

    d E2-E1=a(d^2-1),
    d E1-E2=b(d^2-1),
    a E1-b E2=a^2-b^2.

If (a,b) is nonzero, E1/E2 imply a is nonzero (a=0 would force b=0).
Hence d^2=1, b=-da; conversely substitution for either d=+1 or -1 makes
both equations vanish identically for formal a. The parameter c is arbitrary.
The complete nontrivial solution is a!=0, d=+/-1, b=-da, c arbitrary.
The separate trivial solution a=b=0 allows any c,d. Invertible P has
a^2!=b^2 and inverse (a-b Omega)/(a^2-b^2); it cannot universally match.

The quantifier matters: when R=0 the matching residual vanishes without
either parameter equation. A prefrozen nonzero-Ricci scalar-flat decoy
R0101=1,R0202=-1 gives J=theta^1 gamma_1-theta^2 gamma_2 and matches for
the invertible canonical parameter row. It disproves imposing universal
parameter constraints from just one scalar-flat input.

## Declared norm, chirality and mixed-pairing safeguard

Use only the declared diagnostic pairing: contract exterior indices with
the fixed (7,7) metric and use -ReTr(XY)/128 on coefficients. Clifford
scalar extraction supplies normalized trace, as independently checked by
the bound Phase590 matrix controls. The two Phi1 basis norms are -14,+14,
and both cross terms are zero. Direct polynomial contraction therefore is

    norm(Phi1)=14(-a^2+b^2).

Nontrivial matching parameters make this norm zero. Nullity is neither
vanishing of Phi1 nor source inadmissibility under an unspecified pairing.
It is not alone sufficient for matching: a=b=d=1 is a frozen counterexample.

Define Pi_h=(1+h Omega)/2. At b=-da,d=+/-1,

    P=a(1-d Omega)=2a Pi_-d,
    P gamma=2a Pi_-d gamma=2a gamma Pi_d.

Thus these odd maps take input chirality S_d to output S_-d. They do not
preserve a single input/output chirality. All 28 sign/axis cases check both
left output and right input projectors, their complementary annihilations,
idempotence and the deliberately incorrect same-sign input assertion.
Pi_h has trace64 and rank64 (idempotence and Omega trace zero), but this
does not authorize a physical half-spinor restriction.

For arbitrary odd V,W, anticommutation with Omega proves

    Pi_h V Pi_h W=0.

It does not prove the opposite-sector product zero. The exact controls are

    Tr(Pi_h gamma_a Pi_-h gamma_a)=64 sigma_a != 0.

Every sign/axis tests both the identically zero same-sector product and this
nonzero opposite-sector trace. They decisively reject inferring mixed
pairing nullity from self-pairing nullity. With the bound H=gamma_7...gamma_13,
ordinary dagger reverses blade order and multiplies by the negative-axis
signs. Direct blade products check Omega^dagger H=-H Omega and
Pi_h^dagger H Pi_h=0. This is H-isotropy of each chiral spinor space, not
vanishing of cross-sector H pairings or a choice of physical inner product.

In particular the source Eq9.4 contains a torsion/contraction mixed trace
Tr(TK), not just Tr(K^2). The torsion sector and variations have not been
restricted here. No zero action, zero kinetic operator or inadmissibility
conclusion follows from our self-pairing controls.

## Frozen fixtures, arithmetic and provenance

Five full coefficient-output anchors are flat; positive plane R0101=1;
mixed plane R0707=1; constant K=1 with R_abab=sigma_a sigma_b; and the
nonzero Weyl input R0101=1,R0202=-1,R1313=-1,R2323=1. Their scalar values
are respectively 0,2,-2,182,0. Constant curvature has Ric=13 eta; Weyl has
Ric=0 and every formal output coefficient zero.

Six fixed parameter rows (a,b,c,d) are (1,-1,2,1), (1,1,-3,-1),
(1,0,1,0), (2,1,-2,1), (1,1,0,1), (0,0,7,3). The first two and last
match universally; the middle three do not. Rows three/four are invertible;
row five is null but has the wrong d. Tests on anchors are controls for the
symbolic proof, not its replacement. Opposite companion-sign, wrong input
projector, false invertible match, null-but-wrong-d, scalar-flat and false
self-to-mixed-pairing implications are the complete prefrozen decoy menu.

Coefficients are checked-Int64 reduced rational complex pairs, with a small
exact rational polynomial ring in a,b,c,d. The 212 Clifford masks of grades
0,1,2,12,13,14 give 44944 products checked against independent word insertion.
They cover all operand grades of the declared literal chain and pairings.
All 16384 Hodge masks are checked, including top-star and Omega square.
Tolerance is exactly zero. No dense operator or eigensystem is constructed.

Twelve unique IDs/paths bind own Program, ExactAlgebra helper, project and
proof; primary text; Phase590/591 summaries and contracts; original Phase591
Program; full core manifest; and Directory.Build.props. The helper's methods
and types were extracted from Phase591, with only an enclosing static class
and public method visibility added. It is not byte-identical to the upstream
program. Runtime structural parity compares the exact extracted methods
after reversing the visibility change, and compares the trailing types.
Both actual files have their own exact bindings. The manifest validates all
726 live src .cs/.csproj paths excluding bin/obj, every SHA256, and the sorted
path-space-hash-newline tree digest. No evolving research note is bound.

The fixture object and terminal precedence must match structurally in full.
Prospective resource estimate is 30 CPU seconds/256 MiB, below estimate
ceilings of 60 CPU seconds/512 MiB; these are not measured runtime claims or
runtime termination limits. Full and summary JSON are identical deterministic
outputs, including exact evidence and 14 false authority flags.

Terminal precedence: invalid-or-drifted-input; known-answer-control-failed;
riemann-basis-control-failed; companion-branch-control-failed;
parameter-chirality-control-failed;
`companion-tensor-chirality-controls-pass-source-choice-open`.
Any first scientific failure is preserved; repairs require a versioned pack.
No core/shared edits, source tensor selection, new sampling or physical mass
claim follows. O4 remains pending, Phase561 closed, externalReviewPending=true
and promotedPhysicalMassClaimCount=0.
