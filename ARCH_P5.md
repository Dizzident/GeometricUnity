# ARCH_P5.md -- Phase V Architectural Specification

## Purpose

This document is the detailed architectural spec for Phase V implementation.
It defines exact project structure, interface signatures, data flow, dependency
graph, entry gap closures, and per-milestone implementation guidance for all
implementers.

Phase V adds: branch-independence measurement, refinement/continuum convergence,
environment ladder framework, quantitative validation engine, falsification
engine, claim escalation/demotion, validation dossier system, and a first
high-value campaign -- all on top of the Phase I-IV platform.

---

## 1. Entry Gaps (G-001 through G-006)

These MUST be closed before milestone work begins. They are implementation
and provenance fixes to existing code, not new Phase V modules.

### 1.1 G-001: Runtime branch selection still bypassed

**Owner**: implementer-1 (gap closer)

**Problem**: `RunSolver()` and `SolveCommand()` in `apps/Gu.Cli/Program.cs`
hardcode `ActiveTorsionBranch = "trivial"` and `ActiveShiabBranch = "identity-shiab"`
when constructing fallback BranchManifest objects (lines ~352-374, ~880-910).
The existing `BranchOperatorRegistry` and `BranchVariantResolver` are never
consulted in the main CLI runtime path.

**Files to modify**:
- `apps/Gu.Cli/Program.cs` -- `RunSolver()`, `SolveCommand()`

**Changes needed**:
1. When a persisted BranchManifest is loaded (from `--manifest` flag or run folder),
   use `BranchOperatorRegistry` to resolve torsion and Shiab operators from
   `manifest.ActiveTorsionBranch` and `manifest.ActiveShiabBranch` instead of
   directly instantiating `TrivialTorsionCpu` / `IdentityShiabCpu`.
2. Create a helper method `ResolveBranchOperators(BranchManifest manifest)` that
   returns `(ITorsionBranchOperator, IShiabBranchOperator, IBiConnectionStrategy)`
   by calling `BranchOperatorRegistry.ResolveTorsion()`, `.ResolveShiab()`,
   `.ResolveBiConnection()`.
3. Register the known operator implementations at CLI startup (trivial torsion,
   augmented torsion, identity Shiab, etc.) so the registry is populated.
4. Record `manifest.ActiveTorsionBranch` and `manifest.ActiveShiabBranch` in the
   provenance artifacts written to the run folder, alongside the actually
   instantiated implementation class name.
5. The fallback default manifest (when no manifest exists) may still use
   "trivial"/"identity-shiab" but must log a warning: "Using default trivial
   branch operators -- not suitable for Phase V validation studies."

**Tests**: Add a test in `tests/Gu.Core.Tests/` or a new
`tests/Gu.Phase5.EntryGap.Tests/` project that verifies:
- `BranchOperatorRegistry` resolves "trivial" and "identity-shiab" correctly
- A non-trivial torsion branch ID fails gracefully if not registered
- Provenance artifacts contain the declared AND instantiated operator IDs

### 1.2 G-002: Default solve path uses trivial validation

**Owner**: implementer-1 (gap closer)

**Problem**: `gu solve` defaults to zero omega, zero A0, Mode A residual-only
evaluation. There is no first-class nontrivial validation path.

**Files to modify**:
- `apps/Gu.Cli/Program.cs` -- `SolveCommand()`

**Changes needed**:
1. Add `--initial-state <path>` flag that loads a persisted FieldTensor as
   initial omega (already partially done via `--omega`, extend to load full
   background context).
2. Add `--validation-tier <tier>` flag with values: `residual-only`,
   `stationarity`, `full-solve`. Default remains `residual-only` but the flag
   is now explicit.
3. When `--validation-tier full-solve` is specified, require a non-zero initial
   state (either `--omega` or `--initial-state`).
4. Write a `SolveValidationTier` field into provenance/report artifacts so
   dossiers can distinguish residual-only runs from genuine solves.
5. Reports and artifacts must carry `"validationTier": "residual-only"` or
   `"stationarity"` or `"full-solve"`.

**Tests**: Verify that `--validation-tier full-solve` without `--omega` fails
with a clear error message.

### 1.3 G-003: Toy geometry dominates end-to-end paths

**Owner**: implementer-2 (gap closer)

**Problem**: `RunSolver()`, `ComputeSpectrum()`, and `SolveBackgrounds()` all
call `ToyGeometryFactory.CreateToy2D()` unconditionally.

**Files to modify**:
- `apps/Gu.Cli/Program.cs` -- `RunSolver()`, `ComputeSpectrum()`,
  `SolveBackgrounds()`

**Changes needed**:
1. Add `--geometry <path>` flag that loads a persisted geometry context from a
   JSON file (GeometryContext or a new EnvironmentGeometryBundle).
2. Add `--geometry-tier <tier>` flag with values: `toy`, `structured`, `imported`.
   Default is `toy` (preserving backward compat).
3. When `--geometry toy` is used (or default), continue using
   `ToyGeometryFactory.CreateToy2D()` but record `"geometryTier": "toy"` in
   provenance.
4. When `--geometry <path>` is provided, load the geometry from file and record
   `"geometryTier": "imported"` with the file path hash.
5. Validation dossiers (M52) will separate evidence by geometry tier.

**Tests**: Verify geometry tier is recorded in provenance. Verify `--geometry`
flag loads a file correctly (unit test with a simple serialized mesh).

### 1.4 G-004: compute-spectrum only partially corrected

**Owner**: implementer-2 (gap closer)

**Problem**: `ComputeSpectrum()` loads persisted omega but still rebuilds zero
A0, toy geometry, and hardcoded trivial/identity operators.

**Files to modify**:
- `apps/Gu.Cli/Program.cs` -- `ComputeSpectrum()`

**Changes needed**:
1. Load full background context (omega + A0 + geometry fingerprint) from the
   persisted background record, not just omega.
2. Use `BranchOperatorRegistry` to resolve operators from the loaded manifest
   (same pattern as G-001).
3. Add `--a0 <path>` flag to load persisted A0 state.
4. Add an end-to-end regression test: two different stored backgrounds
   (different omega profiles) must produce different spectrum artifacts.
5. Record consumed A0 source and geometry source in provenance alongside omega
   source.

**Tests**: End-to-end test: two backgrounds with different omega -> different
spectra. Must be in `tests/Gu.Phase5.EntryGap.Tests/` or inline in an existing
integration test project.

### 1.5 G-005: Phase IV family/coupling workflows use branch-local shortcuts

**Owner**: implementer-3 (gap closer)

**Problem**: Phase IV study runner (`Phase4FermionFamilyAtlasStudy.cs`) uses
synthetic stand-ins rather than real cross-branch inputs.

**Files to modify**:
- `studies/phase4_fermion_family_atlas_001/Phase4FermionFamilyAtlasStudy.cs`
- Potentially `src/Gu.Phase4.FamilyClustering/FamilyClusteringEngine.cs`
- Potentially `src/Gu.Phase4.Couplings/CouplingProxyEngine.cs`

**Changes needed**:
1. Add a `--backgrounds-dir <path>` parameter to the study runner that loads
   real solved backgrounds from Phase III instead of fabricating inline.
2. When `--backgrounds-dir` is provided, load BackgroundRecord + omega states
   from persisted artifacts.
3. When not provided (backward compat), use the existing inline fabrication
   but mark the study result with `"inputTier": "synthetic"`.
4. Family clustering: when real branch variants are available (multiple
   backgrounds from different branch manifests), use them instead of perturbed
   duplicates.
5. Coupling extraction: when real perturbation data is available, use it
   instead of zero variation matrices.

**Tests**: Verify `inputTier` field is set correctly. Verify that loading from
`--backgrounds-dir` produces different results than synthetic defaults.

### 1.6 G-006: Checked-in study artifacts not trustworthy

**Owner**: implementer-3 (gap closer)

**Problem**: Checked-in artifacts may lag current code.

**Files to modify**:
- `studies/phase4_fermion_family_atlas_001/run_study.sh` (or create new)
- `studies/bosonic_validation_001/run_study.sh`

**Changes needed**:
1. Add a `--verify-freshness` flag to study runners that computes a hash of
   the current code revision and compares it to the `codeRevision` field in
   persisted artifacts.
2. Create a `gu verify-study-freshness <study-dir>` CLI command that checks
   whether all artifacts in a study directory were produced by the current
   code revision.
3. Validation dossiers (M52) must record freshness status: `fresh` (regenerated
   from current code), `stale` (from older code), `unknown`.
4. Phase V studies must always regenerate artifacts from the current tree.

**Tests**: Verify freshness check correctly identifies stale vs fresh artifacts.

---

## 2. Project Structure

### 2.1 New Source Projects (7 projects)

All under `src/`:

```
src/Gu.Phase5.BranchIndependence/   -- M46: branch sweep, distance matrices, fragility
src/Gu.Phase5.Convergence/          -- M47: refinement ladder, Richardson extrapolation
src/Gu.Phase5.Environments/         -- M48: environment import, structured generators, admissibility
src/Gu.Phase5.QuantitativeValidation/ -- M49: calibrated comparison, uncertainty propagation
src/Gu.Phase5.Falsification/        -- M50: typed falsifier records, severity, registry demotion
src/Gu.Phase5.Dossiers/             -- M51+M52: claim escalation, validation dossier assembly
src/Gu.Phase5.Reporting/            -- M53: campaign runner, reports, atlases
```

