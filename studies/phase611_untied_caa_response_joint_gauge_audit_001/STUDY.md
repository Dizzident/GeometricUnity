# Phase611: untied canonical CAA response and joint quadratic gauge

Prospective A59 audit. All code/helpers/project/proof, coefficients, finite
menus, counts, resources, tolerance and lineage freeze before FIRST science.
Build-only checks are allowed. Complete independent/MAIN review and explicit
MAIN approval precede execution. Success is
`untied-caa-response-controls-pass-source-choice-open`.

## Declared operator and source boundary

Use the canonical untied CAA chain: first commutator C, outer and inner
A=i anticommutator, Phi1=Gamma1 and Phi2=Gamma2. Here
Gamma1=sum theta^a gamma_a and Gamma2=sum_(a<b)theta^ab gamma_a gamma_b.
The first and outer Phi1 occurrences have DIFFERENT brackets. This is
outside591/592's conservatively tied family. The source permits bracket
tools and combinations in8.1, but does not select this untied law as its
missing final operator. Einstein matching does not decide authorial intent.

Signature is+ on axes0..6,- on7..13; increasing exterior/Clifford order,
Omega=gamma0...gamma13 and positive volume are fixed. The declared real
pairing contracts form indices and uses-ReTr(XY)/128, not a Hermitian
positive norm. No physical time, poles, source norm, global functional
domain, stable vacuum, field extraction or unit scale is selected.

For a two-form F, the FULL literal lower operator is

    sf=star(F), one=C(Gamma1,sf), inner=A(Gamma2,sf),
    zero=star(inner), outer=A(Gamma1,zero),
    K(F)=star(one-star(outer)/2).

The outer star and ordered half are retained. Unlike the tied chiral
operator, canonical CAA is not premised to have a nilpotent image flag.
The result cannot overturn the correctly scoped tied-family theorem.

## Full real transpose and physical derivative axis

For the coefficient brackets, real bilinear transposition reverses C but
retains the i in A: it does not complex-conjugate that i. Exterior wedge
transposition includes the contracted form signature and shuffle sign.
Use the immutable600 BracketAdjoint and StarAdjoint primitives, but replace
the outer-C transpose by outer-A. One reverse route explicitly reverses
every literal star and bracket. Independently simplifying adjacent stars
gives

    Kdag_first(Y)=-star C_Gamma1^dag(star Y),
    Kdag_second(Y)=-star A_Gamma2^dag(star A_Gamma1^dag Y)/2.

The two routes agree as FULL two-form/Clifford tensors. Defining forward
pairings reconstruct every possible coefficient on the proved support;
this is not an adjoint defined only after a two-mode projection.

The Fourier phase coordinate is represented by the immutable helper's K0
index. Its PHYSICAL direction is explicitly t=0 or7. Thus

    d_t=theta^t wedge partial_phase,
    d_t^dag=-sigma_t contraction_t partial_phase.

The immutable600 d/dagger routines hard-code physical directions0/1 and
are NOT reused for physical t7. Contraction retains the position of t in
each increasing exterior mask. Negative signature is not obtained by simply
renaming a frequency index without changing its coframe and metric sign.

## Full two-mode closure, not compression

For t=0,7, n=1,2, with gamma2 positive, freeze

    u=theta^t gamma2 cos(n x_t),
    e=sum_(j!=t,2) theta^j gamma2 gamma_j sin(n x_t).

The displayed products are ORDERED words: gamma2 gamma_j and theta^t
wedge theta^j can acquire minus signs when converted to increasing masks.
There are12 terms in e. The exact normalized-period Gram is
diag(-sigma_t/2,6), nondegenerate. The complete adjoints are

    Kdag u=2 sum_(j!=t,2) theta^t wedge theta^j gamma2 gamma_j cos,
    Kdag e=2 sum_(j!=t,2) theta^2 wedge theta^j gamma_j sin.

