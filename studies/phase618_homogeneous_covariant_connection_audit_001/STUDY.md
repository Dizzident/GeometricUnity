# Phase618: homogeneous covariant connection and conditional existence

Prospective A62 v1, on main. All scientific code, helpers, project, this proof,
fixtures, counts, resources and contract freeze before FIRST science.
Release build checks are permitted; scientific execution requires complete
independent and MAIN review and MAIN's explicit approval. No HJ, H^2J,
nonlinear-J coefficient experiment or iterative field solve is in this pack.

## Declared geometry and full source domain

Use the already passed608 alpha1,beta-1/2,sigma-1 geometry at
y0=diag(-1,1,1,1) and y1=diag(-1,4,9,16). At a fixed metric fibre point
write p=y^-1 and tangent vectors X=(u,A), with u horizontal and A symmetric.
The metric is

    G(X,Y)=sigma u^T y v + tr(pA pB)+beta tr(pA)tr(pB).

The canonical UNTIED CAA operator has firstC, outerA, innerA, with
Phi1=Gamma1,Phi2=Gamma2. This is a declared diagnostic convention, not
selection of the author's unresolved operator occurrence. The fixed-geometry
first-action connection gradient is

    F(S)=A0+HS+gamma N(S)+kappa S,
    A0=K(FB),
    H=(K DB+DB-dagger K-dagger)/2,
    N(S)=[KQ(S)+DQ_S-dagger K-dagger S]/3.

The signed real pairing is exterior metric times -ReTr(XY)/128, with the
formal adjoint justified by compact-support variations or vanishing boundary
terms. It is NOT replaced by the auxiliary positive coefficient norm below.
No epsilon/reference variation or metric Euler equation is solved here.

The bound primary draft explicitly gives u(64,64) in3.27 and the adjoint
bundle with fibre u(64,64) in3.34. Its3.27 decomposition includes real
grades1,2 mod4 and imaginary grades0,3 mod4, including central iI.
The source's checkerboard caveat is retained: the executable independently
checks the H-antiHermitian phase for EVERY16384 Clifford mask. The rule is
real phase1 when (-1)^(r(r+1)/2)=-1, otherwise phase i. Thus the real algebra
dimension is16384, and its one-form fibre dimension is14*16384=229376.
There is no special-unitary, traceless, parity or vector/bivector projection.

Source12.26 and12.27 give the commutator and i-anticommutator. For
H-antiHermitian X,Y, [X,Y] and i(XY+YX) are H-antiHermitian by reversing
the product under the adjoint. In particular {gamma0,gamma0}=2iI in the
declared positive gamma0 direction. Discarding the central direction would
destroy closure. The full real algebra is the domain of the existence proof.

## Homogeneity, reductive lift and the moving frame

The affine group GL+(4) semidirect R4 acts by

    (L,t):(x,y) -> (Lx+t,L^-T y L^-1),
    (u,A) -> (Lu,L^-T A L^-1).

Both metric terms are invariant: u'^T y' v'=u^T yv and
p'A'=L(pA)L^-1, so the traces are unchanged. The action is locally
transitive; the stabilizer of (0,y) is SO(y), with Lie algebra y-skew Z.
The two SO(1,3) components are represented by the identity component and
-I4. This statement concerns tensor descent in the declared homogeneous
model, not a global spinor bundle on an unspecified physical spacetime.

Choose the reductive lift

    (u,A) -> (u,B_A), B_A=-pA/2.

Its tangent metric variation is -B_A^T y-yB_A=A. The Lie algebra bracket
splits into

    [X,Y]_m=(-pA v/2+pB u/2,0),
    [X,Y]_h=[pA,pB]/4.

The isotropy representation is

    rho(Z)u=Zu, rho(Z)A=-Z^T A-AZ.

The same expression differentiates the group-transported tangent frame
along exp(tB_A): its moving-frame contribution is rho(B_A), including the
HORIZONTAL derivative B_A u as well as the vertical matrix derivative.
The independent coordinate connection is rebuilt by Koszul from608's
complete metric first jets. Adding frame motion to it gives

    Lambda_A=0,
    Lambda_u v=-sigma W(u,v)/2,
    Lambda_u A=pAu/2,

