# Phase626 — fixed-cubic full stationary residual certificate

## 1. Prospective scope and authority

This is the A67 prospective pack, frozen before its FIRST scientific execution.
No numerical coefficient pilot, root approximation, sampled operator selection,
or observation-dependent menu is permitted. A Release build is not execution.
The literal canonical untied CAA is the same declared operator as611/618/623:
first C, outer A, inner A, with Phi1=Gamma1 and Phi2=Gamma2. It is a declared
conditional source realization, not a resolution of the primary's missing
preferred Shiab construction. The primary anchors are3.27/3.34 and9.4, with
the induced geometry/provenance chain607/608/610/618 held fixed.

The full complex Clifford algebra has16384 blades on a128-dimensional Dirac
module; each chiral half has dimension64. The real domain is u(64,64), including
central iI, both Clifford parities, and all229376 one-form coordinates. Trace is
normalized by128. There is no imposed six-carrier or low-grade closure.

The displayed first action9.4 fixes the cubic relative coefficient1/3; it does
not name a free gamma. We therefore test conditional gamma=1 in the current
bracket/trace normalization. This is closer to that displayed coefficient than
the diagnostic algebraic gamma of624/625, which is not an input here. Neither
the field/action normalization nor the physical units are selected. In the
compact9.4 form the mass coefficient is kappa1/2; in the expanded integral it
is printed kappa1. Our Euler kappa equals kappa1 in the former convention and
2kappa1 in the latter, subject to a common pairing normalization. The fixed
rational kappa907712 is a target-independent diagnostic value, not a physical
mass scale. Rescaling a field cannot silently preserve source and kinetic
coefficients while changing the cubic coefficient.

All14 authority flags remain false, external review remains pending, and no
physical mass claim is promoted. O4, Phase561, the source-transformation bridge,
WZ15/H14 deficits, global bundles, fibre boundaries, mixed Hessian, propagator
poles and units are not discharged by this audit.

## 2. Inputs and finite menu

There are exactly two passed618 orthonormal frames, indexed0 and1. Their full
14 Nomizu matrices are read, not replaced by anticipated response coefficients.
From each passed623 point shard, read all15 actual coefficient rows
(n=1..5,gammaPower=0..2), sum the three powers at gamma1, and independently
compare the five results with the retained623 gamma1 evaluation rows. Expected
tensor columns in623 are never computation inputs.

Write S_n for these five complete tensors and set

    q=907712, lambda0=1/q, X=P5(lambda0), P5(lambda)=sum(n=1..5)lambda^n S_n.

The14 kinetic fields per point, in this exact order, are

    S1,S2,S3,S4,S5,X,PHGamma,PTGamma,PtrGamma,J,B,C,W5,central.

Here B=−(PHGamma wedgeCl gamma_t)/2, C=PTGamma wedgeCl gamma_t,
t=−I/2 is the distinguished trace direction, W5=theta3 Gamma02347
(form8,blade157), and central=theta0 iI. The eight last fields are the original
action probes. PH axes are0,7,8,9; trace axis10; the other nine are PT.
Their signed norms are −4,−9,−1,9,−1,−9,1,1. W5 is not asserted to be an
isotropy-invariant global field; its local first jet is the declared full
connection action used for the pointwise Green control.

For each point evaluate14 complete kinetics, nine nonlinear polynomial
coefficients at orders2..10 using all25 ordered pairs of S1..S5, one direct
self-feedback of X, eight probe self-feedbacks, eight full X/probe crosses,
and one B/PHGamma off-root cross. There are27 nonlinear rows per point.
Retain all G0..10 and all coefficients0..11 of lambda G(lambda).

The six isotropy generators and the disconnected diagonal transformation from
618 act on S1..S5 and X. Each complete infinitesimal action is compared with
the independent ordered-slot route and checked zero. The disconnected action
is checked directly on all form and Clifford slots. This establishes that the
candidate is in the full invariant domain on which618's existence theorem
applies; it is not a declaration that eight test fields exhaust that domain.

