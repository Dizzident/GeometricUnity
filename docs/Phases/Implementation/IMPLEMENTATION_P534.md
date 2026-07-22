# Phase 534 implementation

Phase534 runs the A22 controls in preregistered order. Independent Gaussian
draws and the free-field HMC baseline pass under both seed tables. The
registered theta-zero SD2 witness ray reconstructs a quartic action; its cubic
coefficient reproduces the independent exact Phase527 value, and deterministic
nested quadrature closes.

The reduced interacting HMC produces non-finite and divergent trajectories
under the frozen warmup adaptation. Moment and integration-by-parts checks
otherwise agree, but execution diagnostics take precedence. The preserved
terminal is reduced-interacting-control-failed.

This result does not validate the complete lattice action, dynamical theta, or
production sampling and makes no physical claim.
