# Phase607: source-induced vertical curvature

This is a prospective, deterministic, exact-rational audit under A57. Its
success terminal is
`source-induced-vertical-curvature-controls-pass-flat-reference-not-induced`.
The program has not been scientifically executed when this proof is frozen.
Release build checks do not execute the audit. Independent review and MAIN's
explicit first-execution approval are required before producing evidence.

## 1. Question and precise scope

Can the full induced upstairs reference be flat merely because the downstairs
four-dimensional metric is flat? The declared metric-fiber geometry below
has a nonzero vertical curvature witness, independently of its nondegenerate
trace weight. The source's orthogonal Levi-Civita split then makes that
vertical curvature a component of the induced ambient curvature.

This does not calculate its Shiab contraction. Nonzero curvature does not
imply nonzero contracted curvature: the contraction can have a kernel. No
stationary connection is ruled out by this audit, no full metric Euler
equation is evaluated, and no physical vacuum, time, scale, normalization or
positive norm is selected. Previously declared flat14D controls are not
rewritten; their identification with this particular induced geometry is
the assumption being challenged.

The full mathematical statement is conditional on the explicitly declared
metric family and local block assumptions. The finite controls certify
implementations at two exact points and two weights. The symbolic proof,
not interpolation between those four rows, establishes the general formula.

## 2. Primary source and registered implementation boundaries

The exact-bound primary text is
`docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`.
The load-bearing spans are:

- Lines888–895, Eq3.7: the horizontal dual bundle and its fiber-point metric.
- Lines899–917, Eq3.8–3.9: vertical symmetric tensors, double contraction,
  trace/traceless splitting, and trace-sign freedom.
- Line932, Eq3.10, and lines954–958: the chimeric direct sum and orthogonality,
  with choices of signs for its summands.
- Lines1012–1018, Eq3.15: the downstairs Levi-Civita horizontal identification.
- Lines1022–1049, section3.4 and Eq3.16–3.17: a downstairs metric induces the
  splitting, upstairs metric, and upstairs Levi-Civita connection.
- Lines1060–1065 explicitly exclude most arbitrary upstairs metrics.
- Line2117, Eq9.1: the metric variable is MET(X1,3), not arbitrary MET(Y).
- Lines2196–2198 and2210–2211: the reference spin curvature is induced by
  Levi-Civita geometry and B is its gauge-rotated connection.
- Lines2218–2233, Eq9.5–9.7: the redundant connection/epsilon equations and
  displayed connection variation do not supply a separately calculated
  downstairs metric Euler equation.
- Lines2267–2275, Eq9.11–9.12: a squared norm and transposed derivative are
  written, but no norm or field equation from them is selected here.

Five exact-bound core files preserve the code-side distinction:

- `src/Gu.Phase4.Dirac/CpuSpinConnectionBuilder.cs`, lines14–15,59–62,84:
  P4-IA-003 is an explicit flat Levi-Civita assumption, with zero LC entries
  and `FlatLeviCivitaAssumption` reporting. This is not a constructed induced
  metric-fiber connection.
- `src/Gu.Geometry/ToyGeometryFactory.cs`, lines463–533: the toy and structured
  fiber factories use finite fiber points and an embedding dimension;
  simplicial dimension remains4. These facts do not instantiate the metric
  fiber's Levi-Civita curvature.
- `src/Gu.Geometry/FiberBundleMesh.cs`, line113 and its projection/section
  metadata: ambient embedding dimension is not a curvature certificate.
- `src/Gu.Phase5.Reporting/GeometryEvidenceClassifier.cs`, around line54:
  its draft-aligned dimensional category does not prove realization of the
  induced Frobenius/Zorro geometry. No historical category is changed here.
- `src/Gu.ReferenceCpu/BiConnectionBuilder.cs`, around line73: `WithFlatA0`
  explicitly supplies a toy flat reference rather than recovering that
  geometry.

These source and implementation statements are inspection boundaries, not
claims that every possible future implementation is excluded. All five
files and the entire live726-file core manifest are bound; no core is edited.

## 3. General metric, signature and nondegeneracy

Let y be any real invertible symmetric4x4 matrix in the Lorentz component;
p=y^-1, and A,B are constant symmetric coordinate tangent matrices. Declare

    g_y(A,B) = alpha Tr(p A p B) + beta Tr(p A) Tr(p B).

