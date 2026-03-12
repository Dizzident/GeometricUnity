# ARCH_P4.md -- Phase IV Architectural Specification

## Purpose

This document is the detailed architectural spec for Phase IV implementation.
It defines exact project structure, interface signatures, data flow, dependency
graph, and per-milestone implementation guidance for all implementers.

Phase IV adds: fermionic sector, Dirac operator stack, chirality/conjugation
analysis, boson-fermion coupling extraction, generation clustering, and a
unified particle registry -- all on top of the Phase I-III platform.

---

## 1. Project Structure

### 1.1 New Source Projects (10 projects)

All under `src/`:

```
src/Gu.Phase4.Spin/              -- Clifford algebra, gamma conventions, spinor specs
src/Gu.Phase4.Fermions/          -- Fermion field layout, discrete state containers
src/Gu.Phase4.Dirac/             -- Spin connection, Dirac operator, spectral solver
src/Gu.Phase4.Chirality/         -- Chirality projectors, conjugation, leakage diagnostics
src/Gu.Phase4.Couplings/         -- Boson-fermion coupling proxy extraction
src/Gu.Phase4.FamilyClustering/  -- Fermion mode tracking, generation clustering
src/Gu.Phase4.Registry/          -- Unified particle registry (bosons + fermions + interactions)
src/Gu.Phase4.Observation/       -- Fermion observation pipeline (Y_h -> X_h)
src/Gu.Phase4.Comparison/        -- Fermion/coupling comparison campaigns
src/Gu.Phase4.Reporting/         -- Phase IV reports and dashboards
```

### 1.2 New Test Projects (10 projects)

All under `tests/`:

```
tests/Gu.Phase4.Spin.Tests/
tests/Gu.Phase4.Fermions.Tests/
tests/Gu.Phase4.Dirac.Tests/
tests/Gu.Phase4.Chirality.Tests/
tests/Gu.Phase4.Couplings.Tests/
tests/Gu.Phase4.FamilyClustering.Tests/
tests/Gu.Phase4.Registry.Tests/
tests/Gu.Phase4.Observation.Tests/
tests/Gu.Phase4.Comparison.Tests/
tests/Gu.Phase4.Reporting.Tests/
```

### 1.3 Additional Deliverables

```
tests/Gu.TheoryConformance.Tests/     -- P4-C2 conformance harness
studies/bosonic-validation-001/        -- P4-C3 nontrivial bosonic study
schemas/phase4/                        -- 10 new JSON schemas
```

### 1.4 Solution File Additions

Every new src/ and tests/ project must be added to `GeometricUnity.slnx`
under the appropriate `/src/` or `/tests/` folder.

### 1.5 csproj Pattern

All projects inherit from `Directory.Build.props` (net10.0, nullable, latest).
Each csproj uses this minimal pattern:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <InternalsVisibleTo Include="Gu.Phase4.{Name}.Tests" />
  </ItemGroup>
  <ItemGroup>
    <!-- ProjectReferences here -->
  </ItemGroup>
</Project>
```

Test projects add:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="*" />
  <PackageReference Include="xunit" Version="*" />
  <PackageReference Include="xunit.runner.visualstudio" Version="*" />
</ItemGroup>
```

---

## 2. Dependency Graph

### 2.1 Phase IV Internal Dependencies

```
Gu.Phase4.Spin           -- depends on: Gu.Core, Gu.Math
    |
    v
Gu.Phase4.Fermions       -- depends on: Gu.Phase4.Spin, Gu.Core, Gu.Geometry
    |
    v
Gu.Phase4.Dirac          -- depends on: Gu.Phase4.Fermions, Gu.Phase4.Spin,
    |                        Gu.Core, Gu.Math, Gu.Geometry, Gu.ReferenceCpu,
    |                        Gu.Phase3.Backgrounds, Gu.Phase3.GaugeReduction,
    |                        Gu.Solvers
    v
Gu.Phase4.Chirality      -- depends on: Gu.Phase4.Spin, Gu.Phase4.Fermions,
    |                        Gu.Phase4.Dirac, Gu.Core
    v
Gu.Phase4.Couplings      -- depends on: Gu.Phase4.Dirac, Gu.Phase4.Chirality,
    |                        Gu.Phase4.Fermions, Gu.Phase3.Spectra,
    |                        Gu.Phase3.Properties, Gu.Core
    v
Gu.Phase4.FamilyClustering -- depends on: Gu.Phase4.Couplings, Gu.Phase4.Chirality,
    |                          Gu.Phase4.Dirac, Gu.Phase3.ModeTracking, Gu.Core
    v
Gu.Phase4.Registry       -- depends on: Gu.Phase4.FamilyClustering,
    |                        Gu.Phase4.Couplings, Gu.Phase3.Registry, Gu.Core
    v
Gu.Phase4.Observation    -- depends on: Gu.Phase4.Fermions, Gu.Phase4.Chirality,
    |                        Gu.Observation, Gu.Core
    v
Gu.Phase4.Comparison     -- depends on: Gu.Phase4.Observation, Gu.Phase4.Registry,
    |                        Gu.Phase3.Campaigns, Gu.ExternalComparison, Gu.Core
    v
Gu.Phase4.Reporting      -- depends on: Gu.Phase4.Registry, Gu.Phase4.Comparison,
                             Gu.Phase4.FamilyClustering, Gu.Phase3.Reporting, Gu.Core
```

### 2.2 Key Cross-Phase Dependencies

Phase IV **consumes** these Phase III types (never reimplements):

| Phase III Type | Namespace | Used By |
|---|---|---|
| `BackgroundRecord` | `Gu.Phase3.Backgrounds` | Dirac (background selection) |
| `BackgroundAtlas` | `Gu.Phase3.Backgrounds` | Dirac (atlas queries) |
| `ModeRecord` | `Gu.Phase3.Spectra` | Couplings (bosonic modes) |
| `SpectrumBundle` | `Gu.Phase3.Spectra` | Couplings (bosonic spectra) |
| `OperatorBundleBuilder` | `Gu.Phase3.Spectra` | Dirac (operator pattern) |
| `LanczosSolver` | `Gu.Phase3.Spectra` | Dirac (fermionic eigensolver) |
| `GaugeProjector` | `Gu.Phase3.GaugeReduction` | Dirac (gauge reduction) |
| `GaugeLeakDiagnostics` | `Gu.Phase3.GaugeReduction` | Chirality (leak checks) |
| `ModeFamilyRecord` | `Gu.Phase3.ModeTracking` | FamilyClustering (bosonic families) |
| `ModeMatchingEngine` | `Gu.Phase3.ModeTracking` | FamilyClustering (pattern reuse) |
| `HungarianAlgorithm` | `Gu.Phase3.ModeTracking` | FamilyClustering (optimal matching) |
| `CandidateBosonRecord` | `Gu.Phase3.Registry` | Registry (boson merge), Couplings |
| `BosonRegistry` | `Gu.Phase3.Registry` | Registry (boson source) |
| `BosonClaimClass` | `Gu.Phase3.Registry` | Registry (claim pattern) |
| `DemotionEngine` | `Gu.Phase3.Registry` | Registry (demotion pattern) |
| `ObservedModeSignature` | `Gu.Phase3.Observables` | Observation (pattern reuse) |

Phase IV **consumes** these Phase I/Core types:

| Core Type | Used By |
|---|---|
| `BranchManifest` | All (branch-aware everything) |
| `FieldTensor` | Fermions, Dirac (state storage) |
| `TensorSignature` | Fermions (spinor metadata) |
| `ProvenanceMeta` | All (provenance on every record) |
| `ObservationPipeline` | Observation (Y->X descent) |
| `GeometryContext` | Dirac, Fermions (mesh access) |

### 2.3 Dependency Rule: No Circular References

Phase IV projects MUST NOT reference each other in cycles.
The dependency chain is strictly: Spin -> Fermions -> Dirac -> Chirality -> Couplings -> FamilyClustering -> Registry.
Observation and Comparison are side branches off Fermions/Chirality/Registry.
Reporting depends on Registry + Comparison + FamilyClustering.

---

## 3. Core Type Definitions

All types are **sealed classes** with `required` init properties and
`[JsonPropertyName]` attributes for STJ serialization. NO records.

### 3.1 Gu.Phase4.Spin Types

```csharp
// SpinorRepresentationSpec.cs
namespace Gu.Phase4.Spin;

public sealed class SpinorRepresentationSpec
{
    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("spacetimeDimension")]
    public required int SpacetimeDimension { get; init; }

    [JsonPropertyName("cliffordSignature")]
    public required CliffordSignature CliffordSignature { get; init; }

    [JsonPropertyName("gammaBasisConventionId")]
    public required string GammaBasisConventionId { get; init; }

    [JsonPropertyName("chiralityConventionId")]
    public required string ChiralityConventionId { get; init; }

    [JsonPropertyName("conjugationConventionId")]
    public required string ConjugationConventionId { get; init; }

    [JsonPropertyName("innerProductConventionId")]
    public required string InnerProductConventionId { get; init; }

    [JsonPropertyName("numericField")]
    public required string NumericField { get; init; }  // "real64" | "complex64"

    [JsonPropertyName("spinorComponents")]
    public required int SpinorComponents { get; init; }

    [JsonPropertyName("chiralitySplit")]
    public required int ChiralitySplit { get; init; }

    [JsonPropertyName("insertedAssumptionIds")]
    public required IReadOnlyList<string> InsertedAssumptionIds { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// CliffordSignature.cs
public sealed class CliffordSignature
{
    [JsonPropertyName("positive")]
    public required int Positive { get; init; }

    [JsonPropertyName("negative")]
    public required int Negative { get; init; }

    [JsonIgnore]
    public int Dimension => Positive + Negative;
}

// GammaConventionSpec.cs
public sealed class GammaConventionSpec
{
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    [JsonPropertyName("signature")]
    public required CliffordSignature Signature { get; init; }

    [JsonPropertyName("representation")]
    public required string Representation { get; init; }  // "standard" | "chiral" | "majorana"

    [JsonPropertyName("anticommutationTolerance")]
    public double AnticommutationTolerance { get; init; } = 1e-12;
}

// ChiralityConventionSpec.cs
public sealed class ChiralityConventionSpec
{
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    [JsonPropertyName("signConvention")]
    public required string SignConvention { get; init; }  // "west-coast" | "east-coast"

    [JsonPropertyName("phaseFactor")]
    public required string PhaseFactor { get; init; }  // how i^k is chosen
}

// ConjugationConventionSpec.cs
public sealed class ConjugationConventionSpec
{
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    [JsonPropertyName("conjugationType")]
    public required string ConjugationType { get; init; }  // "dirac-adjoint" | "charge-conjugation" | "majorana"

    [JsonPropertyName("chargeConjugationMatrix")]
    public string? ChargeConjugationMatrixId { get; init; }
}

// CliffordValidationResult.cs
public sealed class CliffordValidationResult
{
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    [JsonPropertyName("anticommutationMaxError")]
    public required double AnticommutationMaxError { get; init; }

    [JsonPropertyName("chiralitySquareError")]
    public required double ChiralitySquareError { get; init; }

    [JsonPropertyName("conjugationConsistencyError")]
    public required double ConjugationConsistencyError { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("diagnosticNotes")]
    public IReadOnlyList<string> DiagnosticNotes { get; init; } = Array.Empty<string>();
}
```