## 3. Literal operators and independent routes

For every kinetic input, compute all14 full derivatives

    nabla_a T=[Spin(Lambda_a),T]+Lambda_a acting on every covector slot.

The primary covector algorithm uses explicit ordered index replacement and
inversion counts. The independent route acts on all exterior Clifford slots
and all exterior form slots. Both are valid at every Clifford grade.
The exterior derivative is sum theta^a wedge nabla_a T. Compute all eight
literal CAA stages by the grouped Clifford kernel and independently by the
word-sign kernel, preserving both complete arrays.

Reverse the actual first-C/outer-A/inner-A chain for Kdag. Independently use
the algebraically simplified reverse chain. Retain the first and second legs
separately, their sum and the simplified sum. For every direction retain
nabla_a(Kdag T), Kdag(nabla_a T), and its simplified reverse evaluation.
The codifferential contracts with −sigma_a; the full response is

    H T = (K D T + Ddag Kdag T)/2.

There is ONE covariant derivative in each leg: H is first differential order,
not a second-order Laplacian. No pointwise self-adjointness on noncompact
homogeneous fields is assumed. Derivative, full adjoint and full reverse grade
checks precede their serialization and do not project any tensor.

For Q(T)=T wedge T, DQ_T[V]=T wedge V+V wedge T. Compute

    N(T)=(K Q(T)+DQ_T^dag Kdag T)/3.

The literal real-bilinear transpose loops every source/output term pair before
testing the exterior subset. Its independent word-sign oracle performs that
entire loop too, with the transposed Clifford word order. There is no complex
conjugation of coefficient scalars in this real trace transpose. Both loops'
raw visit counts are separately instrumented, including rejected pairs.
Likewise the naive product count includes all pre-pruning pairs. The existing
Fourier.CoefficientProducts counter measures only successful grouped products
and cannot substitute for these counts.

All high-grade outputs, including grades9/13, participate in literal/word
transpose equality and actual/naive CAA equality. The eight directional action
probes are additional controls, not a complete dual basis or a justification
for discarding unpaired high-grade coordinates.

## 4. Residual polynomial and direct certificate

For n=2..10 compute, without response forecasts,

    Q_n=sum(i+j=n) S_i wedge S_j,
    E_n=sum(i+j=n) DQ_{S_i}^dag Kdag S_j,
    N_n=(K Q_n+E_n)/3.

The full original Euler residual is

    G(lambda)=A+H P5(lambda)+N(P5(lambda))+P5(lambda)/lambda.
    G0=A+S1; G1=H S1+S2;
    Gn=N_n+[n<=5]H S_n+[n<5]S_(n+1), n=2..10.

Forecast G0..G4=0 follows the passed623 exact recursion. No higher coefficient
is forecast zero. In particular H S5 is newly computed, not omitted. Full
allowed residual grades are, at orders5..10 respectively,

    {2,6},{1,5,9},{2,6},{1,5,9},{2,6},{1,5,9,13}.

These are fail-closed upper envelopes, not declarations that every allowed
grade occurs. All full residuals are assembled before these tests.
The scaled-defect polynomial lambda G has coefficient0=0 and coefficient
n+1=G_n. The polynomial chunk's `scaledDefect` is that coefficient, without
an extra lambda0 factor. By contrast, the direct chunk's `scaledDefect` is
the evaluated tensor lambda0 G(X). Their metadata explicitly distinguishes
these meanings. Every slot0..11 is retained, including zeros.

Compute X directly from the five coefficients. Independently evaluate its
literal H and nonlinear feedback; compare full H, KQ, N and G against the
evaluated coefficient arrays. Compare the directly scaled defect against the
evaluated12-slot defect polynomial. No coefficient norm or stage is obtained
from a signed pairing.

Use618's universal full-domain bounds a=60,h=51520,cN=2576 and R=120lambda0.
For T_lambda(S)=−lambda(A+HS+N(S)), compute exactly

    selfMap=lambda0(a+hR+cN R^2),
    L0=lambda0 h+2lambda0 cN R,
    epsilon_post=lambda0 ||G(X)||_1/(1−L0).

