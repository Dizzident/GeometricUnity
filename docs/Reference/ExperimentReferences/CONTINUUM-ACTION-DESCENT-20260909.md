# Continuum action descent and nonlinear Ward controls

## Status and provenance

Amendments A52-A53, Phases598-599: both first frozen Release runs passed
after complete independent and coordinator review. The predictions below
were fixed before execution; results and the unexecuted next lead are
distinguished explicitly below. The primary text is the April1,2021 author draft, local
`texts/GU-DRAFT-2021-TEXT.txt`, SHA256
`062d6c473d3fd65f0eb179f169c37d3683ee10e950acccbd9c8ed92bdc3fb817`.
Author PDF: <https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf>.
Equations6.18-6.22,7.3,8.1 and9.3-9.7 motivate the declared construction.
These are conditional deductions, not a ruling on inconsistent printed
tuple signs or on the author's intended completion. Previous exact controls
are Phases592-594 and597, not independent physical observations.

## Full fixed-background descent

Hold the metric/Hodge, reference connection D0, unrotated Phi tensors,
couplings and density fixed. Write B=epsilon^-1 D0 epsilon,
T=omega-epsilon^-1 D0 epsilon and S=epsilon T epsilon^-1. The uniformly
conjugated displayed contraction chain gives
`K_epsilon=Ad(epsilon^-1) K0 Ad(epsilon)`.
Direct product differentiation gives
`D_B T=Ad(epsilon^-1)D0 S`,
`F_B=Ad(epsilon^-1)F0`, and
`q(T,T)=Ad(epsilon^-1)q(S,S)`.
Internal conjugation commutes with wedge/Hodge and preserves the trace
pairing. Therefore the entire declared action descends as

`I(epsilon,omega)=I0(S)`

`I0(S)=<S,K0(F0+D0 S/2+gamma q(S,S)/3)>+kappa<S,S>/2`.

This retains the actual action. It does not replace its derivative by the
curvature-only shortcut rejected by593-594. Nor does it establish descent
of the different registered finite-dimensional action tested in589.

For delta epsilon=epsilon alpha, direct differentiation yields
`delta S=Ad(epsilon)(delta omega-D_A alpha)`, where A=B+T.
Let g be the pullback of the ACTUAL S-gradient through this conjugation,
under fixed nondegenerate pairings. With periodic or compact-supported
variations, the omega Euler derivative is g and the right-trivialized
epsilon derivative is `-D_A^dagger g`. Hence the explicit consistent plus
lift `delta omega=D_A alpha` annihilates the full action off shell.
Form-wedge versus adjoint conventions must be fixed before comparing the
printed source Euler signs. This is redundancy, not a physical gauge choice.

## Exact nonlinear fixtures and nonzero checks

The H-anti-Hermitian generators N=gamma2+Gamma12 and M=gamma2-Gamma12
satisfy N^2=M^2=0, NM=2(1+gamma1), MN=2(1-gamma1), [N,M]=4gamma1.
Set f=sin(x0), g=sin(x1), epsilon=(1+fN)(1+gM). Its exact inverse is
(1-gM)(1-fN); both inverse orders and H-unitarity require direct checks.
Then `B=(N+4g gamma1-4g^2 M)df+Mdg` has zero curvature through a
noncommuting cancellation, without exponential truncation or quadrature.

At a=1 and matched h=+1/-1, the two base fixtures are
`S_C=theta0 Gamma01+theta1 Gamma12+theta1 gamma2` and
`S_Q=u+v`, with u=theta0 gamma2 cos(x0), v=theta1 Gamma12 sin(x0).
Their respective nonzero action anchors at kappa=0 are cubic4gamma/3 and
quadratic-1/2. Construct the transformed tensors and full contraction chain
independently, rather than constructing its result by assuming covariance.
The frozen study must specify all formal c slots, couplings, directions,
counts, resources and tolerance before execution.

Two prospective decoys separate a wrong lift from omitted tensor variation.
At epsilon=1, kappa=0, S=lambda(u+v), alpha=gamma2 sin(x0),
`D_A alpha=u+2lambda theta1 gamma1 sin^2(x0)`.
The full literal chain has
`K(theta01 gamma1)=2a sum_(j=2..13) theta_j Gamma0j`.
Its in-plane terms cancel: none may be retained selectively. Using
mean(cos^2 sin^2)=1/8, the wrong minus lift has actual action derivative
`a lambda-2gamma a lambda^3/3`.
Under the correct plus lift, improperly freezing the Phi/K tensors leaves
`gamma a lambda^3/3`; their proper variation cancels precisely that term.
Both predictions were independently derived before phase allocation.

## Why this does not yet connect the sampler to boson predictions

