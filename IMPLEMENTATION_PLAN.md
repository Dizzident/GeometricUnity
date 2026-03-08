# IMPLEMENTATION_PLAN.md

# Geometric Unity Minimal GU v1 Implementation Plan

## Purpose

This document is a **standalone implementation handoff** for building the first executable branch of the Geometric Unity completion program.

It is written so an implementation agent can build the system **without access to the original manuscript**.

The target is **not full Geometric Unity**.
The target is the **first executable bosonic branch**, called **Minimal GU v1**:

- connection-centered,
- observerse-based,
- explicitly discretized,
- CUDA-accelerated where worthwhile,
- orchestrated in C#,
- auditable and reproducible,
- and able to support both simulation and research-gap closure work.

The system must be designed as a **research platform**, not a one-off solver.

---

# 1. What this implementation is and is not

## 1.1 What this implementation is

This implementation is the **first computationally admissible branch** of the completion program.

It must provide:

1. a typed representation of the core mathematical objects,
2. a discrete realization of the active bosonic branch,
3. a CPU reference backend,
4. a CUDA backend for heavy operator/residual/solve workloads,
5. an observation pipeline from native `Y`-space quantities to observed `X`-space quantities,
6. a validation and artifact/replay contract,
7. a way to run controlled “environments” for simulation and research experiments.

## 1.2 What this implementation is not

This implementation is **not**:

- a validated theory of nature,
- a complete implementation of the fermionic sector,
- a proof that the manuscript’s physical identifications are correct,
- a proof of branch-independence,
- a proof of global observerse existence,
- a proof of PDE well-posedness or nonlinear stability,
- a final phenomenology or experiment-fitting engine.

Those remain later work and are listed explicitly at the end of this document.

---

# 2. Governing engineering principle

The system must preserve a strict separation between:

1. **formal mathematical objects**,
2. **branch choices / inserted assumptions**,
3. **discrete numerical realizations**,
4. **observed outputs**,
5. **external comparison results**.

No layer may silently collapse into another.

In particular:

- visualization is diagnostic only,
- raw native `Y`-space quantities must **never** be compared directly to external data,
- branch choices must be serialized and replayable,
- CPU reference correctness comes before CUDA throughput,
- failed runs and negative results are first-class artifacts.

---

# 3. Active implementation scope: Minimal GU v1

## 3.1 Core branch statement

Implement the **minimal bosonic executable branch** with the following continuous objects:

- a base manifold `X`, intended ultimately as a smooth oriented spin 4-manifold,
- an observerse branch `pi : Y -> X`,
- a chosen observation section or observation map `sigma`,
- a principal bundle `P -> Y`,
- a connection `omega` on `P`,
- curvature `F_omega`,
- a torsion-type branch term `T_omega`,
- a Shiab-type branch term `S_omega`,
- bosonic residual

```text
Upsilon_omega = S_omega - T_omega
```

- variational objective

```text
I2(omega) = (1/2) ∫_Y <Upsilon_omega, Upsilon_omega> dmu_Y
```

- second-order stationarity / adjoint residual equation

```text
D_omega^* Upsilon_omega = 0
```

This is the smallest branch that still retains:

- the observerse architecture,
- connection-centered dynamics,
- torsion/Shiab residual structure,
- and an observation pathway back to `X`.

## 3.2 Why this is the correct initial scope

If any of the following are removed, the branch stops reflecting the manuscript’s distinctive structure:

- remove `Y`, and the observerse/recovery architecture disappears,
- remove `omega`, and the connection-centered dynamics disappear,
- remove `T_omega` or `S_omega`, and the defining residual disappears,
- remove `D_omega^* Upsilon_omega = 0`, and there is no practical variational equation to solve,
- remove `sigma^*`, and there is no bridge from native fields to observed quantities.

---

# 4. Mathematical conventions to use in code

This section fixes the conventions Claude should implement.
Where the manuscript was not unique, this document makes an explicit implementation choice.

## 4.1 Spaces

### `X`

`X` is the base manifold.
Implementation target: dimension parameterizable, but the intended physical target is `dim(X) = 4`.

Implementation rule:

- the engine must be dimension-generic where possible,
- the production branch should default to `dimX = 4`,
- toy tests may run in lower dimensions for debugging and parity testing.

### `Y`

`Y` is the native/ambient/observerse total space over `X`.
It carries the dynamical connection field.

Implementation rule:

- represent `Y` as a discretizable geometric carrier independent from `X`,
- maintain an explicit projection `pi : Y -> X`,
- maintain an explicit observation section or observation map `sigma` used for pullback/extraction.

## 4.2 Projection and observation

### Projection `pi : Y -> X`

This is structural geometry metadata.

### Observation `sigma`

Use `sigma` as the implementation’s normalized observed-sector bridge.

In the codebase:

- `pi_h` maps discrete `Y_h` geometry to discrete `X_h` geometry,
- `sigma_h` is the discrete observation map / section,
- `sigma_h^*` is the pullback/extraction map used to build observed outputs.

No observable may bypass this layer.

## 4.3 Bundle data

Implement a principal bundle `P -> Y` in a **local trivialization form**, not as a symbolic atlas engine.

Meaning:

- the semantic model must still know that `P` is a principal bundle,
- but the first implementation will represent fields locally as Lie-algebra-valued coefficient fields over patches/cells/elements,
- with explicit metadata for structure constants, basis order, gauge conventions, and patch compatibility assumptions.

