## Prototype Roadmap

This section converts the Minimal GU v1 implementation strategy into a staged prototype program with explicit prerequisites, deliverables, and exit conditions. Its purpose is to prevent the computational effort from drifting into either premature optimization or premature phenomenology. That sequencing is required by the completion document itself: executable mathematics must proceed from formalization, computable objects, discretization, simulation architecture, high-performance implementation, visualization, and only then to comparison-to-observation and falsification.   

The roadmap also follows the document’s broader scope discipline. The completion effort is explicitly about **internal formal completion**, **computational readiness**, and **falsifiability readiness**, not about asserting that the full physical theory has already been established.  Therefore each milestone is framed as a prototype stage with measurable completion criteria rather than as a claim of theoretical success.

### 1. Roadmap philosophy

The prototype sequence is governed by four rules.

First, no milestone may rely on a theory object that has not yet been fixed in the Minimal GU v1 branch. This follows from the validation protocol’s requirement that every outward-facing claim have a fixed formal source and declared branch before it enters comparison.  

Second, no external observation comparison may occur until the output has been converted into a typed observational product with an explicit observable map, comparison rule, and falsifier. 

Third, symbolic correctness must precede GPU acceleration. The computational chapters explicitly include “validation against symbolic reference implementation” before performance milestones are treated as meaningful. 

Fourth, the roadmap must preserve branch traceability. The ambiguity register and the validation pipeline both require that free choices, inserted assumptions, and branch-specific completions remain explicit rather than disappearing into code defaults.  

### 2. Deliverable structure

Each milestone below is specified in the same format:

* **Inputs**: what must already exist before the milestone begins.
* **Outputs**: the concrete artifacts produced.
* **Completion criteria**: the conditions under which the milestone counts as finished.
* **Failure mode if incomplete**: what it means if the milestone cannot be closed.

This format is intentional. The completion document already distinguishes structural, dynamical, phenomenological, and simulation falsifiers; the roadmap therefore needs milestone-level completion tests that can fail cleanly without being confused with falsification of the entire theory program. 

---

## Milestone 0 — Branch Freeze and Metadata Baseline

This milestone fixes the exact Minimal GU v1 branch that will be implemented.

### Inputs

* Minimal GU v1 Specification
* Mathematical-to-Computational Lowering
* C# / CUDA / Vulkan Implementation Strategy
* the current ambiguity register and inserted-assumption list

These inputs are necessary because the completion scaffold explicitly warns that multiple branches of completion remain possible and that some free choices materially affect outputs. 

### Outputs

* a branch manifest containing:

  * structure group (H),
  * discrete domain choice for (Y_h),
  * observation branch (\sigma_h),
  * torsion branch identifier,
  * Shiab branch identifier,
  * gauge strategy,
  * residual norm convention,
  * operator-adjoint convention,
  * and provenance version identifiers;
* a machine-readable schema for run metadata;
* a registry of inserted assumptions required for the chosen branch.

### Completion criteria

Milestone 0 is complete only if every future simulation-relevant object has a unique branch definition and no unresolved “to be chosen later” item remains in the v1 bosonic path.

### Failure mode if incomplete

If this milestone cannot be closed, the implementation does not yet have a unique formal source. Under the validation protocol, no downstream output would then be admissible for comparison. 

---

## Milestone 1 — Symbolic Domain Model in C#

This milestone creates the typed symbolic representation of Minimal GU v1.

### Inputs

* branch manifest from Milestone 0
* normalized object definitions from the completion document
* operator and bundle signatures for Minimal GU v1

### Outputs

* C# domain classes for:

  * (X), (Y), (\pi), (\sigma),
  * principal bundle and adjoint bundle metadata,
  * connection state,
  * curvature, torsion, Shiab, and residual objects,
  * gauge configuration,
  * observable output descriptors,
  * branch metadata and provenance;
* serialization formats for all core objects;
* a symbolic reference API for evaluation on toy inputs.

This directly implements the “symbolic data model” and “algebraic objects to encode” called for in the executable-mathematics chapter. 

### Completion criteria

Milestone 1 is complete only if every object in the Minimal GU v1 computational state has a typed symbolic representation with versioned serialization and explicit ownership by the orchestration layer.

### Failure mode if incomplete

If symbolic object ownership is unclear, the later interop and GPU layers will blur branch semantics and provenance.

---

## Milestone 2 — Discrete Geometry and Observation Scaffold

This milestone lowers the continuous spaces and maps into discrete numerical carriers.

### Inputs

