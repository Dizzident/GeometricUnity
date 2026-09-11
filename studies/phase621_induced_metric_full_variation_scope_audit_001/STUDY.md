# Phase621: full fixed-Y induced-metric variation and its scope

Prospective A63 audit. Contract
`phase621-a63-induced-metric-full-variation-scope-v1`; success terminal
`induced-metric-full-variation-controls-pass-fixed-domain-only`.
No scientific execution has occurred when this pack is frozen. A successful
Release build is not a scientific run. Independent full-pack review and
explicit MAIN approval must precede FIRST science. No unexecuted620 artifact
is an input. First failures, including resource failures, must be preserved.

## 1. Precise question and primary-source boundary

This audit continues615's DECLARED fixed-coordinate reconstruction, not a
uniquely selected interpretation of the source. The bound primary draft
places the tautological horizontal metric at the fibre point y in3.7
(lines888-895), its vertical contraction and trace freedom in3.8-3.9
(lines899-917), and the orthogonal chimeric sum in3.10 (932,954-958).
The chosen downstairs metric h enters the chimeric-to-TY identification
through LC(h),3.15 (1012-1018) and3.17 (1044-1053);1060-1065 restrict the
upstairs metrics to this induced family. Equation9.1 (2117) names MET(X).
The displayed first action9.4 integrates over Y (2181-2193), with spin
reference curvature defined2196-2198 and gauge-rotated LC reference2210-2211.
The Hodge/volume definitions in12.24 (3431-3448) are upstairs definitions.

Full u(64,64), including central iI and both Clifford parities, is retained:
3.27 (1158),3.34 (1219);12.27 (3462) supplies the i-anticommutator convention.
The canonical untied CAA operator means first C, outer A, inner A,
Phi1=Gamma1, Phi2=Gamma2. It is a declared source-compatible diagnostic
choice, not source operator selection. No SU projection, positive trace
norm, observed particle carrier or fitted coupling is inserted.

The words “metrics pulled up from the base” at1082-1086 leave an
identification ambiguity; the connection-only reading is not authorial
adjudication. Observation is separately discussed in3.1 (785-809),
3.37-3.42 (1237-1294),3.17 (1046-1053), and section11 (2513-2521).
A fixed-Y statement cannot be promoted to an observer-pullback equation.

## 2. Every metric occurrence in the selected first action

Let X have coordinates x0,...,x3 and Y have additionally the ten symmetric
covariant-metric coordinates y. At a fixed fibre point set p=y^-1 and

    G0=diag(-y,Vy),
    Vy(A,B)=Tr(pApB)-(1/2)Tr(pA)Tr(pB).

Thus alpha1,beta-1/2,sigma-1, with the same(7,7) signature and oriented
rational frames as608/610. For the downstairs LC matrices C_i,

    L_i=C_i^T y+y C_i, N_VH=L, N^2=0,
    P=I+N, G_C=P^-T G0 P^-1, G_C^-1=P G0^-1 P^T.

Both y and the coordinate fibre domain are fixed under h variation.
The chimeric metric, Clifford algebra and its chosen local spin labels
are independent of h at fixed y. They become tensors on TY through P.
Use epsilon=I and hold the NATIVE coordinate one-form varpi fixed in
these chimeric spin labels. Then T=varpi-epsilon^-1 D0 epsilon=varpi for
every h, because D0 I=0; consequently delta T_coordinate=0 and
d(delta T_coordinate)=0. This is not “hold the physical connection
A=B0+varpi fixed”: B0 genuinely varies with h.

The original scalar action density, divided by the baseline upstairs
volume density, has four independent pieces

    <T,K(F_B)> + (1/2)<T,K(D_B T)>
       + (gamma/3)<T,K(T wedge T)> + (kappa/2)<T,T>.