where W(u,v)=y Sym(u,v)y-(beta/(1+4beta))(u^T yv)y.
For vertical directions, Gamma_A u=pAu/2 cancels B_Au=-pAu/2.
Likewise Gamma_A B=-(ApB+BpA)/2 cancels
-B_A^T B-BB_A=(ApB+BpA)/2. This is why Lambda_A vanishes.
It is NOT valid to delete coordinate connection terms or declare a
coordinate frame constant and obtain the same result.

The program computes all14 full coordinate matrices on both routes and
compares every entry before using them. In610's fixed oriented frame,
the already constructed bilinear Nomizu map transforms as

    Lambda_frame(ea)=E^-1[sum_i E_ia Lambda_coordinate(ei)]E.

No derivative of a varying frame is missing: that derivative was included
before this pointwise change of basis. Both transported points must agree.

Metric compatibility is Lambda_X^T G+G Lambda_X=0. The mixed cancellation
uses GVV(W(u,v),A)=u^T A v. Torsion is
Lambda_XY-Lambda_YX=[X,Y]_m. These two properties uniquely identify the
Levi-Civita connection of the invariant metric; finite matrix checks
supplement this algebraic proof, rather than proving only selected samples.

## Full curvature and independent controls

For the invariant connection, with the same R(X,Y) convention as608,

    R(X,Y)=[Lambda_X,Lambda_Y]
           -Lambda_[X,Y]_m-rho([X,Y]_h).

The isotropy term is essential. For instance,

    R(A,B)C=-y[[pA,pB],pC]/4,
    R(A,B)u=-[pA,pB]u/4,
    R(A,u)v=-sigma W(pAu,v)/4,
    R(A,u)B=pBpAu/4.

These match608's independently differentiated coordinate connection laws.
The executable builds the full Nomizu curvature from the recovered
coordinate-plus-motion matrices and compares all2*14^4=76832 entries
with608's frozen full curvature, including zeros. No Einstein tensor is
substituted as a curvature input.

Deleting frame motion fails for each of20 vertical coordinate rows.
Deleting isotropy fails on the vertical pair E00,E01+E10 at each point.
Reversing the m-bracket sign fails on the pair E00,horizontal e0.
These nonzero anchors were chosen algebraically before execution.

## Projector derivative and both full kinetic legs

An invariant endomorphism has nabla_X P=[Lambda_X,P]. Thus all14
derivatives of PT are obtained without new coordinate second jets and
compared entrywise with617's complete coordinate-derived result.
At the origin, Lambda_0 e0=-(e1+e2+e3+T)/4, with T=e10=-y/2,
Lambda_0 ei=e0/4 for i1,2,3 and Lambda_0 T=-e0/4.
Consequently [Lambda_0,PT]e0=(e1+e2+e3)/4, agreeing with617's sign.

All canonical Gamma1 and Gamma2 derivatives vanish under the FULL
Clifford-plus-covector action. The code implements the induced exterior
action on every covector and Clifford slot, with no selected grade cutoff.
The Clifford action is independently tested against its spin commutator
on all vector generators for the six Lorentz directions.

Using the full derivative tensor, form DB(PT Gamma1), run both forward
CAA implementations and compute every reverse CAA derivative before
codifferentiation. Both full kinetic legs must equal617's retained J.
The full J also agrees with its independent wedge formula, with
theta0 Gamma01 coefficient-1/2. No new HJ or H^2J is computed.

The curvature source A0 is read from617's bound complete literal CAA
source and compared with -(21/4)Gamma1+(3/2)PTGamma1. It is not promoted
to a source-selected physical operator. Its auxiliary coefficient norm60
is checked at both transported points.

## Complete isotropy and the unrestricted invariant field space

The six base Lorentz generators are Eij-eta_i eta_j Eji for i<j, transported
by D^-1 Z D at the second point, D=diag(1,2,3,4). The code tests all
generator metric conditions and the complete representation bracket
rho([Z,W])=[rho(Z),rho(W)]. It checks the full Nomizu equivariance identity

    [rho(Z),Lambda_X]=Lambda_(rho(Z)X)

