# Phase598: full continuum action descent and actual Ward audit

## Scope fixed before execution

Amendment A52 authorizes this deterministic continuation of Phases594 and597.
The bound primary original draft, §§6–9 and Eqs6.20,7.3,7.4,9.3,9.4,9.7,
supplies the motivating construction. The following fixed-metric continuum
candidate is declared explicitly; it is not a selection of author intent.
In particular, this uses uniform tensor conjugation and a consistent plus
right lift. It does not resolve the printed tuple-sign discrepancies in
6.20/7.4, nor infer equivalence to the registered discrete action.

The geometry is the flat fourteen-torus with signature(7,7), fixed normalized
periodic density, fixed flat reference, and coordinates x0,x1; all other
coordinates are spectators. No physical compactification, contour, measure,
field decomposition or source normalization is chosen.

The full finite nonconstant epsilon is used to assemble actual transformed
tensors, connection, torsion, curvature, operator and action. The descent
identity is a comparison target, not a replacement for those calculations.
Both nonzero base actions and actual product variations are exercised. This
is not a rebranding of a quadratic or zero-only test.

## Exact algebra and typed operator

Use gamma_a gamma_b+gamma_b gamma_a=2 eta_ab, with seven positive axes then
seven negative axes. Omega=gamma0...gamma13 has square+1 and anticommutes
with odd blades. The H real form obeys gamma_a^dagger H=-H gamma_a. A blade
of degree r has H-adjoint sign(-1)^(r(r+1)/2). Fourier conjugation also
complex-conjugates coefficients and reverses frequencies.

The two rows are h=-1,+1, with fixed a=1, and

    Phi1=(1+h Omega) sum_j theta_j gamma_j,
    Phi2=(c-i h Omega) sum_(a<b) theta_ab Gamma_ab.

The coefficient c is formal. Constant and linear c slots are assembled
separately; there is no sampled c, interpolation or fitting. Phi2 appears
once, so these slots exhaust its dependence. Terms independent of Phi2,
including the action mass term, belong only to the constant slot.

The upper/lowered literal CCA operator is

    U(F)=[Phi1 wedge star F]_C
         -(1/2)star[Phi1 wedge star[Phi2 wedge star F]_A]_C,
    K(F)=inverse star1 U(F).

C is the ordinary coefficient commutator and A=i times the coefficient
anticommutator, followed by the indicated exterior wedge. These are declared
tied occurrence choices, not claimed source-mandated choices. The star on
theta_I is shuffle(I,Ic) times the product of metric signs in I, followed by
complementation. Star squared is(-1)^(r(14-r)+7), and inverse star1=star13.
All intermediate form degrees2,12,13,14,0,1,13,1 are checked in both ordinary
and dual-number chains. No one-form is added to a two-form.

The fixed pairing is -ReTr/128 with signed exterior metric. It is indefinite.
Matrix trace invariance, not positive-definiteness, supports descent. The
real-form condition keeps the declared torsion/connection in their carrier;
nonzero H-anti-Hermitian nilpotents are permitted because H is indefinite.

ExactArithmetic.cs repeats the bound592 Clifford conventions with independent
word-insertion checks, but replaces fixed-width rational integers by
BigInteger numerators/denominators. FourierTensor.cs represents finite Laurent
polynomials in exp(i*x0),exp(i*x1). No angles, quadrature, exponential
truncations or numerical derivatives occur. Derivatives multiply by i*k and
integrals select total frequency zero. Products group by form degree/mask and
prune exact zero wedge or coefficient brackets; they never use covariance to
supply an answer. An independent naive kernel uses word multiplication and
checks all192 products of the frozen eight-tensor menu with W,C,A.
Known answers also include256 Clifford products, all16384 Hodge-square cases,
exact2^100 rational reduction, derivative identities, d squared and
mean(cos^2(x0)sin^2(x0))=1/8.

## Finite noncommuting H-unitary epsilon

Let N=gamma2+Gamma12 and M=gamma2-Gamma12. Since gamma2 squared=1,
Gamma12 squared=-1 and they anticommute,

    N^2=M^2=0, NM=2(1+gamma1), MN=2(1-gamma1),
    [N,M]=4gamma1, MNM=4M.

N and M are H-anti-Hermitian. For real f, (1+fN)^dagger H(1+fN)=H, because
the linear terms cancel and N^dagger HN=-HN^2=0. The same holds for M.
For f=sin(x0), g=sin(x1), set

    epsilon=(1+fN)(1+gM), inverse=(1-gM)(1-fN).

These are exact finite expressions, not exponential approximations. Both
inverse orders, both H-unitarity orders and all six nilpotent identities are
computed directly. Then assemble B=inverse*d epsilon by Fourier products.
An independent formula predicts

    B=(N+4g gamma1-4g^2 M)df+Mdg.

This follows from (1-gM)N(1+gM)=N+g[N,M]-g^2 MNM. The program separately
computes dB and B wedge B and requires both to be nonzero and their sum zero.
Thus the flatness sign control is nonvacuous.

