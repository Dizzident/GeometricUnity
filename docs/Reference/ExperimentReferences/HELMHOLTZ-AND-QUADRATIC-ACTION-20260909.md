# Conditional Helmholtz classification and quadratic action

## Status and primary premises

Amendment A51, Phases596-597: both first frozen Release executions passed
after independent and coordinator complete code/proof/fixture/hash review.
The prospective predictions below were fixed before those executions;
the full-action descent successor remains analytical and unexecuted.
The source is the April1,2021 author
draft, local text `texts/GU-DRAFT-2021-TEXT.txt`, SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Author PDF: <https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf>.
Relevant equations are6.18-6.22,7.3,8.1,8.7 and9.3-9.7. The deductions
here are internal conditional mathematics, not an author-intent ruling.

## Why curvature-only forces have a restrictive integrability condition

Hold the background, nondegenerate pairing and coefficient tensors fixed.
Assume a zero-order pointwise linear operator K, independent of the varied
connection A, and require the force to be exactly K F_A on unrestricted
Lie-algebra-valued connection variations. These hypotheses exclude the
actual corrected torsion-dependent Euler operator constructed in594.

Lower the output pairing and write
`E_ia = (1/2) L_ia,jkb F_jk^b`, with L antisymmetric in j,k.
Formal self-adjointness of the principal derivative coefficient forces
`L_ia,jkb = -L_kb,jia = L_kb,ija`. Three cyclic applications force
symmetry in a,b; together with the original antisymmetry they force total
spatial antisymmetry. Thus the coefficient lies in
`Lambda^3(V*) tensor Sym^2(g*)`. The constant-field quadratic force then
has symmetric derivatives only if each symmetric Lie pairing h satisfies
`h(X,[Y,Z]) + h(Y,[X,Z]) = 0`: ad-invariance.

This is a pointwise principal/cubic classification. For nonconstant
coefficients there is an additional differential condition. In the
Chern-Simons description the coefficient is an (n-3)-form beta and must
be closed. The Phase596 variable-coefficient periodic decoy is designed
to pass the pointwise tests while failing the claimed force identity.

For real u(64,64), the simple summand su(64,64) has one invariant symmetric
bilinear form up to scale: complexification is sl(128,C), whose invariant
bilinear forms are multiples of its trace/Killing form. The one-dimensional
center contributes a center-square form. Cross terms vanish because the
simple ideal is perfect. Both surviving forms are fixed by Spin conjugation.
If K is also genuinely pointwise Spin-intertwining, its spatial three-form
coefficient would therefore have to be Spin-invariant. None is: for each
three-axis mask in signature(7,7), choose an occupied and an unoccupied
axis in one same-signature seven-axis block. The real compact pi rotation
in that plane reverses its sign. This conditional argument does not require
extrapolating a small matrix calculation to128-dimensional matrices.

Scope matters. Frame covariance with additional background tensors does NOT
automatically imply pointwise Spin invariance. For the displayed8.1/9.3
operations, invariant Phi tensors and uniform coefficient conjugation give
`K_epsilon = Ad(epsilon^-1) K_0 Ad(epsilon)`; brackets, wedge, Hodge and
trace commute with that uniform internal conjugation. This preserves the
pointwise principal/cubic conditions. A more general lost operator, extra
geometric tensors, connection-dependent or differential K, restricted
variables, or a different target equation is outside this argument.
Adding independent fields cannot by itself repair a nonsymmetric A-A
Hessian block, though changing the actual dependence/variation problem can.

## Phase596 controls

Use spatial dimension3 and u(1,1), with H=diag(1,-1), real basis
(iI,sigma1,sigma2,i sigma3) and normalized pairing -ReTr/2 of Gram
diag(1,-1,-1,1). Direct Gaussian2x2 matrix arithmetic must verify the
real form, all brackets, Jacobi and Gram entries.

