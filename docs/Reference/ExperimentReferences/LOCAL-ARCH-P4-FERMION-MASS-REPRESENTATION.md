# LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION

## Source

- Ref ID: `LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION`
- Title: Phase 4 fermion inner-product and spectral architecture
- Path: `docs/Architecture/ARCH_P4.md`
- Source type: Local architecture workbench, not a primary GU source
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

The Phase 4 architecture defines a mesh-volume fermion mass matrix `M_psi`, the
weighted inner product `<x,y>_M=x^dagger M_psi y`, a generalized fermion
eigenproblem, and the branch-local coupling proxy
`<phi_i, (dD/domega)[b_k] phi_j>_M`.

The implemented discrete Dirac assembler persists an ordinarily Hermitian
matrix. Phase372 showed that directly interpreting that matrix as an
`M_psi`-self-adjoint weighted-space operator is inconsistent on the current toy
mesh. The local diagnosis is to distinguish:

- the Euclidean-Hermitian stiffness representation `K`;
- the weighted-space operator `A=M_psi^-1 K`;
- the Euclidean-Hermitian representative
  `B=M_psi^-1/2 K M_psi^-1/2`.

This distinction is a discrete implementation contract. It is not a derivation
of the physical fermionic branch of Geometric Unity.

## How It Was Used

- Used in Phase372 to compare the identity-weight reciprocal mixed-block control
  against the mesh-volume diagnostic branch.
- Used after Phase372 to interpret the nonzero mesh-volume adjoint residuals as
  a representation mismatch rather than immediate evidence that the discrete
  edge assembly must be rebuilt.
- Used to scope Phase373 as a phase-local representation-transform audit before
  any shared solver repair or physical weighted-pairing claim.
- Used in Phase374 to repair the shared weighted spectral path so it solves
  `B=M_psi^-1/2 K M_psi^-1/2`, transforms modes back with
  `psi=M_psi^-1/2 u`, and reports generalized `K psi=lambda M_psi psi`
  residuals. The persisted Phase12 replay selects kernel modes, so Phase374
  also requires an independent nonuniform-`M_psi` nonzero-spectrum benchmark.
- Used in Phase375 to replay the reciprocal source-block candidate with the
  repaired shared weighted modes under fixed-mesh connection perturbations,
  where `delta M_psi=0` and `deltaA=M_psi^-1 deltaK`. All weighted identities
  pass, but the selected Phase12 source modes remain kernel modes with zero
  nonzero currents and zero nonzero reciprocal derivatives.
- Used in Phase376 to select the complete target-blind lowest nonzero shell and
  replay projected blocks `G_k=Psi_shell^dagger deltaK_k Psi_shell`. All 24
  persisted/analytic projected blocks are nonzero and pass parity checks. The
  audit also discloses that the filtered 48-mode kernel comes from four
  isolated ambient toy-mesh vertices with fallback weight `1.0`.
- Used in Phase377 to compose the Phase376 blocks into the selected-source-mode
  response metric `Q_ab=Re Tr(G_a^dagger G_b)`. Both persisted backgrounds have
  a positive-semidefinite rank-`3` response image and nullity `9` on their
  `12`-mode selected subspaces.
- Used in Phase378 to compute the full `156`-coordinate connection-carrier
  response Gram map. The full map also has stable rank `3`, nullity `153`,
  and reconstructs the Phase377 selected response with relative residual below
  `1e-12`.
- Used in Phase379 to characterize the full-carrier positive image. The image
  is dominated by gauge axes `0` and `2`, suppresses axis `1` below `0.2%` of
  projector trace on both persisted backgrounds, and fails strict background
  transport with minimum singular value near `0.8`.
- Used in Phase380 to verify that the Phase379 response-image diagnostic cannot
  be applied directly to the Phase201 W/Z source-lineage contract. It accepts
  zero contract fields and preserves all 15 W/Z missing fields.
- Used in Phase381 to compare the Phase307 selected W/Z near-pass selector with
  the Phase379 carrier-image sidecar. The selected W row uses charged axes
  `0,1`; Phase379 suppresses axis `1` and dominates on axes `0,2`.
- Used in Phase382 to test whether existing observed-field artifacts can
  separate Phase379 carrier axes from physical photon/W/Z axes. The audit finds
  no namespace-separation map and does not rehabilitate the selected W row.
- Used in Phase383 to exhaust the current Phase307 predeclared selector space
  under the Phase379 suppressed-axis counterfactual. The audit finds no current
  W source definition or predeclared selector that avoids suppressed axis `1`.
- Used in Phase384 to test whether Phase27 basis-energy fractions provide a
  finer proxy than the Phase307 charged-axis labels. They do not: all current
  W definitions remain substantially supported on suppressed axis `1`.