### 2.2 New Test Projects (7 projects)

All under `tests/`:

```
tests/Gu.Phase5.BranchIndependence.Tests/
tests/Gu.Phase5.Convergence.Tests/
tests/Gu.Phase5.Environments.Tests/
tests/Gu.Phase5.QuantitativeValidation.Tests/
tests/Gu.Phase5.Falsification.Tests/
tests/Gu.Phase5.Dossiers.Tests/
tests/Gu.Phase5.Reporting.Tests/
```

### 2.3 Additional Deliverables

```
studies/phase5_su2_branch_refinement_env_validation/  -- M53 reference study
schemas/  -- 10 new JSON schemas (see Section 9)
```

### 2.4 Solution File Additions

Every new src/ and tests/ project must be added to `GeometricUnity.slnx`
under the appropriate `/src/` or `/tests/` folder.

### 2.5 csproj Pattern

All projects inherit from `Directory.Build.props` (net10.0, nullable, latest).
Each src csproj uses this minimal pattern:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <InternalsVisibleTo Include="Gu.Phase5.{Name}.Tests" />
  </ItemGroup>
  <ItemGroup>
    <!-- ProjectReferences here -->
  </ItemGroup>
</Project>
```

Test projects use:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <!-- ProjectReferences to src project + dependencies -->
  </ItemGroup>
</Project>
```

---

## 3. Dependency Graph

### 3.1 Phase V Internal Dependencies

```
Gu.Phase5.BranchIndependence   -- depends on: Gu.Core, Gu.Math, Gu.Branching,
    |                              Gu.Phase2.Branches, Gu.Phase2.Semantics,
    |                              Gu.Phase3.Backgrounds, Gu.Phase3.Spectra,
    |                              Gu.Phase4.Registry
    v
Gu.Phase5.Convergence          -- depends on: Gu.Core, Gu.Math, Gu.Geometry,
    |                              Gu.Phase3.Backgrounds, Gu.Phase3.Spectra,
    |                              Gu.Phase5.BranchIndependence
    v
Gu.Phase5.Environments         -- depends on: Gu.Core, Gu.Geometry,
    |                              Gu.Phase3.Backgrounds, Gu.Artifacts
    v
Gu.Phase5.QuantitativeValidation -- depends on: Gu.Core, Gu.Math,
    |                                Gu.Phase2.Comparison, Gu.ExternalComparison,
    |                                Gu.Phase3.Campaigns, Gu.Phase4.Registry,
    |                                Gu.Phase5.BranchIndependence,
    |                                Gu.Phase5.Convergence,
    |                                Gu.Phase5.Environments
    v
Gu.Phase5.Falsification        -- depends on: Gu.Core,
    |                              Gu.ExternalComparison,
    |                              Gu.Phase3.Registry, Gu.Phase4.Registry,
    |                              Gu.Phase5.BranchIndependence,
    |                              Gu.Phase5.Convergence,
    |                              Gu.Phase5.QuantitativeValidation
    v
Gu.Phase5.Dossiers             -- depends on: Gu.Core, Gu.Artifacts,
    |                              Gu.Phase4.Registry,
    |                              Gu.Phase5.BranchIndependence,
    |                              Gu.Phase5.Convergence,
    |                              Gu.Phase5.Environments,
    |                              Gu.Phase5.QuantitativeValidation,
    |                              Gu.Phase5.Falsification
    v
Gu.Phase5.Reporting            -- depends on: Gu.Core,
                                   Gu.Phase3.Reporting, Gu.Phase4.Reporting,
                                   Gu.Phase5.BranchIndependence,
                                   Gu.Phase5.Convergence,
                                   Gu.Phase5.Environments,
                                   Gu.Phase5.QuantitativeValidation,
                                   Gu.Phase5.Falsification,
                                   Gu.Phase5.Dossiers
```

### 3.2 Key Cross-Phase Dependencies

Phase V **consumes** these types (never reimplements):

| Type | Namespace | Used By |
|---|---|---|
| `BranchManifest` | `Gu.Core` | All (branch-aware everything) |
| `ProvenanceMeta` | `Gu.Core` | All (provenance on every record) |
| `FieldTensor` | `Gu.Core` | Convergence (state comparison) |
| `BranchVariantManifest` | `Gu.Phase2.Semantics` | BranchIndependence (sweep input) |
| `BranchVariantResolver` | `Gu.Phase2.Branches` | BranchIndependence (manifest resolution) |
| `BranchOperatorRegistry` | `Gu.Branching` | BranchIndependence (operator dispatch) |
| `BackgroundRecord` | `Gu.Phase3.Backgrounds` | BranchIndependence, Convergence, Environments |
| `SpectrumBundle` | `Gu.Phase3.Spectra` | BranchIndependence, Convergence |
| `ModeRecord` | `Gu.Phase3.Spectra` | BranchIndependence (spectral comparison) |
| `CandidateBosonRecord` | `Gu.Phase3.Registry` | Falsification (boson demotion) |
| `DemotionEngine` | `Gu.Phase3.Registry` | Falsification (demotion pattern) |
| `BosonClaimClass` | `Gu.Phase3.Registry` | Falsification, Dossiers |
| `UnifiedParticleRegistry` | `Gu.Phase4.Registry` | Falsification, Dossiers, Reporting |
| `ParticleClaimRecord` | `Gu.Phase4.Registry` | Dossiers (claim escalation) |
| `FalsifierCheck` | `Gu.ExternalComparison` | Falsification (existing falsifier types) |
| `FalsifierRegistry` | `Gu.ExternalComparison` | Falsification (hard/soft falsifiers) |
| `ComparisonRecord` | `Gu.ExternalComparison` | QuantitativeValidation |
| `UncertaintyDecomposer` | `Gu.Phase2.Comparison` | QuantitativeValidation |
| `NegativeResultEntry` | `Gu.Phase3.Reporting` | Dossiers, Reporting |
| `ToyGeometryFactory` | `Gu.Geometry` | Environments (toy tier) |

### 3.3 Dependency Rule: No Circular References

Phase V projects MUST NOT reference each other in cycles.
The dependency chain is strictly:
BranchIndependence -> Convergence -> Environments -> QuantitativeValidation -> Falsification -> Dossiers -> Reporting.

QuantitativeValidation, Falsification, and Dossiers also depend on BranchIndependence
and Convergence directly (skip-level deps are allowed; cycles are not).

---

## 4. Core Type Definitions

All types are **sealed classes** with `required` init properties and
`[JsonPropertyName]` attributes for STJ serialization. NO records.
Use `System.Math.Sqrt()` explicitly (not `Math.Sqrt()`) to avoid Gu.Math conflict.

### 4.1 Gu.Phase5.BranchIndependence Types (M46)

```csharp
// BranchRobustnessStudySpec.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class BranchRobustnessStudySpec
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("branchFamilyId")]
    public required string BranchFamilyId { get; init; }

    [JsonPropertyName("variantIds")]
    public required IReadOnlyList<string> VariantIds { get; init; }

    [JsonPropertyName("targetQuantities")]
    public required IReadOnlyList<string> TargetQuantities { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("tolerances")]
    public required BranchDistanceTolerances Tolerances { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// BranchDistanceTolerances.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class BranchDistanceTolerances
{
    [JsonPropertyName("equivalenceThreshold")]
    public double EquivalenceThreshold { get; init; } = 0.05;

    [JsonPropertyName("fragilityThreshold")]
    public double FragilityThreshold { get; init; } = 0.5;

    [JsonPropertyName("distanceMetric")]
    public string DistanceMetric { get; init; } = "relative-l2";
}
```

```csharp
// BranchDistanceMatrix.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class BranchDistanceMatrix
{
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("variantIds")]
    public required IReadOnlyList<string> VariantIds { get; init; }

    /// <summary>
    /// Flattened row-major NxN distance matrix.
    /// distances[i * N + j] = distance between variant i and variant j.
    /// </summary>
    [JsonPropertyName("distances")]
    public required double[] Distances { get; init; }

    [JsonPropertyName("distanceMetric")]
    public required string DistanceMetric { get; init; }

    /// <summary>Get distance between variants i and j.</summary>
    public double Get(int i, int j) => Distances[i * VariantIds.Count + j];

    /// <summary>Matrix dimension.</summary>
    [JsonIgnore]
    public int N => VariantIds.Count;
}
```

```csharp
// BranchEquivalenceClass.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class BranchEquivalenceClass
{
    [JsonPropertyName("classId")]
    public required string ClassId { get; init; }

    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("memberVariantIds")]
    public required IReadOnlyList<string> MemberVariantIds { get; init; }

    [JsonPropertyName("maxInternalDistance")]
    public required double MaxInternalDistance { get; init; }

    [JsonPropertyName("threshold")]
    public required double Threshold { get; init; }
}
```

```csharp
// FragilityRecord.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class FragilityRecord
{
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("variantId")]
    public required string VariantId { get; init; }

    [JsonPropertyName("fragilityScore")]
    public required double FragilityScore { get; init; }

    [JsonPropertyName("maxDistanceToNeighbor")]
    public required double MaxDistanceToNeighbor { get; init; }

    [JsonPropertyName("meanDistanceToFamily")]
    public required double MeanDistanceToFamily { get; init; }

    [JsonPropertyName("isFragile")]
    public required bool IsFragile { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
```