The audit retains the variation of G/G^-1, volume, every Hodge map, BOTH
Phi1 occurrences, Phi2, spin B, reference F_B, D_B T and the potential.
The final pairing is implemented as the original top-degree wedge/Hodge
trace. No printed9.6/9.7 force, Einstein substitution, projected scalar
gradient, formal self-adjoint pointwise inference or bulk field Euler
boundary shortcut replaces this first variation. Gamma and kappa are
formal independent weights; no physical values are selected.

## 3. Complete real metric jets and noncoincident fibre control

The two fibre points are y0=diag(-1,1,1,1) and
y1=diag(-1,4,9,16). Importantly h0=diag(-1,1,1,1) is the SAME constant
downstairs metric at BOTH points. It is not varied together with y.
Choosing h0=y at both points would make the highest-order lift response
longitudinal at those points and conceal the decisive third-jet control.

Use all ten symmetric coordinate basis matrices in615's order, and all
35 multiindices I in N^4 with |I|<=3. For each, take the local real jet

    delta h=M x^I/I! at x=0.

A compactly supported bump equal to1 near0 realizes exactly these jets.
There are1,4,10,20 multiindices of orders0,1,2,3. Symmetry of ordinary
coordinate derivatives means these are the COMPLETE linear jet slots:
10+40+100+200=350 at each point, hence700 contexts. This is not sampling
a Fourier direction, interpolation, fitting or complexification. The
normalization I! makes the matching derivative exactly M, including
repeated and mixed indices. Ordering is degree followed by ascending
first three exponents, with the fourth fixed by the degree.

At a constant h0 the exact dual Koszul formula gives

    delta C^k_ij=(h0)^kl
       (partial_i delta h_jl+partial_j delta h_il-partial_l delta h_ij)/2.

The implementation differentiates h^-1 too: its zero-jet variation is
nonzero, but multiplies the ZERO baseline first derivatives of h.
The zero-order absence is not inserted as a branch. The resulting
connection variation is independently compared to615's Koszul helper.

Let n=delta N. Its horizontal derivatives replace delta C by its base
derivatives; each vertical derivative replaces y by a symmetric coordinate
basis matrix. Second vertical derivatives of n vanish since L is linear
in y. Product differentiation gives

    k=delta G=-n^T G0-G0 n,
    delta G^-1=-G0^-1 k G0^-1=n G0^-1+G0^-1 n^T.

Differentiate this product in ALL14 coordinate directions once and twice.
A separate mixed-block route gives k_HH=k_VV=0 and k_HV=-n^T V and all
its product-rule derivatives. The highest base order is1 in k,2 in Dk,
3 in DDk. Only the already-bound first and second vertical metric jets
are needed: no invented third vertical metric jet is used.

Both inverse identities and Tr(G^-1 k)=0 are checked, but zero volume
is only one control, never the asserted metric-stationarity proof.

## 4. Full LC, curvature and moving spin frame

The first geometry route uses genuine rational dual numbers in the
inverse-matrix derivative, Koszul connection, its complete coordinate
derivative, and

    R_ab=partial_a Gamma_b-partial_b Gamma_a+[Gamma_a,Gamma_b].

The second route explicitly expands the linearized inverse/Koszul
products using independent dense-matrix operations, then uses the
Palatini identity

    delta R_ab=partial_a delta Gamma_b-partial_b delta Gamma_a
               +[Gamma_a,delta Gamma_b]-[Gamma_b,delta Gamma_a].

The lower form-slot connection terms cancel here because baseline torsion
is zero. ALL14^3 connection and14^4 derivative/curvature coefficients are
compared, including zeros. Baseline full curvature is compared to passed608,
not replaced by a restricted block.

Use E_t=(I+t n)E0(y), an actual orthonormal chimeric identification.
Choose E0's first vertical frame jet to be
rho(-y^-1 A/2)E0, the already-validated618 reductive lift; its horizontal
coordinate derivative is0. This is a legitimate local frame extension.
At baseline the vertical connection in that frame cancels and the
horizontal connection is618's Nomizu map. Do not freeze E0 before
differentiating it.

