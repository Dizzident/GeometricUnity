# IMPLEMENTATION_P13.md

## Purpose

Phase XIII turns the Phase XII joined calculation pipeline into an evidence-grade
comparison workflow.

Phase XII proved that one run folder can produce boson spectra, fermion modes,
Dirac bundles, finite-difference coupling artifacts, and a unified registry from
the same persisted structured background family. Phase XIII must not claim
Standard Model reproduction. Its job is to remove the remaining blockers that
currently prevent an honest comparison attempt against known particle data.

## Binding Goal

At the end of Phase XIII, another agent should be able to point at one run folder
and say:

- every live command used the branch, environment, geometry, torsion, Shiab,
  A0, omega, spinor, and observation choices recorded in artifacts;
- any fallback to toy, zero, trivial, identity, proxy, or stub behavior is either
  impossible on the main path or explicitly recorded as a blocking artifact;
- boson candidates can be compared through a versioned external-descriptor path
  sourced from official particle-data references;
- fermion candidates either have full observation-chain evidence or are blocked
  from physical comparison by machine-readable proxy-observation records;
- fermion eigenmodes must pass numerical quality gates before they can influence
  comparison, coupling, family, or registry claims;
- every comparison result is preserved as compatible, incompatible,
  underdetermined, not-applicable, or blocked, with provenance and uncertainty;
- no report describes branch-local or toy-control evidence as theory-level
  validation.

## Current Starting Point

Phase XII closed these execution blockers:

- `assemble-dirac` uses persisted background context on the main path.
- spinor specs are run-derived and validated.
- `compute-spectrum` can consume canonical `solve-backgrounds` output.
- `extract-couplings` writes finite-difference Dirac variation artifacts instead
  of placeholder zero matrices.
- per-background fermion and coupling artifacts are namespaced.
- the unified registry schema validates against the emitted registry shape.

The strongest current demonstration is:

- `studies/phase12_joined_calculation_001/output/background_family`

That run is still not physical evidence. Its own report records:

- structured 2D control geometry, not draft-level observerse recovery;
- internal-profile boson comparison only;
- proxy-level fermion observation only;
- fermion residuals of order 10 and trivial chirality;
- no honest external known-value comparison.

## Implemented Repository Slice

This repository now contains the Phase XIII evidence-gate scaffold:

- generated study artifacts are ignored via `studies/**/output/` and
  `study-runs/`, with `study-runs/.gitkeep` preserving the run area;
- `studies/phase13_external_evidence_001/STUDY.md` documents the Phase XIII
  run target and writes generated artifacts outside version control;
- quantitative validation emits target coverage counts for every declared
  target;
- `validate-quantitative --fail-closed-target-coverage` records missing target
  observables as failed match records instead of silent skips;
- Phase V campaign runs use fail-closed quantitative target coverage by
  default;
- campaign spec validation rejects target tables whose observable/environment
  selectors cannot match the computed observables artifact;
- `run-boson-campaign` accepts `--external-descriptors` and refuses
  external-analogy campaigns without descriptor coverage;
- `scripts/phase13_ci_gate.sh` and `.github/workflows/ci.yml` provide the local
  and hosted build/test gate.

The remaining items below are still scientific and mathematical blockers, not
repo plumbing blockers. They should stay explicit until the code computes the
missing data rather than synthesizing it.

## Non-Negotiable Rules

1. Do not describe Phase XIII as Standard Model reproduction.
2. Do not treat dimensionless lattice proxies as physical masses, charges, or
   coupling constants unless a documented normalization map exists.
3. Do not compare any candidate against external particle data unless the
   observation chain is full-pullback or the comparison record is explicitly
   blocked as proxy-only.
4. Do not let a command instantiate trivial torsion, identity Shiab, zero A0,
   zero omega, toy geometry, or CPU-stub GPU behavior on the main path unless
   the branch manifest explicitly requests that control behavior.
5. Do not weaken falsifiers, demotions, negative-result ledgers, or provenance
   requirements to make outputs look more successful.