on every14-dimensional input and matrix coefficient, not just projectors.
It also checks PT, Gamma1,Gamma2,A0,J under every generator. The
disconnected representative -I4 acts as -I on the four horizontal axes
and +I on the ten vertical axes, has upstairs determinant+1, and obeys
the same full Nomizu/tensor covariance checks. It covers the other
SO(1,3) component. Global spinor descent is deliberately not claimed.

Let V be the FULL isotropy-fixed subspace of the real229376-dimensional
Clifford-valued one-form fibre. Its dimension need not be explicitly
enumerated for this proof. An element of V determines an invariant
Clifford-adjoint tensor field on the homogeneous model, or locally on its
spin description. Naturality of the Clifford algebra, oriented Hodge
maps, solder tensors, Levi-Civita derivative and real trace adjoints
implies that K,DB,their adjoints and the polynomial products preserve
invariant fields. Therefore H:V->V is linear and N:V->V is quadratic.
All grades and central directions are retained in this statement.

This restricts the ALREADY DERIVED FULL tensor gradient F(S) to invariant
fields. It never infers full stationarity by differentiating an invariant
scalar action: the trace pairing restricted to V might be degenerate.
Solving F(S)=0 in V sets the full field to zero pointwise, and hence gives
the local fixed-geometry connection Euler equation for arbitrary allowed
compact-support variations, not just invariant ones.

## Explicit auxiliary bounds, without an enormous invariant basis

Use coefficient l1 in the fixed real H-antiHermitian basis, or equivalently
sum of absolute real and imaginary coefficients. This is only a finite-
dimensional existence norm, not a physical inner product or the source's
unresolved action norm. Product is submultiplicative, signed Hodge is a
coefficient permutation, and C/A brackets have bound2 times product.
Gamma1 has norm14 and Gamma2 norm91. Thus BOTH forward and reverse CAA
have the conservative universal majorant

    k=28+(28*182)/2=2576.

The reverse primitive transpose has the same coefficient sum bound, because
the signed form factors have modulus1; no positive physical adjoint is
being substituted for the trace adjoint.

A single metric-skew coordinate plane acts on any exterior basis blade as
a partial signed permutation: it vanishes if both or neither plane index
is present, otherwise replaces the one present index. Its l1 norm is at
most the plane coefficient, independently of blade degree. On a
Clifford-valued p-form there are two slot representations, hence

    ||rho_p(Lambda)|| <= 2 sum_(b<c) |Lambda^b_c|.

This includes all Clifford grades and all form degrees used by d,dagger.
The executable tests the16 plane-occupancy combinations for all91 planes,
while the preceding bit-replacement proof covers spectator indices and
every grade without exhaustive operator-column generation.

In610's frame each of four horizontal Lambda matrices has pair sum5/2;
the remaining ten vanish. The diagonal-vertical block contributes
4*(1/4)=1; the three incident offdiagonal blocks each contribute
1/8+3/8=1/2. All28 sums are checked exactly from the recovered matrices.
Therefore d and d-dagger on invariant tensors have bound20 and

    ||H|| <= k*20 = 51520 = h.

For the symmetric bilinear polarization B(U,V) with N(S)=B(S,S), the
KQ term contributes at most k/3 and the two adjoint cross terms at most
2k/3. Thus ||B(U,V)|| <=2576||U||||V||, so c=2576.
The source has a=||A0||=9*15/4+5*21/4=60.

No numerical full invariant basis, matrix norm optimization, eigensolver,
fitting or iterative field solution is necessary for these conservative
majorants. They are valid on the full fibre and thus also on V.

## Contraction and genuine conditional local connection solutions

Fix any finite real gamma and let lambda=1/kappa be nonzero. Rewrite the
ORIGINAL full equation equivalently as the fixed-point problem

    S = T_lambda(S) = -lambda A0-lambda HS-lambda gamma N(S).

On the closed V-ball of radius r=2a|lambda|=120|lambda|, put
t=|lambda|h and q=|gamma|ca lambda^2. If t<=1/4 and q<=1/16, then

    ||T_lambda(S)||/r <=1/2+t+2q <=7/8,
    Lip(T_lambda) <=t+4q <=1/2.

The ball is complete since V is a closed finite-dimensional real vector
space. Iterating from0 is Cauchy: successive differences decrease at least
geometrically by1/2. The limit stays in the ball, satisfies the FULL
equation by continuity, and is the unique fixed point in this ball.
This proves actual invariant connection-stationary fields for sufficiently
small nonzero |lambda|, not just cancellation in a formal series.
No convergence radius optimized from observed data is used.

