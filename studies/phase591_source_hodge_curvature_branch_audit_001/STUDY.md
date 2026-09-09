# Phase591: canonical source Hodge curvature branches

Prospective A48 exact algebra. The program, project, proof, complete fixture
menu and bindings must be frozen and reviewed before first scientific execution.
Build-only checks are permitted. No source operator is selected and no core
or historical scientific artifact is changed.

## Declared source-inspired subfamily and conventions

The bound source is `docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`,
SHA256 `062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Eq8.1 and the following text (lines 2026-2034) permit commutator and
i-anticommutator tools; Eqs8.5-8.7 (lines 2060-2078) concern invariant tensors
in exterior degree r tensored with u(64,64). Eq9.3 (line 2126; visually checked
in the A46 source note) has the outer Hodge star and coefficient 1/2 in its
second term. The text extraction alone has displaced those symbols.

Use signature (7,7), sigma_a=+1 for a=0,...,6 and -1 for a=7,...,13;
theta^0 wedge ... wedge theta^13 is the positive volume form, and
gamma_a gamma_b+gamma_b gamma_a=2 sigma_a delta_ab. Freeze the canonical
invariant subfamily

    Phi1=lambda1 sum_a theta^a tensor gamma_a,
    Phi2=lambda2 sum_(a<b) theta^ab tensor gamma_a gamma_b.

Lambda1 and lambda2 are formal real normalizations. No value, normalization
from the word "normed", or source-preferred branch is chosen. Phase590 must
first certify the mixed-signature algebra, real form and invariant tensors.
It also establishes independent volume-dual alternatives Omega gamma_a and
i Omega gamma_a gamma_b. Those noncanonical directions remain open; this
phase does not exhaust admitted invariant tensors, even at degrees one/two.

For a strictly increasing mask I with complement Ic, define

    star(theta^I)=shuffle(I,Ic) product_(i in I) sigma_i theta^Ic.

Thus star squared on degree r is (-1)^(r(14-r)+7), star(1)=vol and
star(vol)=-1. The inverse of star:Omega1->Omega13 is star:Omega13->Omega1
because the corresponding square is +1. It is not an arbitrary rephasing.
Every exterior mask, including top and bottom, is an exact known answer.

The coefficient brackets are C(X,Y)=XY-YX and A(X,Y)=i(XY+YX), combined
with the ordinary exterior wedge in its declared order. No different graded
bracket convention is silently substituted. At epsilon=I the tested typed
operator, mapped back to one-forms, is

    U(F)=star1_inverse( [Phi1 wedge star(F)]_first
             -(1/2) star([Phi1 wedge star([Phi2 wedge star(F)]_inner)]_outer) ).

Its intermediate exterior degrees are respectively 2->12->13 and
2->12->14->0->1->13, then 13->1 by inverse star1. The implementation
performs those literal sparse wedge, bracket and Hodge operations. It does
not replace the chain with the anticipated Ricci formula.

## Curvature space and complete sparse basis

Let R_abcd be a real algebraic Riemann tensor: antisymmetric in a,b and c,d,
symmetric under exchange of the two pairs, and satisfying the first Bianchi
identity. Represent it as a symmetric 91x91 pair matrix indexed by a<b.
This storage is sparse; no dense operator or dense eigensystem is assembled.
The spin-curvature convention is explicitly

    F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d gamma_c gamma_d,
    F=sum_(a<b) theta^ab tensor F_ab.

The factor 1/2 is retained with these ordered-pair sums. The independent
Ricci/scalar computation uses a four-index accessor, not Clifford arithmetic:

    Ric_bd=sum_a sigma_a R_(a b a d),
    Scal=sum_b sigma_b Ric_bb,
    RicGamma_j=sum_d Ric_jd sigma_d gamma_d,
    Gamma=sum_a theta^a gamma_a.

The complete basis has three disjoint coordinate sectors:

1. Ninety-one diagonal pair entries, each set to one.
2. The 1092 symmetric off-diagonal pair coordinates whose pairs share one
   index: 14 choose(13,2)=1092. Each row sets that coordinate and its
   symmetric transpose to one.
3. For each a<b<c<d, the three disjoint-pair coordinates are
   t1=R_abcd, t2=R_acbd, t3=R_adbc, with Bianchi t1-t2+t3=0. Use rows
   (1,1,0) and (0,1,1), giving 2 choose(14,4)=2002 rows, each with four
   nonzero symmetric matrix entries.

The diagonal/shared coordinates and respectively t1,t3 of each quadruple
are unique pivot coordinates. Every row has its own pivot coefficient one
and zero at every other selected pivot, proving independence without numeric
rank fitting. The program verifies that exact pivot property. In the full
symmetric pair space there are 91*92/2=4186 coordinates. Each of the 1001
distinct quadruples contributes one independent Bianchi equation on its own
three-coordinate sector; repeated-index Bianchi identities follow identically
from pair antisymmetry and pair-exchange symmetry. Therefore the algebraic
Riemann dimension is 4186-1001=3185. Our independent set of that size spans.
Every row is checked against all 1001 distinct-quadruple identities and pair
symmetry, so none of the basis or Bianchi properties rests on a sample count.

## Independent derivation of the expected contractions

With the signed Hodge convention above, direct evaluation on theta^bc gives

    star13(theta^a wedge star2(theta^bc))
      =-sigma_a(delta_ab theta^c-delta_ac theta^b).

Consequently the first C contraction has one-form component

    -sum_a sigma_a [gamma_a,F_aj]
      =-sum_(a,d) sigma_a R_(a j a d) sigma_d gamma_d
      =-RicGamma_j,

using [gamma_a,gamma_c gamma_d]=2 sigma_a(delta_ac gamma_d-delta_ad gamma_c).
The factor two cancels the frozen spin-curvature half. This derivation also
fixes the raised Clifford index on the Ricci result, including mixed planes.

The first A contraction instead uses
{gamma_a,gamma_c gamma_d}=2 gamma_[acd]. For each distinct triple a,b,c
its grade-three coefficient is proportional to
R_ajbc-R_bjac+R_cjab, which is Bianchi after pair exchange. It vanishes on
the algebraic Riemann space. That is not a claim on arbitrary ad-valued
two-forms.

The inner contraction after its top Hodge star is

    -1/2 sum_(a<b,c<d) R_abcd sigma_a sigma_b sigma_c sigma_d
                             bracket(gamma_a gamma_b,gamma_c gamma_d).

Its commutator vanishes by pair-exchange symmetry. For A, the diagonal
Clifford products square to -sigma_a sigma_b, leaving
i sum_(a<b) sigma_a sigma_b R_abab=(i/2)Scal times identity.
The grade-four coefficient for a<b<c<d is
-2i sigma_a sigma_b sigma_c sigma_d (t1-t2+t3) gamma_abcd, hence zero.
This independently derives the inner sign using star(vol)=-1.

An outer C bracket with that scalar is zero. An outer A bracket gives
i{gamma_a,(i/2)Scal I}=-Scal gamma_a. Multiplication by the source's
-1/2 produces +(Scal/2)Gamma. The two formal coefficient slots are therefore

| first/outer/inner | coefficient of lambda1 | coefficient of lambda1*lambda2 | Primary tied-Phi1 family? |
| --- | --- | --- | --- |
| CCC | -RicGamma | 0 | Yes |
| CCA | -RicGamma | 0 | Yes |
| CAC | -RicGamma | 0 | No |
| CAA | -RicGamma | (Scal/2)Gamma | No |
| ACC | 0 | 0 | No |
| ACA | 0 | 0 | No |
| AAC | 0 | 0 | Yes |
| AAA | 0 | (Scal/2)Gamma | Yes |

Both occurrences of Phi1 use the same bracket in the four primary rows.
Tying is our conservative primary-family restriction, not a theorem that
Eq9.3 requires it. Eq8.1 does not resolve the occurrence choices. All eight
occurrence choices are an explicitly enlarged diagnostic. In
particular CAA is not silently promoted to an author-permitted or selected
operator. Formal coefficient comparison also prevents cancellation at a
special lambda value from hiding a failed tensor identity. Linearity of the
literal chain and the independent contractions in R extends the all-basis
checks to the entire algebraic Riemann space, and only that declared space.

## Arithmetic, geometric anchors and scope decoys

Coefficients are reduced checked-Int64 rationals with exact complex pairs.
Exterior and Clifford blades use masks. Clifford multiplication is checked
against independent ordered-word insertion on all 106 blades of grade<=2
times all 1471 blades of grade<=4: 155926 products. The elementary Clifford
relations and all 16384 Hodge-square masks are also checked. Tolerance is
exactly zero. No floating point or gamma-matrix diagonalization enters this
phase; the existing builder comparison is bound upstream in Phase590.

Every one of 3185 basis tensors receives eight branch checks (25480), including
12740 primary comparisons, with per-row deterministic output hashes. The
separately indexed Ricci/scalar arrays provide the oracle. Five explicit
anchors retain complete sparse branch coefficients in the output:

- Flat R=0: every contraction zero.
- Positive plane R0101=1: Ric00=Ric11=1, Scal=2.
- Mixed plane R0707=1: Ric00=-1, Ric77=1, Scal=-2.
- Constant sectional curvature K=1: R_abab=sigma_a sigma_b, Ric_ab=13 eta_ab,
  raised Ricci=13 identity, Scal=182. Thus the two nonzero slots are -13 Gamma
  and 91 Gamma; these slots do not fix lambda1 or lambda2.
- Nonzero Weyl anchor: R0101=1, R0202=-1, R1313=-1, R2323=1. Its Ricci/scalar
  are exactly zero, curvature is nonzero, and all eight branches vanish.

Two off-subspace inputs preserve antisymmetry within pairs but remove a
different further premise. A sole M_01,02=1 without its transpose breaks pair
symmetry and has nonzero grade-two inner C. Symmetric M_01,23=M_23,01=1
violates Bianchi and gives nonzero grade-three first A and grade-four inner A.
Their literal nonzero outputs are retained. They forbid extending the zero
branches or one-form Clifford-grade preservation to general gauge curvature.
A top-star-sign decoy flips the actually calculated positive-plane inner A
from +i to -i; it must differ, exposing the otherwise easy overall-sign error.

## Freeze, resources and remaining boundary

Eight unique exact input IDs and paths bind program/project/proof, source,
the passed Phase590 summary and its contract, the full core manifest and
Directory.Build.props. The manifest validates all 726 current src/**/*.cs
and src/**/*.csproj paths, excluding bin/obj, and their SHA256 values and
sorted `path + space + file SHA256 + newline` tree digest. The executable
structurally checks the entire fixture object and terminal precedence before
scientific calculations. No evolving research note is bound.

Resource estimate: 45 CPU seconds and 256 MiB, below frozen estimate ceilings
60 CPU seconds/512 MiB. Inputs have at most four pair entries per basis row;
literal branches are evaluated sequentially and only small sparse tensors
are live. The principal census is 3185*1001 scalar Bianchi checks; retained
basis metadata and exact output hashes do not require a dense full operator.
These are prospective estimates, not timing measurements or execution limits.

Terminal precedence: invalid-or-drifted-input; known-answer-control-failed;
riemann-basis-control-failed; literal-contraction-control-failed;
`canonical-source-hodge-curvature-branches-pass-choice-open`.
First failed outputs are preserved and any scientific repair requires a new
versioned pack. Only a fully controlled result has auditPassed=true.

The conclusion is conditional canonical tensor algebra, not source selection,
action equivalence, dimensional reduction, a kinetic pairing or a physical
spectrum. Volume-dual and other invariant directions, source branch choices,
normalizations, observer/retained-field maps and actual source pairing remain
open. Phase561 stays closed, O4 remains pending, all fourteen authority flags
stay false and promotedPhysicalMassClaimCount=0.