```csharp
// InvarianceCandidateRecord.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class InvarianceCandidateRecord
{
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("equivalenceClassId")]
    public required string EquivalenceClassId { get; init; }

    [JsonPropertyName("invarianceScore")]
    public required double InvarianceScore { get; init; }

    [JsonPropertyName("coverageFraction")]
    public required double CoverageFraction { get; init; }

    [JsonPropertyName("isInvariant")]
    public required bool IsInvariant { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
```

```csharp
// BranchRobustnessRecord.cs
namespace Gu.Phase5.BranchIndependence;

public sealed class BranchRobustnessRecord
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("distanceMatrices")]
    public required IReadOnlyList<BranchDistanceMatrix> DistanceMatrices { get; init; }

    [JsonPropertyName("equivalenceClasses")]
    public required IReadOnlyList<BranchEquivalenceClass> EquivalenceClasses { get; init; }

    [JsonPropertyName("fragilityRecords")]
    public required IReadOnlyList<FragilityRecord> FragilityRecords { get; init; }

    [JsonPropertyName("invarianceCandidates")]
    public required IReadOnlyList<InvarianceCandidateRecord> InvarianceCandidates { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key algorithms to implement**:

```csharp
// BranchDistanceComputer.cs
namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Computes pairwise distance matrices for branch-varying quantities.
///
/// Supported metrics (physicist-confirmed):
///   "relative-l2":
///       d(a,b) = ||a-b||_2 / max(||a||_2, ||b||_2, 1e-14)
///       Default for generic field quantities (residuals, couplings).
///
///   "spectral-linf-relative":
///       d(a,b) = max_i |a_i - b_i| / max(max_i |a_i|, max_i |b_i|, 1e-14)
///       Use for eigenvalue arrays (Hessian, Dirac). Catches single large
///       eigenvalue shifts in mostly-stable spectra.
///
///   "absolute-l2":
///       d(a,b) = ||a-b||_2
///       Scale-dependent; use only when quantities have a known natural scale.
/// </summary>
public static class BranchDistanceComputer
{
    public const double Epsilon = 1e-14;

    public static BranchDistanceMatrix Compute(
        string quantityId,
        IReadOnlyList<string> variantIds,
        IReadOnlyList<double[]> quantityValues,
        string distanceMetric);
}
```

```csharp
// EquivalenceClassifier.cs
namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Clusters variants into equivalence classes using single-linkage clustering
/// at the declared threshold. Two variants are equivalent if their distance
/// is below the threshold.
/// </summary>
public static class EquivalenceClassifier
{
    public static IReadOnlyList<BranchEquivalenceClass> Classify(
        BranchDistanceMatrix matrix,
        double threshold);
}
```

```csharp
// FragilityScorer.cs
namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Computes fragility score for each variant:
///   fragilityScore = maxDistanceToNeighbor / (meanDistanceToFamily + epsilon)
/// A variant is fragile if fragilityScore > fragilityThreshold.
/// </summary>
public static class FragilityScorer
{
    public static IReadOnlyList<FragilityRecord> Score(
        BranchDistanceMatrix matrix,
        double fragilityThreshold);
}
```

```csharp
// BranchRobustnessRunner.cs
namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Orchestrates a full branch-robustness study:
///   1. Load branch family (variant manifests)
///   2. For each variant, resolve to Phase I BranchManifest
///   3. Run target pipeline (bosonic/fermionic) for each variant
///   4. Extract target quantities
///   5. Compute distance matrices
///   6. Classify equivalence classes
///   7. Score fragility
///   8. Identify invariance candidates
///   9. Emit BranchRobustnessRecord
/// </summary>
public sealed class BranchRobustnessRunner
{
    public BranchRobustnessRecord Run(
        BranchRobustnessStudySpec spec,
        IReadOnlyList<BranchVariantManifest> variants,
        BranchManifest baseManifest,
        Func<BranchManifest, IReadOnlyDictionary<string, double[]>> pipelineExecutor);
}
```

### 4.2 Gu.Phase5.Convergence Types (M47)

```csharp
// RefinementStudySpec.cs
namespace Gu.Phase5.Convergence;

public sealed class RefinementStudySpec
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("targetQuantities")]
    public required IReadOnlyList<string> TargetQuantities { get; init; }

    [JsonPropertyName("refinementLevels")]
    public required IReadOnlyList<RefinementLevel> RefinementLevels { get; init; }

    [JsonPropertyName("extrapolationMethod")]
    public string ExtrapolationMethod { get; init; } = "richardson";

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// RefinementLevel.cs
namespace Gu.Phase5.Convergence;

public sealed class RefinementLevel
{
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    [JsonPropertyName("meshParameter")]
    public required double MeshParameter { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
```

```csharp
// RefinementRunRecord.cs
namespace Gu.Phase5.Convergence;

public sealed class RefinementRunRecord
{
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    [JsonPropertyName("meshParameter")]
    public required double MeshParameter { get; init; }

    [JsonPropertyName("quantities")]
    public required IReadOnlyDictionary<string, double> Quantities { get; init; }

    [JsonPropertyName("converged")]
    public required bool Converged { get; init; }

    [JsonPropertyName("residualNorm")]
    public double ResidualNorm { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// ContinuumEstimateRecord.cs
namespace Gu.Phase5.Convergence;

public sealed class ContinuumEstimateRecord
{
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("extrapolatedValue")]
    public required double ExtrapolatedValue { get; init; }

    [JsonPropertyName("errorBand")]
    public required double ErrorBand { get; init; }

    [JsonPropertyName("convergenceOrder")]
    public required double ConvergenceOrder { get; init; }

    [JsonPropertyName("convergenceClassification")]
    public required string ConvergenceClassification { get; init; }

    [JsonPropertyName("confidenceNote")]
    public required string ConfidenceNote { get; init; }

    [JsonPropertyName("runRecords")]
    public required IReadOnlyList<RefinementRunRecord> RunRecords { get; init; }
}
```

```csharp
// RichardsonFitRecord.cs
namespace Gu.Phase5.Convergence;

public sealed class RichardsonFitRecord
{
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("estimatedLimit")]
    public required double EstimatedLimit { get; init; }

    [JsonPropertyName("estimatedOrder")]
    public required double EstimatedOrder { get; init; }

    [JsonPropertyName("residual")]
    public required double Residual { get; init; }

    [JsonPropertyName("meshParameters")]
    public required double[] MeshParameters { get; init; }

    [JsonPropertyName("values")]
    public required double[] Values { get; init; }
}
```

```csharp
// ConvergenceFailureRecord.cs
namespace Gu.Phase5.Convergence;

public sealed class ConvergenceFailureRecord
{
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    [JsonPropertyName("failureType")]
    public required string FailureType { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("observedValues")]
    public required double[] ObservedValues { get; init; }

    [JsonPropertyName("meshParameters")]
    public required double[] MeshParameters { get; init; }
}
```

**Key algorithms to implement**:

```csharp
// RichardsonExtrapolator.cs
namespace Gu.Phase5.Convergence;

/// <summary>
/// Richardson extrapolation: given Q(h) at multiple h values,
/// estimate Q(0) and convergence order p.
///
/// For three levels h1 > h2 > h3 with Q1, Q2, Q3:
///   p = log((Q1-Q2)/(Q2-Q3)) / log(h1/h2)     [assumes h2/h3 = h1/h2]
///   Q_extrap = Q3 + (Q3-Q2) / (r^p - 1)        where r = h2/h3
///
/// For non-uniform refinement ratios, use least-squares fit of
///   Q(h) = Q_0 + C * h^p
/// </summary>
public static class RichardsonExtrapolator
{
    public static RichardsonFitRecord Extrapolate(
        string quantityId,
        double[] meshParameters,
        double[] values);
}
```

```csharp
// ConvergenceClassifier.cs
namespace Gu.Phase5.Convergence;

/// <summary>
/// Classifies convergence behavior:
///   "convergent"       -- successive deltas decrease, p > 0
///   "weakly-convergent" -- deltas decrease but p < 0.5 or non-monotone
///   "non-convergent"   -- deltas do not decrease
///   "insufficient-data" -- fewer than 3 refinement levels
/// </summary>
public static class ConvergenceClassifier
{
    public static string Classify(double[] meshParameters, double[] values);
}
```

```csharp
// RefinementStudyRunner.cs
namespace Gu.Phase5.Convergence;

/// <summary>
/// Orchestrates a refinement study:
///   1. For each refinement level, generate/load mesh
///   2. Run pipeline at each level
///   3. Extract target quantities
///   4. Compute successive deltas
///   5. Attempt Richardson extrapolation
///   6. Classify convergence
///   7. Emit ContinuumEstimateRecord or ConvergenceFailureRecord
/// </summary>
public sealed class RefinementStudyRunner
{
    public IReadOnlyList<ContinuumEstimateRecord> Run(
        RefinementStudySpec spec,
        Func<RefinementLevel, IReadOnlyDictionary<string, double>> pipelineExecutor);
}
```

### 4.3 Gu.Phase5.Environments Types (M48)

```csharp
// EnvironmentImportSpec.cs
namespace Gu.Phase5.Environments;

public sealed class EnvironmentImportSpec
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; init; }

