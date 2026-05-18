# Implementation P171

P171 runs a branch-stable bridge-pair census over every promoted Phase91 mode-index pair and every Phase12 variation candidate, evaluated on both sibling Phase12 backgrounds.

The experiment tests whether the current local artifacts contain a target-independent W/Z bridge source beyond the fixed Phase95 pair used by P164.

## Gates

- same candidate id across sibling backgrounds
- same ordered mode-index pair across sibling backgrounds
- minimum raw-to-target ratio at least `0.95`
- sibling-background relative spread at most `0.05`
- source identity audit required before any prediction promotion

The phase writes `branch_stable_bridge_pair_census.json` and a summary file under the phase output directory.
