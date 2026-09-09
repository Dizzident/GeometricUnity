# Phase615: source-induced splitting and volume scope

Prospective exact-rational audit under A60 and its Phase615 extension.
Success terminal:
`induced-splitting-volume-controls-pass-homothety-outside-declared-image`.
No scientific execution has occurred when this pack is frozen. A Release
build is not a scientific run. Complete independent and MAIN review plus
explicit MAIN approval are required before FIRST execution.

## 1. Question, source and conditional reconstruction

Phase614 concerns independent uniform scaling of a declared 14-dimensional
metric. This audit asks whether that direction belongs to the source-induced
metric family at a FIXED point y of the FIXED metric-bundle total space Y.
It does not change Phase614's frozen calculation or select a new source law.

The bound primary text gives the fibre-point horizontal metric at lines
888–895 / Eq3.7, vertical double contraction and trace freedom at899–917 /
Eq3.8–3.9, and the chimeric sum and orthogonality at932 / Eq3.10 and954–958.
The downstairs Levi-Civita identification is at1012–1018 / Eq3.15;1022–1049 /
Eq3.16–3.17 constructs the induced upstairs metric and connection. Lines
1060–1065 exclude most arbitrary upstairs metrics. Eq9.1 at2117 declares
MET(X), not unrestricted MET(Y), as the metric variable.

Use the same DECLARED coordinate reconstruction as passed Phase608:

    D = diag(H_y,V_y), H_y=-y,
    V_y(A,B)=Tr(y^-1 A y^-1 B)+beta Tr(y^-1 A)Tr(y^-1 B).

The horizontal sign and beta menu are diagnostic conventions, not uniquely
selected source choices. At fixed y these chimeric blocks are independent
of the downstairs metric h and its connection. The latter enters the map
from the chimeric bundle to TY through the horizontal lift. The inclusion
of vertical vectors is canonical.

This statement does not cover evaluation on a moving observer section
y=h(x), a pulled-back action, boundary or global integration choices, a full
metric Euler equation, metric-dependent reference-curvature terms, or a
global stationary vacuum. Constant volume density does not imply constant
action: inverse metric, Hodge star, induced connection and curvature can vary.
Neither flatness nor physical propagation or a boson mass is inferred.

## 2. Exact jet-to-connection-to-lift isomorphism

At a point, let h be any nondegenerate symmetric downstairs 4×4 metric and
J_i[j,k]=partial_i h_jk, symmetric in j,k. Its 40 independent first-jet
components give the torsion-free connection

    C_i[k,j]=Gamma^k_ij
           =h^kl(J_i[j,l]+J_j[i,l]-J_l[i,j])/2.

Thus C_i[k,j]=C_j[k,i]. Conversely compatibility recovers every jet:

    J_i=C_i^T h+h C_i.

Substituting this identity into Koszul recovers C for any torsion-free C;
substituting Koszul recovers J. These are exact linear inverse maps, not
numerically inferred ranks.

Parallel covariant metrics satisfy nabla_i y=0, hence along the horizontal
lift their coordinate fibre motion is

    L_i=C_i^T y+y C_i.

This is the graph(+L) convention. Replacing plus with minus without
changing which map is being represented is an error. Apply Koszul using y
to these L_i to recover C, then lower using h to recover J. Hence the map
J_h -> C -> L_y is an isomorphism between two 40-dimensional spaces at
every admitted h,y, not just at the two finite controls.

For completeness its determinant is (det y/det h)^10. On the connection
space Sym²(R4*) tensor R4, lowering the contravariant output with h acts
independently on the ten symmetric lower-index pairs, so its determinant
is (det h)^10. Passing between lowered connection components and metric
jets is one fixed invertible permutation/symmetrization (Koszul inverse)
independent of h. Those fixed determinants cancel in the ratio between
lowering by y and lowering by h. This proves the formula without fitting
the finite numerical determinants or assuming a positive definite metric.

The finite run checks every one of the 40 coordinate jets in each of the
four y/beta contexts, all 64 torsion entries for each, reconstruction of
every connection and jet matrix, and the complete 40×40 map. Independent
exact Gaussian inverse and rank/determinant routines check rank40, the
formula above and both inverse products.

Changing the value h while holding J fixed can also change C. It creates
no extra pointwise connection directions: the fixed-h 40-jet map is already
surjective onto all torsion-free C. This is only a pointwise statement. It
does not assert that arbitrary connection fields are globally metric or
that independently chosen jets at every point integrate to one metric.

## 3. Full induced metric, inverse and determinant

Order coordinates as four base directions then the ten symmetric fibre
coordinates. Embed the 10×4 lift matrix into N with only its VH block
nonzero. Then N²=0. The map from chimeric vectors to TY and its inverse are

    P=I+N=[[I,0],[L,I]], P^-1=I-N.