The two weights are real and satisfy alpha!=0, alpha+4beta!=0. This
two-parameter family keeps both overall sign/scale and trace weight visible;
it does not attribute two freely selected physical parameters to the author.
Double contraction fixes the traceless form up to that stated overall
convention, and trace reversal is represented by beta=-alpha/2.

Put U=pA and V=pB. The trace direction is A=y and has norm
4(alpha+4beta); it is orthogonal to all trace-free U. Thus the conditions
above are precisely nondegeneracy. In the fixed ordered coordinate basis

    E00,E11,E22,E33,E01+E10,E02+E20,E03+E30,
    E12+E21,E13+E31,E23+E32,

the Gram determinant is

    det g_y = 64 alpha^9(alpha+4beta)/(det y)^5.

For diagonal y, six off-diagonal basis norms are
2alpha/(y_ii y_jj). The diagonal block is
diag(1/y_ii) (alpha I+beta 11^T) diag(1/y_ii), proving the determinant.
Congruence y=L eta L^T extends the formula, since the determinant of the
Sym4 representation of L is (det L)^5. This is a congruence argument,
not a change to the fixed coordinate basis inside the finite tests.

At eta=diag(-1,1,1,1), alpha1, beta0, inertia is7 positive and3 negative.
At beta=-1/2, it is6 positive and4 negative. The trace direction changes
sign while the other nine do not. Our sign labels are mathematical positive/
negative counts; they do not override the source's temporal-sign convention.
The two auxiliary ambient blocks in section7 are not asserted to have total
signature(7,7). Overall summand signs can be chosen separately as in the
source; the vertical connection and total-geodesy proof do not change.

The singular controls alpha0,beta1 have rank1/inertia(1,0,9), while
alpha1,beta=-1/4 has inertia(6,3,1). No inverse is attempted for either.

## 4. Levi-Civita connection from the metric, not analogy

Inverse differentiation gives

    D_X p = -p X p,
    D_Z D_X p = p X p Z p + p Z p X p.

In particular noncommuting X,Z cannot be collapsed to twice one order.
Differentiate both alpha and beta terms of the metric. For constant fields,
the Koszul identity is

    2 g(Gamma(A,B),C) = D_A g(B,C)+D_B g(A,C)-D_C g(A,B).

Substitution and cyclicity of trace give, with BOTH trace-weight terms kept,

    Gamma_y(A,B) = -(A p B+B p A)/2.

One direct compatibility verification is

    g(Gamma(X,A),B)+g(A,Gamma(X,B)) = D_X g(A,B).

For the beta part, Tr(p Gamma(X,A))=-Tr(p X p A); the two resulting
products are exactly the two beta derivatives. For the alpha part, the
four half-weighted traces pair by cyclicity into its two derivatives.
Gamma is symmetric in A,B, so it is torsion free. Nondegeneracy and Koszul
uniqueness prove the formula for every admitted alpha,beta and y.

The executable independent route forms the entire Gram, its first jets,
and its second jets from inverse differentiation. `MetricJet` caches traces
of products U_a=p E_a, through length4. All cached first/second jets are
also compared with uncached matrix product-rule routines `DMetric` and
`DDMetric`. No finite difference or fitted coefficient occurs.

For each pair(a,b), the ten Koszul covector entries are multiplied by the
exact inverse Gram to obtain all ten coefficients of Gamma. The derivative
is independently solved from differentiated Koszul:

    G (D_x Gamma_ab) = D_x Koszul_ab - (D_x G) Gamma_ab.

The second term is retained. All recovered matrices are compared with

    D_X Gamma(A,B) = (A p X p B+B p X p A)/2.

This supplies independent full connection derivatives rather than merely
testing the final curvature formula against itself.

## 5. Curvature and the nonzero witness

For the convention R(A,B)C=nabla_A nabla_B C-nabla_B nabla_A C,

    R(A,B)C = D_A Gamma(B,C)-D_B Gamma(A,C)
              +Gamma(A,Gamma(B,C))-Gamma(B,Gamma(A,C))
            = -y [[p A,p B],p C]/4.

The executable first route uses only the Koszul-recovered Gamma coefficients
and differentiated-Koszul derivatives in the first line, including composition
of recovered coefficients. The independently assembled commutator is the
second route. They are compared for all10^3 ordered inputs, in all four
contexts, as full4x4 symmetric matrices without projection/truncation.