Check ||X||<=R, selfMap<=7R/8, L0<=1/2 and positive1−L0 BEFORE inversion.
Check the exact identity ||lambda0 G||=lambda0||G|| and defect<=R/512.
These establish a posteriori distance<=epsilon_post to the unique fixed
point in this full invariant ball, conditional on618's declared geometry and
operator. They do not give a convergent arbitrary PDE or global bundle result.

The independent Taylor bound is R/768. Holomorphic dependence is a NEW626
deduction from618's universal majorants, not a claim about an executed618
holomorphic calculation. Complexify the finite full invariant space V and use
the same absolute polynomial bounds on the fixed ball of radius120delta,
delta=1/226928, uniformly for |lambda|<=delta. The map is a uniform contraction
there. Its polynomial iterates starting at0 converge uniformly, so their limit
is holomorphic inside the disk. Real uniqueness identifies this limit with the
real fixed point; uniqueness in the larger ball also identifies it with the
smaller radius-R solution at lambda0=delta/4. The coefficient recursion
identifies its first five Taylor coefficients with the actual623 coefficients.
Apply Cauchy on radii delta'<delta and pass to the limit delta' increasing to
delta, using the uniform bound120delta. The order5 remainder is

    120delta (lambda0/delta)^6/(1−lambda0/delta)=R/768.

The branch norm is<=7R/8, so P5 is strictly in the radius-R ball. The fixed-point
defect is then<=(1+1/2)R/768=R/512. This proof precedes measurement. Report
both Taylor and measured a posteriori bounds and their minimum; do not assume
the measured bound is smaller. The bound R/512 and L0<=1/2 alone give only
epsilon_post<=R/256, not R/768.

For complex Cauchy estimates the norm is the genuine complexified l1 norm in
the REAL u(64,64) blade basis. At real lambda each physical coefficient lies
on its blade's fixed real or imaginary axis, so this equals the executable
sum(|Re|+|Im|). The latter expression by itself is not a complex-homogeneous
norm; no unnoticed norm change is used in the holomorphic argument.

## 5. Original-action, Green and negative controls

At fixed X and each of eight probes V, compute all four coefficients of the
ORIGINAL cubic density Pair(X+zV,KQ(X+zV))/3. They are, in ascending z order,

    Pair(X,KQX)/3;
    [Pair(V,KQX)+Pair(X,K(DQ_X V))]/3;
    [Pair(V,K(DQ_X V))+Pair(X,KQV)]/3;
    Pair(V,KQV)/3.

Their three derivative coefficients must equal Pair(V,N(X)),
Pair(V,Ncross(X,V)), Pair(V,N(V)), with factors1,2,3 on the original
coefficients. This differentiates the original action, not a projected action.
The z^3 forecasts are −16,−336,0,0,0,0,0,0. For a diagonal m-axis projector,
the FULL N(V)=2(m−1)(m−2)V+2m(m−1)(Gamma−V). The complementary term is
orthogonal to V; Pair(V,V)=−m, giving the m=4 and9 values after/3. The other
entries vanish by parity or repeated-slot products. The complementary response
must not be discarded merely because this particular pairing cannot see it.

Retain separate source, kinetic, cubic and mass first variations, with
kinetic=[Pair(V,KDX)+Pair(X,KDV)]/2 and mass=q Pair(V,X). Independently form

    j^a=sigma_a Pair(i_a Kdag X,V),
    div j=sum(a,b)(Lambda_a)^a_b j^b.

The pointwise original variation equals Pair(V,G(X))+(div j)/2, not necessarily
Pair(V,G(X)). This is the Green identity with full covector and spin actions.
It is compatible with compact-interior field variations; no boundary condition
or pointwise symmetric H matrix is inferred.