In fact, all unspecified baseline frame derivatives cancel from the
FIRST spin-connection variation:

    omega_t=E_t^-1 Gamma_t E_t+E_t^-1 dE_t,
    delta omega=E0^-1(delta Gamma+dn+[Gamma,n])E0.

This exact product expansion is the reason no higher frame jet is needed.
Its full eta-skew identity is checked before lifting to spin matrices.
For curvature, vary the conjugation AND both external frame arguments.
Equivalently transform the complete effective tensor

    delta R+[R,n]+R(n -, -)+R(-,n -).

Independently lower curvature first: delta(R_lower)=G0 delta R+k R,
then vary ALL four covariant frame slots. All coefficients of both routes
are compared and fully lifted into the spin two-form. No result is
obtained by dropping higher Clifford grades or inserting a Ricci formula.

## 5. Two full action-variation routes

Put a=E0^-1 n E0 and let M_a act on every covector slot of a form by
replacing theta_old with sum_b a[old,b] theta_b; Clifford slots are fixed.
A fixed coordinate tensor has adapted-frame variation M_a tensor.
The coordinate solder tensors therefore have fixed-frame variations

    delta Phi1=-M_a Gamma1, delta Phi2=-M_a Gamma2,

whereas both are constant only in the MOVING adapted frame. Likewise

    delta star(t)=star(M_a t)-M_a(star t).

The fixed-frame route applies dual-number product rules to the original
CAA chain with these solder variations and this moving star at EVERY
occurrence, followed by the original top-form pairing. Its primitive
spin variation is delta B above and its fixed-frame curvature variation
is delta F_adapted-M_a F. It holds native T fixed.

The independent adapted-frame route uses the separate WordSign-based
NaiveProduct kernel and the ordinary constant canonical star/operator,
but retains ALL input-frame variations:

    tdot=M_a T, bdot=delta B_fixed+M_a B,
    delta(D_B T)=bdot wedge T+T wedge bdot
                   +B wedge tdot+tdot wedge B,
    delta Q=tdot wedge T+T wedge tdot.

All eight stages, their values and their first variations, agree after
the appropriate M_a transformation. Both outer-field and inner-input
terms of the kinetic action are retained. This does not use integration
by parts or assert a pointwise adjoint equality on a noncompact field.

## 6. Five fields and independently derived exact anchors

At each point, specify five locally COORDINATE-CONSTANT native fields by
their indicated point values. They are x-independent and have dT=0 for
these fixtures. A field equal to Gamma1 at the point is NOT asserted
parallel in a neighbourhood. Let U=theta0 Gamma01.

| Field | Source | Kinetic | Cubic divided by gamma | Mass divided by kappa |
| --- | ---: | ---: | ---: | ---: |
| Gamma1 |60|0|-1456|-7|
| Gamma1+U |60|-13/4|-4372/3|-13/2|
| Gamma1+U+i theta1 I |60|-13/4|-4372/3|-6|
| (dx0+dy00)gamma0 |21/4|0|0|-3/4|
| dy01 gamma0, point0 |0|0|0|1/4|
| dy01 gamma0, point1 |0|0|0|1|

These are manual forecasts, independently checked before execution.
For Gamma1, <Gamma1,Gamma1>=-14, KQ=312Gamma1 and source pairing60.
The kinetic pairing vanishes by vector/bivector trace parity, not by
assuming D_B Gamma1=0 for the coordinate-constant field.

Kdag Gamma1=-24Gamma2 and Kdag U=2theta01 gamma0.
D_B Gamma1 has theta01 gamma0 coefficient1/4.
D_B U has theta07 Gamma07,theta08 Gamma08,theta09 Gamma09 coefficients
-1/4,+1/4,+1/4. Thus the two mixed kinetic pairings are-1/2 and-6,
giving-13/4 after the factor1/2. The cross Q is2theta01 gamma0 and its
K image is-4U, giving cubic-4372/3. Central iI commutes, so changes
only the signed mass pairing by+1/2. No central generator is deleted.

