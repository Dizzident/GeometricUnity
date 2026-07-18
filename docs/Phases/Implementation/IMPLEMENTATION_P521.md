# Implementation P521 — Frozen Reflection-Compatible Triangulation Census

## Status

Phase521 executed with terminal
`a5-frozen-reflection-compatible-triangulation-census-finite-dual-reflection-compatible-candidate-survives`.
It is an A18 deterministic, zero-sampling geometry census. Its contract
freezes the complete menu before scoring: extents 3 and 4, site- and
link-centered axis-zero reflections, the committed lattice-canonical
simplicial decomposition as a negative control, and the audit-authored order
complex of the periodic cubical cell poset as the only candidate.

## Exact construction and checks

The candidate's vertices are the nonempty cells of the periodic cubical cell
complex, represented exactly by an integer base coordinate and active-axis
mask. Its four-simplices are maximal inclusion chains. All lower-dimensional
simplices are generated as nonempty subsets of those chains. Cubical-cell
inclusion is checked exactly before reflection scoring. The contract
independently freezes cell-rank coefficients `1/4/6/4/1`, barycentric
f-vector coefficients `16/240/800/960/384`, and 384 maximal chains per
periodic four-cube. Generated counts match those formulas at both extents,
every cell occurs in a maximal chain, and duplicate maximal-chain generation
is rejected.

Each reflection acts on a cubical cell's axis-zero base coordinate, with the
extra minus-one displacement required when that axis is active. The phase
freezes the inactive- and active-cell formulas separately. It checks canonical
cell representation, rank-count preservation, and that the induced cell map is
a bijective involution. It then enumerates mapped and
restricted-twice-identity counts for every simplex dimension from zero through
four. Maximal-chain cover incidence and all codimension-one simplicial
incidences are separate exact batteries. It separately reruns the simplicial
checks on the current mesh negative control and refuses any nonfinite,
nonintegral, out-of-range, or duplicate committed-mesh coordinate.

At extent 3 the candidate has simplex counts
1,296 / 19,440 / 64,800 / 77,760 / 31,104 in dimensions zero through
four. At extent 4 the counts are
4,096 / 61,440 / 204,800 / 245,760 / 98,304. Every simplex maps and maps
back under both frozen reflections. The exact incidence batteries execute
699,840 checks per reflection at extent 3 and 2,211,840 per reflection at
extent 4. In contrast, the required negative controls reproduce the Phase518
pattern: at extent 4 both reflections map 2,048/3,840 edges,
3,072/12,800 faces, and zero of 6,144 top-dimensional simplices. At both
extents the negative controls map zero top-dimensional simplices.

## Boundaries

Every result is finite-only and target-blind. A surviving candidate remains
unregistered and is not selected for production. Restricted closure is not
promoted to total closure, and finite survival supplies no all-volume
embedding, orientation, boundary/pullback, measure, target-equality, or
reflection-positivity theorem. The committed mesh is not modified. External
review remains pending, no sampling or production is authorized, and
`promotedPhysicalMassClaimCount` remains zero.
