# Phase567 implementation — path-ordered curvature candidate

Phase567 defines the A35 workbench curvature candidate in
`studies/phase567_path_ordered_curvature_candidate_001`. It leaves the frozen
registered assembler and Phase548 unchanged.

For the composable face path `v0 -> v1 -> v2 -> v0`, the study-local source
uses the second-order BCH expression frozen by Amendment A35. Its analytic
linearization and transpose are reusable by Phase568 only through the exact
hash-bound source file. Phase567 checks the signed correction against the
registered order, chain composition on both the A34 control mesh and the
periodic extent-three target topology, finite-difference and transpose
identities, exact quaternion transport controls, and second- versus
third-order weak-field residual slopes.

The truncated BCH expression is not represented as exactly finite-covariant;
that property belongs to the full group holonomy. A passing terminal opens
only the Phase568 workbench evaluation. It does not select a source, repair a
registered operator, authorize sampling, or support a physical mass claim.
