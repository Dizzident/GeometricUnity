# Residual reconstruction, fixed-domain action and section density

2026-09-08, Amendment A47 / Phases587-589. This record follows the reviewed
[A46 derivations](SIGNED-SECTION-ACTION-LEADS-20260908.md). It is an internal
deterministic research record, not a selection of source theory or a particle
prediction. The user requested continued parallel work on concrete next tests.

Primary source: Eric Weinstein, *Geometric Unity: Author's Working Draft v1.0*,
2021-04-01, [author-hosted PDF](https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf).
PDF SHA256 `3f28d742234a9841fc8e51ff172053200aa3eddf3ece38154a3328b9ebd186d4`;
local `texts/GU-DRAFT-2021-TEXT.txt` SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Section6 motivates the conditional quotient dictionary; sections8-9 the typed
operator and action. No external source supplies the new test conclusions.

## Exact reconstruction question

For the registered contraction on one4-simplex, Q=(WW^T)^-1 W, P=W^T Q,
and M=(I-P)+W^T R Q. The proposed dictionary M F(A)=N QF(A), with a fixed
linear N, would reconstruct every registered residual from the constant
two-form data. Phase587 tests this precise direction of factorization. The
linearized positive control PD=D prevents a dimension-only rejection of the
flat tangent. Polarized nonlinear curvature coefficients and an explicit
tetrahedral boundary provide an exact, prospectively selected challenge.

The theorem is not about arbitrary nonlinear N, every map from M F to a
source residual, equality of nonlinear scalar actions, or all continuum
reconstructions. The single-cell unit-metric split must not be exported to
a globally averaged multicell operator without proof. A favorable smooth
limit can coexist with a finite-mesh reconstruction obstruction.

The A46 visual correction of printed Eq9.3 remains relevant: its second term
has an outer Hodge star and coefficient1/2. The displayed operator can be
typed before its invariant components or a reduced-source correspondence
are selected. Historical bound artifacts remain unchanged.

## Action and force on a fixed physical domain

Phase588 compares n=1,2,4 open triangulations of [0,1]^4, rebuilding coordinates
from CreateUniform4D(n). It separately treats homothety of a fixed complex as
shrinking-domain scaling. The actual default unit face weights are retained.
An independent face census predicts the finite-mesh pairing, including its
boundary terms, on constant curvature. Full-form anisotropy is distinguished
from the potentially scalar restriction to the self-dual image.

Direct per-face residual controls test more than the pulled-back Gram form:
an output sign or isometry could preserve action and directional values.
The physical coefficient dictionary omega=-integral A also requires the
minus sign in D_A S[V]=-GradOmega dot integral V. This is a smooth directional
functional, not a continuum norm of the raw coefficient gradient. Noncommuting
affine profiles, independent triangle moments and prospectively derived
remainder bounds challenge transfer beyond the constant abelian controls.

Neither this bounded classical consistency nor the induced face pairing is
a proof of quantum convergence, source kinetic normalization or physical poles.

## Section density and one literal joint lift

Phase589 keeps two questions distinct. A continuum section chosen before
discretization need not retain the removed finite right action. In contrast,
identifying the existing joint operator as an unreduced model whose gauge
fixing yields the sampled section requires an actual invariant joint action
and measure. The finite Ward battery tests one explicit projected law only.

The local derivative of Log(epsilon exp(alpha)) at epsilon=I is an ordinary
coordinate identity. Its determinant cannot manufacture an invariant action,
global section or density. Distinct invariant weights on a Gaussian compact
toy give different quotient moments despite identical coordinate and local
FP determinants. The auxiliary HMC momentum metric is also not the physical
kinetic pairing.

The twelve frozen smooth Ward cases use exact periodic lifted-edge integrals,
nonconstant noncommuting fields, origin/global controls and two derivative
steps. Zero, intermediate and decisive bands are separate. A controlled
decisive witness rejects the specified literal lift even if another row is
intermediate; required arithmetic/input failures still dominate. Neither
scientific outcome selects a source measure or excludes section-first work.

Pre-execution review identified a convention mismatch in the earlier compact
Jacobian illustration: Phase585's named cocycle C_e=R^-1 B-B gives
Phi=R(c p+B)-B, while its separate Jacobian/cutoff controls used the opposite
affine background shift. Their determinant and cutoff counterexample remain
valid as affine-map controls; they must not be identified with that particular
cocycle's quotient map. Phase589 uses the consistent formula and directly
checks compact right invariance. Frozen Phase585 bytes and its determinant
conclusion remain unchanged.

