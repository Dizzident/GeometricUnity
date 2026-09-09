# Companion tensors, chirality and assumptions still requiring tests

Date: 2026-09-08. Amendment A49, Phases592-593. Internal conditional mathematics,
not an author-selected operator, external ruling or physical prediction.

## Source and lineage

Primary source: the author's [April 2021 draft](https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf),
local transcription `texts/GU-DRAFT-2021-TEXT.txt`, SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Eqs8.1 and8.7 motivate the invariant tensors; Eq9.3 supplies the typed
Hodge chain; Eqs9.4-9.7 assert an action/variation relation; Eq9.11 writes a
second-order squared residual without specifying a positive pairing there.
The PDF's coefficient1/2 and outer Hodge were visually checked in A46.
See `SOURCE-CLIFFORD-CONTRACTION-20260908.md` for exact590/591 lineage.

Phase592 retains d=14, signature(7,7), Omega=gamma0...gamma13, Omega^2=I,
and H=gamma7...gamma13. Its Phi tensors are fixed invariant coefficient
tensors, not the earlier quotient field with a similar name. Tying the two
Phi1 bracket occurrences is a declared family restriction, not author intent.

## Registered companion-family question

Set Phi1=(a+b Omega)G and Phi2=(c+i d Omega)Gamma2 with formal real
a,b,c,d; here the parameter d is not the dimension. C means XY-YX,
A means i(XY+YX), and branch letters label first/outer/inner occurrences.
Write R for scalar curvature and J for the Ricci Clifford-valued one-form.
The independent predictions for the lowered typed chain are:

| Branch | Conditional algebraic-Riemann formula |
| --- | --- |
| CCC | -(a+b Omega)J |
| CCA | -(a+b Omega)J - (d R/2)(b+a Omega)G |
| AAC | 0 |
| AAA | (c R/2)(a+b Omega)G |

The universal CCA match with -(a+b Omega)(J-RG/2) requires
a+bd=0 and b+ad=0. Nontrivial solutions are a!=0, d=+/-1,
b=-da, with c arbitrary; the trivial solution a=b=0 must remain separate.
These conditions concern every algebraic Riemann tensor, not a fit on a
scalar-flat example and not arbitrary ad-valued curvature.

The declared diagnostic pairing is the form-metric contraction of
-ReTr(XY)/128. Phi1 has squared pairing14(-a^2+b^2); nontrivial matching
solutions are null and a+b Omega is noninvertible. This is a conditional
chiral possibility, not source inadmissibility: Phi1=2a P_-d G maps input
chirality d into output chirality -d. It does not select physical spinors.

For odd Clifford elements, same-output-chiral products vanish. Nevertheless
Tr(P_h gamma_a P_-h gamma_a)=64 sigma_a. Thus a nonzero residual may have
zero self-pairing yet enter a nonzero mixed trace coupling. Eq9.4 is a mixed
torsion/residual action; substituting a residual self-square for it would be
an additional assumption. H makes each spinor chirality isotropic and pairs
opposite chiralities. Algebraic matrix nilpotency is not a kinetic spectrum
and supplies no particle masses.

## Phase592 executed result

The first approved frozen Release run passed in1.04seconds. It checked
44944 independent Clifford-word products, all16384 Hodge masks,3185 proved
Riemann basis rows,12740 branch cases and76440 exact coefficient cases;
five anchors, six parameter rows, symbolic necessity/sufficiency, declared
norm and28 chirality/mixed/H-isotropy cases all passed. The two nontrivial
chiral matching families survive the algebraic test. No frozen failure or
core edit occurred. Worker, separate reviewer and coordinator checked the
complete proof/code/12-binding pack before the first execution.

Contract SHA256: `ffa3f692d45dd763ccecf7778d0e0f28164ec95209efd1b60258b9f67527caf6`.
Program SHA256: `c2be95aafa4aeabb794615f4350539c1667783de00f54523a933fa30eea40987`.
Identical full/summary SHA256: `47fac92eaac8baccdc9e17c559bc5ddbfd97ed975f26844ed2a035e5f35e89be`.
Full details: `studies/phase592_companion_tensor_chirality_audit_001/STUDY.md`
and `docs/Phases/Implementation/IMPLEMENTATION_P592.md`.

## Parallel investigations and registered variation follow-up

Three agents pursued implementation, independent review and fresh assumption
checks. A minimal first-variation successor is allocated as593; the broader
census, invariant classification and pairing tests remain proposals. None
extends Phase592's frozen fixture menu or Phase590's lower-bound claim.

### Highest priority: does the candidate equation follow from its action?

Tensor invariance and the Riemann curvature match do not establish the
Eq9.4-to-Eq9.7 variational identity. Freeze epsilon, metric, tensors and B,
write K=star1_inverse S, and let B_form be the declared trace/form pairing.
Use q(U,V)=(U wedge V+V wedge U)/2, so q(T,T)=T wedge T and
F(B+T)=F_B+D_B T+q(T,T). With bracket convention gamma in {1,2}, test

