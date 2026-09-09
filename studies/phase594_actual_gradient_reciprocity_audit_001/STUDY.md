# Phase594: actual gradient, reciprocity and curvature reconstruction

Prospective A50 exact audit. Freeze every scientific input and fixture before
independent review and explicit first-run approval. No old study or core edit.
The action is retained; its actual derivative is computed, not selected as a
replacement source law. The result concerns finite pullbacks and necessary
local variational identities, not an ambient closure theorem.

## Background, tensors and nondegenerate carriers

Use the fixed flat oriented unit-volume (7,7) fourteen-torus, epsilon=I, B=0
and constant forms of Phase593. Thus derivative and boundary terms vanish.
The exact conventions are gamma_a squared sigma_a, Omega=gamma0...gamma13,
Omega squared I, and H=gamma7...gamma13. Hodge signs are
star(thetaI)=shuffle(I,Ic)*product_sigma(I)*thetaIc. Keep the full CCA chain

    K(F)=[Phi1 wedge star(F)]_C
         -(1/2) star([Phi1 wedge star([Phi2 wedge star(F)]_A)]_C),
    L=star1_inverse K, C(X,Y)=XY-YX, A(X,Y)=i(XY+YX),
    Phi1=(a+b Omega)gamma, Phi2=(c+i d Omega)Gamma2.

Pair forms by B=-ReTr(XY)/128 and the signed exterior metric. This is the
declared nondegenerate diagnostic pairing, not a positive source-selected
norm. The actual full K is evaluated before projection; all pairing and
coefficient predictions are checked independently.

There are two three-dimensional torsion carriers, with coordinates (x,y,z):

    E1=(theta0 Gamma01, theta1 Gamma12, theta1 gamma2),
    E2=(theta0 Gamma02, theta1 gamma1, theta2 Omega).

Their computed Gram matrices must be diag(1,1,-1) and diag(1,-1,-1),
respectively. They are nondegenerate, despite the indefinite ambient pairing.
Test E1 for the canonical (a,b,d)=(1,0,0) and matching rows (1,h,-h), h=+/-1;
test E2 only on those two matching rows. Keep c as separate coefficients of
1 and c, not a finite parameter scan. No other source operator is selected.

The one-dimensional curvature images have bases

    f1=theta01 Gamma02, f2=theta12 Omega gamma1,
    Q(T)=T wedge T=2xy f1 or -2yz f2.

Both curvature Grams equal +1. Let K_E be the B-orthogonal projection of
L restricted to this curvature image onto E. Since E is nondegenerate,
this is well-defined and B(T,K_E F)=B(T,L F) for every T in E. The full
output need not belong to E; its discarded component is orthogonal to E,
not absent from the source operator. The predicted c^0 columns of K_E are
(0,0,-2a) and (-2h,0,0); the c^1 columns vanish.

## Independent actual gradient and adjoints

Define q(U,V)=(U wedge V+V wedge U)/2, so Q=q(T,T), DQ[T]V=2q(T,V).
Construct Q_T=q(T,.) by literal associative products. Its coordinate rows
must be (y,x,0) on E1 and (0,-z,-y) on E2. Compute adjoints from the
actual Gram matrices, A^dagger=G_domain^-1 A^T G_range, and verify every
basis pairing identity. Projecting first is valid only for this pulled-back
action; no full ambient-gradient claim is made.

Freeze gamma=1,2 as the two printed-bracket encodings of Phase593, keeping
the geometric curvature Q fixed. The same cubic action is

    I_gamma=(gamma/3) B(T,L Q(T)).

One route constructs T wedge K(Q), takes its top trace, and differentiates
the resulting exact polynomial in x,y,z. A separate route computes

    g_actual=(gamma/3)[K_E Q+2 Q_T^dagger K_E^dagger T].

All three raised gradient components must agree with G_E^-1 times the
independent polynomial derivatives. The predicted derivative covectors are
beta(yz,xz,xy), beta=4gamma/3 on E1 and beta=4hgamma/3 on E2. Desired
curvature-only covectors are (0,0,4xy) and (4hyz,0,0), independent of gamma.
Every c coefficient vanishes. The full three derivatives, not only z, are
the principal new controls beyond Phase593.

## Reciprocity and Euler potential

For i<j define curl_ij=partial_i f_j-partial_j f_i. Desired curls in the
order xy,xz,yz are (0,4y,4x) on E1 and (-4hz,-4hy,0) on E2. Actual
gradient covector curls vanish. Check all nine entries of the direct
polynomial Hessian against G_E times the independently obtained raised
gradient Jacobian. The latter weighted matrix is symmetric. The ordinary
unweighted vector Jacobian is NOT symmetric in the c^0 rows at (1,1,1);
the wrong-metric decoy must reject that test rather than falsely reject a
true gradient. At the origin both curl evaluations vanish, so an origin-only
test would be insufficient; full polynomial identities are authoritative.

A homogeneous quadratic covector f, if it were a gradient, would have the
unique cubic potential P=(x f_x+y f_y+z f_z)/3, up to an irrelevant constant.
Indeed closure allows the derivative of this contraction to become f by
Euler homogeneity. Construct P from the actual desired covector and check
its derivatives: they differ from f, certifying the scoped nonintegrability.
For the actual action verify x I_x+y I_y+z I_z=3I coefficientwise.

## Curvature-only factorization and collisions

