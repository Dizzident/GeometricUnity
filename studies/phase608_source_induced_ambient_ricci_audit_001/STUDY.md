# Phase608: full source-induced ambient curvature and Ricci

Prospective exact A58 audit. The full program, helper, project, this proof,
fixture menu, expected counts, precedence and hash lineage freeze before the
first scientific execution. Build-only checks do not execute this audit.
Success is `source-induced-ambient-ricci-controls-pass-conditional-curved-background`.

## Declared geometry and source boundary

Use commuting coordinates (x,y) on a local metric bundle over a flat
four-dimensional downstairs metric, with y a symmetric Lorentz matrix and
p=y inverse. Horizontal vectors u,v have four components; vertical constant
coordinate fields A,B are symmetric4 matrices. The declared metric is

    GHH(u,v)=sigma u^T y v, GHV=0,
    GVV(A,B)=alpha Tr(p A p B)+beta Tr(p A)Tr(p B).

Assume sigma and alpha nonzero and d=alpha+4beta nonzero. These are declared
local conventions, not a uniquely selected source normalization. Primary
text3.7-3.10 (lines883-935) gives H* as pulled-back cotangents and the
vertical double-contraction/trace choice. The orthogonal summand signs
appear at954-958. The downstairs LC lift at1005-1049 identifies the bundles;
1060-1065 restricts the upstairs metrics induced from downstairs data. At
9.1 the source metric variable is MET(X), not arbitrary MET(Y).

For covariant metric coordinates y, covectors have metric p. The musical
map sends u to yu, so (yu)^T p(yv)=u^T yv. This reconstructs GHH=sigma y;
the primary text does not literally print this coordinate formula. Replacing
y by p while keeping the same commuting vector fields changes the metric
and its derivatives, not merely notation. A y-dependent frame change would
also require its nonzero brackets and connection transformation.

This audit computes no spin lift or Shiab contraction. Nonzero curvature or
Ricci does not imply K(FB) nonzero. No source normalization, field equation,
stationary background, metric Euler equation, norm, physical signature
interpretation, stability, particle or unit choice is selected. Earlier
flat14D controls and the core's explicit flat-LC assumption are unchanged.

## Koszul connection and all independent typed curvature formulas

Write <u,v>=u^T yv, S(u,v)=(uv^T+vu^T)/2, tau=beta/d and

    W(u,v)=[y S(u,v)y-tau <u,v> y]/alpha.

Then GVV(W(u,v),A)=u^T A v. All nonzero connection types are

    Gamma(A,B)=-(A p B+B p A)/2,
    Gamma(A,u)=Gamma(u,A)=p A u/2,
    Gamma(u,v)=-sigma W(u,v)/2.

The first follows the full two-weight607 Koszul derivation. For the mixed
type, 2 GHH(Gamma(A,u),v)=sigma u^T A v; for two horizontal
arguments, 2 GVV(Gamma(u,v),A)=-sigma u^T A v. All remaining components
vanish. In particular VV has no horizontal component (total geodesy),
but the mixed connection is nonzero: the metric is not a product.

For constant vertical X, D_X p=-pXp and

    D_X W(u,v)=[X S(u,v)y+y S(u,v)X
                -tau((u^T Xv)y+<u,v>X)]/alpha.

Hence D_X Gamma(A,B)=(A p X p B+B p X p A)/2,
D_X Gamma(A,u)=-p X p A u/2, and
D_X Gamma(u,v)=-sigma D_X W(u,v)/2. All x derivatives vanish.

Use R(a,b)c=nabla_a nabla_b c-nabla_b nabla_a c for commuting fields.
Substituting these connection derivatives and compositions gives SIX typed
formulas, with the other two ordered types fixed by first-pair skew:

    R(A,B)C=-y [[pA,pB],pC]/4,
    R(A,B)u=-[pA,pB]u/4,
    R(A,u)B=p B p A u/4,
    R(A,u)v=-sigma W(p A u,v)/4,
    R(u,v)A=-sigma[W(u,p A v)-W(v,p A u)]/4,
    R(u,v)w=sigma(alpha+6beta)/(8alpha d)
              *[u<v,w>-v<u,w>].