* symbolic domain model from Milestone 1
* chosen discretization family for (Y_h)
* chosen observation discretization for (X_h)

### Outputs

* discrete mesh, lattice, or finite-element representation of (Y_h);
* discrete observation domain (X_h);
* discrete projection map (\pi_h);
* discrete observation section (\sigma_h);
* quadrature data, local metric samples, and interpolation rules;
* visualization-ready geometry export for both (X_h) and (Y_h).

This milestone corresponds to the completion chapters on computable geometric objects, discretization strategy, and simulation architecture. 

### Completion criteria

Milestone 2 is complete only if:

* (Y_h) supports the carrier spaces needed for connection and residual fields,
* (X_h) supports pullback observation,
* and (\sigma_h^*) can be executed numerically on at least one nontrivial test field.

### Failure mode if incomplete

If the observation map is not operational here, then later “comparison to observation” work would be formally premature, because the draft’s observed physics is explicitly downstream of pullback/recovery from the observerse. 

---

## Milestone 3 — CPU Reference Geometry and Residual Engine

This milestone builds a slow, trusted implementation of the minimal bosonic branch on CPU.

### Inputs

* discrete geometry from Milestone 2
* branch-fixed torsion and Shiab definitions
* operator-adjoint conventions from Milestone 0

### Outputs

* CPU reference implementation for:

  * connection representation,
  * curvature assembly,
  * torsion evaluation,
  * Shiab evaluation,
  * bosonic residual (\Upsilon_h),
  * residual norm,
  * gauge term evaluation,
  * and observation pullback;
* small deterministic benchmark cases;
* unit tests for each operator path.

This milestone is necessary because the implementation plan explicitly requires validation against a symbolic reference implementation before high-performance work is trusted. 

### Completion criteria

Milestone 3 is complete only if the CPU engine can:

* assemble all Minimal GU v1 derived quantities on toy and small benchmark geometries,
* produce repeatable residual values,
* and pass type, gauge, and pullback consistency checks.

### Failure mode if incomplete

Failure here is a structural or dynamical implementation problem, not a performance problem. It indicates that the chosen v1 branch is not yet computationally coherent enough to accelerate.

---

## Milestone 4 — Variational Solve Prototype on CPU

This milestone introduces an actual solver for the second-order Minimal GU v1 bosonic system.

### Inputs

* CPU residual engine from Milestone 3
* gauge-handling strategy
* declared solve mode: residual minimization or adjoint residual solve

### Outputs

* CPU solver capable of:

  * minimizing the residual norm,
  * or solving the gauge-fixed discrete form of (D_\omega^* \Upsilon_\omega = 0);
* convergence diagnostics;
* checkpointed solver traces;
* residual and gauge-violation plots.

This aligns with the completion architecture’s requirement to decide between time evolution and variational solve mode and to define constraint handling explicitly. 

### Completion criteria

Milestone 4 is complete only if the solver can demonstrate all of the following on benchmark problems:

* monotone decrease of the declared objective or residual norm,
* reproducible stopping behavior,
* stable handling of the declared gauge mechanism,
* and successful extraction of observed diagnostics after solve.

### Failure mode if incomplete

If no stable solve exists even on small controlled cases, that counts as an early dynamical warning sign for the chosen branch. It does not yet falsify GU in general, but it does block promotion of that branch to high-performance simulation. This is consistent with the falsifier taxonomy’s distinction between branch failure and broader theory failure. 

---

## Milestone 5 — Native Interop Boundary and State Transfer

This milestone builds the stable boundary between C# orchestration and native compute code.

### Inputs

* symbolic schemas from Milestone 1
* CPU reference state layouts from Milestones 3–4
* finalized ownership and serialization rules

### Outputs

* versioned interop API for:

  * state allocation,
  * upload/download of geometry and field buffers,
  * solver invocation,
  * diagnostic extraction,
  * and run finalization;
* binary layouts for all performance-critical structures;
* automated cross-language roundtrip tests.

This corresponds directly to the completion outline’s explicit inclusion of a native interop boundary. 

### Completion criteria

Milestone 5 is complete only if the same benchmark state can be serialized in C#, executed in native code, and reconstructed without ambiguity or hidden mutation.

### Failure mode if incomplete

If the boundary is not stable, later GPU discrepancies will be uninterpretable because they may come from transport rather than computation.

---

## Milestone 6 — CUDA Residual Parity Prototype

This milestone ports the core residual assembly path to CUDA and proves parity with the CPU reference.

### Inputs

* stable native interop layer from Milestone 5
* benchmark problems from Milestone 3
* CPU reference outputs for all key fields

