# PHASE8_SUMMARY.md

## What Phase VII Changed

Phase VII made the evidence pipeline more honest and more specific.

- The standard campaign no longer used a made-up background atlas file. It now uses a real atlas produced by the repository's background solver and records the exact state files it came from.
- The scorecard no longer quietly picks the first matching record. It now says which environment supplied each comparison result.
- The side evidence files now say where they came from. One channel comes directly from upstream saved data, and the others are clearly marked as bridge-derived instead of being presented as if they were direct measurements.
- Phase VII also added a companion run for a second Shiab operator choice. "Shiab" here means one of the model's operator choices, or one of the allowed ways the code turns geometry data into a field used by the solver.

## What The Upgraded Campaign Tested

The upgraded campaign tested whether the repository still gives the same broad story after those realism upgrades:

- Do the branch-stability checks still pass when their inputs come from real saved solver outputs?
- Do the refinement checks still pass on the same bridge path?
- Do the easy controls still pass while the harder benchmark still fails?
- Are the side evidence channels actually evaluated and labeled by source?
- Does a second Shiab choice keep the same branch and refinement conclusions on the current toy-control path?

## What Now Counts As Stronger Evidence

- It is stronger that the bridge inputs now come from saved solver artifacts rather than a hand-maintained synthetic atlas.
- It is stronger that the failed harder benchmark is tied to a named environment in the artifacts.
- It is stronger that the side evidence now distinguishes direct upstream evidence from bridge-derived evidence.

## What Still Does Not Count As Real-World Evidence

- The standard atlas is real and saved, but it is still a trivial control run on toy geometry. In plain English: the pipeline solved something real in repository terms, but not something scientifically demanding.
- The imported environment is still a synthetic example with fake dataset metadata.
- The harder benchmark is still an internal benchmark, not a lab measurement or outside reference dataset.
- The extra Shiab run broadens scope, but it uses the same trivial control conditions and is not yet part of the main standard campaign.

## What A Project Lead Can Decide Next

- Keep using the current Phase VII campaign as the standard repository evidence path.
- Do not describe the current outputs as physical validation or real imported-data validation.
- Spend Phase VIII effort on five things: nontrivial saved solver backgrounds, a real imported dataset, upstream observation and representation sidecars, candidate-specific promotion and demotion gates, and a nontrivial non-identity Shiab run in the main evidence path.