## 4.4 Field variable `omega`

`omega` is the primary dynamical variable.

Implementation rule:

- treat `omega` as an `ad(P)`-valued discrete connection field over `Y_h`,
- store it as coefficients in a chosen discrete carrier space,
- expose both host-side semantic form and backend-packed numeric form.

## 4.5 Distinguished connection `A0`

The manuscript uses a distinguished connection `A0` as the anchor for the active branch.

Implementation rule:

- `A0` must be represented explicitly in the branch manifest,
- the code must never treat `A0` as an implicit global constant,
- all operators that depend on `A0` must record that dependency in provenance.

## 4.6 Bi-connection construction

The branch requires a map producing a pair of associated connections from the active state.
Use the following implementation abstraction:

```text
(A_omega, B_omega) = Mu_A0(omega, branchParameters, geometryContext)
```

where `Mu_A0` is a **branch-defined construction**.

Do not hardcode a physical interpretation beyond this.

## 4.7 Torsion branch term `T_omega`

The active branch treats augmented/displaced torsion as a corrected torsion-like object derived from the bi-connection data.

Implementation abstraction:

```text
T_omega = TorsionBranch(A_omega, B_omega, geometryContext, branchParameters)
```

The implementation only needs to satisfy:

1. it is typed,
2. it is reproducible,
3. it is branch-traceable,
4. it lands in the same carrier type as `S_omega`.

## 4.8 Shiab branch term `S_omega`

The manuscript leaves Shiab as a family and chooses an active minimal-compatible branch.

Implementation abstraction:

```text
S_omega = ShiabBranch_mc(F_omega, omega, geometryContext, branchParameters)
```

This must be treated as a **declared branch operator**, not as a uniquely canonical one.

## 4.9 Residual `Upsilon_omega`

Always implement:

```text
Upsilon_omega = S_omega - T_omega
```

with `S_omega` and `T_omega` represented in the **same target carrier space**.

## 4.10 Objective and stationarity equation

The computational objective is:

```text
I2(omega) = (1/2) ∫_Y <Upsilon_omega, Upsilon_omega> dmu_Y
```

The principal computational equation is:

```text
D_omega^* Upsilon_omega = 0
```

The implementation should realize this in discrete form as:

```text
I2_h(omega_h) = (1/2) Upsilon_h^T M_Upsilon Upsilon_h
```

and

```text
J_h^T M_Upsilon Upsilon_h = 0
```

where:

- `J_h = ∂Upsilon_h / ∂omega_h`,
- `M_Upsilon` is the mass/quadrature matrix for the residual carrier.

---

# 5. Branch-fixed implementation assumptions

The manuscript is intentionally broader than the first implementation.
To make the system buildable, the following assumptions/choices are fixed for v1.

## IA-1: Dimension-generic engine, 4D target

The engine should support generic dimensions internally when practical, but the active branch metadata must default to:

```text
dim(X) = 4
```

## IA-2: Local trivialization implementation of `P`

The first implementation will not build a full symbolic principal-bundle atlas solver.
Instead:

- `P_h` is represented through local frames/trivializations,
- `omega_h`, `F_h`, `T_h`, `S_h`, and `Upsilon_h` are stored as coefficient arrays with explicit basis metadata,
- transition consistency is assumed within the selected discretization branch.

## IA-3: Variational solve mode first

Do **not** begin with real-time time evolution.
The first solver mode is:

- residual evaluation,
- objective evaluation,
- gradient / Gauss-Newton / Newton-style solve,
- gauge-stabilized stationary solve.

## IA-4: Gauge stabilization is mandatory

The solver must include an explicit gauge treatment.
Allowed initial implementations:

1. penalty term,
2. reduced-coordinate nullspace elimination,
3. explicit constrained solve.

Recommended order:

- v1.0: penalty + diagnostics,
- v1.1: reduced-coordinate or nullspace-aware solve.

## IA-5: CPU reference backend is required before CUDA trust

No CUDA result is admissible until the matching CPU path exists and parity tests pass.

## IA-6: Observation pipeline is typed

No native `Y` quantity may be exported to comparison code unless it first passes through:

1. `sigma_h^*`,
2. optional derived observable transform,
3. normalization/unit policy,
4. output typing,
5. comparison adapter.

## IX-1: Active torsion branch API

The first active torsion branch should be implemented as a **local or weakly local branch operator**:

```text
T_h = TorsionBranchKernel(omega_h, A0_h, branchMetadata, geometryBuffers)
```

with output type identical to the Shiab branch output type.

## IX-2: Active Shiab branch API

The first active Shiab branch should be implemented as a **first-order curvature-derived operator**:

```text
S_h = ShiabMcKernel(F_h, omega_h, branchMetadata, geometryBuffers)
```

The exact formula is branch-controlled and must be declared in the branch manifest.

## IX-3: Representation backend

The first concrete Lie backend should use:

- finite-dimensional real Lie algebra basis,
- explicit structure constants,
- explicit invariant metric / pairing,
- explicit basis order metadata.

Do not hardcode physical Standard Model assumptions into the core engine.

## IX-4: Reference discretization branch

Use a **patch-based finite-element or finite-volume style discretization** on `Y_h`, with a simpler compatible carrier on `X_h`.

Recommended initial choice:

- simplicial or structured patch geometry,
- local basis functions,
- quadrature-based weak assembly,
- sparse or matrix-free linearization support.

## IX-5: Visualization backend is read-only

Visualization may derive render buffers from artifacts, but must not become a hidden state authority.

---

# 6. Required lowering contract

The implementation is only a valid Minimal GU v1 realization if it provides all of the following maps:

```text
X                  -> X_h
Y                  -> Y_h
pi                 -> pi_h
sigma              -> sigma_h
P                  -> P_h
omega              -> omega_h
F_omega            -> F_h
T_omega            -> T_h
S_omega            -> S_h
Upsilon_omega      -> Upsilon_h
D_omega            -> J_h
D_omega^*Upsilon   -> J_h^T M_Upsilon Upsilon_h
sigma^*Q           -> sigma_h^* Q_h
```

If any map is missing, the implementation is only a partial prototype.

This is a hard acceptance rule.

---

# 7. Recommended software architecture

Use three hard separations:

1. **C# semantic/orchestration layer**,
2. **native/CUDA numerical backend**,
3. **Vulkan visualization/inspection layer**.

## 7.1 C# layer responsibilities

C# owns:

- manifests,
- semantic state model,
- branch metadata,
- configuration,
- environment specs,
- CPU reference implementation,
- observation extraction,
- validation registry,
- artifact serialization,
- replay logic,
- external comparison engine,
- CLI/workbench orchestration.

## 7.2 CUDA layer responsibilities

CUDA owns:

- heavy field transforms,
- curvature/torsion/Shiab evaluation,
- residual assembly,
- mass/quadrature reductions,
- Jacobian actions,
- GPU solve paths,
- parity-oriented packed buffer execution.

## 7.3 Vulkan layer responsibilities

Vulkan owns:

- geometry view,
- field view,
- residual view,
- spectrum/linearization diagnostics,
- comparison overlays,
- interactive artifact inspection.

It must consume snapshots and artifacts only.

---

# 8. Recommended repository structure

```text
/GeometricUnity.sln

/apps
  /Gu.Cli
  /Gu.Workbench
  /Gu.Benchmarks

/src
  /Gu.Core
  /Gu.Math
  /Gu.Symbolic
  /Gu.Branching
  /Gu.Geometry
  /Gu.Discretization
  /Gu.ReferenceCpu
  /Gu.Solvers
  /Gu.Observation
  /Gu.Validation
  /Gu.Artifacts
  /Gu.Interop
  /Gu.VulkanViewer
  /Gu.ExternalComparison

/native
  /gu_native_common
  /gu_cuda_core
  /gu_cuda_kernels
  /gu_vulkan_native

/tests
  /Gu.Core.Tests
  /Gu.Geometry.Tests
  /Gu.ReferenceCpu.Tests
  /Gu.Parity.Tests
  /Gu.Observation.Tests
  /Gu.Validation.Tests
  /Gu.Replay.Tests

/schemas
  /branch.schema.json
  /geometry.schema.json
  /artifact.schema.json
  /validation.schema.json
  /observed.schema.json

/examples
  /toy_branch_2d
  /toy_branch_3d
  /minimal_v1_4d

/docs
  /IMPLEMENTATION_PLAN.md
```

---

# 9. Minimum runtime semantic types

The following semantic categories are mandatory.
If a module omits one, it must emulate it explicitly elsewhere.

## 9.1 Core manifests and metadata

```csharp
BranchManifest
BranchRef
GeometryContext
GeometryBinding
ProvenanceMeta
NormalizationMeta
TensorSignature
SpaceRef
ValidationStamp
```

## 9.2 State types

```csharp
DiscreteState
DerivedState
LinearizationState
ObservedState
FieldTensor
```

## 9.3 Operators and residuals

```csharp
LinearOperatorModel
ResidualComponent
ResidualBundle
```

## 9.4 Validation and observation

```csharp
ValidationRecord
ValidationBundle
ObservableSnapshot
```

## 9.5 Artifact packaging

```csharp
ArtifactBundle
ReplayContract
IntegrityBundle
```

---

# 10. Detailed class/interface guidance

## 10.1 Branch manifest

The branch manifest is the single most important control object.

It must include at minimum:

- branch identifier,
- source-equation revision,
- code revision,
- active geometry branch,
- active observation branch,
- active torsion branch,
- active Shiab branch,
- active gauge strategy,
- basis conventions,
- adjoint convention,
- norm convention,
- regularity assumptions,
- selected invariant pairing,
- selected mass/quadrature policy,
- allowed backend classes,
- non-determinism declaration,
- inserted assumptions / inserted choices list.

Suggested shape:

```csharp
public sealed class BranchManifest
{
    public string BranchId { get; init; }
    public string SchemaVersion { get; init; }
    public string SourceEquationRevision { get; init; }
    public string CodeRevision { get; init; }

    public string ActiveGeometryBranch { get; init; }
    public string ActiveObservationBranch { get; init; }
    public string ActiveTorsionBranch { get; init; }
    public string ActiveShiabBranch { get; init; }
    public string ActiveGaugeStrategy { get; init; }

    public int BaseDimension { get; init; }
    public int AmbientDimension { get; init; }

    public string LieAlgebraId { get; init; }
    public string BasisConventionId { get; init; }
    public string ComponentOrderId { get; init; }
    public string AdjointConventionId { get; init; }
    public string PairingConventionId { get; init; }
    public string NormConventionId { get; init; }

    public IReadOnlyList<string> InsertedAssumptionIds { get; init; }
    public IReadOnlyList<string> InsertedChoiceIds { get; init; }
    public IReadOnlyDictionary<string,string> Parameters { get; init; }
}
```