For T=(dx0+dy00)gamma0 and delta h00=f(x0),
delta C^0_00=-f'/2, delta L0,00=f', delta G^{0,00}=f'.
Therefore delta<T,T>=-2f' and the MASS PIECE response is-kappa f'.
The frozen-Hodge control is0 and is explicitly different from-1 at the
unit first jet. The exact periodic scalar identity f=sin x,w=cos x gives
unweighted mean0 and weighted mean-kappa/2. This is an integration
counteridentity, not a physical compactification or a replacement of the
compact-support theorem below.

For the decisive highest-order control at y1 with h0=eta,

    delta h00=x1^3/6,
    partial1^2 delta L0,01=-3/2,
    V01,01=-1/2,
    partial1^2 k0,01=-3/4,
    delta R[1,0,1,01]=+3/4.

Index01 is coordinate fibre index8; its symmetric matrix has BOTH off-diagonal
entries1. Lowering the last slot gives-3/8, hence
delta Ricci0,01=-3/32 and delta scalar=0. The literal canonical CAA
curvature identity forecasts a gamma0 coefficient3/32 on dy01, so
pairing with dy01 gamma0, whose squared signed pairing is+2, forecasts
the ORIGINAL SOURCE ACTION derivative3/16. The executable still computes
FULL R, FULL spin lift and ALL CAA stages; it never substitutes Einstein
for those computations. Three independent analytical derivations agreed.

## 7. Universal fixed-domain cancellation and exactly what it excludes

The finite fields above have dT_coordinate=0. A general invariant field
from618 usually has nonzero y-derivatives, and is NOT one of those finite
constant-coordinate fixtures. The extension to it is structural, not a
claim that3500 finite rows span every native field or its derivatives.

For any smooth x-translation-independent native T(y), dT(y) is also
x-independent and its metric variation is0 in the declared identification.
Every other action occurrence enumerated in section2 depends on h only
through C(h), its derivatives and the induced frame. At constant h0 its
first variation has positive BASE derivative order, at most3. Thus the
FULL first variation, including all kinetic and curvature contributions,
has the form

    delta L(x,y)=sum_(1<=|I|<=3) A_I(y) partial_x^I delta h(x).

No positivity or full connection stationarity is required for this
statement. No field-variation boundary term has been discarded:
delta native T is EXACTLY0. One may integrate each positive base
derivative over x first; for compact-supported delta h the integral is0
for each fixed y. On a fixed compact fibre region D, inside a regular
coordinate/spin chart and independent of x,h, all coefficients are smooth
and bounded. For sufficiently small parameter t the metrics remain
nondegenerate uniformly. The RELATIVE action

    integral_(X times D) [L(h0+t delta h,T)-L(h0,T)]

has compact base support and finite first variation, even if the separate
unsubtracted translation-invariant base action is infinite. Ordinary
compact-domain differentiation and Fubini then justify zero first
variation. There is NO integration by parts in y.

Base-compact variations do not vanish on the fibre cutoff. This proof
does not use bulk g=0 to suppress fibre-boundary terms: it avoids the
field-identification variation altogether. An alternative identification
would require its entire variation and boundary contribution. An x/h
dependent fibre domain, extra weight, nonlocal identification, observer
pullback, divergent full-fibre integral or added direct h dependence lies
outside the conclusion. This is conditional stationarity of the selected
induced fixed-domain metric variation, not unrestricted MET(Y) stationarity,
global vacuum existence, source intent, observed dynamics or a spectrum.

The moving-section control is decisive. Metric compatibility gives

    ds_h(u)=(u,partial_u h)=(u,L_u(C(h),h)),
    s_h^* G_h=sigma h, delta(s_h^*G_h)=sigma delta h.

These observer controls use separate coincident baselines h_section=y at
both fibre points, explicitly distinguished from the audit's h0=eta.
Their zero-jet sigma M is absent from the fixed-y variation. Here sigma=-1;
no different source sign convention is silently identified.

