# Phase613 prospective proof — canonical isotropic potential

This is a prospective exact audit under A60. No scientific execution is
permitted until the complete code, project, proof, fixture and input hashes
receive independent and MAIN review and explicit MAIN authorization.
The reference is flat with constant metric diag(+1 seven times,-1 seven
times), oriented theta0 through theta13. The pairing is the DECLARED real
bilinear B(X,Y)=-ReTr(XY)/128 with the signed exterior metric. It is not
a positive norm or an independently selected interpretation of the source.

Use canonical untied CAA: first commutator, outer i-anticommutator, inner
i-anticommutator, Phi1=Gamma1=sum theta_a gamma_a and
Phi2=Gamma2=sum_(a<b)theta_ab gamma_a gamma_b. Clifford words and exterior
forms are ordered independently. Gamma_a squared=sigma_a, all gamma_a
are H-anti-Hermitian, and real grade5 words are also allowed H-anti fields.
The source is the same §§3/9 text bound in611; no author convention is chosen.

## Full action and trace transpose

For a constant connection field T, define Q(T)=T wedge T and

    I(T)=gamma/3 B(T,KQ(T))+kappa/2 B(T,T),
    G(T)=gamma/3 [KQ(T)+DQ_T^dag K^dag T]+kappa T,
    DQ_T[V]=T wedge V+V wedge T.

This is the actual first-action derivative, not the previously rejected
shortcut KQ+kappa T. K is the literal full Hodge CAA chain from611.
K^dag reverses that chain in the real bilinear pairing. The i in an A
transpose is not complex-conjugated. All full tensors are retained before
pairing. The separately simplified transpose and forward support probes
check the reverse computation independently.

On S=sGamma1, direct Clifford/exterior contraction gives

    Q(S)=2s²Gamma2, KQ=312s²Gamma1,
    K^dag S=-24sGamma2, DQ_S^dag K^dag S=624s²Gamma1.

For the first equality each unordered exterior pair contributes twice.
K on the unit spin plane theta_ab Gamma_ab/2 has the twelve complementary
diagonal vector components with metric contractions cancelling internally;
summing the91 planes at coefficient2 gives4*78=312 per output axis.
The first adjoint of Gamma1 is4Gamma2 and its second adjoint is-28Gamma2.
Also DQ_Gamma1^dag Gamma2=-26Gamma1, since each of thirteen contracted
indices contributes-2. Thus the624 is a full adjoint contraction, not an
inferred derivative of a scalar ansatz.

Consequently

    I(s)=-1456gamma s³-7kappa s²,
    G(s)=(312gamma s²+kappa s)Gamma1.

The action is built independently by literal Q/K products and real trace.
Its derivative is checked against B(Gamma1,G), including every zero
polynomial coefficient. The full tensor gradient has no omitted component.
Homogeneity fixes all dependence: Q and the adjoint composite are quadratic
in s, K^dag S linear, and gamma/kappa enter linearly. Exact coefficient
tests therefore establish the parameter formulas without interpolation.

## Full196-column constant-field Hessian

Represent a vector-valued one-form as V=sum V_ab theta_a gamma_b, let
V^g_ab=sigma_a sigma_b V_ba, and write I for Gamma1. The basis E_ab has
B(E_ab,E_cd)=-sigma_a sigma_b delta_ac delta_bd. This196-dimensional
real Gram is nondegenerate; it is indefinite, not Euclidean.

At S=Gamma1 the three literal derivative legs, independently derived, are

    A(V)=K DQ_S[V]=48[(tr V)I-V],
    Bleg(V)=DQ_V^dag K^dag S=48[(tr V)I-V^g],
    C(V)=DQ_S^dag K^dag V=48[(tr V)I-V].

A follows by expanding the shared-index curvature DQ_I[V]: the first
contraction and scalar inner contraction give48 times the trace projector
minus identity. Its potentially higher-grade inner term vanishes by the
repeated Clifford index. This calculation is LOCAL at the isotropic field;
the corresponding general nonlinear vector-closure claim is false below.
For Bleg, DQ_V^dag Gamma2=-2[(tr V)I-V^g], multiplied by-24.
C is also the transpose of A, whose trace projector and identity are
self-adjoint in B; the executable nevertheless computes the complete
literal C tensor and compares it, rather than inserting this conclusion.