    [JsonPropertyName("sourceFormat")]
    public required string SourceFormat { get; init; }

    [JsonPropertyName("geometryTier")]
    public required string GeometryTier { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// StructuredEnvironmentSpec.cs
namespace Gu.Phase5.Environments;

public sealed class StructuredEnvironmentSpec
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("generatorId")]
    public required string GeneratorId { get; init; }

    [JsonPropertyName("parameters")]
    public required IReadOnlyDictionary<string, double> Parameters { get; init; }

    [JsonPropertyName("baseDimension")]
    public required int BaseDimension { get; init; }

    [JsonPropertyName("ambientDimension")]
    public required int AmbientDimension { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// EnvironmentRecord.cs
namespace Gu.Phase5.Environments;

public sealed class EnvironmentRecord
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("geometryTier")]
    public required string GeometryTier { get; init; }

    [JsonPropertyName("geometryFingerprint")]
    public required string GeometryFingerprint { get; init; }

    [JsonPropertyName("baseDimension")]
    public required int BaseDimension { get; init; }

    [JsonPropertyName("ambientDimension")]
    public required int AmbientDimension { get; init; }

    [JsonPropertyName("edgeCount")]
    public required int EdgeCount { get; init; }

    [JsonPropertyName("faceCount")]
    public required int FaceCount { get; init; }

    [JsonPropertyName("admissibility")]
    public required EnvironmentAdmissibilityReport Admissibility { get; init; }

    [JsonPropertyName("sourceSpec")]
    public string? SourceSpec { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// EnvironmentAdmissibilityReport.cs
namespace Gu.Phase5.Environments;

public sealed class EnvironmentAdmissibilityReport
{
    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("checks")]
    public required IReadOnlyList<AdmissibilityCheck> Checks { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
```

```csharp
// AdmissibilityCheck.cs
namespace Gu.Phase5.Environments;

public sealed class AdmissibilityCheck
{
    [JsonPropertyName("checkId")]
    public required string CheckId { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("value")]
    public double? Value { get; init; }

    [JsonPropertyName("threshold")]
    public double? Threshold { get; init; }
}
```

```csharp
// EnvironmentCampaignSpec.cs
namespace Gu.Phase5.Environments;

public sealed class EnvironmentCampaignSpec
{
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("environmentIds")]
    public required IReadOnlyList<string> EnvironmentIds { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("targetQuantities")]
    public required IReadOnlyList<string> TargetQuantities { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key implementations**:

```csharp
// StructuredEnvironmentGenerator.cs
namespace Gu.Phase5.Environments;

/// <summary>
/// Generates structured analytic environments beyond ToyGeometryFactory.
/// Supported generators:
///   "refined-toy-2d" -- Toy2D with configurable refinement level
///   "structured-3d"  -- 3D simplicial complex with configurable parameters
///   "flat-torus-2d"  -- 2D flat torus with periodic BCs
/// </summary>
public static class StructuredEnvironmentGenerator
{
    public static EnvironmentRecord Generate(StructuredEnvironmentSpec spec);
}
```

```csharp
// EnvironmentImporter.cs
namespace Gu.Phase5.Environments;

/// <summary>
/// Imports external geometry data into an EnvironmentRecord.
/// Supported formats: "gu-json" (native), "simplicial-json" (minimal mesh).
/// Validates admissibility before accepting.
/// </summary>
public static class EnvironmentImporter
{
    public static EnvironmentRecord Import(EnvironmentImportSpec spec);
}
```

```csharp
// EnvironmentAdmissibilityChecker.cs
namespace Gu.Phase5.Environments;

/// <summary>
/// Checks:
///   "mesh-valid"       -- mesh has no degenerate faces, positive volumes
///   "dimension-match"  -- baseDim + fiberDim = ambientDim
///   "connectivity"     -- mesh is connected
///   "orientation"      -- mesh is orientable (for differential forms)
/// </summary>
public static class EnvironmentAdmissibilityChecker
{
    public static EnvironmentAdmissibilityReport Check(
        /* mesh data */ int baseDim, int ambientDim, int edgeCount, int faceCount,
        double[] volumes);
}
```

### 4.4 Gu.Phase5.QuantitativeValidation Types (M49)

```csharp
// QuantitativeObservableRecord.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class QuantitativeObservableRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("value")]
    public required double Value { get; init; }

    [JsonPropertyName("uncertainty")]
    public required QuantitativeUncertainty Uncertainty { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public string? RefinementLevel { get; init; }

    [JsonPropertyName("extractionMethod")]
    public required string ExtractionMethod { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// QuantitativeUncertainty.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class QuantitativeUncertainty
{
    [JsonPropertyName("branchVariation")]
    public double BranchVariation { get; init; } = -1;

    [JsonPropertyName("refinementError")]
    public double RefinementError { get; init; } = -1;

    [JsonPropertyName("extractionError")]
    public double ExtractionError { get; init; } = -1;

    [JsonPropertyName("environmentSensitivity")]
    public double EnvironmentSensitivity { get; init; } = -1;

    [JsonPropertyName("totalUncertainty")]
    public double TotalUncertainty { get; init; } = -1;

    /// <summary>-1 means unestimated (preserved, not zeroed).</summary>
    [JsonIgnore]
    public bool IsFullyEstimated =>
        BranchVariation >= 0 && RefinementError >= 0 &&
        ExtractionError >= 0 && EnvironmentSensitivity >= 0;
}
```

```csharp
// CalibrationPolicy.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class CalibrationPolicy
{
    [JsonPropertyName("policyId")]
    public required string PolicyId { get; init; }

    [JsonPropertyName("mode")]
    public required string Mode { get; init; }

    [JsonPropertyName("sigmaThreshold")]
    public double SigmaThreshold { get; init; } = 5.0;

    [JsonPropertyName("requireFullUncertainty")]
    public bool RequireFullUncertainty { get; init; } = false;
}
```

```csharp
// TargetMatchRecord.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class TargetMatchRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("targetLabel")]
    public required string TargetLabel { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }

    [JsonPropertyName("computedValue")]
    public required double ComputedValue { get; init; }

    [JsonPropertyName("computedUncertainty")]
    public required double ComputedUncertainty { get; init; }

    [JsonPropertyName("pull")]
    public required double Pull { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
```

```csharp
// ConsistencyScoreCard.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class ConsistencyScoreCard
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("matches")]
    public required IReadOnlyList<TargetMatchRecord> Matches { get; init; }

    [JsonPropertyName("totalPassed")]
    public required int TotalPassed { get; init; }

    [JsonPropertyName("totalFailed")]
    public required int TotalFailed { get; init; }

    [JsonPropertyName("overallScore")]
    public required double OverallScore { get; init; }

    [JsonPropertyName("calibrationPolicyId")]
    public required string CalibrationPolicyId { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key algorithms**:

```csharp
// UncertaintyPropagator.cs
namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Builds QuantitativeUncertainty from Phase V sources:
///   branchVariation  = from BranchRobustnessRecord (std dev of quantity across variants)
///   refinementError  = from ContinuumEstimateRecord (errorBand)
///   extractionError  = from observation chain (fixed estimate or sensitivity)
///   environmentSensitivity = from environment campaign (std dev across environments)
///   totalUncertainty = quadrature sum of estimated components
/// </summary>
public static class UncertaintyPropagator
{
    public static QuantitativeUncertainty Propagate(
        double? branchVariation,
        double? refinementError,
        double? extractionError,
        double? environmentSensitivity);
}
```

```csharp
// TargetMatcher.cs
namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Compares computed observable to external target:
///   pull = |computed - target| / sqrt(sigma_computed^2 + sigma_target^2)
///   passed = pull <= sigmaThreshold
/// </summary>
public static class TargetMatcher
{
    public static TargetMatchRecord Match(
        QuantitativeObservableRecord computed,
        ExternalTarget target,
        CalibrationPolicy policy);
}
```

```csharp
// ExternalTarget.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class ExternalTarget
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("value")]
    public required double Value { get; init; }

    [JsonPropertyName("uncertainty")]
    public required double Uncertainty { get; init; }

    [JsonPropertyName("source")]
    public required string Source { get; init; }
}
```

```csharp
// ExternalTargetTable.cs
namespace Gu.Phase5.QuantitativeValidation;

public sealed class ExternalTargetTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("targets")]
    public required IReadOnlyList<ExternalTarget> Targets { get; init; }

    public static ExternalTargetTable FromJson(string json);
    public string ToJson(bool indented = true);
}
```

### 4.5 Gu.Phase5.Falsification Types (M50)

```csharp
// FalsifierRecord.cs
namespace Gu.Phase5.Falsification;