The off-root negative control is B, V=PHGamma, gamma1,kappa0. Its independently
derived source21, kinetic5, cubic−4 give local22, while Euler pairing20 differs
by div j/2=2 (div j=4). All three restricted even-field density pieces are0,
yet the full gradient is nonzero. It rejects scalar-restriction stationarity.
These anchors follow the passed622 complete kinetic/Green columns and623
original cubic formula, not a measured new coefficient.

Only after the COMPLETE G4 is formed, omit the grade5 part of S5 in a separate
decoy. Passed623's600-term grade5 contribution has coefficient−3/4 at
(form8,blade157), so the truncated G4 has600 terms and witness+3/4.
The real residual arrays are never modified by this decoy.

## 6. Complete grades and the cyclic adjoint condition

Input grade sets in field order are

    1;2;1;2;{1,5};{1,2,5};1;1;1;2;2;2;5;0.

Full adjoint, its parallel derivatives and full H have respective upper sets

    2;1;2;1;{2,6};{1,2,6};2;2;2;1;1;1;{2,6};3.

For the listed cyclic bivectors the adjoint grade1 assertion is SPECIAL,
not valid for an arbitrary grade2 field. Define

    W3(T)=sum_a sigma_a gamma_a wedgeCl T_a.

W3(J)=0 by619's full cyclic proof. W3(B)=W3(C)=0 termwise because the form axis
already occurs in the Clifford blade. Passed623 S2/S4 are linear combinations
of these, hence cyclic; X's bivector part is cyclic as well. All six such
bivector parts are checked by an independent exterior-mask construction.
The reverse outer-A term is proportional to W3, so the entire second reverse
leg vanishes on them. The full covariant derivative preserves this identity
because the solder is parallel. Individual unsummed reverse legs are retained;
their support is bounded sparsely, not by pretending arbitrary bivectors lack
grade5. Without cyclicity, adjoint grade5 would invalidate the chosen chunk
envelopes; the code checks every full/simplified/reverse grade before writing.

For an arbitrary grade g, first C yields g−1 when g is even and g+1 when odd;
inner A with Gamma2 yields g±2; outer A with Gamma1 raises an even inner grade
and lowers an odd inner grade. Thus K bands even g to odd{g−1,g+3} and odd g
to even{g+1,g−3}. Those identities check every actual/oracle stage. The central
probe is treated separately: its full adjoint is grade3 and its X-cross
response may have grades3/4/8, never silently omitted.

## 7. Frozen expanded evidence layout

The separately bound `preregistration/chunk_plan_v1.json` lists every path,
collection, exterior degree, form-mask group and tensor identity BEFORE science.
Its3914 entries are independently regenerated by CertificateLayout in preflight
and compared byte-for-byte as compact JSON. No output-dependent splitting,
tensor DAG, adaptive grade omission or post-run serialization repair is allowed.

Per point the exact chunk count is

    14*65 + 10*40 + 17*38 + 1 = 1957.

The14 kinetics have65 chunks each; the nine polynomial rows and direct row
have40 each; eight self/eight cross/one off-root feedbacks have38 each; one
low-residual chunk carries G0/G1 and defect0..2. All empty groups are written.
Two bounded context files and identical full/summary manifests add four files,
so successful output has3918 exact files and no others anywhere under output.

Each chunk is versioned expanded-rational-tensor-chunks-v1. It retains its
collection, degree, exact form group, finite metadata and a `tensors` object
whose keys are the frozen identities. Each tensor is the sorted unique array
of {form,blade,k0,k1,real,imaginary}. Rational strings are reduced, explicit
zero coefficients are forbidden, frequencies are zero and masks span the full
14-dimensional domain. Upstream readers reject duplicate/unsorted keys,
unexpected properties, noncanonical rationals and silently dropped zero rows.
Wire JSON is compact UTF8 with exactly one final LF and explicit relaxed JSON
escaping, matching JSON.stringify for the frozen ASCII metadata and rational
records (in particular literal plus signs are not escaped as HTML characters).
Manifests retain path, SHA256, bytes, record count, collection, degree, form
group and tensor IDs for every chunk. The exact complete pathset and every
hash/byte count are checked after emission. Small contexts retain source,
isotropy/cyclic evidence, scalar certificates, action/Green rows and60 complete
tensor transport fingerprints per point. Their input/H/KQ/N/G tensors are
fully expanded in the chunk collections; fingerprints do not replace them.