Hence the full constant-field potential Hessian is

    H_S V=kappa V+16gamma s[3(tr V)I-2V-V^g].

Every one of196 actual columns and all three legs are compared as full
Clifford-valued tensors, not recovered from a196-dimensional projection.
The gamma*s coefficient and kappa identity coefficient are independently
checked. For all196² ordered pairs U,V, the original forward action gives
the Hessian bilinear

    gamma*s/3 [B(U,K DQ_I V)+B(V,K DQ_I U)
                +B(I,K DQ_U V)] + kappa B(U,V).

The last term is evaluated through the literal forward K on the grouped
DQ_U V, not any reverse adjoint or Hessian oracle. All ordered entries,
including zeros, must equal B(U,H_S V). Group exchanged cubic words before
real trace pairing. Weighted reciprocity follows and is explicitly checked.
This establishes the full constant-field statement on this carrier, NOT
a differential closure:611 already exhibits derivative mixing with bivectors.

## Independent complete adjoint support

For each V_ab=theta_a gamma_b,

    K^dag_first V_ab=2 theta_a wedge(gamma_b wedgeCl Gamma1),
    A_Gamma1^dag V_ab=2i delta_ab,
    K^dag_second V_ab=-2delta_ab Gamma2.

The first term has13 positions on diagonal inputs,12 off diagonal. On a
diagonal input its13 positions cancel the second term's corresponding
positions, leaving78; off diagonal the full support has12. No other
Clifford grade occurs in the full adjoint: the first C transpose produces
bivectors, and the outer A transpose is a scalar. This does not imply
forward K annihilates higher-grade outputs invisible to vector probes.

Probe every one of91 exterior pairs m for every196 input. The only possible
blade in that slot is m XORbit(a) XORbit(b). Use real phase1 when that
blade's H-adjoint sign is negative, otherwise i. The probe Gram is nonzero.
The coefficient B(KF,V_ab)/B(F,F) reconstructs the full adjoint from an
independent forward evaluation. All impossible support slots must vanish.
Counts:17836 probes,3276 nonzero coefficients (182*12+14*78),2366 first
positions (182*12+14*13),1274 second positions (14*91), and14 nonzero
versus182 zero outer transposes. The whole196-output reconstruction, both
adjoint legs and simplified reverse chain are required.

## Sectors and all coupling degeneracies

An explicit basis is trace I; thirteen diagonal differences E_ii-E_00 and
91 metric-symmetric off-diagonal pairs E_ab+sigma_a sigma_b E_ba; and
91 metric-skew pairs E_ab-sigma_a sigma_b E_ba. The dimensions are1,104,91.
The inverse decomposition of every V is (tr V)I/14,
(V+V^g)/2-(tr V)I/14, and(V-V^g)/2. This proves completeness without
assuming a numerical eigensolver has found every direction. The potential
coefficients are kappa+624gamma s, kappa-48gamma s, kappa-16gamma s.
Every explicit sector vector is checked using the actual full columns.

Stationarity is exactly s(312gamma s+kappa)=0. For gamma!=0 there are
the root0 and root-kappa/(312gamma), coincident when kappa0. For gamma0
and kappa!=0 only0; for gamma=kappa=0 every s. These cases are exhaustive
over real couplings by factorization. The finite rational menu is
gamma in{0,1,2}, kappa in{-1,0,1}, s in{-1,0,1}:27 value rows and nine
coupling classes/origin controls. Four nonzero-branch rows use gamma1/2
and kappa±1. The nonzero branch has full potential sector coefficients
(-kappa,15kappa/13,41kappa/39) and action728gamma s³. Removing the
adjoint term instead gives nonzero shortcut gradient-208gamma s²I there.
No finite menu is used to fit any formula or infer omitted real parameters.

## Minimal nonlinear grade5 negative control

Let T=x theta0 gamma2+y theta1 gamma3 and W=sum_(j=4..13)
theta_j Gamma0123j. Literal stages give

    Q=2xy theta01 Gamma23, first lower=0,
    inner=4i xy Gamma0123 top, star inner=-4i xy Gamma0123,
    outer=8xy W, KQ=-4xy W.

All ten grade5 coefficients are retained. Meanwhile

    DQ_T^dag K^dag T=
      4x²(E00+E22-I)+4y²(E11+E33-I).