## Constructive follow-up: recover admissible source tensors

The original Eq8.7 specifies invariant tensors in
[Lambda^r(R^(7,7)) tensor u(64,64)]^Spin(7,7), not merely the registered
Lambda^2 endomorphism labels id0/sd2/asd2/vol4. A constructive successor can
solve these infinitesimal invariance conditions and type-check Eq9.3, instead
of choosing an arbitrary omitted map. This is a proposed algebra task, not
an executed study or a uniqueness claim for the invariant tensors.

There is existing reusable machinery: `Gu.Phase4.Spin/GammaMatrixBuilder.cs`
builds mixed-signature Clifford matrices, but `CliffordAlgebraFactory.cs`
restricts its convenience factory to dimension4. The validator checks the
Clifford relations and chirality; it checks ordinary Hermiticity only in the
Riemannian case. Passing it alone does not certify the source's u(64,64)
real form, invariant tensors, or the full typed source operator.

An explicit algebraic candidate for a prospective mixed-signature control is
H=gamma_7 ... gamma_13, with seven Hermitian positive gammas and seven
anti-Hermitian negative gammas in source signature(7,7). Anticommutation
predicts H^dagger=H, H^2=I, and gamma_mu^dagger H+H gamma_mu=0. Its trace
vanishes by conjugating with a positive gamma. These identities motivate a
balanced Hermitian metric and can be checked independently using exact
Clifford blade arithmetic and the existing matrix builder. They are not a
checked-in execution result, and the builder's chirality convention must
also be tested rather than presumed correct in mixed signature.

Independent algebra review derives the more general blade identity
B_r^dagger H=(-1)^(r(r+1)/2) H B_r. Real Clifford grades1,2 modulo4 are
H-anti-Hermitian; multiplying grades0,3 modulo4 by i makes them so. This
matches the pattern stated in source Eqs8.5-8.6, without choosing Phi^r.
For signature(7,7), the raw volume element Omega has Omega^2=I, whereas the
builder's chirality i^7 Omega squares to -I. This is a prospective convention
control, not an instruction to change core code: the existing4D factory already
documents a related local rephasing, and the raw builder has limited validation
scope. No mixed-signature numerical study has been executed in A47.

Only after that control should a successor construct candidate solder/Clifford
one- and two-form tensors, verify simultaneous form/adjoint Spin invariance,
retain all allowed commutator/anticommutator and normalization choices, and
evaluate the literal Hodge/wedge chain. Dimension and signature reduction,
projection onto retained fields, kinetic pairing and operator selection remain
explicit further steps. Do not identify the source Phi^r with the unrelated
quotient field Phi or infer a unique author choice from one valid candidate.

The independent follow-up supplies a bounded prospective experiment. Set
Phi1=lambda1 sum_a theta^a tensor gamma_a and
Phi2=lambda2 sum_(a<b) theta^(ab) tensor gamma_a gamma_b, retaining formal
real normalizations. Simultaneous Spin invariance is checked using
[Sigma_ab,gamma_c]=eta_bc gamma_a-eta_ac gamma_b together with the dual-form
action. Freeze star(theta^I)=sign(I,Icomplement) product_(i in I)sigma_i
theta^(Icomplement), hence star^2=(-1)^(r(14-r)+7) and star_top(vol)=-1.
Use the actual inverse of star on one-forms when converting outputs.

For declared spin curvature F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d
gamma_c gamma_d, a proposed exact check predicts that the first commutator
contraction gives minus Ricci, whereas the first i-anticommutator vanishes
by Bianchi. The inner commutator vanishes by pair symmetry; the inner
i-anticommutator predicts +(i/2)Scal times identity. These are independently
reviewed algebraic expectations to test, not executed results.

There is a crucial choice boundary: denote the first, outer-second-term and
inner-second-term bracket choices by C=[,] and A=i{,}. The enlarged CAA
branch predicts lambda1[-Ricci+lambda2 Scal gamma/2], but it gives different
brackets to the two Phi1 occurrences. Eq8.1 permits C/A generally; Eq9.3
does not fix independent choices at each occurrence. A successor must test
the four tied-Phi1 branches as its primary family and all eight independent
branches only as an explicitly enlarged diagnostic. Do not silently promote
CAA to an author-selected or necessarily author-permitted Einstein operator.
Under the declared pairing -ReTr(XY)/128, the canonical tensor squared norms
are -14 and91; the source's word 'normed' also does not set both lambdas to1.

