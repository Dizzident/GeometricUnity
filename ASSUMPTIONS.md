# ASSUMPTIONS.md

# Executable Branch Assumptions and Non-Draft Decisions

## Purpose

This file records places where the current repository makes explicit implementation
choices, simplifications, or lowerings that are not uniquely fixed by Eric
Weinstein's April 1, 2021 draft.

It is not a criticism log. It is a branch-discipline log.

If a mathematical or physical result depends on one of these items, that result
must be treated as branch-local unless and until the choice is removed, justified
as canonical, or shown invariant.

---

## 1. Scope assumptions

### A-001 Minimal bosonic branch only

The repository implements a minimal executable bosonic branch rather than the full
draft. The fermionic sector, full observed particle dictionary, and quantum
closure are out of scope for the current codebase.

Impact:
Any claim beyond bosonic branch execution is outside current validation scope.

### A-002 Research platform, not validated theory

The repository and completion manuscript both treat the codebase as a research
engine or completion program, not as a validated physical theory.

Impact:
Passing tests or producing artifacts does not establish physical correctness.

---

## 2. Geometry and discretization assumptions

### A-003 Toy geometry replaces the draft's full observerse ambiguity

Runtime entry points currently use small toy geometries, especially
`ToyGeometryFactory.CreateToy2D()`, instead of a physically motivated or
theory-derived observerse construction.

Current default:
- base mesh `X_h`: 2D toy square triangulation
- ambient mesh `Y_h`: 5D toy fiber construction

Impact:
CLI runs and reports are structural/debugging studies, not direct tests of the
full `X^4` / observerse program in the draft.

### A-004 Simplicial/P1/centroid discretization is an inserted computational branch

The executable branch uses simplicial meshes, `P1` basis functions, and centroid
quadrature as the default lowering choices.

Impact:
All residuals, Jacobians, spectra, and observed outputs are discretization-branch
 dependent.

### A-005 Observation is enforced through a specific discrete pullback pipeline

The codebase treats `sigma_h^*` pullback as the mandatory route from `Y_h` data
to `X_h` observables.

Impact:
This is a strong branch discipline and may be stricter than the original draft's
under-specified recovery language.

---

## 3. Algebra and operator assumptions

### A-006 Lie algebra support is restricted

The runtime path currently exposes a narrow set of Lie algebras, principally
`su2` and `su3`.

Impact:
Any statement about broader gauge structure in the draft is not validated by the
present implementation.

### A-007 Curvature lowering is fixed to the discrete gauge-theory form

The reference CPU branch defines curvature as:

`F = d(omega) + (1/2) [omega, omega]`

This is a reasonable executable lowering, but the draft does not fully specify
the unique discrete realization.

Impact:
Numerical agreement here is branch-consistency, not proof of canonical lowering.

### A-008 Executable augmented torsion is a branch-local lowering

The original 2021 draft gives augmented/displaced torsion at the group level as a
corrected object of the form:

`T_g = $ - epsilon^{-1} d_{A0} epsilon`

The executable CPU implementation instead lowers augmented torsion as:

`T^aug = d_{A0}(omega - A0)`

Impact:
The implementation is not a literal formula-for-formula reproduction of the draft.
It is a branch-defined computational realization.

### A-009 Active Shiab in the runtime is the simplest branch, not the manuscript's richer active branch

The original draft leaves Shiab as a family of operators. The completion
manuscript later introduces a typed Shiab family and an explicit active branch.

The current runtime CLI defaults still use:
- `trivial` torsion
- `identity-shiab` with `S = F`

rather than the richer active Shiab branch described in the completion manuscript.

Impact:
Current CLI runs validate the simplest executable Shiab branch, not the strongest
documented active branch.

Status (updated Phase VII):
The standard Phase VII evidence campaign still uses the identity-shiab branch.
A linked Phase VII companion run now exercises `first-order-curvature` on the same
real persisted toy-control atlas path, but it remains a trivial residual-inspection
control run and is not yet integrated into the main evidence campaign. Shiab scope
therefore remains only partially expanded.

### A-010 Variational residual minimization is chosen over physical time evolution

The executable branch is organized around residual/objective minimization and
stationarity solves, not a physical time-evolution model.

Impact:
Successful solves indicate branch-local residual consistency, not dynamical
realism.

---

## 4. Runtime default assumptions

### A-011 Zero initial connection is the default CLI solve state

The main `gu solve` and CPU pipeline paths default to `omega = 0` and `A0 = 0`
unless explicitly provided otherwise.

Impact:
Out-of-the-box CLI solves tend to land on the trivial zero-residual branch and
therefore are weak mathematical validation tests.

Status (updated G-002):
`gu run` / `gu solve` now emit a `[G-002] WARNING` when the trivial validation
path is detected (Mode A + zero seed) and write a `SolveRunClassification` record
to `logs/solve_run_classification.json`. The `solve-backgrounds` command also
classifies each spec. The zero-seed default remains for backward compatibility but
is no longer silent.

### A-012 Positive-definite trace pairing is often used for executable stability

Many optimization and objective-based tests use the trace pairing rather than a
sign-indefinite or convention-sensitive alternative.

Impact:
This is a practical numerical decision and should not be conflated with a unique
theory-level pairing choice.

### A-013 CPU reference code is the authoritative math implementation

The repository treats the C# CPU reference backend as the trusted implementation,
with CUDA and visualization as secondary or parity-checked layers.

Impact:
Validation currently means agreement with the CPU branch, not independent
derivation from theory.

---

## 5. Phase III and reporting assumptions

### A-014 Phase III background studies are toy-background studies

The background atlas and spectral pipeline currently operate on toy geometries and
small admissible background sets.

Impact:
Reported mode families and boson candidates are numerical toy-model outputs, not
credible physical particle identifications.