All144 lowered operator coefficients are unknowns. Predict234 principal
equations of rank134/nullity10; on that space40 ad-invariance equations
have rank8/nullity2. Independently assembled792 cubic-Jacobian coefficient
equations should give combined rank142/nullity2. The survivors are
center-square and the simple trace pairing. Symmetric h denotes the
LOWERED bilinear coefficient; the actual K includes the inverse Gram.

Nonabelian and central derivative controls exercise both directions:
the first has action2xyz; the second has periodic averaged action-pq/2.
A symmetric noninvariant Lie pairing must fail the cubic condition.
For lambda=cos(theta), central A1=p sin(theta), A2=q gives actual action
pq/4 and derivative(q/4,p/4), while the desired curvature-only covector
is(0,p/2). The missing derivative-of-coefficient term cannot be discarded.

## Phase597 controls

On the flat14-torus set u=theta0 gamma2 cos(x0),
v=theta1 Gamma12 sin(x0), with normalized periodic averages1/2 for
sine-square and cosine-square. For matched Phi1=a(1+h Omega)gamma,
the literal CCA contraction predicts <u,Kdv>=-a, <v,Kdu>=0.
The actual quadratic action on T=s u+t v is
`-a st/2 + kappa(-s^2+t^2)/4`, with Gram diag(-1/2,+1/2).
The symmetric covector Hessian is
`[[-kappa/2,-a/2],[-a/2,kappa/2]]`; raw KD alone is not that Hessian.

Let alpha=gamma2 sin(x0), epsilon=exp(r alpha). Since alpha commutes
with its derivative, B=epsilon^-1 d epsilon=r u exactly. Both dB and
B wedge B vanish, so no hidden background-curvature quadratic term exists.
The exact torsion is T=(s-r)u+t v. Changes in K_epsilon and the connection
inside D_B enter this flat-background action only at cubic or higher
amplitude order. Its joint quadratic Hessian is J^T H J for
`J=[[1,0,-1],[0,1,0]]`, with null vector(1,0,1).
The fixed-epsilon u direction is not null. This is a coordinate-pullback
control, not proof of full nonlinear covariance or physical gauge selection.

A noncommuting two-generator jet makes the flatness sign test nonvacuous:
epsilon=exp(r f X)exp(r g Y), X=gamma2,Y=Gamma12,
f=sin(x0),g=sin(x1). At order r^2,
`dB=-df wedge dg [X,Y]` and `B wedge B=+df wedge dg [X,Y]`.
Both are nonzero and cancel. A wrong relative sign must fail.

Printed source tuple signs and subsequent algebra are not fully consistent;
the declared coordinates do not adjudicate that historical discrepancy.
An indefinite Hessian is not itself a tachyon or stability diagnosis.
Propagation requires the full retained field/constraint system and an
evolution choice; a two-amplitude Hessian is not a physical mass spectrum.

## Executed results and exact lineage

Phase596 passed all matrix, principal/cubic, Fourier and adversarial controls.
Its234 principal rows on144 variables have rank134/nullity10; the40 reduced
rows have rank8/nullity2; the1026 combined rows have rank142/nullity2.
The12-by78 curvature map has rank9/nullity69. Independent fixed-prime
modular ranks match the rational ranks, with separate primality, denominator
and known-answer controls. The subgroup test checks15,288 three-form signs
with zero survivors and168 positive subgroup signs. The conditional14D
argument is a separately reviewed proof, not a u(128) numerical extrapolation.
All11 bindings and the726-file core manifest pass. Frozen contract SHA256:
`9a82952d1967e0eeb400bd46ade16dbf0400b5770eca678eb61a728637310639`.
Identical full/summary SHA256:
`dbb522e4cd869a7de1905b8064ab1d96dfceb5e029d4884059fff0ee6963245b`.

