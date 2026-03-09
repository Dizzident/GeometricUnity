using Gu.Core.Serialization;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Stability.Tests;

public class PrincipalSymbolSerializationTests
{
    [Fact]
    public void PrincipalSymbolRecord_RoundTrips()
    {
        var record = new PrincipalSymbolRecord
        {
            CellIndex = 5,
            Covector = new[] { 1.0, 0.0, 0.0 },
            SymbolMatrix = new[] { new[] { 2.0, 0.1 }, new[] { 0.1, 3.0 } },
            Eigenvalues = new[] { 1.95, 3.05 },
            IsSymmetric = true,
            SymmetryError = 0.0,
            DefinitenessIndicator = "positive-definite",
            RankDeficiency = 0,
            GaugeNullDimension = 0,
            Classification = PdeClassification.EllipticLike,
            BranchManifestId = "branch-1",
            GaugeStudyMode = GaugeStudyMode.GaugeFree,
            OperatorId = "J",
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<PrincipalSymbolRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(5, deserialized!.CellIndex);
        Assert.Equal(PdeClassification.EllipticLike, deserialized.Classification);
        Assert.Equal(GaugeStudyMode.GaugeFree, deserialized.GaugeStudyMode);
        Assert.Equal("branch-1", deserialized.BranchManifestId);
        Assert.Equal(3, deserialized.Covector.Length);
    }

    [Fact]
    public void SymbolStudyReport_RoundTrips()
    {
        var sample = new PrincipalSymbolRecord
        {
            CellIndex = 0,
            Covector = new[] { 1.0 },
            SymbolMatrix = new[] { new[] { 1.0 } },
            Eigenvalues = new[] { 1.0 },
            IsSymmetric = true,
            SymmetryError = 0,
            DefinitenessIndicator = "positive-definite",
            RankDeficiency = 0,
            GaugeNullDimension = 0,
            Classification = PdeClassification.EllipticLike,
            BranchManifestId = "branch-1",
            GaugeStudyMode = GaugeStudyMode.GaugeFree,
            OperatorId = "H",
        };

        var report = SymbolStudyReport.FromSamples(
            "study-1", "branch-1", "bg-1", "H", GaugeStudyMode.GaugeFree, new[] { sample });

        var json = GuJsonDefaults.Serialize(report);
        var deserialized = GuJsonDefaults.Deserialize<SymbolStudyReport>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("study-1", deserialized!.StudyId);
        Assert.Equal(PdeClassification.EllipticLike, deserialized.SummaryClassification);
        Assert.Equal(1, deserialized.TotalSamples);
        Assert.Single(deserialized.Samples);
    }

    [Fact]
    public void PdeClassification_SerializesAsString()
    {
        var record = new PrincipalSymbolRecord
        {
            CellIndex = 0,
            Covector = new[] { 1.0 },
            SymbolMatrix = new[] { new[] { 0.0 } },
            Eigenvalues = new[] { 0.0 },
            IsSymmetric = true,
            SymmetryError = 0,
            DefinitenessIndicator = "zero",
            RankDeficiency = 1,
            GaugeNullDimension = 0,
            Classification = PdeClassification.Degenerate,
            BranchManifestId = "branch-1",
            GaugeStudyMode = GaugeStudyMode.GaugeFixed,
            OperatorId = "J",
        };

        var json = GuJsonDefaults.Serialize(record);
        Assert.Contains("\"Degenerate\"", json);
        Assert.Contains("\"GaugeFixed\"", json);
    }
}