A separately labelled, NONSELECTED alternative musical identification
H_alt=sigma h_alt y^-1 h_alt has derivative2sigma M at h_alt=y.
Its exact symmetric difference is checked; it is an assumption control,
not a licensed replacement for the connection-only construction.
Finally, multiplying the action by an EXTRA downstairs sqrt|det h|
would add120phi for delta h=phi h0 on the Gamma source density60,
or-30phi for delta h00=phi. This term is absent from the declared fixed-Y
action. These controls demonstrate the need for the stated assumptions.

## 8. Complete finite census

The frozen43 count fields are exact loop counts, not observed support
sizes or success tallies:

```json
{
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "fullRealDomainMasks": 16384,
  "points": 2,
  "frameControls": 2,
  "baselineConnectionEntries": 5488,
  "baselineCurvatureEntries": 76832,
  "jetContexts": 700,
  "zeroJetContexts": 20,
  "firstJetContexts": 80,
  "secondJetContexts": 200,
  "thirdJetContexts": 400,
  "metricEntries": 137200,
  "metricFirstJetEntries": 1920800,
  "metricSecondJetEntries": 26891200,
  "inverseVariationEntries": 137200,
  "volumeRows": 700,
  "connectionVariationEntries": 1920800,
  "connectionDerivativeEntries": 26891200,
  "curvatureVariationEntries": 26891200,
  "frameCurvatureEntries": 26891200,
  "frameConnectionSkewEntries": 1920800,
  "spinCurvatureRows": 700,
  "solderMotionRows": 1400,
  "fieldRows": 3500,
  "fieldDomainRows": 3500,
  "actionValueComparisons": 14000,
  "actionDeltaComparisons": 14000,
  "chainStageComparisons": 84000,
  "zeroJetGeometryRows": 20,
  "zeroJetActionEntries": 400,
  "massDerivativeAnchors": 2,
  "frozenHodgeDecoys": 2,
  "weightedIntegrationControls": 4,
  "movingSectionEntries": 320,
  "alternativeIdentificationEntries": 320,
  "addedDensityControls": 4,
  "downstairsConnectionEntries": 44800,
  "thirdJetCurvatureAnchors": 1,
  "thirdJetSourceActionAnchors": 1,
  "shardRows": 700,
  "stageTensorFingerprints": 336000
}
```

Two points times350 jets gives700. Per context,14^2=196,14^3=2744 and
14^4=38416 explain137200,1920800 and26891200 entries respectively.
There are20/80/200/400 jets by derivative order, and64 downstairs
connection entries per jet gives44800. Five fields per jet gives3500;
four scalar pieces gives14000 comparisons. Three inputs times eight
stages gives84000 fixed/adapted stage pairs, with FOUR expanded tensor
fingerprints per pair,336000 total. Zero jets give20*5*4=400 scalar zeros.
Two points times ten M times16 section/alternative entries gives320 each.
The two mass controls, four weighted/unweighted means, four added-density
controls and single third-jet curvature/source anchors are fixed before
execution. The known-answer Clifford census uses16384*(2*15+1)=507904
word products,16384 Hodge-square and16384 full-real-domain checks.

## 9. Complete, bounded, replayable evidence

All primitive geometry variations are retained as full sparse tensors
with zero coefficients implicit, never a selected carrier. Baselines
(including ACTUAL h0, distinct fibreMetricY, frame, B, F and all five
fields) are stored once per point. Each of the700 uniquely named shards
retains n,k,delta G^-1, frame motion, Dk,DDk,delta Gamma,Ddelta Gamma,
delta R,delta B,delta F and each field's complete kinetic/cubic input
variation and four action coefficients.

The eight CAA stages are retained in a LOSSLESS DAG representation,
not claimed to be expanded arrays. The exact graph, input recipes,
product/dual/moving-star semantics, degrees and normalization are frozen
in the contract, Program fixture and output summary. The graph has nine
topologically ordered operations: star, C product, A product, star,
A product, star, scale-1/2, add, star, with explicit IDs and edges.
Primitive leaves and frame motion determine every coefficient uniquely.
Every fixed/adapted stage value AND variation has a canonical SHA256.

