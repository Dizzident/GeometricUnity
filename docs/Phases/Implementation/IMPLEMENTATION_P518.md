# Implementation P518 — A5 Dual-Reflection Exact Consistency

## Status

Executed deterministically with terminal
`a5-dual-reflection-exact-consistency-dual-candidate-oriented-complex-nonclosure`.
The Phase518 contract was frozen before Phase517 consumption and exact-binds the Phase517 summary,
contract, program, and site/link candidate artifacts. `Program.cs` cross-checks
each standalone candidate against its Phase517 summary row and summary artifact
binding, then validates and derives its axis, offset, map, slice partition,
boundary rule, formal-only role, non-registration status, and A5-validation
flags before accepting the census.

## Exact finite battery

Phase518 constructs the committed lattice-canonical periodic `n=4` Kuhn mesh
under Amendment A17 and audits both validated axis-0 maps:

- site-centered: `t -> -t mod 4`;
- link-centered: `t -> 1-t mod 4`.

For each map it verifies the 256-vertex bijection and involution, then enumerates
all 3,840 edges, 12,800 faces, and 6,144 four-simplices. It records mapped and
missing counts, one exact coordinate witness for every missing-image class,
edge-orientation reversals, and twice-identity counts on the mapped restriction.

The executed census maps 2,048 of 3,840 edges, 3,072 of 12,800 faces, and zero
of 6,144 four-simplices for each candidate. This is a finite combinatorial
obstruction only. Restricted twice-identity is
reported separately and cannot be promoted to full closure. The oriented-
complex terminal follows because an oriented-complex automorphism must first
map every underlying simplex to another simplex; the observed underlying-set
nonclosure already rules that out. The phase does not claim face or
four-simplex orientation-parity results.

## Boundaries

The phase validates finite integral in-range unique coordinates, mesh
dimensions and counts, canonical edges, and unique simplex keys before using
integer indices, exact endpoint/tuple lookup, and exact edge signs. It does not
use floating tolerances, silently zero-fill
missing images, evaluate the action or OS bilinear, claim a theorem or target
counterexample, alter Phase482, create Phase515/516, or authorize sampling,
production, launch, or physical claims.

Construction remains target-blind and external review remains pending. Shared
phase-number registry, Amendment A17 plan text, traversal wiring, and
integrity-verifier assertions were completed separately from the standalone
study implementation.