6. Every external target must carry source, version, retrieval/conversion
   metadata, units, uncertainty, and an evidence tier.
7. Any generated comparison report must make incompatible and underdetermined
   results as visible as compatible results.

## Primary Gaps To Close

### G1. Branch And Environment Semantics Are Not Yet Authoritative Everywhere

Current risk:

- `compute-spectrum` loads background and manifest artifacts, but still directly
  constructs `TrivialTorsionCpu` and `IdentityShiabCpu` in the live path.
- `ResolveBackgroundEnvironment` hard-codes a small set of environment IDs.
- continuation and coarse-grid-transfer background seeds still fall back to zero
  initial omega.

Impact:

- a run can claim one branch/environment in artifacts while evaluating another
  operator or geometry in code.

### G2. External Particle Data Is Not Wired Into The Executed CLI Path

Current risk:

- Phase III supports an external analogy mode in code, but the CLI default builds
  only internal profiles.
- Phase V target tables support external metadata, but existing reference targets
  remain toy-placeholder or internal benchmark evidence.
- There is no checked-in official-source adapter or fixture for PDG, CODATA, or
  HEPData-style inputs.

Impact:

- the application cannot yet run a real known-value comparison attempt.

### G3. Fermion Observation Is Proxy-Level

Current risk:

- `FermionObservationPipeline` explicitly labels current observations as
  proxy-observation and does not perform full `sigma_h^*` pullback.

Impact:

- fermion family and chirality summaries cannot be used as physical comparison
  evidence.

### G4. Fermion Numerical Quality Is Too Weak For Claims

Current risk:

- the Phase XII run produced fermion mode residuals of order 10, uniformly
  trivial chirality, and no conjugation pairs.

Impact:

- even with external data, fermion outputs should be blocked from claim
  escalation until eigenpair quality, chirality, and conjugation diagnostics are
  numerically credible.

### G5. Geometry Evidence Is Still Toy-Control

Current risk:

- reports mechanically block X^4/observerse recovery claims for current
  low-dimensional geometry.
- the existing classifier treats dimension matching as draft-aligned, but Phase
  XIII should distinguish dimension-compatible scaffolds from a real observerse
  construction.

Impact:

- current structured environments are useful controls, not physical validation
  environments.

### G6. Falsifier Inputs Are Not Fully End-To-End

Current risk:

- all seven falsifier evaluators exist, but observation, environment,
  representation, and coupling evidence are only as strong as the sidecar inputs
  provided by upstream pipelines.
- quantitative validation can skip targets with no matching computed observable
  unless the campaign treats target coverage as an explicit gate.

Impact:

- false-negative risk remains if comparison campaigns omit sidecars or silently
  skip unmatched external targets.

### G7. GPU Evidence Is Still Native Parity, Not Device Acceleration Proof

Current risk:

- CUDA native parity paths exist, but some paths are host-side parity code rather
  than real device kernels, and the legacy Phase IV acceleration facade is still
  stub-only.

Impact:

- no accelerated result should be trusted as GPU evidence yet.

### G8. Evidence Gates Are Not Enforced By CI

Current risk:

- there is no checked-in CI workflow for full build, full tests, schema
  validation, study spec validation, and fail-closed script execution;
- some study/batch scripts can continue after failures.

Impact:

- validation readiness depends on manual discipline rather than an enforceable
  repository gate.

## Milestones

### P13-M1 Runtime Context Authority

Implement one shared runtime context resolver used by background solving,
spectrum assembly, Dirac assembly, coupling extraction, registries, and
validation campaigns.

Required behavior:

- resolve branch manifest, geometry, environment record, A0, omega, torsion,
  Shiab, algebra, spinor spec, and observation branch from artifacts;
- fail loudly when required artifacts are missing or dimension-mismatched;
- allow control fallbacks only behind explicit flags such as
  `--allow-control-fallback`;
- write a `runtime_context.json` artifact consumed by downstream commands.

Definition of done:

- `compute-spectrum` no longer directly constructs trivial/identity operators
  unless the manifest declares those branches;
