using System.Text.Json;

namespace Gu.Phase3.Properties.Tests;

public class SerializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public void MassLikeScaleRecord_RoundTrips()
    {
        var record = new MassLikeScaleRecord
        {
            ModeId = "m-1",
            Eigenvalue = 4.0,
            MassLikeScale = 2.0,
            ExtractionMethod = "eigenvalue",
            OperatorType = "GaussNewton",
            BackgroundId = "bg-1",
            BranchManifestId = "branch-1",
        };

        var json = JsonSerializer.Serialize(record, Options);
        var deserialized = JsonSerializer.Deserialize<MassLikeScaleRecord>(json, Options)!;

        Assert.Equal(record.ModeId, deserialized.ModeId);
        Assert.Equal(record.MassLikeScale, deserialized.MassLikeScale);
        Assert.Equal(record.BranchManifestId, deserialized.BranchManifestId);
    }

    [Fact]
    public void PolarizationDescriptor_RoundTrips()
    {
        var desc = new PolarizationDescriptor
        {
            ModeId = "m-1",
            BlockEnergyFractions = new Dictionary<string, double>
            {
                ["connection"] = 0.8,
                ["auxiliary"] = 0.2,
            },
            DominantClass = "connection-dominant",
            DominanceFraction = 0.8,
            BackgroundId = "bg-1",
        };

        var json = JsonSerializer.Serialize(desc, Options);
        var deserialized = JsonSerializer.Deserialize<PolarizationDescriptor>(json, Options)!;

        Assert.Equal(desc.DominantClass, deserialized.DominantClass);
        Assert.Equal(2, deserialized.BlockEnergyFractions.Count);
    }

    [Fact]
    public void StabilityScoreCard_RoundTrips()
    {
        var card = new StabilityScoreCard
        {
            EntityId = "fam-1",
            BranchStability = 0.95,
            RefinementStability = 0.99,
            BackendStability = 1.0,
            BranchVariantCount = 3,
            RefinementLevelCount = 2,
            BackendCount = 2,
            MaxEigenvalueDrift = 0.01,
        };

        var json = JsonSerializer.Serialize(card, Options);
        var deserialized = JsonSerializer.Deserialize<StabilityScoreCard>(json, Options)!;

        Assert.Equal(card.BranchStability, deserialized.BranchStability);
        Assert.Equal(card.MaxEigenvalueDrift, deserialized.MaxEigenvalueDrift);
    }

    [Fact]
    public void InteractionProxyRecord_RoundTrips()
    {
        var record = new InteractionProxyRecord
        {
            ModeIds = new[] { "m-1", "m-2", "m-3" },
            CubicResponse = 0.042,
            Epsilon = 1e-3,
            Method = "finite-difference-gradient",
            BackgroundId = "bg-1",
            EstimatedError = 1e-5,
        };

        var json = JsonSerializer.Serialize(record, Options);
        var deserialized = JsonSerializer.Deserialize<InteractionProxyRecord>(json, Options)!;

        Assert.Equal(3, deserialized.ModeIds.Count);
        Assert.Equal(0.042, deserialized.CubicResponse);
        Assert.Equal(1e-5, deserialized.EstimatedError);
    }

    [Fact]
    public void BosonPropertyVector_RoundTrips()
    {
        var vector = new BosonPropertyVector
        {
            ModeId = "m-1",
            BackgroundId = "bg-1",
            MassLikeScale = new MassLikeScaleRecord
            {
                ModeId = "m-1",
                Eigenvalue = 4.0,
                MassLikeScale = 2.0,
                ExtractionMethod = "eigenvalue",
                OperatorType = "GaussNewton",
                BackgroundId = "bg-1",
            },
            Polarization = new PolarizationDescriptor
            {
                ModeId = "m-1",
                BlockEnergyFractions = new Dictionary<string, double> { ["connection"] = 1.0 },
                DominantClass = "mixed",
                DominanceFraction = 1.0,
                BackgroundId = "bg-1",
            },
            Symmetry = new SymmetryDescriptor
            {
                ModeId = "m-1",
                BackgroundId = "bg-1",
                ParityEigenvalue = 1.0,
            },
            GaugeLeakScore = 0.01,
            Multiplicity = 2,
            Stability = new StabilityScoreCard
            {
                EntityId = "m-1",
                BranchStability = 0.95,
                RefinementStability = 0.99,
                BackendStability = 1.0,
                BranchVariantCount = 3,
                RefinementLevelCount = 2,
                BackendCount = 2,
            },
        };

        var json = JsonSerializer.Serialize(vector, Options);
        var deserialized = JsonSerializer.Deserialize<BosonPropertyVector>(json, Options)!;

        Assert.Equal("m-1", deserialized.ModeId);
        Assert.Equal(2.0, deserialized.MassLikeScale.MassLikeScale);
        Assert.Equal(0.01, deserialized.GaugeLeakScore);
        Assert.Equal(2, deserialized.Multiplicity);
        Assert.NotNull(deserialized.Stability);
        Assert.Equal(0.95, deserialized.Stability!.BranchStability);
    }

    [Fact]
    public void SymmetryDescriptor_RoundTrips()
    {
        var desc = new SymmetryDescriptor
        {
            ModeId = "m-1",
            BackgroundId = "bg-1",
            ParityEigenvalue = -1.0,
            SymmetryLabels = new[] { "Z2-odd", "C3-eigenvalue-1" },
            SectorOverlaps = new Dictionary<string, double>
            {
                ["even"] = 0.1,
                ["odd"] = 0.9,
            },
        };

        var json = JsonSerializer.Serialize(desc, Options);
        var deserialized = JsonSerializer.Deserialize<SymmetryDescriptor>(json, Options)!;

        Assert.Equal(-1.0, deserialized.ParityEigenvalue);
        Assert.Equal(2, deserialized.SymmetryLabels.Count);
        Assert.Equal(0.9, deserialized.SectorOverlaps!["odd"]);
    }
}