Phase597 passed all8 full-chain slots,12 mass/joint rows,32 adjoint checks,
8 tensor/connection jets,56 grade controls,24 pointwise stationarity anchors
and both flatness controls. The correct joint null direction passes; all12
wrong-lift and fixed-epsilon-null decoys are rejected. Nonzero mixed
variations distinguish a nonstationary null-pairing force from a zero force.
The pointwise curvature anchors are distinct from the Fourier flat torus.
All15 bindings and the live726-file manifest pass. Frozen contract SHA256:
`3bb6423f17a2bea308a437be16542ab94942149f60d80fab66041ad92aaf0b34`.
Identical full/summary SHA256:
`825feb58360b8a17817cfafd2bf323112c00fc2f8d51085dd8e080b59fb72cf6`.

No failed scientific execution or frozen rewrite occurred. The unavailable
external timing wrapper before596 never launched the study; shell timing
was used for its first execution. Release builds have zero warnings/errors.

## Further constructive lead: full continuum action descent

While596-597 were being implemented, coordinator and independent review
derived a stronger conditional identity. Define S=epsilon T epsilon^-1,
hold metric/Hodge, the reference connection, unrotated Phi tensors, coupling
coefficients and density fixed, and use the displayed uniform coefficient
conjugation. Then

`D_B T = Ad(epsilon^-1) D_0 S`,
`F_B = Ad(epsilon^-1) F_0`,
`q(T,T) = Ad(epsilon^-1) q(S,S)`.

Trace invariance and K_epsilon=Ad(epsilon^-1) K_0 Ad(epsilon) give the
entire fixed-metric bosonic action, not merely its quadratic truncation,

`I(epsilon,omega) = <S,K_0(F_0 + D_0 S/2 + gamma q(S,S)/3)>`
`+ kappa <S,S>/2`.

This uses the consistent explicitly declared plus-sign lift. It neither
settles the printed transformation discrepancy nor proves that a registered
finite discretization realizes this identity. Phase583's quotient kinematics
and Phase589's rejection of one different finite joint completion remain
unchanged. Density/measure selection also does not follow from action descent.
This analytically reviewed lead could support a new frozen nonlinear
action/variation identity test after596-597; it is not allocated or executed.

The independently checked infinitesimal form is also constructive. For the
right-trivialized variation alpha=epsilon^-1 delta epsilon,
`delta S = Ad(epsilon)(delta omega - D_A alpha)`, A=B+T.
Let g=Ad(epsilon^-1) grad_S I_0 be the ACTUAL action gradient. With fixed
nondegenerate pairings and periodic or compact-supported variations,
`delta I = <g,delta omega>_1 + <-D_A^dagger g,alpha>_0`.
Thus the epsilon equation is redundant once the actual omega equation
vanishes, and delta omega=D_A alpha gives off-shell cancellation. This
recovers a conditional redundancy mechanism without the rejected K F_A
shortcut. Converting to upper-form wedge conventions requires explicit
sign choices; it is not a ruling on the printed Eq9.6 convention.

A bounded exact implementation lead is now independently reviewed. Let
N=gamma2+Gamma12 and M=gamma2-Gamma12. Then N^2=M^2=0,
NM=2(1+gamma1), MN=2(1-gamma1), and both are H-anti-Hermitian.
For real f,g, epsilon=(1+fN)(1+gM) is exactly H-unitary with polynomial
inverse(1-gM)(1-fN). No exponential truncation or sampled matrix angles
are needed. With f=sin(x0),g=sin(x1), its exact flat connection is
`B=(N+4g gamma1-4g^2 M)df+M dg`.
Two separate base fields give nonvacuous action controls: the constant
Phase594 first carrier at unit amplitudes has cubic action4a/3; Phase597's
u+v has quadratic action-a/2 and zero averaged cubic term. Compare full
transformed tensor/action components and actual variations, with a finite
nilpotent right-orbit Ward control. Freeze a proven nonzero decoy and bounded
resources before any execution. This successor remains unallocated;598+ free.

All fourteen authority flags remain false; O4 pending, Phase561 closed,
externalReviewPending=true, promotedPhysicalMassClaimCount=0. No source
model, kinetic normalization, dimensional reduction or units are selected.