Each diagonal square comes from twelve-4 contractions, and the two mixed
terms vanish by disjoint vector/bivector commutation. It is vector-valued,
so it cannot cancel the grade5 piece of the actual cubic gradient.
Take the allowed V=theta4 Gamma01234. Its commutators with gamma2 and
gamma3 both vanish. Thus Q(T+tV)=Q(T) EXACTLY, while

    B(V,KQ)=4xy, I_gamma(T+tV)=4gamma xyt/3 at kappa0.

The degree-three literal polynomial chain independently checks every
coefficient and all three action derivatives for gamma1 and2. A restriction
to vector variations would miss this nonzero derivative, even though the
constant isotropic linear potential carrier is valid. This is not a new
source convention or a full-field stationary solution.

## Prospective census, resources and fail-closed scope

The complete fixture freezes exact counts for the selected31 word products
on each of16384 blades (507904),16384 Hodge controls,196 full columns,
588 separate potential legs,17836 support probes,38416 ordered action and
Gram/reciprocity entries,196 sector vectors and196 exact decompositions.
The scalar polynomial menu has35 monomials through degree4 in(s,gamma,kappa).
The decoy has20 monomials through degree3 in(x,y,t). Parameter, branch and
decoy subcounts are frozen in Program's full FixtureJson before execution.

More explicitly, four rational and four polynomial known answers precede
five isotropic tensor, eight independent-forward-stage and two isotropic
adjoint-leg controls. Each of35 degree-four monomials gets an action,
full-gradient and first-variation check. The196 adjoint inputs give392
separate leg comparisons,196 simplified comparisons,196 outer-transpose
checks and196 full reconstructions, alongside the exact support counts
already derived above. Each potential input has eight optimized/independent
forward stages, hence1568 stage checks. Gram, original-action bilinear,
weighted reciprocity, mass identity and potential-matrix entry counts are
each196²=38416; sector and decomposition counts are each196. The27
parameter rows, nine coupling/origin rows, four nonzero branches, twelve
branch-sector coefficients and four omitted-adjoint rejections are fixed.
The decoy has eight stage checks and five sets of20 monomial comparisons:
Q, KQ, adjoint composite, full cubic gradient and unchanged extended Q.
Two gamma rows give40 action and120 derivative coefficients, plus one
nonzero signed pairing and ten retained grade5 terms. These are the51
named count fields; no count was obtained from a scientific execution.

All fields are constant: Fourier frequencies are exactly0, not sampled.
Only five immutable600/611 compiled helpers are used. A Fourier coefficient
of any actual input has at most91 positions. A forward first leg has at
most2*91=182 exterior-compatible candidates; the inner at most91 and
outer at most14*91=1274, giving1456 candidates before cancellations.
Reverse and DQ-adjoint partial products have at most14*91=1274 candidate
terms. Polynomial coefficients are stored separately. The fixed8192-term
guard therefore exceeds a pre-run structural bound, not a measured maximum.
Degree at most4 gives35 monomials. Planned CPU120s and memory256MiB are
estimates, not runtime guarantees. Tracked grouped Fourier products have
a prospective ceiling100000000; maximum stored tensor8192 and zero
frequency are checked. Naive known-word operations are separate from that
counter and bounded by their finite menu. No resource limit is tuned to a run.

Exactly19 unique bindings cover own Program/helper/project/proof, the
three600 compiled helpers plus its program/contract/passed summary, the
two611 compiled helpers plus program/proof/contract/passed summary, source,
live726 sorted core manifest and Directory.Build.props. The contract does
not self-bind; it is separately hashed. Inputs, full fixture equality,
upstream success and exact named14 false firewalls are checked before science.
Terminal precedence: invalid-or-drifted-input; known-answer-control-failed;
isotropic-gradient-control-failed; complete-adjoint-control-failed;
potential-hessian-control-failed; stationary-branch-control-failed;
nonlinear-grade-five-control-failed; resource-census-control-failed; then
canonical-isotropic-stationarity-controls-pass-potential-not-spectrum.
All tests use exact zero tolerance. No source metric, norm, vacuum, stability,
pole or physical spectrum is selected. All14 authority flags remain false,
external review pending, promotedPhysicalMassClaimCount0. Preserve any first
failure unchanged and version repairs. Only the unbound implementation note
may receive results after the approved first execution.
