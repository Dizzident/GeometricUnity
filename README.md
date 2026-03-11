# Geometric Unity

A reproducible research engine implementing the first executable bosonic branch of Eric Weinstein's Geometric Unity framework. Connection-centered, observerse-based, explicitly discretized, with CPU reference backend, CUDA acceleration, Vulkan visualization, a full Phase II research instrumentation layer for systematic branch-independence studies, and a Phase III boson spectrum extraction pipeline for candidate boson identification and comparison campaigns.

## Overview

This system implements the **minimal bosonic executable branch** of the Geometric Unity completion program:

- **Connection omega** on a principal bundle P -> Y is the primary dynamical variable
- **Curvature** F_omega, **torsion** T_omega, and **Shiab** S_omega define the residual Upsilon = S - T
- **Variational objective** I2(omega) = (1/2) integral of Upsilon, Upsilon drives the solver
- **Observation pipeline** maps Y-space quantities to X-space observables via pullback sigma_h*
- **Branch manifest** controls all choices — no silent hardcoding
- **Branch families** (Phase II) parameterize variant choices for systematic independence studies
- **Linearization workbench** classifies PDE type, computes spectra, and probes stability
- **Recovery graphs** trace the full chain from native fields to physical identification
- **Comparison campaigns** validate predictions against external datasets with uncertainty decomposition
- **Background atlas** (Phase III) builds a catalog of stationary background connections
- **Boson spectrum** (Phase III) extracts fluctuation eigenmodes via Lanczos on the Hessian
- **Mode tracking** (Phase III) follows mode families across backgrounds with split/merge detection
- **Candidate boson registry** (Phase III) assembles, classifies, and compares candidate particles