## 8. Code-specific support and storage bounds

The conservative input support sum for S1..S5 is740 (the tighter passed sparse
sum is727). X has at most663 terms, the eight probes at most65 combined.
Each nonzero Nomizu generator has10 Clifford spin planes and maximum graph
vertex degree4. A one-form term therefore creates at most10 spin and4 covector
replacement keys per active direction. Exactly four directions0,7,8,9 are
active. These are properties of the full passed matrices, not a flat-reference
approximation.

For one kinetic input with at most663 terms, the input plus both derivative
routes has at most663+8*14*663=74919 records. The exterior pair has at most
2*4*14*663=74256. A single one-form input term creates at most13 first-leg and
91 second-leg adjoint terms: both unsummed legs together are<=104*663=68952.
The full adjoint grade union1/2/6 has3108 blades per exterior plane. Paired
adjoints on16 planes therefore use<=99456 records; three reverse routes on
10 planes use<=93240. The central grade3 case has364 blades and is smaller.
The first CAA kinetic output pair on14 forms is<=87024 records. Other kinetic
chunks use the smaller explicit groups listed in CertificateLayout.

For nonlinear rows Q may have grades1/2/5/6/10:6111 blades per two-form plane,
or556101 full keys. Direct raw W accumulation is also bounded BEFORE forbidden
grades cancel by663^2=439569; final-Q grades are not incorrectly imposed on
those raw products. The full one-form output union1/2/5/6/9/13 has7126 blades
per form, or99764 total keys. The central mixed-adjoint union3/4/8 has4368
blades per form and is smaller. Inner CAA zero-form stages have at most7892
possible blades; raw outer accumulation at most14*7892=110488. Every tracked
single dictionary, including pre-cancellation Put accumulation, is bounded by
the fresh600000 ceiling, not the insufficient65536 ceiling from625.

General paired CAA chunks have at most2*8*6111=97776 records in exterior
degrees2/12 or2*7*7126=99764 records in degrees1/13. Zero/top-form stages are
one fixed group. Each direct/polynomial triple result chunk has four one-form
masks, at most3*4*7126=85512 records. The100000-record hard cap covers all.

A six-property record, including separator, uses at most580 UTF8 bytes when
BOTH rational strings have256 characters;640 is a conservative record budget
independent of H-anti phase cancellation. At most100000 records plus1MiB
metadata and<64KiB fixed wrapper gives<65.2MB, below64MiB=67108864 bytes.
The writer enforces records and rational lengths while streaming and checks
the final byte length BEFORE the disk write. It holds one bounded memory
stream, not all chunks or a monolithic tensor serialization. Empty records and
the finite small header also fit.

Each context is capped at1MiB; each identical full/summary manifest at16MiB.
Each of3914 pins has<2KiB from the actual fixed names, <=91 form masks and
<=30 tensor IDs, hence one pin array is<8MiB. The manifests contain one such
array plus the two<=1MiB contexts and bounded provenance/scalars, below16MiB.
The2GiB aggregate CHUNK cap is a prewrite preservation/failure guard, NOT a
proof that every dense permitted support configuration succeeds. A guard
failure keeps the partial exact manifest and does not authorize rerunning with
a larger layout. Actual sparse evidence is expected to be much smaller than
the sum of all independent dense per-file maxima; no pilot is used to claim
its byte size. The scientific result is allowed to be a resource failure.

## 9. Rational height, operations and memory

No serialization bound assumes the measured candidate is small. From passed623
each coefficient tensor has<2^18 slots and coefficients<10^22<2^74. Summing
three gamma powers gives ||S_n(1)||<2^94. A common denominator is
D=2^30*3^6<2^40. Since2^19<q<2^20,

    ||X||<5*2^94/q<2^78, denominator(X) divides D*q^5.

