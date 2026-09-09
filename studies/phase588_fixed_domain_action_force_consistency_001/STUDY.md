# Phase588: fixed-domain action and directional-force consistency

Prospective Amendment A47 deterministic mathematics. Freeze program, project,
this proof, complete fixtures, source tree and upstream artifacts before the
first scientific execution. No core or previous phase is changed. The source
dictionary, physical kinetic pairing and rough-field limit remain unfilled.

## Actual target and independent constant-curvature oracle

CreateUniform4D(n) covers [0,n]^4. Rebuild its topology with coordinates/n
for n=1,2,4, hence a fixed physical [0,1]^4 domain and spacing h=1/n. All
meshes are open; no periodic-ensemble equivalence is asserted. Use actual
CurvatureAssembler, EinsteinianShiabOperator and default CpuMassMatrix
(unit face weights). Theta is fixed to zero, with explicit IncidentAverage
vertex rule. Compare identity id0/none and sd2/id0 coefficient1/2, whose
independent six-dimensional matrices are R=I and R=P_+/2. The Lambda2 basis
is (01,02,03,12,13,23) with standard positive Euclidean Hodge convention.

Physical edge integrals a=integral A use omega=-a and physical residual
u=-C Kreg(-a). The residual sign disappears in S=|u|^2/2. The directional
chain rule remains D_A S[V]=-GradOmega dot integral V. Raw coefficient
gradient norms across different meshes are not continuum force norms.

An oriented Kuhn face has vertices x,x+h chi_U,x+h(chi_U+chi_V), with
ordered disjoint nonempty axis subsets U,V. There are 50 such types. For
m=|U union V|, multiplicity on the open n-grid is n^m(n+1)^(4-m). The
program checks actual global-face counts, rather than cell-face incidences.
Consequently face count is 50n^4+48n^3+12n^2; at n=4 the mesh has 625
vertices, 5936 edges, 16064 faces and 6144 cells. With b=chi_U wedge chi_V,

    G_h = (1/4) sum_types (1+h)^(4-m) b b^T
        = (3/2+2h+h^2/2)I+(3/2+h/2)B^T B,

where B has column e_j-e_i for pair ij. The type sum and closed formula
are checked independently with dyadic exact arithmetic at all three sizes.
At h=0, G has eigenvalues 3/2 and 15/2, each multiplicity three, since
B B^T is the complete-graph Laplacian and B^T B has eigenvalues 0 and 4.
Explicit vectors in both eigenspaces are tested. The identity member's
limiting metric is therefore not a scalar Euclidean metric.

However P_+ B^T B P_+=2P_+, giving
P_+ G_h P_+=(9/2+3h+h^2/2)P_+. This is verified as a matrix identity;
the self-dual image has a scalar limiting factor 9/2 relative to R^T R.
Neither this cancellation nor the full-space anisotropy selects source data.

For the 21 vectors F=e_i and e_i+e_j, use the abelian connection
A_mu=-(1/2)sum_nu F_mu,nu x_nu Jz. Then dA=F and all brackets vanish.
For these exact integrated constant forms the registered action is exactly
S_h=F^T R^T G_h R F/2, with no asymptotic approximation. Check self
directions D S[A]=2S and six additional basis directions at n=1. These
quadratic and directional values determine a pulled-back Gram form, not C.
Therefore all six basis forms at every size/member additionally check the
actual signed per-face residual map Cf=W^T R F/2 independently. Here W
contains unnormalized geometric bivectors, so f=W^T F/2. These direct
checks detect output sign or isometry errors that action checks cannot.
Also compare the full assembled physical residual -C Kreg(-a) with the same
independent expected faces in each direct-basis row.

## Uniform contraction bound, including global averaging

On a unit Kuhn 4-simplex the cumulative vertex-edge matrix T is an axis
permutation of the triangular cumulative-sum matrix. T^-1 is bidiagonal,
with induced 1- and infinity-norms at most 2, hence ||T^-1||_2<=2. Six
columns of W, the faces containing the first vertex, form wedge^2 T.
Thus sigma_min(W)>=sigma_min(wedge^2 T)>=1/4. All 60 entries of the
6x10 matrix W have magnitude at most 1, so ||W||_2<=sqrt(60)<8 and
Q=(WW^T)^-1 W has norm <=4. For both members ||R-I||_2<=1.