The system is designed as a **research platform**, not a one-off solver.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Optional) CUDA Toolkit 13.1+ for GPU acceleration
- (Optional) Vulkan SDK 1.4+ for visualization

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet build && dotnet test --no-build
```

There are 2,252 tests across 38 test projects covering core types, geometry, reference CPU operators, interop, validation, artifacts, observation, external comparison, all Phase II modules (semantics, branches, execution, canonicity, stability, recovery, continuation, predictions, comparison, reporting), and all Phase III modules (backgrounds, gauge reduction, spectra, mode tracking, properties, observables, registry, CUDA spectra, campaigns, reporting).

## CLI Usage

The `Gu.Cli` application provides commands for working with branch manifests, run folders, and replay validation.

```bash
# Run the CLI
dotnet run --project apps/Gu.Cli -- <command> [args]
```

### Commands

**Create a branch manifest:**
```bash
dotnet run --project apps/Gu.Cli -- create-branch [branchId] [outputPath]
```
Creates an empty branch manifest JSON file. Default branch ID is `minimal-gu-v1`.

**Create an environment spec:**
```bash
dotnet run --project apps/Gu.Cli -- create-environment [environmentId] [branchId] [outputPath]
```
Creates an empty environment specification for running controlled experiments.

**Initialize a run folder:**
```bash
dotnet run --project apps/Gu.Cli -- init-run <run-folder> [branchId]
```
Creates a canonical run folder with the standard directory structure, branch manifest, and runtime info.

**Validate replay:**
```bash
dotnet run --project apps/Gu.Cli -- validate-replay <original-run-folder> <replay-run-folder> [tier]
```
Validates that a replay run matches the original at the specified tier (R0/R1/R2/R3). Default tier is R2.

**Verify integrity:**
```bash
dotnet run --project apps/Gu.Cli -- verify-integrity <run-folder>
```
Computes or verifies SHA-256 integrity hashes for all files in a run folder.

**Phase III — Boson Spectrum:**
```bash
dotnet run --project apps/Gu.Cli -- create-background-study [output.json]
dotnet run --project apps/Gu.Cli -- solve-backgrounds <study.json> [--output <dir>] [--lie-algebra su2|su3]
dotnet run --project apps/Gu.Cli -- compute-spectrum <run-folder> <backgroundId> [--num-modes N] [--formulation p1|p2]
dotnet run --project apps/Gu.Cli -- track-modes <run-folder> [--context continuation|branch|refinement]
dotnet run --project apps/Gu.Cli -- build-boson-registry <run-folder>
dotnet run --project apps/Gu.Cli -- run-boson-campaign <run-folder> [--campaign <campaignSpec.json>]
dotnet run --project apps/Gu.Cli -- export-boson-report <run-folder> [options]
```

## Running Benchmarks

```bash
dotnet run --project apps/Gu.Benchmarks
```

Runs scaling benchmarks (Mode A residual-only), solve benchmarks (Mode B gradient descent), and CPU/GPU parity benchmarks. Results are written to `benchmark-results/`.

## Project Structure

```
GeometricUnity/
├── apps/
│   ├── Gu.Cli/                    # Command-line interface
│   ├── Gu.Workbench/              # Interactive workbench (Vulkan)
│   └── Gu.Benchmarks/             # Performance benchmarks
├── src/                           # 36 source libraries
│   ├── Gu.Core/                   # Core types: BranchManifest, FieldTensor, TensorSignature
│   ├── Gu.Math/                   # Lie algebras, structure constants, pairings
│   ├── Gu.Branching/              # Branch operators: torsion, Shiab interfaces
│   ├── Gu.Geometry/               # Simplicial meshes, projections, quadrature
│   ├── Gu.Discretization/         # Discrete exterior calculus, covariant derivatives
│   ├── Gu.ReferenceCpu/           # CPU reference: curvature, torsion, Shiab, Jacobian
│   ├── Gu.Solvers/                # Solver modes (A/B/C/D), gauge penalty, convergence
│   ├── Gu.Observation/            # Observation pipeline: pullback, transforms
│   ├── Gu.Validation/             # Algebraic validation rules, parity checking
│   ├── Gu.Artifacts/              # Run folders, replay contracts, integrity hashing
│   ├── Gu.Interop/                # Native interop, GPU buffers, CUDA backend
│   ├── Gu.VulkanViewer/           # Vulkan visualization (read-only from artifacts)
│   ├── Gu.ExternalComparison/     # External comparison engine
│   ├── Gu.Symbolic/               # Symbolic computation (placeholder)
│   ├── Gu.Phase2.Semantics/       # Branch family manifests, variant specs, claim classes
│   ├── Gu.Phase2.Branches/        # Variant resolution, operator dispatch, validation
│   ├── Gu.Phase2.Execution/       # Phase II branch sweep runner, run records
│   ├── Gu.Phase2.Canonicity/      # Canonicity analysis, pairwise distance, fragility
│   ├── Gu.Phase2.Stability/       # Linearization workbench, Hessian, spectra, PDE classification
│   ├── Gu.Phase2.Recovery/        # Recovery DAG, identification gate, claim demotion
│   ├── Gu.Phase2.Continuation/    # Pseudo-arclength continuation, stability atlas
│   ├── Gu.Phase2.Predictions/     # Prediction test records, uncertainty budgets
│   ├── Gu.Phase2.Comparison/      # Comparison campaigns, dataset adapters, strategies
│   ├── Gu.Phase2.Reporting/       # Research reports, dashboards, batch runner
│   ├── Gu.Phase2.CudaInterop/     # Phase II GPU acceleration (stubs)
│   ├── Gu.Phase2.Viz/             # Phase II visualization
│   ├── Gu.Phase3.Backgrounds/     # Background atlas: stationary connection catalog
│   ├── Gu.Phase3.GaugeReduction/  # Gauge reduction, Coulomb slice, zero-mode removal
│   ├── Gu.Phase3.Spectra/         # Lanczos eigensolver, spectrum bundles
│   ├── Gu.Phase3.ModeTracking/    # Mode family tracking, split/merge/crossing detection
│   ├── Gu.Phase3.Properties/      # Mass, spin, charge extraction from eigenmodes
│   ├── Gu.Phase3.Observables/     # Observation normalization, dispersion fitting
│   ├── Gu.Phase3.Registry/        # Candidate boson registry, claim classification
│   ├── Gu.Phase3.CudaSpectra/     # GPU-accelerated Lanczos (CUDA)
│   ├── Gu.Phase3.Campaigns/       # Boson comparison campaigns
│   └── Gu.Phase3.Reporting/       # Boson atlas reports, dashboards
├── tests/                         # 38 test projects (Phase I + Phase II + Phase III)
├── native/                        # CUDA kernels (C/CUDA)
├── examples/                      # Toy 2D/3D/4D geometries for debugging
└── schemas/                       # 26 JSON schemas for all phases
```

## Architecture

The system enforces three hard separations:

1. **C# orchestration** — types, metadata, control flow, serialization
2. **Native/CUDA compute** — numerical kernels for heavy workloads
3. **Vulkan visualization** — read-only, artifact-driven diagnostics

### Core Pipeline

```
omega (connection)
  → F_omega (curvature)
  → T_omega (torsion branch) + S_omega (Shiab branch)
  → Upsilon = S - T (residual)
  → I2 = (1/2) ⟨Upsilon, Upsilon⟩ (objective)
  → J = dUpsilon/domega (Jacobian)
  → G = J^T M Upsilon (gradient)
  → solver update → iterate
  → sigma_h* pullback → observed state
  → validation → artifacts
