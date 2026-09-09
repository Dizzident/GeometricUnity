# Full trace adjoint and closed Hessian carriers

## Status and source

Amendment A54, Phases600-601: both first frozen Release executions passed
after complete independent and coordinator code/proof/hash review. The
predictions below were fixed before execution; the separate next leads
remain analytical and unexecuted. Primary source: author draft,
April1,2021, equations8.1,8.7,9.3-9.12, local
`texts/GU-DRAFT-2021-TEXT.txt`, SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Author PDF: <https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf>.
This note derives conditional mathematical tests from the locally available
source and Phases592,594,597-599, not a new external physics authority.

## Actual gradient, not the curvature-only shortcut

Fix flat signature(7,7), reference connection, density and P=1+h Omega,
h=+1/-1, and use the matched CCA source chain with Phi2=(c-i h Omega)Gamma2.
The declared pairing B=-ReTr(XY)/128 includes the signed exterior metric.
It is bilinear over real field coefficients, not a positive Hermitian norm.
For periodic fields or compact-supported variations, the literal source
chain K has a trace/exterior transpose Kdag. The quadratic operator is
H0=(Kd+d-dagger Kdag)/2. For Q=S wedge S, its actual nonlinear gradient is

`G=K F0+H0 S+(gamma/3)[K Q+(DQ_S)-dagger Kdag S]+kappa S`.

Here DQ_S[V]=S wedge V+V wedge S is the FULL derivative. An extra factor2
would be an error. The commutator transpose changes sign; the transpose of
i times the anticommutator does not conjugate i. The index7 Hodge transpose
is star_p-dagger=(-1)^(p(14-p))star_(14-p)=-star_p-inverse. These signs require
independent nonzero known answers and forward-pairing checks on complete
structurally possible support, not only agreement between two reverse chains.

On S=theta2 E1 sin(x0), E1=-(Gamma01+Gamma23)/4, Q=0 but its derivative
is not zero. The full quadratic gradient is

`P[-theta0 gamma3/4+theta2 gamma1/2-theta3 gamma0/4
 -(ic/4)sum_(j!=0,3)theta_j Gamma0j gamma3]cos(x0)`.

The nonlinear correction before gamma/3 is

`P[(theta0 gamma0+theta1 gamma1)/4
 +(ic/4)(theta0 Gamma123-theta1 Gamma023)]sin^2(x0)`.

The second forward leg annihilates dS, but its adjoint leg does NOT vanish;
discarding it would omit both a c-independent and a c-dependent term.
The derivative along theta0 gamma3 cos(x0) is1/8. The normalized integrated
trace square is kappa^2/16. The separate positive coefficient diagnostic is
3/8+gamma^2/96+c^2(3/4+gamma^2/96)+kappa^2/16. At kappa0 the actual gradient
is therefore nonzero but null. Under the standard13-form metric, raising
with Hodge flips the nonzero mass-control trace sign. Neither pairing is
silently selected as the incomplete norm notation in source9.11.

The actual registered SD2/id0 half residual uses the different positive
core metric and has periodic half-square1/32, with identity control1/4.
An accidental kappa1 trace half-square equality is not a source parameter
fit or an action bridge. Replacing the separately defined source residual
by the corrected first-action gradient would itself change the second action.

## Why closing the carrier matters

Let E and O be the even/odd Clifford parts of the full H-anti real carrier.
Parity preserves the real carrier and B; E and O are orthogonal and each
restriction is nondegenerate. The full H0 flips parity and sends E into
L=P*Cl_odd. For odd X,Y, (PX)(PY)=0, so L is isotropic. Formal
self-adjointness gives B(E,H0 L)=B(H0 E,L)=0; since H0 L is even,
nondegeneracy implies H0 L=0. Hence H0^2 E=0 and H0^3=0 on the full
unconstrained compatible-domain carrier. This is a conditional operator
proof; a small exact matrix would not by itself establish it.

For u=theta0 gamma2 cos(x0), the predicted e=H0u is
`[sum_(j!=0,2)theta_j gamma2 gamma_j+ic theta2 I]sin(x0)`.
Its norm g=(12+c^2)/2 is strictly positive for real c. With f=H0e,
self-adjointness gives B(u,f)=g, so the third Jordan step is nontrivial.
The closed span{u,e,f} has Gram[[-1/2,0,g],[0,g,0],[g,0,0]], determinant-g^3.
The executable must compute f and H0f by the full source chain, not insert
the desired Jordan matrix. This carrier depends on c; coefficientwise
closure under each c-slot operator is a different assertion.

