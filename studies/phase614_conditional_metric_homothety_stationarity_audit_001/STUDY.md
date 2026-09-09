# Phase614: conditional full-chain metric homothety

Prospective A60 audit, with complete scientific inputs frozen before FIRST
execution. Independent and MAIN review and explicit MAIN first-run approval
are mandatory. The terminal is
`metric-homothety-controls-pass-source-admissibility-open`.

## Independent declared metric family, not a source variation selection

Fix the oriented flat14-dimensional coefficient metric of signature(7,7),
positive axes0..6, negative axes7..13, epsilon=I and flat reference B=0.
All fields tested here are constant. Hold gamma and kappa fixed and include
only the declared first action: curvature source, half derivative term,
gamma/3 cubic term and kappa/2 mass term. Curvature and derivative terms
vanish on this control because FB=0 and dT=0, not because they were omitted
from the physical source. No additional metric action or coupling variation
is inserted to change the conclusion.

Take the independent homothety g_lambda=lambda squared g, lambda positive,
with Gamma1_lambda=lambda Gamma1 and Gamma2_lambda=lambda squared Gamma2.
The Clifford matrices remain fixed in this orthonormal spin identification;
the coordinate coframe coefficients vary. On fixed coordinate r-forms,

    star_r,lambda=lambda^(14-2r) star_r,
    Pair_r,lambda=lambda^(-2r) Pair_r,0,
    volume_lambda=lambda^14 volume_0.

The declared pairing is signed exterior contraction times-ReTr(XY)/128,
not a Hermitian positive norm. The scalar pairing and integrated density
must not be conflated: the latter includes the volume factor exactly once.
The source's MET(X)-induced geometry on Y need not admit this independent
14D homothety. In particular changes of an LC-induced splitting are not
automatically changes of the fixed vertical metric. This audit establishes
NO admissibility of this direction in that source family. It is not a
source-level/global vacuum rejection, norm choice or physical metric equation.

## Complete forward and reverse chains

Use canonical UNTIED CAA, first C and outer/inner A=i anticommutator.
The source8.1/9.3 supplies tools but does not select this occurrence choice.
The literal lowered operator, with every Hodge star rescaled, is

    sf=star2 F, one=C(Gamma1,sf), inner=A(Gamma2,sf),
    zero=star14 inner, outer=A(Gamma1,zero),
    upper=one-star1 outer/2, K F=star13 upper.

For FIXED F the exact lambda exponents of these eight stages, including
the input, are(0,10,11,12,-2,-1,11,-1). Both lowered legs have exponent-1.
This holds for any input coefficient tensor; it is a typed homogeneity proof,
not an inference from one scalar action. The implementation carries exact
Laurent polynomials of FULL Fourier/Clifford tensors through each operation.
No final scaling oracle is substituted inside the chain.

The full real transpose is independently formed by reversing every operation.
For star_r, its adjoint scales as lambda^(2r-14), because the output and
input form metrics scale differently. For wedge by a p-form Phi, its real
C/A transpose has an additional lambda^(-2p) relative to its actual Phi
coefficient. Transposition of A retains i, not its complex conjugate.
The reverse stages(inputY,upper,first,outerOne,zero,top,twelve,second) have
exponents(0,12,1,0,-1,13,11,1) for fixed Y. Therefore

    Kdag_lambda(Y)=lambda Kdag_0(Y),
    DQ_T-dagger,lambda(Y)=lambda^-2 DQ_T-dagger,0(Y)

when T and Y are fixed coordinate tensors. The explicit reverse chain is
also compared with both independent frozen611 transpose routes at lambda1.
Defining full forward/adjoint pairings are checked at all three finite scales.
Transport of a field supplies its own extra lambda; it is not hidden in these
fixed-input operator identities.

## Nonzero full-tensor and wrong-convention controls

Four fixed curvature anchors are Gamma2,theta01 Gamma01/2,
theta01 Gamma23 and theta01 Gamma12. At lambda1 their lowered(first,second)
legs are respectively

    (-26Gamma1,182Gamma1),
    (-theta0 gamma0-theta1 gamma1,Gamma1),
    (0,-2sum(j4..13)theta_j Gamma0123j),
    (2theta0 gamma2,0).

Thus neither the first nor the second leg is tested only on inputs that
annihilate it; the grade5 curvature anchor prevents a vector-only projection.
Two adjoint inputs Gamma1 and theta0 gamma2 have full answers
-24Gamma2 and2theta0 wedge(gamma2 wedgeCl Gamma1), respectively. The
Gamma1 adjoint splits4Gamma2-28Gamma2. Every nonzero coefficient and all
zero stages are retained. In the4-by2 pairing menu, the nonzero unit-scale
pairings are-2184,-12,-2. At general lambda the scalar defining pairing is
lambda^-3 times its base value on either side.

Freezing Phi1/Phi2 while still rescaling stars is a planted wrong convention:
its two lowered legs scale as lambda^-2 and lambda^-4, not lambda^-1.
All four anchors reject it at lambda1/2 and2; their derivatives also reject
it at lambda1 even though the tensor values then coincide. Deleting each
nonzero lowered leg is independently rejected at all three scales. Another
decoy omits the inverse one-form metric factor from the density: it gives
lambda13 cubic and lambda14 mass terms for fixed coordinates. It agrees
at lambda1 but has an incorrect derivative whenever the unit action is
nonzero. This is distinct from the wrong tensor scaling.

## Actual constant-field gradient and two different metric paths