For both inputs A_Gamma1^dag Y vanishes term by term: for u it contains
{gamma_t,gamma2}=0, and for e it contains {gamma_j,gamma2 gamma_j}=0.
Consequently the second adjoint vanishes, but the full first adjoints each
have12 nonzero curvature components BEFORE differentiation. Deleting
Kdag e would incorrectly pass the final divergence alone, since its forms
contain no t. This audit compares and reconstructs that tensor explicitly.

The support proof is coefficientwise. For an input u, the only possible
adjoint Clifford mask on an exterior pair ab is ab XOR t XOR2; for e it
is ab XOR2. Every exterior pair is probed, including zero candidates.
The first-C transpose and the identically zero outer-A transpose exclude
all other masks, independently of the finite pairing values. Each candidate
uses the real H-anti phase1 if its adjoint sign is-1 and i otherwise;
its nonzero signed form/trace Gram is used to reconstruct the coefficient.
For Gamma1 the candidate is blade ab itself. No positive-norm substitution
or incomplete probe-only completeness assertion is used.

Now d u=0. The full ddag Kdag u is2sigma_t n e, whereas ddag Kdag e=0.
For d e, the inner A term vanishes by the shared Clifford index and the
first lower term is-24n u. Hence the actual first-action Hessian is

    H=(K d+ddag Kdag)/2,
    H u=sigma_t n e, H e=-12n u, H squared=-12sigma_t n squared I.

The executable computes both FULL columns and FULL second applications,
then recovers and reconstructs coefficients with the signed Gram. It does
not obtain closure merely by taking pairings. The matrix in basis(u,e) is
[[0,-12n],[sigma_t n,0]], with nonzero determinant12sigma_t n squared.
Its exact quadratic minimal polynomial is lambda squared+12sigma_t n squared;
no numerical eigenvalue or physical frequency interpretation is used.
This determinant/minimal polynomial belongs ONLY to the proved closed
two-dimensional carrier, not the full field operator. Other full-field
longitudinal directions can be null even at non-null covector; no full
operator rank, invertibility or characteristic-cone assertion follows.

The original quadratic action, independently expanded with the literal K,
is Pair(xu+ye,K d(xu+ye))/2=6sigma_t nxy. Its full polarized Hessian is
Gram times the raised matrix, with off-diagonal entries6sigma_t n and
zero diagonal entries. This verifies weighted reciprocity without confusing
gradient vectors and covectors in the indefinite pairing.

At physical n0, e vanishes and u=theta^t gamma2 is constant. Its Gram is
-sigma_t, not the averaged half. The actual carrier is ONE-dimensional,
H u=0. The full Kdag u is still checked before its zero divergence. There
is no periodic alpha=sin(nx_t)/n at n0; its formal limit x_t is not a
periodic scalar, so no exact-mode gauge assertion is borrowed for this row.

## Nonvacuous second-A and old cubic controls

The Fourier seeds alone cannot validate the second leg. Independently take
F=theta01 gamma0 gamma1/2 and Y=Gamma1. The literal stage predictions are

    first lower=-theta0 gamma0-theta1 gamma1,
    inner=-i top, zero=+i I, outer=-2Gamma1,
    second lower=+Gamma1, K(F)=sum_(j!=0,1)theta^j gamma_j.

The full adjoint on Gamma1 is first4Gamma2 plus second-28Gamma2, hence
-24Gamma2. Both defining pairings equal-12. Its outer-A transpose is28iI;
this exposes the sign of the real A transpose. The second forward/adjoint
legs are individually nonzero and cannot be removed because the seeds
happen to kill them. All91 exterior-pair adjoint candidates are reconstructed.

