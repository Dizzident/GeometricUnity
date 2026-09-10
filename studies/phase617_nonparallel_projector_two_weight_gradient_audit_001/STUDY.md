# Phase617: nonparallel projector and full two-weight gradient

Prospective A61 v1. No scientific execution is authorized by this document.
First execution requires independent and MAIN review of all frozen bytes,
full fixtures, counts, resources, bindings and zero-warning Release build.
The implementation note alone is unbound and may later record results.

## Source, declared geometry and question

The bound April 1, 2021 draft, equations 3.7, 3.8, 3.10, 3.15, 3.17,
9.1 and 9.4, motivates a source-induced upstairs geometry and the first
connection action. This audit selects neither author intent nor the missing
physical norm. The already frozen 608 model has alpha=1, beta=-1/2,
sigma=-1 and y=diag(-1,1,1,1) or diag(-1,4,9,16). Its ambient signature
is (7,7); its traceless-vertical projector PT has rank9. 610 provides the
declared oriented frames and independently certified spin lift.

The declared operator is canonical UNTIED CAA: first C, outer A, inner A,
Phi1=Gamma1 and Phi2=Gamma2. This is not 610's tied chiral operator.
The real pairing is the signed form metric times -ReTr(XY)/128.
The geometry, reference B, tensors and real gamma,kappa remain fixed.
The formal adjoint assumes compactly supported variations, or vanishing
boundary terms. The local calculation does not supply a global integration
domain, boundary condition, physical Hilbert norm or observer restriction.

Test S=(aPT+b(I-PT))Gamma1 with CONSTANT real weights a,b. The complete
fixed-geometry connection gradient to be tested is

    G = K(FB) + (K DB S + DB-dagger K-dagger S)/2
        + gamma[K Q(S) + DQ_S-dagger K-dagger S]/3 + kappa S.

No scalar restricted-action stationary point is accepted without this full
tensor gradient. This phase has no dependency on unexecuted Phase616.

## Complete covariant projector derivative

In the fixed coordinate basis (four horizontal vectors, ten symmetric
vertical matrices), PT acts as A -> A-y tr(y^-1 A)/4 vertically and is zero
horizontally. For a vertical coordinate direction X,

    partial_X PT(A)
      = [y tr(y^-1 X y^-1 A)-X tr(y^-1 A)]/4,

and partial_u PT=0. This follows by differentiating the inverse identity
partial_X y^-1=-y^-1 X y^-1, not by holding PT's eigenbasis fixed.

The executable reconstructs ALL coordinate Christoffel matrices directly
from 608's metric first jets by Koszul and compares every entry with 608's
frozen sparse connection. Then it assembles

    nabla_i PT = partial_i PT + Gamma_i PT - PT Gamma_i.

An independent typed formula, derived from the horizontal/vertical
connection laws, is used as the oracle:

    (nabla_u PT)v = sigma/2[y Sym(u,v)y-(u^T y v)y/4],
    (nabla_u PT)A = y^-1(PT A)u/2,
    nabla_A PT = 0.

The ten vertical coordinate partials and commutators are separately
nonzero and cancel; deleting the partial term fails. Four horizontal
derivatives are nonzero despite zero coordinate partial. Thus deleting
the connection term or declaring the projector parallel fails.

Only AFTER assembling the coordinate covariant tensor is it transported:

    (nabla_ea PT)_frame = E^-1 [sum_i E_ia nabla_i PT] E.

There is no derivative of a varying frame being silently omitted: this
is pointwise transformation of an already covariant (1,2)-tensor.
In the resulting frame PT is diagonal on indices1,2,3,4,5,6,11,12,13,
zero on0,7,8,9,10. All14 direction matrices agree at the transported
points. They obey trace0, metric self-adjointness and
(nabla PT)PT+PT(nabla PT)=nabla PT. Their signed divergence is zero.
For the distinguished direction,
(nabla_0 PT)e0=(e1+e2+e3)/4.

## Both kinetic legs and complete Clifford support

Since the Clifford solder tensors are Levi-Civita parallel, K and K-dagger
are parallel bundle maps. At any orthonormal frame the differential legs
can therefore be computed from the full covariant derivative tensor as

    DB(PT Gamma1) = sum_a theta_a wedge gamma((nabla_a PT) .),
    DB-dagger K-dagger(PT Gamma1)
      = -sum_a sigma_a interior_a K-dagger(gamma((nabla_a PT) .)).