Canonical tensors use numeric tuple ordering(form,blade,k0,k1), reduced
rational strings and the exact Fourier.Terms property order; their digest
is SHA256 of compact UTF8 JSON WITHOUT a newline. Complete JSON documents
use compact UTF8 and exactly ONE LF. Each manifest records shard identity,
path, byte count and SHA256. Full and summary documents are identical
small manifests, not duplicated expanded tensors.

The independently implemented --verify-evidence mode reads the full
input provenance and ALL700 shards, then reconstructs EVERY stage using
the separate NaiveProduct/WordSign kernel and an independently written
ordered-wedge slot-replacement motion. It checks336000 tensor fingerprints,
84000 frame-stage identities and14000 original action scalar coefficients.
It also checks canonical read format, complete sparse geometry shapes,
all baseline/input IDs, ACTUAL fixed h0 versus fibre y, and exact graph/
semantic parity with the frozen contract and summary. A missing, extra,
duplicate, renamed, reordered/noncanonical or hash/byte-mismatched shard
fails closed. The complete actual shard pathset must equal the700-member
menu. The mode is strictly read-only, INCLUDING preflight failure:
Emit is guarded before any file creation, so no failure can overwrite
scientific outputs. FIRST science does not silently count as this
independent postflight replay; MAIN must request the latter separately.

## 10. Prospective resources and failure policy

All coefficients are exact arbitrary-precision rationals; tolerance0.
There is no floating-point derivative, random draw, fit or eigentolerance.
All scientific local tensors have frequency0; only the explicitly separate
sin/cos counteridentity uses frequency1 inputs and frequency2 products.

The full CAA input Clifford grades are1/2. Stage supports are bounded by
91*(14+91)=9555 for two/twelve-forms;6566 for the first C leg;
364+1+1001=1366 for the top-form inner A leg; and
14*(91+14+2002)=29498 for the outer/combined one/thirteen-form legs.
C/A kernels combine AB plus/minus BA before insertion, so forbidden
grades are not transiently materialized. Ordinary W temporaries are also
covered: T has<=16 terms, delta T<=44, baseline B<=40
(four horizontal axes, ten mixed planes each), delta B<=14*91=1274.
Each delta B*T order has<=20384 terms; its reversed order has the SAME
support, not double. B*delta T has<=1760; Q and its variations have
<=256 and1408 before collection. Their commutator sums have final
grades1/2. Top-form products have at most16384 Clifford masks.
Thus32768 covers every inserted tensor, including partial accumulations.

Tracked matrix products have a conservative dense allocation below
10 billion: per context metric-jet products need1626 matrix
multiplications; independent connection/Palatini construction1710;
frame/conjugation and auxiliary work fewer than900 more. At14^3 per
dense multiplication and700 contexts this is under8.2 billion, with
downstairs4-dimensional elimination/construction and baseline work
comfortably inside the remaining margin. The dual scalar products are
bounded by the14^5 inverse-derivative/Koszul loops and curvature sums;
the frozen4-billion ceiling exceeds their complete finite allocation.

For coefficient products, each fixed-chain dual first-C, inner-A and
outer-A leg is bounded by306000,889000,306000 products, including
varied solder terms and form-overlap restrictions. Three inputs and3500
fields give less than15.8 billion; original top pairings and primitive
W products add less than2.2 billion. The20-billion ceiling is prospective.
The NaiveProduct and direct Rational operations are NOT counted by these
particular counters; the complete fixed finite loop census bounds them.
Planning estimates are900 CPU seconds/1GiB peak, with conservative
2400 seconds/8GiB estimates; these are not measured outcomes or tuning
targets. Arrays have at most14^4 entries and one context is serialized
at a time; expanded stage arrays are not accumulated across contexts.

