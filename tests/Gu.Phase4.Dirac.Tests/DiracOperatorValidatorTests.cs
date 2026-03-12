using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Tests;

/// <summary>
/// Tests for DiracOperatorValidator (M36 validation harness).
/// </summary>
public class DiracOperatorValidatorTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-validator",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2, simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 3, 2 } });

    private static DiracOperatorBundle BuildBundle()
    {
        var mesh = TwoTriangles();
        var spec = new SpinorRepresentationSpec
        {
            SpinorSpecId = "s2", SpacetimeDimension = 2,
            CliffordSignature = new CliffordSignature { Positive = 2, Negative = 0 },
            GammaConvention = new GammaConventionSpec
            {
                ConventionId = "dirac-tensor-product-v1",
                Signature = new CliffordSignature { Positive = 2, Negative = 0 },
                Representation = "standard", SpinorDimension = 2, HasChirality = true,
            },
            ChiralityConvention = new ChiralityConventionSpec
            {
                ConventionId = "c1", SignConvention = "left-is-minus",
                PhaseFactor = "-1", HasChirality = true,
            },
            ConjugationConvention = new ConjugationConventionSpec
            {
                ConventionId = "h1", ConjugationType = "hermitian", HasChargeConjugation = true,
            },
            SpinorComponents = 2, ChiralitySplit = 1, Provenance = TestProvenance(),
        };
        var layout = FermionFieldLayoutFactory.BuildStandardLayout("l1", spec, 1, TestProvenance());
        var bg = new BackgroundRecord
        {
            BackgroundId = "bg-v1", EnvironmentId = "env", BranchManifestId = "m1",
            GeometryFingerprint = "fp", StateArtifactRef = "ref",
            ResidualNorm = 0.001, StationarityNorm = 0.001,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 0.001, StationarityNorm = 0.001, ObjectiveValue = 0.1,
                GaugeViolation = 0, SolverIterations = 5, SolverConverged = true,
                TerminationReason = "ok", GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R1", Provenance = TestProvenance(),
        };
        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
        var connBuilder = new CpuSpinConnectionBuilder();
        var omega = new double[mesh.EdgeCount];
        var conn = connBuilder.Build(bg, omega, spec, layout, mesh, TestProvenance());
        var assembler = new CpuDiracOperatorAssembler();
        return assembler.Assemble(conn, gammas, layout, mesh, TestProvenance());
    }

    [Fact]
    public void ValidateShape_Valid_ReturnsNoErrors()
    {
        var bundle = BuildBundle();
        var errors = DiracOperatorValidator.ValidateShape(bundle);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateExplicitMatrixShape_Valid_ReturnsNoErrors()
    {
        var bundle = BuildBundle();
        var errors = DiracOperatorValidator.ValidateExplicitMatrixShape(bundle);
        Assert.Empty(errors);
    }

    [Fact]
    public void CheckHermiticity_ZeroMatrix_IsZero()
    {
        var bundle = BuildBundle();
        // With zero omega and flat LC, D = 0, which is trivially Hermitian
        double violation = DiracOperatorValidator.CheckHermiticity(bundle);
        Assert.Equal(0.0, violation, 12);
    }

    [Fact]
    public void Validate_Full_IsValid()
    {
        var bundle = BuildBundle();
        var report = DiracOperatorValidator.Validate(bundle);
        Assert.True(report.IsShapeValid);
        Assert.True(report.IsValid);
        Assert.Equal("hermitian", report.HermiticityStatus);
    }

    [Fact]
    public void Validate_ShapeReport_HasCorrectFields()
    {
        var bundle = BuildBundle();
        var report = DiracOperatorValidator.Validate(bundle);
        Assert.Equal(0.0, report.HermiticityResidual, 12);
        Assert.NotNull(report.Messages);
    }
}