For example the VVH derivative part is -[pA,pB]u/2 and its
connection-product part is +[pA,pB]u/4. The VHV products cancel the
pApB ordering, leaving pBpA/4. The HHH formula follows from
pW(v,w)u=[v<w,u>/2+w<v,u>/2-tau<v,w>u]/alpha.
These derivations retain both the trace term and all derivative terms.

The independent executable route builds the FULL14 Gram and its first and
second jets: horizontal derivatives D_A GHH=sigma A and DD GHH=0;
all x derivatives and cross blocks are zero; the vertical block uses the
immutable607 inverse-metric jet helper. Every jet entry is compared with
uncached matrix-product differentiation, including every zero slot.
The full Koszul covector is inverted against G for all ordered pairs.
Differentiated Koszul independently solves

    G D_x Gamma_ab = D_x Koszul_ab-(D_x G)Gamma_ab.

It is not replaced with the typed derivative oracle. Curvature composes the
recovered coordinate coefficients and uses the recovered derivatives. Full
14-component outputs, including zeros, are compared with the six formulas.
Lowering, first/last-pair skew, pair exchange and Bianchi are checked on
every ordered argument, not just an independent subset.

## Ricci, scalar and invariant projectors

Define Ric(b,c)=trace[a maps to R(a,b)c], an ordinary endomorphism trace.
The executable contracts its recovered coordinate R. A separate contraction
using the inverse Gram and lowered tensor must agree; neither route inserts
the expected Ricci formula.

The vertical intrinsic trace is -Tr(UV)+Tr(U)Tr(V)/4 in dimension4.
Horizontal inputs in R(u,A)B add -Tr(UV)/4. Therefore

    RicVV(A,B)=-5Tr(UV)/4+Tr(U)Tr(V)/4, U=pA,V=pB.

For horizontal Ricci the trace of A maps to W(pAu,v) is
(5/2-tau)<u,v>/alpha. This follows by tracing the symmetrized rank-one
map on Sym4; its trace is (4+1)<u,v>/2, while the scalar trace-correction
map has trace <u,v>. The vertical contribution is
-sigma(5/2-tau)<u,v>/(4alpha). The horizontal trace contributes
3sigma(1+2tau)<u,v>/(8alpha). Adding yields

    RicHH=-GHH/(4d), RicHV=0.

Let PT act on vertical A by A-Tr(pA)y/4 and kill horizontals. Its exact
rank is9, PT squared=PT; the complementary projector has rank5. Then

    G inverse Ric = -5 PT/(4alpha)-(I-PT)/(4d),
    scalar=-45/(4alpha)-5/(4d).

The executable verifies the full raised matrix, both projector relations and
ranks, scalar and full Einstein endomorphism. It does not infer multiplicity
from a numerical eigenvalue tolerance. At alpha1,beta-1/2 the Ricci
eigenvalues are -5/4 (nine) and+1/4 (five), scalar-10; Einstein eigenvalues
15/4 and21/4. At beta0 they are -5/4 and-1/4, scalar-25/2; Einstein
eigenvalues5 and6. Both full Ricci tensors are nonzero and non-Einstein.

## Menu, signatures and congruence

Four full contexts: y=diag(-1,1,1,1) or diag(-1,4,9,16), alpha1,
beta0 or-1/2, sigma-1. Use all four horizontal coordinate vectors then the
ten607 ordered symmetric basis matrices. The latter order is
E00,E11,E22,E33,E01+E10,E02+E20,E03+E30,E12+E21,E13+E31,E23+E32.

The determinant of the full Gram is
64 sigma^4 alpha^9 d/(det y)^4, from the607 vertical determinant and
det(sigma y). Its inertia is(8 positive,6 negative) for beta0 and(7,7)
for beta-1/2. Four additional metric-only sigma+1 checks give(10,4)
and(9,5), rejecting an unqualified(7,7) label. This is a sign-convention
control, not a rejection of other possible source summand conventions.