## Full action descent and its assumptions

Fix all reference, metric, density, tensor coefficients and couplings. Write
Ad_epsilon(X)=epsilon X inverse, and define

    T=inverse S epsilon, omega=B+T,
    Phi_epsilon=inverse Phi0 epsilon.

Uniform conjugation commutes with the fixed star and respects coefficient
products, C and A. Thus, analytically,

    D_B T=inverse(dS)epsilon,
    T wedge T=inverse(S wedge S)epsilon,
    K_epsilon(inverse F epsilon)=inverse K0(F)epsilon.

F_B=0 here. For a nonflat fixed reference the analogous background curvature
would transform as well, but no such fixture is executed. Cyclic trace and
the unchanged exterior pairing give termwise descent of

    I(epsilon,omega)=<T,K_epsilon F_B>
      +<T,K_epsilon D_B T>/2
      +gamma<T,K_epsilon(T wedge T)>/3+kappa<T,T>/2

to I0(S). The parameter gamma=1 or2 records the two declared bracket
encodings with Q=T wedge T fixed. It does not repair or assume the rejected
curvature-only variation shortcut. Kappa=0,1 is independently controlled.

For each fixture, chirality and formal-c slot, the program computes both
transformed tensors by actual products, then the literal chain on actual
D_B T and T squared. It compares their complete output tensors to independently
conjugated base outputs, not merely integrated traces. Separate ordinary and
dual-number chain evaluations agree. Every action piece is compared to the
base piece and its analytically predicted value.
The original and transformed tensors are checked H-anti-Hermitian;24 independent
top-form product traces check the integrated quadratic, cubic and mass pairings.

The two fixed base fields and variation directions are

    S_C=theta0 Gamma01+theta1 Gamma12+theta1 gamma2,
    V_C=theta1 gamma2;
    S_Q=u+v, u=theta0 gamma2 cos(x0), v=theta1 Gamma12 sin(x0),
    V_Q=u.

In order(background, quadratic-with-half, raw-cubic, mass-with-half), the
constant-slot action pieces are(0,0,4,1/2) and(0,-1/2,0,0). Their full
actions are4gamma/3+kappa/2 and-1/2, hence nonzero throughout the frozen
coupling menu. The cubic integral for S_Q vanishes because three frequencies
each equal to±1 in x0 cannot sum to zero, not because its pointwise
curvature/operator is zero. For S_C, the nonzero cubic follows from
Q01=2Gamma02 and K(Q)_1=-4(1+h Omega)gamma2.

The corresponding directional pieces are(0,0,4,-1) and
(0,-1/2,0,-1/2): derivatives4gamma/3-kappa and-1/2-kappa/2. At nontrivial
epsilon the fixed-epsilon coordinate direction is explicitly
delta omega=inverse V epsilon, not the untransformed V. The c-slot pieces
vanish for these fixtures; each is checked coefficientwise, not omitted.

## Actual variations and Ward redundancy

All infinitesimal calculations use exact first-order dual numbers. Product
rules act on epsilon, its inverse, B, T, both independently constructed
tensors, D_B T, F_B, T squared and every action pairing. Both inverse product
derivatives are checked to be zero. No desired force K F_A is used as a
gradient. Base derivatives are independently assembled as

    delta I0= (<V,K dS>+<S,K dV>)/2
      +gamma(<V,K Q>+<S,K(V wedge S+S wedge V)>)/3
      +kappa<S,V>.

For right-trivialized delta epsilon=epsilon eta, direct differentiation gives
delta B=D_B eta and

    delta S=Ad_epsilon(delta omega-D_A eta), A=B+T=omega.

Indeed delta T=delta omega-D_B eta and differentiating its conjugation adds
[eta,T]=-[T,eta]. The actual fixed-omega gradient g is the conjugated
action-derived base gradient, not the shortcut K F_A. With periodic
integration by parts the epsilon derivative is -D_A^dagger g, so the actual
Ward redundancy follows. This is conditional on the explicitly declared
fixed metric/reference/tensor construction and plus lift.

The nonconstant generator eta=N sin(x0) is used in two full variations at
the noncommuting epsilon. The Ward tangent has delta omega=D_A eta and must
give delta S=0 and zero derivative of every action piece. The epsilon-only
tangent has delta omega=0; its directly computed derivative is compared to
the independently assembled base derivative in direction
-Ad_epsilon(D_A eta). This is a full nonlinear-background variation, not a
zero-torsion or two-amplitude Hessian identity. Both epsilon and torsion
variations are checked nonzero where required.

As a supplementary finite-orbit control, g_rho=1+rho eta has inverse
1-rho eta. The program computes epsilon_rho=epsilon*g_rho and
omega_rho=g_rho^-1 omega g_rho+g_rho^-1 d g_rho as exact coefficient arrays.
Both inverse orders, H-unitarity and S_rho=epsilon_rho T_rho epsilon_rho^-1
are checked coefficientwise through degree4. Nonconstant omega coefficients
must be present while every nonconstant S coefficient vanishes. This is
explicitly a finite coordinate-orbit control; it is not advertised as a
separately executed full action polynomial in rho. The preceding literal
finite-epsilon action and actual-variation calculations are independent of
this reconstruction test.