**Key implementation classes in Gu.Phase4.Spin:**

```csharp
// GammaMatrixBuilder.cs -- constructs gamma matrices for given signature
public sealed class GammaMatrixBuilder
{
    // Build gamma matrices via tensor product of Pauli matrices
    // Input: CliffordSignature
    // Output: double[,][] (array of dim x dim matrices, one per direction)
    public GammaOperatorBundle Build(CliffordSignature signature, GammaConventionSpec convention);
}

// GammaOperatorBundle.cs -- precomputed gamma action operators
public sealed class GammaOperatorBundle
{
    public required string ConventionId { get; init; }
    public required CliffordSignature Signature { get; init; }
    public required int SpinorDimension { get; init; }
    public required double[][,] GammaMatrices { get; init; }  // [direction][row, col]
    public double[,]? ChiralityMatrix { get; init; }
    public CliffordValidationResult? ValidationResult { get; init; }
    public required ProvenanceMeta Provenance { get; init; }
}

// CliffordAlgebraValidator.cs
public sealed class CliffordAlgebraValidator
{
    // Validates: {Gamma_a, Gamma_b} = 2 * eta_{ab} * I
    // Validates: Gamma_chi^2 = I (for even dimensions)
    // Validates: conjugation matrix properties
    public CliffordValidationResult Validate(GammaOperatorBundle bundle, CliffordSignature signature);
}
```

### 3.2 Gu.Phase4.Fermions Types

```csharp
// FermionFieldLayout.cs
namespace Gu.Phase4.Fermions;

public sealed class FermionFieldLayout
{
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    [JsonPropertyName("meshElementCount")]
    public required int MeshElementCount { get; init; }

    [JsonPropertyName("spinorComponentsPerElement")]
    public required int SpinorComponentsPerElement { get; init; }

    [JsonPropertyName("totalDof")]
    public required int TotalDof { get; init; }

    [JsonPropertyName("numericField")]
    public required string NumericField { get; init; }  // "real64" | "complex64"

    [JsonPropertyName("blockOffsets")]
    public required IReadOnlyList<int> BlockOffsets { get; init; }

    [JsonPropertyName("chiralityBlockInfo")]
    public ChiralityBlockInfo? ChiralityBlockInfo { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// ChiralityBlockInfo.cs
public sealed class ChiralityBlockInfo
{
    [JsonPropertyName("leftStartIndex")]
    public required int LeftStartIndex { get; init; }

    [JsonPropertyName("leftCount")]
    public required int LeftCount { get; init; }

    [JsonPropertyName("rightStartIndex")]
    public required int RightStartIndex { get; init; }

    [JsonPropertyName("rightCount")]
    public required int RightCount { get; init; }
}

// DiscreteFermionState.cs
public sealed class DiscreteFermionState
{
    [JsonPropertyName("stateId")]
    public required string StateId { get; init; }

    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    [JsonPropertyName("coefficients")]
    public required double[] Coefficients { get; init; }

    [JsonPropertyName("isComplex")]
    public required bool IsComplex { get; init; }

    // If IsComplex, Coefficients stores interleaved [re0, im0, re1, im1, ...]
    // TotalDof = Coefficients.Length / (IsComplex ? 2 : 1)

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// FermionBackgroundRecord.cs
public sealed class FermionBackgroundRecord
{
    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("bosonicBackgroundId")]
    public required string BosonicBackgroundId { get; init; }

    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    [JsonPropertyName("diracBundleRef")]
    public required string DiracBundleRef { get; init; }

    [JsonPropertyName("spinConnectionRef")]
    public required string SpinConnectionRef { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// FermionModeRecord.cs
public sealed class FermionModeRecord
{
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("eigenvalue")]
    public required double Eigenvalue { get; init; }

    [JsonPropertyName("eigenvalueImaginary")]
    public double EigenvalueImaginary { get; init; }  // nonzero for non-Hermitian D

    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("modeVector")]
    public required double[] ModeVector { get; init; }

    [JsonPropertyName("modeIndex")]
    public required int ModeIndex { get; init; }

    [JsonPropertyName("chiralityDecomposition")]
    public ChiralityDecomposition? ChiralityDecomposition { get; init; }

    [JsonPropertyName("conjugationPairId")]
    public string? ConjugationPairId { get; init; }

    [JsonPropertyName("gaugeLeakScore")]
    public required double GaugeLeakScore { get; init; }

    [JsonPropertyName("observedSignatureRef")]
    public string? ObservedSignatureRef { get; init; }

    [JsonPropertyName("modeVectorArtifactRef")]
    public string? ModeVectorArtifactRef { get; init; }
}
```

### 3.3 Gu.Phase4.Dirac Types

```csharp
// SpinConnectionBundle.cs
namespace Gu.Phase4.Dirac;

public sealed class SpinConnectionBundle
{
    [JsonPropertyName("connectionId")]
    public required string ConnectionId { get; init; }

    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    /// <summary>Levi-Civita spin connection from Y_h metric geometry.</summary>
    [JsonPropertyName("leviCivitaCoefficients")]
    public required double[] LeviCivitaCoefficients { get; init; }

    /// <summary>Gauge coupling coefficients from bosonic omega via rho(T_a).</summary>
    [JsonPropertyName("gaugeCouplingCoefficients")]
    public required double[] GaugeCouplingCoefficients { get; init; }

    /// <summary>Gauge representation used (e.g., "adjoint", "fundamental").</summary>
    [JsonPropertyName("gaugeRepresentationId")]
    public required string GaugeRepresentationId { get; init; }

    [JsonPropertyName("assemblyMethod")]
    public required string AssemblyMethod { get; init; }

    /// <summary>Whether the Levi-Civita part is zero (flat assumption P4-IA-003).</summary>
    [JsonPropertyName("flatLeviCivitaAssumption")]
    public bool FlatLeviCivitaAssumption { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// DiracOperatorBundle.cs
public sealed class DiracOperatorBundle
{
    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    [JsonPropertyName("matrixShape")]
    public required int[] MatrixShape { get; init; }  // [rows, cols]

    [JsonPropertyName("hasExplicitMatrix")]
    public required bool HasExplicitMatrix { get; init; }

    [JsonPropertyName("explicitMatrixRef")]
    public string? ExplicitMatrixRef { get; init; }

    [JsonPropertyName("isHermitian")]
    public required bool IsHermitian { get; init; }

    [JsonPropertyName("massBranchTermIncluded")]
    public required bool MassBranchTermIncluded { get; init; }

    [JsonPropertyName("correctionTermIncluded")]
    public required bool CorrectionTermIncluded { get; init; }

    [JsonPropertyName("gaugeReductionApplied")]
    public required bool GaugeReductionApplied { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key implementation classes in Gu.Phase4.Dirac:**

```csharp
// ISpinConnectionBuilder.cs
public interface ISpinConnectionBuilder
{
    SpinConnectionBundle Build(
        BackgroundRecord background,
        SpinorRepresentationSpec spinorSpec,
        GeometryContext geometry,
        BranchManifest manifest);
}

// CpuSpinConnectionBuilder.cs -- CPU reference implementation
public sealed class CpuSpinConnectionBuilder : ISpinConnectionBuilder { ... }

// IDiracOperatorAssembler.cs
public interface IDiracOperatorAssembler
{
    // NOTE: Takes SimplicialMesh (actual topology/geometry data) instead of
    // GeometryContext (serialization metadata). All branch-level parameters
    // are carried by SpinConnectionBundle and GammaOperatorBundle.
    // ProvenanceMeta is passed explicitly for the output bundle.
    DiracOperatorBundle Assemble(
        SpinConnectionBundle connection,
        GammaOperatorBundle gammas,
        FermionFieldLayout layout,
        SimplicialMesh mesh,
        ProvenanceMeta provenance);

    // Matrix-free apply: D * psi
    double[] Apply(DiracOperatorBundle bundle, double[] psi);
}

// CpuDiracOperatorAssembler.cs -- CPU reference implementation
public sealed class CpuDiracOperatorAssembler : IDiracOperatorAssembler { ... }

// FermionSpectralSolver.cs
public sealed class FermionSpectralSolver
{
    // Solves generalized eigenproblem: D_h phi = lambda M_psi phi
    // Uses LanczosSolver from Phase III for Hermitian case
    // Uses Arnoldi for non-Hermitian case
    public FermionSpectralResult Solve(
        DiracOperatorBundle diracBundle,
        FermionFieldLayout layout,
        FermionSpectralConfig config);
}

// FermionSpectralResult.cs
public sealed class FermionSpectralResult
{
    public required string ResultId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required IReadOnlyList<FermionModeRecord> Modes { get; init; }
    public required FermionSpectralDiagnostics Diagnostics { get; init; }
    public required ProvenanceMeta Provenance { get; init; }
}

// FermionSpectralConfig.cs
public sealed class FermionSpectralConfig
{
    public required string TargetRegion { get; init; }  // "lowest-magnitude" | "near-zero" | "lowest-positive"
    public required int ModeCount { get; init; }
    public bool GaugeReduction { get; init; } = true;
    public bool NullspaceDeflation { get; init; } = true;
    public double ConvergenceTolerance { get; init; } = 1e-10;
    public int MaxIterations { get; init; } = 1000;
}
```

### 3.4 Gu.Phase4.Chirality Types

```csharp
// ChiralityDecomposition.cs
namespace Gu.Phase4.Chirality;

