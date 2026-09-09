# Source Clifford tensors and typed curvature contraction

2026-09-08, Amendment A48 / Phases590-591. Internal deterministic research,
following the [A47 proposal](RECONSTRUCTION-ACTION-SECTION-20260908.md).
No source choice, physical model selection or mass prediction follows.

Primary source: Eric Weinstein, *Geometric Unity: Author's Working Draft v1.0*,
2021-04-01, [author-hosted PDF](https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf),
SHA256 `3f28d742234a9841fc8e51ff172053200aa3eddf3ece38154a3328b9ebd186d4`.
Local text `texts/GU-DRAFT-2021-TEXT.txt` SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Eqs8.1,8.5-8.7 define available contraction tools and invariant tensors;
Eq9.3 supplies the displayed Hodge chain. The A46 visual correction preserves
its outer Hodge star and explicit coefficient1/2. The source's lost final
operator is not recovered merely by testing that displayed construction.

## Prospective real-form and tensor controls

In signature(7,7), let gamma0 through gamma6 square to+I and the remaining
seven square to-I. The prospective H=gamma7...gamma13 is Hermitian, squares
toI and anticommutes with a positive gamma, giving balanced signature(64,64).
The blade identity B_r^dagger H=(-1)^(r(r+1)/2) H B_r yields real grades1,2
modulo4 and imaginary grades0,3 modulo4 in the declared anti-Hermitian real
form. Independent exact Clifford arithmetic will challenge the existing
matrix builder. Its raw chirality-square -I is an expected convention decoy,
not an instruction to modify core code; the normalized volume squares toI.

Canonical tensor components gamma_a and gamma_a gamma_b define one admitted
candidate at form degrees1 and2. Independent pre-run review identifies
volume-dual companions Omega gamma_a and i Omega gamma_a gamma_b. Since
Omega commutes with Spin, these give additional candidates in grades13 and12.
Their different Clifford grades establish independence. Phase590 will check
simultaneous form/adjoint invariance and the real form for all four tensors:
this proves an at-least-two lower bound, not a complete multiplicity theorem.
The canonical norms under -ReTr(XY)/128 are predicted to be -14 and91.
Indefinite normalization is substantive; do not silently set source scales.

## Prospective canonical curvature contraction

Phase591 restricts to the declared canonical pair, with formal real lambda1
and lambda2, not the full invariant-tensor family. It compares literal sparse
wedge/Hodge/Clifford operations to independent Ricci/scalar contractions on a
proved3185-element algebraic Riemann basis. Flat, single-plane, constant-
curvature and nonzero Ricci-flat Weyl fixtures prevent vacuous agreement.
Off-subspace decoys distinguish algebraic Riemann identities from arbitrary
adjoint-valued gauge curvature. No continuum dynamics or spectrum is computed.

Order the three bracket occurrences as first, outer-second and inner-second,
with C=[,] and A=i{,}. The four tied-Phi1 choices CCC,CCA,AAC,AAA form the
primary declared family; all eight independent choices are a deliberately
enlarged diagnostic. Under the frozen prospective conventions, the predicted
canonical formulas are minus Ricci for CCC,CCA,CAC; minus Ricci plus half
scalar curvature for CAA; zero for ACC,ACA,AAC; and pure half-scalar for AAA.
Lambda1 multiplies the first term and lambda1 lambda2 the second. The
Einstein-shaped CAA branch is not in the tied family and is not selected by
the source. Noncanonical invariant directions remain untested by this phase.

## Further prospective lead: tied brackets with companion tensors

Independent algebra review considers Phi1=(a+b Omega)gamma and
Phi2=(c+i d Omega)Gamma2 with real formal coefficients. For algebraic Riemann
input, let J denote the Ricci-valued Clifford one-form, G the canonical
Clifford one-form and R scalar curvature. The predicted tied CCA result is

    K=-(a+b Omega)J-(d R/2)(b+a Omega)G.

Matching -(a+b Omega)(J-RG/2) as a full Clifford-valued identity requires
db=-a and da=-b. Nontrivial solutions have d=+/-1 and b=-d a; they are
chiral and have zero squared norm14(-a^2+b^2) under the declared trace
pairing. The invertible a^2!=b^2 cases do not meet this matching condition.
This is a concrete proposed exact parameter-polynomial/chiral-control test,
not executed by591 and not a no-go for all tensors or other source pairings.
Nullity under this declared pairing alone does not establish inadmissibility
under an unspecified source normalization. A physical chiral projection and
the resulting action/kinetic interpretation remain separate, unproved choices.

## Execution record and boundary

First scientific execution requires coordinator-reviewed frozen code, proof,
complete fixtures, exact input/core-tree hashes, tolerances, predictions and
terminal precedence. The earlier sections record prospective requirements;
executed results are recorded below. Failed frozen runs are preserved;
repairs require versioning.

Phase590 passed its first frozen Release run. All229376 dense matrix entries,
16384 blades,229376 right-generator products and19110 matrix commutators
agree exactly with their independent controls. All364 tensor/generator rows
have zero invariance residual. Canonical/companion norms are -14,91,14,-91
with zero cross pairings. The deliberate wrong-dual-sign controls fail91 times
per tensor; the explicitly mixed-only wrong-metric controls fail49 times.
An all-Euclidean replacement would also affect21 negative same-signature
generators, but that broader decoy is not the frozen implementation.

The H signature(64,64), raw chirality-square -I and diagnostic normalized
volume-square I are confirmed, without changing core code. Thus the tensor
nonuniqueness lower bound is an executed result, not only a speculative lead.
All twelve exact bindings and the726-file live source tree match.
Contract SHA256 `028664b9aba7241c2c63da46fe13799d444b893bef08634cc4cd15cb48a4a4ac`;
identical full/summary SHA256
`21d3e4b1bbcb54a030f4c2de56eef909503405a9f414eac713ed9134a7ae365d`.

Phase591 also passed its first frozen Release run. The155926 independent
Clifford-word controls and16384 Hodge masks pass exactly. The3185 basis rows
have distinct pivots and zero Bianchi residual; all25480 branch comparisons
(12740 in the primary family) match the independent Ricci/scalar oracle.
Because both sides are linear in the algebraic curvature tensor and the
basis spans, the conditional identities cover that full declared vector space,
not merely a numerical sample. The five anchors pass, including a nonzero
Weyl tensor annihilated by every canonical branch. No lambda is selected.

The off-Riemann controls produce the anticipated nonzero Clifford grades:
the pair-asymmetric inner C has grade2; the Bianchi-violating first A has
grade3 and its inner A is -2i gamma0123. The positive-plane scalar inner
term is +iI, while the wrong-top-star decoy gives -iI. Thus neither the zero
branches nor their Clifford-grade preservation extend automatically to
arbitrary gauge curvature. The source-inspired contraction is constructed
conditionally at epsilon=I; no registered-model or physical equivalence follows.
All eight exact bindings and the same726-file source tree match.
Contract SHA256 `0ff70e732c234cb1426c6a3b16f948b7a84cec6250bf70003d61ab0d8a84683f`;
identical full/summary SHA256
`0de24e16dbafee7ec713f8cabc8a69ca45f14a3a080da98031a5b9b366d1c797`.

All fourteen authority flags remain false, O4 remains pending, Phase561
remains closed and promotedPhysicalMassClaimCount=0. No core changes,
new sampling, historical-terminal replacement, target fitting or production.