- Used in Phase385 as part of the observed electroweak namespace-map intake
  candidate set. The response-image and basis-energy artifacts remain carrier
  or proxy diagnostics, not observed photon/W/Z/H projection rows or
  source-lineage theorems.
- Used in Phase388 to test whether the full VO-7 shell-response chain contains
  a theorem-level bridge from local carrier diagnostics to observed
  electroweak namespace/source rows. It does not: the current architecture
  still lacks a physical `M_psi` branch, physical Hessian, observed namespace
  map, and source/scale/unit lineages.

## Prediction Relevance

A consistent `M_psi` representation is required before a branch-local
fermionic bilinear or reciprocal mixed block can be interpreted physically. It
is one prerequisite for the larger coupled-action program, not a W/Z/H
bridge-source law.

The architecture note does not supply a fixed GU fermionic action, explicit
Yukawa functional, solved coupling map, observed-field extraction, Higgs scalar
source row, target-independent mass scale, pole extraction, or GeV
normalization.

## Limitation

Treat this as a local implementation reference. The mesh-volume weights remain
a provisional discretization choice. Phase374 validates a bounded dense shared
solver repair, Phase375 validates a fixed-mesh zero-mode control, Phase376
validates nonzero projected-shell blocks, Phase377 validates a study-defined
selected-source response metric, and Phase378 validates its full-carrier
restriction at the discrete level. Phase379 characterizes the resulting
carrier image, Phase380 rejects a direct W/Z contract application, and Phase381
shows the strongest Phase302/307 selected near-pass is not sidecar-compatible
with that image because its W row uses the suppressed axis. Phase382 further
shows that the current observed-field artifacts do not provide a
namespace-separation theorem between carrier axes and physical W/Z axes.
Phase383 shows that the existing Phase307 selector space cannot repair that
conflict internally because every current W definition uses the suppressed
charged axis. Phase384 shows that Phase27 basis-energy metadata does not repair
the conflict either: the minimum W suppressed-axis basis-energy fraction is
`0.33627838840903185`, and the selected P302-scaled common W definition has
`0.44501616745604355`. None establishes that the weights, operator branch,
carrier axes, basis-energy proxy, or image projector are physically canonical.
Phase385 confirms that these diagnostics also cannot fill the observed
electroweak namespace-map intake contract. Phase388 confirms that the combined
chain still lacks the theorem-level observed namespace/source law needed to
move from architecture diagnostics to Phase201/Phase256 source evidence. No
physical boson mass can be promoted from this convention alone.

## Follow-Up

- Preserve Phase374's nonzero-spectrum benchmark when replacing the bounded
  dense reference solver with a scalable implementation.
- Replace the isolated-vertex toy mesh before interpreting its topology kernel
  physically.
- Add explicit representation metadata before enabling mesh-volume weighting
  in production branches.
- Derive a fixed GU fermionic action and physical `M_psi` branch before using
  weighted reciprocal blocks as source-law evidence.
- Add a target-blind observed photon/W/Z/H namespace theorem before treating
  carrier-axis response data as physical W/Z/H rows.

## Phase389 Usage

Phase389 uses the persisted Phase12 explicit base Dirac matrices, the persisted
`background_states/{bg}_omega.json` connection coefficients, and the persisted
fermion modes to construct and machine-verify the discrete gauge-compatibility
identity `[D(omega), X_hat] = delta_D[v(X)] + R(X)` on the identity-weight
control branch. Two representation facts were confirmed in passing: the
persisted Dirac assembly is exactly linear in `omega`
(`D = D_kin + delta_D[omega]`, reconstruction residual exactly zero), and the
persisted fermion modes are not tight eigenmodes of the persisted explicit
base Dirac matrix (mode artifacts record `residualNorm ~ 12`), so Ward
zero-current statements cannot be sharply tested on this branch until an
`M_psi`-compatible branch with converged modes is rebuilt.

## Phase390 Usage

Phase390 rebuilds converged fermion modes on both control branches with an
in-study complex Hermitian Jacobi solver: the identity-weight branch
(eigensolve of the persisted base Dirac matrix) and the mesh-volume `M_psi`
branch (`B = M^{-1/2} D M^{-1/2}`, `v = M^{-1/2} w`, `D v = lambda M v`).
Eigen-residuals reach `3e-13`; the persisted Phase12 modes are confirmed to be
non-eigen mixtures (relative residuals `12.1-18.5`, best overlap with any
converged eigenvector `0.376`). The toy mesh's 4 isolated vertices give an
exact 48-dimensional structural kernel per branch, excluded by the
target-blind selection rule. `[M_psi, X_hat] = 0` holds exactly, so the
mesh-volume branch carries the Phase389 gauge-compatibility identity by exact
conjugation, and the sharp pure-gauge Ward zero-current statement holds on
both rebuilt branches (max current `9.3e-13`). This closes the Phase372
actionable obstruction at the control-branch level; it does not create a
physical `M_psi`-compatible GU branch.