## 10.2 Geometry context

This must contain both the semantic and discrete geometry information.

```csharp
public sealed class GeometryContext
{
    public SpaceRef BaseSpace { get; init; }          // X_h
    public SpaceRef AmbientSpace { get; init; }       // Y_h
    public string DiscretizationType { get; init; }
    public string QuadratureRuleId { get; init; }
    public string BasisFamilyId { get; init; }

    public GeometryBinding ProjectionBinding { get; init; }   // pi_h
    public GeometryBinding ObservationBinding { get; init; }  // sigma_h

    public IReadOnlyList<PatchInfo> Patches { get; init; }
    public byte[] GeometryPayload { get; init; }
}
```

## 10.3 Discrete state

The implementation should make a clean distinction between independent state and derived state.

```csharp
public sealed class DiscreteState
{
    public BranchRef Branch { get; init; }
    public GeometryContext Geometry { get; init; }
    public FieldTensor Omega { get; init; }           // independent field
    public ProvenanceMeta Provenance { get; init; }
}
```

## 10.4 Derived state

```csharp
public sealed class DerivedState
{
    public FieldTensor CurvatureF { get; init; }
    public FieldTensor TorsionT { get; init; }
    public FieldTensor ShiabS { get; init; }
    public FieldTensor ResidualUpsilon { get; init; }
    public IReadOnlyDictionary<string, FieldTensor> Diagnostics { get; init; }
}
```

## 10.5 Linearization state

```csharp
public sealed class LinearizationState
{
    public LinearOperatorModel Jacobian { get; init; }
    public LinearOperatorModel? Adjoint { get; init; }
    public FieldTensor GradientLikeResidual { get; init; }  // J^T M Upsilon
    public IReadOnlyDictionary<string,double> SpectralDiagnostics { get; init; }
}
```

## 10.6 Observed state

```csharp
public sealed class ObservedState
{
    public string ObservationBranchId { get; init; }
    public IReadOnlyDictionary<string, ObservableSnapshot> Observables { get; init; }
    public ProvenanceMeta Provenance { get; init; }
}
```

---

# 11. Tensor, basis, and adjoint conventions

## 11.1 Tensor signature metadata is mandatory

Every tensor-like object must declare:

- ambient space,
- carrier type,
- degree (0-form / 1-form / 2-form / mixed),
- Lie algebra basis,
- component ordering,
- numeric precision,
- memory layout,
- backend packing descriptor.

Silent changes in component order between CPU and GPU are forbidden.

## 11.2 Adjoint convention is mandatory

Any operator used in the variational or linearization path must either:

1. provide an explicit adjoint representation, or
2. explicitly declare that the adjoint is approximated numerically, with that approximation recorded in the artifact and validation bundle.

Do not silently use “transpose = adjoint” unless the branch says that approximation is admissible.

## 11.3 Pairing convention

The residual norm depends on the chosen inner product / pairing.
The implementation must serialize:

- Lie algebra pairing,
- differential form carrier metric or mass convention,
- quadrature weighting rule,
- any regularization terms.

---

# 12. Discretization strategy

## 12.1 High-level rule

The implementation is a **typed discretization**, not a separate theory.
Every continuous object must become a typed finite object.

## 12.2 Recommended discrete carriers

Use the following initial carrier strategy:

- `X_h`: lower-dimensional base discretization for observed-space extraction,
- `Y_h`: primary computational mesh/patch/lattice on which `omega_h` lives,
- `omega_h`: coefficient field in a discrete connection carrier,
- `F_h`: curvature carrier,
- `T_h`, `S_h`, `Upsilon_h`: common residual carrier,
- `J_h`: sparse or matrix-free tangent operator on perturbations `delta omega_h`.

## 12.3 Recommended first implementation choice

For the CPU reference backend, prefer clarity:

- patch-partitioned dense field arrays where feasible,
- explicit sparse operators or transparent matrix-free operator wrappers,
- clear host-visible metadata,
- immutable manifests with versioned state snapshots.

For the CUDA backend, prefer performance with reversibility:

- structure-of-arrays layout,
- packed coefficient buffers,
- fused kernels where justified,
- serialized memory-layout descriptors,
- explicit mapping back to the logical public state.

## 12.4 Torsion and Shiab lowering

The most important branch-dependent lowering is:

```text
T_h = T_h(omega_h)
S_h = S_h(omega_h)
```

Implementation categories allowed:

### `T_h`

- algebraic local map at quadrature points,
- sparse stencil,
- nonlinear sparse operator,
- face/cell assembly rule.

### `S_h`

- first-order operator on curvature data,
- projected curvature transform,
- mixed local/nonlocal operator.

**Hard rule:** `T_h` and `S_h` must land in the same discrete carrier type.

## 12.5 Residual and objective

Store:

```text
Upsilon_h = S_h - T_h
```

as either:

- one Lie-algebra block per discrete 2-form carrier,
- or one block per quadrature sample if weakly assembled.

The objective is:

```text
I2_h = (1/2) Upsilon_h^T M_Upsilon Upsilon_h
```

This is the primary scalar metric used for:

- solver descent,
- convergence tracking,
- backend parity,
- branch sensitivity studies.

## 12.6 Jacobian lowering

Implement:

```text
J_h = ∂Upsilon_h / ∂omega_h
```

Possible realizations:

- sparse matrix,
- block sparse matrix,
- matrix-free operator,
- bundle-aware tangent action.

Recommended progression:

1. local analytic derivative on CPU,
2. finite-difference verification harness,
3. matrix-free actions for large problems,
4. explicit sparse assembly only when needed for diagnostics or direct solvers.

---

# 13. Numerical workflow

## 13.1 Mandatory solve progression

The development order must be:

1. **CPU symbolic/reference residual evaluation**,
2. **GPU residual-only evaluation**,
3. **GPU gradient / Gauss-Newton solve**,
4. **full sparse Newton or matrix-free Krylov with gauge stabilization**.

Do not skip the CPU reference stage.

## 13.2 High-level solver loop

```text
load branch manifest
load geometry context
build initial omega_h
assemble derived state: F_h, T_h, S_h, Upsilon_h
assemble objective I2_h
assemble gauge diagnostics
if solve mode:
    assemble J_h or J_h action
    assemble gradient-like term G_h = J_h^T M_Upsilon Upsilon_h (+ gauge terms)
    solve/update omega_h
    checkpoint artifacts
    iterate until convergence / failure / invalidity
extract observed state via sigma_h^*
run validation records
write artifact package
```

## 13.3 Solve modes to support

### Mode A: residual-only

Use to validate assembly and parity.

### Mode B: objective minimization

Minimize `I2_h` with:

- line search gradient descent,
- nonlinear conjugate gradient,
- Gauss-Newton.

### Mode C: stationarity solve

Solve:

```text
J_h^T M_Upsilon Upsilon_h + GaugePenalty = 0
```

### Mode D: research branch sensitivity

Re-run the same environment under multiple torsion/Shiab branch manifests and compare:

- convergence behavior,
- final residual norms,
- observed outputs,
- replayability,
- failure modes.

## 13.4 Convergence criteria

Track all of the following:

- `I2_h`,
- `||Upsilon_h||`,
- `||J_h^T M_Upsilon Upsilon_h||`,
- gauge violation norm,
- step norm,
- relative change in observed diagnostics,
- backend parity residual if running CPU/GPU comparison.

---

# 14. CPU reference backend plan

The CPU reference backend is the scientific baseline.
It must favor transparency over speed.

## 14.1 Responsibilities

The CPU backend must be able to:

- build `F_h`,
- build `T_h`,
- build `S_h`,
- build `Upsilon_h`,
- evaluate `I2_h`,
- build `J_h` or apply `J_h`,
- apply `J_h^T M_Upsilon`,
- compute `sigma_h^* Q_h`,
- generate validation-grade artifacts.

## 14.2 Numerical precision

Recommended:

- `double` by default,
- optional high-precision test harness for tiny benchmark cases,
- no mixed-precision CUDA path until CPU parity is already strong.

## 14.3 CPU implementation strategy

- implement geometry traversal in clear loops first,
- implement local field transforms in small pure functions,
- implement Jacobian checks with finite-difference diagnostics,
- prefer immutable manifests and explicit state snapshots,
- write human-readable debug dumps for tiny test cases.

---

# 15. CUDA backend plan

## 15.1 CUDA design principle

The CUDA backend exists to accelerate the parts that are both:

1. dominant in runtime,
2. already validated by CPU reference.

## 15.2 First kernel priorities

Port these in order:

1. geometry/basis gather-scatter support,
2. curvature assembly,
3. torsion branch evaluation,
4. Shiab branch evaluation,
5. residual subtraction,
6. quadrature/mass reductions,
7. objective evaluation,
8. Jacobian-vector products,
9. adjoint/JT-vector products,
10. solver update primitives.

## 15.3 CUDA memory layout

Use structure-of-arrays where possible.

Recommended packed buffers:

- connection coefficients,
- curvature coefficients,
- torsion coefficients,
- Shiab coefficients,
- residual coefficients,
- quadrature weights,
- basis derivative tables,
- Lie algebra structure constants,
- layout descriptors.

All packed layouts must have a reversible mapping to semantic state.

## 15.4 CUDA parity requirements

A CUDA path is not admitted until all of the following match the CPU backend within declared tolerances:

- curvature assembly,
- torsion branch,
- Shiab branch,
- residual field,
- objective value,
- gauge term,
- observation pullback,
- any derived observable used in validation.

## 15.5 Solver acceleration stages

### Stage 1

GPU residual and objective only.

### Stage 2

GPU gradient/Gauss-Newton with CPU-controlled orchestration.

### Stage 3

GPU matrix-free Krylov / Newton support.

### Stage 4

Optional advanced preconditioners and mixed precision.
Only after Stage 3 is stable.

---

# 16. Native interop boundary

The interop layer must be strict and versioned.

## 16.1 What crosses the boundary

Allow only:

- immutable manifest snapshots,
- packed geometry buffers,
- packed field buffers,
- packed operator descriptors,
- solver request payloads,
- result payloads,
- explicit error/failure packets.

## 16.2 What must not cross implicitly

Do not allow hidden global state such as:

- undeclared branch metadata,
- undeclared basis ordering,
- undeclared gauge conventions,
- undeclared memory layout assumptions,
- undeclared random seeds / nondeterminism settings.

