# Implementation P551: Flat-Sector Independent Spectral Adjudication

Amendment A30. Phase550 computes its own numbers and its own verdict, so the
plan required an isolated assessment surface. Phase551 holds no project
reference to Phase550 and reuses none of its code.

## Independence per quantity

The second-order form is re-derived by four-point antisymmetric extraction from
the registered joint gradient rather than from the operator's linearization
primitives. The inertia counts come from a dense LDL^T with symmetric diagonal
pivoting rather than from a tridiagonalization plus Sturm recurrence. The
extremal eigenvalues come from block subspace iteration closed by a
Rayleigh-Ritz projection rather than from Lanczos. The integer incidence rank
uses two different primes, reverse row order and a last-nonzero-column pivot.
The null basis is rebuilt from the vertex-edge incidence arrays and the lattice
index decomposition and orthonormalized by Householder QR. The homogeneous
decomposition is solved from `t = 1, 2, 3` rather than `t = 1, -1, 2`.

The plan named randomized subspace iteration; the start block is a frozen
deterministic fill because this phase must be byte-reproducible and may
construct no RNG. The algorithm family is otherwise subspace iteration and is
not the audited phase's Lanczos. This is disclosed in the contract.

## Known-answer battery, before any audited datum

Four synthetic matrices with exactly planted spectra. Two aim at the audited
phase's specific failure modes: `soft-cluster-between-rungs` puts seven
eigenvalues at `3e-08`, between the frozen rungs, so a route that reads any
single rung as the nullity is wrong by exactly seven and the plateau test must
fail; `degenerate-largest` gives the top eigenvalue multiplicity three, where a
single-vector iteration stalls. All four pass, including the expected plateau
failure.

## Repair lineage

The first registered attempt is preserved under `lineage/v1/` and is
non-citable. The adjudication caught two defects in this phase's own code and
specification.

The independent homogeneous solve used an incorrect elimination for the
degree-four coefficient. It reproduced `S(1)` exactly, so the error was invisible
in the value and appeared only as disagreement with the audited split.

The smallest-eigenvalue comparison target was not well posed, and that defect was
present in both contracts. Deflating against the constructed null basis does not
remove the assembled form's roundoff-level near-null eigenvectors, so the
deflated operator genuinely retains an eigenvalue near zero alongside the intended
gap: the audited Lanczos converged to the residue, this phase's shifted block
iteration to `0.186888 +/- 0.000371`. Both are valid a-posteriori intervals
around genuine eigenvalues, so the non-overlap was never a disagreement about the
operator. The v2 rule compares the well-posed quantity - the smallest eigenvalue
strictly above the measured flat sector - and records separately, without gating,
whether the audited iterative interval brackets it. It does not, at either
audited base point: at the origin it sits around the roundoff-level residue
instead of the gap, and at the pilot position it misses by about `3e-14`, a few
times its own width. Both are findings about the tightness and target of the
audited arm D, not disagreements about the operator.

Neither repair touched an integer comparison, a rung, the battery, a firewall, or
the terminal taxonomy, and every load-bearing agreement already held in v1.

## Scope

Confirming a measurement grants no authority and does not upgrade a fact about a
matrix into a fact about the theory. There is no adjudicator of the adjudicator:
the recursion stops here as it did at Phase549. The dense form is re-derived at a
frozen subset of two base points, recorded as a prospective scope limit rather
than a silent truncation. No quotient, no gauge fixing, no measure normalization,
no sampling, no production authority; external review remains pending and
`promotedPhysicalMassClaimCount = 0`.