Literal direct quadratic CAA intermediates have norm<2^172 even before
cancellation. A safe common denominator for direct G is
3*2^6*D^2*q^10<2^288; the defect adds q, giving<2^308. With norm<2^174,
numerators are<2^482. Thus at most146 numerator digits+93 denominator digits
+2 sign/slash=241 characters, strictly below256. Unevaluated polynomial
tensors can have norm<2^208 but denominator<2^88, giving at most119 printed characters;
do not combine that larger norm with the unrelated direct q^11 denominator.
The conservative powers of2 accommodate the one derivative in either H leg;
they do not describe H as a two-derivative operator.

Original cubic/kinetic/Green/mass scalars have norm<2^304 and common denominator
3*2^8*D^3*q^15<2^430. Numerators<2^734 give at most353 printed characters,
below512. For B=q^2−h*q−240*cN>0, epsilon_post=(q/B)||G|| has denominator
<2^328 and numerator<2^482, hence at most247 characters. Absolute l1 summation
preserves these common denominators. All tensor strings are checked during
chunk serialization and all context scalar rationals are recursively scanned.

Per point, raw ordered polynomial Q candidates<=740^2=547600, direct<=439569,
eight X/probe crosses<=86190, eight self probes<=1493 and off-root<=32.
Their total is<1.076 million. A forward CAA uses at most119 visits per Q key
(14 first C,91 inner A,14 outer A); the exterior complement bounds the inner
zero-form support by the input support. The kinetic exterior support sum is
<=56*1468=82208 per point. Both points therefore use<279 million naive product
visits including the raw W inputs, below300 million.

All feedback transpose visits are bounded by104 times the same input-pair
total: <224 million for both points. Literal+simplified kinetic adjoints add
2*238*57*1468<40 million. Thus literal transpose visits<264 million<300 million;
word transpose visits<224 million<250 million. The extra isotropy actions add
<2 million grouped products and<2 million slot replacements even using91
planes per generator.500 million grouped products and200 million recorded slot
replacements are conservative. Unlike row counts, these are ceilings, not
expected measured values. Every raw transpose/naive ceiling is checked BEFORE
its potentially expensive loop. Grouped/slot/support/managed-memory guards
are checked after completed operator stages/rows and cannot interrupt one
already-running primitive call.

The exact wrapper-call census is28 kinetics,392 primary derivative slots,
420 adjoints,164 forwards,2624 literal transposes,104 word transposes and350
naive products. There are82 naive forwards with THREE products each, plus104
raw polynomial/self/cross products:82*3+104=350.

The implementation retains only H/forward/adjoint responses between kinetics,
not all complete kinetic results. A full kinetic record is written, its local
reference nulled and garbage collected before the next field. The largest
simultaneous family is the12 nonzero full/parallel/simplified reverse arrays,
bounded by12*282828<3.4 million records. Its stages, input derivatives,
unsummed adjoint legs, one current primitive's scratch dictionaries and the
retained response cache fit a conservative12-million live-record envelope.
The later polynomial/action pass stores at most nine KQ, nine N and11 G arrays,
plus direct and eight probe responses; it never caches nine complete CAA
stage arrays. Its cached+one-current-row envelope also fits12 million records.

For normalized rational coefficient records a conservative512-byte/record
allowance covers dictionary capacity slack, tuple/Scalar entries and the four
BigInteger backing arrays at the stated heights. Twelve million records are
about6.15GB. The numeric transient proof is separate: Rational '+' forms two
unreduced numerator products and a denominator product before GCD; '*' likewise
forms unreduced products. Allow doubled numerator/denominator bit budgets
(and the two Scalar products before addition), with<4096-bit local arithmetic
temporaries. Those temporaries are per currently evaluated scalar operation,
not additional persistent large arrays per dictionary entry.