## Two independently predicted nonzero decoys

At epsilon=I, kappa=0, S=lambda(u+v), use alpha=gamma2 sin(x0). The formal
lambda coefficients are computed by polynomial convolution, never sampled
or fitted. Here D_S alpha=u+2lambda theta1 gamma1 sin^2(x0).

The full matched CCA map on F=theta01 gamma1 has first contribution
2h theta0 Omega-2theta1 Gamma01. Its inner-lowered A is-2h Omega gamma0;
the second term cancels both in-plane components, leaving exactly

    K(theta01 gamma1)=2 sum_(j=2..13)theta_j Gamma0j.

This complete output is checked independently before the decoy conclusions.
It is independent of c and h. In particular, there is no overlooked
in-plane grade1 contribution to the following pairings.

Wrong lift: delta omega=-D_S alpha, delta epsilon=alpha. The actual
coordinate variation has delta T=-2u-2lambda theta1 gamma1 sin^2, while
the pulled-back variation is delta S=-2u-4lambda theta1 gamma1 sin^2.
The program uses the former in direct product variation, including delta B
and delta Phi, and the latter in an independent base derivative.
The quadratic derivative is lambda. The extra delta Q in the base derivative
is8lambda^2 theta01 Gamma12 cos sin^2; its K output pairs to
-16lambda^3 mean(cos^2 sin^2)=-2lambda^3. The other cubic pairing is zero by
the full out-of-plane support above. Thus

    wrong-lift residual=lambda-2gamma lambda^3/3.

Correct plus lift but frozen tensors: delta T=2lambda theta1 gamma1 sin^2,
delta(D_B T)=2lambda theta01 gamma1 sin cos, and delta Q is
-4lambda^2 theta01 Gamma12 cos sin^2. Quadratic pairings vanish; the cubic
pairing is+lambda^3. Hence the residual is gamma lambda^3/3.
The actual tensor variation contributes -gamma lambda^3/3 and restores zero.

All these terms, not just the final sums, are computed from full products.
At lambda1 and gamma1,2 the wrong-lift values are+1/3,-1/3; frozen-tensor
values are+1/3,+2/3. Thus the decoys are genuinely nonzero for both encodings
and chiralities. In the c coefficient all these functionals vanish, which is
separately tested. Kappa is fixed to zero in these decoys: a wrong-lift mass
term would otherwise add kappa*lambda and change the prediction.

## Frozen counts, resources, precedence and boundaries

The complete fixture JSON is present in Program and the contract and checked
for structural equality before any scientific operation. Counts are256 word
cases,16384 Hodge cases,192 kernel-parity cases,6 nilpotent identities,
2 finite-orbit rows,8 fixture/chirality/formal-c contexts,32 action rows,
32 fixed-direction rows,32 Ward rows,32 epsilon-only rows,16 complete operator
descent comparisons,24 top-form trace comparisons,8 decoy rows,4 nonzero wrong-lift rejections,
4 nonzero frozen-tensor rejections and4 nonzero base-action contexts.

All arithmetic comparisons have tolerance0. Prospective CPU estimate60seconds,
upper estimate300seconds; peak memory estimate128MiB, upper estimate512MiB.
These are unmeasured resource estimates, not empirical science results.
Final epsilon and inverse have at most17 terms. Conjugation by them mixes
each blade only within a four-blade coset generated by gamma1,gamma2 and
has frequency support[-2,2]^2. Therefore transformed S_C has at most200 terms,
S_Q at most280, Phi1 at most2800, and either Phi2 slot at most18200. These
conservative support bounds are checked. Dynamic product counts and largest
temporary dictionaries are diagnostic evidence, not fitted acceptance targets.

Terminal precedence: invalid/drifted input; known-answer failure;
nilpotent/background/orbit/support failure; full action-descent failure;
actual variation/Ward failure; nonzero-decoy/count failure; then
`continuum-action-descent-ward-controls-pass-source-choice-open`.
Any failed first run and its frozen pack must be retained; repairs require
separate versioning and review rather than silent changes.

All16 unique file bindings are frozen: Program, both local helpers, project,
this proof, primary source,597 summary/contract/program/Fourier helper,
594 summary/contract/program,592 algebra helper, full core manifest and
Directory.Build.props. The live sorted726-file core set, every hash and its
tree digest are checked. Full and summary JSON are deterministic identical.
No core or historical frozen files are changed.

All fourteen authority flags remain false, O4 remains pending, external review
remains pending and promotedPhysicalMassClaimCount=0. Action descent and the
actual Ward redundancy do not select a measure, source signs or coefficients,
nor establish registered-action equivalence, hyperbolicity, physical mode
extraction, pole masses, unit normalization or any physical prediction.