### Outputs

* CUDA kernels for:

  * connection-to-curvature lowering,
  * torsion and Shiab evaluation,
  * residual assembly,
  * quadrature/mass reductions,
  * gauge term assembly,
  * and residual diagnostics;
* parity test harness against CPU reference;
* GPU profiling baseline.

The completion outline explicitly identifies tensor/operator kernels, CUDA-targeted workloads, sparse-vs-dense decisions, and validation against symbolic reference as part of the implementation path. 

### Completion criteria

Milestone 6 is complete only if GPU and CPU outputs agree within predeclared tolerances for:

* curvature,
* torsion,
* Shiab,
* residual,
* residual norm,
* gauge term,
* and observed pullback quantities.

Tolerance declarations must be recorded in advance, consistent with the validation protocol’s treatment of numerical uncertainty and comparison discipline. 

### Failure mode if incomplete

A parity failure here is a simulation falsifier for the implementation, not yet for the theory. The falsification protocol explicitly distinguishes implementation disagreement and lack of convergence from broader phenomenological disagreement. 

---

## Milestone 7 — CUDA Solve and Scaling Prototype

This milestone moves from GPU residual assembly to GPU-assisted solution of the Minimal GU v1 system.

### Inputs

* CUDA residual parity from Milestone 6
* solver formulation from Milestone 4
* sparse operator strategy

### Outputs

* GPU or hybrid GPU/CPU nonlinear solve path;
* Jacobian or matrix-free operator actions on GPU;
* convergence and scaling benchmarks;
* performance reports across increasing mesh/problem sizes.

### Completion criteria

Milestone 7 is complete only if:

* the GPU solver reproduces CPU benchmark solutions on small and medium problems,
* iteration histories are stable,
* scaling behavior is measured and recorded,
* and convergence failure modes are classified rather than hidden.

### Failure mode if incomplete

If assembly is correct but solve stability collapses under scaling, the issue is not formal lowering alone but the chosen solver architecture or discrete conditioning.

---

## Milestone 8 — Vulkan Diagnostic Visualization Prototype

This milestone builds the first interactive visualization layer.

### Inputs

* geometry exports from Milestone 2
* CPU/GPU field outputs from Milestones 4–7
* declared visualization targets

### Outputs

* Vulkan renderer for:

  * (Y_h) geometry,
  * (X_h) observation geometry,
  * residual density,
  * selected invariants,
  * convergence playback,
  * and observed-vs-native overlays;
* diagnostic UI controls for switching branch metadata and field channels.

This is directly supported by the visualization chapter, which calls for geometric objects, field and bundle diagnostics, operator spectra, deformation visualization, observed-vs-native comparisons, and a Vulkan pathway. 

### Completion criteria

Milestone 8 is complete only if a user can inspect:

* native (Y)-space fields,
* observed (X)-space fields,
* and their relationship under the chosen observation branch

within one reproducible run session.

### Failure mode if incomplete

Without this milestone, debugging of branch behavior, residual localization, and observation artifacts remains substantially harder, though the mathematical core can still progress.

---

## Milestone 9 — Output Typing and Observation Comparison Engine

This milestone creates the formal bridge from computational outputs to admissible observational claims.

### Inputs

* observed outputs from Milestones 4–8
* output classification rules
* comparison protocol and falsifier taxonomy

### Outputs

* C# comparison engine that:

  * classifies each output as exact structural, semi-quantitative, or quantitative,
  * attaches formal source metadata,
  * attaches an observable map,
  * records remaining assumptions,
  * declares tolerance bands,
  * and associates a falsifier;
* adapters for curated external datasets;
* machine-readable comparison reports.

This milestone is required by the comparison-to-observation framework and the validation protocol. The document is explicit that no GU claim may enter empirical comparison unless it has a typed output class, derivation path, observable map, comparison rule, and falsifier.  

### Completion criteria

Milestone 9 is complete only if every compared output carries:

* its registry class,
* its formal source,
* remaining assumptions,
* an explicit observable-extraction rule,
* a declared external dataset,
* and a concrete falsifier.

That requirement is not optional; it is stated directly in the validation protocol and prediction discipline. 

### Failure mode if incomplete

If outputs are numerically produced but not typed and mapped, they remain internal diagnostics rather than admissible observational claims.

---

## Milestone 10 — External Comparison Pilot

This milestone performs the first disciplined external comparison on a deliberately limited set of outputs.

### Inputs

* typed outputs from Milestone 9
* curated external source declarations
* declared statistical and logical comparison rules