At diagnostic c0, include v=theta1 Gamma12 sin(x0) and
w=P(theta0 gamma2+theta2 gamma0)cos(x0). The full closed span{u,v,e,w}
obeys Hu=e, Hv=w, He=-12w, Hw=0, with Jordan blocks3 and1. All its
eigenvalues vanish. Yet its B-orthogonal compression to{u,v} is
[[0,1],[-1,0]], whose eigenvalues are plus/minus i. This agrees with597's
restricted action: the earlier derivative is not wrong. What fails is
interpreting a NON-INVARIANT compression as the full operator spectrum.
Leakage e+v and w-u has nonzero trace norms11/2 and1/2 respectively.

## How it is used and what remains open

600 passed the exact full adjoint, gradient, norm and actual core controls.
Its2912 full-adjoint and336 DQ forward-pairing checks include368 and32
nonzero cases. All16 full-gradient rows and8 formal norm rows passed,
including the nonzero omitted adjoint leg and actual core half-squares.
601 passed full-source cyclic closure, Gram and leakage controls:40 operator
slot applications,18 formal cyclic and32 four-carrier independent action
bilinears. The full closed matrices have ranks2,1,0, while their two-mode
compression retains characteristic t^2+1 and the exact nonzero leakage.
All predictions, code/helpers, resource estimates, precedence and input
bindings were frozen before complete independent/coordinator reviews.

600 first-run wall0.839seconds; contract
f288d88fdcda02b32605322fceb84600301e46de655aee6ce969d47b2b543357,
identical full/summaryfc907ef3c371ab40fb7db375b7c4f6ff80eed5be08bfcd80ee2d98df8d408b7e.
601 first-run wall0.549seconds; contract
1eaf3887a4720723d9793fc9a7d66507feee7136969c68ee35ff3ec32d9cd83f,
identical full/summary294e2b9cfd4a7ed4eeb20f3b5820e8ccaebef0c08d22fb847f20be215b0cfc4c.
No frozen failure, post-execution repair or historical rewrite occurred.

This could remove a false spectral lead without invalidating the restricted
action calculation. Conversely it cannot settle another pairing, a physical
field reduction, a nonflat background, nonlinear vacuum dynamics, boundary
conditions outside the proof, or the separately defined squared-residual
action. In particular the latter uses (Dr)-dagger Dr at a zero-residual
background, not the first action's H0; its dynamics need a separate test.
No physical time, pole, scale or unit normalization has been selected.
All fourteen authority flags remain false, O4 and external review pending,
Phase561 closed, source deficits15/14 and promoted physical mass claims0.

## Independently reviewed next lead, not allocated or executed

The source9.11 squared-residual action needs its own Hessian test. At the
flat zero-residual origin, kappa0, define A=Kd. The half-square action has
raised Hessian T=A-dagger A, not H0. The same fixed-geometry parity argument
gives A(E) and A-dagger(E) in L, hence A(L)=A-dagger(L)=0. Thus T kills
E, sends O into L, and T^2=0. This statement concerns the DECLARED real
trace pairing; it does not decide the source's unspecified norm.

There is a nonzero prospective control, avoiding a vacuous null-only test:
z=theta1 gamma1 sin(x0) gives
A z=2 sum_(j=2..13)theta_j Gamma0j cos(x0), independently controlled in598.
Its trace square is24. Direct transposition predicts

`T z=-48 P theta1 gamma1 sin(x0)
     -44 P sum_(j=2..13)theta_j gamma_j sin(x0)`.

This output is nonzero and null, with B(z,Tz)=24. The closed two-step
carrier would have Gram[[-1/2,24],[24,0]], determinant-576. Neither this
operator nor the first action's H0 can be identified with a positive-metric
registered Hessian solely by matching a compressed matrix. Freeze an
independent action polarization, full-source iterates and nonzero controls
before any successor run;602+ remains unassigned.

