# Implementation P541: Complete-Lattice Integrator Transfer Control

Phase541 implements Amendment A26 as a deterministic, zero-sampling control
over the registered theta-zero SD2 operator on the lattice-canonical
extent-three complex. Its contract exact-binds the Phase533-540 evidence and
freezes the complete state/momentum menu, numerical tolerances, two integrator
ladders, resource refusal, precedence, and authority firewalls before the first
execution.

The Phase534 scalar action reconstructs exactly on the embedded three-edge
witness ray. Analytic force checks pass: the scalar derivative agrees with the
full gradient to `8.55e-16`, and an independent directional finite difference
agrees to `1.25e-10`. Both the `0.25 x 8` selected-row ladder and the
`0.0125 x 6` pilot-row ladder remain finite, reverse to machine precision, and
show the expected step-halving energy improvement on the frozen deterministic
complete-lattice state.

Transfer nevertheless fails earlier. The raw embedding has Euclidean norm
squared `3`, so its induced scalar kinetic metric is `3`, while the reduced
sampler used metric `1`. The full force also has a maximum transverse fraction
`0.3968890282526692`, so the witness ray is not invariant under the complete-
lattice flow. The terminal is `embedding-kinetic-metric-mismatch`. This is a
mapping failure, not an integrator implementation failure.

No RNG, HMC, sampling, warmup, adaptation, accept/reject decision, or
configuration retention occurred. Phase535 remains closed and unchanged. No
Phase481 pack, production default, Phase458 gate, O4 ruling, source-contract
application, production authority, physical-unit claim, or GeV claim follows;
`promotedPhysicalMassClaimCount=0`.