public sealed class FalsifierRecord
{
    [JsonPropertyName("falsifierId")]
    public required string FalsifierId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("falsifierType")]
    public required string FalsifierType { get; init; }

    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    [JsonPropertyName("targetId")]
    public required string TargetId { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("environmentId")]
    public string? EnvironmentId { get; init; }

    [JsonPropertyName("triggerValue")]
    public double? TriggerValue { get; init; }

    [JsonPropertyName("threshold")]
    public double? Threshold { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("evidence")]
    public required string Evidence { get; init; }

    [JsonPropertyName("active")]
    public required bool Active { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// FalsifierType.cs (static string constants, not enum)
namespace Gu.Phase5.Falsification;

public static class FalsifierTypes
{
    public const string BranchFragility = "branch-fragility";
    public const string NonConvergence = "non-convergence";
    public const string ObservationInstability = "observation-instability";
    public const string EnvironmentInstability = "environment-instability";
    public const string QuantitativeMismatch = "quantitative-mismatch";
    public const string RepresentationContent = "representation-content";
    public const string CouplingInconsistency = "coupling-inconsistency";
}
```

```csharp
// FalsifierSeverity.cs
namespace Gu.Phase5.Falsification;

public static class FalsifierSeverity
{
    public const string Fatal = "fatal";
    public const string High = "high";
    public const string Medium = "medium";
    public const string Low = "low";
    public const string Informational = "informational";
}
```

```csharp
// FalsifierSummary.cs
namespace Gu.Phase5.Falsification;

public sealed class FalsifierSummary
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("falsifiers")]
    public required IReadOnlyList<FalsifierRecord> Falsifiers { get; init; }

    [JsonPropertyName("activeFatalCount")]
    public required int ActiveFatalCount { get; init; }

    [JsonPropertyName("activeHighCount")]
    public required int ActiveHighCount { get; init; }

    [JsonPropertyName("totalActiveCount")]
    public required int TotalActiveCount { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key implementations**:

```csharp
// FalsifierEvaluator.cs
namespace Gu.Phase5.Falsification;

/// <summary>
/// Evaluates falsifier conditions against Phase V study results.
/// Creates FalsifierRecord for each triggered condition.
///
/// Falsifier sources:
///   BranchFragility      -- from BranchRobustnessRecord (any variant with fragilityScore > threshold)
///   NonConvergence       -- from ConvergenceFailureRecord (any non-convergent quantity)
///   ObservationInstability -- from observation chain sensitivity
///   EnvironmentInstability -- from environment campaign variance
///   QuantitativeMismatch   -- from ConsistencyScoreCard (failed matches)
///   RepresentationContent  -- structural mismatch (e.g., wrong number of modes)
///   CouplingInconsistency  -- coupling proxy inconsistency across branches
/// </summary>
public sealed class FalsifierEvaluator
{
    public FalsifierSummary Evaluate(
        string studyId,
        BranchRobustnessRecord? branchRecord,
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        IReadOnlyList<ConvergenceFailureRecord>? convergenceFailures,
        ConsistencyScoreCard? scoreCard,
        FalsificationPolicy policy,
        ProvenanceMeta provenance);
}
```

```csharp
// FalsificationPolicy.cs
namespace Gu.Phase5.Falsification;

public sealed class FalsificationPolicy
{
    [JsonPropertyName("branchFragilityThreshold")]
    public double BranchFragilityThreshold { get; init; } = 0.5;

    [JsonPropertyName("convergenceFailureSeverity")]
    public string ConvergenceFailureSeverity { get; init; } = "high";

    [JsonPropertyName("quantitativeMismatchSeverity")]
    public string QuantitativeMismatchSeverity { get; init; } = "high";

    [JsonPropertyName("environmentInstabilityThreshold")]
    public double EnvironmentInstabilityThreshold { get; init; } = 0.3;
}
```

```csharp
// RegistryDemotionIntegrator.cs
namespace Gu.Phase5.Falsification;

/// <summary>
/// Applies falsifier-driven demotions to UnifiedParticleRegistry.
/// For each active falsifier:
///   Fatal  -> cap affected candidate at C0
///   High   -> demote by 2 levels
///   Medium -> demote by 1 level
///   Low    -> warning only (no demotion)
/// </summary>
public static class RegistryDemotionIntegrator
{
    public static UnifiedParticleRegistry ApplyDemotions(
        UnifiedParticleRegistry registry,
        FalsifierSummary falsifiers);
}
```

### 4.6 Gu.Phase5.Dossiers Types (M51 + M52)

```csharp
// ClaimEscalationRecord.cs
namespace Gu.Phase5.Dossiers;

public sealed class ClaimEscalationRecord
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("previousClaimClass")]
    public required string PreviousClaimClass { get; init; }

    [JsonPropertyName("newClaimClass")]
    public required string NewClaimClass { get; init; }

    [JsonPropertyName("direction")]
    public required string Direction { get; init; }

    [JsonPropertyName("gatesPassed")]
    public required IReadOnlyList<string> GatesPassed { get; init; }

    [JsonPropertyName("gatesFailed")]
    public required IReadOnlyList<string> GatesFailed { get; init; }

    [JsonPropertyName("evidence")]
    public required string Evidence { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// EscalationGate.cs
namespace Gu.Phase5.Dossiers;

public static class EscalationGates
{
    public const string BranchRobust = "branch-robust";
    public const string RefinementBounded = "refinement-bounded";
    public const string MultiEnvironment = "multi-environment";
    public const string ObservationChainValid = "observation-chain-valid";
    public const string QuantitativeMatch = "quantitative-match";
    public const string NoActiveFatalFalsifier = "no-active-fatal-falsifier";
}
```

```csharp
// ClaimEscalationEngine.cs
namespace Gu.Phase5.Dossiers;

/// <summary>
/// Evaluates escalation gates for each candidate:
///   1. BranchRobust: candidate quantity is in an equivalence class covering >50% of variants
///   2. RefinementBounded: continuum estimate has errorBand < 10% of value
///   3. MultiEnvironment: quantity computed on >= 2 environment tiers
///   4. ObservationChainValid: observation provenance chain is complete
///   5. QuantitativeMatch: at least one target match passed
///   6. NoActiveFatalFalsifier: no fatal falsifier targets this candidate
///
/// Promotion: all gates passed -> promote by 1 level (max C5)
/// Demotion: any fatal falsifier active -> demote to C0
/// Hold: some gates failed, no fatal -> no change
/// </summary>
public sealed class ClaimEscalationEngine
{
    public IReadOnlyList<ClaimEscalationRecord> Evaluate(
        UnifiedParticleRegistry registry,
        BranchRobustnessRecord? branchRecord,
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        ConsistencyScoreCard? scoreCard,
        FalsifierSummary? falsifiers,
        IReadOnlyList<string> environmentTiersCovered,
        ProvenanceMeta provenance);
}
```

```csharp
// ValidationDossier.cs
namespace Gu.Phase5.Dossiers;

public sealed class ValidationDossier
{
    [JsonPropertyName("dossierId")]
    public required string DossierId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("branchFamilySummary")]
    public BranchRobustnessRecord? BranchFamilySummary { get; init; }

    [JsonPropertyName("refinementSummary")]
    public IReadOnlyList<ContinuumEstimateRecord>? RefinementSummary { get; init; }

    [JsonPropertyName("convergenceFailures")]
    public IReadOnlyList<ConvergenceFailureRecord>? ConvergenceFailures { get; init; }

    [JsonPropertyName("environmentSummary")]
    public IReadOnlyList<EnvironmentRecord>? EnvironmentSummary { get; init; }

    [JsonPropertyName("quantitativeComparison")]
    public ConsistencyScoreCard? QuantitativeComparison { get; init; }

    [JsonPropertyName("falsifierSummary")]
    public FalsifierSummary? FalsifierSummary { get; init; }

    [JsonPropertyName("claimEscalations")]
    public IReadOnlyList<ClaimEscalationRecord>? ClaimEscalations { get; init; }

    [JsonPropertyName("negativeResults")]
    public IReadOnlyList<NegativeResultEntry>? NegativeResults { get; init; }

    [JsonPropertyName("freshness")]
    public required string Freshness { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    [JsonPropertyName("generatedAt")]
    public required DateTimeOffset GeneratedAt { get; init; }

    public string ToJson(bool indented = true);
    public static ValidationDossier FromJson(string json);
}
```

```csharp
// DossierIndex.cs
namespace Gu.Phase5.Dossiers;

public sealed class DossierIndex
{
    [JsonPropertyName("indexId")]
    public required string IndexId { get; init; }

    [JsonPropertyName("entries")]
    public required IReadOnlyList<DossierIndexEntry> Entries { get; init; }
}

public sealed class DossierIndexEntry
{
    [JsonPropertyName("dossierId")]
    public required string DossierId { get; init; }

    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("outcome")]
    public required string Outcome { get; init; }

    [JsonPropertyName("artifactPath")]
    public required string ArtifactPath { get; init; }
}
```

```csharp
// NegativeResultLedger.cs
namespace Gu.Phase5.Dossiers;

public sealed class NegativeResultLedger
{
    [JsonPropertyName("ledgerId")]
    public required string LedgerId { get; init; }

    [JsonPropertyName("entries")]
    public required List<NegativeResultEntry> Entries { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    public void Add(NegativeResultEntry entry) => Entries.Add(entry);
}
```

```csharp
// DossierAssembler.cs
namespace Gu.Phase5.Dossiers;

/// <summary>
/// Assembles a complete ValidationDossier from Phase V study results:
///   1. Gather branch robustness record
///   2. Gather continuum estimates and failures
///   3. Gather environment records
///   4. Gather consistency scorecard
///   5. Gather falsifier summary
///   6. Run claim escalation engine
///   7. Collect negative results
///   8. Verify identity consistency (branch/environment/refinement)
///   9. Emit dossier
/// </summary>
public sealed class DossierAssembler
{
    public ValidationDossier Assemble(
        string studyId,
        BranchRobustnessRecord? branchRecord,
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        IReadOnlyList<ConvergenceFailureRecord>? convergenceFailures,
        IReadOnlyList<EnvironmentRecord>? environments,
        ConsistencyScoreCard? scoreCard,
        FalsifierSummary? falsifiers,
        UnifiedParticleRegistry? registry,
        IReadOnlyList<string> environmentTiersCovered,
        string freshness,
        ProvenanceMeta provenance);
}
```

### 4.7 Gu.Phase5.Reporting Types (M53)

```csharp
// Phase5Report.cs
namespace Gu.Phase5.Reporting;

public sealed class Phase5Report
{
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("dossierIds")]
    public required IReadOnlyList<string> DossierIds { get; init; }

    [JsonPropertyName("branchIndependenceAtlas")]
    public BranchIndependenceAtlas? BranchIndependenceAtlas { get; init; }

    [JsonPropertyName("convergenceAtlas")]
    public ConvergenceAtlas? ConvergenceAtlas { get; init; }

    [JsonPropertyName("falsificationDashboard")]
    public FalsificationDashboard? FalsificationDashboard { get; init; }

    [JsonPropertyName("negativeResultSummary")]
    public IReadOnlyList<NegativeResultEntry>? NegativeResultSummary { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    [JsonPropertyName("generatedAt")]
    public required DateTimeOffset GeneratedAt { get; init; }

    public string ToJson(bool indented = true);
    public static Phase5Report FromJson(string json);
}
```

```csharp
// BranchIndependenceAtlas.cs
namespace Gu.Phase5.Reporting;

public sealed class BranchIndependenceAtlas
{
    [JsonPropertyName("totalQuantities")]
    public required int TotalQuantities { get; init; }

    [JsonPropertyName("invariantCount")]
    public required int InvariantCount { get; init; }

    [JsonPropertyName("fragileCount")]
    public required int FragileCount { get; init; }

    [JsonPropertyName("equivalenceClassCount")]
    public required int EquivalenceClassCount { get; init; }

    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }
}
```

```csharp
// ConvergenceAtlas.cs
namespace Gu.Phase5.Reporting;

public sealed class ConvergenceAtlas
{
    [JsonPropertyName("totalQuantities")]
    public required int TotalQuantities { get; init; }

    [JsonPropertyName("convergentCount")]
    public required int ConvergentCount { get; init; }

    [JsonPropertyName("nonConvergentCount")]
    public required int NonConvergentCount { get; init; }

    [JsonPropertyName("insufficientDataCount")]
    public required int InsufficientDataCount { get; init; }

    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }
}
```

```csharp
// FalsificationDashboard.cs
namespace Gu.Phase5.Reporting;

public sealed class FalsificationDashboard
{
    [JsonPropertyName("totalFalsifiers")]
    public required int TotalFalsifiers { get; init; }

    [JsonPropertyName("activeFatalCount")]
    public required int ActiveFatalCount { get; init; }

    [JsonPropertyName("activeHighCount")]
    public required int ActiveHighCount { get; init; }

    [JsonPropertyName("promotionCount")]
    public required int PromotionCount { get; init; }

    [JsonPropertyName("demotionCount")]
    public required int DemotionCount { get; init; }

    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }
}
```

```csharp
// Phase5CampaignRunner.cs
namespace Gu.Phase5.Reporting;

/// <summary>
/// Orchestrates the full M53 reference study:
///   1. Load/generate branch family with >=1 nontrivial torsion/Shiab variation
///   2. Run branch robustness study (M46)
///   3. Run refinement study on surviving branches (M47)
///   4. Run on toy + structured environments (M48)
///   5. Compute quantitative observables and compare to targets (M49)
///   6. Evaluate falsifiers (M50)
///   7. Run claim escalation (M51)
///   8. Assemble positive/mixed dossier + negative dossier (M52)
///   9. Generate final report
///  10. Write all artifacts to study directory
/// </summary>
public sealed class Phase5CampaignRunner
{
    public Phase5Report Run(Phase5CampaignSpec spec, string artifactsDir);
}
```

```csharp
// Phase5CampaignSpec.cs
namespace Gu.Phase5.Reporting;

public sealed class Phase5CampaignSpec
{
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("branchFamilySpec")]
    public required BranchRobustnessStudySpec BranchFamilySpec { get; init; }

    [JsonPropertyName("refinementSpec")]
    public required RefinementStudySpec RefinementSpec { get; init; }

    [JsonPropertyName("environmentCampaignSpec")]
    public required EnvironmentCampaignSpec EnvironmentCampaignSpec { get; init; }

    [JsonPropertyName("externalTargetTablePath")]
    public required string ExternalTargetTablePath { get; init; }

    [JsonPropertyName("calibrationPolicy")]
    public required CalibrationPolicy CalibrationPolicy { get; init; }

    [JsonPropertyName("falsificationPolicy")]
    public required FalsificationPolicy FalsificationPolicy { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

```csharp
// Phase5ReportGenerator.cs -- builds markdown + JSON from dossiers
namespace Gu.Phase5.Reporting;

public static class Phase5ReportGenerator
{
    public static Phase5Report Generate(
        string studyId,
        IReadOnlyList<ValidationDossier> dossiers,
        ProvenanceMeta provenance);

    public static string ToMarkdown(Phase5Report report);
}
```

---

## 5. CLI Commands

Add these to the switch statement in `apps/Gu.Cli/Program.cs`:

```csharp
case "branch-robustness":
    return BranchRobustness(args);
case "refinement-study":
    return RefinementStudy(args);
case "import-environment":
    return ImportEnvironment(args);
case "build-structured-environment":
    return BuildStructuredEnvironment(args);
case "validate-quantitative":
    return ValidateQuantitative(args);
case "build-validation-dossier":
    return BuildValidationDossier(args);
case "verify-study-freshness":
    return VerifyStudyFreshness(args);
```

Each CLI command follows the established pattern:
1. Parse args and flags
2. Load spec from JSON file (first positional arg)
3. Execute the corresponding runner
4. Write JSON output to stdout or specified output directory
5. Return 0 on success, 1 on failure

**CLI csproj additions**: The `apps/Gu.Cli/Gu.Cli.csproj` must add
ProjectReferences to all 7 new Phase V src projects.

---

## 6. JSON Schemas

Add to `schemas/`:

```
schemas/branch_robustness_study.schema.json
schemas/branch_robustness_record.schema.json
schemas/branch_distance_matrix.schema.json
schemas/refinement_study.schema.json
schemas/continuum_estimate.schema.json
schemas/environment_campaign.schema.json
schemas/environment_record.schema.json
schemas/quantitative_validation.schema.json
schemas/falsifier_record.schema.json
schemas/validation_dossier.schema.json
```

Each schema follows the existing pattern:
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "gu://schemas/{name}.schema.json",
  "title": "{TypeName}",
  "description": "...",
  "type": "object",
  "required": [...],
  "properties": { ... },
  "additionalProperties": false
}
```

---

## 7. Artifact Layout

Phase V study artifacts go under:

```
studies/phase5_su2_branch_refinement_env_validation/
  run_study.sh
  artifacts/
    branch_independence/
      branch_robustness_record.json
      distance_matrices/
        {quantityId}.json
    convergence/
      continuum_estimates/
        {quantityId}.json
      convergence_failures/
        {quantityId}.json
      richardson_fits/
        {quantityId}.json
    environments/
      {environmentId}.json
    quantitative_validation/
      consistency_scorecard.json
      target_matches/
        {observableId}.json
    falsifiers/
      falsifier_summary.json
      {falsifierId}.json
    dossiers/
      positive_dossier.json
      negative_dossier.json
      dossier_index.json
    reports/
      phase5_report.json
      phase5_report.md
      negative_result_ledger.json
```

---

## 8. Milestone Implementation Guide

### 8.1 M46 -- Branch-independence substrate

**Owner**: implementer assigned to task #8
**Project**: `src/Gu.Phase5.BranchIndependence/`
**Depends on**: Entry gaps G-001 closed (branch resolution working)

**Implementation order**:
1. `BranchDistanceTolerances.cs`
2. `BranchDistanceMatrix.cs`
3. `BranchDistanceComputer.cs` -- pairwise distance computation
4. `EquivalenceClassifier.cs` -- single-linkage clustering
5. `FragilityScorer.cs` -- fragility computation
6. `InvarianceCandidateRecord.cs`
7. `BranchRobustnessStudySpec.cs`
8. `BranchRobustnessRecord.cs`
9. `BranchRobustnessRunner.cs` -- orchestrator

**Tests** (~40 tests):
- BranchDistanceComputer: zero distance for identical arrays, known distance for test data, relative-l2 and absolute-l2 metrics
- EquivalenceClassifier: single cluster, multiple clusters, singleton outlier
- FragilityScorer: all-equal -> score 0, one outlier -> high fragility
- BranchRobustnessRunner: integration test with 3 synthetic variants producing different spectral values
- Invariance: quantity that is the same across all variants -> isInvariant=true

**Completion criterion**: Branch-family studies run on at least one bosonic and one fermionic target quantity.

### 8.2 M47 -- Refinement and continuum framework

**Owner**: implementer assigned to task #9
**Project**: `src/Gu.Phase5.Convergence/`
**Depends on**: M46

**Implementation order**:
1. `RefinementLevel.cs`
2. `RefinementRunRecord.cs`
3. `RefinementStudySpec.cs`
4. `RichardsonFitRecord.cs`
5. `RichardsonExtrapolator.cs` -- core math
6. `ConvergenceClassifier.cs`
7. `ContinuumEstimateRecord.cs`
8. `ConvergenceFailureRecord.cs`
9. `RefinementStudyRunner.cs` -- orchestrator

**Tests** (~35 tests):
- RichardsonExtrapolator: known analytic function Q(h)=Q0+C*h^2, verify p~2 and Q0 recovery
- RichardsonExtrapolator: non-uniform mesh ratios
- ConvergenceClassifier: convergent, weakly-convergent, non-convergent, insufficient-data
- RefinementStudyRunner: integration test with 4-level ladder on a synthetic quantity
- ConvergenceFailure: oscillating values -> non-convergent classification preserved

**Completion criterion**: At least one spectral quantity and one observed quantity can be analyzed across a refinement ladder.

### 8.3 M48 -- Structured and imported environments

**Owner**: implementer assigned to task #10
**Project**: `src/Gu.Phase5.Environments/`
**Depends on**: none (can proceed in parallel with M46/M47)

**Implementation order**:
1. `AdmissibilityCheck.cs`
2. `EnvironmentAdmissibilityReport.cs`
3. `EnvironmentAdmissibilityChecker.cs`
4. `EnvironmentRecord.cs`
5. `StructuredEnvironmentSpec.cs`
6. `StructuredEnvironmentGenerator.cs`
7. `EnvironmentImportSpec.cs`
8. `EnvironmentImporter.cs`
9. `EnvironmentCampaignSpec.cs`

**Tests** (~30 tests):
- EnvironmentAdmissibilityChecker: valid mesh passes, zero-volume face fails, dimension mismatch fails
- StructuredEnvironmentGenerator: "refined-toy-2d" produces valid mesh, "flat-torus-2d" produces valid mesh
- EnvironmentImporter: valid JSON loads, invalid format rejects
- EnvironmentRecord: geometry tier correctly recorded

**Completion criterion**: At least one imported or structured non-toy environment runs through the admissibility pipeline.

### 8.4 M49 -- Quantitative validation engine

**Owner**: implementer assigned to task #11
**Project**: `src/Gu.Phase5.QuantitativeValidation/`
**Depends on**: M46, M47, M48

**Implementation order**:
1. `QuantitativeUncertainty.cs`
2. `CalibrationPolicy.cs`
3. `ExternalTarget.cs`, `ExternalTargetTable.cs`
4. `QuantitativeObservableRecord.cs`
5. `UncertaintyPropagator.cs`
6. `TargetMatchRecord.cs`
7. `TargetMatcher.cs`
8. `ConsistencyScoreCard.cs`
9. `QuantitativeValidationRunner.cs` -- orchestrator

**Tests** (~30 tests):
- UncertaintyPropagator: quadrature sum correct, unestimated components preserved as -1
- TargetMatcher: exact match -> pull=0, 5-sigma mismatch -> passed=false
- ConsistencyScoreCard: all pass -> overallScore=1.0, all fail -> overallScore=0.0
- ExternalTargetTable: round-trip JSON serialization
- Integration: synthetic observable vs synthetic target with known pull

**Completion criterion**: At least one registry candidate can be quantitatively compared against an external target with explicit uncertainty and demotion rules.

### 8.5 M50 -- Falsification engine

**Owner**: implementer assigned to task #12
**Project**: `src/Gu.Phase5.Falsification/`
**Depends on**: M46, M47, M49

**Implementation order**:
1. `FalsifierTypes.cs`, `FalsifierSeverity.cs`
2. `FalsifierRecord.cs`
3. `FalsificationPolicy.cs`
4. `FalsifierSummary.cs`
5. `FalsifierEvaluator.cs` -- core logic
6. `RegistryDemotionIntegrator.cs`

**Tests** (~30 tests):
- FalsifierEvaluator: fragile branch -> branch-fragility falsifier emitted
- FalsifierEvaluator: convergence failure -> non-convergence falsifier emitted
- FalsifierEvaluator: quantitative mismatch -> quantitative-mismatch falsifier emitted
- FalsifierEvaluator: no issues -> empty falsifier list
- RegistryDemotionIntegrator: fatal falsifier -> candidate capped at C0
- RegistryDemotionIntegrator: high falsifier -> demote by 2
- Serialization round-trip for all types

**Completion criterion**: At least one study emits an explicit falsifier artifact when a branch or quantity fails.

### 8.6 M51+M52 -- Claim escalation and validation dossier system

**Owner**: implementer assigned to task #13
**Project**: `src/Gu.Phase5.Dossiers/`
**Depends on**: M46, M47, M48, M49, M50

**Implementation order**:
1. `EscalationGates.cs`
2. `ClaimEscalationRecord.cs`
3. `ClaimEscalationEngine.cs`
4. `NegativeResultLedger.cs`
5. `DossierIndexEntry.cs`, `DossierIndex.cs`
6. `ValidationDossier.cs`
7. `DossierAssembler.cs`

**Tests** (~35 tests):
- ClaimEscalationEngine: all gates pass -> promote
- ClaimEscalationEngine: fatal falsifier -> demote to C0
- ClaimEscalationEngine: some gates fail, no fatal -> hold
- ClaimEscalationEngine: cannot promote above C5
- DossierAssembler: assembles all fields correctly
- DossierAssembler: null optional fields handled gracefully
- ValidationDossier: JSON round-trip
- NegativeResultLedger: entries accumulate correctly
- DossierIndex: entries sorted by outcome

**Completion criterion**: Candidates cannot be promoted without passing declared gates. One complete study can be summarized by a single dossier artifact.

### 8.7 M53 -- First high-value Phase V campaign + Reporting

**Owner**: implementer assigned to task #14
**Project**: `src/Gu.Phase5.Reporting/`
**Depends on**: M46-M52 all complete

**Implementation order**:
1. `BranchIndependenceAtlas.cs`
2. `ConvergenceAtlas.cs`
3. `FalsificationDashboard.cs`
4. `Phase5Report.cs`
5. `Phase5CampaignSpec.cs`
6. `Phase5CampaignRunner.cs`
7. `Phase5ReportGenerator.cs`
8. Reference study: `studies/phase5_su2_branch_refinement_env_validation/`

**Tests** (~30 tests):
- Phase5ReportGenerator: produces valid markdown
- Phase5Report: JSON round-trip
- BranchIndependenceAtlas: correct counts
- ConvergenceAtlas: correct counts
- FalsificationDashboard: correct counts
- Phase5CampaignRunner: integration test (end-to-end with synthetic data)

**Reference study implementation**:
- `studies/phase5_su2_branch_refinement_env_validation/run_study.sh`
- Inputs: su(2) branch family with trivial + identity-shiab + (augmented-torsion if registered), toy + structured environments, 3-level refinement ladder, small external target table
- Expected outputs: branch robustness report, continuum estimate, environment sensitivity, quantitative validation, one positive/mixed dossier, one negative dossier

**Completion criterion**: The campaign produces registry-grade evidence rather than only intermediate diagnostics.

---

## 9. Testing Strategy

### 9.1 Test counts target

| Project | Target |
|---|---|
| Gu.Phase5.BranchIndependence.Tests | ~40 |
| Gu.Phase5.Convergence.Tests | ~35 |
| Gu.Phase5.Environments.Tests | ~30 |
| Gu.Phase5.QuantitativeValidation.Tests | ~30 |
| Gu.Phase5.Falsification.Tests | ~30 |
| Gu.Phase5.Dossiers.Tests | ~35 |
| Gu.Phase5.Reporting.Tests | ~30 |
| **Total** | **~230** |

### 9.2 Required test categories

**Unit tests**: distance math, fragility scoring, extrapolation logic, uncertainty propagation, falsifier classification, dossier assembly

**Integration tests**: branch family -> comparison -> report, refinement ladder -> continuum estimate -> report, environment import -> admissibility -> pipeline, quantitative validation -> escalation/demotion

**Replay tests**: Each major Phase V artifact must roundtrip serialize/deserialize with identical content

**Negative-result tests**: non-convergent studies remain serialized, falsified branches remain in ledger, reports preserve failure reasoning

### 9.3 Build/test command

```bash
dotnet build && dotnet test --no-build
```

Always use this two-step pattern. Never `dotnet test` alone (triggers rebuild, can be flaky).

---

## 10. CUDA Parity

Phase V is primarily a post-processing and analysis layer. It does not introduce new CUDA kernels. However:

1. `BranchRobustnessRunner` must work with both CPU and GPU-computed pipeline outputs. The `pipelineExecutor` callback abstraction handles this.
2. `RefinementStudyRunner` similarly uses callbacks, backend-agnostic.
3. All records carry provenance with backend identity.
4. `ComputedWithUnverifiedGpu` flags propagate through falsification and dossier systems.
5. No new GPU stubs needed -- Phase V consumes existing Phase III/IV outputs.

---

## 11. Key Design Decisions

1. **Sealed classes, not records**: All types follow the existing pattern. No `with` syntax.
2. **STJ serialization**: All JSON properties use `[JsonPropertyName]` with kebab-case.
3. **IReadOnlyList / IReadOnlyDictionary for inputs, List for mutable outputs**: Follow existing Phase III/IV pattern.
4. **Callbacks for pipeline execution**: `BranchRobustnessRunner` and `RefinementStudyRunner` take `Func<>` delegates rather than hard-coding specific pipeline steps. This keeps Phase V decoupled from Phase III/IV internals.
5. **Unestimated uncertainty = -1**: Follows the `UncertaintyDecomposer` convention from Phase II.
6. **Falsifier types are strings, not enums**: For extensibility and JSON friendliness. Static constants in `FalsifierTypes`.
7. **Claim class strings match Phase III/IV pattern**: "C0" through "C5" with suffixes.
8. **No circular dependencies**: Strict DAG from BranchIndependence -> ... -> Reporting.
9. **ValidationDossier is the central Phase V product**: Everything feeds into it.
10. **Negative results are first-class**: `NegativeResultLedger` and `ConvergenceFailureRecord` are preserved, not discarded.

---

## 12. Implementer Assignment Summary

| Task | Milestone | Owner |
|---|---|---|
| G-001, G-002 | Entry gaps | implementer-1 |
| G-003, G-004 | Entry gaps | implementer-2 |
| G-005, G-006 | Entry gaps | implementer-3 |
| M46 | BranchIndependence | implementer-4 |
| M47 | Convergence | implementer-5 |
| M48 | Environments | implementer-6 |
| M49 | QuantitativeValidation | implementer-4 (after M46) |
| M50 | Falsification | implementer-5 (after M47) |
| M51+M52 | Dossiers | implementer-6 (after M48) |
| M53 | Reporting + Campaign | implementer-4 (after M49/M50/M51) |
| CLI commands | All new commands | whoever implements the corresponding milestone |
| Schemas | All new schemas | whoever implements the corresponding milestone |

Gap closers (implementer-1, implementer-2, implementer-3) should transition to
helping with M49/M50/M51+ after their gaps are closed.

---

## 13. Physicist-Resolved Physics Decisions

All 4 physics questions have been answered by the physicist. These are now
**binding implementation decisions**.

### 13.1 Branch Distance Metrics (RESOLVED)

**Default metric**: relative-L2 for generic field-valued quantities:
```
d(a, b) = ||a - b||_2 / max(||a||_2, ||b||_2, epsilon)
```
where `epsilon = 1e-14` (floor to avoid 0/0).

**Spectral metric** for eigenvalues (Hessian, Dirac): relative spectral L-infinity:
```
d_spec(lambda, mu) = max_i |lambda_i - mu_i| / max(max_i |lambda_i|, max_i |mu_i|, epsilon)
```

**Why**: L-infinity catches single large eigenvalue shifts in mostly-stable spectra.
Relative normalization makes fragility scores comparable across quantities with
different natural scales.

**Implementation**:
- `BranchDistanceComputer` must support metric IDs: `"relative-l2"`, `"spectral-linf-relative"`, `"absolute-l2"`
- `BranchDistanceMatrix` stores the metric ID string
- `BranchRobustnessStudySpec` allows overriding the default metric
- M53 reference study: use `"spectral-linf-relative"` for bosonic/fermionic eigenvalues, `"relative-l2"` for everything else

### 13.2 External Target Values for M53 (RESOLVED)

Use **eigenvalue ratios** (normalized to largest eigenvalue), not raw eigenvalues.
Set uncertainties at 30-50% of value.

**Toy bosonic targets** (3 entries):
```json
[
  { "label": "light-boson", "observableId": "boson-eigenvalue-ratio-1", "value": 0.1, "uncertainty": 0.05, "source": "synthetic-toy-v1" },
  { "label": "medium-boson", "observableId": "boson-eigenvalue-ratio-2", "value": 1.0, "uncertainty": 0.3, "source": "synthetic-toy-v1" },
  { "label": "heavy-boson", "observableId": "boson-eigenvalue-ratio-3", "value": 5.0, "uncertainty": 1.5, "source": "synthetic-toy-v1" }
]
```

**Toy fermionic targets** (2 entries):
```json
[
  { "label": "light-fermion", "observableId": "fermion-dirac-eigenvalue-1", "value": 0.05, "uncertainty": 0.02, "source": "synthetic-toy-v1" },
  { "label": "heavy-fermion", "observableId": "fermion-dirac-eigenvalue-2", "value": 2.0, "uncertainty": 0.5, "source": "synthetic-toy-v1" }
]
```

**CRITICAL constraints**:
- Target table MUST include `targetProvenance: "synthetic-toy-v1"` -- no downstream
  report may mistake these for real experimental values.
- Labeled as `evidenceTier: "toy-placeholder"` in all dossier artifacts.
- Values chosen so toy su(2) spectrum will likely match SOME (testing "compatible") and
  miss others (testing "incompatible" / falsifier path).

### 13.3 Richardson Extrapolation (RESOLVED -- valid with caveats)

Richardson extrapolation IS appropriate for the discrete GU mesh hierarchy.

**Refinement parameter**: For product mesh Y_h = X_h x F_h, use **uniform joint
refinement** (halve both h_X and h_F together) so there's a single h parameter.
Use `h = max(h_X, h_F)`.

**Expected convergence orders** (P1 elements, centroid quadrature):
- Eigenvalues of second-order operators (Hessian): p ~ 2
- Eigenvalues of first-order operators (Dirac): p ~ 1 (unstructured), p ~ 2 (structured)
- Residual norms (L2): p ~ 2
- Field values (omega components): p ~ 2

**Implementation of Richardson**:
1. Use at least 3 levels, prefer 4.
2. Fit log|Q_h - Q_{h/2}| vs log(h) to estimate p.
3. Extrapolate: Q_inf = (Q_{h/2} * r^p - Q_h) / (r^p - 1) where r = 2.
4. Report estimated p, fit residual, and error band.
5. Flag `convergenceNote: "anomalous-order"` if |p_estimated - p_expected| > 1 or p < 0.

**When Richardson fails** (record as result, not bug):
- Pre-asymptotic: first 2-3 levels don't follow power law -> `"pre-asymptotic"`
- Mesh-topology-sensitive: p < 0 or wildly varying -> `"non-convergent"`
- Gauge drift: inadequate gauge-fixing masks convergence

**CRITICAL**: Keep gauge penalty coefficient lambda FIXED across refinements.
If lambda scales with h, the effective equation changes and Richardson is invalid.

### 13.4 Branch Family for M53 (RESOLVED -- 4 variants)

**Available operators in codebase**:
- Shiab: `"identity-shiab"` (IdentityShiabCpu), `"first-order-curvature"` (equivalent to identity)
- Torsion: `"trivial"` (TrivialTorsionCpu), `"augmented-torsion"` (AugmentedTorsionCpu)
- BiConnection: `"simple-a0-omega"` (A=A0, B=omega), `"a0-plus-minus-omega"` (A=A0+omega, B=A0-omega)

**M53 branch family** (4 variants):

| Variant | Shiab | Torsion | BiConnection | Physics meaning |
|---------|-------|---------|-------------|-----------------|
| V1 (baseline) | identity-shiab | trivial | simple-a0-omega | Upsilon = F, simplest branch |
| V2 | identity-shiab | augmented-torsion | simple-a0-omega | Upsilon = F - d_{A0}(omega-A0), torsion correction |
| V3 | identity-shiab | trivial | a0-plus-minus-omega | Upsilon = F, different bi-connection |
| V4 | identity-shiab | augmented-torsion | a0-plus-minus-omega | Full: torsion + different bi-connection |

**V2 and V4 REQUIRE nonzero A0.** Use cos-sin profile:
```
A0_e = 0.3 * cos(pi * mx_e) * T_1 + 0.3 * sin(pi * mx_e) * T_2
```
Scale factor 0.3 keeps A0 < omega (A0 is background, omega is dynamical).

**What this tests**:
- V1 vs V2: torsion sensitivity
- V1 vs V3: bi-connection sensitivity
- V2 vs V4: torsion/bi-connection interaction
- 4 variants = minimum for meaningful 4x4 distance matrix

**Known limitations** (record in PHASE_5_OPEN_ISSUES.md):
- No non-identity Shiab (e.g., metric-contracted *F or [g,F])
- No non-adjoint gauge representations for Dirac
- Note as `"ShiabVariationScope: identity-only"` in M53 dossier

---

## 14. Definition of Done for Phase V

1. All 6 entry gaps (G-001 through G-006) closed
2. 7 new src projects build with 0 errors, 0 warnings
3. 7 new test projects pass (~230 tests)
4. 7 new CLI commands functional
5. 10 new JSON schemas committed
6. Reference study produces complete artifacts
7. At least one positive/mixed validation dossier
8. At least one negative-result validation dossier
9. ASSUMPTIONS.md updated
10. PHASE_5_OPEN_ISSUES.md created