public sealed class ChiralityDecomposition
{
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("leftFraction")]
    public required double LeftFraction { get; init; }

    [JsonPropertyName("rightFraction")]
    public required double RightFraction { get; init; }

    [JsonPropertyName("mixedFraction")]
    public required double MixedFraction { get; init; }

    [JsonPropertyName("chiralityTag")]
    public required string ChiralityTag { get; init; }  // "left" | "right" | "mixed"

    [JsonPropertyName("leakageDiagnostic")]
    public required double LeakageDiagnostic { get; init; }
}

// ConjugationPairRecord.cs
public sealed class ConjugationPairRecord
{
    [JsonPropertyName("pairId")]
    public required string PairId { get; init; }

    [JsonPropertyName("modeIdA")]
    public required string ModeIdA { get; init; }

    [JsonPropertyName("modeIdB")]
    public required string ModeIdB { get; init; }

    [JsonPropertyName("overlapScore")]
    public required double OverlapScore { get; init; }

    [JsonPropertyName("conjugationType")]
    public required string ConjugationType { get; init; }

    [JsonPropertyName("isConfident")]
    public required bool IsConfident { get; init; }
}
```

**Key implementation classes:**

```csharp
// ChiralityAnalyzer.cs
public sealed class ChiralityAnalyzer
{
    // Project mode onto left/right subspaces using convention-parameterized projectors:
    // P_L = (I + s*Gamma_chi)/2, P_R = (I - s*Gamma_chi)/2
    // where s = chiralityConvention.SignConvention (+1 or -1)
    public ChiralityDecomposition Analyze(
        FermionModeRecord mode,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        FermionFieldLayout layout);

    public IReadOnlyList<ChiralityDecomposition> AnalyzeAll(
        IReadOnlyList<FermionModeRecord> modes,
        GammaOperatorBundle gammas,
        FermionFieldLayout layout);
}

// ConjugationAnalyzer.cs
public sealed class ConjugationAnalyzer
{
    // Find conjugation pairs among fermion modes
    public IReadOnlyList<ConjugationPairRecord> FindPairs(
        IReadOnlyList<FermionModeRecord> modes,
        ConjugationConventionSpec convention,
        GammaOperatorBundle gammas,
        double overlapThreshold = 0.8);
}
```

### 3.5 Gu.Phase4.Couplings Types

```csharp
// DiracVariationBundle.cs
namespace Gu.Phase4.Couplings;

public sealed class DiracVariationBundle
{
    [JsonPropertyName("variationId")]
    public required string VariationId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("symmetryNotes")]
    public IReadOnlyList<string> SymmetryNotes { get; init; } = Array.Empty<string>();

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// BosonFermionCouplingRecord.cs
public sealed class BosonFermionCouplingRecord
{
    [JsonPropertyName("couplingId")]
    public required string CouplingId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    [JsonPropertyName("fermionModeIdI")]
    public required string FermionModeIdI { get; init; }

    [JsonPropertyName("fermionModeIdJ")]
    public required string FermionModeIdJ { get; init; }

    [JsonPropertyName("couplingProxyReal")]
    public required double CouplingProxyReal { get; init; }

    [JsonPropertyName("couplingProxyImaginary")]
    public double CouplingProxyImaginary { get; init; }

    [JsonPropertyName("couplingProxyMagnitude")]
    public required double CouplingProxyMagnitude { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("selectionRuleNotes")]
    public IReadOnlyList<string> SelectionRuleNotes { get; init; } = Array.Empty<string>();

    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// CouplingAtlas.cs
public sealed class CouplingAtlas
{
    [JsonPropertyName("atlasId")]
    public required string AtlasId { get; init; }

    [JsonPropertyName("couplings")]
    public required IReadOnlyList<BosonFermionCouplingRecord> Couplings { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("bosonRegistryVersion")]
    public required string BosonRegistryVersion { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key implementation class:**

```csharp
// CouplingProxyEngine.cs
public sealed class CouplingProxyEngine
{
    // Compute g_ijk = <phi_i_bar, delta_D[b_k] phi_j>
    // where delta_D[b_k] is the variation of D w.r.t. bosonic mode b_k
    public CouplingAtlas ComputeAll(
        FermionSpectralResult fermionResult,
        IReadOnlyList<ModeRecord> bosonModes,
        DiracOperatorBundle diracBundle,
        IDiracOperatorAssembler assembler,
        CouplingExtractionConfig config);
}
```

### 3.6 Gu.Phase4.FamilyClustering Types

```csharp
// FermionModeFamilyRecord.cs
namespace Gu.Phase4.FamilyClustering;

public sealed class FermionModeFamilyRecord
{
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    [JsonPropertyName("memberModeIds")]
    public required IReadOnlyList<string> MemberModeIds { get; init; }

    [JsonPropertyName("contextIds")]
    public required IReadOnlyList<string> ContextIds { get; init; }

    [JsonPropertyName("meanEigenvalue")]
    public required double MeanEigenvalue { get; init; }

    [JsonPropertyName("eigenvalueSpread")]
    public required double EigenvalueSpread { get; init; }

    [JsonPropertyName("dominantChirality")]
    public required string DominantChirality { get; init; }

    [JsonPropertyName("conjugationPairFamilyId")]
    public string? ConjugationPairFamilyId { get; init; }

    [JsonPropertyName("branchPersistenceScore")]
    public required double BranchPersistenceScore { get; init; }

    [JsonPropertyName("refinementPersistenceScore")]
    public required double RefinementPersistenceScore { get; init; }

    [JsonPropertyName("isStable")]
    public required bool IsStable { get; init; }

    [JsonPropertyName("ambiguityCount")]
    public int AmbiguityCount { get; init; }
}

// FamilyClusterRecord.cs
public sealed class FamilyClusterRecord
{
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    [JsonPropertyName("clusterLabel")]
    public required string ClusterLabel { get; init; }  // e.g., "FamilyLikeCluster-001"

    [JsonPropertyName("memberFamilyIds")]
    public required IReadOnlyList<string> MemberFamilyIds { get; init; }

    [JsonPropertyName("clusteringMethod")]
    public required string ClusteringMethod { get; init; }

    [JsonPropertyName("featureSummary")]
    public required FamilyFeatureSummary FeatureSummary { get; init; }

    [JsonPropertyName("ambiguityScore")]
    public required double AmbiguityScore { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

// FamilyFeatureSummary.cs
public sealed class FamilyFeatureSummary
{
    [JsonPropertyName("massLikeRange")]
    public required double[] MassLikeRange { get; init; }  // [min, max]

    [JsonPropertyName("chiralityProfile")]
    public required string ChiralityProfile { get; init; }

    [JsonPropertyName("conjugationPattern")]
    public required string ConjugationPattern { get; init; }

    [JsonPropertyName("couplingProfileSummary")]
    public string? CouplingProfileSummary { get; init; }
}
```

**Key implementation classes:**

```csharp
// FermionModeTracker.cs
public sealed class FermionModeTracker
{
    // Mirrors Phase III ModeMatchingEngine but adds chirality + conjugation invariants
    // Uses HungarianAlgorithm from Gu.Phase3.ModeTracking
    public IReadOnlyList<FermionModeFamilyRecord> TrackAcrossContexts(
        IReadOnlyList<FermionSpectralResult> results,
        FermionTrackingConfig config);
}

// FamilyClusteringEngine.cs
public sealed class FamilyClusteringEngine
{
    // 1. Rule-based grouping by conjugation + chirality
    // 2. Hierarchical clustering by mass-like + coupling profile
    // 3. Ambiguity scoring
    public IReadOnlyList<FamilyClusterRecord> Cluster(
        IReadOnlyList<FermionModeFamilyRecord> families,
        CouplingAtlas? couplingAtlas,
        FamilyClusteringConfig config);
}
```

### 3.7 Gu.Phase4.Registry Types

```csharp
// ParticleType.cs
namespace Gu.Phase4.Registry;

public enum ParticleType
{
    Boson,
    Fermion,
    InteractionCandidate,
    CompositeCandidate
}

// ParticleClaimClass.cs
public enum ParticleClaimClass
{
    C0_NumericalMode = 0,
    C1_LocalPersistentMode = 1,
    C2_BranchStableCandidate = 2,
    C3_ObservedStableCandidate = 3,
    C4_PhysicalAnalogyCandidate = 4,
    C5_StrongIdentificationCandidate = 5,
}

// UnifiedParticleRecord.cs
public sealed class UnifiedParticleRecord
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("particleType")]
    public required ParticleType ParticleType { get; init; }

    [JsonPropertyName("sourceRegistryId")]
    public required string SourceRegistryId { get; init; }  // boson or fermion registry

    [JsonPropertyName("backgroundIds")]
    public required IReadOnlyList<string> BackgroundIds { get; init; }

    [JsonPropertyName("branchVariantIds")]
    public IReadOnlyList<string> BranchVariantIds { get; init; } = Array.Empty<string>();

    [JsonPropertyName("massLikeEnvelope")]
    public required double[] MassLikeEnvelope { get; init; }  // [min, mean, max]

    [JsonPropertyName("chiralityData")]
    public ChiralityDecomposition? ChiralityData { get; init; }

    [JsonPropertyName("conjugationData")]
    public ConjugationPairRecord? ConjugationData { get; init; }

    [JsonPropertyName("symmetryData")]
    public SymmetryEnvelope? SymmetryData { get; init; }

    [JsonPropertyName("couplingData")]
    public IReadOnlyList<BosonFermionCouplingRecord>? CouplingData { get; init; }

    [JsonPropertyName("observationData")]
    public string? ObservationSummaryRef { get; init; }

    [JsonPropertyName("comparisonData")]
    public string? ComparisonSummaryRef { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    [JsonPropertyName("refinementStabilityScore")]
    public double RefinementStabilityScore { get; init; }

    [JsonPropertyName("observationStabilityScore")]
    public double ObservationStabilityScore { get; init; }

    [JsonPropertyName("claimClass")]
    public required ParticleClaimClass ClaimClass { get; init; }

    [JsonPropertyName("demotions")]
    public IReadOnlyList<ParticleDemotionRecord> Demotions { get; init; } = Array.Empty<ParticleDemotionRecord>();

    [JsonPropertyName("ambiguityNotes")]
    public IReadOnlyList<string> AmbiguityNotes { get; init; } = Array.Empty<string>();

    [JsonPropertyName("familyClusterId")]
    public string? FamilyClusterId { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }
}

// ParticleDemotionRecord.cs
public sealed class ParticleDemotionRecord
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("reason")]
    public required string Reason { get; init; }

    [JsonPropertyName("previousClaimClass")]
    public required ParticleClaimClass PreviousClaimClass { get; init; }

    [JsonPropertyName("demotedClaimClass")]
    public required ParticleClaimClass DemotedClaimClass { get; init; }

    [JsonPropertyName("details")]
    public required string Details { get; init; }

    [JsonPropertyName("triggerValue")]
    public double? TriggerValue { get; init; }

    [JsonPropertyName("threshold")]
    public double? Threshold { get; init; }
}

// UnifiedParticleRegistry.cs
public sealed class UnifiedParticleRegistry
{
    [JsonPropertyName("registryVersion")]
    public string RegistryVersion { get; init; } = "1.0.0";

    [JsonPropertyName("candidates")]
    public List<UnifiedParticleRecord> Candidates { get; init; } = new();

    [JsonPropertyName("familyClusters")]
    public List<FamilyClusterRecord> FamilyClusters { get; init; } = new();

    [JsonPropertyName("bosonSourceVersion")]
    public string? BosonSourceVersion { get; init; }

    [JsonPropertyName("fermionSourceVersion")]
    public string? FermionSourceVersion { get; init; }

    public string ToJson(bool indented = true);
    public static UnifiedParticleRegistry FromJson(string json);
}
```

**Key implementation classes:**

```csharp
// UnifiedRegistryBuilder.cs
public sealed class UnifiedRegistryBuilder
{
    // Merges boson registry + fermion families + coupling atlas + observations
    public UnifiedParticleRegistry Build(
        BosonRegistry bosonRegistry,
        IReadOnlyList<FermionModeFamilyRecord> fermionFamilies,
        CouplingAtlas? couplingAtlas,
        IReadOnlyList<FamilyClusterRecord>? clusters,
        UnifiedRegistryConfig config);
}

// UnifiedDemotionEngine.cs
public sealed class UnifiedDemotionEngine
{
    // Extends Phase III DemotionEngine pattern with fermion-specific rules:
    // - ChiralityLeakage: demote if chirality is unstable across branches
    // - ConjugationInconsistency: demote if conjugation pairs break
    // - CouplingFragility: demote if coupling proxy is branch-fragile
    // Plus all 7 rules from BosonDemotionEngine
    public UnifiedParticleRecord ApplyDemotions(UnifiedParticleRecord candidate);
}
```

### 3.8 Gu.Phase4.Observation Types

```csharp
// FermionObservationSummary.cs
namespace Gu.Phase4.Observation;

public sealed class FermionObservationSummary
{
    [JsonPropertyName("summaryId")]
    public required string SummaryId { get; init; }

    [JsonPropertyName("fermionModeId")]
    public required string FermionModeId { get; init; }

    /// X-chirality (base, physically observed 4D chirality).
    /// This is the canonical chirality for comparison to external targets.
    [JsonPropertyName("xChirality")]
    public required ChiralityDecomposition XChirality { get; init; }

    /// Y-chirality (full ambient space). Diagnostic only — not directly
    /// observable but needed for consistency checks and decomposition validation.
    /// Null if dimY is odd (Y-chirality undefined).
    [JsonPropertyName("yChirality")]
    public ChiralityDecomposition? YChirality { get; init; }

    [JsonPropertyName("observedMassLikeScale")]
    public required double ObservedMassLikeScale { get; init; }

    [JsonPropertyName("observedSignatureRef")]
    public required string ObservedSignatureRef { get; init; }

    [JsonPropertyName("observationPipelineId")]
    public required string ObservationPipelineId { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

**Key implementation class:**

```csharp
// FermionObservationPipeline.cs
public sealed class FermionObservationPipeline
{
    // Extends Phase I ObservationPipeline for fermionic modes on Y_h -> X_h
    // RULE: No raw Y_h spinor data may be compared to external targets
    public FermionObservationSummary Observe(
        FermionModeRecord mode,
        FermionFieldLayout layout,
        ObservationPipeline basePipeline);
}
```

### 3.9 Gu.Phase4.Comparison Types

```csharp
// FermionComparisonResult.cs
namespace Gu.Phase4.Comparison;

public sealed class FermionComparisonResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("targetId")]
    public required string TargetId { get; init; }

