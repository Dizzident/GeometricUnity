# Phase 622: full homogeneous kinetic carrier audit

## 1. Status, scope and freeze

This is a prospective, exact, zero-sampling audit allocated by A64.
A Release build is permitted; no scientific execution, coefficient pilot,
output or adaptive menu tuning is permitted before the complete pack has
independent and MAIN approval. A failed first execution is retained.
The declared terminal is
full-homogeneous-kinetic-carrier-controls-pass-conditional-linear-branch.

The scientific object is the literal canonical untied CAA operator of the
declared diagnostic first action, not an author-selected interpretation:
Phi1=Gamma1, Phi2=Gamma2, first-C, outer-A, inner-A. Source anchors are
3.7,3.8,3.10,3.15,3.17,3.27,3.34,9.1,9.4,12.26,12.27.
The full complex Cl14 Dirac module has dimension128, with two64-dimensional
chiral halves. The Hermitian signature is(64,64), so u(64,64) acts on the
full128-dimensional module; the normalized trace is Tr/128. These dimensions
follow the declared Clifford representation, not a chosen particle mass.
All16384 real u(64,64) blades, both Clifford parities and central iI remain
available. Grade checks are performed only after full products.

The passed600 arithmetic/trace conventions,607/608 coordinate geometry,
610 rational orthonormal frames,611 literal CAA, and618 complete homogeneous
connection are immutable inputs. There is no623 or other unexecuted input.
The coordinate connection is not replaced by a curvature or Einstein oracle.
No physical vacuum, coupling, field interpretation, units or pole is selected.
All14 authority flags remain false, external review pending, claims0.

## 2. Geometry and independently constructed fields

Write W=H+T+Rt, where H is the four-dimensional horizontal representation,
T is the nine-dimensional traceless self-adjoint representation, and t=-I/2.
Their metrics are gH=-y, tr(AB), and g(t,t)=-1. In the passed oriented
orthonormal frame H axes are0,7,8,9; T axes1,2,3,4,5,6,11,12,13; t axis10.
The signature is seven positive followed by seven negative entries.

For horizontal u, Lambda_u=U_u+R_u, while all vertical Lambda_a=0:
U_u v=-(u tensor vflat+v tensor uflat)/4+gH(u,v)I/8,
U_u A=Au/2, U_u t=0,
R_u v=-gH(u,v)t/4, R_u t=-u/4, R_u A=0.
These are statements in the moving homogeneous frame, not a globally
constant coordinate frame. Coordinate connection plus frame-motion matrices
are read separately and added; the derivative index is transformed by E
and both endomorphism indices by E^-1 and E. All entries are checked against
the passed618 full frame matrices, and metric skewness is checked separately.

Fields are constructed from matrix projectors and the actual Lambda:
J_a=-4 Spin(PH Lambda_a PT+PT Lambda_a PH);
B_a=-4 Spin(PH Lambda_a Ptr+Ptr Lambda_a PH);
C_A=gamma_A wedgeCl gamma_t, C_u=C_t=0.
Thus B_u=-(1/2)gamma_u wedgeCl gamma_t, and L=-B+C.
The other three fields are PHGamma, PTGamma, PtrGamma.
PT and the full36-term J are independently compared with passed618 input.
No forecast response matrix is used to compute any field derivative.

The full tensor derivative is spin commutator minus action on every
covector slot. KineticCarrier implements ordered-slot replacements directly;
immutable618 Homogeneous.Action is a separate exterior-slot oracle.
Spin lift signs are checked on all14 gamma generators in every connection
direction and at both points. An additional91-plane by16-mask menu checks
the full action against the independent oracle, including central phases.

## 3. Literal operator and kinetic variation

D_B S=sum_a theta^a wedge nabla_a S.
For a two-form Y, D_B^dag Y=-sum_a sigma_a i_a nabla_a Y.
The Clifford connection acts as [Spin(Lambda_a),Y], while the same Lambda
acts with a minus sign on every covector slot. No pointwise transpose of
a finite response matrix defines this codifferential.

K uses the full eight-stage chain
F, star F, C(Gamma1,star F), A(Gamma2,star F),
star A(Gamma2,star F), A(Gamma1,star A(Gamma2,star F)),
first-minus-half-star-outer, star(last).
The optimized full Clifford product and the independent word product
compute separate complete chains. Kdag is assembled by literal reversal
of every Hodge/bracket step, plus an independently simplified reversal.
Actual derivatives of Kdag S are compared with Kdag(nabla_a S), rather
than assuming parallelism in place of the derivative.