Preserve the593 counterexample, now evaluated through this literal CAA:
T=x theta0 Gamma01+y theta1 Gamma12+z theta1 gamma2 has
Q=T wedge T=2xy theta01 Gamma02. Its inner-A and outer-A transpose vanish,
so the cubic action is4gamma xyz/3, while the claimed force covector is
(0,0,4xy). Both gamma1 and2 are tested coefficientwise. The actual three
action derivatives are(4gamma yz/3,4gamma xz/3,4gamma xy/3), not that
claimed covector. Einstein matching and non-nilpotent quadratic response
do not repair the earlier action/equation mismatch.

## Joint epsilon/connection quadratic test

For nonzero n only, alpha=gamma2 sin(nx_t)/n gives d alpha=u.
Use epsilon=exp(r alpha), omega=s u+y e, and the declared plus lift
B=epsilon inverse d epsilon, T=omega-B. Since alpha and its derivative
commute, B=r u and FB=0 exactly. The finite scalar/connection expressions
are not used to bypass the independent computation: the executable builds
epsilon and its inverse through degree2, computes B, FB, both conjugated
Phi tensors, D_B T, Q and the original action by polynomial products.
Every retained coefficient comes from the literal chain.

The exact degree-two action is

    I2=6sigma_t n(s-r)y.

Corrections from the tensor conjugation and B in D_B T enter action degree3
or higher. The code checks their nonzero tensor first jets and the B/FB
identities, not an assumption that those fields are constant under epsilon.
All nine actual Hessian entries must equal

    6sigma_t n [[0,1,0],[1,0,-1],[0,-1,0]].

It has rank2 and null vector(1,0,1); the fixed-epsilon s direction is NOT
null. The planted wrong T=omega+B gives6sigma_t n(s+r)y and maps that
same purported null vector to(0,12sigma_t n,0). All nine wrong-lift entries
and the nonzero defect are tested, rather than merely labeling the lift wrong.

This is quadratic joint gauge redundancy, not removal of u alone or a
finite-amplitude gauge quotient. Indeed Ad_epsilon T contains at order ry
the new field(2ry/n)sum_(j!=t,2)theta^j gamma_j sin squared(nx_t), outside
span(u,e). No finite Ward orbit closure on these two modes is asserted.
Global gauge domains, source printed tau signs, metric coupling and physical
quotient dynamics remain unselected.

## Frozen menu and count derivation

There are four Fourier rows(t0/7,n1/2) and two actual constant rows(t0/7,n0).
Full source adjoint support probes number4*2*91+2*91+91=1001, the last91
for Gamma1. Each probe uses the forward literal CAA, an independent signed
Gram denominator, and reconstructs the entire adjoint on its proved support.
Ten full Hessian columns and eighteen original-action bilinears cover the
four2D and two1D carriers. Eight FULL H-squared outputs cover both vectors
of each Fourier row. Each Fourier row has all nine joint Hessian and all
nine wrong-lift entries,36 each across the menu. The complete polynomial
coefficients, all three derivatives and rank/null controls are separately
frozen in FixtureJson; no count is obtained by trial execution.

The complete census is dimension-derived. There are16384*31=507904
selected word products and16384 Hodge-square controls, plus four rational
and four polynomial known answers. Ten carrier inputs plus Gamma1 give
eleven full adjoint/leg/reconstruction rows; the ten carrier inputs have
zero outer adjoint and nonzero full adjoint. Ten columns times seven
forward stages give70 optimized-versus-independent-word comparisons.
The1001 support probes use optimized forward products, not a claim of
1001 individually naive-word evaluations. Gram, recovered matrix,
reciprocity and original-action counts are each4*4+2=18. Eight squared
columns, four minimal-polynomial/fixed-epsilon-nonnull rows and two
constant zero rows cover the physical carrier menu. The unit plane has
eight stage, three adjoint-leg, two pairing and two nonzero-second controls.