Include the passed623 point JSON (about36MiB) and618 JSON (about7.4MiB), their
DOM/UTF8 storage, bounded metadata/plan, and at most768MiB additional buffers:
MemoryStream geometric capacity<128MiB, ToArray<64MiB, current sorted tuple/index
buffers, fingerprint Terms/UTF8 materialization, and one<=64MiB postflight
rehash buffer. This gives a credible below8GiB engineering envelope on the
declared64-bit runtime, not an allocator-independent RSS theorem. The recorded
8GiB GC.GetTotalMemory ceiling is sampled managed memory, NOT a hard OS/RSS
limit or proof that a primitive cannot transiently exceed it. The fixed
operation menu makes CPU finite; rational word-sign loops may take minutes to
hours on this machine. No exact wall-time prediction is asserted without a
pilot, and the parent must monitor the FIRST process while preserving evidence.
The sampled maximum is nondeterministic runtime telemetry and is printed only
to standard error. Pinned scientific JSON retains the deterministic guard-pass
boolean and8GiB ceiling, not the measured byte value. Exact FIRST byte pins and
deterministic scientific serialization therefore remain enforceable on reruns.

## 10. Counts, provenance and failure precedence

The full ordered FixtureJson is part of Program and must exactly match the
contract fixture, including property order for shared verifier parity. The46
expected count fields are prospectively fixed there. Their main derivations:

    2 points; 28 kinetics; 54 nonlinear rows;
    656 paired forward-stage equalities;392 derivative equalities;
    784 parallel-adjoint equalities;50 ordered products and50 transposes;
    18 self products/transposes;36 cross products/transposes;
    64 cubic coefficients;48 derivative identities;16 original Green rows;
    510 signed pairings;3528 Green connection slots;
    120 transport fingerprints and60 across-frame comparisons;
    2452 typed tensor checks;3572 grade-envelope checks;
    72 isotropy and12 disconnected checks;12 cyclic checks;
    22 full residual coefficients;10 old zero coefficients;
    22 defect shifts;8 direct equalities;2 direct defect equalities;
    2 certificates,2 truncation decoys,2 off-root decoys;
    2 contexts and3914 exact chunks.

Known-answer controls cover four rational/complex identities,507904 full-mask
Clifford word comparisons,16384 Hodge signs,16384 real-domain masks and two
central controls. Reflection matrix entries are392; connection entries5488;
source lineage checks2; input coefficient rows30; gamma1 reconstructions10;
probe norms16 and cubic z^3 forecasts16. These enumerate all46 fixture counts.

There are67 unique exact bindings, including all nine actual compiled files:
the three owned C# files plus immutable600 arithmetic/Fourier/trace-adjoint,
611 CAA,619 feedback reader and623 homogeneous derivative helpers. Linked
provenance includes passed600/607/608/610/611/618/619/622/623 Program/project/
proof/contracts/summaries, the two actual623 point shards, primary text,
Directory.Build.props and the726-file live core manifest. There is no624/625
scientific input and no unexecuted sibling dependency. Recursive own .cs
discovery excludes bin/obj and combines with explicit Compile Includes; each
actual compiled path must have a live matching binding. The complete sorted
live core file set, every individual hash and the tree hash are checked.

Failure precedence is invalid input, known-answer, full-domain, geometry,
operator, polynomial, original-action, contraction certificate, resource/census,
then success. An already observed earlier scientific failure survives a later
resource exception. Checks inspect full computed grades before subsequent
resource rejection; no missing future control is invented when an operator's
own pre-loop guard stops execution. Partial counts, flags, emitted chunk pins,
context pins and error identity are retained. A capped manifest may omit its
duplicated point summaries while keeping those summaries in the already
pinned context files; it must not substitute knownAnswerPassed=true.
The emitted terminal determines the process exit status even if a manifest cap
downgrades an otherwise successful result. OOM, OS/process termination and an
unwritable filesystem cannot be promised a complete failure artifact.

Only a complete success with exact counts/call census, full bounds, exact
3918-file pathset, all hashes and all original controls authorizes the stated
CONDITIONAL local invariant stationary-branch certificate. Independent and
MAIN full-pack approval are required before the FIRST run. No mass prediction,
full Hessian, unrestricted continuation or source-intent ruling follows.
