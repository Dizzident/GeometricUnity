# Actual gradients and invariant tensor closure

## Evidence and scope

Amendment A50, Phases594-595. These are exact conditional mathematical
audits, not a source-model selection, physical spectrum, or theory-wide no-go.
Both complete packs received independent and coordinator proof/code/hash
review before their first scientific Release execution. Both first runs passed.
No frozen scientific failure or core change occurred.

Primary source: Eric Weinstein, *Geometric Unity*, April 1, 2021 draft,
Eqs8.7 and9.3-9.7. Local text:
`docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`,
SHA256 `062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Author PDF: <https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf>.
The following deductions are ours; they are not statements of author intent.
The exact operator family, brackets, pairing and admitted variations are
declared in each frozen STUDY and contract. Earlier Phase590 lower-bound and
Phase592 restricted-curvature results retain their historical scope.

## Phase594: retain the action, differentiate it correctly

On both Phase593 three-coordinate carriers, construct the full literal CCA
operator before the finite Gram pullback. Write Q(T)=T wedge T and
Q_T(V)=(T wedge V+V wedge T)/2. For the cubic action
I(T)=gamma/3 <T,K Q(T)>, its raised gradient on the carrier is

`grad I = gamma/3 [K_E Q(T) + 2 Q_T^dagger K_E^dagger T]`.

Adjoints are defined by the actual nondegenerate indefinite Grams, not by
an unweighted transpose. Direct polynomial differentiation independently
agrees in all60 gradient components. All180 Jacobian entries,60 curl
components,30 K-adjoint and30 Q-adjoint checks pass. The gradient covector
has zero curl; the proposed curvature-only force does not (10 nonzero rows).
Ordinary symmetry of the raised-vector Jacobian is the wrong test here;
10 deliberate unweighted tests detect that mistake.

For the first carrier the actual derivative is
`4 gamma/3 (yz,xz,xy)`, whereas the desired covector is `(0,0,4xy)`.
Its desired x/z and y/z curl components are4y and4x. On the second carrier
the actual derivative is `4h gamma/3 (yz,xz,xy)` and the desired covector
is `(4hyz,0,0)`. These formulas refer to the nonzero formal coefficient;
all10 formal-c rows vanish exactly. Both bracket encodings were tested.

Each full-curvature quadratic map has rank1 and kernel dimension5 in the
declared six-monomial symmetric-square basis. The actual gradient does not
annihilate that kernel. Equal-full-curvature pairs also have unequal actual
gradients, excluding even a single-valued nonlinear curvature-only
refactorization on these carriers. This does NOT exclude explicit torsion
dependence, differential operators, or a different variation problem.

A declared Riemann projection changes the inputs: it halves the first
desired force and erases the second curvature. It is not a repair of the
same equation on the same unrestricted inputs. All10 projection rows,
15 mass controls and two genuine cyclic Chern-Simons positive controls pass.
Adding the tested quadratic mass term cannot cancel an antisymmetric curl.
No ambient closure of the finite carrier is claimed.

Frozen contract SHA256:
`fb0a60401127df0575dbfdd4fdfa6cb40cfa885c70659c111c893c39d29f9a7f`.
Identical full/summary output SHA256:
`1b4de79a520c14f9ffe676a3225cb4814bf309feef2bc971a647f52b7b34777b`.
Twelve unique input bindings and the726-file live core manifest pass.

## Phase595: the declared invariant tensor spaces really have dimension two

Complexify the real u(64,64) module with an explicit negative-axis gamma
and dual-coframe phase bridge. Thirteen even sign characters test all
1,720,320 possible coefficients (22,364,160 character decisions). Only
canonical and volume-complement supports survive:28 at degree1 and182 at
degree2. Omitting one character deliberately leaves56 and364.

Signed quarter-turn constraints have ranks26 and180, including338 and2186
non-tree cycle checks. The four connected signed components give upper
bounds2 at each degree. Phase590's real full-Spin invariants supply matching
lower bounds; therefore both real invariant spaces have dimension2.
The finite subgroup is used only for an upper bound. Complex quarter turns
are not represented as real mixed-signature compact rotations.

All212,992 independent Clifford rotation controls,105 phase-bridge checks,
52 wrong-dual and26 wrong-complement controls pass. This closes the missing
invariant-direction lead in this declared module. It does not choose the
coefficients, normalization, pairing, source operator, or dimensional reduction.

Frozen contract SHA256:
`d2d32ff4e3689a05b4d46276b43e20874fc9e73ca259b80413191c43cc7fc54e`.
Identical full/summary output SHA256:
`e42ee13516ce4140a7eb8e37ef4dbf95af4e41af4c08258b16a7aa8f547cc071`.
Nine unique bindings and the726-file core manifest pass.

## Concrete next investigations (not executed or allocated)

The constructive route is to retain the source action and its actual
torsion-dependent Euler derivative. At fixed epsilon, metric and background
B, the action's linear term has gradient K F_B at T=0. Thus correcting the
cubic derivative does not automatically discard the previously successful
Einstein-shaped curvature contraction. Determine the quadratic derivative
and principal symbol next, retaining derivatives of K inside formal adjoints
unless covariant constancy has actually been proved. A Hessian alone is not
a boson spectrum: gauge constraints, kinetic signs, physical field extraction
and units remain separate requirements.

Writing D_B for the fixed background covariant exterior derivative, the
full fixed-background raised T-gradient is

`K F_B + (K D_B T + D_B^dagger(K^dagger T))/2`
`+ gamma/3 [K q(T,T) + 2 Q_T^dagger(K^dagger T)] + kappa T`.

Its T=0 Hessian is `(K D_B + D_B^dagger K^dagger)/2 + kappa I`, with
compact-support or periodic boundary conditions. The cubic coefficient
does not enter this Hessian. T=0 stationarity requires K F_B itself to
vanish, not merely a null self-pairing. These general derivative identities
were independently checked analytically; no successor symbol computation
has yet been executed.

An independent conditional Helmholtz classification is also promising:
for fixed zero-order pointwise linear K, unrestricted full gauge variations,
and a force exactly K F_A, principal self-adjointness requires a spatial
three-form valued in symmetric Lie pairings; cubic reciprocity requires
those pairings to be ad-invariant. Nonconstant coefficients require further
differential conditions. A tiny u(1,1) exact rank/cyclic-control audit and a
general proof could determine which assumptions must change. Full Spin
equivariance must be justified for the actual background, not merely assumed.
This prospective argument does not cover connection-dependent K, differential
K, restricted variables, additional geometric tensors, or the actual corrected
Euler operator. It is not an established source-wide impossibility result.

All fourteen authority flags remain false; O4 pending, Phase561 closed,
externalReviewPending=true, promotedPhysicalMassClaimCount=0. Neither audit
fills the missing physical source contracts. Registry596+ remains free.