Each of four joint rows has epsilon inverse, B, FB and retained-coefficient
type controls, plus two transported tensor jets. Three variables through
degree2 have binomial(5,3)=10 monomials:40 action coefficients and40
wrong-action coefficients,120 derivative coefficients,36 Hessian and36
wrong-Hessian entries. There are four null/rank/wrong-lift rows each.
The separate cubic Q/K/inner have one row each and three outer-adjoint
zeros. Two gamma values times binomial(6,3)=20 monomials give40 action,
120 derivative and120 target coefficients, plus two certified mismatches.

Exact arithmetic uses immutable600 arbitrary-precision rationals, Clifford,
Fourier and transpose primitives. New CAA and physical-axis derivatives are
local code, not modifications of frozen helpers. Formal polynomial degree
is capped at2 for joint controls and3 for the distinct cubic control; those
are exact jets, not amplitude samples or fitted approximations. The largest
carrier matrix is3, but FULL tensor coefficients are retained throughout.
All tolerances are zero. Planning resource estimates and instrumented
operation ceilings are specified in the full fixture; none is a measured
result or a relaxation of the fixed mathematical menu.

The8192 Fourier-term guard has a pre-execution support margin, including
partial products rather than relying on final cancellations. For a fixed
joint polynomial coefficient, the argument's degree-one part has at most24
Fourier entries; the degree-two yy part has66 form/blade pairs times three
harmonics=198 (other degree-two parts have at most36). Wedge with the
complementary two-form permits at most two coframe choices in the first
Gamma1 leg, giving396 candidates; the first tensor jet against the linear
argument adds at most96, hence492. The inner Gamma2 leg must match the
exact exterior pair, giving at most198+48=246 candidates. The subsequent
outer leg has at most14*246+26*24=4068; adding the first leg gives4560.
These counts bound candidates before cancellation and therefore partial
accumulations as well. Epsilon conjugation has at most91 blades times
three harmonics=273 entries per Gamma2 coefficient, or42 for Gamma1.
Ordinary single-pair support probes, the two seed adjoints, and the single
constant cubic-curvature term are smaller. Polynomial coefficients remain
separate, with at most20 monomials, and scalar pairing stores no FT product.
Every retained CAA-chain stage is checked for exterior type and H-anti real
form; scalar pairing rejects non-H-anti coefficient inputs. Ordered cubic
words are grouped before taking real scalar pairings.

The estimated60 CPU seconds and256MiB peak memory are planning estimates,
not measured results or runtime time/memory guarantees. The hard recorded
checks are at most50000000 tracked grouped Fourier coefficient products
and at most8192 entries in each instrumented Fourier tensor. Naive-word
kernel operations are outside that product counter and are independently
bounded by the finite frozen menus. Polynomial degree3,20 coefficients and
matrix dimension3 are structural bounds of the declared loops. No observed
runtime or failure is used to adjust any ceiling.

Unique exact lineage binds every compiled helper and own scientific file,
the primary source and prior scoped findings, and the complete live726 core
manifest plus Directory.Build.props. Full fixture equality, every ID/path/hash,
all fourteen exact false flags and upstream success are mandatory before
science. Every new file has one final newline; full/summary evidence is
deterministic and byte-identical. Failed first outputs are preserved and
repairs require versioning and renewed review. Only the unbound implementation
note may receive approved results. All14 flags remain false, external review
pending and promotedPhysicalMassClaimCount0.

There are exactly23 unique bindings: five own scientific files; the three
compiled600 helpers and its program/contract/passed summary; the proof,
contract and passed summary of each591,593,601; and primary source,
complete core manifest and Directory.Build.props. The contract is separately
hashed and does not self-bind. Terminal precedence is invalid-or-drifted-input,
known-answer-control-failed, full-adjoint-control-failed,
full-hessian-action-control-failed, joint-quadratic-control-failed,
cubic-boundary-control-failed, resource-census-control-failed, then
untied-caa-response-controls-pass-source-choice-open. Success requires all
frozen controls, exact bindings, live core closure and counts; it selects no
source operator, norm, physical propagation or finite gauge quotient.
