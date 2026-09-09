# Phase610: induced spin curvature and literal source contraction

Prospective exact A58 extension. All scientific files, expected coefficients,
menus, counts, tolerances, resources and lineage freeze before first science.
Build-only checks are permitted; complete independent/MAIN review and explicit
MAIN execution approval are mandatory. Success terminal is
`induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source`.

## Scope and immutable curvature input

Use ONLY the two alpha1,beta-1/2,sigma-1 rows of passed608, at
y=eta=diag(-1,1,1,1) and L eta L^T, L=diag(1,2,3,4). Their signature
is mathematically(7positive,7negative). This convention is not uniquely
selected by the source.608's beta0 control is not inserted into a(7,7)
Clifford representation. Source3.7-3.17 supplies the conditional geometry
and musical/lift interpretation;8.1 and9.3 supply the declared bracket/Hodge
chain. Source normalization, final operator choice and9.11 norm remain open.

The actual input is the full recovered coordinate curvature in608's output,
including mixed horizontal/vertical components and every implicit zero.
This audit does not feed an Einstein tensor or typed curvature oracle into
the literal side. It reconstructs all14^4 coordinate entries from the bound
sparse output, rejecting duplicate/out-of-range/zero records. Gram, Ricci
and Einstein arrays are also read with exact dimensions. Upstream pass,
contract hash and full immutable code/proof lineage are verified first.

## Rational frame, inverse and orientation

Order coordinate vectors H0,H1,H2,H3 followed by vertical basis
E00,E11,E22,E33,N1,N2,N3,P1,P2,P3, with N on01,02,03 and P on12,13,23.
Put A_k=eta U_k with diagonal U rows

    U1=(1,1,-1,-1)/2,
    U2=(1,-1,1,-1)/2,
    U3=(1,-1,-1,1)/2,
    T=-eta/2.

The A_k are trace-free and have vertical norms+1; T has norm-1 and is
orthogonal to them. Each P has norm+2, each paired N norm-2. Thus
p_i=(3P_i+N_i)/4 and n_i=(P_i+3N_i)/4 have norms+1,-1 and cross0.
The positively oriented frame order is

    H0,A1,A2,A3,p1,p2,p3; H1,H2,H3,T,n1,n2,n3.

For the diagonal vertical block(A1,A2,A3,T), the determinant is+1.
The off-diagonal block in raw order(N,P) and column order(p,n) is
[[I,3I],[3I,I]]/4, determinant-1/8. Reordering the full frame above costs
21 inversions, hence its determinant is+1/8. The alternative trace column
+eta/2 leaves the metric diagonal but reverses orientation; it is a planted
orientation error, not permission to switch chirality silently.

The hand inverse is independent of Gaussian elimination. For diagonal
A=(a0,a1,a2,a3), its A1,A2,A3 coordinates are
(-a0+a1-a2-a3,-a0-a1+a2-a3,-a0-a1-a2+a3)/2 and its T coordinate is
(a0-a1-a2-a3)/2. For raw off-diagonal(P,N), the p and n coordinates are
(3P-N)/2 and(-P+3N)/2. Horizontals are unchanged and reordered.

At the second point transport horizontal vectors by L^-T and vertical
matrices by L A L^T. The total map Q has determinant(det L)^4=24^4,
so the frame determinant is41472. Its inverse is E_eta inverse Q inverse.
Both frames satisfy E^T G E=diag(+7,-7); check both inverse products,
hand inverse and Gaussian inverse. This is a POINTWISE curvature-frame
transformation, not a connection transformation omitting dE.

## Two complete frame transforms and spin typing

Let coordinate R(a,b)c have output components R^d_abc. One route assembles
each endomorphism from all recovered entries, changes its two input slots
and conjugates the matrix by E inverse and E. Independently lower the
coordinate output with G to L_abcd=G(R(a,b)c,d), then transform all four
covariant slots by E. The results obey L_frame_abcd=sigma_d R_frame^d_abc.
All14^4 entries at both points participate. Complete curvature symmetries,
Bianchi and Ricci contractions are checked again in the oriented frame.
Transported points must yield identical framed curvature and Ricci.