For fixed coordinate T=sGamma1, literal source products give
Q=2s squared Gamma2, KQ=312s squared Gamma1,
Kdag T=-24sGamma2 and DQ_T-dagger Kdag T=624s squared Gamma1 atlambda1.
The full actual cubic gradient contains KQ and ONE full DQ transpose;
there is no extra factor2, since DQ already differentiates both ordered
products. The metric-dependent fixed-coordinate gradient is

    G_lambda=(312gamma s squared/lambda+kappa s)Gamma1.

For transported T_lambda=sGamma1_lambda=s lambda Gamma1, it instead is
lambda(312gamma s squared+kappa s)Gamma1. The program builds both full
gradients from K, reversed Kdag and the metric DQ transpose, not their scalar
coefficients. Full field stationarity on the transported branch persists
along lambda; a fixed coordinate field chosen stationary atlambda1 generally
is NOT field-stationary atlambda1/2 or2.

Let C=-1456gamma s cubed and M=-7kappa s squared. The ORIGINAL action is
computed directly as the top-form trace of T wedge star1(KQ) timesgamma/3
plus T wedge star1(T) timeskappa/2. An independent route uses the signed
one-form pairing and lambda14 volume. Both yield exact polynomials

    I_fixed(lambda)=C lambda11+M lambda12,
    I_transported(lambda)=(C+M)lambda14.

The transported scalar L=C+M is constant; its integrated density is not.
All polynomial coefficients are compared before evaluating or differentiating;
the three rational scales are not a numerical fit. Formal derivatives of
the assembled polynomials are checked against independent explicit powers.

The paths coincide atlambda1. Their derivative difference there is

    I_transported'(1)-I_fixed'(1)=3C+2M
      =-4368gamma s cubed-14kappa s squared=Pair_0(G_1,T).

This moving-field contribution is checked with the independently assembled
FULL gradient. Off stationarity it need not vanish. At other lambda values,
the two paths have different coordinate fields, so no equality of their
partial metric derivatives is presumed. At the nonzero field-stationary
branch kappa=-312gamma s with gamma,s nonzero, both derivatives at1 equal
10192gamma s cubed, nonzero. The unit density there is728gamma s cubed.
This is a conditional metric-stationarity obstruction for the declared action
and direction only, not a selection or rejection of source physical vacua.

The algebraic degeneracies are separate: s0 is field-stationary for all
couplings; if gamma0 and kappa nonzero, only s0 is stationary; if kappa0
and gamma nonzero, again only s0; if both couplings vanish, every s is
stationary and the declared action/metric derivative is zero. No division
by a vanishing gamma, kappa or s occurs in the executable.

## Prospective finite census and resource limits

Use lambda=(1/2,1,2) and16 scalar cases: four nonzero branches with s=+/-1,
gamma1/2,kappa=-312gamma s; four offbranches with s=+/-1,gamma1/2,kappa1;
two zero-field rows with(gamma,kappa)=(1,1),(0,0); two mass-only rows
s1,gamma0,kappa=+/-1; two cubic-only rows s=+/-1,gamma1,kappa0; and two
identically-zero-action rows s=+/-1,gamma0,kappa0. There are48 case/scale
contexts and96 path evaluations. Eight unit-scale cases are field-stationary;
the four nonzero branches have nonzero metric derivatives, while the zero
field and zero-action stationary rows do not.

The complete36-field count fixture binds the28 distinct Hodge/form-metric masks
(prefix masks and their seven-axis cyclic rotations, including empty/full),
84 scaled Hodge and84 scalar metric checks, four full forward anchors and
two reverse anchors,32 forward/16 reverse stage polynomials,24 defining
adjoint pairings with9 nonzero values, and wrong-power/omitted-leg controls.
The scalar menu checks32 path action polynomials and derivatives,128 original
action legs,32 full gradient polynomials with96 source legs,128 intermediate
Q/KQ/Kdag/DQdag anchor polynomials and all96 value/derivative/gradient
evaluations. Sixteen chain-rule rows include8 nonzero offbranch differences;
the wrong density pairing has12 derivative and24 nonunit-scale rejections.

Exact arbitrary-precision Laurent coefficients allow negative intermediate
powers; there is no truncated expansion in lambda. All fields here are
constant, so stored Fourier frequency is zero. At most two exponent slots
are required by the declared actions/gradients and wrong tensor scaling;
the safety ceiling is four. Hodge/action absolute exponents are at most14;
the scalar norm of a fixed14-form additionally uses exponent-28, so the
overall absolute-exponent ceiling is28.
Input source forms have at most91 Clifford/form positions; a cancellation-free
forward estimate is182 first contributions plus91 inner contributions and
1274 outer contributions, below2048. Reversing on14 one-form entries gives
at most182 first and1274 second contributions. Products of the14-term state
and91-term curvature data are bounded finite exact work; a65536 sparse-term
and50million optimized-product ceiling leave generous safety margin.
Planning time is20seconds(maximum estimate120seconds), memory128MiB(maximum
estimate512MiB). These are prospective estimates, not observed measurements.
The optimized Product counter excludes naive products, transpose arithmetic
and total rational work. Mathematical coefficient comparisons have zero tolerance.

Every compiled helper and own scientific file, passed600/611 lineage, primary
source, live726 core manifest and build properties is uniquely hash-bound.
No613 output is required. Full fixture/precedence/firewall closure precedes
science. Failed first outputs are preserved and require versioned repairs.
Full and summary JSON are deterministic and byte-identical. Only the unbound
implementation note may receive results after execution. All14 authority
flags remain false, external review pending and physical mass claims zero.
