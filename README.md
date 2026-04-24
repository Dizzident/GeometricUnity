# Geometric Unity

A reproducible research engine implementing an executable bosonic and fermionic branch of Eric Weinstein's Geometric Unity framework. Connection-centered, observer-based, explicitly discretized, with CPU reference backend, CUDA acceleration hooks, Vulkan visualization, a Phase II research instrumentation layer, a Phase III boson spectrum extraction pipeline, a Phase IV fermionic spectrum and family-clustering layer, and a Phase V quantitative validation and falsification framework.

## Current Status

This repository is currently a **benchmark and validation platform**, not yet a real particle-property prediction engine.

What works now:

- computed GU observables are serialized with provenance and uncertainty;
- target tables compare computed observables against toy, internal, and external benchmark values;
- target coverage fails closed when a declared target has no computed observable;
- the reference Phase V campaign runs branch, refinement, environment, quantitative, falsification, dossier, and report stages;
- the active external benchmark is DOI-backed: Zenodo record `10.5281/zenodo.16739090`, a pure SU(2) plaquette-chain exact-diagonalization benchmark;
- generated/downloaded study outputs are kept under ignored `study-runs/`.

Latest checked-in reference campaign status:

- total targets: 9;
- matched targets: 9;
- missing targets: 0;
- passed matches: 8;
- failed matches: 1;
- score: 8 / 9 = 0.8888888888888888.

The remaining quantitative failure is `bosonic-mode-2-imported-repo-benchmark`: computed value `0.98`, target value `0.6`, pull `5.374`.

What is not done yet:

- no current observable is validated as a physical W/Z/Higgs/photon property;
- no physical unit calibration to GeV or measured couplings exists yet;
- no PDG-style experimental target campaign is active yet;
- active fatal/high falsifiers still block physical prediction claims.

See `IMPLEMENTATION_P15.md` for the next benchmark-repair milestone and `IMPLEMENTATION_P16.md` for the physical-observable mapping plan.

## Overview

This system implements the **minimal bosonic and fermionic executable branch** of the Geometric Unity completion program:

- **Connection omega** on a principal bundle P -> Y is the primary dynamical variable
- **Curvature** F_omega, **torsion** T_omega, and **Shiab** S_omega define the residual Upsilon = S - T
- **Variational objective** I2(omega) = (1/2) integral of Upsilon, Upsilon drives the solver
- **Observation pipeline** maps Y-space quantities to X-space observables via pullback sigma_h*
- **Branch manifest** controls all choices — no silent hardcoding
- **Branch families** (Phase II) parameterize variant choices for systematic independence studies
- **Linearization workbench** classifies PDE type, computes spectra, and probes stability
- **Recovery graphs** trace the full chain from native fields to physical identification
- **Comparison campaigns** compare computed observables against target tables with uncertainty decomposition
- **Background atlas** (Phase III) builds a catalog of stationary background connections
- **Boson spectrum** (Phase III) extracts fluctuation eigenmodes via Lanczos on the Hessian
- **Mode tracking** (Phase III) follows mode families across backgrounds with split/merge detection
- **Candidate boson registry** (Phase III) assembles and classifies candidate modes; physical particle naming remains gated
- **Fermionic spectrum** (Phase IV) extracts Dirac eigenmodes on the spin bundle over Y
- **Chirality and conjugation** (Phase IV) classifies fermionic modes by X/Y/F-chirality operators
- **Family clustering** (Phase IV) groups near-degenerate fermionic triplets into candidate families
- **Coupling extraction** (Phase IV) computes interaction proxies between bosonic and fermionic modes
- **Unified registry** (Phase IV) merges boson and fermion candidate records with claim classes C0-C5
- **Branch-independence validation** (Phase V) measures how much outputs vary across branch variants
- **Continuum extrapolation** (Phase V) applies Richardson extrapolation across mesh refinement levels
- **Quantitative validation** (Phase V) computes pull statistics against external targets with 4-source uncertainty
- **Falsification engine** (Phase V) fires 7 typed falsifiers that demote or cap candidate claim classes
- **Claim escalation** (Phase V) promotes candidates through 6 mandatory gates to higher evidence tiers
- **Validation dossiers** (Phase V) assemble all Phase V evidence into per-study dossier artifacts
- **Target coverage blockers** (Phase XIV) make intentionally missing observables explicit
- **Spectrum observable extraction** (Phase XIV) turns externally generated eigenvalue lists into quantitative observable records
- **Physical-observable mapping plan** (Phase XVI) defines the bridge needed before real boson-property comparisons

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

