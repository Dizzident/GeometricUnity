# Phase597: actual quadratic action and joint-coordinate null audit

## Prospective scope and authority

Amendment A51 allocates this exact, zero-sampling study. The primary source is
the locally bound original draft, §§6–9, especially Eqs6.20,7.3,7.4,9.3,9.4
and9.7. Phases592 and594 supply the independently passed companion-family
and action-gradient lineage. This study does not select the author's tensor
coefficients, pairing, normalization, transformation signs or physical fields.
The printed initial tuple signs in6.20/7.4 differ from signs used in their
following calculations. Nothing here resolves that discrepancy.

The target is a declared continuum control on a flat fourteen-torus with
signature(7,7), periodic coordinates x0,x1, normalized integral and fixed
metric/trace pairing. All other coordinates are spectators. This is not a
physical compactification choice. The computation certifies a finite action
pullback, not a full ambient field Hessian or physical gauge quotient.

## Algebra, types and exact arithmetic

Use gamma_a gamma_b+gamma_b gamma_a=2 eta_ab, with seven positive axes
followed by seven negative axes. Omega=gamma0...gamma13 has square+1,
anticommutes with odd blades and commutes with even blades. With the reviewed
H real form, grades1,2,13 have real H-anti-Hermitian coefficients; grade12
requires imaginary coefficients. The four rows are a=±1,h=±1, with

    Phi1=a(1+h Omega) sum_a theta_a gamma_a,
    Phi2=(c-i h Omega) sum_(a<b) theta_ab gamma_a gamma_b.

The scalar c is formal: compute its constant and linear coefficients
separately, with no interpolation, sampled c values or coefficient search.
Phi2 occurs once, hence these two coefficients exhaust c dependence.

On theta_I, star is shuffle(I,Ic) times the product of metric signs in I,
followed by complementation. Consequently star squared on degree r is
(-1)^(r(14-r)+7); inverse star1 equals star13. The literal tied CCA chain is

    U(F)=[Phi1 wedge star F]_C
         -(1/2) star[Phi1 wedge star[Phi2 wedge star F]_A]_C,
    K(F)=inverse star1 U(F),

where C is the coefficient commutator and A is i times its anticommutator.
The respective intermediate form degrees are2,12,13; the inner A has
degree14, its star degree0, the outer C degree1 and its star degree13.
Every executed chain checks these types. The tied occurrence choices are a
declared conservative candidate family, not an assertion about author intent.

The pairing is the declared -ReTr/128 with signed exterior metric. Equivalently
pair one-forms through their wedge with star. It is nondegenerate on the
chosen coefficient spaces but indefinite, not a positive norm.

FourierTensor.cs stores exact finite Laurent polynomials in exp(i*x0) and
exp(i*x1), with rational complex coefficients and form/Clifford masks.
Derivatives multiply by i*k; integration selects total frequency zero.
Clifford products use the independently bound592 helper. Its word-insertion
oracle is checked on256 selected pairs, and star-square on all16384 masks.
Independent trigonometric identities, derivatives, d squared and quadratic
polynomial differentiation are known-answer controls. The594 polynomial helper
is separately bound and compiled directly, as is the592 algebra helper.

## Correct fixed-background variation

Let B1,B2 denote the fixed nondegenerate pairings on degrees1,2; K may vary
spatially but is independent of T. Let D_B be the background covariant exterior
derivative, q(T,T)=T wedge T and Q_T(V)=(T wedge V+V wedge T)/2. The declared
action with cubic convention parameter gamma is

    I(T)=B1(T,K F_B)+B1(T,K D_B T)/2
          +gamma B1(T,K q(T,T))/3+kappa B1(T,T)/2.

Direct variation, using defining adjoints rather than a curvature shortcut,
gives

    G(T)=K F_B+(K D_B T+D_B^dagger(K^dagger T))/2
          +gamma(K q(T,T)+2 Q_T^dagger(K^dagger T))/3+kappa T.

At zero torsion, G(0)=K F_B and

    H0 V=(K D_B V+D_B^dagger(K^dagger V))/2+kappa V.

Adjoints use periodic integration by parts. In a metric-compatible frame,
(D_B^dagger W)_j=-nabla_B^i W_ij. The derivative acts on the product K^dagger V:
the term involving nabla_B K^dagger cannot generally be discarded. This
study uses constant untransformed tensors and B=0, so that particular
coefficient derivative vanishes. It does not generalize that simplification
to variable coefficients or arbitrary registered code.