```

### Solver Modes

- **Mode A** — Residual-only: assemble and inspect Upsilon without solving
- **Mode B** — Objective minimization: gradient descent with optional gauge penalty
- **Mode C** — Stationarity solve: drive ||J^T M Upsilon|| -> 0
- **Mode D** — Branch sensitivity: sweep variant parameters and compare outcomes

### Phase II Research Pipeline

```
Branch family (variant parameters)
  -> Phase2BranchSweepRunner (parallel variant execution)
  -> CanonicityAnalyzer (pairwise distance, fragility, equivalence classes)
  -> LinearizationWorkbench (Hessian, spectrum, PDE classification)
  -> ContinuationRunner (pseudo-arclength, bifurcation detection)
  -> StabilityAtlasBuilder (stability regions over parameter space)
  -> RecoveryGraph (native -> observed -> extracted -> identified)
  -> IdentificationGate (6-field physical ID with claim demotion)
  -> PredictionValidator (uncertainty decomposition, falsifiers)
  -> CampaignRunner (structural/semi-quantitative/quantitative comparison)
  -> ResearchReportGenerator (summary, dashboards, negative results)
```

### Phase III Boson Spectrum Pipeline

```
BackgroundStudySpec (parameter sweep over background connections)
  -> BackgroundAtlasBuilder (solve + catalog stationary backgrounds)
  -> GaugeReductionOperator (Coulomb slice, zero-mode removal)
  -> LanczosSpectrumProbe (M-orthogonal Krylov, GPU-accelerated)
  -> SpectrumBundle (eigenmodes with ComputedWithBackend provenance)
  -> ModeMatchingEngine (overlap O2, split/merge/avoided-crossing detection)
  -> ModeFamily / ModeFamilyTracker (cross-background mode threading)
  -> MassExtractor / SpinExtractor / ChargeExtractor (property extraction)
  -> DispersionFitMassExtractor (dispersion curve fitting)
  -> CandidateBosonRecord (Polarization, Symmetry, InteractionProxy envelope)
  -> BosonRegistry (claim classes C0-C5, 7 demotion rules)
  -> BosonCampaignRunner (comparison campaigns)
  -> BosonReportGenerator (atlas report, dashboards)
```

### Replay Tiers

- **R0** — Schema-only (archival)
- **R1** — Observable-invariant structural replay
- **R2** — Numerical replay (required for validation)
- **R3** — Bit-exact cross-backend replay (required for parity claims)

## Canonical Run Folder

Every run produces a self-contained artifact folder:

```
run/
  manifest/       # branch.json, geometry.json, runtime.json
  state/          # initial_state.bin, final_state.bin, derived/
  residuals/      # residual_bundle.json
  linearization/  # linearization_bundle.json
  observed/       # observed_state.json
  validation/     # validation_bundle.json, records/
  integrity/      # SHA-256 hashes
  replay/         # replay_contract.json
  logs/           # solver.log, environment.txt
```

## Examples

Toy geometries are provided for testing and debugging:

- `examples/toy_branch_2d/` — 2D base manifold (unit square, 4 triangles)
- `examples/toy_branch_3d/` — 3D base manifold

These are small enough for CPU debugging and step-through verification.

## Workbench (Visualization)

The Vulkan-based workbench loads run folder artifacts for diagnostic visualization and export:

```bash
dotnet run --project apps/Gu.Workbench -- <run-folder> [options]
  --export-obj <path>       Export mesh to OBJ format
  --export-ply <path>       Export mesh to PLY format
  --export-csv <path>       Export convergence history to CSV
  --color-scheme <scheme>   viridis | plasma | coolwarm | diverging
```

Visualization is strictly **read-only** — it consumes artifact snapshots and never modifies computation state.

## Key Design Principles

- **Branch manifest controls everything** — all variable choices (torsion operator, Shiab operator, Lie algebra, gauge parameters) are declared in the manifest and serialized into artifacts
- **CPU reference before CUDA trust** — every GPU kernel must match a verified CPU reference implementation
- **Observation discipline** — no Y-space quantity reaches comparison without passing through the σ_h* pullback
- **Failed runs are first-class artifacts** — negative results are preserved, never silently replaced
- **No silent hardcoding** — when a formula is not uniquely determined, it becomes a branch-defined interface (Section 23)

## Codebase

- ~450 C# source files
- ~250 test files
- 15 native source files (CUDA/Vulkan)
- 2,252 tests across 38 test projects
- Phase 1 (Minimal GU v1): **Complete** — all 13 milestones (M0-M12)
- Phase 2 (Research Instrumentation): **Complete** — all 10 milestones (M13-M22)
- Phase 3 (Boson Spectrum Extraction): **Complete** — all 10 milestones (M23-M32) + all 12 gap closures

## Theory Context

The `TheoryCompletitionRevisions/` directory contains the evolving Geometric Unity Completion manuscript. The software implements the executable portion of this completion program. See `IMPLEMENTATION_PLAN.md` (Phase I), `IMPLEMENTATION_PLAN_P2.md` (Phase II), and `IMPLEMENTATION_PLAN_P3.md` (Phase III) for the full technical specifications.

## License

See LICENSE file for details.