### Outputs

* one pilot comparison set for each output class that has become admissible:

  * structural match checks,
  * interval or ordering checks,
  * and, where justified, quantitative comparisons;
* reproducible data-ingestion records;
* explicit pass/fail/underdetermined judgments.

This structure follows the validation protocol’s separation of structural, semi-quantitative, and quantitative comparison modes. 

### Completion criteria

Milestone 10 is complete only if every comparison report includes:

* source name,
* source version or release,
* access date,
* preprocessing applied,
* reason for source appropriateness,
* declared metric or logical test,
* tolerance-band decomposition,
* and falsifier result.

Those source declarations and comparison rules are required by the empirical framework.  

### Failure mode if incomplete

A missing audit trail here invalidates the comparison, even if the numerical overlay looks persuasive.

---

## Milestone 11 — Benchmark Suite and Independent Reproduction

This milestone turns the prototype into a reproducible research-grade computational framework.

### Inputs

* stable comparison pilot from Milestone 10
* stable CPU and GPU execution paths
* persisted run metadata and branch manifests

### Outputs

* benchmark suite spanning:

  * operator correctness,
  * discretization refinement,
  * solver convergence,
  * CPU/GPU parity,
  * observation-pullback stability,
  * and comparison reproducibility;
* independent re-run scripts or secondary implementation checks;
* regression dashboard for future branch work.

The delivery outline explicitly anticipates appendices for benchmark suites and computational specification, and the falsification protocol identifies disagreement between independent implementations or unstable convergence as simulation falsifiers.  

### Completion criteria

Milestone 11 is complete only if:

* benchmark runs are repeatable,
* comparison outputs are reproducible from stored metadata,
* and at least one independent rerun path confirms the main implementation on declared benchmark cases.

### Failure mode if incomplete

Without this milestone, the project remains a prototype demonstration rather than a falsifiability-ready computational platform.

---

## Milestone 12 — Prototype v1 Release Gate

This milestone is the final release gate for the Minimal GU v1 prototype.

### Inputs

* all prior milestones
* open-risk register
* remaining ambiguity list

### Outputs

* frozen Prototype v1 release package;
* computational specification appendix;
* benchmark appendix;
* comparison appendix;
* known-limitations statement;
* and a decision memo on whether extension to fermions, richer operator families, or phenomenology is justified.

### Completion criteria

Prototype v1 is complete only if all of the following are true:

1. the implemented branch is explicitly frozen and documented;
2. the symbolic, CPU, and CUDA paths agree on benchmark cases within declared tolerances;
3. the observation map is operational and produces typed outputs;
4. the external comparison pipeline is auditable;
5. visualization and diagnostics are sufficient to inspect failures, not merely successes;
6. benchmark and rerun artifacts exist;
7. known limitations are stated without collapsing them into rhetorical claims.

### Failure mode if incomplete

If these conditions are not met, the result should be described as a partial prototype stage rather than a completed Minimal GU v1 implementation.

---

## 3. Dependency order

The dependency structure of the roadmap is strict:

* Milestone 0 feeds everything.
* Milestones 1 and 2 are the minimum required to represent the model.
* Milestones 3 and 4 establish computational coherence.
* Milestones 5 through 8 add performance and inspection.
* Milestones 9 and 10 create admissible empirical contact.
* Milestones 11 and 12 convert the prototype into a reproducible platform.

This order mirrors the document-wide dependency map, which places formal completion ahead of simulation and places prediction/observation only after observation machinery, dynamics, and output layers are defined. 

## 4. What counts as success

Success for this roadmap is **not** “confirming GU.” Success is narrower and more disciplined:

* the minimal bosonic branch becomes executable,
* the symbolic and numerical layers remain traceable,
* the observation step becomes operational,
* external comparison occurs only through typed outputs,
* and failures can be localized to branch choice, implementation, numerics, or observation mapping rather than being rhetorically blurred together.

That interpretation is exactly aligned with the completion document’s scope statement and validation philosophy.  

## 5. Status of this roadmap

This roadmap should be labeled as an **implementation and delivery scaffold** for Minimal GU v1. It is neither a theorem nor a phenomenological claim. Its function is organizational: to ensure that the path from formal branch selection to simulation and comparison proceeds by explicit deliverables, explicit exit criteria, and explicit failure states.

That is the appropriate form for a completion document whose stated aim is to move from incomplete geometric proposal to assessable executable framework without overstating what has been achieved. 

I can turn this into a compact table version next, which may fit better as an appendix or project-tracking artifact.