## 16.3 Interop tests

The same benchmark case must:

1. serialize from C#,
2. run in native/CUDA,
3. reconstruct back into semantic state,
4. replay under the artifact contract.

---

# 17. Observation and environment simulation model

The user wants the application to simulate environments and use those runs to try to close research gaps.

Treat every environment as a first-class object.

## 17.1 EnvironmentSpec

```csharp
public sealed class EnvironmentSpec
{
    public string EnvironmentId { get; init; }
    public BranchRef Branch { get; init; }
    public string ScenarioType { get; init; }

    public GeometryContext Geometry { get; init; }
    public BoundaryConditionBundle BoundaryConditions { get; init; }
    public InitialConditionBundle InitialConditions { get; init; }
    public GaugeConditionBundle GaugeConditions { get; init; }

    public IReadOnlyList<ObservableRequest> ObservableRequests { get; init; }
    public IReadOnlyList<string> ComparisonTemplateIds { get; init; }
    public ProvenanceMeta Provenance { get; init; }
}
```

## 17.2 Environment categories

Support at least:

1. **toy consistency environments**
   - tiny meshes,
   - known expected structure,
   - CPU-debuggable.

2. **branch sensitivity environments**
   - same geometry, different torsion/Shiab branch manifests.

3. **observation pipeline environments**
   - emphasize `sigma_h^*` correctness and observed-state stability.

4. **scaling environments**
   - larger Y-side workloads for CUDA benchmarking.

5. **external-comparison-ready environments**
   - only once observed outputs are typed and stable.

## 17.3 Observation pipeline

All observed outputs must pass through:

```text
native Y-state
-> sigma_h^*
-> optional derived observable transform
-> normalization / units
-> output typing
-> comparison adapter
```

## 17.4 Output typing

Every observed output must be tagged as one of:

- `ExactStructural`,
- `SemiQuantitative`,
- `Quantitative`.

The system must never force a quantitative comparison on an output that is only structural.

---

# 18. External comparison engine

This is part of the application, but it is downstream of the math and simulation layers.

## 18.1 Comparison rule

Every comparison item must record:

- formal source,
- branch data,
- observable definition,
- optional auxiliary model,
- comparison rule,
- comparison scope,
- tolerance policy,
- falsifier condition,
- source data identity/version.

## 18.2 Comparison adapter types

Support pluggable adapters for:

- curated reference tables,
- simulated benchmark datasets,
- structural facts / constants,
- later experimental datasets.

## 18.3 Comparison scope discipline

Every comparison must explicitly say whether it tests:

- raw branch structure,
- observation map,
- auxiliary effective model,
- numerical implementation,
- or a combination.

## 18.4 Negative-result preservation

If a branch fails structurally, numerically, or observationally, preserve the failure as an artifact.
Do not silently replace it with a better branch.

---

# 19. Validation and benchmark suite

The first release must include a benchmark and validation suite.

## 19.1 Mandatory v1 tests

### Well-definedness test

Check that:

- all carriers match,
- all tensor signatures match,
- all required lowering maps are present,
- no observation output bypasses `sigma_h^*`.

### Gauge-consistency test

Check that small gauge-equivalent perturbations do not catastrophically destabilize the gauge-fixed solver.

### Residual test

Check that solve iterations reduce `I2_h` and ideally drive `||Upsilon_h||` down.

### Branch-sensitivity test

Change the torsion or Shiab branch and verify that the output change is:

- controlled,
- logged,
- provenance-preserved.

### Observation test

Check that observed outputs on `X_h` are:

- smooth enough for the chosen discretization,
- reproducible,
- parity-consistent across backends.

### Discretization/refinement test

Check that refinement reduces residual and stabilizes observed diagnostics.

## 19.2 Additional parity tests

Add separate tests for:

- curvature assembly,
- torsion branch,
- Shiab branch,
- objective value,
- Jacobian-vector action,
- adjoint action,
- observation pullback,
- artifact replay.

---

# 20. Artifact schema and reproducibility contract

This implementation must write canonical artifact packages.

## 20.1 Minimal artifact package contents

Every validation-grade run must persist:

- branch manifest,
- geometry manifest,
- runtime manifest,
- initial state,
- final state,
- derived state bundle,
- residual bundle,
- optional linearization bundle,
- observed-state bundle,
- validation bundle,
- integrity/hash bundle,
- replay contract.

## 20.2 Folder layout

Use this canonical layout:

```text
run/
  manifest/
    branch.json
    geometry.json
    runtime.json
  state/
    initial_state.bin
    final_state.bin
    derived/
  residuals/
    residual_bundle.json
  linearization/
    linearization_bundle.json
  observed/
    observed_state.json
  validation/
    validation_bundle.json
    records/
  integrity/
    hashes.json
    package_root.txt
  replay/
    replay_contract.json
    reproduce.sh
  logs/
    solver.log
    environment.txt
```

Equivalent archives are acceptable only if the logical partition remains visible.

## 20.3 Reproducibility tiers

Implement the following replay classes:

- `R0`: archival only,
- `R1`: structural replay,
- `R2`: numerical replay,
- `R3`: cross-backend replay.

Validation claims require at least `R2`.
Backend parity claims require `R3`.

## 20.4 Replay procedure

A replay run must:

1. verify hashes,
2. load manifests,
3. check branch coherence,
4. load initial state and geometry,
5. rebuild derived-state objects,
6. re-evaluate residuals,
7. rebuild linearization if present,
8. rerun observed extraction if present,
9. rerun validation records,
10. compare with replay contract,
11. emit pass/fail/invalid replay report.

---

# 21. Recommended milestone sequence

This is the implementation order Claude should follow.

## Milestone 0 — Bootstrap and schema skeleton

### Goal

Create the solution, projects, manifests, schemas, and core semantic types.

### Deliverables

- repo structure,
- `BranchManifest`, `GeometryContext`, `DiscreteState`, `ArtifactBundle`,
- JSON schemas,
- CLI to create empty branch/environment manifests,
- basic serialization tests.

### Acceptance

- can create/load manifests,
- can serialize minimal empty artifact bundle,
- schema versioning exists.

---

## Milestone 1 — Branch freezing and conventions

### Goal

Freeze the Minimal GU v1 implementation branch.

### Deliverables

- active branch manifest,
- inserted assumptions ledger,
- basis/order/adjoint convention registry,
- initial Lie-algebra backend.

### Acceptance

- a run cannot start without a valid branch manifest,
- tensor signatures enforce basis/component order,
- branch ID is written to all outputs.

---

## Milestone 2 — Geometry and discretization skeleton

### Goal

Implement `X_h`, `Y_h`, `pi_h`, `sigma_h`, and geometry storage.

### Deliverables

- patch/mesh representation,
- geometry bindings,
- quadrature metadata,
- basis family API,
- tiny toy environment geometry examples.

### Acceptance

- can create a toy `Y_h -> X_h` mapping,
- can evaluate `sigma_h^*` on a simple test field,
- geometry artifacts replay.

---

## Milestone 3 — CPU reference field layer

### Goal

Implement `omega_h`, `F_h`, and field tensor infrastructure.

### Deliverables

- `FieldTensor`,
- connection carrier,
- curvature assembly,
- tensor signature checks,
- tiny-case debug printer.

### Acceptance

- CPU curvature path works on toy cases,
- parity checks exist for local curvature identities/tests.

---

## Milestone 4 — CPU torsion and Shiab branches

### Goal

Implement active `T_h` and `S_h` branch operators.

### Deliverables

- `ITorsionBranchOperator`,
- `IShiabBranchOperator`,
- active minimal-compatible implementations,
- same-carrier validation checks.

### Acceptance

- `T_h` and `S_h` both build,
- carrier compatibility enforced,
- branch metadata serialized.

---

## Milestone 5 — CPU residual, objective, and Jacobian

### Goal

Build `Upsilon_h`, `I2_h`, `J_h`, and adjoint action.

### Deliverables

- residual assembly,
- mass/quadrature matrix builder,
- Jacobian action,
- adjoint action,
- finite-difference Jacobian verification harness.

### Acceptance

- objective evaluates,
- gradient-like residual evaluates,
- Jacobian finite-difference consistency tests pass on tiny cases.

---

## Milestone 6 — CPU solver and gauge stabilization

### Goal

Solve the variational/stationary problem on CPU.

### Deliverables

- residual-only mode,
- objective minimization mode,
- gauge penalty,
- convergence diagnostics,
- solver log artifacting.

### Acceptance

- a toy branch reduces objective and residual,
- gauge diagnostics are emitted,
- failures are preserved as artifacts.

---

## Milestone 7 — Observation and observed-state layer

### Goal

Implement typed observed outputs.

### Deliverables

- `ObservedState`,
- `ObservableSnapshot`,
- `sigma_h^*` extraction pipeline,
- normalization metadata,
- output typing (`ExactStructural`, `SemiQuantitative`, `Quantitative`).

### Acceptance

- no output reaches comparison without observation typing,
- observed-state bundle replays.

---

## Milestone 8 — Validation and artifact contract

### Goal

Make runs replayable and validation-grade.

### Deliverables

- canonical run folder writer,
- integrity hashes,
- replay contract,
- validation records and bundle,
- replay CLI.

### Acceptance

- `R2` numerical replay works for CPU backend,
- invalid replay conditions are detected and reported.

---

## Milestone 9 — Native interop and CUDA residual parity

### Goal

Port the heavy field/residual computations to CUDA.

### Deliverables

- interop payloads,
- packed buffer descriptors,
- CUDA curvature/torsion/Shiab/residual kernels,
- CPU/GPU parity tests.

### Acceptance

- GPU residual path matches CPU within tolerance,
- parity artifacts written,
- packed layouts are reversible.

---

## Milestone 10 — CUDA solve path

### Goal

Accelerate solving, not just assembly.

### Deliverables

- GPU objective/gradient loop,
- Jacobian-vector / adjoint-vector kernels,
- Gauss-Newton/Krylov support,
- scaling benchmarks.

### Acceptance

- GPU solve decreases the same objective as CPU,
- backend parity claims reach `R3` for selected benchmarks.

---

## Milestone 11 — Vulkan workbench

### Goal

Build inspection and diagnostics views.

### Deliverables

- geometry view,
- field view,
- residual view,
- convergence view,
- comparison overlay view.

### Acceptance

- viewer consumes artifact snapshots only,
- no hidden state mutation path exists.

---

## Milestone 12 — External comparison engine

### Goal

Support structured comparison without violating observation discipline.

### Deliverables

- comparison templates,
- adapter interface,
- validation record generation,
- negative-result preservation.