The second line retains ALL14 derivative inputs and both literal reverse
CAA summands. It is not a partial derivative of a frame-conjugated tensor.
Forward CAA is independently recomputed with word-list Clifford products;
each reverse chain is also compared with its separately simplified
transpose. Both complete kinetic legs are predicted to equal

    J_b = -2 sum_a sigma_a gamma_a wedgeCl gamma((nabla_a PT)b).

This identity follows by inserting a self-adjoint V into the full adjoint

    (K-dagger(V Gamma))_ab
      = 2[gamma(Va) wedgeCl gamma_b + gamma_a wedgeCl gamma(Vb)]
        -2 tr(V) gamma_a wedgeCl gamma_b,

using trace(nabla V)=0 and div(V)=0. Both conditions hold for PT.
The program nevertheless computes both chains literally before comparing
with J assembled from the INDEPENDENT typed derivative oracle.
Each kinetic half alone is J/2, not the full J, so deleting either fails.

Full support is even bivector-valued: the forward first commutator of
vectors yields grade2. The inner anticommutator with Gamma2 yields grade3;
the outer anticommutator of grades1 and3 retains their contraction grade2.
The reverse adjoint starts in a vector-valued one-form and ends in a
bivector-valued two-form, whose divergence preserves Clifford grade.
No higher-grade component is removed by a projection in the code.

The coefficient at theta0 Gamma01 is -1/2 for PT, so for S above it is
-(a-b)/2. The oriented frames and eta signs are bound to610, rather than
chosen to manufacture this sign. All full stages and reverse components
are retained in the result.

## Complete algebraic gradient and independent action variations

At both points the full coordinate curvature from608 is lowered,
transported in all four slots and spin-lifted by immutable610 code.
The resulting complete spin tensor must equal610's bound tensor. It is
then fed through both literal canonical CAA chains, not replaced by an
Einstein oracle. The predicted FULL source is

    K(FB)=-(15/4)PT Gamma1-(21/4)(I-PT)Gamma1.

For diagonal V with eigenvalues t_i, tau=sum t_i and rho=sum t_i^2,
Q(V Gamma1) has matching form/Clifford pair blades. Its full image and
the complete reverse variation satisfy

    (KQ)_i = 2[(tau-t_i)^2-(rho-t_i^2)],
    (K-dagger S)_ij = 2(t_i+t_j-tau) Gamma_ij,
    DQ_S-dagger K-dagger S = 2KQ.

These are full-support statements: matching pair indices eliminate the
disjoint inner grade4 curvature term; DQ-dagger then contracts the
bivector adjoint with a vector and gives grade1. The code computes all
outputs before testing these formulas. Neither full support nor the
adjoint term is inferred only from restricted scalar derivatives.

Since V has nine eigenvalues a and five b, the exact degree2 KQ
coefficients in monomials a^2,ab,b^2 are respectively

    PT:      112,160,40;
    I-PT:    144,144,24.

The program explicitly forms all three Q coefficients and both cross
adjoint terms. Complete tensor equality with these oracles proves the
polynomial identities for every real a,b, not just the parameter menu.
K-dagger PT and K-dagger(I-PT) are also reconstructed from all91 matching
pair probes via the ORIGINAL forward operator and nonzero probe Gram.
Their complete adjoint tensors are compared, not only projected pairings.

For each of three coefficients, all196 vector variation directions are
tested by differentiating the ORIGINAL first action:

    delta I_cubic = [Pair(deltaS,KQ)+Pair(S,K DQ_S deltaS)]/3.

This agrees with Pair(deltaS,(KQ+DQ-dagger K-dagger S)/3).
These1176 two-point action tests supplement, rather than substitute for,
the full-grade construction and support proof above.

Consequently the complete algebraic/source vector coefficients are

    G_a=4gamma(28a^2+40ab+10b^2)+kappa a-15/4,
    G_b=4gamma(36a^2+36ab+6b^2)+kappa b-21/4.

The prospective planted control gamma1,kappa11,a3/4,b-3/4 has
KQ=(-9/2)PT Gamma1+(27/2)(I-PT)Gamma1, and both vector residuals zero,
but its full differential theta0 Gamma01 coefficient is -3/4.
This exact control was derived before implementation; it is not fitted.

For a!=b the nonzero grade2 witness cannot cancel against grade1
algebraic/source terms, for ANY real gamma,kappa. For a=b the differential
part is zero but G_a-G_b=3/2. Hence no constant two-weight ansatz member
solves this declared local connection equation, including zero couplings.
This is not a theorem about nonconstant weights, additional Clifford
components, other source conventions, the coupled metric equation or
global physical vacua. No spectrum or mass scale is inferred.