    [JsonPropertyName("outcome")]
    public required string Outcome { get; init; }  // Compatible | Incompatible | Underdetermined | InsufficientEvidence

    [JsonPropertyName("matchScore")]
    public required double MatchScore { get; init; }

    [JsonPropertyName("uncertaintyNotes")]
    public IReadOnlyList<string> UncertaintyNotes { get; init; } = Array.Empty<string>();

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
```

### 3.10 Gu.Phase4.Reporting Types

```csharp
// Phase4Report.cs
namespace Gu.Phase4.Reporting;

public sealed class Phase4Report
{
    public required string ReportId { get; init; }
    public required string StudyId { get; init; }
    public required FermionAtlasSummary FermionAtlas { get; init; }
    public required CouplingAtlasSummary CouplingAtlas { get; init; }
    public required UnifiedRegistrySummary RegistrySummary { get; init; }
    public required IReadOnlyList<string> NegativeResults { get; init; }
    public required ProvenanceMeta Provenance { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
}

// --- Fermion Atlas Summary (matches Phase III BosonAtlasReport pattern) ---

public sealed class FermionAtlasSummary
{
    public required string SummaryId { get; init; }
    public required IReadOnlyList<FermionFamilySheet> FamilySheets { get; init; }
    public required IReadOnlyList<ChiralitySummaryEntry> ChiralitySummaries { get; init; }
    public required IReadOnlyList<ConjugationSummaryEntry> ConjugationSummaries { get; init; }
    public required int TotalFamilies { get; init; }
    public required int StableFamilies { get; init; }
    public required int AmbiguousFamilies { get; init; }
}

public sealed class FermionFamilySheet
{
    public required string FamilyId { get; init; }
    public required double MeanEigenvalue { get; init; }
    public required double EigenvalueSpread { get; init; }
    public required int MemberCount { get; init; }
    public required bool IsStable { get; init; }
    public required string ClaimClass { get; init; }
    public required IReadOnlyList<string> MemberModeIds { get; init; }
}

public sealed class ChiralitySummaryEntry
{
    public required string FamilyId { get; init; }
    public required double LeftProjection { get; init; }
    public required double RightProjection { get; init; }
    public required double LeakageNorm { get; init; }
    public required string ChiralityType { get; init; } // "Y", "X", "F"
    /// "definite-left", "definite-right", "mixed", "trivial" (odd dimY), "not-applicable"
    public required string ChiralityStatus { get; init; }
    public IReadOnlyList<string>? DiagnosticNotes { get; init; }
}

public sealed class ConjugationSummaryEntry
{
    public required string FamilyId { get; init; }
    public required bool HasConjugatePair { get; init; }
    public required string? PairedFamilyId { get; init; }
    public required double PairingScore { get; init; }
}

// --- Coupling Atlas Summary ---

public sealed class CouplingAtlasSummary
{
    public required string SummaryId { get; init; }
    public required IReadOnlyList<CouplingMatrixSummary> CouplingMatrices { get; init; }
    public required int TotalCouplings { get; init; }
    public required int NonzeroCouplings { get; init; }
    public required double MaxCouplingMagnitude { get; init; }
}

public sealed class CouplingMatrixSummary
{
    public required string BosonModeId { get; init; }
    public required int FermionPairCount { get; init; }
    public required double MaxEntry { get; init; }
    public required double FrobeniusNorm { get; init; }
}

// --- Unified Registry Summary ---

public sealed class UnifiedRegistrySummary
{
    public required string SummaryId { get; init; }
    public required int TotalBosons { get; init; }
    public required int TotalFermions { get; init; }
    public required int TotalInteractions { get; init; }
    public required IReadOnlyDictionary<string, int> ClaimClassCounts { get; init; }
    public required IReadOnlyList<CandidateParticleSummary> TopCandidates { get; init; }
}

public sealed class CandidateParticleSummary
{
    public required string CandidateId { get; init; }
    public required string ParticleType { get; init; } // "boson", "fermion"
    public required string ClaimClass { get; init; }
    public required double MassLikeValue { get; init; }
    public required int DemotionCount { get; init; }
}
```

---

## 4. Data Flow

### 4.1 End-to-End Pipeline

```
Phase III BackgroundAtlas
    |
    | (select backgrounds with ReplayTier >= R2, AdmissibilityLevel >= B1)
    v
SpinorRepresentationSpec + GammaOperatorBundle  (M33)
    |
    v
FermionFieldLayout  (M34)
    |
    v
SpinConnectionBundle  (M35: from bosonic background + geometry)
    |
    v
DiracOperatorBundle  (M36: gamma + connection + layout -> D_h)
    |
    +---> GaugeReduction (reuse Phase III GaugeProjector)
    |
    v
FermionSpectralResult  (M38: eigensolve D_h phi = lambda M phi)
    |
    +---> ChiralityDecomposition per mode  (M37)
    +---> ConjugationPairRecord per pair   (M37)
    |
    v
FermionModeFamilyRecord  (M39: track across branches/refinement)
    |
    +---> CouplingAtlas  (M40: g_ijk = <phi_i_bar, dD[b_k] phi_j>)
    |
    v
FamilyClusterRecord  (M41: generation-like grouping)
    |
    v
UnifiedParticleRegistry  (M42: merge bosons + fermions + couplings)
    |
    +---> FermionObservationSummary  (M43: Y_h -> X_h descent)
    +---> FermionComparisonResult   (M43: vs external targets)
    |
    v
Phase4Report  (Reporting: atlases + dashboards)
```

### 4.2 Artifact Directory Layout

```
run/
  fermions/
    spinor_spec.json                     -- SpinorRepresentationSpec
    gamma_bundle.json                    -- GammaOperatorBundle metadata
    fermion_layout.json                  -- FermionFieldLayout
    spin_connection/
      conn-{backgroundId}.json           -- SpinConnectionBundle
    dirac_operator/
      dirac-{backgroundId}.json          -- DiracOperatorBundle
      dirac-{backgroundId}.matrix.bin    -- explicit matrix (if small)
    fermion_modes/
      modes-{backgroundId}.json          -- FermionSpectralResult
      mode_vectors/                      -- binary mode vectors
    chirality/
      chirality-{backgroundId}.json      -- ChiralityDecomposition[]
      conjugation-{backgroundId}.json    -- ConjugationPairRecord[]
    couplings/
      coupling_atlas-{backgroundId}.json -- CouplingAtlas
  particle_registry/
    fermion_families.json                -- FermionModeFamilyRecord[]
    family_clusters.json                 -- FamilyClusterRecord[]
    unified_particle_registry.json       -- UnifiedParticleRegistry
  observations/
    fermion_observations.json            -- FermionObservationSummary[]
  comparisons/
    fermion_comparisons.json             -- FermionComparisonResult[]
  reports/
    fermion_family_atlas.md
    coupling_atlas.md
    unified_particle_report.md
    phase4_report.md
```

---

## 5. Interface Contracts Between Modules

### 5.1 Spin -> Fermions

Fermions receives `SpinorRepresentationSpec` and `GammaOperatorBundle` from Spin.
These determine the per-element spinor block size and the gamma action matrices.

### 5.2 Fermions -> Dirac

Dirac receives `FermionFieldLayout` which defines the global DOF count,
block offsets, and chirality block structure. Dirac also consumes
`DiscreteFermionState` for apply operations.

### 5.3 Phase III -> Dirac

Dirac consumes `BackgroundRecord` (the solved bosonic background state)
and `GeometryContext` to build the spin connection. It also uses
`GaugeProjector` from `Gu.Phase3.GaugeReduction` for gauge reduction.

### 5.4 Dirac -> Chirality

Chirality receives `FermionModeRecord` entries from the spectral solver
and the `GammaOperatorBundle` (for the chirality matrix) from Spin.

### 5.5 Dirac + Chirality -> Couplings

Couplings receives `DiracOperatorBundle` + `IDiracOperatorAssembler`
(to compute variations), `FermionSpectralResult` (fermion modes),
and `ModeRecord` from Phase III (bosonic modes).

### 5.6 FamilyClustering -> Registry

Registry receives `FermionModeFamilyRecord[]`, `FamilyClusterRecord[]`,
and `CouplingAtlas` from FamilyClustering/Couplings, plus `BosonRegistry`
from Phase III.

### 5.7 Registry -> Observation -> Comparison

Observation takes `FermionModeRecord` + `FermionFieldLayout` and produces
`FermionObservationSummary` via the existing `ObservationPipeline`.
Comparison takes `UnifiedParticleRecord` + observation summaries and
produces `FermionComparisonResult`.

---

## 6. Prerequisite Specs (P4-C1, P4-C2, P4-C3)

These MUST complete before M33 begins.

### 6.1 P4-C1: Runtime Branch State Consumption

**Owner:** implementer-1

**What to change:**
1. `Gu.Cli/Program.cs` -- `RunSolver()`, `SolveCommand()`, `ComputeSpectrum()` methods
2. These currently reconstruct fresh toy defaults internally
3. Patch them to load and honor persisted BranchManifest + GeometryContext + BackgroundRecord

**Files to modify:**
- `apps/Gu.Cli/Program.cs` (3 command handlers)
- Possibly `src/Gu.Core/Factories/BranchManifestFactory.cs` (load-from-file method)

**New tests (in existing test projects or a new integration test file):**
- Two distinct stored backgrounds produce distinct operator bundles
- Replayed runs preserve loaded branch/background identity
- CLI no longer silently substitutes toy defaults

**Completion signal:** Two backgrounds with different omega produce different spectra.

### 6.2 P4-C2: Theory Conformance Harness

**Owner:** implementer-2

**New project:** `tests/Gu.TheoryConformance.Tests/`

**What to implement:**
1. Tests that confirm active runtime torsion/Shiab/pairing/observation/geometry branches match declared artifact provenance
2. Tests that detect silent fallback to zero-state execution
3. Tests that distinguish branch-local from theory-level validation
4. Machine-readable conformance artifact: `ConformanceResult` type

**New files:**
- `tests/Gu.TheoryConformance.Tests/Gu.TheoryConformance.Tests.csproj`
- `tests/Gu.TheoryConformance.Tests/BranchConformanceTests.cs`
- `tests/Gu.TheoryConformance.Tests/FallbackDetectionTests.cs`
- `tests/Gu.TheoryConformance.Tests/ConformanceArtifactTests.cs`
- `schemas/theory_conformance.schema.json`

**Key type:**
```csharp
public sealed class ConformanceResult
{
    public required string BranchManifestId { get; init; }
    public required IReadOnlyList<ConformanceCheck> Checks { get; init; }
    public required bool AllPassed { get; init; }
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class ConformanceCheck
{
    public required string CheckId { get; init; }
    public required string Category { get; init; }  // "branch-match" | "fallback-detection" | "level-distinction"
    public required bool Passed { get; init; }
    public required string Details { get; init; }
}
```

### 6.3 P4-C3: Nontrivial Bosonic Validation Study

**Owner:** implementer-3

**New directory:** `studies/bosonic-validation-001/`

**What to implement:**
1. Study config JSON (nonzero initial omega, nontrivial torsion/Shiab branch)
2. Scripted runner (`run-study.sh` or dotnet-based)
3. Integration tests ensuring the study produces nontrivial outputs
4. Markdown report template

**New files:**
- `studies/bosonic-validation-001/study_config.json`
- `studies/bosonic-validation-001/run-study.sh`
- `studies/bosonic-validation-001/README.md`
- `tests/Gu.Phase3.Backgrounds.Tests/BosonicValidationStudyTests.cs` (or dedicated test project)

**Completion signal:** Study produces nonzero intermediate fields and nontrivial artifact chain.

---

## 7. Per-Milestone Implementation Specs

### 7.1 M33: Spinor Convention and Validation Substrate

**Project:** `Gu.Phase4.Spin`

**Dependencies:** Gu.Core, Gu.Math

**Files to create:**
```
src/Gu.Phase4.Spin/
  Gu.Phase4.Spin.csproj
  SpinorRepresentationSpec.cs
  CliffordSignature.cs
  GammaConventionSpec.cs
  ChiralityConventionSpec.cs
  ConjugationConventionSpec.cs
  GammaMatrixBuilder.cs
  GammaOperatorBundle.cs
  CliffordAlgebraValidator.cs
  CliffordValidationResult.cs
tests/Gu.Phase4.Spin.Tests/
  Gu.Phase4.Spin.Tests.csproj
  GammaMatrixBuilderTests.cs
  CliffordAlgebraValidatorTests.cs
  SpinorRepresentationSpecTests.cs
```

**Test targets (minimum 15 tests):**
- Gamma anticommutation {Gamma_a, Gamma_b} = 2*eta_{ab}*I for signatures (1,3), (7,7), (0,14)
- Chirality matrix squares to identity
- Conjugation matrix properties
- Serialization round-trip for SpinorRepresentationSpec
- Convention reproducibility across instantiations

**CRITICAL: Gu.Math conflict.** Code in this project that needs `Math.Sqrt()` etc. must use `System.Math.Sqrt()` explicitly since `Gu.Math` namespace shadows `System.Math`.

**PHYSICS (RESOLVED):** Spinor dimension = 2^floor(n/2) where n = dimY.
- **Toy dimY=5 (Riemannian, Cl(5,0)):** 4x4 complex gamma matrices. Gamma_1..4 = standard 4D Dirac gammas, Gamma_5 = gamma_5 (4D chirality becomes regular gamma in 5D). NO chirality grading in odd dimensions.
- **Physical dimY=14 (Cl(14,0) default):** 128x128 complex gamma matrices. Has chirality grading (even dim).
- Signature is a branch parameter, not hardcoded. Start with Riemannian (all positive).
- Test with dim=2 (1x1 trivial), dim=4 (4x4), and dim=5 (4x4 odd-dim no-chirality) cases.

### 7.2 M34: Fermionic Field Layout and Storage

**Project:** `Gu.Phase4.Fermions`

**Dependencies:** Gu.Phase4.Spin, Gu.Core, Gu.Geometry

**Files to create:**
```
src/Gu.Phase4.Fermions/
  Gu.Phase4.Fermions.csproj
  FermionFieldLayout.cs
  FermionFieldLayoutBuilder.cs
  ChiralityBlockInfo.cs
  DiscreteFermionState.cs
  DiscreteDualFermionState.cs
  FermionBackgroundRecord.cs
  FermionModeRecord.cs
  FermionStateSerializer.cs
tests/Gu.Phase4.Fermions.Tests/
  Gu.Phase4.Fermions.Tests.csproj
  FermionFieldLayoutBuilderTests.cs
  DiscreteFermionStateTests.cs
  FermionStateSerializerTests.cs
```

**Test targets (minimum 12 tests):**
- Layout builder produces correct DOF count (meshElements * spinorComponents)
- Block offsets are contiguous and cover total DOF
- Fermion state creation, serialization, round-trip
- Complex interleaved storage convention verified
- Layout compatible with both real and complex numeric fields

**Key design decision:** Complex spinor fields are stored as interleaved `double[]` with `[re0, im0, re1, im1, ...]` layout. The `IsComplex` flag on `DiscreteFermionState` controls interpretation. This matches the existing `FieldTensor.Coefficients` pattern (flat `double[]`).

### 7.3 M35: Spin Connection Builder

**Project:** `Gu.Phase4.Dirac`

**Dependencies:** Gu.Phase4.Fermions, Gu.Phase4.Spin, Gu.Core, Gu.Math, Gu.Geometry, Gu.ReferenceCpu, Gu.Phase3.Backgrounds

**Files to create (in Gu.Phase4.Dirac):**
```
src/Gu.Phase4.Dirac/
  Gu.Phase4.Dirac.csproj
  ISpinConnectionBuilder.cs
  CpuSpinConnectionBuilder.cs
  SpinConnectionBundle.cs
tests/Gu.Phase4.Dirac.Tests/
  Gu.Phase4.Dirac.Tests.csproj
  CpuSpinConnectionBuilderTests.cs
```

**Test targets (minimum 8 tests):**
- Connection builds from a toy 2D/4D bosonic background
- Connection coefficients are reproducible (deterministic)
- Provenance correctly references the source background
- Distinct backgrounds produce distinct connections

**PHYSICS (RESOLVED):** The spin connection Omega_spin derives from the **Levi-Civita connection of the metric on Y**, NOT from the gauge connection omega. The gauge connection omega is a SEPARATE object that enters D_h through M_branch/C_branch correction terms. CpuSpinConnectionBuilder approximates Omega_spin from discrete metric data on Y_h (edge lengths, cell geometry). See Section 11.4 for full details.

### 7.4 M36: Dirac Operator CPU Reference

**Project:** `Gu.Phase4.Dirac` (continuation)

**Additional files:**
```
src/Gu.Phase4.Dirac/
  IDiracOperatorAssembler.cs
  CpuDiracOperatorAssembler.cs
  DiracOperatorBundle.cs
  DiracOperatorValidator.cs
tests/Gu.Phase4.Dirac.Tests/
  CpuDiracOperatorAssemblerTests.cs
  DiracOperatorValidatorTests.cs
```

**Test targets (minimum 12 tests):**
- Dirac operator assembles for toy 2D (2-spinor) system
- Matrix-free apply matches explicit matrix apply
- Operator dimensions match layout (matrixShape = [totalDof, totalDof])
- Self-adjointness check (if Hermitian)
- Gauge reduction hooks integrate with Phase III GaugeProjector

**Key algorithm (D_h assembly from physicist guidance):**

D_h = Gamma^mu * (nabla^{LC}_{spin,mu} + omega_mu^a * rho(T_a)) + M_branch + C_branch

```
For each mesh edge (i,j) with direction mu:
  // Levi-Civita spin covariant derivative term
  D_h[i,j] += Gamma_h(mu) * leviCivitaCoeff[i,j,mu]
  // Gauge coupling term (omega acting on spinors via rho)
  D_h[i,j] += Gamma_h(mu) * gaugeCouplingCoeff[i,j,mu]
For each mesh element i:
  D_h[i,i] += M_branch[i]   // torsion-dependent mass-like corrections
  D_h[i,i] += C_branch[i]   // additional geometric couplings
```

**Hermiticity diagnostic (REQUIRED):** For Riemannian signature, D_h should be Hermitian. Compute `||D_h - D_h^dagger|| / ||D_h||` and store in DiracOperatorBundle diagnostics.

The implementation must support both:
1. Matrix-free apply: `Apply(bundle, psi)` computes `D_h * psi` without forming the full matrix
2. Explicit assembly: for small systems, build the full dense/sparse matrix

### 7.5 M37: Chirality and Conjugation Analysis

**Project:** `Gu.Phase4.Chirality`

**Dependencies:** Gu.Phase4.Spin, Gu.Phase4.Fermions, Gu.Phase4.Dirac, Gu.Core

**Files to create:**
```
src/Gu.Phase4.Chirality/
  Gu.Phase4.Chirality.csproj
  ChiralityDecomposition.cs
  ChiralityAnalyzer.cs
  ConjugationPairRecord.cs
  ConjugationAnalyzer.cs
  GaugeLeakSummary.cs
  FermionStabilityRecord.cs
tests/Gu.Phase4.Chirality.Tests/
  Gu.Phase4.Chirality.Tests.csproj
  ChiralityAnalyzerTests.cs
  ConjugationAnalyzerTests.cs
```

**Test targets (minimum 10 tests):**
- Pure left-chiral mode: leftFraction = 1.0, rightFraction = 0.0
- Pure right-chiral mode: rightFraction = 1.0
- Mixed mode: both fractions nonzero, sum to 1.0
- Conjugation pair detection: two modes related by charge conjugation
- Leakage diagnostic: chirality changes under branch variation

**Chirality projection formula (convention-parameterized):**
```
s = ChiralityConventionSpec.SignConvention  // +1 (west-coast) or -1 (east-coast)
P_L = (I + s * Gamma_chi) / 2
P_R = (I - s * Gamma_chi) / 2
leftFraction = ||P_L * phi||^2 / ||phi||^2
rightFraction = ||P_R * phi||^2 / ||phi||^2
```
**CRITICAL:** The sign `s` MUST come from `ChiralityConventionSpec.SignConvention`,
never hardcoded. The choice of which eigenvalue of `Gamma_chi` is called "left"
is purely conventional and varies across textbooks. Both `s=+1` and `s=-1` are
physically valid; the branch manifest declares which convention is in use.

### 7.6 M38: Fermionic Spectral Solver

**Project:** `Gu.Phase4.Dirac` (continuation)

**Additional files:**
```
src/Gu.Phase4.Dirac/
  FermionSpectralSolver.cs
  FermionSpectralResult.cs
  FermionSpectralConfig.cs
  FermionSpectralDiagnostics.cs
tests/Gu.Phase4.Dirac.Tests/
  FermionSpectralSolverTests.cs
```

**Test targets (minimum 10 tests):**
- Toy 4x4 Hermitian Dirac: eigenvalues match dense solve
- Mode residual norms below tolerance
- Null mode detection (eigenvalue near zero)
- Gauge reduction produces fewer physical modes
- Mode count matches requested count

**Key algorithm:**
1. Apply gauge reduction via `GaugeProjector` (if enabled)
2. For Hermitian D: use `LanczosSolver` from Phase III
3. For non-Hermitian D: implement Arnoldi iteration (new)
4. Solve `D_h phi = lambda M_psi phi` as generalized eigenproblem
5. Post-process: normalize, compute residuals, attach chirality decompositions

### 7.7 M39: Fermion Mode Tracking and Persistence Atlas

**Project:** `Gu.Phase4.FamilyClustering`

**Dependencies:** Gu.Phase4.Couplings, Gu.Phase4.Chirality, Gu.Phase4.Dirac, Gu.Phase3.ModeTracking, Gu.Core

**Files to create:**
```
src/Gu.Phase4.FamilyClustering/
  Gu.Phase4.FamilyClustering.csproj
  FermionModeTracker.cs
  FermionModeFamilyRecord.cs
  FermionModeFeatureVector.cs
  FermionTrackingConfig.cs
tests/Gu.Phase4.FamilyClustering.Tests/
  Gu.Phase4.FamilyClustering.Tests.csproj
  FermionModeTrackerTests.cs
```

**Test targets (minimum 10 tests):**
- Modes tracked across 2 branch variants
- Modes tracked across 2 refinement levels
- Chirality invariant preserved in matching
- Conjugation pairs tracked together
- Ambiguity recorded when matching is uncertain

**Key difference from Phase III mode tracking:** The matching score combines:
- eigenvalue band similarity (weight 0.3)
- eigenspace overlap (weight 0.2)
- chirality profile similarity (weight 0.2)
- conjugation relation consistency (weight 0.15)
- observed-signature similarity (weight 0.15)

Uses `HungarianAlgorithm` from `Gu.Phase3.ModeTracking` for optimal assignment.

### 7.8 M40: Boson-Fermion Coupling Proxy Engine

**Project:** `Gu.Phase4.Couplings`

**Dependencies:** Gu.Phase4.Dirac, Gu.Phase4.Chirality, Gu.Phase4.Fermions, Gu.Phase3.Spectra, Gu.Phase3.Properties, Gu.Core

**Files to create:**
```
src/Gu.Phase4.Couplings/
  Gu.Phase4.Couplings.csproj
  DiracVariationBundle.cs
  DiracVariationComputer.cs
  BosonFermionCouplingRecord.cs
  CouplingProxyEngine.cs
  CouplingAtlas.cs
  CouplingExtractionConfig.cs
tests/Gu.Phase4.Couplings.Tests/
  Gu.Phase4.Couplings.Tests.csproj
  CouplingProxyEngineTests.cs
  DiracVariationComputerTests.cs
```

**Test targets (minimum 10 tests):**
- Coupling proxy computes for a toy system
- Coupling is zero when fermion modes are orthogonal to variation
- Selection rules: coupling zero for symmetry-forbidden combinations
- Coupling atlas serialization round-trip
- Distinct boson modes produce distinct coupling patterns

**Key algorithm for delta_D[b_k] (from physicist guidance):**

The bosonic modes b_k live in CONNECTION SPACE (perturbations delta omega). The variation is:
```
dD_h/d_omega[b_k] = Gamma^mu * b_k_mu^a * rho(T_a) + dM_branch/d_omega[b_k]
```

The Levi-Civita part does NOT depend on omega, so it drops out of the variation. The first term (gauge-coupling variation) is the dominant coupling vertex.

**Two computation methods (implementer should support both):**
1. **Analytical (preferred):** Directly compute `Gamma^mu * b_k_mu^a * rho(T_a)` as a spinor operator. Exact, no epsilon needed.
2. **Finite difference (fallback):** `delta_D[b_k] * phi ≈ (D(z_* + epsilon*b_k) * phi - D(z_*) * phi) / epsilon`. Use for validation.

Then: `g_ijk = sum_cells c: conj(phi_i(c)) * (delta_D[b_k] * phi_j)(c) * vol(c)`

**IMPORTANT:** These are coupling **proxies**, not scattering amplitudes. Label them as such in all serialized output.

### 7.9 M41: Generation/Family Clustering

**Project:** `Gu.Phase4.FamilyClustering` (continuation)

**Additional files:**
```
src/Gu.Phase4.FamilyClustering/
  FamilyClusteringEngine.cs
  FamilyClusterRecord.cs
  FamilyFeatureSummary.cs
  FamilyClusteringConfig.cs
tests/Gu.Phase4.FamilyClustering.Tests/
  FamilyClusteringEngineTests.cs
```

**Test targets (minimum 8 tests):**
- Conjugation pairs grouped together
- Chirality-based pre-grouping
- Hierarchical clustering deterministic and reproducible
- Conservative labels (FamilyLikeCluster-NNN), never physical names
- Ambiguity score assigned to unstable clusters

**Clustering algorithm (3 stages):**
1. **Rule-based pre-grouping:** Group by conjugation pairs and chirality tag
2. **Hierarchical clustering:** Ward's method on feature vectors [mass-like, couplingMagnitude, chiralityFraction]
3. **Ambiguity scoring:** Silhouette-like score; clusters with score < 0.3 are flagged as ambiguous

### 7.10 M42: Unified Particle Registry

**Project:** `Gu.Phase4.Registry`

**Dependencies:** Gu.Phase4.FamilyClustering, Gu.Phase4.Couplings, Gu.Phase3.Registry, Gu.Core

**Files to create:**
```
src/Gu.Phase4.Registry/
  Gu.Phase4.Registry.csproj
  ParticleType.cs
  ParticleClaimClass.cs
  UnifiedParticleRecord.cs
  ParticleDemotionRecord.cs
  UnifiedParticleRegistry.cs
  UnifiedRegistryBuilder.cs
  UnifiedDemotionEngine.cs
  UnifiedRegistryConfig.cs
tests/Gu.Phase4.Registry.Tests/
  Gu.Phase4.Registry.Tests.csproj
  UnifiedRegistryBuilderTests.cs
  UnifiedDemotionEngineTests.cs
  UnifiedParticleRegistryTests.cs
```

**Test targets (minimum 12 tests):**
- Registry merges boson + fermion candidates correctly
- Interaction candidates created from coupling atlas
- Demotion engine applies all rules (gauge leak, chirality, coupling fragility, etc.)
- Serialization round-trip (JSON)
- Schema validation passes
- Empty inputs produce empty but valid registry

### 7.11 M43: Observation and Comparison Extension

**Projects:** `Gu.Phase4.Observation` + `Gu.Phase4.Comparison`

**Files to create:**
```
src/Gu.Phase4.Observation/
  Gu.Phase4.Observation.csproj
  FermionObservationPipeline.cs
  FermionObservationSummary.cs
  InteractionObservationSummary.cs
src/Gu.Phase4.Comparison/
  Gu.Phase4.Comparison.csproj
  FermionComparisonResult.cs
  FermionComparisonAdapter.cs
  FermionCampaignRunner.cs
tests/Gu.Phase4.Observation.Tests/
  ...
tests/Gu.Phase4.Comparison.Tests/
  ...
```

**Test targets (minimum 10 tests per project):**
- Fermion observation descends Y_h mode to X_h summary
- No raw Y_h data leaks into comparison
- Comparison produces Compatible/Incompatible/Underdetermined outcomes
- Campaign runner extends Phase III campaign pattern

### 7.12 M44: CUDA Acceleration and Parity Closure

This milestone extends the existing CUDA infrastructure (Gu.Interop, Gu.Phase2.CudaInterop, Gu.Phase3.CudaSpectra).

**Core interface (mirrors Phase III `ISpectralKernel` pattern):**

```csharp
namespace Gu.Phase4.Dirac;

public interface IDiracKernel
{
    /// Apply single gamma matrix: result = Gamma(mu) * spinor
    void ApplyGamma(int mu, ReadOnlySpan<double> spinor, Span<double> result);

