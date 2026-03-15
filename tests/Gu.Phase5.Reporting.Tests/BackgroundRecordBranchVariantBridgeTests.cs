using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class BackgroundRecordBranchVariantBridgeTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static BackgroundMetrics MakeMetrics(bool converged = true, double residual = 0.001) =>
        new BackgroundMetrics
        {
            ResidualNorm = residual,
            StationarityNorm = residual * 0.5,
            ObjectiveValue = residual * residual,
            GaugeViolation = 1e-8,
            SolverIterations = 42,
            SolverConverged = converged,
            TerminationReason = converged ? "residual-tolerance" : "max-iterations",
            GaussNewtonValid = converged,
        };

    private static BackgroundRecord MakeRecord(
        string backgroundId = "bg-001",
        string branchManifestId = "branch-1",
        string geometryFingerprint = "fp-abc123",
        AdmissibilityLevel level = AdmissibilityLevel.B1,
        bool converged = true) =>
        new BackgroundRecord
        {
            BackgroundId = backgroundId,
            EnvironmentId = "env-toy",
            BranchManifestId = branchManifestId,
            GeometryFingerprint = geometryFingerprint,
            StateArtifactRef = "artifacts/states/bg-001.json",
            ResidualNorm = 0.001,
            StationarityNorm = 0.0005,
            AdmissibilityLevel = level,
            Metrics = MakeMetrics(converged),
            ReplayTierAchieved = "B1",
            Provenance = MakeProvenance(),
        };

    private static BackgroundAtlas MakeAtlas(IReadOnlyList<BackgroundRecord> admitted,
        IReadOnlyList<BackgroundRecord>? rejected = null) =>
        new BackgroundAtlas
        {
            AtlasId = "atlas-001",
            StudyId = "study-001",
            Backgrounds = admitted,
            RejectedBackgrounds = rejected ?? [],
            RankingCriteria = "residual-norm",
            TotalAttempts = admitted.Count + (rejected?.Count ?? 0),
            Provenance = MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int> { ["B1"] = admitted.Count },
        };

    [Fact]
    public void DeriveVariantId_SameRecord_IsDeterministic()
    {
        var record = MakeRecord();
        var id1 = BackgroundRecordBranchVariantBridge.DeriveVariantId(record);
        var id2 = BackgroundRecordBranchVariantBridge.DeriveVariantId(record);
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void DeriveVariantId_DifferentBackgroundIds_AreDifferent()
    {
        var r1 = MakeRecord(backgroundId: "bg-001");
        var r2 = MakeRecord(backgroundId: "bg-002");
        Assert.NotEqual(
            BackgroundRecordBranchVariantBridge.DeriveVariantId(r1),
            BackgroundRecordBranchVariantBridge.DeriveVariantId(r2));
    }

    [Fact]
    public void DeriveVariantId_DifferentManifestIds_AreDifferent()
    {
        var r1 = MakeRecord(branchManifestId: "branch-1");
        var r2 = MakeRecord(branchManifestId: "branch-2");
        Assert.NotEqual(
            BackgroundRecordBranchVariantBridge.DeriveVariantId(r1),
            BackgroundRecordBranchVariantBridge.DeriveVariantId(r2));
    }

    [Fact]
    public void ExtractQuantityValues_ContainsExpectedKeys()
    {
        var record = MakeRecord();
        var values = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(record);

        Assert.True(values.ContainsKey("residual-norm"));
        Assert.True(values.ContainsKey("stationarity-norm"));
        Assert.True(values.ContainsKey("objective-value"));
        Assert.True(values.ContainsKey("gauge-violation"));
        Assert.True(values.ContainsKey("solver-converged"));
        Assert.True(values.ContainsKey("solver-iterations"));
    }

    [Fact]
    public void ExtractQuantityValues_SolverConverged_ReturnsOnePointZero()
    {
        var convergedRecord = MakeRecord(converged: true);
        var divergedRecord = MakeRecord(converged: false);

        var cv = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(convergedRecord);
        var dv = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(divergedRecord);

        Assert.Equal(1.0, cv["solver-converged"][0]);
        Assert.Equal(0.0, dv["solver-converged"][0]);
    }

    [Fact]
    public void ExtractQuantityValues_EachQuantityIsSingleElementArray()
    {
        var record = MakeRecord();
        var values = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(record);

        foreach (var (key, arr) in values)
            Assert.True(arr.Length == 1, $"Expected single-element array for '{key}', got length {arr.Length}.");
    }

    [Fact]
    public void BuildVariantMap_ExcludesRejectedByDefault()
    {
        var admitted = new[] { MakeRecord("bg-001"), MakeRecord("bg-002") };
        var rejected = new[] { MakeRecord("bg-003", level: AdmissibilityLevel.Rejected) };

        var map = BackgroundRecordBranchVariantBridge.BuildVariantMap(admitted.Concat(rejected));

        Assert.Equal(2, map.Count);
    }

    [Fact]
    public void BuildVariantMap_IncludesRejectedWhenRequested()
    {
        var admitted = new[] { MakeRecord("bg-001") };
        var rejected = new[] { MakeRecord("bg-002", level: AdmissibilityLevel.Rejected) };

        var map = BackgroundRecordBranchVariantBridge.BuildVariantMap(
            admitted.Concat(rejected),
            includeRejected: true);

        Assert.Equal(2, map.Count);
    }

    [Fact]
    public void BuildVariantMap_FromAtlas_UsesAdmittedBackgrounds()
    {
        var admitted = new[] { MakeRecord("bg-001"), MakeRecord("bg-002") };
        var rejected = new[] { MakeRecord("bg-003", level: AdmissibilityLevel.Rejected) };
        var atlas = MakeAtlas(admitted, rejected);

        var map = BackgroundRecordBranchVariantBridge.BuildVariantMap(atlas);

        Assert.Equal(2, map.Count);
    }

    [Fact]
    public void GetVariantIds_ReturnsOneIdPerAdmittedBackground()
    {
        var admitted = new[] { MakeRecord("bg-001"), MakeRecord("bg-002"), MakeRecord("bg-003") };
        var atlas = MakeAtlas(admitted);

        var ids = BackgroundRecordBranchVariantBridge.GetVariantIds(atlas);

        Assert.Equal(3, ids.Count);
        Assert.Equal(ids.Distinct().Count(), ids.Count); // all unique
    }

    [Fact]
    public void CreateBranchExecutor_ReturnsQuantitiesForKnownVariant()
    {
        var record = MakeRecord("bg-001");
        var atlas = MakeAtlas([record]);
        var variantId = BackgroundRecordBranchVariantBridge.DeriveVariantId(record);

        var executor = BackgroundRecordBranchVariantBridge.CreateBranchExecutor(atlas);
        var result = executor(variantId);

        Assert.NotEmpty(result);
        Assert.True(result.ContainsKey("residual-norm"));
    }

    [Fact]
    public void CreateBranchExecutor_ReturnsEmptyForUnknownVariant()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001")]);
        var executor = BackgroundRecordBranchVariantBridge.CreateBranchExecutor(atlas);

        var result = executor("unknown-variant-id");

        Assert.Empty(result);
    }

    [Fact]
    public void CreateBranchExecutor_ExtraQuantitiesOverrideStandard()
    {
        var record = MakeRecord("bg-001");
        var atlas = MakeAtlas([record]);
        var variantId = BackgroundRecordBranchVariantBridge.DeriveVariantId(record);

        var extra = new Dictionary<string, IReadOnlyDictionary<string, double[]>>
        {
            [variantId] = new Dictionary<string, double[]>
            {
                ["residual-norm"] = [99.9], // override
                ["custom-quantity"] = [1.23],
            }
        };

        var executor = BackgroundRecordBranchVariantBridge.CreateBranchExecutor(atlas, extra);
        var result = executor(variantId);

        Assert.Equal(99.9, result["residual-norm"][0]);
        Assert.Equal(1.23, result["custom-quantity"][0]);
    }
}