The original local kinetic first variation is
[Pair(V,K D_B S)+Pair(S,K D_B V)]/2.
Its full Euler expression is Pair(V,H S), where
H=(K D_B+D_B^dag Kdag)/2.
These are not generally equal pointwise for invariant fields.

## 4. Prospective kinetic coefficients and their derivation

For J, traceless completeness gives
sum_A sigma_A A^2=9I/4,
sum_u sigma_u U_u u=0,
sum_u sigma_u U_u(v)u=-9v/8.
Define rho(X) by [X,gamma(v)]=gamma(rho(X)v) for a Clifford bivector X.
Full exterior differentiation yields the vector-action endomorphisms
rho((DJ)_uv)=-8[U_u,U_v], rho((DJ)_uA)=2U_(Au), rho((DJ)_ut)=-U_u.
The first CAA contraction has weights(9/4,-1,0), trace0.
The outer scalar and grade-five terms vanish. Independently,
Kdag J has only vector two-form entries Y_uA=-2Au; its full
codifferential has the same three weights.

For B replace U by R in those vector-action exterior expressions. The first contraction
has weights(7/4,0,1), total trace8, and the scalar CAA term adds-4Gamma.
Kdag B has only Y_ut=u. Full covector and spin differentiation gives the
reverse weights(3/4,0,1); the last covector slot must not be dropped.

For C its coefficient endomorphism W_A obeys W_A A'=-2tr(AA')t,
W_A t=-2A, W_A u=0. Only rho((DC)_uA)=[Lambda_u,W_A] remains.
The first contraction is(-9/2,-2,0), trace-36; the scalar term adds18Gamma.
Kdag C has Y_At=-2A, whose full codifferential is(0,-2,0).

The complete prospective table is:

| Input | K D_B | D_B^dag Kdag | H |
| --- | --- | --- | --- |
| PHGamma | -J-B | -J+3B-2C | -J+B-C |
| PTGamma | J | J | J |
| PtrGamma | B | -3B+2C | -B+C |
| J | (9/4,-1,0) | (9/4,-1,0) | (9/4,-1,0) |
| B | (-9/4,-4,-3) | (3/4,0,1) | (-3/4,-2,-1) |
| C | (27/2,16,18) | (0,-2,0) | (27/4,7,9) |
| L | (63/4,20,21) | (-3/4,-2,-1) | (15/2,9,10) |

Triples use(PHGamma,PTGamma,PtrGamma). The PT calculation follows its
self-adjoint projector derivative. Ptr has div Ptr=t, giving the additional
reverse divergence term; PH follows from the separately checked nabla Gamma=0.
The two legs are deliberately unequal on B,C,L.

The cyclic map is the full metric-contracted exterior/Clifford wedge:
sum sigma_form theta-index gamma wedgeCl coefficient. It vanishes on all
seven inputs and their exterior derivatives. For bivector inputs,
OuterAdjoint=2i times the cyclic three-form, hence zero. Differentiating
this parallel cyclic map proves that the CAA four-form contraction of DB
vanishes; no projected grade-five response is fed into the calculation.

## 5. Completeness and pairings

The invariant vector one-forms have three independent projector weights.
The invariant bivector one-forms have exactly the three carriers J,B,C.
Here completeness is only for Clifford grades1 and2, not the entire
Clifford-valued invariant space or the nonlinear solution.

A Lorentz-invariant tritensor on H+Sym0(H)+R with its last two slots
antisymmetric is generated by metric contractions and at most one epsilon.
Odd numbers of H vector indices cannot be contracted. The HHT contraction
is unique and gives J; HHt gives B; TTt gives C. A TTT metric contraction
reduces to tr(ABC), which is symmetric for self-adjoint A,B,C by transpose
and cyclicity, so its required antisymmetrization is zero. An epsilon
among three symmetric matrices necessarily contracts two indices of one
matrix and vanishes. Input T with two horizontal antisymmetric output
slots is also zero by symmetry. There is no invariant two-form with trace
input. Repeated trace slots or all-trace terms vanish on antisymmetrization.
The same contractions show there is no invariant three-form. This is the
ordinary orthogonal contraction argument explained in the existing
stationary-background reference, not a new GU source equation.
Both infinitesimal Lorentz invariance and disconnected horizontal inversion
are checked on every constructed field at both points.

