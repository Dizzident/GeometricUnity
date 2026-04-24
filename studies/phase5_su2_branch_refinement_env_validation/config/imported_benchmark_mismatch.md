# Imported Benchmark Mismatch Diagnosis

Phase XIV preserves the current `bosonic-mode-2-imported-repo-benchmark`
failure as a negative result.

Current comparison:

- observable: `bosonic-eigenvalue-ratio-2`;
- environment: `env-imported-repo-benchmark`;
- computed value: `0.98`;
- target value: `0.6`;
- computed uncertainty: `0.05`;
- target uncertainty: `0.05` upper-side sigma from the asymmetric Gaussian
  target;
- pull: `5.37401153701776`;
- threshold: `5.0`.

Diagnosis:

- environment selection is correct: the target requests
  `env-imported-repo-benchmark` and tier `imported`, and exactly one computed
  observable matches that selector;
- observable selection is correct: the matched record is
  `bosonic-eigenvalue-ratio-2` from `observables.json`;
- uncertainty accounting is working as designed: the target uses
  `gaussian-asymmetric`, the computed value is above the target, and the matcher
  uses the target's upper uncertainty of `0.05`;
- the pull calculation is therefore
  `abs(0.98 - 0.6) / sqrt(0.05^2 + 0.05^2) = 5.37401153701776`.

Conclusion:

This is not a campaign wiring bug. With the current artifacts, the mismatch is a
target-definition or model-disagreement problem. It should not be repaired by
widening uncertainty just to pass the scorecard.

Closure requirement:

1. Find the provenance for target value `0.6` in
   `imported-repo-benchmark-v1`.
2. Confirm whether that target is supposed to be the same normalization as the
   computed `bosonic-eigenvalue-ratio-2 = 0.98`.
3. If the target definition is wrong, replace it with a provenance-backed target
   and record the correction.
4. If the target definition is right, preserve this as a genuine model
   disagreement and keep the failure active.
