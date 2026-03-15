# ARCH_P9.md

## Purpose

Phase IX starts from the completed Phase VIII run at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z`

Phase VIII made the standard evidence path more honest again, but it also surfaced the
next realism boundary clearly: the pipeline can now distinguish nontrivial runs,
upstream sidecars, and internal benchmarks, yet it still cannot produce a broad
multi-variant nontrivial atlas or real external evidence.

## Verified Starting State

The repository now proves all of the following from generated artifacts:

- the standard atlas is no longer mislabeled as a trivial residual-inspection run,
- the imported environment record now carries a real checked-in mesh path, dataset id,
  source hash, and conversion version,
- all four sidecar channels are upstream-sourced in the standard run,
- scorecards explicitly separate controls from internal benchmarks,
- candidate escalation artifacts now record candidate-specific evidence IDs,
- a paired `first-order-curvature` Shiab run exists on the same nontrivial solve path.

## Architectural Boundary

The remaining boundary is no longer provenance labeling. It is evidence sufficiency.

The decisive remaining limits are:

- the standard nontrivial atlas admits only one background and rejects three,
- the paired non-identity Shiab atlas also admits only one background and rejects three,
- the standard and paired branch studies are therefore inconclusive,
- the imported benchmark is real in-repo provenance but still not external evidence,
- branch and refinement claim gates fail closed because the registry does not expose
  candidate-linked branch/background provenance,
- convergence remains a bridge-derived ladder seeded from one admitted background rather
  than a richer nontrivial family.

## Workstreams

### W1. Multi-Variant Nontrivial Atlas

Required outcome:

- admit at least two nontrivial backgrounds in the standard atlas without relaxing the
  current admissibility tolerances,
- preserve rejection records for any seeds that still fail,
- regenerate the branch bridge from those admitted nontrivial variants.

### W2. Candidate-Linked Branch And Refinement Provenance

Required outcome:

- registry candidates or companion evidence products carry explicit links to branch
  variant ids and background ids,
- branch and refinement escalation gates cite candidate-linked evidence IDs instead of
  failing with empty evidence lists,
- dossier output distinguishes "gate failed because evidence disproved it" from
  "gate failed because no candidate-linked provenance exists."

### W3. Real External Imported Evidence

Required outcome:

- imported-tier benchmark(s) point to data outside the repository rather than to a
  checked-in internal mesh,
- copied inputs preserve the real external dataset provenance,
- summaries can finally distinguish internal benchmark misses from true external misses.

### W4. Broader Shiab Scope With More Than One Admitted Variant

Required outcome:

- the paired `first-order-curvature` run admits more than one nontrivial background,
- the paired branch study becomes evaluable rather than inconclusive,
- final interpretation states whether Shiab scope expansion changes any conclusions.

### W5. Convergence Realism Beyond Single-Background Bridge Extrapolation

Required outcome:

- refinement evidence is anchored to a stronger nontrivial family than a single admitted
  zero-valued branch variant,
- convergence summaries distinguish synthetic bridge corrections from direct solver
  evidence when applicable.

## Exit Criteria

Phase IX is complete only when all of the following are true:

- the standard atlas admits a nontrivial multi-variant family,
- branch conclusions are evaluable on that family,
- candidate branch/refinement escalation gates are backed by candidate-linked evidence ids,
- imported-tier evidence includes at least one real external dataset,
- the paired non-identity Shiab path is also evaluable on a nontrivial multi-variant family.
