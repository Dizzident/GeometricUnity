# Phase582: relative transport observable control

Amendment A44. First executed successfully under the prospectively frozen v1
contract on 2026-09-08. Deterministic mathematical control, no sampling or RNG.
The executable references no core or prior-study project and reads no saved
configuration. Exact hashes bind its code, project, primary GU text, Phase559
bridge status, Phase581 result, and the registered Phase450 SO(3) convention.

## Source and derivation are distinct

GU draft Eq. 12.6 defines displaced torsion as a difference of connections:
T=A-B, with B the epsilon-transformed reference spin connection. This motivates
the construction; it does not specify these compact edge variables or prove
they realize the registered workbench. Phase559's negative is retained.

Choose a common oriented edge i to j with transports U and V for A and B.
Use the convention U'=g_i U g_j^-1 and V'=g_i V g_j^-1. Then

    R = U V^-1,        R' = g_i R g_i^-1.

With V=epsilon_i^-1 V0 epsilon_j and a fixed reference transport V0, the rule
epsilon_i'=epsilon_i g_i^-1 gives precisely the required law for V. This is
the finite endpoint version of a common gauge transformation of both
connections, not an invariance claim under independent transformations of A
and B or the entire inhomogeneous GU group.

Represent SU(2) elements as unit quaternions (w,v), corresponding to
w I - i v.sigma. The adjoint character is 4w^2-1, so

    q = (3-Tr_Ad R)/4 = |v(R)|^2,       0 <= q <= 1.

This gives an invariant under every endpoint transformation, not just the
three numerical fixtures. The finite tests exercise that algebraic proof,
including independent complex-matrix multiplication. q is also unchanged
under either lift U -> -U or V -> -V and therefore descends to SO(3).
The fundamental character alternative 1-w is deliberately rejected because
it changes under R -> -R. This descent matches the recorded reduced adjoint
convention; it recovers neither full Spin(7,7) content nor center-sensitive
fermionic information. q is an internal-gauge invariant, not a proved
observer-space Lorentz scalar or an identified Higgs operator.

For A=B, R=I, including nontrivial pure-gauge links. Holding V fixed while
transforming U is a deliberate bad control, not a gauge symmetry of the pair.

## Local classical limit

In the convention t_a=-i sigma_a/2, U=I+a A_mu+O(a^2) and V=I+a B_mu+O(a^2).
Consequently R=I+a(A_mu-B_mu)+O(a^2), and 4q/a^2 tends to the Euclidean
coefficient norm squared of that directional difference. This fixes the factor
four; it is not a fitted normalization. An observer-space contraction still
requires its metric, edge reconstruction, and source justification.

The centered ordered midpoint product has U(-a)=U(a)^-1. Cyclicity and
SU(2) trace reality make q even in a, explaining the tested second-order
error. Constant noncommuting profiles and independent affine profiles use
a=0.2,0.1,0.05,0.025. A 128-subdivision product checks the frozen 64-subdivision
product; a commuting analytic sine identity is a separate control.

This is consistency of one observable under smooth-field refinement. It is not
a proof of continuum convergence of an action, a quantum measure, or a pole.
No reflected measure, transfer matrix, or universality theorem is supplied.

## Conditional measure, not a new workbench target

If U is normalized Haar conditional on V, right invariance makes UV^-1 Haar.
This also works when V is constructed from correlated vertex rotations,
provided that conditional-Haar premise holds. It is not asserted for the
interacting registered measure. With R=(cos(theta),sin(theta)*n), integration
over n gives (2/pi) sin^2(theta) dtheta for theta in [0,pi]. The two branches
of q=sin^2(theta) give

    p(q) = (2/pi) sqrt(q/(1-q)),        0 < q < 1.

This is Beta(3/2,1/2), with moments 1,3/4,5/8 at orders 0,1,2. Deterministic
Simpson integration in theta checks these moments without integrating the
endpoint singularity in q directly. Its preference for large q is present in
the free Haar control: it is not a dynamically generated condensate. Uniform
dq has the wrong first moment. The registered Lebesgue omega measure and
epsilon Haar measure cannot silently be replaced by this conditional model.

## Boundaries and next work

Local invariance is now available for this explicitly declared compact link
construction, unlike the Phase581 fixed-linear family. Applying it to the
registered program requires deriving the A/B/omega/epsilon dictionary and a
compatible action and measure. Merely exponentiating old coefficients and
relabeling a statistic would not close that bridge. The draft's two-connection
structure deserves that derivation; no author-prescribed lattice is necessary
if equivalence and controlled approximation can actually be established.

No original output, source contract, O4 intake, Phase561 gate, Phase458 gate,
or production pack changes. promotedPhysicalMassClaimCount=0 on every branch.

Run: `dotnet run -c Release --project studies/phase582_relative_transport_observable_control_001/Phase582RelativeTransportObservableControl.csproj`.
