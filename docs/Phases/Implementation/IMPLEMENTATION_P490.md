# Phase490 Implementation

Phase490 implements Amendment A7's committed-evidence zero-mode quotient
audit. It hashes the Phase428, Phase430, and Phase455 programs and summaries,
then independently constructs the Gell-Mann half-basis, the three audited ray
matrices, and the adjoint commutator Gram operator. The reconstructed
fundamental and adjoint spectra must match the committed exact spectra.

The audit separately records geometric zero momentum and the three additional
vanishing-sine tuples of the naive lattice. At generic nonzero background
parameter it counts the exact fundamental-fermion, adjoint-fermion, and
bosonic-adjoint kernels for every ray axis.

Gauge-volume classification requires more than spectral nullity. The
repository contains an explicit connection-space generator image and
projector, plus code capable of applying a supplied fermion projector, but it
does not contain a committed intertwiner or derived projector for the
Phase455 determinant mode bases. The quotient-aware spectral formulation is
also explicitly unimplemented, and the prior fermion-lift artifact does not
claim a nontrivial fermion quotient. These domain-mismatched artifacts are
hashed and reported without transferring their classifications.

The exact taxonomy is `unique-quotient-derived`, `quotient-family-derived`,
`quotient-underdetermined`, or `invalid-committed-inputs`. Valid inputs with
no compatible generator-image derivation produce the valid green scientific
terminal `zero-mode-quotient-audit-quotient-underdetermined`; uniqueness is
never forced.

The result remains target-blind exploration. It does not alter Phase455,
author a human or O4 ruling, satisfy or evaluate Phase458, authorize sampling
or production, fill a source contract, or support a physical-unit claim.
`noGevPromotion=true` and `promotedPhysicalMassClaimCount=0` on every branch.