The complete first-pair skew symmetry and cyclic Bianchi identity are checked
as matrices. Lowering with the independently assembled Gram gives all10^4
entries, checked for last-pair skew symmetry and pair interchange. Every
coordinate and every lowered entry participates, including all zeros.

At eta, take A=diag(0,1,-1,0), B=E12+E21. On the positive spatial2-plane,
[A,B]=2(E12-E21) and [[A,B],B]=4A. Therefore

    R(A,B)B=-A,
    g(A,A)=g(B,B)=2alpha, g(A,B)=0,
    g(R(A,B)B,A)=-2alpha,
    sectional(A,B)=-1/(2alpha).

The trace weight drops out because all fields in this witness are trace free.
For the finite scaled point, the program transports the fields:
A'=diag(0,4,-9,0), B'=6(E12+E21). It does not reuse the untransported
coordinate matrices and assume the same answer.

The commuting diagonal control and central trace direction y have zero R.
They establish that the nonzero witness is not an always-nonzero flag.
Two planted errors have independent nonzero defects:

- Flat Gamma=0 violates compatibility: for D=diag(0,1,1,-2),
  D_D g(D,D)=-2alpha Tr(D^3)=12alpha, with no beta term. Transport D too.
- Omitting the derivative-of-Gamma terms gives +A instead of -A.
  Here Gamma(A,B)=0 at eta despite nonzero curvature; pointwise zeros of
  some connection coefficients do not justify a flat-curvature inference.

## 6. Complete finite menu and hand-counted support

The menu has y=eta and diag(-1,4,9,16), with L=diag(1,2,3,4), alpha1,
and beta0,-1/2. All basis inputs are ordered; none are sampled or removed
using a symmetry before comparison. There are four contexts.

The exact Gram determinants are -64,+64 at eta and
-1/990677827584,+1/990677827584 at the scaled point. Since det(y)=-576,
576^5/64=990677827584. Inertia is computed by exact symmetric congruence
elimination, using a1x1 pivot when available or an off-diagonal2x2 block
with one positive and one negative direction. Separate2x2/singular known
answers exercise both pivot cases, independently of the Gram oracle.

Connection support per context:4 same-diagonal inputs,24 incident diagonal/
off-diagonal inputs,6 equal off-diagonal inputs,24 distinct off-diagonal
inputs sharing a vertex. This gives58 nonzero ordered pair outputs,
112 matrix entries, and64 symmetric-coordinate coefficients. Disjoint
supports give zero; none of the listed entries cancel at the declared points.

Curvature support per context:48 ordered(A,B) pairs have nonzero commutator,
24 diagonal/off-diagonal incidences and24 shared-edge pairs. Each is one
plane generator. Its action on Sym4 is nonzero on7 C inputs: the2 endpoint
diagonals, its own edge, and4 incident other edges. Thus336 outputs have
672 nonzero matrix entries. Its own-edge C produces2 diagonal coordinates;
the other6 each produce1 coordinate, giving48*8=384 coordinate entries.
The lowered tensor likewise has384 entries. Its output has Tr(p R)=0,
so the beta part contributes nothing, and each diagonal difference pairs
nontrivially with exactly its two endpoint diagonals.

Across four contexts the frozen totals are232/448/256 for the connection
and1344/2688/1536 for curvature outputs/matrix entries/coordinates, with
1536 lowered entries. The program checks these separately in each context
and in total, and reports every nonzero connection/curvature coordinate.

Main finite counts:400 Gamma outputs,4000 Gamma derivatives,4000 curvature
outputs,4000 first jets,40000 second jets,40000 second-jet symmetry tests,
4000 Koszul pairings,4000 compatibility tests,400 torsion controls,4000
first-pair and4000 Bianchi controls,40000 lowered entries and40000 each
last-skew/pair-interchange controls. Congruence checks include200 Gram,
200 connection and2000 curvature transports. Four witness rows,8 flat
controls,4 flat-connection and4 omitted-derivative decoys are frozen.
The complete exact menu and every smaller count are in `FixtureJson` and
must equal the full contract object, not merely its hash.

## 7. Ambient vertical curvature: what is and is not proved