The minimal proposed battery uses signed-permutation matrices from the existing
14D builder against independent exact Clifford blades, then literal sparse
wedge/Hodge operations against separately contracted Ricci/scalar formulas.
An explicit3185-dimensional algebraic Riemann basis has91 diagonal,
1092 shared-index and2002 Bianchi combinations (two per distinct quadruple),
each with at most four nonzero symmetric pair-matrix entries. Check all eight
branches with flat, single-plane, constant-curvature and Weyl-kernel controls.
Estimated budget60seconds/512MiB; no dense full-operator diagonalization.
Freeze actual fixtures, operation conventions, resource estimates and terminal
precedence before execution. No successor phase is allocated in A47.

## Execution record and boundaries

Phase587 passed its first frozen Release run in approximately0.28seconds.
Exact ranks are W,Q,P,D:6, M:7, MD:3 per Lie component; the nonlinear mixed
coefficient span has rank30. All30 noncommuting and30 commuting polarization
controls agree exactly. Maximum actual contraction-column error is1.11e-16,
witness error2.22e-16, below the1e-12 frozen tolerance. The witness norm squared is4;
flat-tangent, projected-residual and full Gram controls pass. All seven input
bindings and the726-file live core manifest match. This confirms precisely
the stated nonlinear reconstruction obstruction, not a physical no-go.

The study proofs, prospective contracts and implementation notes are the
authoritative execution records. First scientific execution requires reviewed
frozen code, complete fixtures and exact input/source-tree bindings. Failed
frozen runs must be preserved and scientific repairs versioned.

Phase588 passed its first frozen Release run:126 constant-action,36 direct
residual,252 cross-direction,12 anchor,16 homothety and24 smooth rows, with48
Richardson comparisons. The independent face pairing is
G_h=(1.5+2h+0.5h^2)I+(1.5+0.5h)B^T B. Its full-form limit has eigenvalues
1.5 and7.5, but Pplus G_h Pplus=(4.5+3h+0.5h^2)Pplus. Thus full-form
anisotropy alone does not obstruct the self-dual image. Maximum constant
action error2.22e-16, direct signed residual error2.22e-16 and Richardson
error6.064e-10 pass the frozen tolerances. The ideal exact-arithmetic smooth
O(h) bounds pass but are deliberately loose, not accuracy certificates:
the four-component SD2 action at n=4 is1.8532481 versus limit1.5692139.
Some zero-limit directional rows are nonmonotone. No fitted slope is used.
The actual float64 implementation is tested only at the frozen n/scales;
its absolute inversion pivot floor prevents an unqualified h->0 assertion.

Phase589 passed all frozen controls and rejected the specified literal joint
completion: ten zero-band rows, no intermediate rows and two decisive Ward
witnesses, both on the nonconstant field/periodic generator. Their normalized
defect is approximately1.52727e-5, above the1e-6 decisive threshold. Maximum
normalized derivative-check error2.020e-9 is below2e-7; exact Fourier/direct
Gauss integrals differ by at most4.444e-16. The corrected compact map is right
invariant to1.571e-16; the opposite-shift decoy has defect2sqrt(2).
Equal Jacobians/invariance still allow Gaussian radial moments3 and1.5.
This excludes one off-slice projected completion, not the theta=0 sampled
target or section-before-discretization. Source density remains unselected.

Deterministic full/summary SHA256 values:

- Phase587: `f5096d6765eada94cdca6f24c43cd9e35d6b8a8a9897532e32324b5ce0a9c9eb`.
- Phase588: `7dec161d84073ff72fdfadbacd63534538b7f829690edaa7ce775d2227caeef4`.
- Phase589: `7e448c82d4f0492ebab2dbf111cb90293f353b64a9fc4990d28e9f5d47e29c3a`.

No old terminal or core operator is changed. O4 remains pending, Phase561
remains closed, all fourteen authority flags remain false, and
promotedPhysicalMassClaimCount=0. No new sampling, target fitting or production.
