using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;
using System.Text.Json;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class InternalVectorBosonSourceSpectrumTests
{
    [Fact]
    public void ModeRecord_RoundTripsSelectorMetadata()
    {
        var record = MakeMode("mode-a", "source-a", "branch-a", "L1", "env-a", 1.25);

        var roundTripped = GuJsonDefaults.Deserialize<InternalVectorBosonSourceModeRecord>(
            GuJsonDefaults.Serialize(record));

        Assert.NotNull(roundTripped);
        Assert.Equal("branch-a", roundTripped.BranchVariantId);
        Assert.Equal("L1", roundTripped.RefinementLevel);
        Assert.Equal("env-a", roundTripped.EnvironmentId);
        Assert.Equal("source-a", roundTripped.SourceCandidateId);
    }

    [Fact]
    public void UncertaintyEstimator_CompleteMatrix_EstimatesTotalInQuadrature()
    {
        var modes = new[]
        {
            MakeMode("m-1", "source-a", "branch-a", "L0", "env-a", 1.00),
            MakeMode("m-2", "source-a", "branch-b", "L0", "env-a", 1.01),
            MakeMode("m-3", "source-a", "branch-a", "L1", "env-b", 1.02),
            MakeMode("m-4", "source-a", "branch-b", "L1", "env-b", 1.03),
        };

        var uncertainty = InternalVectorBosonSourceUncertaintyEstimator.Estimate(modes);

        Assert.True(uncertainty.IsFullyEstimated);
        Assert.True(uncertainty.TotalUncertainty > 0);
    }

    [Fact]
    public void UncertaintyEstimator_SingleAxis_RemainsIncomplete()
    {
        var modes = new[] { MakeMode("m-1", "source-a", "branch-a", "L0", "env-a", 1.00) };

        var uncertainty = InternalVectorBosonSourceUncertaintyEstimator.Estimate(modes);

        Assert.False(uncertainty.IsFullyEstimated);
        Assert.Equal(-1, uncertainty.TotalUncertainty);
    }

    [Fact]
    public void MatrixCampaign_GeneratesReadyIdentityNeutralSourcesFromSelectorMatrix()
    {
        using var temp = new TempDir();
        var result = InternalVectorBosonSourceMatrixCampaign.Run(
            MakeCampaignSpec(),
            MakeSeedTable(),
            MakeReadinessSpec(),
            temp.Path,
            MakeProvenance());

        Assert.Equal("candidate-source-ready", result.SourceCandidates.TerminalStatus);
        Assert.True(result.SourceCandidates.Candidates.Count(c => c.Status == "candidate-source-ready") >= 2);
        Assert.All(result.SourceCandidates.Candidates, candidate =>
        {
            Assert.NotEmpty(candidate.BranchSelectors);
            Assert.NotEmpty(candidate.RefinementLevels);
            Assert.NotEmpty(candidate.EnvironmentSelectors);
            Assert.DoesNotContain("w-boson", candidate.SourceCandidateId, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("z-boson", candidate.SourceCandidateId, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void MatrixCampaign_WithSelectorCellBundles_EmitsSolverBackedSpectrumProvenance()
    {
        using var temp = new TempDir();
        var bundleManifestPath = Path.Combine(temp.Path, "selector_cell_bundle_manifest.json");
        File.WriteAllText(bundleManifestPath, GuJsonDefaults.Serialize(MakeBundleManifest()));

        var result = InternalVectorBosonSourceMatrixCampaign.Run(
            MakeCampaignSpec(),
            MakeSeedTable(),
            MakeReadinessSpec(),
            temp.Path,
            MakeProvenance(),
            bundleManifestPath);

        var entry = result.Manifest.Entries[0];
        Assert.NotNull(entry.SelectorCellBundleId);
        var spectrumPath = Path.Combine(temp.Path, entry.SpectrumPath);
        using var spectrum = JsonDocument.Parse(File.ReadAllText(spectrumPath));
        var root = spectrum.RootElement;
        Assert.Equal("selector-cell-bundle-materialized-source-matrix-v1", root.GetProperty("solverMethod").GetString());
        Assert.Equal("materialized-selector-cell-source-operator", root.GetProperty("operatorType").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("operatorBundleId").GetString()));
        Assert.True(root.GetProperty("modeRecords").GetArrayLength() > 0);
        Assert.Contains(result.ModeRecords, m => !string.IsNullOrWhiteSpace(m.SelectorCellBundleId));
    }

    [Fact]
    public void MatrixCampaign_WithSelectorCellBundles_FailsClosedOnMissingBundle()
    {
        using var temp = new TempDir();
        var bundleManifestPath = Path.Combine(temp.Path, "selector_cell_bundle_manifest.json");
        File.WriteAllText(bundleManifestPath, GuJsonDefaults.Serialize(new
        {
            bundles = new[]
            {
                new
                {
                    bundleId = "bundle-branch-a-L0-env-a",
                    branchVariantId = "branch-a",
                    refinementLevel = "L0",
                    environmentId = "env-a",
                    written = true,
                    backgroundRecordPath = "bundles/bundle-branch-a-L0-env-a/background.json",
                },
            },
        }));

        var ex = Assert.Throws<InvalidOperationException>(() => InternalVectorBosonSourceMatrixCampaign.Run(
            MakeCampaignSpec(),
            MakeSeedTable(),
            MakeReadinessSpec(),
            temp.Path,
            MakeProvenance(),
            bundleManifestPath));
        Assert.Contains("Missing materialized selector-cell bundle", ex.Message, StringComparison.Ordinal);
    }

    private static InternalVectorBosonSourceModeRecord MakeMode(
        string modeId,
        string sourceId,
        string branch,
        string refinement,
        string environment,
        double value)
        => new()
        {
            ModeRecordId = modeId,
            SourceCandidateId = sourceId,
            SourceFamilyId = "family-a",
            BranchVariantId = branch,
            RefinementLevel = refinement,
            EnvironmentId = environment,
            MassLikeValue = value,
            ExtractionError = 0.001,
            GaugeLeakEnvelope = [0, 0, 0],
            SourceArtifactPaths = ["studies/internal/source.json"],
            Status = "computed",
            Blockers = [],
            Provenance = MakeProvenance(),
        };

    private static InternalVectorBosonSourceSpectrumCampaignSpec MakeCampaignSpec() => new()
    {
        CampaignId = "phase22-test",
        SchemaVersion = "1.0.0",
        SourceCandidateTablePath = "source_candidates.json",
        ReadinessSpecPath = "readiness.json",
        BranchVariantIds = ["branch-a", "branch-b"],
        RefinementLevels = ["L0", "L1"],
        EnvironmentIds = ["env-a", "env-b"],
        SourceQuantityIds = ["massLikeValue"],
        MissingCellPolicy = "block-missing-cell",
        IdentityScope = "identity-neutral-vector-boson-source-candidates",
        Provenance = MakeProvenance(),
    };

    private static InternalVectorBosonSourceReadinessCampaignSpec MakeReadinessSpec() => new()
    {
        CampaignId = "phase21-readiness-test",
        SchemaVersion = "1.0.0",
        BranchVariantIds = ["branch-a", "branch-b"],
        RefinementLevels = ["L0", "L1"],
        EnvironmentIds = ["env-a", "env-b"],
        SourceQuantityIds = ["massLikeValue"],
        ReadinessPolicy = new InternalVectorBosonSourceReadinessPolicy
        {
            PolicyId = "test-policy",
            MinimumClaimClass = "C2_BranchStableCandidate",
            MinimumBranchStabilityScore = 0.5,
            MinimumRefinementStabilityScore = 0.5,
            MaximumAmbiguityCount = 0,
            RequireBranchSelectors = true,
            RequireRefinementCoverage = true,
            RequireEnvironmentSelectors = true,
            RequireCompleteUncertainty = true,
            AllowedClaimClasses = ["C2_BranchStableCandidate"],
        },
        IdentityScope = "identity-neutral-vector-boson-source-candidates",
        Provenance = MakeProvenance(),
    };

    private static InternalVectorBosonSourceCandidateTable MakeSeedTable() => new()
    {
        TableId = "seed",
        SchemaVersion = "1.0.0",
        TerminalStatus = "source-blocked",
        Candidates =
        [
            MakeSeedCandidate("seed-a", 1.0),
            MakeSeedCandidate("seed-b", 1.2),
        ],
        SummaryBlockers = [],
        Provenance = MakeProvenance(),
    };

    private static InternalVectorBosonSourceCandidate MakeSeedCandidate(string id, double value) => new()
    {
        SourceCandidateId = id,
        SourceOrigin = InternalVectorBosonSourceCandidateAdapter.SourceOrigin,
        ModeRole = InternalVectorBosonSourceCandidateAdapter.ModeRole,
        SourceArtifactPaths = ["studies/phase21/source_candidates.json"],
        SourceModeIds = [$"{id}-mode"],
        SourceFamilyId = $"{id}-family",
        MassLikeValue = value,
        Uncertainty = new QuantitativeUncertainty { ExtractionError = 0.001 },
        BranchSelectors = [],
        EnvironmentSelectors = ["env-source"],
        RefinementLevels = [],
        BranchStabilityScore = 0.3,
        RefinementStabilityScore = 1.0,
        BackendStabilityScore = 1.0,
        ObservationStabilityScore = 1.0,
        AmbiguityCount = 1,
        GaugeLeakEnvelope = [0, 0, 0],
        ClaimClass = "C0_NumericalMode",
        Status = "source-blocked",
        Assumptions = ["identity-neutral source candidate"],
        ClosureRequirements = [],
        Provenance = MakeProvenance(),
    };

    private static object MakeBundleManifest()
    {
        var bundles = new List<object>();
        foreach (var branch in new[] { "branch-a", "branch-b" })
        foreach (var refinement in new[] { "L0", "L1" })
        foreach (var environment in new[] { "env-a", "env-b" })
        {
            var id = $"bundle-{branch}-{refinement}-{environment}";
            bundles.Add(new
            {
                bundleId = id,
                branchVariantId = branch,
                refinementLevel = refinement,
                environmentId = environment,
                written = true,
                backgroundRecordPath = $"bundles/{id}/background.json",
            });
        }

        return new { bundles };
    }

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase22-selector-source-spectra", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gu-p22-{Guid.NewGuid():N}");

        public TempDir() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