The15 prospective rational certificates use gamma=-2,-1,0,1,2 and
lambda=sign/[4(a+h+c(|gamma|+1))], sign=-1,0,1. The positive denominator
makes t<=1/4. Also |gamma|ca <=(a+c|gamma|)^2/4, so q<=1/64 and
therefore <=1/16. The program checks both inequalities, the exact ball
image bound, r<=1 and Lipschitz<=1/2 without constructing a field solution.
The five lambda0 rows are formal base checks only: they do not correspond
to an original equation with finite kappa. The ten nonzero rows illustrate
a theorem valid for every fixed finite gamma and sufficiently small
|lambda|; these parameter choices are NOT selected physical couplings.

The solution is local as a physical statement. Nothing here proves finite
total action on the noncompact model, specified spacetime topology or spin
descent, boundary admissibility, metric or epsilon stationarity, stability,
the source's operator choice, observed-field extraction, a pole spectrum
or a GeV normalization. In particular no assertion is made at an unknown
prescribed source coupling outside the sufficient small-|lambda| regime.

## Finite census, resources and failure precedence

The duplicated full fixture freezes tolerance0 and46 exact counter fields:
4 arithmetic,507904 signed word,16384 Hodge and16384 full real-domain masks;
2 central and1456 plane-majorant controls;3 universal majorant identities;
2 contexts and2 oriented-frame controls;
5488 entries each for Nomizu equality, metric compatibility and torsion;
76832 full curvature entries;20 omitted frame,2 omitted isotropy and2
wrong bracket controls;28 plane-norm rows and2 derivative-majorant rows;
5488 projector derivative entries,2744 Nomizu transport entries;
56 canonical parallel,16 kinetic stage,28 reverse,4 kinetic-leg,
2 full J and1 J transport rows;2 source and2 source-norm checks;
192 base Lorentz entries,2352 isotropy-metric and2352 projector entries,
32928 Nomizu covariance entries,168 spin commutators,60 tensor actions,
14112 representation Lie entries;
2 disconnected orientation,392 metric,392 projector,5488 Nomizu and10
tensor checks;15 contraction certificates,5 formal-zero and10 nonzero,
30 sufficient-inequality,15 ball-image and15 Lipschitz checks.

The largest matrix is14 and dense curvature array14^4=38416. Matrix
product ceiling200M and sparse coefficient product ceiling100M are
instrumented counter limits, not counts of every rational operation.
Tensor ceiling32768 is conservative: the current source/J/derivative
chains use at most91^2=8281 two-form bivector slots and at most14*91
one-form bivector slots. Exterior actions preserve Clifford grade.
Known-domain masks are individual one-term tensors. No Fourier frequency
other than0 is present, and at most15 certificate rows are retained.
CPU120s and memory256MiB are engineering estimates, not measured outcomes
or wall-clock acceptance gates. Structural dimensions are enforced by the
fixed code and loops, while counters, tensor maximum and frequency are
checked at runtime.

Failure precedence is invalid/drifted input; known answer/domain;
homogeneous connection; full curvature; projector/kinetic; full isotropy;
conditional-existence majorant; omission decoy; resource/census; success.
All required booleans and exact counts must pass. A failed FIRST artifact
must be preserved; changing frozen scientific expectations requires a
new reviewed version, not repair in place.

There are39 unique direct bindings: own Program/project/helper/STUDY4;
600's three compiled helpers plus Program/STUDY/contract/summary7;
607,608,610,611,617 each compiled helper plus full provenance5;
primary source/core manifest/Directory.Build.props3. Every compiled file
is directly hash-bound. Upstream terminal, known-answer/control pass,
contract hash, all14 false flags, pending external review and zero promoted
claims are checked. Full fixture deep equality and live726 sorted path,
each SHA256 and aggregate tree closure precede scientific arithmetic.

All14 authority flags remain false. O4/external review remains pending,
Phase561 closed, source-field deficits WZ15/H14 and physical mass claims0.
Full and summary outputs are deterministic and byte-identical. Only the
unbound implementation note may receive approved result updates.