The signed normalized Pair Gram diagonal on the six independent fields is
(-4,-9,-1,9,-1,-9), with all off-diagonal entries zero. L=-B+C gives
Pair(B,L)=1, Pair(C,L)=-9, Pair(L,L)=-10. All49 ordered pairings per point
are independently computed. The full algebraic adjoint pairing is checked
on the same49 ordered pairs, before any integration-by-parts claim.

## 6. Green current and nonzero negative controls

For S,V define j^a=sigma_a Pair(i_a Kdag S,V).
Product differentiation and the metric-compatible connection give
Pair(Kdag S,D_B V)=Pair(D_B^dag Kdag S,V)+div j.
In a homogeneous frame with constant invariant components,
div j=sum_ab Lambda_a[a,b]j^b. The current is built from the literal
adjoint and pairings; it is never defined from a residual or H mismatch.

For S=B,V=PHGamma the raised trace contraction gives j=-4t:
i^t Kdag B=PHGamma and Pair(PHGamma,PHGamma)=-4.
Since sum_a Lambda_a[a,10]=-1, div j=4.
The original variation is5, Pair(PHGamma,HB)=3, hence5=3+4/2.
Separately Pair(PHGamma,HB)-Pair(HPHGamma,B)=4.
This is evidence against pointwise symmetry of the restricted H matrix,
not against the formal integrated adjoint on admissible compact variations.
All49 ordered currents and divergences are retained at both points.

For nabla_0 Gamma, the spin-only coefficient at(form1,blade2) is-1/4,
the covector-only coefficient is+1/4, and the full derivative is zero.
Both omitted-slot calculations are retained nonzero decoys.
The six nonzero-kappa solutions below also retain the nonzero residual
obtained by omitting the actual kinetic term. Central generators are not
used as vacuous decoys.

## 7. Exact gamma=0 diagnostic linear problem

A=-(21/4)PHGamma-(15/4)PTGamma-(21/4)PtrGamma and HA=3J/2.
The full second response restricted to(J,L) has columns
(-13/4,-9/4) and(3/2,5/2). For kappa in{-3,-2,-1,1,2,3},
put D=4kappa^4+3kappa^2-19,
j=(6kappa^2-15)/D, l=-27/(2D),
vH=(21/4-9j/4-15l/2)/kappa,
vT=(15/4+j-9l)/kappa, vt=(21/4-10l)/kappa.
Then S=vH PHGamma+vT PTGamma+vt PtrGamma+jJ+lL.
Eliminating the vector block gives determinant D/4 for
kappa^2 I-H^2 on(J,L). The three D values at absolute kappa1,2,3 are
-12,57,332, all nonzero. Every S is independently passed through the full
operator and checked against A+HS+kappa S=0. The omitted-H residual
A+kappa S=-HS is required to be nonzero.
The formula's kappa0 exclusion is checked before any division.
This homogeneous denominator is not momentum, a mass pole or an
invertibility statement on the full229376-dimensional one-form domain.

Two additional full-carrier controls avoid overreading that exclusion.
Z=(11J-27B+8C)/99 has HZ=Gamma and HGamma=0; direct substitution of the
three bivector columns gives numerator99 in each vector component.
The bivector-to-vector determinant is-99/4, while the vector-to-bivector
block has rank2 and kernel Gamma. Thus on this six-dimensional carrier
H has rank5, kernel span Gamma and a nontrivial zero Jordan chain.
No physical zero mode or full-domain kernel classification follows.

At gamma=kappa=0, S*=(7/12)J-(9/44)B+(37/66)C satisfies HS*=-A.
The complete six-carrier solutions are S*+aGamma because that carrier's
kernel is Gamma. Its B+C coefficient sum47/132 is nonzero, so S* lies
outside the source-generated five-carrier. This is not the kappa-to-zero
continuation of the preceding rational family, not a selected coupling,
and not classification of all invariant Clifford fields.
Both Z and S* receive full independent computations at both points.

## 8. Exact finite menus and counters

Two passed points are used, with no interpolation or fitting. Each point
has seven named fields, Gamma, A, two second-response inputs, Z, S* and
six rational-branch inputs:19 complete computations per point,38 total.
Every computation retains all14 input derivatives, independent derivative
oracles,14 actual adjoint derivatives,14 parallel-adjoint controls,
all eight stages in both forward chains, both full adjoint constructions,
both codifferentials and the full H result. Nothing is replaced by weights.
There are30 transported input/response comparisons at the second point.
The exact independent loop census is:

