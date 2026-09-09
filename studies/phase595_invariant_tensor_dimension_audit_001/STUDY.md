# Phase595: complete invariant dimensions at exterior degrees one and two

Prospective Amendment A50 exact classification. Freeze and independently review
the program, project, this proof, complete fixtures and every binding before
first scientific execution. Only build checks are allowed beforehand. This
study does not rewrite Phase590's frozen lower-bound result or select a source
operator, normalization, action or physical interpretation.

## Source question and the real-to-complex bridge

The primary draft's Eqs8.5-8.7 concern the real Spin(7,7) modules

    [Lambda^r(V*) tensor_R u(H)]^Spin(7,7),  r=1,2.

V has signature(7,7), gamma_a gamma_b+gamma_b gamma_a=2 eta_ab, and
H=gamma_7...gamma_13 is the balanced Hermitian form constructed in590.
That bound study checks the actual128-dimensional matrix representation,
all16384 independent Clifford blades, and two real invariant directions at
each of these exterior degrees. The present task supplies an upper bound on
the FULL invariant space, not just on selected Clifford grades.

The conjugate-linear involution sigma(X)=-H^-1 X^dagger H on End_C(S) has
fixed real space u(H). Every complex matrix is uniquely U+iW with U,W in
u(H): U=(X+sigma(X))/2 and W=(X-sigma(X))/(2i). Thus
u(H) tensor_R C is End_C(S), not merely the real Clifford subalgebra.
The real Spin action preserves u(H), and complexification of the finite
system of real infinitesimal invariance equations commutes with its kernel.
Consequently the real invariant dimension equals the complex invariant
dimension. The faithful Clifford representation from590 identifies
End_C(S) with the complete complex Clifford algebra as a vector space,
including every blade grade from zero through fourteen.

Use a complex orthonormal basis e'_a=e_a on positive axes and e'_a=i e_a
on negative axes. Its dual coframe is Theta^a=theta^a on positive axes and
Theta^a=-i theta^a on negative axes. The Clifford generators Gamma_a obey
the same change, Gamma_a=i gamma_a on negative axes, and now Gamma_a^2=1.
Both the vector/gamma and the CONTRAGREDIENT coframe change are necessary.
Canonical tensors sum Theta^I Gamma_I equal sum theta^I gamma_I.

Write Omega_E=Gamma_0...Gamma_13. Here Omega_E=-i Omega and Omega_E^2=-1,
whereas the original mixed-signature Omega^2=+1. These are not competing
claims about one convention. In the complex orthonormal basis the two
coefficient patterns used below are

    C_r=sum_|I|=r Theta^I Gamma_I,
    D_r=sum_|I|=r Theta^I Omega_E Gamma_I.

The real companions from590, Omega C_1 and i Omega C_2, become respectively
i D_1 and -D_2. The executable verifies these phases for all105 form masks,
using the original signature signs and the independent vector/coframe basis
changes. Nonzero complex scales do not change invariant dimensions.

All finite rotations below belong to the COMPLEXIFIED Spin group. A real
infinitesimal invariant is fixed by its complexified Lie algebra and its
exponentials. We do not pretend that a mixed real-signature plane has compact
quarter-turns. This complexification is essential to the upper-bound proof.

## Thirteen sign characters on the entire coefficient carrier

For j=1,...,13 use the determinant-one coordinate sign flip that negates
axes0 and j. It is a complex Spin action: it is the square of a plane
quarter-turn, with an even Clifford lift. On Theta^I tensor Gamma_J its
character is

    (-1)^( |(I xor J) intersect {0,j}| ).

Let K=I xor J. All thirteen characters are+1 precisely when bit_j(K)
equals bit_0(K) for every j. Thus K is either empty or the full14-mask.
The only possible nonzero invariant coefficients have J=I or J=Icomplement.
No representation-theory multiplicity table or restricted Clifford-grade
assumption is inserted into this argument.

The exact census checks every one of105*16384=1720320 coefficient positions
and every one of their thirteen characters:22364160 evaluations. It also
checks the independently derived equivalence with J=I or Icomplement.
Survivors are28 at degree1 and182 at degree2.

As a missing-generator decoy, omit the flip(0,13). Bits0 through12 of K
must then agree but bit13 is free. Four K choices survive, rather than two:
empty, full, {13}, and full without13. This doubles the survivor counts to
56 and364. The same exhaustive loop records this separate control.

## Signed quarter-turn constraints, exact ranks and all cycles

For p=0,...,12 use the adjacent complex orthonormal quarter-turn

    e'_p -> e'_(p+1),  e'_(p+1) -> -e'_p.

