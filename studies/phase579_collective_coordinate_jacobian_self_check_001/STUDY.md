# Phase 579: collective-coordinate Jacobian self-check

Phase579 executes the one Phase485 falsifier that had remained defined but
unrun: `O4-F1-COLLECTIVE-COORDINATE`, method
`collective-coordinate Jacobian and solvable-limit comparison`. It exact-binds
the Phase450 contract, implementation, and full/summary binding-condition
record, then independently reconstructs Phase450's committed `n=3`
lattice-canonical coordinate.

The check is deterministic and performs no sampling. `Random(20260703)` is
used only because those exact pseudo-random bytes are part of the registered
Phase450 ray definition; no random variable is drawn for inference.

## Checks

1. Rebuild the 15 edge-type oSign ray, apply the registered global-`su(2)`
   tangent projection, and reproduce the committed dimensions and ray norms.
2. Evaluate the analytic Jacobian `dPhi/domega = u_inv` and coarea factor
   `||grad Phi||`, and compare every component with centered finite differences
   at three frozen step sizes.
3. Compare the constrained construction with the closed-form isotropic free
   Gaussian, including its constant coarea factor, using an independently
   assembled collective-plus-transverse basis and deterministic tensor
   quadrature in a four-dimensional fixture.
4. Check all 81 exact lattice translations and the three registered
   global-orbit tangents at machine precision. For the decisive gauge test,
   apply exact finite global adjoint rotations actively to `omega` while
   holding Phase450's registered `u_inv` fixed. Simultaneously rotating both
   objects is retained only as a passive-covariance diagnostic and cannot
   satisfy the active invariance gate.

Passing is evidence for the registered reduced-setting convention only. It
does not author an O4 ruling, discharge external review, repair or reinterpret
Phase450, establish a Faddeev-Popov normalization, or promote any physical
claim. Failure is retained as a first-class negative against the Phase450
lineage convention. `mayAuthorRuling:false` and
`promotedPhysicalMassClaimCount=0` hold on every terminal.

## Executed result

The terminal is `phase450-lineage-convention-defective-preserved-negative`.
The reconstruction, all-component finite-difference Jacobian, constrained
free-Gaussian comparison, translations, passive adjoint covariance, and
infinitesimal tangent projection all pass. The decisive active finite gauge
test fails: fixed `u_inv` coordinate changes reach `1.809e-2` on the frozen
generic state and `9.157e-1` on the registered ray, against `5e-14`.
Machine-zero tangent overlap is therefore only infinitesimal evidence, and
simultaneous ray/field rotation is only passive covariance. Phase450 remains
byte-unchanged; this phase records the defect without repairing or
reinterpreting the committed result.
