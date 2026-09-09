# Phase587: exact residual factorization audit

Prospective A47 deterministic mathematics. Coordinator review and explicit
first-run approval are required after freezing this pack. No core changes,
source-parameter choices, sampling, physical spectrum, or old-result revision.

## Precise question and direction

Can any field-independent LINEAR map N reconstruct the full registered residual
M F(A) from the constant-two-form reconstruction Q F(A), for EVERY retained
edge coefficient field A on a single standard oriented4-simplex?

The predicted answer is no on the full nonlinear coefficient space, while the
flat linear tangent and projected residual satisfy the proposed factorization.
This is not a claim about all maps from registered residuals to source residuals.
It does not exclude nonlinear maps, higher-order reconstruction, every scalar
action equivalence, or continuum consistency. No source N is constructed.

## Literal source bookkeeping

The original draft Eq9.3 on PDF page43 has an outer Hodge star in its second
term, displayed as the numerator of */2. For bracket/wedge maps E_r using
epsilon^-1 Phi^r epsilon and star_k:k-forms->(d-k)-forms, it reads

    S_e = E_1 star_2 - (1/2) star_1 E_1 star_d E_2 star_2.

Composition is right to left. The degree chains are 2->d-2->d-1 and
2->d-2->d->0->1->d-1. The literal d=14 source carrier and a declared d=4
reduction are distinct. Lowering by the actual inverse star_1 gives
U_lower=K_e F+kappa T in a common one-form carrier. No missing Hodge sign,
Phi components, Clifford representation, normalization or pairing is invented
by this audit. Recovering the printed example does not choose the author's
lost final operator. The prior physics-decisions transcription is retained.
Visual provenance and the corrected expression are recorded in
`docs/Reference/ExperimentReferences/SIGNED-SECTION-ACTION-LEADS-20260908.md`.
The primary PDF SHA256 is
`3f28d742234a9841fc8e51ff172053200aa3eddf3ece38154a3328b9ebd186d4`.
The extracted source text is exact-bound here; no network content is needed
at execution. This bookkeeping motivates the reconstruction question, without
identifying the reduced two-form operator with the literal source expression.

## Exact registered reconstruction

Freeze vertices v0=0,v1=e0,v2=e1,v3=e2,v4=e3 and all ten ascending edges
and faces. Check the repository topology builder against the complete frozen
incidence arrays. There is one cell, not the multicell CreateUniform4D(1)
mesh or the sampled periodic ensemble. Lie pairing is compact su(2) trace;
theta=0, member sd2/id0, Einstein coefficient1/2, unit face weights.

The actual core constructs W from face bivectors, Q=(WW^T)^-1 W,
P=W^T Q and M=I+W^T(R-I)Q. Independently form these matrices with normalized
BigInteger rationals, with R=P_+/2=(I+star)/4 in basis(01,02,03,12,13,23).
Thus

    M=(I-P)+W^T R Q,       Q W^T=I6,       P^2=P=P^T.

W,Q,P and the edge-to-face incidence D have rank6. M has rank7: rank4
on the complement of representable forms and rank3 on its self-dual component.
Tensoring each map with the Lie identity triples these ranks. Because constant
two-forms on this simplex produce closed face cochains and dimensions agree,
im D=im W^T. Therefore PD=D and MD=W^T R QD. MD has rank3 per Lie component.
Counting ten faces alone would incorrectly reject this flat-tangent control.

## Polynomial coefficient witness

Write the registered curvature F(A)=D A+F2(A), with its registered boundary
pair order unchanged. For face i<j<k, let u have Jx only on edge ij and v
have Jy only on edge ik. The independently assembled mixed coefficient is

    F(u+v)-F(u)-F(v)+F(0) = -Jz/2 on face ijk, zero on all other faces.

The two nonzero edges share exactly one face. Its registered signed pair is
(+u,-v), so the factor is -1/2. Repeat for cyclic generator pairs
(Jx,Jy,Jz),(Jy,Jz,Jx),(Jz,Jx,Jy): thirty fixed columns span the full
30-dimensional face/Lie carrier. Thirty same-generator pairs are commuting
zero controls. Each column is independently obtained from the rational bracket
formula and actual core polarization and compared to the isolated-face answer.

Freeze the tetrahedral boundary vector

    n=(e123-e023+e013-e012) tensor Jz.

Geometric closure gives Qn=0. The actual registered M gives Mn=n and its
positive unit face/Lie norm is n^T n=4. Build n independently as the prescribed
linear combination of the thirty recovered mixed columns. If the proposed
linear residual equality held for every A, equality of polynomial coefficients
would imply Mn=NQn=0. This contradicts n nonzero. The argument uses linearity
of N and the direction of reconstruction explicitly.

It does not assert that every linear combination of mixed coefficients is
itself the curvature of one field. Equality of linear maps on a polynomial
for every field implies equality on each coefficient, and hence their span;
that is all the proof uses. Nor does it turn coefficient-span rank into a
no-go for scalar action equality: polynomial Gram representations can be
nonunique.

The positive projected control is MP=W^T R Q and MPn=0. This map is evaluated
only in the study; the registered operator is not changed. Check the complete
Gram-matrix identity (including all off-diagonal terms)

    M^T M = (I-P)^T(I-P) + (W^T R Q)^T(W^T R Q).

It verifies the positive orthogonal norm split on the coefficient carrier,
without inferring a source quadratic form or physical kinetic metric.

## Standalone controls, freeze and outcomes

Before source/upstream interpretation, exact known answers test rational
normalization and arithmetic, a matrix inverse, singular rank and the Lie
cross product. There are thirty actual linear-curvature columns and thirty
actual contraction columns, the thirty noncommuting and thirty commuting
polarizations, the member matrix comparison and actual witness pairing. The
actual source routines are compared against independently assembled exact
matrices. Every scientific fixture, expected rank/count and tolerance is
structurally validated against an embedded complete fixture menu.

Seven distinct exact file bindings cover program, project, this proof, primary
text, Phase586 summary, build properties and the core-source manifest. The
manifest is copied unchanged from Phase586 and covers726 cs/csproj source
files, excluding bin/obj. Standalone validation checks its exact live file list,
every file hash and the SHA256 of sorted path-space-hash-newline entries.
All fourteen Phase586 authority keys and terminal precedence are also checked.

Exact rational tolerance is zero; actual float64 comparison tolerance is1e-12.
Estimated runtime5seconds is below the frozen10second estimate cap; estimated
peak128MiB is below the256MiB cap. These are preallocation estimate checks,
not timing-dependent scientific outputs. Build checks precede the freeze and
are not scientific execution. All outputs are deterministic and the full and
summary JSON bytes are identical. Preserve failed frozen outputs; any scientific
repair requires a versioned successor.

Terminal precedence:

1. invalid-or-drifted-input
2. known-answer-control-failed
3. registered-map-or-polynomial-control-failed
4. nonlinear-residual-factorization-obstructed-flat-tangent-control-passes
5. expected-factorization-obstruction-not-confirmed

The fourth is the predicted successful audit; the fifth preserves an unexpected
witness result after the other controls pass. No terminal selects a source
operator or measure, changes prior results, opens Phase561, discharges O4,
authorizes sampling/production, or supports a physical mass claim. Every output
retains externalReviewPending=true and promotedPhysicalMassClaimCount=0.