## The two-amplitude Fourier carrier

Choose

    u=theta0 gamma2 cos(x0), v=theta1 Gamma12 sin(x0), T=s u+t v,
    w=theta01 Gamma12 cos(x0).

Then du=0, dv=w and d^dagger w=v. Full literal K gives

    K w=2a theta0(1+h Omega)gamma2 cos(x0).

The full inner A vanishes in both formal-c slots; the c coefficient of K w
is zero. This follows also from {Gamma01,Gamma12}=0 and is tested at every
component, before any carrier projection. Gamma and complementary grades
are retained; they are not projected away before evaluating the operator.

The actual integrated Gram is G=diag(-1/2,+1/2), and the curvature Gram is
G_F=1/2. Thus B(u,Kdv)=-a, whereas B(v,Kdu)=0. The raw force covector matrix
is M=[[0,-a],[0,0]], which fails reciprocity for all four operator rows.

The finite K matrix, after full-chain evaluation and Gram projection, is
(2a,0)^T; its adjoint is (-2a,0). The d matrix is (0,1), and its adjoint is
(0,1)^T. These are adjoints of the finite restrictions, not an assertion that
K's ambient adjoint has only one curvature component. Defining pairings and
the independently computed differential adjoint are checked32 times.
Since d maps the carrier into span(w), these finite matrices give the exact
quadratic pullback Hessian. A separate top-form trace assembly checks M.

The true quadratic action and covector Hessian are

    I2=-a*s*t/2+kappa*(-s*s+t*t)/4,
    H=[[-kappa/2,-a/2],[-a/2,kappa/2]].

For each kappa=-1,0,1, exact polynomial differentiation, finite adjoint
construction, and the symmetrized raw matrix agree. The raised Jacobian
G^-1 H is not ordinarily symmetric; it is G-weighted symmetric. This supplies
a second negative control against using the wrong metric in reciprocity.
The c coefficient is tested separately and vanishes; mass belongs to the
constant coefficient, not independently to every formal-c slot.

## Joint lift and flatness

Take alpha=gamma2 sin(x0), epsilon=exp(r alpha) and omega=s u+t v. On this
declared flat reference, B=epsilon^-1 d epsilon=r u exactly, because alpha
commutes with its derivative. Therefore T=omega-B=(s-r)u+t v. Both dB and
B wedge B vanish separately and F_B vanishes to all orders.
The program checks the commuting-generator condition and the explicit
second-order jet. The all-orders assertion follows analytically by
d exp(r alpha)=exp(r alpha) r d alpha, not by extrapolating a finite jet or
claiming that an infinite exponential series was executed.

For J=[[1,0,-1],[0,1,0]], independent polynomial differentiation of the
joint quadratic action equals J^T H J and annihilates (1,0,1). The incorrect
plus-sign lift fails this null test in all12 mass/operator rows. The fixed
epsilon u direction is not null. This is a coordinate pullback identity, not
a statement that a fixed-epsilon torsion variation is a physical gauge mode.
No metric on epsilon-coordinate space or physical mode count is introduced.

For the explicitly declared conjugation Phi_epsilon=epsilon^-1 Phi epsilon,
the first tensor jet is [Phi,alpha]. The code differentiates both tensor
occurrences in K and independently checks

    delta K(F)=K[alpha,F]-[alpha,KF].

This is only the first coefficient identity of this specified construction,
not a full nonlinear source Ward identity. Also
D_B T=dT+r(u wedge T+T wedge u); its v correction is nonzero before
integration. Since T and dT each have total amplitude degree1, either tensor
or connection correction enters B(T,K D_B T)/2 at degree at least3. The
computed first corrections here average to zero by Fourier parity; the
degree argument does not rely on that accidental cancellation. F_B=0
eliminates the potentially dangerous B(T,K F_B) term exactly. Higher tensor
jets enter at degree at least4. Thus these variations do not alter I2.