The actual cell map M=I+W^T(R-I)Q therefore obeys ||M||_2<=33 and every
row has absolute sum at most sqrt(10)*33<132. Freeze K=132. With geometry
scaled by h, W scales as h^2 and Q as h^-2, so the same bound applies.
This argument uses actual cell shapes and does not assume that arbitrary
varying continuum curvature is representable as a constant form on a cell.
The core averages contributions from cells incident to each global face
with positive weights summing to one. Therefore a uniform per-cell face
error bound remains the same after global averaging; no unbounded cell
incidence or inverse averaging factor is introduced.

The core matrix inversion has an absolute pivot floor 1e-12. All proposed
scales are at least 1/8; the bound above leaves the Gram matrix away from
that very small-scale regime. No core tolerance or geometry rule is changed.

## Independent global smooth error proof

All vector norms in this bound are coefficient l1 norms, including sums over
coordinate and Lie indices. They dominate the Euclidean dot-product norm.
For an affine connection A, let Abar be the sum of absolute constant and
linear coefficients, and Hbar the sum of absolute derivative coefficients.
For an affine direction V define Vbar and Jbar analogously. On [0,1]^4,

    ||F||_1 <= T = 2Hbar+Abar^2,
    ||delta F||_1 <= DT = 2Jbar+2Abar Vbar,
    sum_nu ||partial_nu F||_1 <= L = 2Abar Hbar,
    sum_nu ||partial_nu delta F||_1 <= DL = 2(Hbar Vbar+Abar Jbar).

These deliberately loose estimates follow from the cross-product inequality
||[x,y]||_1<=||x||_1||y||_1. For a face with first vertex p, write each
oriented exact affine edge integral as ell+d, where ell integrates the
constant A(p). All coordinate displacements are at most h, so
||ell||_1<=h Abar and ||d||_1<=h^2 Hbar. The linear boundary sum is exactly
the affine curl integral. Expanding the three pairs in Qreg with its factor
1/2 gives the paired curvature error relative to h^2 b.F(p)/2 bounded by

    h^3 E,     E=3Abar Hbar+(3/2)Hbar^2,    0<h<=1.

Differentiating that polynomial in the direction V gives

    h^3 DE,    DE=3(Abar Jbar+Vbar Hbar+Hbar Jbar).

To compare one cell's output on a target face f, freeze the continuum form
at that target face's first vertex p, NOT at each input face separately.
Every vertex in the same Kuhn cell is within h coordinatewise of p. A
different input face's own first vertex changes its leading curvature by
at most h^3 L/2, and its directional curvature by h^3 DL/2, since each
unit face bivector component has magnitude <=1. Constant F(p) is represented
exactly on all cell faces, so the cell map sends it to R F(p) exactly.
After the actual global averaging, therefore,

    ||u_f-g_f||_1 <= h^3 C,    C=K(E+L/2),
    ||delta u_f-delta g_f||_1 <= h^3 DC,  DC=K(DE+DL/2),
    g_f=(h^2/2)b_f.R F(p_f).

This explicitly includes geometry/averaging error, not only the difference
between Kreg and an independently integrated face curvature. Let r be the
induced l1 norm of R (1 for identity, 1/2 for this SD2 member), and define
U=rT/2, DU=rDT/2, LU=rL/2, LDU=rDL/2. Then |g_f|_1<=h^2 U,
|delta g_f|_1<=h^2 DU. Since the global face count is <=110h^-4,
the total difference between actual and anchored leading actions is at most
110h(U C+C^2/2), and the directional difference is at most
110h(U DC+DU C+C DC).

For each of 50 types, restrict anchors to the n^4 lower grid corners of
[0,1]^4. Their contributions are left Riemann sums of a fixed smooth density.
The per-type action Lipschitz bound is U LU and the directional density bound
is U LDU+DU LU. The extra boundary faces number 48n^3+12n^2<=60h^-3.
Combining quadrature and boundary terms yields the frozen global bounds

    |S_h-S_limit| <= h [110(U C+C^2/2)+50U LU+30U^2],
    |D S_h-D S_limit| <= h [110(U DC+DU C+C DC)
                           +50(U LDU+DU LU)+60U DU],
    S_limit=(1/2) integral_[0,1]^4 F^T R^T G R F,
    D S_limit=integral_[0,1]^4 F^T R^T G R delta F.

