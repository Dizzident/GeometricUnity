# Phase581: collective-coordinate assumption audit

Amendment A43 authorizes this zero-sampling mathematical follow-up to Phase579.
The contract fixes all numerical fixtures and source/program hashes before the
first execution. Predictions are analytic and stated before computation; no
measured particle value enters the study. The three-edge integer arrays are
mathematical controls, not reconstructed lattice configurations. Applicability
to the 1,215 adjoint triples follows from the blockwise proof and the bound
Phase579 reconstruction, not from extrapolating three sampled rotations.

For the adjoint generators J, skew-symmetry implies u^T J u=0 for every u.
Thus projecting u away from J u imposes no invariance constraint. Invariance
of a fixed linear functional at every omega instead requires J^T u=0.
The exactly verified sum J^T J=2I proves that the only such functional is zero.

For matrices U and W with edge vectors as rows, Phi(R W) contracts U^T W
with R. The first SO(3) moment vanishes and its second moment is
E[R_ia R_jb]=delta_ij delta_ab/3. The finite rotation menu verifies these
coefficient identities exactly, so the degree-two average is the Haar average
for arbitrary real U,W. Consequently 3 E[Phi^2]=||U^T W||_F^2 is globally
invariant. This is not a claim that the finite group reproduces higher Haar
moments. The norm identity also proves continuous invariance; a Rodrigues
rotation is an independent numerical control.

This construction is a global-invariant diagnostic only. An independent local
SU(2) pure-gauge triangle demonstrates why connection coefficient norms are
not automatically local-gauge invariants. It does not define the registered
connection's transformation law or select a different discretization.

The Gaussian control retains the full radial Jacobian. The nonzero mode of a
finite-dimensional radial density, and its change under q=r^2, demonstrate
that a displaced density peak is not sufficient evidence of dynamical breaking.
Factoring the known radial weight is an analytic control decomposition, not
permission to discard a measure factor in an interacting theory.

No original artifact, model, sampling pack, source contract, or review intake
is changed. No local observable, electroweak particle, or pole is established.
No external interpretation or source selection is inferred.

Run: `dotnet run -c Release --project studies/phase581_collective_coordinate_assumption_audit_001/Phase581CollectiveCoordinateAssumptionAudit.csproj`.
