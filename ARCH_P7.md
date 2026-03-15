# ARCH_P7.md

## Purpose

Phase VII starts from the first standard campaign that is genuinely evidence-bearing in
repository terms, not just contract-complete. The campaign rerun at:

- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase6_evidence_campaign/20260315T022228Z`

now proves all of the following at once:

- branch robustness is evaluated on bridge-backed quantities from a persisted background atlas,
- convergence is evaluated on the same bridge-backed quantity family,
- sidecar channels are evaluated rather than omitted or skipped,
- the typed dossier exposes observation coverage, falsifier coverage, and sidecar coverage,
- quantitative comparison can now distinguish clean control-study matches from a stronger benchmark miss,
- claim escalation is exercised on a non-empty registry.

That is the architectural floor for Phase VII. Phase VII is not about rebuilding the
campaign shell again. It is about improving the scientific quality of the evidence that
now flows through that shell.

## Verified Starting State

The post-Phase-VI evidence run produced:

- branch robustness: `5/5` robust quantities,
- convergence: `5/5` convergent quantities,
- quantitative scorecard: `7` passing matches and `1` failing stronger benchmark,
- falsification: `2` active falsifiers (`1` fatal representation-content, `1` high quantitative mismatch),
- sidecar coverage: all four channels evaluated,
- claim escalation: `2` promotions and `1` demotion.

The repository therefore already has:

- a strict campaign validator,
- bridge export in the standard path,
- evaluated sidecars in the standard path,
- multi-environment records in the standard path,
- dossier/report output that a non-developer can read without source inspection.

## Architectural Boundary

The new campaign is evidence-bearing, but it is still not evidence-grade in the
scientific sense required for real validation claims.

The decisive remaining boundary is not tooling completeness. It is input realism.

Specifically:

- the bridge-backed atlas is a checked-in synthetic reference atlas, not a solver-generated
  scientific atlas,
- the imported environment record is still synthetic-example provenance,
- observation / representation / coupling sidecars are derived heuristically from registry
  and observable inputs rather than from upstream persisted observation and coupling artifacts,
- the stronger quantitative benchmark is still an internal benchmark, not an experimental dataset,
- quantitative matching is still environment-agnostic in scoring because the scorecard takes
  the first observable per `observableId`,
- escalation gates remain mostly global rather than candidate-specific for branch/refinement
  and quantitative evidence,
- identity-only Shiab still dominates the main path.

Phase VII therefore targets evidence quality, specificity, and realism.

## Workstreams

### W1. Replace Synthetic Bridge Inputs With Solver-Generated Upstream Artifacts

The bridge path now works and is exercised. Phase VII must replace the checked-in synthetic
reference atlas with a persisted atlas emitted by the real upstream solve/background pipeline.

Required outcome:

- the standard campaign consumes a real persisted atlas,
- the bridge manifest points to real source record IDs and persisted state artifacts,
- the branch/refinement evidence path stops depending on synthetic reference metrics.

### W2. Replace Synthetic Imported Environment With Genuine Imported Geometry

The imported environment contract is implemented and exercised, but it is still a synthetic
example. Phase VII must connect the standard campaign to at least one genuine imported
geometry dataset with real provenance fields.

Required outcome:

- imported environment evidence refers to an actual external dataset,
- dataset provenance is concrete and reproducible,
- environment-side evidence can distinguish toy/structured/imported behavior on real inputs.

### W3. Promote Sidecars From Heuristic Derivation To Upstream Evidence Products

Sidecars are now evaluated, but their current records are derived from registry and observable
shapes rather than from upstream observation/coupling artifacts.

Required outcome:

- observation-chain records come from persisted observation provenance,
- representation-content records come from explicit mode-family/representation artifacts,
- coupling-consistency records come from actual coupling proxy outputs,
- environment-variance records are derived from true per-environment quantitative runs,
- dossier language can distinguish measured upstream evidence from inferred placeholder records.

### W4. Make Quantitative Evaluation Environment-Aware And Benchmark-Honest

The current scorecard mixes control and stronger targets successfully, but the matching logic
still collapses multiple environment-specific observables down to the first record per
`observableId`.

Required outcome:

- the scorecard is explicit about which environment/run supplied each match,
- stronger targets are matched against the intended environment tier,
- internal benchmarks and real external benchmarks are clearly separated,
- a failed stronger benchmark is preserved as a first-class result rather than hidden by
  control-study success.

### W5. Expand Beyond Identity-Only Shiab In The Main Evidence Path

Branch and convergence evidence remain local to the identity-only Shiab slice.

Required outcome:

- at least one non-identity Shiab branch appears in the standard campaign or a clearly linked
  companion evidence campaign,
- Phase VII artifacts state whether branch/convergence conclusions survive that extension.

## Exit Criteria

Phase VII is complete only when all of the following are true:

- the standard campaign is fed by real persisted upstream artifacts rather than a synthetic
  bridge atlas,
- imported-tier evidence is backed by real dataset provenance,
- sidecar evidence channels are sourced from upstream evidence products rather than heuristic
  derivation,
- quantitative scoring is environment-aware and target provenance is explicit in artifacts,
- stronger benchmark failures or passes are interpretable per environment and per candidate,
- the main evidence path is no longer identity-only in Shiab scope.
