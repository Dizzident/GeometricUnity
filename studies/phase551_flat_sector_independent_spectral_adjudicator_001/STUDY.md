# Phase551 - Flat-Sector Independent Spectral Adjudication

Amendment A30. Phase550 computes its own numbers and its own verdict, so it needs
an isolated assessment surface. Phase551 holds no project reference to Phase550
and reuses none of its code.

## Independence is structural, not asserted

Every quantity is recomputed by a different algorithm family.

| quantity | audited route | this route |
|---|---|---|
| second-order form | the operator's exact linearization primitives | four-point antisymmetric extraction from the registered joint gradient |
| inertia counts | Householder tridiagonalization plus LDL^T Sturm | dense LDL^T with symmetric diagonal pivoting |
| extremal eigenvalues | Lanczos with full reorthogonalization | block subspace iteration closed by Rayleigh-Ritz |
| integer incidence rank | two primes, forward rows, first-nonzero pivot | two different primes, reverse rows, last-nonzero pivot |
| null basis | edge endpoint list, modified Gram-Schmidt | vertex-edge incidence arrays and the lattice index decomposition, Householder QR |
| homogeneous decomposition | solved from `t = 1, -1, 2` | solved from `t = 1, 2, 3` |

The plan named randomized subspace iteration. The start block is a frozen
deterministic fill instead of a sampled one, because this phase must be
byte-reproducible and may construct no RNG; the algorithm family is otherwise
subspace iteration and is not Lanczos. This is disclosed in the contract.

## The battery runs first

Four synthetic matrices with exactly planted spectra, evaluated before any
Phase550 datum is read. Two of them target the audited phase's specific failure
modes: `soft-cluster-between-rungs` places seven eigenvalues at `3e-08`, between
the frozen rungs, so a route that reads any single rung as the nullity is wrong
there by exactly seven and the plateau test must fail; `degenerate-largest`
gives the top eigenvalue multiplicity three, where a single-vector iteration
stalls and only a block method certifies a tight residual. All four pass,
including the expected plateau failure.

## Repair lineage, preserved and non-citable

The first registered attempt is preserved under `lineage/v1/` and may not be
cited. The adjudication caught two defects in this phase's own code and
specification.

The independent homogeneous solve used an incorrect elimination for the
degree-four coefficient. It still reproduced `S(1)` exactly, so the error was
invisible in the value and showed up only as disagreement with the audited split.

The smallest-eigenvalue comparison target was not well posed, and that defect was
present in both contracts. Deflating against the constructed null basis does not
remove the assembled form's roundoff-level near-null eigenvectors, so the
deflated operator genuinely retains an eigenvalue near zero alongside the
intended gap: the audited Lanczos converged to the residue and this phase's
shifted block iteration converged to `0.186888`. Both are valid a-posteriori
intervals around genuine eigenvalues, so their non-overlap was never a
disagreement about the operator. The v2 rule compares the well-posed quantity -
the smallest eigenvalue strictly above the measured flat sector - and records
separately, without gating, whether the audited iterative interval brackets it.
It does not, at either audited base point: at the origin it sits around the
roundoff-level residue instead of the gap, and at the pilot position it misses by
about `3e-14`, a few times its own width.

Neither repair touched an integer comparison, a rung, the battery, a firewall, or
the terminal taxonomy, and every load-bearing agreement already held in v1.

## What was confirmed

The threshold-free integer flat-sector lower bound of `252` and the coboundary
rank `1131`, over two different primes with a different pivot rule and a null
basis rebuilt from different mesh arrays. The inertia counts at all eight rungs,
the negative inertia, and the largest eigenvalue to the last few digits, at the
origin and at the first preserved pilot position. The measured null-basis
residual, the exact homogeneous decomposition at all six preserved positions, and
the flatness ladder including both negative controls.

The dense form is re-derived at a frozen subset of two base points, which is a
prospective scope limit recorded in the contract and in the artifact rather than a
silent truncation; the remaining base points are compared on the value-level
decomposition, which needs no dense form.

## Scope

Confirming a measurement grants no authority and does not upgrade a fact about a
matrix into a fact about the theory. There is no adjudicator of the adjudicator:
the recursion stops here, as it did at Phase549. No quotient, no gauge fixing, no
measure normalization, no sampling, no production authority, external review still
pending, `promotedPhysicalMassClaimCount = 0`.