    /// Apply full Dirac operator: result = D_h * spinor
    void ApplyDirac(ReadOnlySpan<double> spinor, Span<double> result);

    /// Apply mass matrix: result = M_psi * spinor (block-diagonal volume-weighted)
    void ApplyMass(ReadOnlySpan<double> spinor, Span<double> result);

    /// Apply chirality projector: result = P_L or P_R * spinor
    /// Throws InvalidOperationException for odd dimY (no chirality).
    void ApplyChiralityProjector(bool left, ReadOnlySpan<double> spinor, Span<double> result);

    /// Coupling proxy: g = <spinorI_bar, dD/d_omega[bosonK] * spinorJ>
    (double Real, double Imag) ComputeCouplingProxy(
        ReadOnlySpan<double> spinorI,
        ReadOnlySpan<double> spinorJ,
        ReadOnlySpan<double> bosonK);

    /// Spinor array length (interleaved complex: 2 * cellCount * spinorDim * dimG)
    int SpinorDimension { get; }
}
```

**What to add:**
1. `IDiracKernel` interface (in `Gu.Phase4.Dirac`)
2. `CpuDiracKernel` implementation (in `Gu.Phase4.Dirac`)
3. `GpuDiracKernel` implementation (behind `Gu.Interop` native call)
4. `DiracParityChecker` (mirrors `SpectralParityChecker` pattern exactly)
5. Parity tests for all 5 operations

**Spinor data format:** All spinor arrays use interleaved `double[]` with `[re0, im0, re1, im1, ...]`. The existing `SpectralParityChecker.Compare` works directly on `ReadOnlySpan<double>` element-wise -- no adaptation needed for complex data.

**Parity test matrix (both even and odd dimY):**

| Operation | dimY=4 (even) | dimY=5 (odd) |
|---|---|---|
| ApplyGamma | YES | YES |
| ApplyDirac | YES | YES |
| ApplyMass | YES | YES |
| ApplyChiralityProjector | YES | SKIP (no chirality) |
| ComputeCouplingProxy | YES | YES |

**REQUIRED:** All M44 parity tests MUST carry the `[Trait("Category", "CpuGpuParity")]` attribute so cuda-guy can filter them:

```csharp
[Trait("Category", "CpuGpuParity")]
public class DiracParityTests { ... }
```

**Pattern:** Follow the existing `Gu.Phase3.CudaSpectra` pattern:
- Backend interface (`IDiracKernel`) in the library project
- CUDA implementation behind `Gu.Interop` native call
- CPU fallback always available
- `DiracParityChecker.RunFullCheck(cpuKernel, gpuKernel)` in the test project

### 7.13 M45: End-to-End Reference Study

**Study name:** `Phase4-FermionFamily-Atlas-001`

**Delivers:**
- Study config under `studies/phase4-fermion-atlas-001/`
- Scripted runner
- Selects 3-5 Phase III backgrounds
- Runs full M33-M43 pipeline
- Generates `fermion_family_atlas.md`, `coupling_atlas.md`, `unified_particle_registry.json`, `phase4_report.md`
- Preserves negative results

---

## 8. JSON Schemas (for implementer-4 / SCHEMAS task)

Create under `schemas/phase4/`:

| Schema File | Validates |
|---|---|
| `spinor_representation.schema.json` | SpinorRepresentationSpec |
| `gamma_operator_bundle.schema.json` | GammaOperatorBundle metadata |
| `fermion_field_layout.schema.json` | FermionFieldLayout |
| `dirac_operator_bundle.schema.json` | DiracOperatorBundle |
| `fermion_mode.schema.json` | FermionModeRecord |
| `boson_fermion_coupling.schema.json` | BosonFermionCouplingRecord |
| `family_cluster.schema.json` | FamilyClusterRecord |
| `unified_particle_registry.schema.json` | UnifiedParticleRegistry |
| `phase4_report.schema.json` | Phase4Report |
| `theory_conformance.schema.json` | ConformanceResult |

All schemas follow the existing pattern in `schemas/` (JSON Schema draft-07).

---

## 9. CLI Command Extensions

Add to `apps/Gu.Cli/Program.cs`:

```csharp
case "build-spin-spec":        return BuildSpinSpec(args);
case "assemble-dirac":         return AssembleDirac(args);
case "solve-fermion-modes":    return SolveFermionModes(args);
case "analyze-chirality":      return AnalyzeChirality(args);
case "analyze-conjugation":    return AnalyzeConjugation(args);
case "extract-couplings":      return ExtractCouplings(args);
case "build-family-clusters":  return BuildFamilyClusters(args);
case "build-unified-registry": return BuildUnifiedRegistry(args);
case "report-phase4":          return ReportPhase4(args);
```

Each command follows the existing pattern: parse args, load artifacts, call engine, write output, return 0.

---

## 10. Critical Engineering Rules

1. **All types are `sealed class`** -- no records, no `with` syntax
2. **`System.Math.Sqrt()`** -- not `Math.Sqrt()` when `Gu.Math` is imported
3. **`List<T>` for STJ deserialization** -- not `IReadOnlyList<T>` (see GAP-1 from Phase III)
4. **CPU reference path first** -- no CUDA code until CPU passes all tests
5. **No Y_h physical comparison** -- everything goes through observation pipeline
6. **Conservative labels only** -- `FamilyLikeCluster-001`, never `electron`
7. **Coupling proxies are NOT amplitudes** -- label them as proxies
8. **Provenance on every record** -- background, branch, spinor convention, operator, observation, replay tier
9. **Preserve negative results** -- failed solves, unstable chirality, fragile couplings are first-class
10. **Never hide ambiguity** -- serialize it, don't choose silently
11. **Build/test: `dotnet build && dotnet test --no-build`** -- never just `dotnet test`
12. **Clean build if weird errors: `dotnet clean && dotnet build`**

---

## 11. Physics Decisions (Resolved by Physicist)

The following physics questions have been answered by the physicist. These are
**binding design decisions** -- implementers must follow them exactly.

### 11.1 Clifford Signature on Y

- Y carries a metric inherited from its construction as Met(X) (bundle of metrics over X).
- The signature on Y is NOT the same as the Lorentzian signature on X.
- Y carries a Riemannian or mixed-signature metric depending on the fiber metric structure.
- **The signature MUST be a branch parameter** (CliffordSignature in SpinorRepresentationSpec).
- **Toy case (dimY=5):** Cl(5,0) or Cl(4,1). Spinor dimension = 2^floor(5/2) = 4 complex.
- **Physical case (dimY=14):** Cl(14,0) or Cl(p,14-p). Spinor dimension = 2^7 = 128 complex.
- **Default for development:** Start with Riemannian signature (all positive) on Y.

### 11.2 Gamma Matrix Construction

- For dim=n, need n gamma matrices satisfying {Gamma_mu, Gamma_nu} = 2*g_{mu,nu}*I.
- For Riemannian signature (all +): {Gamma_mu, Gamma_nu} = 2*delta_{mu,nu}*I.
- Gamma matrices are Hermitian, size 2^floor(n/2) x 2^floor(n/2).
- **Toy dimY=5:** 4x4 complex matrices via Pauli tensor products. Gamma_1..4 = standard 4D Dirac gammas, Gamma_5 = gamma_5 (4D chirality becomes a regular gamma in 5D).
- **Even dim (e.g., 14):** Chirality operator = Gamma_chi = i^{n/2} * Gamma_1 * ... * Gamma_n.

### 11.3 Chirality -- Critical Dimension Parity Rule + Three-Operator Decomposition

**Dimension parity determines chirality existence:**
- **Even dimY (e.g., 14):** Nontrivial chirality operator exists. Gamma_chi^2 = I, {Gamma_chi, Gamma_mu} = 0. Spinors decompose into Weyl (chiral) representations of dimension 2^{n/2 - 1}. This is where Standard Model chiral structure could emerge.
- **Odd dimY (e.g., toy dimY=5):** NO nontrivial chirality grading exists. The Clifford algebra is simple (not a direct sum). Spinors do NOT decompose into Weyl spinors. Product of all gammas is proportional to identity.
- **IMPLEMENTATION RULE:** ChiralityAnalyzer must check `spacetimeDimension % 2 == 0` before computing chirality projections. For odd dimensions, chirality decomposition should report `chiralityTag = "trivial"` with `leftFraction = 0.5`, `rightFraction = 0.5`, `mixedFraction = 0.0`.
- The chirality sign convention (P_L = (1/2)(I +/- Gamma_chi)) MUST be a branch parameter.

**Three chirality-related operators (for even dimY):**

The physical 4D chirality observed on X arises from the DECOMPOSITION of Y-chirality under dimensional reduction. Implement THREE operators:

1. **Y-chirality (full):** Product of all dim(Y) gammas. Decomposes 128-dim spinors into two 64-dim Weyl representations.
2. **X-chirality (base):** Product of the dim(X) "horizontal" gammas (along base X directions). This relates to observed 4D chirality.
3. **F-chirality (fiber):** Product of the dim(F) "vertical" gammas (along fiber directions).
4. **Relation:** Y-chirality = X-chirality * F-chirality (up to sign convention).

A 14D Weyl spinor (eigenstate of Y-chirality) decomposes into components that may or may not be chiral from the 4D perspective.

**Important test case:** Toy dimY=5 with base dimX=2 (even). Y-chirality is absent (odd dim), but base X-chirality IS well-defined. This exercises the code path where Y-chirality is absent but partial chirality diagnostics still apply.

```csharp
// ChiralityConventionSpec should include:
public sealed class ChiralityConventionSpec
{
    // ...existing fields...
    [JsonPropertyName("fullChiralityOperator")]
    public required string FullChiralityOperator { get; init; }  // "Y-chirality"