There are 2,960+ tests across 52 test projects covering core types, geometry, reference CPU operators, interop, validation, artifacts, observation, external comparison, all Phase II modules, all Phase III modules, all Phase IV modules, and all Phase V modules.

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

**Phase IV — Fermionic Spectrum:**
```bash
dotnet run --project apps/Gu.Cli -- build-spin-spec <branchManifest.json> [--out <spinSpec.json>]
dotnet run --project apps/Gu.Cli -- assemble-dirac <spinSpec.json> [--out <diracBundle.json>]
dotnet run --project apps/Gu.Cli -- solve-fermion-modes <diracBundle.json> [--num-modes N] [--out <dir>]
dotnet run --project apps/Gu.Cli -- analyze-chirality <fermionModes.json> [--out <chirality.json>]
dotnet run --project apps/Gu.Cli -- analyze-conjugation <fermionModes.json> [--out <conjugation.json>]
dotnet run --project apps/Gu.Cli -- extract-couplings <bosonRegistry.json> <fermionModes.json> [--out <couplings.json>]
dotnet run --project apps/Gu.Cli -- build-family-clusters <fermionModes.json> [--out <clusters.json>]
dotnet run --project apps/Gu.Cli -- build-unified-registry <bosonRegistry.json> <fermionModes.json> [--out <registry.json>]
```

**Phase V — Quantitative Validation:**
```bash
dotnet run --project apps/Gu.Cli -- branch-robustness --study <studySpec.json> --values <quantityValues.json> [--out <output.json>]
dotnet run --project apps/Gu.Cli -- validate-quantitative --observables <obs.json> --targets <targets.json> [--environment-records <env1.json,env2.json,...>] [--out <scorecard.json>] [--fail-closed-target-coverage]
dotnet run --project apps/Gu.Cli -- extract-spectrum-observable --eigenvalues <eigenvalues.json> --observable-id <id> --environment-id <id> [--out <observable.json>]
dotnet run --project apps/Gu.Cli -- validate-phase5-campaign-spec --spec <campaign.json> [--require-reference-sidecars]
dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec <campaign.json> --out-dir <dir> [--validate-first]
```

Reference campaign:

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json \
  --out-dir study-runs/readme_reference_campaign \
  --validate-first
```

Generated campaign outputs should go under `study-runs/`, which is ignored except for `.gitkeep`.

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
├── src/                           # Source libraries
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
│   ├── Gu.Phase3.Reporting/       # Boson atlas reports, dashboards
│   ├── Gu.Phase4.Spin/            # Spin connection builder on bundle over Y
│   ├── Gu.Phase4.Fermions/        # Fermionic mode extraction infrastructure
│   ├── Gu.Phase4.Dirac/           # Dirac operator assembly (CPU + GPU stub)
│   ├── Gu.Phase4.Chirality/       # X/Y/F-chirality operators and classification
│   ├── Gu.Phase4.FamilyClustering/# Near-degenerate triplet → family cluster grouping
│   ├── Gu.Phase4.Couplings/       # Interaction proxy extraction, coupling matrix
│   ├── Gu.Phase4.Registry/        # Fermion registry, unified particle registry
│   ├── Gu.Phase4.Observation/     # Fermionic observation pipeline
│   ├── Gu.Phase4.Comparison/      # Fermion comparison campaigns
│   ├── Gu.Phase4.Reporting/       # Phase IV reports and dashboards
│   ├── Gu.Phase4.CudaAcceleration/# GPU Dirac solve (stub; TODO(M44-GPU))
│   ├── Gu.Phase5.BranchIndependence/ # Branch-robustness engine, fragility scores (M46)
│   ├── Gu.Phase5.Convergence/     # Richardson extrapolation, convergence classification (M47)
│   ├── Gu.Phase5.Environments/    # Structured/imported environments, admissibility (M48)
│   ├── Gu.Phase5.QuantitativeValidation/ # Uncertainty propagation, pull statistics (M49)
│   ├── Gu.Phase5.Falsification/   # 7 falsifier types, severity policy, registry demotion (M50)
│   ├── Gu.Phase5.Dossiers/        # Claim escalation, negative-result ledger, dossiers (M51-M52)
│   └── Gu.Phase5.Reporting/       # Phase V reports, atlases, falsification dashboard (M53)
├── tests/                         # 52 test projects (Phase I + II + III + IV + V)
├── native/                        # CUDA kernels (C/CUDA)
├── examples/                      # Toy 2D/3D/4D geometries for debugging
└── schemas/                       # JSON schemas for all phases
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

### Phase IV Fermionic Spectrum Pipeline

```
SpinConnectionSpec (spin group, Lie algebra, background connection)
  -> CpuSpinConnectionBuilder (spin connection on bundle over Y)
  -> DiracOperatorAssembler (Dirac operator D_A on spinor bundle)
  -> GpuDiracKernelStub (GPU dispatch; TODO(M44-GPU))
  -> FermionModeExtractor (eigenvalue solve: mass ~ |lambda|, NOT sqrt)
  -> ChiralityAnalyzer (Y-chirality, X-chirality (primary), F-chirality)
  -> ConjugationAnalyzer (C-conjugation, CPT classification)
  -> FamilyClusterEngine (near-degenerate triplet → "cluster-N" label)
  -> CouplingExtractor (interaction proxy between bosons and fermions)
  -> FermionRegistry (claim classes C0-C5 analog, demotion rules)
  -> RegistryMergeEngine (UnifiedParticleRegistry: bosons + fermions)
  -> FermionComparisonCampaignRunner (fermion comparison campaigns)
  -> Phase4ReportGenerator (unified registry reports)