The bound591/592 Riemann convention is R592_abcd=-L_frame_abcd. With
gamma_a gamma_b+gamma_b gamma_a=2sigma_a delta_ab, the spin lift is

    F_ab=(1/2) sum_(c<d) R592_abcd sigma_c sigma_d gamma_c gamma_d,
    F=sum_(a<b) theta^ab F_ab.

Indeed [gamma_c gamma_d,gamma_e]=2(sigma_d delta_de gamma_c
-sigma_c delta_ce gamma_d), hence

    [F_ab,gamma_e]=sum_d sigma_d L_frame_abed gamma_d
                 =gamma(R_frame(a,b)e).

Both internal metric signs and the ordered-pair half factor are necessary.
All91 external pairs and14 gamma directions are checked at BOTH points,
as full Clifford outputs, not just selected nonzero coefficients.
F is a real H-antiHermitian Clifford-bivector-valued two-form.

Independent unit-plane known answers cover all91 internal pairs: assign
R_cdcd=sigma_c sigma_d, giving F_cd=gamma_c gamma_d/2. All14 vector
commutators are tested. Omitting the metric factors rejects49 mixed-sign
planes and leaves42 same-sign controls unchanged; dropping the half factor
and reversing the curvature sign each reject all91 nonzero planes. These
counts concern this precise planted law, not every possible wrong convention.

An actual induced mixed-sign anchor uses framed H0 and H1, indices0,7.
Its commutator with gamma7 is-gamma0/4. Omitting metric factors or reversing
the last-index convention gives+gamma0/4; dropping the half gives-gamma0/2.
The two point rows each execute these three independent nonzero decoys.

## Literal CCA chain and independent Einstein oracle

Use the immutable600 exact Clifford/form kernel, both h signs and formal
real c, Phi1=P Gamma1, P=1+hOmega, Phi2=(c-i hOmega)Gamma2. Gamma1
and Gamma2 use lower-index gamma matrices and dual coframes, with no extra
metric signs inside those invariant tensors. Positive volume follows E.
The literal chain is

    sf=star(F), one=[Phi1,sf]_C,
    inner=[Phi2,sf]_A, zero=star(inner),
    outer=[Phi1,zero]_C,
    upper=one-star(outer)/2, K(F)=star(upper),

where A(X,Y)=i(XY+YX). The c slot disables the c-independent one term;
no c-dependent first term is accidentally duplicated. Both Phi1 occurrences
are tied as in the declared592 matching family. Every product is also
computed with the independent600 naive word-reduction kernel and every
stage compared. All stages, including zero stages, retain exact form typing
and H-antiHermitian membership. Nonzero inner legs are recorded before
their subsequent cancellation.

Bound592 proves on ALL algebraic Riemann tensors, for a1,bh,d-h and
arbitrary c, that K(F)=-P EinsteinGamma. The framed608 Ricci contraction
must agree independently with its transported Ricci and with the expected
eigenvalues-5/4 on9 traceless vertical directions and+1/4 on5 others.
Its scalar is-10. Therefore Einstein eigenvalues are15/4 on the9 and21/4
on horizontal4 plus trace1. The full literal input is never replaced by
these oracle values.

Here the complete intermediate forecasts are particularly simple. The c0
inner stage is5hOmega times the top form; the c1 inner is5i times top.
After star these are-5hOmega and-5i. The c0 outer is10P Gamma1 and its
contribution to the final lower result is-5P Gamma1. The first lower term
is-P RicciGamma. The c1 outer vanishes by its central scalar coefficient;
its first term is disabled, so the full formal-c coefficient is zero.

The four c0 rows (two points,two h) have28 nonzero final coefficients;
the four c1 rows vanish, but ALL eight inner rows are nonzero. The output
raw coefficient-square is

    2[9(15/4)^2+5(21/4)^2]=2115/4.

This is a coordinate diagnostic, not a selected source norm. Under the
declared real trace/form pairing, Pair(Gamma1,KF)=9(15/4)+5(21/4)=60,
while Pair(KF,KF)=0 because the chiral odd image is isotropic. A vanishing
self-pairing therefore does not conceal this nonzero curvature source.
Changing h reverses the grade13 companion, not the grade1 coefficient;
the outputs are distinct even though these three scalar controls agree.

