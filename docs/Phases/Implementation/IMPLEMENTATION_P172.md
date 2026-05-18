# Implementation P172

P172 evaluates the full Phase12 variation-candidate subspace as a possible W/Z bridge source.

The phase computes, for each ordered mode-index pair, the target-independent norm:

`sqrt(sum_k |<mode_i | variation_k | mode_j>|^2)`

The same pair is evaluated on both sibling Phase12 backgrounds. A candidate is only useful for the next prediction attempt if it passes:

- minimum raw-to-target ratio at least `0.95`
- sibling-background relative spread at most `0.05`
- later source-identity audit before promotion

This broadens P171 from single-matrix bridge sources to the complete current variation subspace without fitting target residuals.
