# DIRAC-SHELL-RESPONSE-BOUNDARY

## Sources

- Geometric Unity Author's Working Draft v1.0:
  https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf
- Ofir E. Alon and L. S. Cederbaum, *Hellmann-Feynman theorem at degeneracies*:
  https://doi.org/10.1103/PhysRevB.68.033105
- Edwin Langmann, *Generalized Yang-Mills actions from Dirac operator
  determinants*:
  https://arxiv.org/abs/math-ph/0104011
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

The GU draft motivates a mixed boson-fermion deformation program but does not
stabilize a physical coupled Hessian. Alon and Cederbaum show that derivative
information at an exact degeneracy is obtained by diagonalizing the derivative
operator within the degenerate subspace. Langmann treats a fermion-induced
gauge action as a regularized Dirac-determinant effective action.

Together these sources define a useful boundary for the local experiments:
shell-restricted Dirac perturbation blocks can support a discrete response
diagnostic, but they cannot be promoted into a physical effective-action
Hessian without the missing action, regularization, branch, and gauge
identities.

## How It Was Used

- Used in Phase377 to define the bounded response metric
  `Q_ab = Re Tr(G_a^dagger G_b)` from Phase376 projected shell blocks.
- Used in Phase378 to lift the same response construction to the full
  `156`-coordinate connection-`1`-form carrier and verify that Phase377 is its
  selected-source restriction.
- Used in Phase379 to characterize the positive rank-`3` response-image
  projector in the full carrier while preserving the distinction from a
  physical W/Z source law.
- Used in Phase380 to stress-test that response image against the Phase201 W/Z
  source-lineage contract. The response image accepted zero W/Z contract fields
  and left the Phase201 template unmutated.
- Used in Phase381 as the boundary source for comparing the Phase379
  response-image sidecar with the Phase302/307 selected W/Z near-pass selector.
  The result is a sidecar conflict, not a W/Z source-row promotion.
- Used in Phase382 to require an observed electroweak projection map before
  reinterpreting Phase379 carrier axes as separate from physical photon/W/Z
  axes. No such map is present in the current artifact set.
- Used in Phase383 to keep the Phase307 suppressed-axis counterfactual within
  the existing predeclared selector space. The audit does not invent a new W
  source law and does not promote a physical W/Z row.
- Used in Phase384 to keep the Phase27 basis-energy proxy test within the same
  non-promotional boundary. Basis-energy fractions are treated as internal
  metadata, not observed W/Z projection rows or a physical response-image
  projector.
- Used in Phase388 to require a theorem-level observed electroweak
  namespace/source law before promoting the VO-7 shell-response diagnostics to
  Phase201 or Phase256 contract evidence.
- Used to keep the Phase377 response metric distinct from a physical
  fermion-determinant effective-action Hessian.
- Used to record that the Phase376 absolute-value spectral shell is not an
  exact-degeneracy energy-slope theorem.

## Prediction Relevance

Phase377, Phase378, Phase379, Phase380, Phase381, Phase382, Phase383, and
Phase384 can test whether connection directions induce a stable, nonzero
discrete shell response and can characterize its carrier image. Phase379 finds
a strong two-axis carrier diagnostic, and Phase380 confirms that this diagnostic
still does not provide Phase201 W/Z source-lineage fields, Higgs scalar-source
lineage, observed-field extraction, or GeV normalization. Phase381 further
shows that the Phase307 selected near-pass W row uses the Phase379-suppressed
axis, so the diagnostic does not support the selected charged-ladder route.
Phase382 shows that current observed-field artifacts cannot separate that
carrier-axis diagnostic from physical W/Z axes. Phase383 shows that the current
Phase307 predeclared selector space has no alternate W source definition that
avoids the suppressed axis. Phase384 shows that Phase27 basis-energy metadata
does not provide a lower-axis proxy escape. Phase388 shows that the combined
VO-7 shell-response branch still has no candidate theorem that can bridge these
diagnostics to observed photon/W/Z/H namespace rows or W/Z/H source rows.

## Limitation