I_gamma(T)=B_form(T,K F_B)+(1/2)B_form(T,K D_B T)
 +(gamma/3)B_form(T,K q(T,T))+(kappa/2)B_form(T,T).

Compare its direct derivative with B_form(V,K F(B+T)+kappa T).
For C(U,V,W)=B_form(U,K q(V,W)), the cubic derivative is
gamma/3 times [C(V,T,T)+2C(T,V,T)]. At gamma=1 the required equality
is full symmetry of C; gamma=2 instead tests the graded-bracket reading of
the source coefficient. Do not silently settle that notation ambiguity.
The quadratic term additionally needs formal self-adjointness of K D_B.

A small proposed battery uses positive axes0,1,2 and the16 real H-anti-
Hermitian Clifford directions generated by gamma0,gamma1,gamma2,Omega,
with the correct grade-dependent i phases:48 one-form coefficients.
Retain both matching CCA chiralities and formal c slots. Precompute the
literal K, test48^3 cubic coefficients and the three principal-symbol
matrices J_k(U,V)=B_form(U,K(k wedge V)); formal self-adjointness requires
J_k+J_k^T=0. Reconstruct every reported failure by independent exact
coefficient extraction from I(T+sV). Include an ordinary3D Chern-Simons
positive control. A pass would cover only this explicit carrier.

The useful first-C diagnostic T0=x gamma01, T1=y gamma1 on positive
axes has F01=2xy gamma0, K(F)0=-4xy gamma01, K(F)1=0.
The raw trace cubic is512 x^2 y: its y derivative is nonzero whereas
the proposed y-force vanishes. This analytical candidate is NOT a full
chiral-CCA counterexample. Independent review finds that the full CCA
operator on this same F has coefficients proportional to a+bd and b+ad,
which both cancel at the matching parameters. Test that cancellation as a
control and challenge the full operator, not just its first term.
Torsion also produces curvature outside the Riemann subspace of591/592.

Two independent reviewers subsequently found and checked a cleaner full-CCA
witness, now registered in593: T0=x gamma01, T1=y gamma12+z gamma2.
F01=2xy gamma02 is exactly independent of z; the complete inner-A term
vanishes, and K1=-4xy(a+b Omega)gamma2, K0=0. The proposed force paired
with V1=gamma2 is4a xy, while the cubic action's direct z derivative is
(4 gamma a/3)xy. This survives both matching chiralities, arbitrary c,
and uses only admitted real grades1 and2.

The complementary registered fixture has Phi1=(1+h Omega)G,
Phi2=(c-i h Omega)Gamma2, T0=x gamma02, T1=y gamma1, T2=z Omega.
Its curvature vanishes at z=0, yet the cubic action has a nonzero z
derivative. Omega is an admitted H-anti-Hermitian grade14 direction.
The first fixture would require a common cubic prefactor1, the second0:
even an arbitrary common prefactor cannot repair both. These independent
analytic predictions were confirmed by593's frozen literal-chain/polynomial run.
The constant background is a declared unit-volume flat(7,7)14-torus,
not identified with physical Y. Independently, homogeneous cubic mismatch
cannot be universally canceled by lower-degree background/derivative/mass
terms at fixed epsilon, metric and operator.

Phase593's first approved Release run passed in0.197seconds. All256 word
products,16384 Hodge masks, five operator/witness combinations,10 coefficient
rows and20 action/force polynomial rows passed their predictions:10 nonzero
variation mismatches and10 identically zero formal-c controls. All100
amplitude checks (including40 zero controls),15 mass checks and both genuine
Chern-Simons normalization controls passed. Both chiral reweighting
certificates give incompatible requirements1 and0. An audit pass certifies
this scoped negative finding; it does not certify the candidate field equation.

Phase593 contract SHA256:
`cde88414f1af7790f472d81419250b826f1c25fd9dd844639ebcf98919001fa4`.
Program SHA256:
`6ca291a18e95e95228188e55f47d17af1d662d75662c7a3b834373929ccce788`.
Identical full/summary SHA256:
`67000008c72eddbfe44d9872170cfdf5e3725f333bbbbdb327f4e59d745cb1be`.
See `studies/phase593_companion_action_first_variation_audit_001/STUDY.md`
and `docs/Phases/Implementation/IMPLEMENTATION_P593.md`.
The complete code/proof/10-binding pack was independently reviewed before
coordinator approval and first execution. No frozen failure occurred.

Restricting torsion and variations to one chiral block can hide an ambient
first derivative. A same-chiral action restriction can vanish while an
opposite-chiral variation detects a nonzero residual. No such restriction
is source-selected here. Re-derive variation for a changed tensor family;
do not inherit Eq9.7 automatically or infer that all dynamics vanish.

### Constructive continuation: differentiate the action that was actually written

A separate analytical investigation supplies a concrete repair candidate for
the equation, without changing the action. In this paragraph K is again the
lowered map into one-forms; all adjoints use the fixed nondegenerate declared
trace/form pairings, not a selected positive physical norm. Set
Q_T(V)=q(T,V). Holding epsilon, metric, B and K fixed, integration by parts
under compact-support or periodic boundary conditions gives the true gradient

