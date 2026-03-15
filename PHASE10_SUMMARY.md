# PHASE10_SUMMARY.md

## What Phase IX Changed

Phase IX removed the one-variant bottleneck that limited Phase VIII.

- The standard atlas is now a real multi-variant nontrivial family: `4/7` backgrounds
  are admitted on the original thresholds and `3/7` are still rejected.
- The paired `first-order-curvature` Shiab atlas now does the same: `4/7` admitted and
  `3/7` rejected on the same strict path.
- The standard branch study is no longer inconclusive. It is now measurable and `mixed`.
- The paired Shiab branch study is also no longer inconclusive. It is now measurable and `mixed`.
- The convergence report now says plainly that its ladder is bridge-derived from four
  admitted backgrounds and is not a direct solver-backed refinement family.
- The CLI branch-robustness path now accepts the bridge exporter’s native table output,
  so the paired branch study can run directly on the exported artifact.

## What Phase IX Did Not Change

- Candidate branch/refinement escalation still does not have real candidate-linked
  evidence IDs in the executed dossier.
- The imported benchmark is still repo-internal provenance, not external evidence.
- The only quantitative miss is still an internal benchmark miss.
- The branch story is broader than before, but it is not robust: `gauge-violation` and
  `solver-iterations` remain fragile on the measured families.
- The refinement story is broader than before, but it is still bridge-derived.

## What Now Counts As Stronger Evidence

- A multi-variant nontrivial atlas is stronger than a single admitted variant because it
  can support a real branch comparison.
- A branch result labeled `mixed` is stronger than an `inconclusive` one because it is a
  measured negative or partial result, not a missing study.
- A convergence summary that states `bridge-derived` and `Direct solver-backed refinement family: no`
  is stronger than a cleaner-sounding summary that hides the evidence source.
- A paired Shiab run that is actually evaluable is stronger than one that exists only as
  a companion artifact without enough admitted variants to compare.

## What Still Does Not Count As Real-World Evidence

- The imported benchmark still does not count as external evidence because the copied
  dataset record is still repo-internal.
- The branch study still does not count as robustness evidence because both standard and
  paired paths are mixed rather than robust.
- The refinement study still does not count as direct refinement evidence because it is
  still bridge-derived from atlas exports.
- Candidate escalation still does not count as fully evidence-linked because the real
  dossier output still lacks candidate-linked branch/background evidence IDs.

## What A Project Lead Can Decide Next

- Treat Phase IX as a real evidence-breadth upgrade.
- Do not treat it as a robustness or external-validation milestone.
- Make Phase X focus on four things: real candidate-linked branch/background provenance,
  genuine external imported evidence, direct solver-backed refinement evidence, and a
  strict answer to whether the observed branch fragility can be reduced under the same thresholds.