All coefficients depend only on the frozen field/menu and proved constants;
no observed error or fitted slope is used. The constants are conservative,
so satisfying a finite-mesh bound is not evidence of small relative error.
The explicit bounds tending to zero prove consistency of the ideal
exact-arithmetic discretization for this prescribed smooth family. Actual
float64 implementation claims are limited to the frozen n/scales: its absolute
pivot floor prevents arbitrarily fine numerical refinement. The finite runs
check the actual implementation against the bounds at those sizes only.
This does not prove solution convergence, coercivity, a continuum
kinetic norm, source action equivalence or any rough-field/quantum limit.

## Smooth oracle and other controls

The two frozen affine noncommuting profiles and two directions are listed
fully in the contract. Independently form F and delta F as coordinate
polynomials of degree at most two. Exact affine midpoint integration supplies
edge coefficients. Exact barycentric triangle moments supply independent
integrated continuum face values, and monomial moments integral x^k=1/(k+1)
on each unit interval supply the limiting action and directional integral
(degree at most four). Arithmetic is float64 with dyadic polynomial
coefficients; rational integration denominators are rounded and checked with
the frozen tolerances. An independent degree-two triangle/cube moment is a
known answer before actual scientific field evaluations.

Apply actual C to independently integrated curvature and variation as a
finite-mesh insertion oracle. Report its action/directional differences
separately from the proved global limit bounds. This insertion oracle shares
C and therefore does not independently validate C. The direct constant-form
residual controls serve that purpose on the represented image.

Check the reverse-pass directional gradient independently through quadratic
curvature polarization and both Richardson pairs. With central derivatives
D(e) at e=2^-8,2^-10,2^-12, the quartic objective has only an e^2 central
derivative truncation term. Use (16D(e/4)-D(e))/15 for both successive pairs;
both must pass, with no best-step selection. Freeze scaled force error 1e-8,
constant/direct/homothety scaled error 1e-10, and zero-anchor absolute error
1e-12. The wrong chain-rule sign must fail at every nonzero constant fixture.

Zero and constant-flat connections check zero curvature, objective and both
gradients. Separately homothetically shrink the fixed n=1 complex at
1,1/2,1/4,1/8. Use two prospectively chosen representatives (first basis and
first pair sum), since 21 forms are already covered by the fixed-domain arm.
Check action/directional scaling h^4 and C invariance on a fixed face-vector
probe. These 16 rows retain fixed topology and shrinking physical volume;
they cannot demonstrate fixed-domain refinement or continuum convergence.

## Freeze, resources and authority

Complete row counts: 126 constant action/self-direction, 36 direct residual,
252 cross-direction, 12 zero/flat anchor, 16 homothety and 24 smooth rows;
48 Richardson pairs. Exact-bind the program/project/proof, source text,
Phase583/584/586 summaries and contracts, Directory.Build.props, and a
manifest of all 726 live src .cs/.csproj files excluding bin/obj. Standalone
validation checks complete structural fixtures, binding identities and live
tree closure. No RNG, chains, protected seeds or core edits.

The maximum mesh has 6144 cell maps with 100 doubles each (under 5 MB of
raw cell-map payload), 17808 edge coefficients and 48192 face coefficients.
Only one fixed-domain mesh/member is active at a time. Counts, temporary
geometry, polynomial objects and reverse-pass arrays motivate a conservative
256 MiB peak estimate below the 512 MiB ceiling. There are 166 analytic
gradient calls and 144 smooth objective-only finite-difference calls; a
prospective 120 CPU-second estimate is below the 180-second estimate ceiling.
These are estimates, not a measured benchmark or enforced process-memory cap.

Terminal precedence is invalid/drifted inputs, known-answer/topology failure,
analytic force parity failure, exact action/pairing failure, global smooth
bound failure, then
`fixed-domain-action-force-controls-pass-induced-pairing-scoped`.
Any failed frozen run is preserved; scientific changes require a versioned
successor. The 14 inherited authority flags remain false, Phase561 stays
closed, external review is pending and promotedPhysicalMassClaimCount=0.