### A-015 Candidate registry output is evidence-ranked and intentionally weak

The current registry/reporting layer permits low-confidence classes such as
`C0_NumericalMode` and preserves demotions and negative results.

Impact:
This is good scientific hygiene, but it also means the pipeline is designed to
avoid claiming physical success from weak evidence.

### A-016 `compute-spectrum` currently rebuilds a zero state instead of consuming the stored solved background

The current CLI `compute-spectrum` path loads a background record for metadata,
but then constructs fresh zero `omega` and zero `A0` tensors before assembling
the operator bundle and observed signatures.

Impact:
Current Phase III spectrum artifacts are not valid evidence about the specific
stored background that the command name suggests.

Status (RESOLVED by G-004):
`compute-spectrum` now loads the persisted omega state from
`background_states/{bgId}_omega.json` and per-background manifest from
`background_states/{bgId}_manifest.json` (both written by `solve-backgrounds`).
The fallback to zero is retained only when no persisted state exists, with a
logged warning. Spectrum artifacts now represent the actual solved background.

---

## 6. Validation boundary

### A-017 Internal consistency does not imply original-theory validation

The current test suite can establish:
- code-level correctness relative to the chosen branch,
- finite-difference consistency,
- artifact/replay integrity,
- end-to-end pipeline execution.

It cannot by itself establish:
- that the original draft is mathematically complete,
- that the current branch is canonical,
- that the lowering choices are unique,
- or that the resulting outputs match physical reality.

Impact:
All current validation language should be phrased as branch-local executable
validation unless stronger evidence is added.

---

## 7. Phase V validation assumptions

### A-018 Richardson extrapolation uses a single mesh parameter proxy

Phase V (M47) Richardson extrapolation uses the `MeshParameter` field from
`RefinementLevel` directly as `h`. The formal ARCH_P5.md specification called for
`h = max(h_X, h_F)` where h_X and h_F are separate X-space and fiber-space mesh
parameters. The current implementation conflates these into one scalar.

Impact:
Convergence rate estimates and continuum extrapolations are branch-local to the
single-parameter parameterization. The distinction between X-space and fiber-space
refinement is not tracked.

Status:
Acceptable for MVP. Fix requires introducing separate h_X and h_F fields to
RefinementLevel and updating RichardsonExtrapolator.

---

### A-019 Four of seven falsifier types are reserved placeholders

Phase V (M50) FalsifierEvaluator actively evaluates three of the seven defined
falsifier types: BranchFragility, NonConvergence, and QuantitativeMismatch.
The remaining four — ObservationInstability, EnvironmentInstability,
RepresentationContent, and CouplingInconsistency — are defined as string constants
in FalsifierTypes but are not connected to any input data source.

Impact:
False-negative risk: studies with observation-pipeline instability or environment
sensitivity will not trigger falsifiers from these four types. The `FalsifierTypes`
string constants remain available for external use or future wiring.

Status:
Deferred. Input data types for these falsifiers (observation replay, environment
variance records, representation diffs) are not yet available.

---

### A-020 External quantitative targets use synthetic-toy-v1 placeholder data

Phase V (M49, M53) quantitative validation targets are not sourced from real
experimental data. The reference study uses `targetProvenance = "synthetic-toy-v1"`
and `evidenceTier = "toy-placeholder"`. Target values are eigenvalue ratios
constructed to match the toy-geometry solve outputs.

Impact:
Pull statistics and pass/fail verdicts in the reference study are self-consistent
but do not constitute evidence for or against the physical theory.

Status:
This is explicitly by design for the current evidence tier. Upgrading to
"structured" or "imported" evidence requires real background solutions and
physically-motivated observables.

---

### A-021 Phase V validation is branch-local relative to the declared branch family

All Phase V branch-independence studies (M46) measure variation within a declared
family of branch variants. The branch family is defined by the study spec and
may not span the full space of valid GU branches. Invariance within the declared
family does not imply invariance across all possible branches.

Impact:
"Branch-invariant" results should be read as "branch-invariant within the declared
family under declared tolerances." Expanding the family may change the invariance
verdict.

Status:
This is a fundamental limitation of the finite-sample branch methodology.
Always report branch family size and variant IDs alongside invariance verdicts.

---

### A-023 `solve-backgrounds` requires explicit manifest flags to use non-trivial branch operators

`solve-backgrounds` now supports `--manifest <path>` and `--manifest-dir <dir>`
flags to load branch manifests for each `BranchManifestId` declared in the study
spec. Without these flags, the command falls back to an inline default manifest
with `ActiveTorsionBranch = "trivial"` and `ActiveShiabBranch = "identity-shiab"`.

Impact:
Background atlas studies that do not pass explicit manifest files use trivial/identity
branch operators regardless of the `BranchManifestId` declared in the study spec.
A `[G-001] WARNING` is emitted to stderr when the fallback is used. Per-background
manifests are written to `background_states/{bgId}_manifest.json` so that
`compute-spectrum` can read the exact manifest used to solve each background.

Status (added G-001):
The fallback is intentional for backward compatibility. Remove it and require
explicit manifests once all study specs carry accurate manifest file paths.

---

### A-022 Claim escalation requires all six gates to pass simultaneously

Phase V claim escalation (M51) promotes a candidate by one claim class level only
if all six mandatory gates (branch-robust, refinement-bounded, multi-environment,
observation-chain-valid, quantitative-match, no-active-fatal-falsifier) pass.
Partial evidence does not promote.

Impact:
Candidates that pass five of six gates are treated identically to candidates that
pass zero. There is no partial-credit or "evidence weight" promotion path.

Status:
Conservative by design. Future work may introduce a weighted escalation pathway,
but this requires physics guidance on how to combine partial evidence.