Phase583's invariant S coordinate retains distinct lambda*S_C right orbits.
On that flat real carrier the first-order completion is
`I0(x,y,z)=(4a gamma/3)xyz+kappa(x^2+y^2-z^2)/2`.
For nonzero cubic coefficient it is unbounded below. A ray by itself has
zero coefficient-Lebesgue measure, so it cannot alone prove divergence.
For positive alpha=4a gamma/3, disjoint boxes x,y in[t,2t], z in[-2t,-t],
t=3^k, have volume t^3 and I0<=-alpha t^3+6|kappa|t^2.
This proves divergence for the declared real finite-dimensional coefficient
integral exp(-I0); a Gaussian reference cannot suppress the cubic either.
Reverse the sign box for negative alpha. This conditional proof does not
specify a continuum measure or contour, impose Euclidean continuation, or
exclude constraints, additional terms, or a different source action.

The registered theta=0 SU(2) action instead squares a residual under a
positive discrete pairing: `S_reg=Upsilon^T M Upsilon/2`.
Core references are EinsteinianShiabOperator.cs:817-841 and
CpuMassMatrix.cs:13-38,134-138; Phase577 uses this registered target.
Along a connection ray it has the form ||lambda L+lambda^2 Q||_M^2/2.
Its nonnegative quartic term is not the first action's standalone cubic.
Odd cross terms mean positivity does not imply exact parity; conversely,
positivity alone does not prove normalizability in the presence of flat
directions. Phase588's smooth classical limit does not prove equality of
these actions or convergence of their quantum measures.

The primary draft separately presents a squared-residual action in9.11.
Its chain rule does not require its residual to equal the first action's
gradient. With the displayed residual r=K_epsilon F_A+kappa T and fixed
epsilon/metric, Dr[V]=K_epsilon D_A V+kappa V. Thus the derivative of
its self-pairing is 2(Dr)^dagger r even if r differs from grad I1.
Replacing r by the corrected first-action gradient would define a different
second action. Failure of the claimed first-action derivative therefore does
not alone invalidate the separately defined residual-square dynamics.
An explicit residual, field-reduction, torsion and pairing dictionary is
still required before identifying it with the registered sampler. Checking
that restricted bridge is the constructive follow-up; none is selected
here. No physical W/Z/H field, pole, scale or unit normalization is supplied.
All fourteen authority flags remain false, O4 pending, Phase561 closed,
external review pending and promoted physical mass claims0.

## A53 / Phase599: a pointwise residual-kernel test

Allocated after independent analytical review; first frozen run passed.
Use the positive four-plane0..3 with Euclidean star4, distinct from star14
in the literal source contraction chain. Set
Sigma=(theta01+theta23,theta02-theta13,theta03+theta12) and
J_i=Gamma(Sigma_i)/2. The brackets are [J_i,J_j]=-2 epsilon_ijk J_k,
so the core's epsilon-normalized basis embeds as E_i=-J_i/2. Source trace
Gram(E_i,E_j)=delta_ij/8; a Lie embedding is not automatically an isometry.

R=Sigma1 tensor Sigma1-Sigma2 tensor Sigma2 is nonzero algebraic Riemann
curvature, with Ricci0 and scalar0. Its distinct-four Bianchi coefficient
is1-1+0=0. The frozen source ordered-pair spin-curvature normalization gives
F=Sigma1 J1-Sigma2 J2=-2Sigma1 E1+2Sigma2 E2. All four tied source
canonical/companion chains vanish on it in every formal coefficient slot.
In contrast the registered SD2/id0 half member (I+star4)/4 yields F/2.
Under the core identity pairing the squared norms are16 and4, respectively;
the pointwise half-square is2. These are diagnostic, dimensionless values,
not a source-pairing choice or a boson observable.

The opposite-duality control uses a separately labeled compact embedding
barSigma=(theta01-theta23,theta02+theta13,theta03-theta12),
barJ=Gamma(barSigma)/2, barE=barJ/2. Both the source chain and registered
selfdual projection kill its nonzero Weyl curvature. An independent full-so4
single-plane R0101=1 control has nonzero source contraction. It lies outside
the strict selfdual-only Bianchi carrier, which is Weyl and has no Ricci
positive control. Zero and identity-contraction controls are also required.
The executable will exercise the actual registered endomorphism API, not
only a retyped selfdual matrix, and independently compute full source chains.

The witness would exclude any zero-preserving reconstruction of the
registered residual from the source residual, and any injective map in the
opposite direction, under this declared input embedding. It cannot exclude
noninjective projection or an altered field/curvature dictionary. Nor does
a pointwise residual mismatch by itself exclude action equality modulo a
boundary term: selfdual curvature squares have topological subtleties.
The local gauge potential A_mu=-sum_nu F_mu,nu x^nu/2 realizes F at x=0,
but neither a globally constant field nor a registered sampled connection
is asserted. T=0 at a point is not T identically0 on a flat background.