- hard-coded environment ID resolution is replaced by artifact-driven loading;
- tests cover mismatched manifest/operator/environment cases;
- Phase XII reproduction still runs when it explicitly declares control choices.

### P13-M2 External Reference Data Adapters

Add source adapters and checked-in small fixtures for official external reference
data.

Initial sources:

- PDG API or downloaded PDG SQLite/JSON for particle properties;
- NIST CODATA constants for conversion constants and unit normalization;
- optional HEPData-style fixture support for later datasets.

Required behavior:

- preserve source URL, edition/version, retrieval date, content hash, license or
  terms note, unit, uncertainty, and conversion steps;
- convert external data into existing `ExternalTarget`,
  `ExternalAnalogyDescriptor`, or a new typed particle-reference schema;
- label all imported targets as external-reference, not validation success.

Definition of done:

- tests verify deterministic conversion from PDG/CODATA fixtures;
- malformed units or missing uncertainties block ingestion;
- generated target tables validate against schemas and include provenance.

### P13-M3 External Boson Descriptor Campaign

Wire the external boson path into the CLI and reports.

Required behavior:

- add CLI support for external descriptor files in `run-boson-campaign`;
- support descriptor classes for photon-like, gluon-like, W/Z-like, Higgs-like,
  and blocked/underdetermined cases;
- compare only observable quantities that the GU pipeline actually computes;
- keep internal profiles as controls, but never mix them with external results
  without a report-level evidence-tier split.

Definition of done:

- one executed run produces both internal-control and external-reference boson
  campaign artifacts;
- all external boson results are preserved, including negative and blocked
  records;
- reports identify which target values came from PDG/CODATA fixtures and which
  were internal controls.

### P13-M4 Fermion Full-Observation Contract

Replace proxy-only fermion observation with a two-lane contract.

Required behavior:

- `FullPullbackObservation`: uses explicit `sigma_h^*` provenance from native
  Y-space fermion data to observed X-space summaries;
- `ProxyObservation`: remains available but is barred from physical comparison;
- every fermion candidate receives an `ObservationChainRecord`;
- the unified registry records observation path and confidence separately from
  spectral/family clustering.

Definition of done:

- reports cannot count proxy-only fermion summaries as external comparison
  evidence;
- tests verify that proxy observations produce blocked physical comparison
  records;
- full-pullback observation, if not yet mathematically possible, emits a clear
  blocker artifact rather than synthetic values.

### P13-M5 Fermion Numerical Quality Gates

Introduce numerical gates before fermion outputs influence comparison or claim
escalation.

Required behavior:

- compute normalized eigenpair residuals, orthogonality defects, Hermiticity
  checks, chirality stability, conjugation pairing stability, and refinement
  stability;
- mark each fermion mode as quality-pass, quality-warning, or quality-fail;
- prevent quality-fail modes from promoting family, registry, coupling, or
  comparison claims.

Definition of done:

- the Phase XII fermion residual pattern becomes an explicit quality-fail or
  quality-warning result;
- coupling atlases record whether their fermion modes passed quality gates;
- tests cover residual thresholds and propagation into registry demotions.

### P13-M6 Geometry Evidence Ladder

Refine geometry evidence labels and add the next geometry scaffold.

Required labels:

- `toy-control`: current low-dimensional controls;
- `structured-control`: larger structured controls without observerse claim;
- `dimension-compatible-control`: dim(X)=4 and dim(Y)=14 but missing full
  observerse derivation;
- `observerse-complete`: full required observation geometry and pullback data;
- `physical-comparison-ready`: observerse-complete plus normalization and
  external target compatibility.

Required behavior:

- dimension matching alone must not imply draft-level validation;
- reports must state which geometry gate blocks physical comparison.

Definition of done:

- existing reports still classify Phase XII as control evidence;
- a minimal 4D/14D scaffold can be generated and run as
  `dimension-compatible-control`;
- no candidate can be promoted on dimension-compatible geometry alone.

### P13-M7 Candidate-Linked Evidence Dossier

