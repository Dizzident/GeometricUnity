using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Spectra.Tests;

public class SerializationTests
{
    [Fact]
    public void OperatorBundleArtifact_RoundTrips()
    {
        var artifact = new OperatorBundleArtifact
        {
            BundleId = "bundle-1",
            BackgroundId = "bg-1",
            BranchManifestId = "branch-1",
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.ProjectedComplement,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            GaugeLambda = 0.5,
            StateDimension = 9,
            PhysicalDimension = 3,
            GaugeRank = 6,
            HasResidualCorrection = true,
        };

        var json = OperatorBundleSerializer.Serialize(artifact);
        var deserialized = OperatorBundleSerializer.Deserialize(json);

        Assert.NotNull(deserialized);
        Assert.Equal("bundle-1", deserialized!.BundleId);
        Assert.Equal("bg-1", deserialized.BackgroundId);
        Assert.Equal(SpectralOperatorType.FullHessian, deserialized.OperatorType);
        Assert.Equal(PhysicalModeFormulation.ProjectedComplement, deserialized.Formulation);
        Assert.Equal(AdmissibilityLevel.B1, deserialized.BackgroundAdmissibility);
        Assert.Equal(0.5, deserialized.GaugeLambda);
        Assert.Equal(9, deserialized.StateDimension);
        Assert.Equal(3, deserialized.PhysicalDimension);
        Assert.Equal(6, deserialized.GaugeRank);
        Assert.True(deserialized.HasResidualCorrection);
    }

    [Fact]
    public void OperatorBundleArtifact_NullOptionalFields_RoundTrips()
    {
        var artifact = new OperatorBundleArtifact
        {
            BundleId = "bundle-2",
            BackgroundId = "bg-2",
            BranchManifestId = "branch-2",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            GaugeLambda = 1.0,
            StateDimension = 12,
        };

        var json = OperatorBundleSerializer.Serialize(artifact);
        var deserialized = OperatorBundleSerializer.Deserialize(json);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized!.PhysicalDimension);
        Assert.Null(deserialized.GaugeRank);
        Assert.False(deserialized.HasResidualCorrection);
    }

    [Fact]
    public void LinearizedOperatorSpec_RoundTrips()
    {
        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-spec",
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.ProjectedComplement,
            GaugeLambda = 2.5,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
        };

        var json = OperatorBundleSerializer.Serialize(spec);
        var deserialized = OperatorBundleSerializer.DeserializeSpec(json);

        Assert.NotNull(deserialized);
        Assert.Equal("bg-spec", deserialized!.BackgroundId);
        Assert.Equal(SpectralOperatorType.FullHessian, deserialized.OperatorType);
        Assert.Equal(2.5, deserialized.GaugeLambda);
    }

    [Fact]
    public void FromBundle_ExtractsMetadata()
    {
        int n = 9;
        var diag = new double[n];
        Array.Fill(diag, 1.0);
        var spectralOp = new TestHelpers.DiagonalOperator(diag);
        var massOp = new TestHelpers.DiagonalOperator(diag);
        var jacobian = new TestHelpers.DiagonalOperator(diag);

        var bundle = new LinearizedOperatorBundle
        {
            BundleId = "test-fb",
            BackgroundId = "bg-fb",
            BranchManifestId = "branch-fb",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Jacobian = jacobian,
            SpectralOperator = spectralOp,
            MassOperator = massOp,
            GaugeLambda = 0.1,
            StateDimension = n,
        };

        var artifact = OperatorBundleArtifact.FromBundle(bundle);

        Assert.Equal("test-fb", artifact.BundleId);
        Assert.Equal("bg-fb", artifact.BackgroundId);
        Assert.Equal(SpectralOperatorType.GaussNewton, artifact.OperatorType);
        Assert.False(artifact.HasResidualCorrection);
    }

    [Fact]
    public void FromBundle_DetectsResidualCorrection()
    {
        int n = 9;
        var diag = new double[n];
        Array.Fill(diag, 1.0);
        var hgn = new TestHelpers.DiagonalOperator(diag);
        var massOp = new TestHelpers.DiagonalOperator(diag);
        var jacobian = new TestHelpers.DiagonalOperator(diag);

        Func<Gu.Core.FieldTensor, Gu.Core.FieldTensor> correction = v => v;
        var fullH = new FullHessianOperator(hgn, correction);

        var bundle = new LinearizedOperatorBundle
        {
            BundleId = "test-corr",
            BackgroundId = "bg-corr",
            BranchManifestId = "branch-corr",
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Jacobian = jacobian,
            SpectralOperator = fullH,
            MassOperator = massOp,
            GaugeLambda = 0.1,
            StateDimension = n,
        };

        var artifact = OperatorBundleArtifact.FromBundle(bundle);
        Assert.True(artifact.HasResidualCorrection);
    }

    [Fact]
    public async Task FileRoundTrip_WritesAndReads()
    {
        var artifact = new OperatorBundleArtifact
        {
            BundleId = "file-rt",
            BackgroundId = "bg-file",
            BranchManifestId = "branch-file",
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.ProjectedComplement,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            GaugeLambda = 1.0,
            StateDimension = 9,
            PhysicalDimension = 3,
            GaugeRank = 6,
        };

        var path = Path.Combine(Path.GetTempPath(), $"spectra-test-{Guid.NewGuid()}.json");
        try
        {
            await OperatorBundleSerializer.WriteArtifactAsync(path, artifact);
            var loaded = await OperatorBundleSerializer.ReadArtifactAsync(path);

            Assert.NotNull(loaded);
            Assert.Equal("file-rt", loaded!.BundleId);
            Assert.Equal(9, loaded.StateDimension);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