The response metric is study-defined and shell-scoped. Phase378 removes the
selected-subspace limitation for the current `156`-coordinate carrier, but it
is still not a GU-native physical mass operator. Phase379 characterizes the
carrier image, not a physical observed-field projection. Phase380 is a
non-mutating contract audit, not a prediction. Phase381 is a selector
compatibility audit, not a prediction. Phase382 is an observed-projection
requirement audit, not a prediction. Phase383 is a counterfactual selector
space audit, not a prediction. Phase384 is a basis-energy proxy audit, not a
prediction. Phase388 is a theorem-probe boundary audit, not a prediction.

## Follow-Up

- Supply a fixed GU fermionic action and gauge-compatible mixed blocks before
  attempting a physical effective-action Hessian.
- Supply observed-field extraction, pole extraction, and unit normalization
  before any W/Z/H prediction attempt.
- Supply a direct observed electroweak namespace/source theorem before applying
  any shell-response branch to Phase201 or Phase256.

## Phase389 Usage

Phase389 extends the boundary cluster with a gauge-compatibility result: the
discrete commutator `[D(omega), X_hat]` decomposes exactly into the candidate
mixed block `delta_D[v(X)]` along the discrete covariant differential plus an
explicitly characterized symmetric anticommutator obstruction `R(X)`. The
obstruction vanishes for constant gauge parameters (exact equivariance) and is
a midpoint-discretization artifact for vertex-local parameters. This remains a
study-defined diagnostic on the toy control branch and does not upgrade the
shell-response Gram to a physical effective-action Hessian.

## Phase391 Usage

Phase391 replays the Phase378/379 shell-response Gram and carrier-axis
characterization on the exact dense generalized eigensolve from Phase390 and
confirms the invariants quantitatively (shell eigenvalues to 1.5e-10, axis
fractions to 1.7e-10, transport singular value to 2.2e-11). The rank-3
carrier image and the suppressed gauge axis are therefore solver-independent
properties of the discretized control branch. This strengthens the boundary:
the shell-response diagnostics are robust, but they remain study-defined
objects, not a physical effective-action Hessian or observed namespace map.

## Phase392 Usage

Phase392 realizes the degenerate second-order perturbation (Hellmann-Feynman)
side of this boundary cluster: the fermion-induced carrier response
R_kl = sum_s Re<delta_D[e_k] psi_s, (D - lambda_s M)^+ delta_D[e_l] psi_s>
computed exactly on the converged shell. Its structure (near-full rank,
mixed signature, nearly isotropic gauge axes) diverges from the
Hilbert-Schmidt pullback Gram (rank 3, suppressed axis 1), establishing that
the suppressed-axis obstruction is metric-dependent. Neither operator is a
physical effective-action Hessian: the Gram is study-defined, and the
second-order response is evaluated at a background that is not a coupled
critical point.

## Phase393 Usage

Phase393 evaluates the first-order side of the boundary cluster: the
fermionic source currents J_k = Re<psi_s, delta_D[e_k] psi_s> on the
converged shell. The shell-aggregated source cancels exactly between
plus/minus eigenvalue partners (so symmetric occupation is first-order
coupled-stationary and the Phase392 second-order response is the leading
backreaction object), and each per-mode source lies identically in the
rank-3 carrier image. The persisted bosonic Gauss-Newton spectrum is
numerical-kernel-only, so the asymmetric first-order backreaction is not
constructible from persisted artifacts.

## Phase394 Usage

Phase394 completes the first-order coupled toolkit: the full positive bosonic
Gauss-Newton spectrum (recomputed with production provenance; PSD, 18-dim
kernel, spectral gap 0.0629, exact su(2) triplet degeneracy) and the
constructed backreaction delta_omega = -kappa H_B^+ J. The backreaction
direction reproduces the Phase379 suppressed-axis fractions because the
source currents lie in the rank-3 Gram image, refining the Phase392
metric-dependence conclusion: the suppression is absent from the fermion-loop
response operator but present in the source-driven boson displacement. Still
study-defined and toy-branch: no physical coupling, scale, or contract
fields.