| Counter | Expected |
| --- | --- |
| arithmeticControls | 4 |
| wordCases | 507904 |
| hodgeCases | 16384 |
| fullRealDomainMasks | 16384 |
| centralDomainControls | 2 |
| planeRepresentationControls | 1456 |
| contexts | 2 |
| coordinateConnectionEntries | 5488 |
| frameConnectionEntries | 5488 |
| metricSkewEntries | 5488 |
| spinLiftControls | 392 |
| fieldContexts | 14 |
| fieldInputControls | 14 |
| fieldCyclicControls | 28 |
| bivectorOuterAdjointControls | 8 |
| gradientForecastRows | 42 |
| gradientGradeRows | 42 |
| isotropyControls | 84 |
| disconnectedControls | 14 |
| gramRows | 98 |
| algebraicAdjointPairRows | 98 |
| originalKineticGreenRows | 98 |
| greenAnchorRows | 2 |
| pointwiseAsymmetryRows | 2 |
| unequalKineticLegRows | 6 |
| omittedConnectionRows | 4 |
| parallelGammaRows | 2 |
| sourceResponseRows | 2 |
| hSquaredRows | 4 |
| linearRows | 12 |
| linearResidualRows | 12 |
| omittedKineticDecoys | 12 |
| excludedZeroKappaRows | 2 |
| transportRows | 30 |
| validatedResults | 38 |
| derivativeComparisons | 532 |
| parallelAdjointComparisons | 532 |
| chainComparisons | 304 |
| chainTypeChecks | 304 |
| chainRealityChecks | 304 |
| adjointComparisons | 38 |
| exteriorComparisons | 38 |
| codifferentialComparisons | 38 |
| resultDomainChecks | 38 |
| jordanRows | 2 |
| zeroCouplingRows | 2 |

The helper census is38 Compute calls,532 derivative slots,2604 connection
actions:38*28+91*16+2*7*6. Matrix entry checks are2*14^3=5488 each.
Word checks are16384*(2*15+1)=507904. All tolerance is exactly zero.

## 9. Resources and failure handling

Planning estimates are60 CPU seconds and512MiB peak; conservative planning
allowances are900 seconds and8GiB. These are estimates, not a wall-clock
watchdog or allocator limit. Acceptance ceilings are2 billion tracked
coefficient products,100 million tracked matrix scalar products,
32768 intermediate tensor entries,128 characters per retained tensor
coefficient rational and
64MiB per file, aggregate128MiB for the two identical compact UTF8/LF
output files. These are commit/push-oriented hard rejection limits.
All actual counters and the maximum retained Result tensor-coefficient
rational length are reported. Coordinate-matrix and Pair/current scalar
rational lengths are bounded analytically below, not separately monitored.

A finite sparsity envelope can be derived without a coefficient pilot.
Only four frame directions have nonzero Lambda. Each has at most10 spin
planes and four nonzero entries per matrix row/column. A declared input
has at most63 terms:14 vector projector entries,36 J entries,13 B/C
entries. Its literal adjoint has at most189 terms:91 vector-input
bivector entries plus two contraction entries for each of49 bivector
input entries. The cyclic outer-adjoint part vanishes analytically.
One input derivative has at most(10+4)*63=882 terms; one adjoint
derivative at most(10+8)*189=3402 terms, including both covector slots.
The exterior derivative therefore has at most3528 terms.
Parallel adjoint derivatives satisfy the independently proved equivariance
identity and have the same bound, which is checked rather than substituted.

The first-C output has at most14*(14+91)=1470 entries.
Before cancellation the inner top contraction allows grades0,3,4, at most
1+364+1001=1366 entries; completed cyclic cancellation leaves only scalar.
Its outer step then has at most14 entries. Even without that cancellation,
the outer grades1,2,5 have at most14*(14+91+2002)=29498 entries,
below32768. Literal reverse intermediate first legs have at most9555
entries (91 form pairs times105 Clifford vectors/bivectors); the outer
three-form accumulation has at most365 including scalar. The final reverse
is built without grade projection. Success-case cyclic cancellations,
independently justified above, keep every intermediate under32768.
The word/control batteries have smaller supports. An unexpected algebraic
failure is not repaired by projection or by increasing this ceiling.