Make every candidate claim trace to concrete evidence IDs.

Required behavior:

- link each unified registry candidate to branch robustness, refinement,
  observation, external comparison, representation, coupling consistency, and
  falsifier records;
- record missing evidence as missing, not zero or pass;
- emit one machine-readable dossier per candidate plus a study-level summary.

Definition of done:

- candidate escalation gates no longer have empty evidence ID lists;
- no candidate can pass a gate without source artifact IDs;
- reports expose why each candidate is blocked, demoted, or eligible.

### P13-M8 Fail-Closed Validation Gate

Make missing evidence a failing validation condition unless explicitly waived.

Required behavior:

- unmatched external targets become blocked or failed target-coverage records,
  not silent skips;
- missing sidecar channels become explicit coverage failures;
- solver freshness is checked by hashes from configs, input artifacts, source
  states, and generated values;
- solver convergence or numerical quality failures propagate into campaign and
  dossier results.

Definition of done:

- tests cover missing target observables, stale value tables, missing sidecars,
  and unconverged solver records;
- study scripts stop on validation failures;
- reports include target coverage counts, skipped target IDs, waiver IDs, and
  failure reasons.

### P13-M9 CI And Schema Gate

Add repository-level automation for evidence readiness.

Required behavior:

- build the full solution;
- run all test projects;
- validate JSON schemas for checked-in studies and generated reference fixtures;
- validate Phase XIII study specs before execution;
- run lightweight artifact freshness checks;
- fail on masked test/script errors.

Definition of done:

- a checked-in CI workflow exists;
- the local equivalent command is documented;
- failures in tests, schemas, target coverage, or stale artifacts block the gate.

### P13-M10 End-To-End Evidence Run

Produce a single reproducible Phase XIII run.

Required behavior:

- run from checked-in config and explicit artifacts only;
- execute background solve, boson spectra, fermion modes, couplings, full or
  blocked observation, external-reference ingestion, comparison campaigns,
  falsification, and dossiers;
- validate JSON schemas and replay/integrity checks.

Definition of done:

- checked-in `studies/phase13_external_evidence_001/STUDY.md`;
- generated report says exactly which physical-comparison gates are open or
  closed;
- the report includes no Standard Model reproduction claim.

### P13-M11 GPU Trust Boundary

Keep GPU acceleration out of evidence claims until real device proof exists.

Required behavior:

- separate native-host parity, CPU reference, and real CUDA device execution in
  artifacts;
- deprecate or rewire the legacy Phase IV GPU stub facade;
- record CUDA hardware, driver, toolkit, kernel path, and parity tolerance when
  used.

Definition of done:

- no artifact can say GPU-verified when the path is host parity or CPU stub;
- real hardware parity tests are optional for Phase XIII success but mandatory
  before any GPU-backed comparison claim.

## Recommended Execution Order

1. P13-M1 Runtime Context Authority.
2. P13-M5 Fermion Numerical Quality Gates.
3. P13-M4 Fermion Full-Observation Contract.
4. P13-M2 External Reference Data Adapters.
5. P13-M3 External Boson Descriptor Campaign.
6. P13-M6 Geometry Evidence Ladder.
7. P13-M7 Candidate-Linked Evidence Dossier.
8. P13-M8 Fail-Closed Validation Gate.
9. P13-M9 CI And Schema Gate.
10. P13-M10 End-To-End Evidence Run.
11. P13-M11 GPU Trust Boundary, in parallel where hardware is available.

## Success Criteria

Phase XIII succeeds if the application can run an honest external-reference
comparison campaign and preserve the result even when the likely result is
blocked, underdetermined, or incompatible.

The target outcome is not "match observed particles." The target outcome is a
reproducible machine-readable answer to:

- what was computed;
- under which branch and geometry choices;
- what was observed through full pullback versus proxy;
- what was compared against external references;
- whether every external target was matched, waived, blocked, or failed;
- which candidates failed, passed, or were blocked;
- which falsifiers are active;
- what exact mathematical or numerical gap remains before a physical claim could
  be considered.