The one-generator flatness control alone cannot test the Maurer-Cartan
relative sign. A noncommuting independent-coordinate control therefore uses
f=sin(x0), g=sin(x1), X=gamma2, Y=Gamma12 and
epsilon=exp(r fX)exp(r gY). Expand

    E1=fX+gY,
    E2=f^2 X^2/2+fgXY+g^2 Y^2/2,
    epsilon^-1=1-r E1+r^2(E1^2-E2)+O(r^3).

The code independently differentiates these jets and checks the inverse on
both sides. It obtains B1=dE1, B2=dE2-E1 dE1=g df[X,Y], with [X,Y]=-2gamma1.
The two nonzero terms satisfy

    dB2=-df wedge dg[X,Y], B1 wedge B1=df wedge dg[X,Y].

Their sum is zero, whereas the wrong difference is
4 df wedge dg gamma1, nonzero since f,g use different coordinates. This is
an exact second-order jet control, not sampled exponentiation.

## Pairing-independent stationarity controls

For P=a(1+h Omega), a nonzero, and a pure grade1 vector z, Pz has distinct
grade1 and grade13 pieces. Its grade1 projection is az, so Pz=0 implies
z=0 without any positivity or trace-pairing hypothesis. All56 basis rows
check this projection, distinct grades, nonzero image and null self-pairing.
Linearity of the grade projection proves injectivity on the entire grade1
subspace, including metric-null vectors.

For algebraic Riemann input only, use
F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d Gamma_cd. Let
J_b=sum_d sigma_d Ric_bd gamma_d and Ggamma=sum_b theta_b gamma_b.
The matched full force is -P(J-R Ggamma/2). Consequently K F=0 implies
Ric=R eta/2 and tracing in dimension14 yields R=7R, hence R=0 and Ric=0.
This is a fixed-torsion-sector stationarity statement, not stationarity of
every source field or a selected gravitational model.

Test flat curvature, a nonzero Ricci-flat Weyl tensor with
R0101=R2323=1, R0202=R1313=-1, and constant sectional curvature1 with
R_abab=sigma_a sigma_b. The latter has Ric=13 eta, scalar182 and force
78 P Ggamma. The complete literal chain is compared to independent Ricci
contraction in all24 background/operator/formal-c rows. The first two have
zero force; the constant-curvature force is nonzero in its constant slot.
These are separate pointwise algebraic Riemann anchors, not a claim that the
same flat torus/reference connection has nonzero background curvature.
Its self-pairing vanishes, but the allowed mixed variation
theta0(1-h Omega)gamma0/2 pairs to -78a, explicitly nonzero. Thus a null
self-pairing cannot be used to infer zero first variation.

## Frozen predictions, resources and terminal precedence

The complete fixture object in Program.cs is repeated verbatim structurally
in the contract and checked before any calculation. Expected counts are:
256 word pairs,16384 Hodge masks,4 operator rows,8 formal chain slots,
4 raw reciprocity rejections,12 mass rows,12 joint rows,12 wrong-lift
rejections,12 fixed-epsilon non-null rows,8 epsilon-jet rows,32 defining
adjoint checks,56 grade rows,24 stationarity rows and2 flatness controls.
All comparisons use exact rational/complex coefficients with tolerance0.
Prospective CPU estimate3seconds, upper estimate15seconds; memory estimate
64MiB, upper estimate128MiB. These are resource estimates, not scientific
acceptance tolerances or measured results.

Precedence is invalid/drifted input; known-answer failure; full-chain/adjoint
failure; quadratic/joint failure; background-flatness/jet failure;
grade/stationarity/count failure; then
`actual-quadratic-joint-null-controls-pass-dynamics-unselected`.
Any failed first run is retained and investigated before any versioned repair.

All15 unique file bindings are verified, including this proof, the project,
Program, Fourier helper, both external compiled helpers, primary source,
592/594 programs/contracts/passed summaries, the full726-file core manifest
and Directory.Build.props. The live sorted core path set, each file hash and
tree digest are checked, not merely the manifest's own digest. No core files
are changed. Full and summary reports are deterministic identical JSON.

All fourteen authority firewalls remain false, O4 remains pending,
externalReviewPending=true and promotedPhysicalMassClaimCount=0. Neither an
indefinite two-amplitude Hessian nor formal self-adjointness establishes a
dynamics failure, stability, hyperbolicity, dispersion, spectrum or mass.
Those questions require the retained field/constraint system, appropriate
evolution split and physical decomposition. No such source choice is made.
