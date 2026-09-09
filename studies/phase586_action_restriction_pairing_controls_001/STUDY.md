# Phase586: action restriction and pairing controls

Prospective A46 deterministic controls. This pack must be reviewed by the
coordinator before its first scientific execution. Build checks are permitted.
No sampling, source-map selection, core edit, or old-result reinterpretation.

## Restriction and compatible carriers

Draft sections 9.1-9.2 motivate a curvature-plus-relative-field residual and
its squared norm; section12.4 explicitly discusses omitting the first-order
quadratic potential. Thus kappa=0 is a discussed subcase, not an author-selected
restriction and not the geometric equation T=A-B=0. The draft norm square has
no factor 1/2; this control declares the conventional normalization S=|U|^2/2.
The source pairing, signature, Hodge legs and exact carrier map remain unfilled.

Use a compact su(2) Lie-vector pairing |X|^2=-Tr(ad(X)^2)/2 and a positive
Euclidean coefficient metric. On an open patch, A0=0 and T=t X dx1 give
F(A0+T)=0 although T is nonzero. The actual registered curvature assembler and
EinsteinianShiabOperator.ComputeJointGradient evaluate exact edge integrals
omega_e=t*(x1(end)-x1(start))*Jx on CreateUniform4D(1), with theta=0, sd2/id0
and coefficient 1/2. Curvature, objective and both gradients should vanish for
t=-2,-1,0,1,2. Nonzero coefficient norm is checked for every nonzero t.
This is an open mesh control of the actual code, not the periodic sampled
ensemble, a globally trivialization theorem, or an identification omega=T.
A flat reference on a torus could still have nontrivial holonomy.

The separate illustrative continuum first-jet carrier is R6. Explicit maps
K(F12)=(F12,0) and C(T1,T2)=(T1,T2) give U=K F+kappa C T.
Here F12 is a two-form component and T1,T2 are one-form components; only their
images in the declared common carrier are added. No edge one-form array is
added to a face two-form array. In the commuting fixture |C T|^2=t^2 and
S=kappa^2*t^2/2 for kappa=-2,0,2. This distinguishes coupling omission from
T=0 under the declared positive pairing without assigning a physical mass.

## Nonzero reference and variations

At the origin of a two-coordinate patch freeze A0_i and T_i and affine curls
as in contract fixtures. They can be realized by setting partial1 A2 equal
to the listed curl and all other first derivatives zero. Matrix commutators
assemble F(A0+T) directly. An independent Lie-vector cross-product expansion
checks F0+d0 T+T wedge T, with F0 nonzero and a reference-omission decoy.

In either compatible Phase583 sign family, A=A0+c*p, B=A0 on epsilon=I,
p=c*T and c in {-1,+1}. At fixed reference, eta=epsilon^-1 delta epsilon:

    delta T=c*delta p-d_B eta,
    delta U=K d_A(c*delta p)+kappa C(c*delta p-d_B eta).

Central directional differences of both U and S check the analytic p and
epsilon derivatives. The epsilon jet is independently realized by
epsilon(s,x)=exp(s eta) exp(s x1 deta1) exp(s x2 deta2) at x=0, so
B_i=epsilon^-1 A0_i epsilon+s deta_i. A third direction varies A0 with
T fixed, hence delta T=0 and delta U=K d_A(delta A0). Every sign/coupling
row checks all six residual/action derivatives. No derivative of source K is
discarded: K is explicitly fixed in this toy model. For the source one must
add (delta K)F, variation of its carrier/pairing where applicable, and show
the required integration-by-parts and cyclic identities for its first-order
transgression action. Those identities are not claimed here.

## Omitted linear maps

For field-independent L, W=L^T M_source L remains in S=y^T W y/2.
At y=0, H=J^T W J. At nonzero y, also retain
sum_a (W y)_a Hessian(y_a). Linearity removes derivatives of L, not W.
Equality of pulled-back forms on the reachable residual subspace is sufficient
for action equality; at zero residual equality on the tangent image suffices
for Hessian equality. Equality of eigenvalue lists alone does not prove either.

With y(q)=q in R2, M=I, L=diag(2,1) changes ordinary Hessian eigenvalues
from (1,1) to (1,4). A 90-degree rotation is the isometric control. With
y(q)=(q^2,q), the one-field Hessian is 6*q^2+1 before L and 24*q^2+1
after the nonisometric L. At q=0 and q=1 the program checks this through both
analytic derivatives and a central second action difference. The rotation
preserves both values, including the nonzero-residual second-derivative term.

For q=R z, R=diag(2,3), H=[[2,1],[1,3]], and kinetic metric G=I,
H_z=R^T H R and G_z=R^T G R. Ordinary eigenvalues change, while the
generalized eigenvalues of (H_z,G_z) equal those of (H,G). Here diagonal G_z
allows independent symmetric whitening before the analytic 2x2 eigenvalue
formula. No ordinary Hessian eigenvalue is identified as a physical pole.

Finally y(t)=(t^2,t), L=(0,1) annihilates the leading vector. The norm-square
degree falls from four to two. The exact samples t=0,1,2 recover a fourth-degree
coefficient of 1/2 before projection and zero after; t=3 checks the prediction.
Linearity preserves a degree upper bound. Exact degree needs nonannihilation
and a nonzero leading quadratic norm. This clarifies the recorded degree
reduction scope without rewriting prior degree results.

## Freeze and terminal

Contract fixtures are checked structurally against the entire embedded menu;
every tolerance, sign, amplitude, map and shape is used or checked. Ten unique
id/path bindings include program, project, this study, draft, Phase583 summary,
three core files, Directory.Build.props, and a source-tree manifest. The tree
digest is SHA256 of sorted `path + space + file SHA256 + newline` entries for
every src/**/*.cs and src/**/*.csproj excluding bin/obj. It covers transitive
compiled repository sources without relying on a shared integration verifier.
The exact-bound file `preregistration/core_source_manifest_v1.json` lists every
sorted path/hash pair and the tree digest. Before any scientific control the
program validates that list, all live hashes, the algorithm and the tree digest.
The program validates exact binding identities and count, all 14 inherited
firewalls, terminal precedence, externalReviewPending=true and mass count zero.

Exact algebra tolerance is 1e-12, derivative tolerance 2e-7, dyadic step 2^-16,
and nonzero floor 0.01. No tolerance tuning after a frozen scientific run.
Estimated Release execution is under 10 seconds and 256 MiB; no chains or fits.
Terminal precedence: invalid-or-drifted-input; known-answer-battery-failed;
mathematical-control-failed; action-restriction-controls-pass-pairing-bridge-unresolved.
The success terminal certifies only the declared controls. Actual source maps,
pairing, action descent, quantum convergence and pole interpretation remain open.
Every output retains the exact Phase583 authority firewall, external review
pending and promotedPhysicalMassClaimCount=0.
