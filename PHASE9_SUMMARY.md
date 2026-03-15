# PHASE9_SUMMARY.md

## What Phase VIII Changed

Phase VIII made the evidence pipeline stricter and more honest.

- The standard atlas is no longer presented as a trivial zero-seed inspection. The solver now records these runs as real objective solves, and the nonzero ansatz seeds are labeled explicitly.
- The imported-environment path now uses a real file inside the repository with a real content hash and import record, instead of fake placeholder dataset metadata.
- The side evidence files are stronger. All four sidecar channels now come from saved upstream artifacts rather than from bridge-only reconstruction.
- The scorecard now separates easy controls from harder internal benchmarks.
- The claim-escalation records now attach candidate-specific evidence IDs instead of borrowing success from the whole campaign.
- The extra Shiab run now uses the same nontrivial solve setup as the standard path.

## What The Upgraded Campaign Tested

The upgraded campaign asked four harder questions than Phase VII:

- Does the standard atlas still look acceptable once the run is a genuine objective solve instead of a trivial inspection?
- Do the side evidence channels still support the story when they must point back to saved upstream artifacts?
- Does the harder imported benchmark still fail once it is clearly marked as an internal benchmark instead of being blurred together with controls?
- Does the same story survive when the Shiab operator choice is broadened to `first-order-curvature`?

`Shiab` here is one of the model's operator choices: one of the allowed ways the code turns geometry information into a field used by the solver.

## What Now Counts As Stronger Evidence

- A background solve labeled `objective-solve` with explicit seed provenance is stronger than a residual check on a zero seed.
- A sidecar labeled `upstream-sourced` is stronger than one reconstructed only from bridge summaries.
- A benchmark labeled `internal-benchmark` is stronger than a toy control because it is meant to be harder, even though it is still not external evidence.
- A candidate gate with explicit evidence IDs is stronger than a gate result that only says the campaign passed somewhere.

## What Still Does Not Count As Real-World Evidence

- The imported benchmark is still not outside evidence. It is a real imported file path in the repository, not a lab measurement or outside dataset.
- The standard atlas is still toy-geometry evidence, and only one nontrivial background survives admission. That means the branch study is inconclusive, not robust.
- The paired first-order Shiab run has the same limit: it is nontrivial now, but it still collapses to one admitted background and cannot support a real branch comparison.
- The convergence story is still based on bridge-derived refinement values seeded from one admitted branch, not on a broad nontrivial family.

## What A Project Lead Can Decide Next

- Treat Phase VIII as a real honesty upgrade, not as a physics-validation milestone.
- Keep the new sidecar, benchmark-separation, and candidate-evidence behavior as the repository standard.
- Do not claim branch robustness for the standard or paired Shiab paths until the atlas admits more than one nontrivial variant.
- Make Phase IX focus on four things: admit a multi-variant nontrivial atlas, add candidate-linked branch/background provenance, bring in a true external imported dataset, and make the paired Shiab path evaluable rather than inconclusive.
