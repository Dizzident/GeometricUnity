# HOW TO: Geometric Unity Research Platform

A comprehensive guide to building, running, testing, and exploring the Geometric Unity (GU) codebase — the first executable bosonic branch of Eric Weinstein's Geometric Unity completion program.

---

## Table of Contents

1. [Prerequisites and Setup](#1-prerequisites-and-setup)
2. [Building the Project](#2-building-the-project)
3. [Project Structure Overview](#3-project-structure-overview)
4. [CLI Reference](#4-cli-reference)
5. [Running Your First Solver](#5-running-your-first-solver)
6. [Understanding Branch Manifests](#6-understanding-branch-manifests)
7. [Geometry and Environment Specs](#7-geometry-and-environment-specs)
8. [Solver Modes and Methods](#8-solver-modes-and-methods)
9. [Observation Pipeline](#9-observation-pipeline)
10. [Artifacts, Replay, and Reproducibility](#10-artifacts-replay-and-reproducibility)
11. [Running and Writing Tests](#11-running-and-writing-tests)
12. [Phase II: Research Instrumentation](#12-phase-ii-research-instrumentation)
13. [Vulkan Visualization and Export](#13-vulkan-visualization-and-export)
14. [Benchmarking](#14-benchmarking)
15. [Key Physics and Conventions](#15-key-physics-and-conventions)
16. [Exploring the Codebase](#16-exploring-the-codebase)
17. [Common Pitfalls and Troubleshooting](#17-common-pitfalls-and-troubleshooting)

---

## 1. Prerequisites and Setup

### Required

- **.NET 10.0 SDK** (the project uses `net10.0` target framework)
  ```bash
  dotnet --version   # Should show 10.0.x
  ```

### Optional

- **CUDA 13.1+** for GPU acceleration (Phase I CUDA kernels, Phase II stubs)
  ```bash
  nvcc --version
  ```
- **Vulkan SDK 1.4+** for the visualization workbench
  ```bash
  vulkaninfo
  ```
- **CMake** for building native CUDA/Vulkan code under `native/`

### Clone and Verify

```bash
git clone <repo-url> GeometricUnity
cd GeometricUnity
dotnet build
```

If the build succeeds with 0 errors and 0 warnings, you are ready to go.

---

## 2. Building the Project

### Full Build

```bash
dotnet build
```

### Clean Build (recommended if you see stale errors)

```bash
dotnet clean && dotnet build
```

The solution uses the `.slnx` format (new in .NET 10). The file `GeometricUnity.slnx` lists all 26 source projects, 3 application projects, and 28 test projects.

### Global Settings

`Directory.Build.props` applies to all projects:
- Target: `net10.0`
- Nullable: enabled
- ImplicitUsings: enabled
- LangVersion: latest

### Building Native Code (CUDA/Vulkan)

```bash
cd native
cmake -B build
cmake --build build
```

This produces shared libraries consumed by `Gu.Interop` and `Gu.VulkanViewer` via P/Invoke. Not required for CPU-only usage.

---

## 3. Project Structure Overview

```
GeometricUnity/
├── apps/                              # Executable applications
│   ├── Gu.Cli/                        # Command-line interface (main entry point)
│   ├── Gu.Workbench/                  # Vulkan visualization tool
│   └── Gu.Benchmarks/                 # Performance benchmarking
│
├── src/                               # Source libraries (26 projects)
│   ├── Gu.Core/                       # Core types: BranchManifest, FieldTensor, etc.
│   ├── Gu.Math/                       # Lie algebras, structure constants, pairings
│   ├── Gu.Geometry/                   # Simplicial meshes, projections, quadrature
│   ├── Gu.Discretization/             # Discrete exterior calculus, covariant derivatives
│   ├── Gu.Branching/                  # Branch operators: torsion, Shiab interfaces
│   ├── Gu.ReferenceCpu/               # CPU reference: curvature, torsion, Shiab, Jacobian
│   ├── Gu.Solvers/                    # Solver modes A/B/C/D, convergence, gauge penalty
│   ├── Gu.Observation/                # Observation pipeline: pullback, transforms
│   ├── Gu.Validation/                 # Algebraic validation rules, parity checking
│   ├── Gu.Artifacts/                  # Run folders, replay contracts, integrity hashing
│   ├── Gu.Interop/                    # Native interop, GPU buffers, CUDA backend
│   ├── Gu.VulkanViewer/               # Vulkan visualization
│   ├── Gu.ExternalComparison/         # External comparison engine
│   ├── Gu.Symbolic/                   # Symbolic computation (placeholder)
│   ├── Gu.Phase2.Semantics/           # Branch family manifests, variant specs
│   ├── Gu.Phase2.Branches/            # Variant resolution, operator dispatch
│   ├── Gu.Phase2.Execution/           # Branch sweep runner, run records
│   ├── Gu.Phase2.Canonicity/          # Canonicity analysis, distance matrices
│   ├── Gu.Phase2.Stability/           # Linearization, Hessian, spectra, PDE classification
│   ├── Gu.Phase2.Recovery/            # Recovery DAG, identification gate
│   ├── Gu.Phase2.Continuation/        # Pseudo-arclength continuation, stability atlas
│   ├── Gu.Phase2.Predictions/         # Prediction test records, uncertainty budgets
│   ├── Gu.Phase2.Comparison/          # Comparison campaigns, dataset adapters
│   ├── Gu.Phase2.Reporting/           # Research reports, dashboards, batch runner
│   ├── Gu.Phase2.CudaInterop/         # Phase II GPU acceleration (stubs)
│   └── Gu.Phase2.Viz/                 # Phase II visualization
│
├── tests/                             # Test projects (28 projects, ~1631 tests)
├── native/                            # CUDA/Vulkan native code (CMake)
├── examples/                          # Toy geometries for testing
│   ├── toy_branch_2d/                 # 2D unit square (simplest)
│   ├── toy_branch_3d/                 # 3D test geometry
│   └── minimal_v1_4d/                 # 4D production reference (dim(X)=4, dim(Y)=14)
├── schemas/                           # 18 JSON schema files
├── benchmark-results/                 # Performance test results
├── IMPLEMENTATION_PLAN.md             # Phase I plan (M0-M12)
├── IMPLEMENTATION_PLAN_P2.md          # Phase II plan (M13-M22)
├── IMPLEMENTATION_PLAN_P3.md          # Phase III plan (boson extraction)
└── ARCHITECTURE_P2.md                 # Phase II architecture document
```

### Dependency Flow

```
Gu.Core  (types, metadata — no physics)
  ↓
Gu.Math  (Lie algebras, pairings)
  ↓
Gu.Geometry  (simplicial meshes, projections)
  ↓
Gu.Discretization  (discrete exterior calculus)
  ↓
Gu.Branching  (operator interfaces: ITorsionBranchOperator, IShiabBranchOperator)
  ↓
Gu.ReferenceCpu  (CPU implementations of all operators)
  ↓
Gu.Solvers  (SolverOrchestrator, modes A/B/C/D)
  ↓
Gu.Observation  (pullback pipeline, transforms)
  ↓
Gu.Artifacts  (run folders, replay, integrity)
  ↓
Gu.Validation  (algebraic checks)
  ↓
Gu.Phase2.*  (research instrumentation layer)
```

---

## 4. CLI Reference

The CLI is the primary user-facing tool. All commands run via:

```bash
dotnet run --project apps/Gu.Cli -- <command> [options]
```

### `create-branch` — Generate a Branch Manifest

```bash
dotnet run --project apps/Gu.Cli -- create-branch <branch-id> <output-path>
```

Creates a branch manifest JSON file with default values. Edit this file to configure your branch choices.

**Example:**
```bash
dotnet run --project apps/Gu.Cli -- create-branch my-su2-test branch.json
```

### `create-environment` — Generate an Environment Spec

```bash
dotnet run --project apps/Gu.Cli -- create-environment <env-id> <branch-id> <output-path>
```

Creates an environment specification with geometry, boundary conditions, initial conditions, and observable requests.

### `init-run` — Initialize a Run Folder

```bash
dotnet run --project apps/Gu.Cli -- init-run <run-folder> [branch-id]
```

Creates the canonical run folder directory structure:

```
run-folder/
├── manifest/      (branch.json, geometry.json, runtime.json)
├── state/         (initial_state.bin, final_state.bin, derived/)
├── residuals/     (residual_bundle.json)
├── linearization/ (linearization_bundle.json)
├── observed/      (observed_state.json)
├── validation/    (validation_bundle.json, records/)
├── integrity/     (file_hashes.json)
├── replay/        (reproduce.sh)
└── logs/          (solver.log, environment.txt)
```

### `run` — Execute Solver on Existing Run Folder

```bash
dotnet run --project apps/Gu.Cli -- run <run-folder> [options]
```

**Options:**
| Option | Default | Description |
|--------|---------|-------------|
| `--backend` | `cpu` | Backend: `cpu` or `cuda` |
| `--mode` | `A` | Solve mode: `A`, `B`, `C`, or `D` |
| `--lie-algebra` | `su2` | Gauge algebra: `su2` or `su3` |
| `--max-iter` | `100` | Maximum solver iterations |
| `--step-size` | `0.01` | Initial step size |
| `--method` | `GradientDescent` | Solver method: `GradientDescent`, `ConjugateGradient`, `GaussNewton` |
| `--gauge-lambda` | `0.0` | Gauge penalty coefficient |
| `--gauge-strategy` | `L2Penalty` | Gauge strategy: `L2Penalty` or `Coulomb` |

### `solve` — Init + Run in One Step

```bash
dotnet run --project apps/Gu.Cli -- solve <run-folder> [options]
```

Combines `init-run` and `run`. Same options as `run`. This is the easiest way to execute a complete solver run.

**Example — Run Mode B with Gauss-Newton on SU(2):**
```bash
dotnet run --project apps/Gu.Cli -- solve my-run \
  --backend cpu \
  --mode B \
  --method GaussNewton \
  --lie-algebra su2 \
  --max-iter 200 \
  --step-size 0.01 \
  --gauge-lambda 1.0
```

### `reproduce` — Replay a Previous Run

```bash
dotnet run --project apps/Gu.Cli -- reproduce <original-run-folder> [replay-folder] [--validate]
```

Re-executes a run using the stored replay contract. With `--validate`, checks results against the original at the declared replay tier.

### `sweep` — Branch Sensitivity Sweep

```bash
dotnet run --project apps/Gu.Cli -- sweep --branches b1.json,b2.json [--output dir] [options]
```

Runs the solver across multiple branch manifests to compare behavior under different branch choices.

### `validate-replay` — Validate Replay Tier

```bash
dotnet run --project apps/Gu.Cli -- validate-replay <original> <replay> [tier]
```

Validates a replayed run against the original at a specified tier:
- **R0**: Schema-only (archival verification)
- **R1**: Structural replay (same observed quantities)
- **R2**: Numerical replay (within tolerance)
- **R3**: Bit-exact cross-backend replay

### `verify-integrity` — Verify SHA-256 Hashes

```bash
dotnet run --project apps/Gu.Cli -- verify-integrity <run-folder>
```

Computes or verifies SHA-256 hashes for all files in the run folder.

### `validate-schema` — Validate JSON Against Schema

```bash
dotnet run --project apps/Gu.Cli -- validate-schema <file> <schema>
```

Validates a JSON file against one of the 18 JSON schemas in `schemas/`.

---

## 5. Running Your First Solver

### Quick Start: Mode A (Residual Evaluation)

The simplest run — evaluates the residual at the initial flat connection without optimization:

```bash
dotnet run --project apps/Gu.Cli -- solve my-first-run --mode A
```

This will:
1. Create the `my-first-run/` directory structure
2. Set up a default 2D toy geometry with SU(2) gauge algebra
3. Evaluate curvature F, torsion T, Shiab S, and residual Upsilon = S - T
4. Write all artifacts to disk
5. Compute integrity hashes

**Inspect the results:**
```bash
# View the branch manifest
cat my-first-run/manifest/branch.json | python3 -m json.tool

# View observed state
cat my-first-run/observed/observed_state.json | python3 -m json.tool

# View convergence (for modes B/C)
cat my-first-run/logs/solver.log
```

### Mode B: Objective Minimization

Minimize the objective functional I2_h = (1/2)||Upsilon||^2_M:

```bash
dotnet run --project apps/Gu.Cli -- solve mode-b-run \
  --mode B \
  --max-iter 100 \
  --step-size 0.01 \
  --gauge-lambda 1.0
```

The solver iterates with backtracking line search until:
- `totalObjective < objectiveTolerance` (converged), OR
- `maxIterations` reached (did not converge)

### Mode C: Stationarity Solve

Find a stationary point where the gradient vanishes:

```bash
dotnet run --project apps/Gu.Cli -- solve mode-c-run \
  --mode C \
  --method ConjugateGradient \
  --max-iter 200 \
  --step-size 0.001
```

Mode C converges when `gradientNorm < gradientTolerance` regardless of the objective value.

### Mode D: Branch Sensitivity

Analyze how the solution changes across different branch choices:

```bash
dotnet run --project apps/Gu.Cli -- solve mode-d-run --mode D
```

---

## 6. Understanding Branch Manifests

The **BranchManifest** is the single most important control object. It encodes every choice that is not uniquely determined by the theory.

### Key Fields

```json
{
  "branchId": "my-experiment",
  "schemaVersion": "1.0.0",
  "sourceEquationRevision": "r1",
  "codeRevision": "abc123",

  "baseDimension": 4,
  "ambientDimension": 14,
  "lieAlgebraId": "su2",

  "activeGeometryBranch": "simplicial",
  "activeObservationBranch": "sigma-pullback",
  "activeTorsionBranch": "trivial",
  "activeShiabBranch": "identity-shiab",
  "activeGaugeStrategy": "penalty",

  "basisConventionId": "canonical",
  "componentOrderId": "face-major",
  "adjointConventionId": "adjoint-explicit",
  "pairingConventionId": "pairing-trace",
  "normConventionId": "norm-l2-quadrature",
  "differentialFormMetricId": "hodge-standard",

  "insertedAssumptionIds": ["IA-1", "IA-5"],
  "insertedChoiceIds": ["IX-1", "IX-2"],

  "parameters": {
    "biConnectionStrategy": "sum",
    "a0Variant": "flat",
    "gaugePenaltyWeight": "1.0"
  }
}
```

### Branch-Sensitive Choices Explained

| Field | Code Reference | What It Controls |
|-------|---------------|-----------------|
| `activeTorsionBranch` | `ITorsionBranchOperator` | How torsion T is computed from the connection |
| `activeShiabBranch` | `IShiabBranchOperator` | How the Shiab operator S maps curvature |
| `activeGaugeStrategy` | `IGaugePenalty` | Gauge fixing method (L2 penalty or Coulomb) |
| `pairingConventionId` | `LieAlgebraPairing` | Inner product on Lie algebra (trace vs. Killing) |
| `differentialFormMetricId` | Hodge metric | Separate metric on differential forms |

### Inserted Assumptions (IA)

- **IA-1**: Simplicial discretization
- **IA-2**: Flat background metric
- **IA-3**: Compact gauge group
- **IA-4**: Gauge penalty regularization
- **IA-5**: CPU reference before GPU trust
- **IA-6**: Finite-dimensional approximation

### Inserted Choices (IX)

- **IX-1**: Torsion operator choice
- **IX-2**: Shiab operator choice
- **IX-3**: Bi-connection strategy
- **IX-4**: Observation map
- **IX-5**: Extraction method

### Available Torsion Operators

| ID | Class | Description |
|----|-------|-------------|
| `trivial` | `TrivialTorsionCpu` | T = 0 (no torsion) |
| `augmented` | `AugmentedTorsionCpu` | T^aug = d_{A0}(omega - A0) |

### Available Shiab Operators

| ID | Class | Description |
|----|-------|-------------|
| `identity-shiab` | `IdentityShiabCpu` | S = F (simplest: Shiab is the identity) |

### Lie Algebra Choices

| ID | Factory Method | Notes |
|----|---------------|-------|
| `su2` | `LieAlgebraFactory.CreateSu2WithTracePairing()` | 3D algebra, trace pairing (positive definite) |
| `su2-killing` | `LieAlgebraFactory.CreateSu2()` | Killing form g = -2*delta (NOT positive definite) |
| `su3` | `LieAlgebraFactory.CreateSu3()` | 8D algebra, Killing form g = -3*delta |

**Important:** Mode B solver requires positive-definite pairing. Use `pairing-trace` (not Killing) for convergence.

---

## 7. Geometry and Environment Specs

### Toy Geometries (examples/)

Three pre-built geometries are provided for testing:

#### 2D Toy (`examples/toy_branch_2d/`)

- Base manifold X: unit square, 5 vertices, 4 triangles
- Ambient space Y: 5D (fiber dimension = 3 for SU(2))
- Quadrature: centroid rule
- Basis: P1 simplex
- Use case: Quick debugging, unit tests

#### 3D Toy (`examples/toy_branch_3d/`)

- Base manifold X: 3D simplicial mesh
- Intermediate-size debugging

#### 4D Production (`examples/minimal_v1_4d/`)

- Base manifold X: dim(X) = 4 (physical target)
- Ambient space Y: dim(Y) = 14 (the observerse)
- SU(2) gauge algebra
- Solver budget: 200 iterations
- Use case: Production reference runs

### GeometryContext Structure

The `GeometryContext` encodes both semantic and discrete geometry:

```json
{
  "baseSpace": { "id": "X_h", "dimension": 4, "label": "base-manifold" },
  "ambientSpace": { "id": "Y_h", "dimension": 14, "label": "ambient-observerse" },
  "discretizationType": "simplicial",
  "quadratureRuleId": "centroid",
  "basisFamilyId": "P1-simplex-4d",
  "projectionBinding": {
    "description": "pi_h: Y_h -> X_h (many-to-one fiber projection)"
  },
  "observationBinding": {
    "description": "sigma_h: X_h -> Y_h (section, vertex-aligned)"
  }
}
```

### Key Geometry Concepts

- **pi_h (projection)**: Many-to-one fiber map Y -> X. Multiple ambient vertices map to one base vertex.
- **sigma_h (section)**: One-to-one section X -> Y. Each base vertex picks one ambient vertex.
- **Pullback sigma_h***: Maps Y-space quantities to X-space for observation. No Y-space quantity reaches comparison without passing through this.

### EnvironmentSpec Structure

A complete experiment specification:

```json
{
  "environmentId": "my-experiment",
  "branch": { "branchId": "my-branch", "schemaVersion": "1.0.0" },
  "scenarioType": "toy-consistency",
  "geometry": { "..." },
  "boundaryConditions": { "type": "periodic" },
  "initialConditions": { "type": "flat-connection" },
  "gaugeConditions": { "strategy": "penalty", "coefficient": 1.0 },
  "observableRequests": [
    { "observableId": "curvature", "outputType": "Quantitative" },
    { "observableId": "residual-norm", "outputType": "SemiQuantitative" },
    { "observableId": "topological-charge", "outputType": "ExactStructural" }
  ]
}
```

### Creating Custom Geometries Programmatically

```csharp
// From ToyGeometryFactory (for tests)
var bundle = ToyGeometryFactory.CreateToy2D();
var geometry = bundle.ToGeometryContext("centroid", "P1");

// Or build a mesh manually
var mesh = new SimplicialMesh(
    vertices: new double[,] { {0,0}, {1,0}, {0,1}, {1,1} },
    edges: new int[,] { {0,1}, {1,2}, ... },
    faces: new int[,] { {0,1,2}, {1,2,3} });
```

---

## 8. Solver Modes and Methods

### Four Solve Modes

| Mode | Enum Value | What It Does | Convergence Criterion |
|------|-----------|-------------|----------------------|
| **A** | `ResidualOnly` | Single evaluation, no optimization | N/A (one step) |
| **B** | `ObjectiveMinimization` | Minimize I2_h = (1/2) Upsilon^T M Upsilon | `totalObjective < tolerance` AND `totalObjective >= 0` |
| **C** | `StationaritySolve` | Find J^T M Upsilon + gauge = 0 | `gradientNorm < gradientTolerance` |
| **D** | `BranchSensitivity` | Sweep across branch parameters | Per-branch convergence |

### Three Solver Methods (for Modes B/C)

| Method | Algorithm | Best For |
|--------|-----------|----------|
| `GradientDescent` | Steepest descent + Armijo backtracking | Simple problems, debugging |
| `ConjugateGradient` | Polak-Ribiere + restart | Medium problems, faster convergence |
| `GaussNewton` | J^T J delta = -J^T M Upsilon (inner CG loop) | Near-convergence, quadratic rate |

### SolverOptions Reference

```csharp
var options = new SolverOptions
{
    // Core
    Mode = SolveMode.ObjectiveMinimization,   // A, B, C, or D
    Method = SolverMethod.GaussNewton,         // GradientDescent, ConjugateGradient, GaussNewton

    // Iteration control
    MaxIterations = 100,                       // Default: 100
    ObjectiveTolerance = 1e-10,                // Default: 1e-10
    GradientTolerance = 1e-8,                  // Default: 1e-8

    // Step size
    InitialStepSize = 0.01,                    // Default: 0.01

    // Gauge penalty
    GaugePenaltyLambda = 1.0,                  // Default: 0.0 (no gauge penalty)
    GaugeStrategy = GaugeStrategy.L2Penalty,   // L2Penalty or Coulomb

    // Line search
    ArmijoParameter = 1e-4,                    // Default: 1e-4
    BacktrackFactor = 0.5,                     // Default: 0.5
    MaxBacktrackSteps = 20,                    // Default: 20

    // Gauss-Newton CG inner loop
    MaxCgIterations = 50,                      // Default: 50
    CgTolerance = 1e-6,                        // Default: 1e-6
};
```

### Convergence History

Each solver iteration produces a `ConvergenceRecord`:

| Field | Description |
|-------|-------------|
| `Iteration` | Iteration number |
| `Objective` | I2_h value |
| `ResidualNorm` | ||Upsilon||_M |
| `GradientNorm` | ||J^T M Upsilon + gauge|| |
| `GaugeViolation` | Gauge constraint violation |
| `StepNorm` | ||delta omega|| |
| `StepSize` | Accepted step size after line search |
| `GaugeToPhysicsRatio` | GaugeViolation / ResidualNorm |

### SolverResult

```csharp
public sealed class SolverResult
{
    public bool Converged { get; init; }
    public string TerminationReason { get; init; }  // "converged", "max-iterations", etc.
    public double FinalObjective { get; init; }
    public double FinalResidualNorm { get; init; }
    public int Iterations { get; init; }
    public SolveMode SolveMode { get; init; }
    public IReadOnlyList<ConvergenceRecord> History { get; init; }
    public DiscreteState FinalState { get; init; }
    public DerivedState FinalDerivedState { get; init; }
    public ArtifactBundle ArtifactBundle { get; init; }
}
```

---

## 9. Observation Pipeline

### How It Works

The observation pipeline is the **sole path** from Y-space (ambient) quantities to X-space (base) observables. No Y-space quantity reaches comparison without passing through the declared pullback.

```
Y-space fields (DerivedState)
  → PullbackOperator (sigma_h*)          [Y_h → X_h]
  → IDerivedObservableTransform[]        [optional transforms]
  → INormalizationPolicy                 [normalization]
  → ObservableSnapshot (IsVerified=true) [output]
```

### Built-in Observable IDs

| Observable ID | Source Field | Description |
|--------------|-------------|-------------|
| `curvature` | CurvatureF | Curvature 2-form F = d(omega) + (1/2)[omega, omega] |
| `torsion` | TorsionT | Torsion field |
| `shiab` | ShiabS | Shiab operator output |
| `residual` | ResidualUpsilon | Residual Upsilon = S - T |

### Available Transforms

| Transform ID | Observable ID | Output |
|-------------|--------------|--------|
| `topological-charge-v1` | `topological-charge` | Proxy: sum of curvature coefficients |
| `curvature-norm` | `curvature-norm` | ||F|| (norm of curvature) |
| `residual-norm` | `residual-norm` | ||Upsilon|| |
| `objective-passthrough` | `objective` | I2_h direct pass |

### OutputType Classification

| Type | Meaning | Example |
|------|---------|---------|
| `ExactStructural` | Topological/discrete quantity | Topological charge |
| `SemiQuantitative` | Order-of-magnitude reliable | Curvature norm |
| `Quantitative` | Numerically precise | Converged residual |

### Using the Pipeline Programmatically

```csharp
var pullback = new PullbackOperator(fiberBundle);
var transforms = new IDerivedObservableTransform[]
{
    new TopologicalChargeTransform(),
    new CurvatureNormTransform(),
};
var normalization = new DimensionlessNormalizationPolicy();

var pipeline = new ObservationPipeline(pullback, transforms, normalization);

var requests = new[]
{
    new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
    new ObservableRequest { ObservableId = "topological-charge", OutputType = OutputType.ExactStructural },
};

var observed = pipeline.Extract(derivedState, discreteState, geometry, requests, manifest);

// Inspect results
foreach (var (id, snapshot) in observed.Observables)
{
    Console.WriteLine($"{id}: {snapshot.Values.Length} values");
    Console.WriteLine($"  Verified: {snapshot.Provenance.IsVerified}");
}
```

---

## 10. Artifacts, Replay, and Reproducibility

### Run Folder Layout

Every solver run produces a canonical directory:

```
my-run/
├── manifest/
│   ├── branch.json              # Full branch manifest (all choices)
│   ├── geometry.json            # Geometry specification
│   └── runtime.json             # Execution environment metadata
├── state/
│   ├── initial_state.bin        # omega_h at start
│   ├── final_state.bin          # omega_h at convergence
│   └── derived/
│       ├── curvature.bin        # F_omega
│       ├── torsion.bin          # T_omega
│       ├── shiab.bin            # S_omega
│       └── residual.bin         # Upsilon = S - T
├── residuals/
│   └── residual_bundle.json     # Residual metadata and components
├── linearization/
│   └── linearization_bundle.json  # Hessian, eigenvalues, eigenvectors
├── observed/
│   └── observed_state.json      # Extracted X-space observables
├── validation/
│   ├── validation_bundle.json   # Validation records
│   └── records/                 # Per-rule validation details
├── integrity/
│   └── file_hashes.json         # SHA-256 hashes of all files
├── replay/
│   └── reproduce.sh             # Bash script to re-execute
└── logs/
    ├── solver.log               # Iteration-by-iteration history
    └── environment.txt          # OS, runtime, hardware info
```

### ReplayContract and Tiers

Every artifact bundle includes a `ReplayContract` specifying reproducibility guarantees:

| Tier | Name | Guarantee |
|------|------|-----------|
| **R0** | Archival | Schema-only validation |
| **R1** | Structural | Same observed quantities (types, shapes) |
| **R2** | Numerical | Results within tolerance |
| **R3** | Bit-exact | Identical floating-point results across backends |

### Reproducing a Run

```bash
# Method 1: CLI reproduce command
dotnet run --project apps/Gu.Cli -- reproduce original-run/ replay-run/ --validate

# Method 2: Generated script
bash original-run/replay/reproduce.sh

# Verify integrity
dotnet run --project apps/Gu.Cli -- verify-integrity original-run/
```

### Integrity Hashing

SHA-256 hashes protect against accidental data corruption:

```bash
# Compute and store hashes
dotnet run --project apps/Gu.Cli -- verify-integrity my-run/

# Verify later
dotnet run --project apps/Gu.Cli -- verify-integrity my-run/
# Returns exit code 0 if all hashes match, non-zero otherwise
```

---

## 11. Running and Writing Tests

### Running All Tests

```bash
# ALWAYS use --no-build to avoid flaky mid-build failures
dotnet build && dotnet test --no-build
```

### Running Specific Test Projects

```bash
# Single project
dotnet build && dotnet test tests/Gu.Core.Tests --no-build

# Multiple projects
dotnet build && dotnet test tests/Gu.Solvers.Tests --no-build
dotnet build && dotnet test tests/Gu.ReferenceCpu.Tests --no-build
```

### Running Specific Tests by Name

```bash
# By class name
dotnet test --no-build --filter "ClassName=Gu.Core.Tests.EndToEndPipelineTests"

# By method name
dotnet test --no-build --filter "FullyQualifiedName~BranchManifest_RoundTrip"

# By trait
dotnet test --no-build --filter "Category=Integration"
```

### Test Project Inventory

| Project | Tests | Focus |
|---------|-------|-------|
| `Gu.Core.Tests` | ~309 | Type serialization, validation, end-to-end pipeline |
| `Gu.Geometry.Tests` | ~154 | Mesh construction, projection, quadrature |
| `Gu.Math.Tests` | ~20 | Lie algebra, structure constants, pairings |
| `Gu.ReferenceCpu.Tests` | ~151 | CPU curvature, torsion, Shiab, Jacobian |
| `Gu.Discretization.Tests` | ~30 | Discrete exterior calculus |
| `Gu.Solvers.Tests` | ~48 | Solver convergence, gauge penalty |
| `Gu.Observation.Tests` | ~25 | Pullback, transforms, pipeline |
| `Gu.Artifacts.Tests` | ~56 | Run folders, replay, integrity |
| `Gu.Interop.Tests` | ~151 | GPU buffers, CUDA backend |
| `Gu.VulkanViewer.Tests` | ~110 | Visualization data, color maps |
| `Gu.Validation.Tests` | ~81 | Algebraic rules, parity checks |
| `Gu.ExternalComparison.Tests` | ~59 | Comparison adapters, strategies |
| `Gu.Branching.Tests` | ~50 | Operator dispatch |
| `Gu.Phase2.*.Tests` | ~344 | All Phase II modules |

### Writing New Tests

Tests use **xUnit 2** with `[Fact]` and `[Theory]` attributes:

```csharp
using Xunit;

public class MyTests
{
    [Fact]
    public void SolverConverges_WithTrivialTorsion()
    {
        // Arrange
        var bundle = ToyGeometryFactory.CreateToy2D();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(bundle.AmbientMesh, algebra);
        var shiab = new IdentityShiabCpu(bundle.AmbientMesh, algebra);
        var backend = new CpuSolverBackend(bundle.AmbientMesh, algebra, torsion, shiab);

        var options = new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 50,
            InitialStepSize = 0.01,
        };

        var solver = new SolverOrchestrator(backend, options);
        var omega = ConnectionField.Zero(bundle.AmbientMesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(bundle.AmbientMesh, algebra).ToFieldTensor();
        var manifest = TestManifestFactory.CreateDefault();
        var geometry = bundle.ToGeometryContext("centroid", "P1");

        // Act
        var result = solver.Solve(omega, a0, manifest, geometry);

        // Assert
        Assert.True(result.Converged);
        Assert.True(result.FinalObjective < 1e-8);
    }

    [Theory]
    [InlineData("su2", 3)]
    [InlineData("su3", 8)]
    public void LieAlgebra_HasCorrectDimension(string id, int expectedDim)
    {
        var algebra = LieAlgebraFactory.Create(id);
        Assert.Equal(expectedDim, algebra.Dimension);
    }
}
```

### Key Test Patterns to Study

| Test File | What It Demonstrates |
|-----------|---------------------|
| `tests/Gu.Core.Tests/EndToEndPipelineTests.cs` | Full pipeline: geometry → solve → observe → artifacts |
| `tests/Gu.Solvers.Tests/` | Solver convergence for all modes/methods |
| `tests/Gu.ReferenceCpu.Tests/` | Physics: curvature assembly, Jacobian verification |
| `tests/Gu.Observation.Tests/` | Pullback, transforms, normalization |
| `tests/Gu.Artifacts.Tests/` | Run folder I/O, replay validation, integrity |
| `tests/Gu.Phase2.IntegrationTests/` | Phase II end-to-end workflows |

---

## 12. Phase II: Research Instrumentation

Phase II adds a research instrumentation layer on top of Phase I. It enables systematic exploration of branch-sensitive choices.

### Branch Families and Variants

A **BranchFamilyManifest** defines a family of related branch variants:

```csharp
var family = new BranchFamilyManifest
{
    FamilyId = "su2-torsion-study",
    Description = "Compare torsion operators for SU(2)",
    VariantIds = new[] { "trivial-torsion", "augmented-torsion" },
};
```

A **BranchVariantManifest** specifies a single variant's choices:

```csharp
var variant = new BranchVariantManifest
{
    Id = "variant-trivial",
    ParentFamilyId = "su2-torsion-study",
    TorsionVariant = "trivial",
    ShiabVariant = "identity-shiab",
    GaugeVariant = "penalty",
    PairingVariant = "pairing-trace",
    A0Variant = "a0-flat",
    BiConnectionVariant = "biconnection-sum",
    ObservationVariant = "sigma-pullback",
    ExtractionVariant = "direct",
    RegularityVariant = "c-infinity",
    ExpectedClaimCeiling = "Quantitative",
};
```

**Convert to Phase I manifest:**
```csharp
var manifest = BranchVariantResolver.Resolve(variant, baseManifest);
```

### Branch Sweep Runner

Execute solver across multiple variants:

```csharp
var runner = new Phase2BranchSweepRunner(backendFactory, solverOptions);
var results = runner.RunAll(variants, baseManifest, geometry);

foreach (var record in results)
{
    Console.WriteLine($"Variant {record.Variant.Id}: " +
        $"Converged={record.Converged}, " +
        $"Objective={record.FinalObjective:E6}");
}
```

### Canonicity Analysis

Track whether branch-sensitive objects are unique (canonical):

```csharp
var analyzer = new CanonicityAnalyzer();
var docket = analyzer.CreateDocket(
    objectClass: "torsion-operator",
    variants: new[] { "trivial", "augmented" },
    distanceMatrix: pairwiseDistances);

// Docket status: Open, Closed (proven unique), or Disputed
Console.WriteLine($"Status: {docket.Status}");
Console.WriteLine($"Evidence: {docket.CurrentEvidence.Count} records");
```

### Stability Analysis (Linearization Workbench)

Compute Hessian and spectrum at a solution:

```csharp
var hessian = new HessianOperator(jacobian, massMatrix, gaugeConstraint);
var probe = new LanczosSpectrumProbe(maxIterations: 100);
var spectrum = probe.ComputeExtremeEigenvalues(hessian, numEigenvalues: 10);

var summary = new HessianSummary
{
    SmallestEigenvalue = spectrum.Min(),
    NegativeModeCount = spectrum.Count(e => e < 0),
    SoftModeCount = spectrum.Count(e => e > 0 && e < 1e-6),
    StabilityClassification = ClassifyStability(spectrum),
};
```

**Stability classifications:**
- `strictly-positive-on-slice`: All eigenvalues positive (stable minimum)
- `soft-modes-present`: Small positive eigenvalues (nearly flat directions)
- `near-zero-kernel`: Near-zero eigenvalues (approximate symmetry)
- `negative-modes-saddle`: Negative eigenvalues (saddle point, unstable)

### Continuation Framework

Follow solution families G(u, lambda) = 0 as parameters vary:

```csharp
var spec = new ContinuationSpec
{
    ParameterName = "gauge-lambda",
    LambdaStart = 0.0,
    LambdaEnd = 10.0,
    InitialStepSize = 0.1,
    MaxSteps = 100,
    ProbeSpectrum = true,
    SpectrumProbeCount = 6,
};

var runner = new PseudoArclengthContinuationRunner(backend, spec);
var result = runner.Run(initialSolution, manifest, geometry);

// Inspect bifurcation points
foreach (var event in result.Events)
{
    Console.WriteLine($"At lambda={event.Lambda}: {event.EventType}");
}
```

### Recovery DAG

Trace how native Y-space quantities become physical observables:

```
Native (Y-space)  →  Observation (sigma_h*)  →  Extraction  →  Interpretation
```

Each node carries a `ClaimClass` ceiling:
- `Structural`: Topological/discrete (strongest)
- `SemiQuantitative`: Order-of-magnitude
- `Quantitative`: Numerically precise (weakest, easiest to falsify)

### Identification Gate

The 6-field physical claim with demotion rules:

```csharp
var gate = new IdentificationGate();
var result = gate.Evaluate(
    convergenceField: convergenceRecord,
    canonicityField: docketStatus,
    stabilityField: hessianSummary,
    extractionField: observableSnapshot,
    comparisonField: comparisonResult,
    uncertaintyField: uncertaintyRecord);

Console.WriteLine($"Claim class: {result.ClaimClass}");  // Structural, SemiQuantitative, Quantitative
Console.WriteLine($"Justification: {result.Justification}");
Console.WriteLine($"Demoted: {result.WasDemoted}");
Console.WriteLine($"Demotion reason: {result.DemotionReason}");
```

### Predictions and Comparison Campaigns

```csharp
var prediction = new PredictionTestRecord
{
    TestId = "boson-mass-ratio",
    ClaimClass = ClaimClass.SemiQuantitative,
    FormalSource = "GU bosonic branch equation",
    BranchManifestId = "my-branch",
    ObservableMapId = "mass-spectrum",
    TheoremDependencyStatus = "open",
    NumericalDependencyStatus = "converged",
    ApproximationStatus = "leading-order",
    Falsifier = "Mass ratio differs from SM by more than 2 orders of magnitude",
};

var campaign = new ComparisonCampaignRunner();
var results = campaign.Run(predictions, externalData, comparisonStrategy);
```

---

## 13. Vulkan Visualization and Export

### Launching the Workbench

```bash
dotnet run --project apps/Gu.Workbench -- <run-folder> [options]
```

**Options:**
| Option | Description |
|--------|-------------|
| `--export-obj <path>` | Export mesh to Wavefront OBJ format |
| `--export-ply <path>` | Export mesh to PLY format |
| `--export-csv <path>` | Export convergence data to CSV |
| `--color-scheme <name>` | Color scheme: `viridis`, `plasma`, `coolwarm`, `diverging` |

### Export Examples

```bash
# Export mesh with curvature coloring
dotnet run --project apps/Gu.Workbench -- my-run/ --export-obj mesh.obj --color-scheme viridis

# Export convergence history
dotnet run --project apps/Gu.Workbench -- my-run/ --export-csv convergence.csv
```

### Visualization Features

- Handles meshes of any embedding dimension (projects to 3D)
- Per-vertex scalar field coloring
- Supports fixed or auto-computed color ranges
- Zero-centering for diverging quantities (residuals, differences)
- Strictly read-only: never modifies run artifacts

### Programmatic Visualization

```csharp
var visualizer = new ScalarFieldVisualizer(ColorScheme.Viridis);
var vizData = visualizer.PrepareVisualization(curvatureField, mesh);

// vizData contains:
//   Positions: float[] [x0,y0,z0, x1,y1,z1, ...]
//   Colors: float[] [r0,g0,b0,a0, r1,g1,b1,a1, ...]
//   Indices: uint[] triangle indices
//   VertexCount, TriangleCount
```

---

## 14. Benchmarking

### Running Benchmarks

```bash
dotnet run --project apps/Gu.Benchmarks
```

Results are written to `benchmark-results/`.

### Available Benchmarks

| Benchmark | Description |
|-----------|-------------|
| Scaling (Mode A) | Residual-only on increasing problem sizes (N=10, 50, 100, 500, 1000) |
| Solve (Mode B) | Gradient descent convergence timing |
| CPU vs CPU parity | Reference implementation self-consistency |
| CPU vs CUDA parity | GPU kernel validation against CPU reference |
| GPU solve (Mode A) | CUDA-accelerated residual evaluation |
| Mesh scaling | Real simplicial mesh performance (N=100, 1K, 10K, 100K) |

### Interpreting Results

Benchmark reports include:
- Wall-clock time per iteration
- Memory allocation per step
- Convergence rate (iterations to tolerance)
- Parity deviation (max absolute difference vs reference)

---

## 15. Key Physics and Conventions

### Core Equation

The Geometric Unity bosonic branch seeks solutions to:

```
D_omega* Upsilon_omega = 0
```

where:
- `omega` is a connection on principal bundle P -> Y
- `Upsilon = S_omega - T_omega` (Shiab minus torsion)
- `D_omega*` is the gauge-covariant codifferential

### Curvature

```
F = d(omega) + (1/2)[omega, omega]
```

**Linearization (critical 0.5 factor):**
```
dF/domega(delta) = d(delta) + 0.5 * sum_{i<j} ([omega_i, delta_j] + [delta_i, omega_j])
```

### Augmented Torsion

```
T^aug = d_{A0}(omega - A0)
```

This is linear in omega with constant Jacobian dT/domega = d_{A0}.

### Covariant Exterior Derivative

```
d_{A0}(beta) = d(beta) + [A0 wedge beta]
```

The bracket term has NO 0.5 factor (unlike the curvature self-wedge).

### Bi-Connection Strategy

Two strategies for constructing the bi-connection from A0 and omega:

| Strategy | A | B | Notes |
|----------|---|---|-------|
| Sum | A0 + omega | A0 - omega | Standard (BiConnectionBuilder) |
| Simple | A0 | omega | Simplest (SimpleBiConnectionCpu) |

### Objective Functional

```
I2_h = (1/2) Upsilon^T M Upsilon
```

where M is the mass matrix (from quadrature + metric).

### Gradient

```
G = J^T M Upsilon + lambda * gauge_gradient
```

where J = dUpsilon/domega is the Jacobian.

### Gauge Penalty

| Strategy | Formula | Notes |
|----------|---------|-------|
| L2 Penalty | (lambda/2) \|\|omega - omega_ref\|\|^2 | Simple norm penalty |
| Coulomb | (lambda/2) \|\|d_{A0}^*(omega - omega_ref)\|\|^2 | Proper gauge (uses codifferential) |

### Dimensions

| Quantity | Symbol | Default |
|----------|--------|---------|
| Base manifold | dim(X) | 4 |
| Ambient space (observerse) | dim(Y) | 14 |
| Fiber | dim(Y) - dim(X) | 10 |
| SU(2) Lie algebra | dim(G) | 3 |
| SU(3) Lie algebra | dim(G) | 8 |

### Lie Algebra Pairings

| Pairing | Formula | Positive Definite? | Use With |
|---------|---------|-------------------|----------|
| Trace | delta_{ab} | Yes | Mode B solver (required) |
| Killing (SU(2)) | -2 * delta_{ab} | No | Analysis only |
| Killing (SU(3)) | -3 * delta_{ab} | No | Analysis only |

**Important:** Mode B requires positive-definite pairing. Killing form gives negative objective, triggering immediate convergence via `totalObjective < tolerance`.

---

## 16. Exploring the Codebase

### Finding Key Types

| To find... | Look in... |
|------------|-----------|
| Core data types | `src/Gu.Core/*.cs` |
| Branch manifest | `src/Gu.Core/BranchManifest.cs` |
| Solver orchestration | `src/Gu.Solvers/SolverOrchestrator.cs` |
| Solver options/modes | `src/Gu.Solvers/SolverTypes.cs` |
| CPU physics | `src/Gu.ReferenceCpu/` |
| Curvature assembly | `src/Gu.ReferenceCpu/CurvatureAssembler.cs` |
| Lie algebras | `src/Gu.Math/LieAlgebraFactory.cs` |
| Mesh types | `src/Gu.Geometry/SimplicialMesh.cs` |
| Observation pipeline | `src/Gu.Observation/ObservationPipeline.cs` |
| Artifact I/O | `src/Gu.Artifacts/RunFolderWriter.cs` |
| JSON schemas | `schemas/*.schema.json` |
| Example configs | `examples/toy_branch_2d/` |
| CLI entry point | `apps/Gu.Cli/Program.cs` |

### JSON Schemas

18 JSON schemas in `schemas/` define the contract for all JSON files:

| Schema | Validates |
|--------|-----------|
| `branch.schema.json` | BranchManifest |
| `geometry.schema.json` | GeometryContext |
| `observed.schema.json` | ObservedState |
| `artifact.schema.json` | ArtifactBundle |
| `validation.schema.json` | ValidationBundle |
| `branch_family.schema.json` | BranchFamilyManifest |
| `branch_variant.schema.json` | BranchVariantManifest |
| `study_spec.schema.json` | BranchSweepSpec, StabilityStudySpec |
| `stability_atlas.schema.json` | StabilityAtlasResult |
| `continuation_result.schema.json` | ContinuationRunResult |
| `prediction_test_record.schema.json` | PredictionTestRecord |
| `comparison_campaign.schema.json` | ComparisonCampaignSpec |
| `research_report.schema.json` | ResearchReportDocument |
| `branch_sweep_result.schema.json` | BranchSweepResult |

### Reading the Implementation Plans

| Document | Lines | Coverage |
|----------|-------|----------|
| `IMPLEMENTATION_PLAN.md` | 1,830 | Phase I (M0-M12): core bosonic dynamics |
| `IMPLEMENTATION_PLAN_P2.md` | 1,898 | Phase II (M13-M22): research instrumentation |
| `IMPLEMENTATION_PLAN_P3.md` | 1,643 | Phase III: boson spectrum extraction |
| `ARCHITECTURE_P2.md` | 815 | Phase II architecture details |

---

## 17. Common Pitfalls and Troubleshooting

### Build Issues

**Problem:** Stale build cache causes phantom test failures.
**Solution:** Always use `dotnet clean && dotnet build` before testing.

**Problem:** `dotnet test` shows flaky failures.
**Solution:** Always use `dotnet build && dotnet test --no-build` to prevent mid-test rebuilds.

**Problem:** `Gu.Math` namespace conflicts with `System.Math`.
**Solution:** Use `System.Math.Sqrt()` explicitly in files that import `Gu.Math`.

### Solver Issues

**Problem:** Mode B converges immediately with zero iterations.
**Solution:** Check your Lie algebra pairing. Killing form (g = -2I) gives negative objective, triggering `totalObjective < tolerance`. Use trace pairing (`CreateSu2WithTracePairing()`).

**Problem:** Mode B diverges.
**Solution:** Reduce `InitialStepSize` (try 0.001 or 0.0001). Increase `MaxBacktrackSteps`. Check that gauge penalty lambda is not too large.

**Problem:** Mode C never converges.
**Solution:** Mode C requires `gradientNorm < gradientTolerance`, independent of objective. Try smaller `GradientTolerance` or more iterations.

### Type System Issues

**Problem:** Cannot use `with` syntax on core types.
**Solution:** Core types are sealed classes, NOT records. Use constructor/property initialization.

**Problem:** `record` used accidentally causes `, with` compilation error.
**Solution:** Replace with manual property copying.

### Test Issues

**Problem:** Test discovers wrong test assembly.
**Solution:** Specify the test project explicitly: `dotnet test tests/Gu.Core.Tests --no-build`

**Problem:** SU(3) structure constants seem wrong.
**Solution:** Ensure `SetTotallyAntiSymmetric` is used (all 6 permutations of indices).

### Physics Issues

**Problem:** Jacobian finite-difference verification fails.
**Solution:** Check the 0.5 factor in curvature linearization. The formula is:
```
dF/domega(delta) = d(delta) + 0.5 * sum_{i<j}([omega_i, delta_j] + [delta_i, omega_j])
```

**Problem:** Pullback output has wrong length.
**Solution:** Pullback of Lie-algebra-valued 2-forms: output length = `BaseMesh.FaceCount * dimG`, not just `FaceCount`.

**Problem:** Covariant derivative d_{A0}(beta) gives wrong results.
**Solution:** The bracket term `[A0 wedge beta]` has NO 0.5 factor (unlike the curvature self-wedge).

---

## Appendix A: Complete Type Reference (Gu.Core)

30 public sealed classes:

| Type | Purpose |
|------|---------|
| `BranchRef` | Simple branch reference (ID + schema version) |
| `BranchManifest` | Central control object with all branch choices |
| `FieldTensor` | Discrete field data with metadata (immutable coefficients) |
| `TensorSignature` | Tensor type metadata (degree, algebra, ordering) |
| `GeometryContext` | Combined semantic + discrete geometry |
| `SpaceRef` | Space reference (ID, dimension, label) |
| `PatchInfo` | Patch metadata |
| `GeometryBinding` | Projection/observation map descriptor |
| `DiscreteState` | Branch + geometry + omega field |
| `DerivedState` | Curvature F, torsion T, Shiab S, residual Upsilon |
| `ObservedState` | X-space observables after pullback |
| `ObservableRequest` | Query for specific observable |
| `ObservableSnapshot` | Single observable result with provenance |
| `ObservationProvenance` | Observable extraction trace |
| `NormalizationMeta` | Normalization policy metadata |
| `LinearizationState` | Linearized operator model, spectral data |
| `LinearOperatorModel` | Hessian, mass matrices, spectrum |
| `SolverResult` | Convergence history, final state, termination |
| `ResidualBundle` | Residual field components |
| `ResidualComponent` | Single residual field component |
| `ValidationBundle` | Validation records collection |
| `ValidationRecord` | Single validation check result |
| `ValidationStamp` | Validation record with timestamp and rule ID |
| `ArtifactBundle` | All outputs from a solver run |
| `EnvironmentSpec` | Experiment parameters |
| `BoundaryConditionBundle` | Boundary conditions |
| `InitialConditionBundle` | Initial state configuration |
| `GaugeConditionBundle` | Gauge strategy configuration |
| `IntegrityBundle` | SHA-256 file hashes |
| `ReplayContract` | Replay tier and validation requirements |
| `ProvenanceMeta` | Creation timestamp, code revision, branch |
| `InsertedAssumption` | Documentation of assumptions |

## Appendix B: Interface Reference

### Phase I Interfaces

| Interface | Module | Purpose |
|-----------|--------|---------|
| `ITorsionBranchOperator` | Gu.Branching | Compute torsion T from connection |
| `IShiabBranchOperator` | Gu.Branching | Compute Shiab S from curvature |
| `IBiConnectionStrategy` | Gu.Branching | Map state to (A, B) pair |
| `ILinearOperator` | Gu.Branching | Matrix-free Apply(v) and ApplyTranspose(w) |
| `ISolverBackend` | Gu.Solvers | Backend-agnostic solver interface |
| `IGaugePenalty` | Gu.Solvers | Gauge penalty evaluation |
| `IDerivedObservableTransform` | Gu.Observation | Transform derived quantities |
| `INormalizationPolicy` | Gu.Observation | Normalize observables |
| `IValidationRule` | Gu.Validation | Single algebraic check |
| `IComparisonAdapter` | Gu.ExternalComparison | Adapt external datasets |
| `IComparisonStrategy` | Gu.ExternalComparison | Compare predictions vs reference |
| `INativeBackend` | Gu.Interop | P/Invoke to native CUDA/Vulkan |
| `IFieldVisualizer` | Gu.VulkanViewer | Render field data |

### Phase II Interfaces

| Interface | Module | Purpose |
|-----------|--------|---------|
| `IPhase2JacobianKernel` | Gu.Phase2.CudaInterop | GPU Jacobian-vector product |
| `IPhase2HessianKernel` | Gu.Phase2.CudaInterop | GPU Hessian-vector product |
| `IPhase2BatchKernel` | Gu.Phase2.CudaInterop | GPU batch processing |
| `ISpectrumProbe` | Gu.Phase2.Stability | Eigenvalue computation |

## Appendix C: Suggested Explorations

### For Understanding the Core Physics

1. Run Mode A on the 2D toy and inspect all derived fields
2. Run Mode B with trace pairing and watch convergence history
3. Compare trivial vs augmented torsion on the same geometry
4. Verify Jacobian accuracy with finite-difference tests

### For Understanding Branch Sensitivity

1. Create two branch manifests differing only in torsion operator
2. Run both with identical solver options
3. Compare observables: do they converge to the same solution?
4. Use Mode D for automated sensitivity analysis

### For Understanding Reproducibility

1. Run a solver, then reproduce it with `reproduce`
2. Validate at tier R2 (numerical tolerance)
3. Verify integrity hashes before and after
4. Modify one artifact file and verify that integrity check fails

### For Understanding Phase II

1. Create a branch family with 3+ variants
2. Run a branch sweep and inspect all run records
3. Build a canonicity docket from pairwise distances
4. Compute Hessian eigenvalues at a converged solution
5. Run pseudo-arclength continuation on gauge-lambda

### For Performance Analysis

1. Run benchmarks on increasing mesh sizes
2. Compare gradient descent vs conjugate gradient vs Gauss-Newton
3. Profile time per iteration across solver methods
4. If CUDA available, compare CPU vs GPU parity and timing