A complete retained Result uses fewer72000 term records: two input-derivative
families at most7056, two adjoint-derivative families27216, two exterior
derivatives7056, two chains22964, and fewer6700 other input/adjoint/response
entries (63 input, four adjoint tensors at most189 each, four responses
at most1470 each give6699). The sum70991 is below72000. Thus38 Results plus plane,
connection, Gram/current and decoy evidence remain below3 million term
records. Each Result is serialized once. All finite-menu tensors have
pure-real or pure-imaginary coefficients, so a record with the rational
length cap fits256 UTF8 bytes. This deliberately dense envelope is below768 million bytes per copy
plus metadata; it does NOT certify success under the much tighter64MiB
file cap. The actual sparse footprint is expected to be substantially
smaller: only four derivative directions are active, each field starts
with4-63 terms, and almost all of the dense possible grade slots are zero.
No coefficient pilot has estimated actual bytes. The64MiB per-file guard
is authoritative and an oversized first result must remain a failure,
not trigger serializer tuning. Identical copies imply aggregate128MiB.
The complete compact string is checked before either file is written.

For arithmetic growth, the menu coefficients have magnitude and total
coefficient norm below2^14; the finite frame denominators involve only2,3,
while linear/Jordan/zero-control denominators add11,19,83.
A common generous input denominator is2^16*3^8*11*19*83.
At most two connection actions and the finite linear CAA/adjoint operations
occur in a retained tensor; multiplying by another2^32 bounds their
denominators below2^80. The L1 operator bounds8 for a nonzero connection
direction and2576 for each K/Kdag give retained norms below2^40.
A signed pairing doubles denominator exponents but the actual field/response
pairing uses one undeformed input and one at most twice-differentiated
response, hence denominator below2^128 and numerator below2^218.
Their reduced printed rational needs at most107 characters, including
minus sign and slash. The128 cap is a deliberately looser acceptance check.
No floating point arithmetic or numerator truncation is used.

Per Compute, the largest raw forward word-product menu is
(14+91)*3528 candidate pairs, plus14 outer pairs; two chains give under
750000 candidate pairs. Spin products, exterior products, both reversed
adjoints and14 parallel reversals add less250000 under the preceding
support bounds. Therefore38 million candidate pairs cover all38 Computes;
the finite plane, field construction, spin-lift, isotropy and other controls
fit well within an additional10 million. Tracked optimized coefficient
products are a subset of these loops; naive and literal-transpose work is
not mislabeled as tracked. The2 billion cap is ample without treating the
tracked counter as a universal count of every scalar multiplication.
The matrix work comprises fewer2000 dense14x14 multiplication-equivalents,
plus scaling/entries and two small inverses, below10 million scalar products;
the100 million tracked matrix ceiling is conservative.

Counter, support and rational checks are final acceptance checks, not
interrupts inside immutable helpers. Full evidence serialization allocates
before its byte check, so8GiB is a planning allowance, not a guarantee.
If the output cap is exceeded, a bounded failure record preserves every
observed scalar/boolean diagnostic, full counts and original terminal,
sets controls/resources false and fullTensorEvidenceAvailable=false.
No oversized evidence is written. Earlier diagnosed scientific terminals
take precedence over resource failure. Unhandled process/resource exceptions
are not success and their first process failure must be preserved by MAIN.

Failure order is invalid-or-drifted-input, known-answer-control-failed,
homogeneous-connection-control-failed, full-kinetic-response-control-failed,
invariant-carrier-control-failed, original-kinetic-green-control-failed,
exact-linear-branch-control-failed, resource-census-control-failed, success.
There is no retry, alternate menu, silent pilot or post-run bound-input repair.

## 10. Provenance and handoff

The contract binds45 unique paths/IDs, including every directly compiled
helper: two owned source files plus eight immutable linked helpers.
SDK default source discovery is recursive and excludes only bin/obj path
segments; unexpected additional compiled sources fail closure.
All six upstream complete provenance groups are bound, not just their
summaries. Passed summary terminals, core/control flags, contract hashes
and all14 firewalls are rechecked. The primary text, build properties and
live726 core path/hash/tree manifest are bound and checked.
The complete FixtureJson equals the contract fixture structurally.

Full and summary outputs are identical deterministic compact JSON with one
final LF; every sparse tensor coefficient is retained in numeric tuple order.
No output exists before FIRST execution approval. The implementation note
is unbound handoff status, never a scientific premise. Shared101/202 and
claim-integrity integration belongs to MAIN after first outputs.