Its signed permutation has determinant+1 and a Spin lift. On coframes the
dual action is obtained from the inverse transpose. For an orthonormal
signed permutation it has the same basis images as the vector action; the
program constructs the inverse matrix entries separately to obtain this.
Exterior signs are counted from inversions of the transformed index list.
Clifford signs are independently obtained by successively multiplying the
transformed generators with the Euclidean Clifford rule. Every one of
13*16384=212992 masks checks agreement between these two transformations,
grade preservation, the correct two-axis sign flip on squaring, and the
identity on the fourth power. These are integer checks, not numerical
exponentiation of complex matrices.

For a surviving coefficient node v=(I,J), each generator gives one signed
constraint x_target=s x_v. Retain every directed row, including self-loops.
Build an undirected traversal with the same reversible signs, find a spanning
forest and propagate exact coefficients from each root. Check every original
row, including all non-tree rows, against the propagated values. Each forest
edge eliminates exactly one independent coefficient; consistency of all
remaining rows proves they impose no additional condition. Thus rank is
number of nodes minus number of connected components, with no floating-point
rank threshold or assumed consistency.

The canonical and complement sectors cannot mix: their Clifford grades are
r and14-r, which are distinct for r=1,2. Adjacent coordinate transpositions
connect all r-subsets, so each sector is one orbit. Expected degree1 counts
are28 nodes,364 directed rows, two14-node components, rank26 and338 non-tree
rows. Degree2 has182 nodes,2366 directed rows, two91-node components, rank180
and2186 non-tree rows. Non-tree counts INCLUDE loops and redundant directed
rows; they are not dimensions of an independent constraint system.

The propagated coefficients are compared with separately declared invariant
patterns. Canonical coefficients are1. Complement coefficients are
EuclideanBladeSign(full,I), because Omega_E Gamma_I reduces to that sign
times Gamma_Icomplement. The common scale is fixed by the chosen root only
for comparison; it is not a source normalization.

Two prospective sign decoys make these constraints nonvacuous. Applying the
inverse rotation to covectors instead of the correct dual action fails on
each of thirteen generators for all four candidate tensors:52 rows. Dropping
the complement orientation signs and assigning all+1 coefficients fails on
each generator at both degrees:26 rows. Every failure has maximum coefficient
defect2. Exactly2 terms change at degree1, and24 at degree2, because precisely
those form masks containing one of the two rotating axes change sign. Correct
candidate tensors are checked for zero defect in the same loops.

## From an upper bound to the exact REAL dimension

The finite subgroup calculation provides an upper bound, not by itself full
Spin invariance. Its fixed spaces have complex dimension at most two at each
requested degree. Phase590 independently supplies two linearly independent
REAL full-Spin invariant tensors at each degree, with verified simultaneous
form/adjoint invariance and H-anti-Hermitian coefficients. Read the passed
summary, contract and program as exact-bound upstream evidence. The
complexification argument above and these lower bounds force

    dim_R [Lambda^1(V*) tensor_R u(H)]^Spin(7,7) = 2,
    dim_R [Lambda^2(V*) tensor_R u(H)]^Spin(7,7) = 2.

These equalities concern this representation and the specified tensor
degrees. They do not classify nonlinear field-dependent constructions,
different representations or degrees, contractions made from additional
geometric data, or all source actions. They do establish that the canonical
and volume-dual directions span these two particular real invariant spaces.
No direction, coefficient, norm, bracket occurrence or operator is selected.

## Freeze and fail-closed evidence

Nine unique exact file bindings cover own program, project and proof; primary
source text; Phase590 summary, contract and program; its complete core source
manifest; and Directory.Build.props. Verify all726 live src C#/project paths,
excluding bin/obj, every file hash and the sorted path-space-hash-newline
tree digest. The passed590 terminal and all safety flags are checked before
scientific calculations. The entire fixture object and terminal precedence
must match structurally, not merely be present or externally hashed.

All arithmetic is exact integer sign, phase and graph arithmetic; tolerance
is zero. Prospective estimate10 CPU seconds/64MiB, estimate ceilings30 CPU
seconds/128MiB. These are estimates, not execution limits or measured claims.
No dense eigensystem, parameter fitting, sampling or external ruling is used.
Full and summary JSON outputs are deterministic and byte-identical.

Terminal precedence: invalid-or-drifted-input; known-answer-control-failed;
character-census-control-failed; signed-orbit-control-failed;
real-dimension-or-decoy-control-failed;
`invariant-tensor-dimensions-two-certified-source-choice-open`.
Only the last terminal is a passed audit. Preserve failed first outputs;
scientific repairs require a versioned pack. All fourteen authority flags
remain false, externalReviewPending=true and promotedPhysicalMassClaimCount=0.
Phase561 stays closed and O4 pending. No core or frozen historical edits.
