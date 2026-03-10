using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Backgrounds.Tests;

public class SerializationTests
{
    [Fact]
    public void BackgroundRecord_RoundTrips()
    {
        var record = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B2);
        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("bg-1", deserialized.BackgroundId);
        Assert.Equal(AdmissibilityLevel.B2, deserialized.AdmissibilityLevel);
    }

    [Fact]
    public void BackgroundAtlas_RoundTrips()
    {
        var atlas = new BackgroundAtlas
        {
            AtlasId = "atlas-1",
            StudyId = "study-1",
            Backgrounds = new[]
            {
                TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B2),
                TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1),
            },
            RejectedBackgrounds = new[]
            {
                TestHelpers.MakeRecord("bg-rej", AdmissibilityLevel.Rejected),
            },
            RankingCriteria = "admissibility-then-residual",
            TotalAttempts = 3,
            Provenance = TestHelpers.MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int>
            {
                ["B2"] = 1,
                ["B1"] = 1,
                ["Rejected"] = 1,
            },
        };

        var json = BackgroundAtlasSerializer.SerializeAtlas(atlas);
        Assert.Contains("atlas-1", json);
        Assert.Contains("B2", json);

        var deserialized = BackgroundAtlasSerializer.DeserializeAtlas(json);
        Assert.NotNull(deserialized);
        Assert.Equal("atlas-1", deserialized.AtlasId);
        Assert.Equal(2, deserialized.Backgrounds.Count);
        Assert.Single(deserialized.RejectedBackgrounds);
    }

    [Fact]
    public void BackgroundStudySpec_RoundTrips()
    {
        var study = new BackgroundStudySpec
        {
            StudyId = "study-1",
            Specs = new[] { TestHelpers.MakeSpec("s1"), TestHelpers.MakeSpec("s2") },
            DeduplicationThreshold = 1e-8,
            RankingCriteria = "residual-then-stationarity",
        };

        var json = BackgroundAtlasSerializer.SerializeStudy(study);
        var deserialized = BackgroundAtlasSerializer.DeserializeStudy(json);
        Assert.NotNull(deserialized);
        Assert.Equal("study-1", deserialized.StudyId);
        Assert.Equal(2, deserialized.Specs.Count);
        Assert.Equal(1e-8, deserialized.DeduplicationThreshold);
    }

    [Fact]
    public void BackgroundSpec_RoundTrips()
    {
        var spec = TestHelpers.MakeSpec("spec-1");
        var json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundSpec>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("spec-1", deserialized.SpecId);
        Assert.Equal("env-1", deserialized.EnvironmentId);
    }

    [Fact]
    public void BackgroundSolveOptions_RoundTrips()
    {
        var options = new BackgroundSolveOptions
        {
            SolveMode = Gu.Solvers.SolveMode.StationaritySolve,
            SolverMethod = Gu.Solvers.SolverMethod.GaussNewton,
            MaxIterations = 500,
            ToleranceStationary = 1e-8,
        };

        var json = GuJsonDefaults.Serialize(options);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundSolveOptions>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(Gu.Solvers.SolveMode.StationaritySolve, deserialized.SolveMode);
        Assert.Equal(500, deserialized.MaxIterations);
    }

    [Fact]
    public void WriteAndReadAtlas_FileRoundTrip()
    {
        var atlas = new BackgroundAtlas
        {
            AtlasId = "atlas-file-test",
            StudyId = "study-file",
            Backgrounds = new[] { TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1) },
            RejectedBackgrounds = Array.Empty<BackgroundRecord>(),
            RankingCriteria = "admissibility-then-residual",
            TotalAttempts = 1,
            Provenance = TestHelpers.MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int> { ["B1"] = 1 },
        };

        var path = Path.Combine(Path.GetTempPath(), $"gu-test-atlas-{Guid.NewGuid()}.json");
        try
        {
            BackgroundAtlasSerializer.WriteAtlas(atlas, path);
            Assert.True(File.Exists(path));

            var loaded = BackgroundAtlasSerializer.ReadAtlas(path);
            Assert.NotNull(loaded);
            Assert.Equal("atlas-file-test", loaded.AtlasId);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void AdmissibilityLevel_SerializesAsString()
    {
        var record = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B2);
        var json = GuJsonDefaults.Serialize(record);
        Assert.Contains("\"B2\"", json);
    }
}