## Executed results and exact lineage

Phase598 passed its first frozen Release execution in3.017seconds wall time.
All32 action rows,32 each fixed-direction/Ward/epsilon-only variations,
16 full operator-descent comparisons,24 top-form traces,2 finite coordinate
orbits and8 decoy rows pass. Nonzero epsilon-only variations agree with
separate actual-gradient directional oracles. Four nonzero wrong-lift and
four nonzero frozen-tensor controls reject exactly as predicted. All16 file
bindings and the live726-file core manifest verify. Contract SHA256
`daa0785a3b3aada95c665b5a7234d43d2bea67140d5a4e5651a9e72f8ec20ecd`;
identical full/summary SHA256
`d7d8b0576ec4a15edfb0a3bcfb7cbff900b651c1be326c0805d019c05b191cfb`.

Phase599 passed its first frozen Release execution in0.468seconds wall time.
All96 literal formal source rows pass (8nonzero/88zero), along with153664
Riemann component cases,784 Ricci entries, both compact Lie dictionaries,
108 actual core matrix checks and108 residual entries. The positive Weyl
input has core squared norm16 and registered residual squared norm4, while its source
contraction vanishes. Opposite-duality and nonzero-plane controls hold.
All13 bindings and the live726 core files verify. Contract SHA256
`a3807b93dfc04003baa7a285ac8fdbeb309e68ee437e17e3dcb212ad975bdd17`;
identical full/summary SHA256
`17c29c711e3c5a4cd48e19b4773f4a9b5d6ce76ca84c5842d90832852e71b2ac`.

No failed scientific run or frozen rewrite occurred. Targeted Release
builds have zero warnings/errors; package/checklist/integrity pass379/3,
with the same three standing physical failures and zero physical claims.

## Unexecuted next lead: periodic actual-gradient and norm comparison

This analytical lead is not allocated or scientifically executed. Retain
the same compact embedding E1=-(Gamma01+Gamma23)/4 and set
S=theta2 E1 sin(x0), epsilon=1 on the fixed flat torus, kappa=0. Then Q=0,
F=theta02 E1 cos(x0). The inner A contraction vanishes because Gamma02
shares one index with each bivector in E1. For P=1+h Omega the literal
matched CCA output is P(theta2 gamma1-theta0 gamma3)cos(x0)/2.
It is nonzero but self-pairs to zero under the declared trace/form pairing.

The cancellation generalizes to even Clifford curvature: Phi2 and the
inner contraction are even, so the full K(F) lies in P*Cl_odd.
For odd X,Y, (PX)(PY)=P(1-h Omega)XY=0. This is a matrix-product identity,
not positive definiteness or vanishing of the output. Even T has zero trace
cross-pairing with this odd output. A shortcut residual-square restricted
to even fields therefore retains only its mass term under this pairing.

A SEPARATE adjoint proof applies to the actual first-action gradient.
For even S, cyclic trace transfers a commutator with P gamma into
[S,P gamma]=P[S,gamma], up to the transpose sign. The transpose of the
i-anticommutator under the real bilinear trace pairing retains i, rather
than complex-conjugating it as in a positive Hermitian pairing. Its even
Phi2 coefficients preserve P*Cl_odd. Form/Hodge transposes act on form
indices. Thus K^dagger maps even one-forms into P*Cl_odd two-forms;
derivative and Q_S adjoints preserve this subspace for fixed P and even S.
The action-consistent gradient consequently lies there too at kappa0.
Its trace self-square is null, by this separately derived argument rather
than by substituting the rejected curvature-only gradient.

Nonvanishing has an exact directional witness V=theta0 gamma3 cos(x0).
Here dV=0, <V,KF>=1/4 and the cubic derivative pairs to zero by the complete
K(theta02 gamma2) output support, so delta I1[V]=1/8. The registered core
SD2/id0 half residual-square has normalized periodic average1/32; identity
control1/4. These prospective integrated values cannot be dismissed merely
as nonzero pointwise densities of a periodic total derivative. A complete
independent trace/exterior adjoint implementation and direct directional
checks must still be frozen before the successor experiment.

The norm choice is essential: Eq9.11 does not fully identify the norm used
in its residual square. Upper degree13 standard form metric and lowered
degree1 pullback differ by a Hodge sign in index7; zero remains zero but
nonzero mass terms change sign. A positive coefficient/Hilbert diagnostic
can certify a nonzero output without being a source-admissible replacement
norm. Full Euler equations cannot be inferred from restricting to the even
slice; transverse odd variations may remain nonzero. No physical action,
norm, contour, retained sector or boson spectrum is selected by this lead.