With L=diag(1,2,3,4), y'=L y L^T, use u'=L^-T u and
A'=L A L^T. These are a CONSTANT coordinate change on the total local
space. Full metric, connection, curvature and Ricci transport are checked.
Transporting horizontals by L instead is not the isometry here.

## Fixed positive anchors and planted errors

All fields below are transported at the second point. At eta put
A=diag(0,1,-1,0), B=E12+E21, u=e1, v=e2 and Y=y.
The three sectional curvatures are

    vertical(A,B): -1/2,
    mixed(Y,u): -1/(16d),
    horizontal(u,v): (1+6beta)/(8d).

The nonzero vertical anchor repeats607 without inferring the mixed terms.
The mixed trace anchor has Ric(Y,Y)=-1 even though the intrinsic vertical
trace direction is flat. Its norm is4d, consistent with the five-dimensional
eigenvalue. The full horizontal curvature tests another nonproduct component.

Two product-metric errors are separately exposed: intrinsic-only Ric(A,A)
is-2 instead of full-5/2; product horizontal Ric(u,u) is0 instead of
1/(4d). The trace error is0 instead of-1. The wrong-musical metric sigma p
is assembled independently for a Koszul test, not merely assigned a wrong
expected sign: for transported A=E11 and u=e1 its mixed Gamma is-u/2
instead of+u/2. At the scaled point both u and A are transported, so the
same typed defect is-u. Equal values y=p at eta do not conceal the jet error.

## Prospective census, resources and immutable lineage

Eight arithmetic known answers cover rational reduction, inverse matrices,
determinant, symmetric inertia and rank including singular/off-diagonal
pivots. Eight inverse controls check y and the full Gram in the four contexts.
Four contexts give784 Gram entries,10976 first jets,153664 second jets
and153664 second-jet symmetry checks. There are784 connection outputs,
10976 Koszul pairings,784 torsion checks,10976 compatibility checks,
10976 full connection-derivative outputs and10976 full curvature outputs.
First-pair skew and Bianchi each have10976 checks. Lowered entries,
last-pair skew and pair exchange each have153664 checks. Direct Ricci,
inverse-Gram Ricci, raised Ricci and Einstein each compare784 entries.
Four scalar/projector/metric-inertia/determinant rows are separate controls.
Across two weights, congruence gives392 metric,392 connection,5488
curvature and392 Ricci checks. Twelve sectional anchors, four trace Ricci
anchors, eight product Ricci decoys, four wrong-musical decoys and four
alternate-signature controls complete the geometric menu.

All arithmetic is exact arbitrary-precision rational; tolerance0. No finite
difference, interpolation, random sampling, fitting or target calibration is
performed. Maximum matrix dimension14 and dense jet/curvature array size
14^4=38416 are enforced by construction. The tracked Matrix.Mul ceiling
is200000000; it counts only calls to that instrumented multiplication,
not every Rational operation. Every loop bound is fixed. Planning estimates
are80 CPU seconds/256MiB, conservative240 seconds/768MiB; these are not
measured outcomes or runtime rejection gates.

Fifteen unique bindings include own program/project/proof/helper, immutable607
helper and its passed summary/contract/program/proof, immutable600 exact arithmetic and its
passed summary/contract, primary source, complete live726-file core manifest
and Directory.Build.props. No upstream bytes are copied or changed. Full
fixture equality, unique IDs/paths/hashes, passed upstream controls, all14
exact false flags and live core path/hash/tree equality are mandatory before
arithmetic. Full/summary outputs are byte-identical and deterministic.

Precedence: invalid-or-drifted-input; known-answer-control-failed;
metric-connection-control-failed; full-curvature-control-failed;
ricci-projector-control-failed; congruence-decoy-control-failed; success.
Every required control must pass for auditPassed. Failed first artifacts are
preserved; repair requires versioning and new review, never silent edits.
All new files have exactly one final newline before freeze. Only the unbound
implementation note may receive approved results. All14 flags remain false,
externalReviewPending=true and promotedPhysicalMassClaimCount=0.
