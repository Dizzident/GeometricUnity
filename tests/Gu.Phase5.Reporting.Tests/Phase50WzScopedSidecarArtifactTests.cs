using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Falsification;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase50WzScopedSidecarArtifactTests
{
    [Fact]
    public void Phase50CampaignSpec_UsesWzScopedSidecarsAndValidates()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/config/campaign.json");
        var specDir = Path.GetDirectoryName(specPath)!;

        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        Assert.Equal("../wz_scoped_registry.json", spec!.RegistryPath);
        Assert.Equal("../representation_content.json", spec.RepresentationContentPath);
        Assert.Equal("../observation_chain.json", spec.ObservationChainPath);
        Assert.Equal("../environment_variance.json", spec.EnvironmentVariancePath);
        Assert.Equal("../coupling_consistency.json", spec.CouplingConsistencyPath);
        Assert.DoesNotContain("phase4_fermion_family_atlas", spec.RegistryPath, StringComparison.Ordinal);
        Assert.DoesNotContain("phase5_su2_branch_refinement_env_validation", spec.RepresentationContentPath, StringComparison.Ordinal);

        var validation = Phase5CampaignSpecValidator.Validate(spec, specDir, requireReferenceSidecars: true);
        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Errors));
    }

    [Fact]
    public void Phase50RepresentationContent_ContainsOnlySelectedWzModes()
    {
        var repoRoot = FindRepoRoot();
        var records = Load<List<RepresentationContentRecord>>(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/representation_content.json");

        Assert.Equal(2, records.Count);
        Assert.Contains(records, r => r.CandidateId == "phase22-phase12-candidate-0");
        Assert.Contains(records, r => r.CandidateId == "phase22-phase12-candidate-2");
        Assert.DoesNotContain(records, r => r.CandidateId.StartsWith("fermion-registry-phase4-toy", StringComparison.Ordinal));
        Assert.All(records, r =>
        {
            Assert.True(r.Consistent);
            Assert.Equal(0, r.MissingRequiredCount);
            Assert.Equal(0.0, r.StructuralMismatchScore);
            Assert.Equal("bridge-derived", r.Origin);
        });
    }

    [Fact]
    public void Phase50ScopedSidecars_DoNotEmitToyFermionRepresentationFalsifier()
    {
        var repoRoot = FindRepoRoot();
        var observation = Load<List<ObservationChainRecord>>(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/observation_chain.json");
        var environment = Load<List<EnvironmentVarianceRecord>>(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/environment_variance.json");
        var representation = Load<List<RepresentationContentRecord>>(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/representation_content.json");
        var coupling = Load<List<CouplingConsistencyRecord>>(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/coupling_consistency.json");
        var sidecarSummary = Load<SidecarSummary>(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/config/sidecar_summary.json");

        var summary = new FalsifierEvaluator().Evaluate(
            "phase50-wz-scoped-falsification-sidecars",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: new FalsificationPolicy(),
            provenance: MakeProvenance(),
            observationRecords: observation,
            environmentVarianceRecords: environment,
            representationContentRecords: representation,
            couplingConsistencyRecords: coupling,
            sidecarSummary: sidecarSummary);

        Assert.Equal(0, summary.ActiveFatalCount);
        Assert.Equal(0, summary.ActiveHighCount);
        Assert.DoesNotContain(summary.Falsifiers, f =>
            f.TargetId == "fermion-registry-phase4-toy-v1-0000" &&
            f.FalsifierType == FalsifierTypes.RepresentationContent);
    }

    [Fact]
    public void Phase50Scope_ExplicitlyExcludesPhase4ToyFermionRegistry()
    {
        var repoRoot = FindRepoRoot();
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            repoRoot,
            "studies/phase50_wz_scoped_falsification_sidecars_001/sidecar_scope.json")));

        var excluded = doc.RootElement.GetProperty("excludedCandidateIds")
            .EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        var included = doc.RootElement.GetProperty("includedModeIds")
            .EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();

        Assert.Contains("fermion-registry-phase4-toy-v1-0000", excluded);
        Assert.Contains("phase22-phase12-candidate-0", included);
        Assert.Contains("phase22-phase12-candidate-2", included);
    }

    private static T Load<T>(string repoRoot, string relativePath)
        => GuJsonDefaults.Deserialize<T>(File.ReadAllText(Path.Combine(repoRoot, relativePath)))
           ?? throw new InvalidOperationException($"Failed to load {relativePath}.");

    private static ProvenanceMeta MakeProvenance()
        => new()
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-28T00:00:00+00:00"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "cpu",
        };

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "GeometricUnity.slnx")))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