    [JsonPropertyName("baseChiralityOperator")]
    public string? BaseChiralityOperator { get; init; }  // "X-chirality" (if dimX even)

    [JsonPropertyName("fiberChiralityOperator")]
    public string? FiberChiralityOperator { get; init; }  // "F-chirality" (if dimF even)

    [JsonPropertyName("baseDimension")]
    public int? BaseDimension { get; init; }

    [JsonPropertyName("fiberDimension")]
    public int? FiberDimension { get; init; }
}
```

### 11.4 Spin Connection Source (THREE Distinct Connection Objects)

There are THREE distinct connection-like objects in GU. Conflating them is a common error.

1. **Levi-Civita spin connection Omega^LC_spin:** Connection on Spin(Y) induced by the metric on Y. Uniquely determined by Y_h geometry (edge lengths, cell shapes, dihedral angles). This is the PRIMARY spin connection.

2. **Gauge connection omega:** The ad(P)-valued connection that is the primary dynamical variable in Phase I-III. Lives on the gauge bundle P -> Y, NOT on the spin bundle. Does NOT directly appear in the spin connection.

3. **Gauge-coupled Dirac operator structure:**

```
D_h = Gamma^mu * (nabla^{LC}_{spin,mu} + omega_mu^a * rho(T_a)) + M_branch + C_branch
```

where:
- `nabla^{LC}_{spin}` uses the Levi-Civita spin connection (from Y_h geometry)
- `omega_mu^a * rho(T_a)` is the gauge connection acting on spinors through representation rho
- `rho(T_a)` are generators of the gauge group in the spinor representation
- `M_branch` captures torsion-dependent mass-like corrections (from contorsion tensor)
- `C_branch` captures additional geometric couplings

**Why this matters for coupling:** The variation `delta D_h / delta omega` comes PRIMARILY from the gauge-coupling term. When you perturb the bosonic state (omega), the Levi-Civita spin connection does NOT change (it depends on the metric, not omega). The gauge-coupling term changes linearly in delta omega. This is where the boson-fermion vertex lives.

**SpinConnectionBundle must have two parts:**
- LeviCivitaPart: from Y_h geometry
- GaugeCouplingPart: from bosonic omega via rho(T_a)

**TOY SIMPLIFICATION (legitimate):** For initial implementation, set the Levi-Civita part to zero (flat spin connection) and only include the gauge-coupling part. Equivalent to working on flat Y with gauge fields. Still gives nontrivial fermionic spectra and couplings. Record as inserted assumption `P4-IA-003: Flat spin connection on Y_h`.

### 11.4a Gauge Representation rho (NEW Branch Parameter)

How the gauge group acts on spinors is itself a branch choice:
- `rho(T_a)` maps gauge algebra generators to matrices acting on spinor space
- **Recommended default:** Adjoint representation: `rho(T_a)_{bc} = f^c_{ab}` (structure constants). Natural, gauge-covariant, requires no new representation theory machinery.
- This is a branch parameter. Record in SpinorRepresentationSpec (new field `gaugeRepresentationId`).
- For su(2): dimG=3, so rho(T_a) are 3x3 matrices acting on an internal index
- Total spinor-with-gauge block size per cell: spinorDim * dimG (e.g., 4 * 3 = 12 for toy dimY=5 + su(2))

### 11.5 Fermionic Inner Product

- M_psi must be positive definite for the generalized eigenproblem to be well-posed.
- **For Riemannian signature on Y (recommended default):** M_psi = standard Hermitian inner product integrated with Y-space volume form:

```
<psi_h, phi_h>_M = sum over cells c: psi_h[c]^dagger * phi_h[c] * vol(c)
```

- M_psi is **block-diagonal**: one block per cell, each block = `vol(c) * I_s` where I_s is the identity on the local spinor space (dim = 2^floor(n/2)). This is positive definite.
- For Lorentzian/mixed signature: M_psi = psi^dagger * Gamma_0 * phi (signature-dependent, may be indefinite -- requires different eigensolver algorithms).
- **RECOMMENDATION (physicist-endorsed):** Start with Riemannian signature on Y and standard Hermitian inner product. This avoids indefinite metric complications while capturing the algebraic structure.
- The inner product convention MUST be serialized in SpinorRepresentationSpec.innerProductConventionId.
- This integrates cleanly with the existing LanczosSolver from Phase III (already handles generalized eigenproblems `H v = lambda M v` with positive definite M, including CG-based M^{-1} solve).

**Hermiticity diagnostic (REQUIRED):** For Riemannian signature, D_h should be Hermitian (self-adjoint). Implementers MUST compute `||D_h - D_h^dagger|| / ||D_h||` as a validation diagnostic. Large values indicate discretization or assembly errors. Large imaginary parts in eigenvalues are a related red flag.

### 11.6 Coupling Proxy Structure

- g_ijk = <phi_i_bar, (dD_h/d_omega)[b_k] phi_j>_{M_psi} where variation is w.r.t. CONNECTION omega.
- Bosonic modes b_k from Phase III live in CONNECTION SPACE (perturbations delta omega), not curvature/torsion space.
- It IS a trilinear form (one bosonic mode, two fermionic modes).

**Concrete form of dD_h/d_omega[delta_omega]:**

Since D_h = Gamma^mu * (nabla^{LC}_{spin,mu} + omega_mu^a * rho(T_a)) + M_branch + C_branch, and Levi-Civita does NOT depend on omega:

```
dD_h/d_omega[delta_omega] = Gamma^mu * delta_omega_mu^a * rho(T_a) + dM_branch/d_omega[delta_omega]
```

For simplest branch (M_branch linear in omega from augmented torsion):
```
dD_h/d_omega[delta_omega] = Gamma^mu * delta_omega_mu^a * rho(T_a) + (torsion correction linear in delta_omega)
```

This is a LINEAR operator applied to b_k, then sandwiched between fermionic modes. No chain rule through curvature/torsion assembly needed for the gauge-coupling term.

- **Selection rules come from:** chirality (if dimY even), gauge quantum numbers via rho(T_a), and tensor structure of delta_D.
- **This is NOT a scattering amplitude** -- it lacks propagator dressing, loop corrections, and proper on-shell projection. Label as proxy everywhere.

### 11.7 Generation Structure

- Generation/family structure would emerge from multiple fermionic zero modes or near-zero modes of D_h with similar quantum numbers but different mass-like eigenvalues.
- The fiber structure of Y over X provides extra dimensions that can support multiple copies.
- **Whether this produces 3 generations is an OPEN QUESTION -- do not assume it.**
- Clustering algorithm should look for groups of modes with: same chirality, same gauge quantum numbers, but distinct mass-like scales.

### 11.8 Candidate Fermion Definition

A candidate fermion in Phase IV is:
1. An eigenmode of D_h(z_*) with well-defined chirality (if dimY even)
2. That persists across branch variation and mesh refinement
3. That has a nonzero coupling proxy to at least one bosonic mode
4. That descends nontrivially through sigma_h^* to X_h
5. It is NOT yet identified with a known Standard Model fermion unless comparison evidence supports it

### 11.9 Summary of Branch Parameters vs Fixed Quantities

| Design Decision | Value | Branch Parameter? |
|---|---|---|
| Spinor dimension | 2^{floor(dimY/2)} complex | No (mathematically fixed) |
| Clifford signature (p,q) | p+q = dimY | YES |
| Has Y-chirality | dimY even: yes; dimY odd: no | No (mathematically fixed) |
| Spin connection source | Levi-Civita + gauge coupling (separate) | LC part can be flat as simplification |
| Chirality sign convention | Which eigenvalue = "left" | YES |
| Base vs fiber chirality | Decompose when Y = X x F | Requires product mesh structure |
| Inner product M_psi | Hermitian + volume form (Riemannian) | Signature convention is branch param |
| Coupling variation | d/d_omega (connection space) | No (fixed by physics) |
| Gauge representation rho | How gauge group acts on spinors | YES (major branch parameter) |
| Weyl vs Dirac spinors | Implement both; Weyl is projection of Dirac | Decomposition is a diagnostic, not a choice |

---

## 12. Milestone Dependency and Parallelism

```
P4-C1 ─┐
P4-C2 ─┼──> M33 ──> M34 ──> M35 ──> M36 ──> M37 ──> M38 ──> M39 ──> M40 ──> M41 ──> M42
P4-C3 ─┘                                       |                               |       |
                                                └──> SCHEMAS (parallel)         |       |
                                                                                v       v
                                                                          M43 ──┼──> M44 ──> M45
                                                                                |
                                                                                v
                                                                          REPORTING
```

**What can run in parallel:**
- P4-C1, P4-C2, P4-C3 (all 3 are independent prerequisites)
- SCHEMAS task can start after M33 types are defined (doesn't need full M33 tests)
- M37 (Chirality) depends on M36 output but needs SCHEMAS for config files
- REPORTING can start after M41 (needs family clusters + coupling atlas)

**What is strictly sequential:**
- M33 -> M34 -> M35 -> M36 (each builds on the previous)
- M38 depends on M36 (Dirac operator must exist before spectral solve)
- M39 depends on M38 (modes must exist before tracking)
- M40 depends on M38 (modes must exist before coupling extraction)
- M41 depends on M39 + M40 (families + couplings feed clustering)
- M42 depends on M41 (clusters feed registry)
- M44 depends on M36-M42 CPU path (CUDA comes after CPU)
- M45 depends on everything (end-to-end study)