G=K F_B+(1/2)(K D_B+D_B^dagger K^dagger)T
 +(gamma/3)[K q(T,T)+2 Q_T^dagger K^dagger T]+kappa T.

The adjoint derivative includes derivatives of K unless its covariant
parallelism is established. Replacing this with the desired curvature
residual assumes both quadratic formal self-adjointness and cubic cyclicity.
The same action can be retained and this actual gradient tested by independent
exact differentiation; that changes the asserted equation, not source intent.

Cyclic symmetrization alone does not change the cubic action, since
Sym(C)(T,T,T)=C(T,T,T). It exposes the true derivative. It need not be
representable as K' q(T,T) for another linear contraction K': the symmetric
trilinear coefficients must annihilate the kernel of the linear polarization
map q_lin:Sym^2(E)->curvature carrier for that factorization. Testing only
the nonlinear zero set q(T,T)=0 is insufficient. This is a linear-algebra test,
not a license to insert a new operator or declare the same physical dynamics.

An additional prospective reciprocity test asks whether the desired force
is itself an action gradient on the unrestricted chosen coefficient space.
For a homogeneous quadratic force F(T), any differentiable scalar potential
with that gradient must have cubic part B(T,F(T))/3. Failure of this candidate
is an integrability warning, not merely a wrong guessed action. Freeze the
precise finite coefficient carrier and its nondegenerate pairing, then check
the B-weighted Jacobian symmetry B(U,DF[T]V)=B(V,DF[T]U) and the actual
gradient's corresponding symmetry independently. Equivalently check ordinary
Jacobian symmetry of the force covector, not the force vector in arbitrary
indefinite-pairing coordinates. A failure would not exclude restricted variations,
changed field content, changed force, or other source contractions.

The smallest independently derived prospective carrier is already the first
593 fixture: coordinates(x,y,z) on(theta0 gamma01,theta1 gamma12,
theta1 gamma2), with Gram diag(1,1,-1). Its desired pulled-back force
covector is(0,0,4a xy); its x/z reciprocity defect is4a y, and its y/z
defect is4a x. In contrast, the actual cubic action has covector derivative
(4 gamma a yz/3,4 gamma a xz/3,4 gamma a xy/3), with symmetric mixed
partials. Raising the last index changes its sign. Freeze these full
three-coordinate derivative controls before executing a successor;593 tests
the designated z variation, not this whole reciprocity certificate.

On this same carrier q(T,T) is independent of z, whereas the corrected
cubic force's x/y components depend on z. Thus a pointwise linear K' of
this same curvature alone cannot supply that corrected force. This is a
prospective exact factorization obstruction, suggesting that a constructive
continuation should retain the true gradient's explicit torsion dependence,
change the operator/input ansatz, or declare a justified variation restriction.
A restriction changes the Euler problem: it requires the covector to
annihilate allowed variations, not necessarily the full residual to vanish.

### Invariant-family completeness without a large numerical solve

An independent analytical proof route could upgrade the at-least-two
directions in590 to exactly two at each degree1,2. The involution
X -> -H^-1 X^dagger H exhibits u(H) as a real form of End_C(S).
Complexifying the real invariance kernel preserves its dimension. Rescale
negative Clifford axes by i, then work in the complexified Lie algebra;
this does NOT assume mixed real planes admit compact rotations.
Thirteen even coordinate sign flips(0,j) force I xor J to be empty or
the full14-mask for a theta^I tensor gamma_J coefficient. Only J=I or
J=I-complement survive. Determinant-one signed coordinate rotations connect
all r-subsets, fixing relative coefficients in each family. The two590
real invariant tensors would attain that upper bound.

Proposed exact census:105 form masks times16384 Clifford masks,13 binary
characters, then signed-rotation orbits. Predicted surviving coefficients:
28 at degree1,182 at degree2; two orbits of size14 or91, ranks26 or180,
real nullity2 in each degree. Freeze the signed rotation action and proof
before numerical execution; a character count alone is not the full proof.

### Pairing positivity is an additional assumption

The source's typically normed tensors do not select a positive invariant
spinor-adjoint pairing. Even fixing a nonzero indefinite norm leaves
continuous tensor families. An embedded u(1,1) control can make the issue
precise: X=sigma1, Y=i sigma3, Z=sigma2 satisfy [X,Y]=2Z,
[X,Z]=2Y. Ad-invariance forces B(Z,Z)+B(Y,Y)=0, excluding a
positive-definite invariant real symmetric pairing on this algebra.
Ordinary positive Hilbert-Schmidt pairing fails that invariance identity.
This does not exclude positivity after an additionally specified reduction
or continuation. No such operation is selected here.

## Boundaries

No registered4D/source14D bridge, retained-field map, selected action,
positive kinetic normalization, observed W/Z/H identification or unit law
has been supplied. O4 remains pending, Phase561 remains closed, all fourteen
authority flags remain false and promotedPhysicalMassClaimCount=0.
No core changes, new sampling, target fitting or external ruling occurred.
Registry592-593 is allocated;594+ remains free until a successor is registered.