## Prospective finite census and resources

The complete fixture is duplicated byte-independently as structured JSON
in Program and contract and compared using deep equality. Tolerance is0.
The expected38 counters are literal loop-count forecasts:
4 arithmetic controls; 16384*31=507904 signed word checks and16384 stars;
2 points with2 frame and2 projector checks;
2*14^3=5488 coordinate connection, coordinate derivative and frame
derivative entries each; one-point transport14^3=2744 entries;
2*14=28 derivative trace, projector-derivative and divergence rows each;
8 horizontal and20 vertical omission controls;2 horizontal anchors;
16 kinetic stage comparisons;28 reverse type and28 simplified comparisons;
4 full kinetic-leg,2 full kinetic,2 witness,4 omission and1 transport checks;
2 spin reconstructions,16 source stage and2 source checks;
4 algebraic adjoints,4 reconstructions,4*91=364 forward probes;
6 coefficient checks each for KQ, adjoint composite and full potential;
2*3*196=1176 original-action variations;
2*3 gamma*3 kappa*3 a*3 b=162 parameter rows,54 equal and108 unequal;
2 planted false-stationary rows.

Parameter values are gamma0,1,2; kappa-1,0,1; a,b-1,0,1. These rows
exercise zero and nonzero cases; all-real conclusions depend on the
complete coefficient identities and parity proof, not sampling this grid.
There is no random sampling, fitting or numerical interpolation.

Resource ceilings are100M tracked sparse coefficient products and100M
tracked dense-matrix products,32768 terms in any Fourier dictionary,
zero frequency, matrix dimension14 and largest dense array14^4=38416.
The only formal polynomial consists of three stored degree2 coefficients.
At most162 parameter rows are retained. Algebraic and derivative inputs
are vector/bivector finite masks. A generic spin-curvature input has at
most91^2=8281 entries. The first C leg lies in14 form positions times
Clifford grades1 or3 (14+364), at most5292 terms. The inner A leg lies in
one form position with grades0 or4, at most1002 terms; its outer A leg
lies in14 form positions and grades1,3,5, but produces at most14*1002
=14028 raw contributions. All are below32768, including partial
accumulations before cancellation. Reverse vector adjoints have at most
91*(91+1001) generic grade2/4 positions but the compiled reverse
operations on the actually supplied vector inputs only yield grade2:
first C reverse sends vector-valued one-forms to grade2, while the
second summand is trace times Gamma2, hence at most91^2=8281. Kinetic
input derivatives never exceed196 vector slots per one-form.
For full coefficient/action probes the diagonal source or one elementary
variation further narrows these bounds. The declared32768 ceiling is
conservative and does not assume generic8281<8192.

The runtime enforces the product counters, tensor maximum, all counters
and frequency predicate. Matrix/array dimensions, polynomial coefficient
count and retained row count are structural from finite loops; they are
not advertised as sampled runtime memory measurements. Estimated CPU120s
and memory256MiB are prospective engineering estimates, not physical
thresholds or wall-clock pass gates.

## Failure precedence, binding closure and claim firewall

The contract fixes the ordered failure classes:
invalid/drifted input; known answer; coordinate connection; covariant
projector; complete kinetic adjoint; curved source; full algebraic
gradient; planted false stationarity; resource/census; success.
No failed first result may be repaired by editing these frozen inputs.
A repair requires a newly reviewed version and preserved failed output.

There are34 unique direct bindings: own Program/project/helper/STUDY4;
600's three compiled helpers plus Program/STUDY/contract/summary7;
607,608,610,611 each compiled helper plus Program/STUDY/contract/summary5;
primary source/core manifest/Directory.Build.props3. Every compiled helper
is directly hash-bound. Every upstream summary must be audited passing,
match its contract hash and expected terminal, retain known-answer/control
success, all14 false authority flags, pending external review and zero
promoted claims. The live726-file core manifest is verified by exact
sorted path list, each SHA256 and aggregate tree digest before science.

All14 authority flags remain false. O4/external review stays pending,
Phase561 stays closed, WZ15/H14 source-field deficits remain and there
are zero promoted physical mass claims. No upstream or core scientific
bytes may be changed. Results are deterministic; full and summary JSON
are identical and contain complete retained tensors, not only booleans.