All nilpotency assertions above are for the full CONNECTION variation
carrier at fixed metric/reference. Source9.1 also includes metric variables.
A coupled metric/connection Hessian, a nonflat reference, different physical
variation space, norm or continuation needs separate analysis. Calling the
fixed-background result a full-theory no-go would exceed the evidence.

### Constructive stationary odd-background lead

While600-601 were being implemented, coordinator and independent reviewer
separately derived a concrete way the zero-background conclusion can fail.
At kappa0, the constant odd connection S0=lambda theta0 gamma0 has Q=0
AND (DQ_S0)-dagger K-dagger S0=0 for every real c and both h. The latter
identity is essential: theta0 gamma2 has Q=0 but is not stationary.
For the aligned candidate, K-dagger S0 on theta0j is proportional to
ic Omega Gamma0j, which commutes with gamma0. The other two-form components
do not enter DQ_S0-dagger. Thus the actual first-action gradient vanishes.
This is a family of stationary points, not a selected vacuum or scale.

Initially derived at diagnostic c0, and then independently extended to every
formal real c by a third reviewer, define V_i=theta_i gamma_i and
W_i=theta_i Omega gamma_i, i=1..13. The full constant connection Hessian
of the first action closes on all26 directions. With k=4 gamma lambda/3,

`H V_i=k sum_(j!=i, j=1..13)(3 V_j+h W_j)`;
`H W_i=k sum_(j!=i, j=1..13)(-h V_j+W_j)`.

The internal matrix M=[[3,-h],[h,1]] has (M-2I)^2=0, while the index
matrix is J13-I13. Consequently this CLOSED carrier has real eigenvalues
32 gamma lambda on the uniform pair and -8 gamma lambda/3 on the twelve
traceless pairs, each with a size2 Jordan block for gamma lambda nonzero.
The declared Gram is diag(-I13,+I13), and the raised matrix is self-adjoint
with respect to that indefinite Gram. Pure V_i compression would again be
misleading; retaining W_i is necessary.

These are independently reviewed analytical predictions, not executed
results. They supply a concrete successor candidate after600-601: test actual
stationarity, full nonlinear action polarization, all26 source-chain columns,
closure and nonzero controls before interpreting any spectrum. All c-slot
cancellations require complete checks; lambda is unfixed and neither a physical
time/pole interpretation nor a stable vacuum has been established. These first-action
eigenvalues do not automatically carry over to the source's separate
squared-residual action. No phase number or execution is allocated here.

### Homogeneous stationary family tied to the existing couplings

A second independently checked analytical lead uses the complete two-dimensional
invariant one-form carrier S=a Gamma1+b Omega Gamma1. Put r=a-hb,s=a+hb.
Then Q=2rs Gamma2, KQ=312rs P Gamma1, B(S,S)=-14rs and

`I(S)=-1456 gamma r^2 s-7 kappa r s`.

The independently derived FULL constant gradient is

`G=104 gamma r s P Gamma1+(208 gamma r+kappa)S`.

Its c slot vanishes by the shared-index Clifford identities. For example,
K-dagger S=-24r Gamma2-28ihcr Omega Gamma2; the second term is annihilated
by DQ_S-dagger, and the first gives624rS. This directly verifies full
stationarity instead of relying only on a restricted extremum. Independently,
local Spin equivariance and595's complete invariant one-form classification
put the constant gradient in this same two-dimensional space, whose Gram
diag(-14,+14) is nondegenerate. This is local algebra, not a claim about
Spin preserving the torus lattice or vanishing metric variations.

For nonzero gamma and kappa, the only stationary points in this invariant
carrier are the origin and
`S*=-kappa(1-h Omega)Gamma1/(416 gamma)`.
For kappa0 and gamma nonzero the r0 family is stationary. The nonzero
branch is null and Q=0, yet the cubic adjoint variation cancels kappa S*.
The rejected curvature-only shortcut would miss this branch. Its invariant
two-mode Hessian is -kappa I2, versus +kappa I2 at the origin; complete
invariance/closure must be retained, not assumed for arbitrary truncations.

This candidate ties amplitude to EXISTING symbolic couplings; it does not
fix their physical values, units, a stable vacuum or a particle mass. Freeze
all formal parameter/branch degeneracies, the full gradient and nonzero
controls before execution. Neither this lead nor the26D stationary-background
lead has a phase allocated yet. Both remain next-step research, not evidence
that any physical blocker is solved.