Freeze monomials (x^2,xy,xz,y^2,yz,z^2). Their symmetric-tensor basis uses
e_i tensor e_i on the diagonal and e_i tensor e_j+e_j tensor e_i off the
diagonal, WITHOUT one-half. Thus T tensor T has exactly these monomial
coordinates. Extract the complete 1x6 curvature coefficient map from Q;
its sole nonzero column is 2 in xy for E1 and -2 in yz for E2. Both have
rank1 and kernel dimension5. Compute the exact rational kernel rather than
testing only nonlinear zero sets. The actual raised-gradient coefficient
map does not annihilate that kernel in c^0 rows; in particular the xz column
survives. Therefore no linear curvature-only K' can reproduce that gradient.
The c^1 zero map correctly passes this factorization control.

Equal-curvature pairs (1,1,0)/(1,1,1) on E1 and (0,1,1)/(1,1,1) on E2
also have unequal corrected gradients. Verify the full Clifford-valued
curvature equality, not just one coordinate. This excludes even a
single-valued nonlinear function of this same curvature on the carrier.
It does not exclude explicit torsion inputs, different observables, or
restricted coefficient spaces. No alternative K' is installed.

## Riemann projection is an input-changing decoy

On the three curvature directions (theta01 Gamma02, theta02 Gamma01,
theta12 Omega gamma1), the declared algebraic-Riemann projection is

    Pi=[[1/2,1/2,0],[1/2,1/2,0],[0,0,0]].

The first two directions are a shared-index pair and its transpose;
symmetrization satisfies Bianchi on this small carrier. The grade13
direction is not spin-bivector Riemann curvature and is discarded. This
is the restriction of the projection to these declared directions, not an
implementation of a full ambient 3185-dimensional projector. Verify its
Gram, rank1, idempotence, self-adjointness, preserved symmetric direction
and killed antisymmetric/grade13 directions. Apply it before the full
literal K. It halves E1's desired covector and annihilates E2's curvature.
This changes the inputs and cannot be called an unchanged-source repair.

## Genuine positive controls and complete menu

Use the same compact three-coordinate Chern-Simons control as Phase593:
A=x theta0 Gamma01+y theta1 Gamma12+z theta2 Gamma02. Let
nu11=theta3 wedge ... wedge theta13 and K_CS(F)=F wedge nu11. The E Gram
is I3; curvature basis is (theta01 Gamma02,theta02 Gamma12,theta12 Gamma01),
also Gram I3. Literal products must yield Q=(2xy,-2xz,2yz) and the pulled
back matrix [[0,0,1],[0,-1,0],[1,0,0]]. The normalized action is2xyz,
force and true gradient are(2yz,2xz,2xy), curls vanish, Euler holds and
the q coefficient map has rank3/kernel3 annihilated by the gradient map.
Check both bracket encodings using coefficient1/(3gamma) on gamma Q.
Also differentiate (kappa/2)B(T,T), kappa=-1,0,1, on all five source rows:
the raised gradient is kappa T, covector is kappa G_E T and weighted
Jacobian is symmetric. Adding this mass contribution cannot change the
desired cubic-force curl.

Five source operator/carrier rows, two c coefficients and two gamma
encodings give20 full-gradient rows,60 derivative components,180 Jacobian
entries and60 curl components. There are10 nonzero desired-curl rows,
10 zero c rows,10 wrong-unweighted-Jacobian rejections,10 linear and
10 nonlinear curvature-factorization rejections. Each of10 coefficient
rows has three K-adjoint and three Q_T-adjoint basis-pairing checks.
Five computed source curvature kernels each have dimension5. Test10
Riemann-projection coefficient rows,15 mass rows, and two CS encodings.
The finite point menu is only origin and (1,1,1) for curl decoys plus the
two declared equal-curvature pairs; no sampling or fitted tolerances.

Known answers retain256 independent word/blade products on the16 masks
generated by axes0,1,2 and Omega, all16384 Hodge squares, exact polynomial
derivative checks and rational inverse/rank/kernel controls. Existing
broader Clifford controls are bound upstream, not reclassified as this
small mask menu. All comparisons have exact tolerance zero.

## Freeze and boundaries

Own program/project/proof and polynomial-tensor helper are exact-bound.
The project also explicitly compiles the unchanged Phase592 ExactAlgebra.cs;
that complete build input, Phase593 program/summary/contract, primary source,
Directory.Build.props and the live726-file core manifest are bound. No
copied helper is untracked. The new polynomial helper's provenance is the
bound593 code, with explicit coefficient/evaluation methods for this audit.
Structural fixture equality, unique bindings and upstream pass validation
precede science. Full and summary outputs are byte-identical and deterministic.

Estimated resources:5 CPU seconds/64MiB, prospective ceilings20 CPU
seconds/128MiB. Terminal precedence: invalid-or-drifted-input;
known-answer-control-failed; carrier-adjoint-control-failed;
gradient-reciprocity-control-failed; factorization-projection-control-failed;
positive-control-failed; then
`actual-gradient-controls-pass-curvature-only-force-rejected`.
Only the final terminal passes the audit. Failed frozen outputs are retained;
scientific repairs require a versioned successor and fresh approval.

All14 authority firewalls remain false, externalReviewPending=true,
promotedPhysicalMassClaimCount=0. No action, registered core, source choice,
physical dimensional reduction, positive kinetic norm or mass claim changes.