Passing does not reject609's possible torsion cancellation. It does not
compute D_B S, the covariant gradient, global admissibility, a metric Euler
equation, source9.11, a stable vacuum or a physical spectrum.

## Bounded census, exact resources and freeze

The numerical menu is precisely two passed608 branch points, h=-1,+1 and
two formal coefficient slots. No c fit, scientific trial run or random
sampling is authorized. All arithmetic uses arbitrary-precision rational
and Gaussian-rational coefficients with tolerance0. The complete expected
counts and all rational coefficients appear in the frozen FixtureJson.

Known Clifford controls use every16384 blade, multiplied on each side by
each14 singleton generator and Omega, plus its square:31 products per blade,
507904 selected word-reduction comparisons, not an all-pairs enumeration.
All16384 Hodge-square cases are included. Independent plane controls have
1274 full vector outputs. Actual spin controls have2548 full vector outputs.
Frame curvature and each full coefficient symmetry census have76832 entries.
The eight chain rows have56 naive-versus-grouped stage comparisons and64
type/real-form checks. Four c0 norm/cross/isotropy rows and four c1 zero rows
are distinct controls; no count is learned from a scientific output.

Eight arithmetic controls precede the Clifford battery. Two frames give392
metric entries and392 inverse entries; determinant/orientation and reversed
orientation each have two controls. Ricci and Einstein each have392 entries.
Transport between the two framed curvature tensors has38416 comparisons.
Each of2548 actual spin outputs also has an independent word-kernel check.
There are two lift type/real-form rows, and two actual mixed-plane decoys
for EACH of wrong metric, wrong half factor and wrong curvature sign.
The91 known planes separately give49 metric-error rejections and42 accepted
same-sign controls; factor and sign errors each give91 rejections.

Every chain row checks both invariant tensors and seven stage equalities;
all eight stages have separate type and real-form counts64 each. Inner,
first-lower, outer, full-lower and immutable literal-chain comparisons each
have eight rows. Eight nonzero-inner controls, four nonzero-K and four
zero-c controls are separate. The four nonzero rows each check28 output
coefficients, coefficient-square, cross-pairing and self-pairing. Two
chirality-difference controls have14 nonzero coefficients and coefficient
square2115/2. Four cross-point chain equalities complete the census.

Fixed largest coordinate matrix14 and largest scalar curvature array38416
entries bound the tensor transforms. A curvature two-form has at most91^2
bivector coefficients,8281. The first C leg has at most14*(14+14)=392
entries (grades1/13). Before cancellation each fixed Phi2 slot's top-form
inner can have at most1002 coefficients (grades0/4 or10/14). The proved
actual inner has one coefficient, so its outer has at most28; all later
stages have at most392. Thus8281 also bounds every tracked intermediate
tensor in these fixed loops, including partial accumulation of each product.
The instrumented Matrix.Mul and form-kernel coefficient-product ceilings
are100000000 and50000000. They do not count every rational operation, and
the naive word kernel does not increment the grouped-kernel counter.
Planning estimates are60 CPU seconds/256MiB, with180 seconds/768MiB
allowances, not measured outcomes or runtime rejection gates. No
data-dependent search, interpolation or eigensolver occurs.

Twenty-four unique exact bindings cover own program/project/proof/helper,608's passed
output and full scientific pack,592's passed lemma/proof/contract, immutable
600 compiled arithmetic/form helpers and passed lineage,607's matrix helper
and passed lineage, primary source, complete live726 core manifest and
Directory.Build.props. Full fixture parity, every unique ID/path/hash and
every live core path/hash/tree must pass before arithmetic. All new files
have exactly one final newline before freeze; outputs are deterministic and
full/summary byte-identical. Any failed first output is preserved; repairs
require versioning and renewed full review.

Terminal precedence is invalid-or-drifted-input; known-answer-control-failed;
frame-curvature-control-failed; typed-spin-control-failed;
literal-contraction-control-failed; diagnostic-resource-control-failed;
success. Every required boolean and exact count must pass for auditPassed.

All14 authority flags remain false, externalReviewPending=true and
promotedPhysicalMassClaimCount=0. Only the unbound implementation note may
receive approved results after execution. No frozen historical bytes change.
