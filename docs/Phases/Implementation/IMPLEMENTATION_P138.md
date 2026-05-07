# Implementation P138: Repaired-Row Coupling Transition Graph

## Status

Implemented `studies/phase138_repaired_row_coupling_transition_graph_001`.

## Purpose

P138 materializes the finite-difference coupling graph between the two P131 repaired fermion rows and checks whether it can supply the missing W/Z sector transition rule.

## Result

Terminal status:

`fermion-coupling-transition-graph-diagnostic-only`

The transition graph is materialized and contains nontrivial off-diagonal coupling evidence. The strongest off-diagonal transition is carried by `candidate-8`, but the graph is not promotable because the repaired rows still lack explicit sector labels and the off-diagonal transitions are symmetric in ordered row direction.

## Next Work

Combine the transition graph with an independent fermion sector-label rule, or derive a direction/conjugation rule that can distinguish charged-current transitions before rerunning the corrected W/Z sweep.