Consequently the metric in coordinate TY is

    G_L=P^-T D P^-1
       =[[H_y+L^T V_y L,-L^T V_y],[-V_y L,V_y]],
    G_L^-1=P D^-1 P^T.

These full block formulas are checked entry by entry against multiplication
by P^-1 and against an independently computed Gaussian inverse. Both
inverse products and symmetry participate. Since det(P)=1,

    det G_L=det H_y det V_y
           =64(1+4beta)/(det y)^4.

The positive prefactor is important: Phase607 gives
det V_y=64(1+4beta)/(det y)^5 and det(-y)=det y in dimension4.
The two signs from det y multiply; introducing another minus is incorrect.
This sign correction was made before any scientific execution.

For beta0 and beta=-1/2 the full inertia is respectively (8,6,0) and
(7,7,0), unchanged by invertible congruence. Sylvester elimination in the
bound helper checks it, with separate diagonal/off-diagonal/singular
known answers. At nonzero L the wrong P^T D P has the opposite mixed block,
and so differs from G. At L=0 the two metrics agree, so that test is
deliberately restricted to the eight nonzero-splitting rows.

## 4. Complete tangent, volume, and homothety controls

For an arbitrary lift variation delta N in the VH block,

    delta G=-(delta N)^T D(I-N)-(I-N)^T D delta N,
    delta G_HH=delta L^T V L+L^T V delta L,
    delta G_HV=-delta L^T V, delta G_VV=0,
    delta G^-1=delta N D^-1 P^T+P D^-1(delta N)^T
              =-G^-1 delta G G^-1.

The run compares the full product-rule tangent against separately assembled
blocks and the exact symmetric difference
[G(N+delta N)-G(N-delta N)]/2. This is an EXACT coefficient identity because
G is a polynomial of degree2, not a floating-point approximate derivative.
The second difference also satisfies

    G(N+delta N)+G(N-delta N)-2G(N)=2(delta N)^T D delta N.

Gaussian inverses of the two endpoint metrics independently recover the
inverse tangent by the same symmetric-difference identity. Their exact
determinants each equal the baseline determinant. None of these routes
substitutes the desired zero volume trace.

Since det G is constant in L and G is nondegenerate, Jacobi's formula gives

    Tr(G^-1 delta G)=0.

Direct inverse-Gram contraction checks this identity in every tangent row.
Every VV entry is checked zero. At L0, both diagonal blocks of delta G
vanish; the HH test is applied to all160 L0 tangent rows and VV is checked
in all480 rows. The mixed block is nonzero for every nonzero jet because
the jet-to-L map and V are invertible. Recovering
delta L=-V^-1 delta G_VH checks all40 independent tangent directions;
therefore the induced metric tangent image has dimension40 everywhere.
Changing the sign of both N and delta N preserves the diagonal derivative
but flips its nonzero mixed block and is rejected in every tangent row.

Uniform homothety delta G=2G instead has trace28 and VV restriction2V,
which is nonzero at every allowed point. In particular its [4,4] entry is

    2(1+beta)/(y00)^2 !=0.

The run records the full homothety matrix and checks the whole VV block,
trace and explicit nonzero witness in all12 rows. Thus homothety is outside
this DECLARED fixed-y image at every L, not merely at L0. This does not
exclude changes of y or other source-admissible variations outside the
scope stated in section1.

Under a constant nonzero scale r, downstairs h -> r h and J -> r J,
Koszul gives unchanged C, since h^-1 supplies r^-1. Therefore L and G at
fixed y are unchanged. The finite positive scales r=1/2,2 are tested for
every jet and every splitting seed. These are scales of h itself, not
the independent 14D lambda² metric homothety in Phase614.

## 5. Full prospective finite menu and census

The finite y menu is diag(-1,1,1,1) and diag(-1,4,9,16); h is the first
of these, alpha1, sigma-1 and beta0,-1/2. The ten symmetric basis order is
E00,E11,E22,E33,E01+E10,E02+E20,E03+E30,E12+E21,E13+E31,E23+E32.
Jet index a=0..39 places basis[a%10] at derivative[a/10].
Seed J_i=(i+1)basis[(3i+1)%10] gives L_seed. Splittings are
0,L_seed,-2L_seed, yielding 4 contexts ×3=12 complete metric rows, each
with all40 independent tangent jets, hence480 tangent rows.

The 33 frozen counts are exact loop counts, not sampled success tallies:

```json
{
  "arithmeticControls": 8,
  "contexts": 4,
  "connectionMapColumns": 160,
  "connectionTorsionEntries": 10240,
  "connectionReconstructionRows": 160,
  "jetRescalingRows": 320,
  "mapRanks": 4,
  "mapDeterminants": 4,
  "mapInverseProducts": 8,
  "metricRows": 12,
  "shearControls": 12,
  "metricBlockEntries": 2352,
  "inverseEntries": 2352,
  "inverseProducts": 24,
  "metricDeterminants": 12,
  "metricInertias": 12,
  "verticalMetricEntries": 1200,
  "homothetyVerticalEntries": 1200,
  "homothetyTraceRows": 12,
  "wrongMetricSignRows": 8,
  "metricRescalingRows": 24,
  "tangentRows": 480,
  "tangentBlockEntries": 94080,
  "centralDifferenceEntries": 94080,
  "secondDifferenceEntries": 94080,
  "inverseDerivativeEntries": 94080,
  "tangentDeterminants": 960,
  "verticalTangentEntries": 48000,
  "volumeTraceRows": 480,
  "mixedNonzeroRows": 480,
  "tangentReconstructionRows": 480,
  "wrongTangentSignRows": 480,
  "zeroSplittingDiagonalRows": 160
}
```


The factors are:4×40=160 jet columns and reconstructions;
4×40×4³=10240 torsion entries; two scales give320 jet and24 metric
rescaling rows. Four map ranks/determinants and two inverse products per
context give4/4/8. Each full metric comparison has12×14²=2352 entries,
each VV metric/homothety comparison12×10²=1200 entries. Each complete
tangent comparison has480×14²=94080 entries; its VV census is480×10²
=48000. Two endpoint determinants give960. Nonzero splitting counts
are4×2=8; L0 tangent counts are4×40=160. Remaining named row counts
follow the 12 or480 rows and are never inferred from observed values.

Eight rational/matrix known answers exercise normalization, inverse,
rank, determinant and inertia including a singular and off-diagonal pivot.
The complete fixture object is embedded in Program.cs and must exactly
equal the entire contract fixture object before any mathematics is run.

## 6. Resources, exact lineage, and failure precedence

All arithmetic is exact arbitrary-precision rational with tolerance0.
No sampling, fitting, experimental calibration, eigenvalue tolerance or
floating-point derivative is used. Matrices are at most40×40, Gaussian
elimination has at most40 pivots, and the immutable608 Ambient helper's
largest dense derivative array has14^4=38416 entries. Retention is exactly
480 tangent rows. Each retains three14×14 text matrices (deltaN,deltaG,
delta inverse) plus scalar metadata; connection rows retain40 small jets.

Planning estimates are60 CPU seconds and128MiB; conservative planning
bounds are180 seconds and512MiB. These are not runtime scientific gates.
The enforceable Matrix.Products ceiling is200000000. It counts calls to
the bound Matrix.Mul only, not every direct Rational multiplication or
the uninstrumented Rank method. Fixed finite loops bound the rest.

A conservative operation allocation is below200 million tracked products:
each tangent has at most26 dense multiplications
and four elimination passes, each Gaussian inverse bounded by4×14³ and
each determinant by2×14³, for under
(26+2×4+2×2)×14³×480 <51 million. The four40-dimensional map inverses,
determinants and inverse-product pairs use fewer than3 million more.
Ambient Gram construction and fixed known-answer/seed work add fewer than
10 million; the ceiling gives substantial margin without adapting to a run.
Sparse loops in actual matrices reduce this bound; no measured time or
product count was used to select it.

Twenty unique exact bindings comprise own Program/project/proof/helper;
immutable600 exact arithmetic and passed summary/contract;
immutable607 helper and passed summary/contract/Program/proof;
immutable608 helper and passed summary/contract/Program/proof;
primary source, core manifest and Directory.Build.props. Every compiled
external helper is bound directly, never copied and modified. This proof
was checked against the full607/608 proofs and helpers. No unexecuted
613/614 output is bound or consumed.

All binding IDs/paths/hashes must be unique and match the code-side full
path dictionary. Upstream600/607/608 passed terminals, exact contract hashes,
known answers, controls and authority flags are required. The complete live
sorted src .cs/.csproj pathset excluding bin/obj must match all726 manifest
entries, every file hash and the sorted path-space-hash-newline tree hash.
The manifest itself is bound. No core source or upstream bytes are changed.

Precedence is invalid/drifted input; known-answer failure; connection-map
failure; metric-block failure; tangent-volume failure; decoy/count/resource
failure; then success. Full and summary outputs are identical deterministic
JSON. All bound scientific files and the contract become immutable at first
execution, whether it passes or fails. First failures are preserved; repairs
require a new version, preregistration and review. Only the unbound
IMPLEMENTATION_P615 note may later record approved results.

All14 authority flags remain false; externalReviewPending=true, Phase561
closed, O4 pending, WZ15/H14 deficits and physical mass claims0. The result
is an admissibility-boundary audit, not a coupled metric/connection vacuum
or a physical boson prediction.