A conservative coefficient-size certificate suffices for storage.
At these integer diagonal y points, inverse metric denominators divide
2^4*3^2, metric first/second jets divide2^13*3^6 and2^17*3^8;
the shear jets add at most one factor2. Following the explicit product
formulas bounds delta Gamma denominators by2^32*3^14, delta R by
2^50*3^22, the lowered four-frame spin variation by2^68*3^30,
and the final chain/pairing denominators by less than2^128. Each of the
four E factors can contribute a denominator dividing2^2*3, including
the horizontal1/3 entry; these factors must not be dropped. Including
the operator half, field coefficients and action weights gives the loose
uniform final bound2^72*3^31<2^122<2^128.
Using |G|<=16, |DG|<=32, |DDG|<=192, |G^-1|<=512,
|n|,|Dn|,|DDn|<=64 and |E|,|E^-1|<=16 in those same finite
sums, then the operator support/majorants above, gives the deliberately
loose absolute coefficient bound2^160. Hence numerator<2^288,
denominator<2^128 and at most1+87+1+39=128 printed characters,
including a possible numerator minus sign and the fraction slash.
The enforced ceiling is128 characters for every retained rational and
every expanded-stage tensor fingerprint input.

A shard has at most120736 sparse geometry records
(2*2744+3*38416), four dense14^2 matrices,1274 spin-connection and8281
spin-curvature terms, and ten field-input arrays of at most9555 terms.
Their retained coefficients are REAL: central iI drops out of the
commutators. Allowing200 UTF8 bytes per geometry record and210 per
tensor record at the128-character coefficient ceiling, plus all480
stage fingerprints and metadata, gives less than48MB. The64MiB
per-shard cap is therefore below ordinary100MiB Git blob limits.
Full/summary baseline+manifest data is much smaller.

A separate2GiB AGGREGATE shard-write ceiling prevents a pessimistic
many-shard scenario from consuming the workspace. This is an enforced
resource guard, not a prediction of observed total size. Before each
write, the next byte count is checked; crossing the cap stops science,
preserves all previously written evidence, and emits an explicit
failure summary with partial manifest and byte totals. Any earlier
already-failed scientific comparison keeps its higher-priority terminal;
the resourceFailure and all observed booleans are retained regardless.
Incomplete loop counts are not interpreted as a separate count failure.
No truncation, deletion, parameter adaptation, compression substitution
or automatic rerun is allowed. Success records actual total/max shard
bytes, and postflight recomputes both exactly.

## 11. Lineage, freeze and claims firewall

Forty-six unique exact bindings include six owned files
(Program/project/proof/three helpers), seven600 bindings, five each for
607/608/610/611/615/618, and primary source/core manifest/build props.
Every compiled immutable helper is directly hash-bound. Full upstream
Program/proofs/contracts and passed summaries are bound; passed terminal,
contract hash, known answers, controls, core closure and all14 authority
flags are checked. The entire fixture object must equal the contract,
not a hand-picked subset. All726 live sorted core paths, each hash and
the sorted path-space-hash-newline tree must match the bound manifest.
No upstream scientific bytes or shared/core files are edited.

Failure precedence: invalid/drifted input; known-answer domain failure;
metric-jet failure; full connection/curvature failure; moving-spin-frame
failure; original-action variation failure; scope-negative-control
failure; resource/census failure; success. All bound scientific files
become immutable at FIRST execution, whether pass or fail. Any repair
requires a new reviewed version; only the unbound IMPLEMENTATION note
may record results.

All14 authority flags remain false; externalReviewPending=true;
Phase561 stays closed; O4 and external review remain pending; WZ15/H14
deficits and promotedPhysicalMassClaimCount=0 remain. No source operator,
norm, physical coupling, global action, observer equation, physical
vacuum, characteristic pole, units or boson mass is selected. This audit
can establish a conditional fixed-domain first-variation theorem and
show exactly why its tempting observer/global extensions do not follow.