Take a flat downstairs metric in affine coordinates x. Its LC horizontal
split is the coordinate split on the local metric bundle with fiber y.
The declared orthogonal induced metric has no horizontal/vertical cross
terms, and gVV(y) has no x dependence. The horizontal block H(y) may
depend on y. For vertical constant A,B and horizontal constant X, Koszul
gives

    2 G(nabla_A B,X) = A G(B,X)+B G(A,X)-X G(A,B)=0.

All coordinate brackets vanish. Since the horizontal block is nondegenerate,
the horizontal part of nabla_A B is zero. The fibers are totally geodesic;
restricting the ambient connection/curvature to three vertical arguments
gives exactly the intrinsic vertical connection/curvature already audited.
This proof does NOT require derivatives of H in vertical directions to be
zero, and does NOT say the entire metric is a Riemannian product.

As auxiliary executable controls, H=y and H=y^-1 are separately assembled
into full14x14 block metrics. All first derivative slots are assembled;
the horizontal vertical derivatives are explicitly nonzero. A complete
14-component Koszul solve for every ordered vertical pair produces its
previously recovered vertical Gamma and four zero horizontal components:
800 full VV outputs and8 nonproduct controls across the menu. Neither H
choice selects the source's C-to-TY identification or total signature.
Multiplying either summand by a constant nonzero sign leaves the argument
unchanged. The symbolic proof, not a guessed H identification, is the bridge.

For a source-compatible nondegenerate total metric with its chosen signature,
this nonzero ambient curvature is a nonzero orthogonal Lie-algebra
endomorphism. Its Dirac spin lift is injective at Lie-algebra level:
independent bivector Clifford blades represent independent generators.
Consequently the induced reference spin curvature is nonzero. An invertible
gauge conjugation cannot send a nonzero curvature to zero. This is not a
claim of injectivity of Shiab, nor an evaluation of any action. Pulling back
to a four-dimensional section may discard vertical two-form legs; that is
not an identification of the full14D reference connection with a flat one.

## 8. Freeze, resources, evidence and review

The sixteen unique file bindings are own Program/project/proof/helper;
immutable600 exact arithmetic, passed summary, contract and Program;
the primary text; five core assumption files; the726-file core manifest;
and Directory.Build.props. The only externally compiled code is the600
arithmetic file. Its Clifford/complex utilities are not used by this audit.
No upstream scientific file is copied and silently modified. The manifest
is itself exact-bound, and every live src .cs/.csproj path/hash excluding
bin/obj is compared with its sorted entries and combined tree hash.

`FixtureJson`, terminal precedence, all16 IDs/paths/hashes, all14 false
firewall keys, external-review-pending and physical-claim count0 are checked
before arithmetic. The passed600 evidence and its contract hash are also
checked. Full and summary artifacts are byte-identical deterministic JSON.

All arithmetic is BigInteger rational with exact tolerance0. There is no
floating-point derivative, random input, parameter fitting, or observational
calibration. Largest matrix dimension14 and largest jet array10000 are
structural bounds fixed by the loops. Four contexts, finite10^4 jet/symmetry
loops, and sparse diagonal/basis products give an estimated20 CPU seconds
with a conservative120-second planning bound; expected peak128MiB with
512MiB planning allowance. These wall/memory figures are estimates, not
measurements or time-based scientific gates.

The enforceable `maximumTrackedMatrixProducts` ceiling is100000000.
`Matrix.Products` counts calls through `Matrix.Mul` in instrumented matrix
arithmetic only; direct Rational products in scalar metric formulas are not
included. It is expressly not a global operation census. All remaining
scalar work is bounded by the explicit fixed loops above; no data-dependent
search can enlarge the mathematical menu. Gaussian elimination terminates
in at most14 pivots. Output contains four complete finite coefficient rows
and four independent witness rows, not huge traces of every zero check.

Terminal precedence is invalid/drifted input, known-answer failure,
metric/connection failure, curvature failure, ambient failure, nonzero/decoy
or census/resource failure, then the success terminal. No failed artifact
may be deleted, reclassified as a success, or silently repaired. A scientific
repair requires a new version and new review before another run. The
implementation note is unbound and may receive factual approved results;
the frozen proof/code/helper/project/contract must remain untouched.

All fourteen authority flags stay false. O4 and external review remain
pending, Phase561 is closed, and physical mass claims remain0. This is a
source-geometry assumption audit, not a source choice or physical prediction.