### Acceptance

- comparison records are reproducible,
- no raw native `Y` quantity is compared directly to data.

---

# 22. Definition of done for Minimal GU v1

Minimal GU v1 is complete only if all of the following are true:

1. the branch manifest is frozen and serialized,
2. all required lowering maps exist,
3. the CPU reference backend evaluates residuals and solves small cases,
4. CUDA residual parity passes for selected benchmarks,
5. at least one CUDA solve path exists,
6. observed outputs are produced through `sigma_h^*`,
7. validation bundles are generated,
8. artifact packages replay at least at `R2`,
9. at least one cross-backend parity benchmark reaches `R3`,
10. failure cases and invalid runs are preserved,
11. visualization is artifact-driven and read-only,
12. the system can run environment specs and emit typed outputs.

---

# 23. Explicit “do not improvise” rules for Claude

Claude should **not** improvise the missing parts of the full theory as though they were settled.

When a formula or structure is not uniquely determined by this plan, Claude must:

1. implement it as a **branch-defined interface**,
2. record the choice in the branch manifest,
3. serialize the choice into artifacts,
4. expose it to validation and branch-sensitivity testing.

In particular, Claude must **not** silently hardcode:

- a unique Shiab definition as though no branch choice exists,
- a unique torsion correction as though no branch choice exists,
- a direct physical interpretation of a field as though it is already validated,
- a direct comparison of native `Y` fields to data,
- a hidden gauge convention,
- a hidden adjoint convention,
- a hidden tensor basis order.

---

# 24. Remaining items intentionally NOT included in this implementation

This section is the handoff for next time.
These items are **not failures of this plan**. They are intentionally beyond Minimal GU v1.

## 24.1 Full fermionic sector

Not included:

- full topological/chimeric spinor implementation,
- complete topological-to-metric spinor transition,
- physically interpretable fermionic field recovery,
- fermionic action closure tied to the executable branch.

## 24.2 Full observerse/global geometry closure

Not included:

- proof of global observerse existence under the desired hypotheses,
- canonicity or uniqueness of the observerse realization,
- proof-level closure of horizontal/vertical decomposition in all cases,
- proof of global compatibility conditions for all branch choices.

## 24.3 Full structure-group and gauge-closure uniqueness

Not included:

- final uniqueness of the active structure group,
- proof that the chosen tilted gauge realization is canonical,
- full classification of admissible gauge branches.

## 24.4 Torsion and Shiab uniqueness / branch-independence

Not included:

- proof that the active torsion branch is unique,
- proof that the active Shiab branch is unique,
- proof that physical or observed outputs are branch-independent,
- exhaustive classification of admissible torsion/Shiab branches.

The software should support studying these, but does not resolve them.

## 24.5 Full PDE well-posedness and nonlinear stability proofs

Not included:

- full functional-analytic proof of well-posedness,
- full existence/uniqueness theory,
- nonlinear stability proofs,
- rigorous long-time behavior theory,
- proof that the chosen gauge strategy is mathematically optimal.

The software can generate evidence, diagnostics, and counterexamples, but not the proofs.

## 24.6 Final physical identification / decomposition closure

Not included:

- final mapping of all sectors to observed physics,
- final representation decomposition and particle interpretation,
- final electroweak/strong/family identifications,
- final mass/coupling predictions.

Those remain downstream interpretive/phenomenological work.

## 24.7 Quantitative confrontation with external observations

Not included in the sense of “finished science”:

- final observational fit programs,
- trusted quantitative predictions across branches,
- empirical confirmation of the framework,
- acceptance of any physical claim beyond the typed-comparison protocol.

The engine should support this later, but v1 does not claim completion here.

## 24.8 Quantum consistency / UV / anomaly / renormalization issues

Not included:

- quantization strategy,
- anomaly analysis,
- renormalizability claims,
- UV completion claims,
- full quantum field-theoretic closure.

## 24.9 Full production-grade performance work

Not included in first implementation:

- advanced preconditioners,
- mixed precision,
- multi-GPU domain decomposition,
- out-of-core giant runs,
- distributed replay clusters,
- final production UX polish.

---

# 25. Recommended next-session pickup order

Once this plan is implemented, the next strongest moves are:

1. **branch-independence study framework**
   - run the same benchmark suite under multiple torsion/Shiab branches,
   - quantify output and convergence sensitivity.

2. **PDE/stability research layer**
   - use the linearization and replay artifacts to study conditioning,
   - classify failure modes and branch fragility.

3. **fermionic sector onboarding**
   - add a separate semantic branch for topological/chimeric spinors,
   - do not mix into the bosonic core before its contracts are stable.

4. **physical identification gate**
   - add explicit hypothesis-gated decomposition logic,
   - keep mappings tagged as structural/candidate/tentative until supported.

5. **quantitative comparison expansion**
   - only after observed outputs are stable and replayable.

---

# 26. Final implementation instruction

Build this as a **reproducible research engine**.

Do not optimize away the things that make the work scientifically auditable:

- branch manifests,
- inserted assumption tracking,
- tensor signatures,
- CPU reference paths,
- replay artifacts,
- observation discipline,
- negative-result preservation.

The first success condition is not “fast.”
It is:

```text
typed + reproducible + branch-traceable + CPU-valid + CUDA-parity-capable + observation-disciplined
```

Only after that should the implementation chase larger environments and stronger phenomenology.