```

### Phase V Quantitative Validation Pipeline

```
Phase5CampaignSpec (branch family, refinement spec, env campaign, targets)
  -> BranchRobustnessEngine (pairwise distances, equivalence classes, fragility)
  -> RefinementStudyRunner (Richardson extrapolation, convergence classification)
  -> StructuredEnvironmentGenerator / EnvironmentImporter (M48 environments)
  -> UncertaintyPropagator (4-source quadrature: branch, refinement, extraction, env)
  -> TargetMatcher (pull statistic |Δ|/sqrt(σ_c²+σ_t²), pass ≤ 5σ default)
  -> FalsifierEvaluator (7 falsifier types, fatal/high/medium/low severity)
  -> RegistryDemotionIntegrator (fatal→C0, high→-2, medium→-1, low→warning)
  -> Phase5DossierAssembler (branch+convergence+env+quant+falsifiers → dossier)
  -> ClaimEscalationRecord (6 mandatory gates for promotion)
  -> NegativeResultLedger (branch-fragility, non-convergence, mismatch, etc.)
  -> Phase5ReportGenerator (atlases, dashboards, markdown report)
```

### Phase XIV External Benchmark Coverage

```
Zenodo SU(2) plaquette-chain package
  -> ignored study-runs download / transient Python dependencies
  -> checked-in eigenvalue fixture
  -> SpectrumObservableExtractor
  -> QuantitativeObservableRecord
  -> Phase V target match
```

The active external benchmark is sourced from Zenodo DOI `10.5281/zenodo.16739090`. It is an external SU(2) lattice-gauge benchmark, not a real-world particle measurement.

### Phase XVI Physical Observable Roadmap

Phase XVI is the planned bridge from benchmark quantities to physical boson properties. It requires:

- explicit mappings from computed GU artifacts to physical observables;
- unit and scale calibration;
- authoritative experimental target tables;
- report gates that prevent benchmark success from being described as particle prediction.

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

- ~600 C# source files
- ~350 test files
- 15 native source files (CUDA/Vulkan)
- 2,960+ tests across 52 test projects
- Phase 1 (Minimal GU v1): **Complete** — all 13 milestones (M0-M12)
- Phase 2 (Research Instrumentation): **Complete** — all 10 milestones (M13-M22)
- Phase 3 (Boson Spectrum Extraction): **Complete** — all 10 milestones (M23-M32) + all 23 gap closures
- Phase 4 (Fermionic Spectrum): **Complete** — all 13 milestones (M33-M45) + all 8 gap closures
- Phase 5 (Quantitative Validation): **Complete** — all 8 milestones (M46-M53) + all 6 entry gap closures

## Theory Context

The `TheoryCompletitionRevisions/` directory contains the evolving Geometric Unity Completion manuscript. The software implements executable portions of this completion program and a validation framework around them. See `IMPLEMENTATION_PLAN.md` through `IMPLEMENTATION_P16.md` for the phase history and next milestones.

## License

See LICENSE file for details.
