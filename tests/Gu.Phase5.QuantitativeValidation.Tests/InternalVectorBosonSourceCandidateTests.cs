using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class InternalVectorBosonSourceCandidateTests
{
    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase20-source-candidates", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    [Fact]
    public void InternalVectorBosonSourceCandidate_JsonRoundTrip_PreservesBlockedStatus()
    {
        var table = new InternalVectorBosonSourceCandidateTable
        {
            TableId = "sources",
            SchemaVersion = "1.0.0",
            TerminalStatus = "source-blocked",
            Candidates =
            [
                new InternalVectorBosonSourceCandidate
                {
                    SourceCandidateId = "phase12-candidate-0",
                    SourceOrigin = InternalVectorBosonSourceCandidateAdapter.SourceOrigin,
                    ModeRole = InternalVectorBosonSourceCandidateAdapter.ModeRole,
                    SourceArtifactPaths = ["studies/phase12/example.json"],
                    SourceModeIds = ["mode-0"],
                    SourceFamilyId = "family-0",
                    MassLikeValue = 1.0e-6,
                    Uncertainty = new QuantitativeUncertainty { ExtractionError = 1.0e-7 },
                    BranchSelectors = [],
                    EnvironmentSelectors = ["bg-a"],
                    RefinementLevels = [],
                    BranchStabilityScore = 0.3,
                    RefinementStabilityScore = 1.0,
                    BackendStabilityScore = 1.0,
                    ObservationStabilityScore = 1.0,
                    AmbiguityCount = 1,
                    GaugeLeakEnvelope = [0.0, 0.0, 0.0],
                    ClaimClass = "C0_NumericalMode",
                    Status = "source-blocked",
                    Assumptions = ["identity-neutral source"],
                    ClosureRequirements = ["branch selectors are missing"],
                    Provenance = MakeProvenance(),
                },
            ],
            SummaryBlockers = ["no ready source"],
            Provenance = MakeProvenance(),
        };

        var json = GuJsonDefaults.Serialize(table);
        var roundTrip = GuJsonDefaults.Deserialize<InternalVectorBosonSourceCandidateTable>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("source-blocked", roundTrip!.TerminalStatus);
        Assert.Equal("phase12-candidate-0", roundTrip.Candidates[0].SourceCandidateId);
        Assert.Equal("source-blocked", roundTrip.Candidates[0].Status);
    }

    [Fact]
    public void GenerateFromPhase12_CheckedInArtifacts_ProducesSourceBlockedIdentityNeutralCandidates()
    {
        var repoRoot = FindRepoRoot();
        var registry = Path.Combine(repoRoot, "studies", "phase12_joined_calculation_001", "output", "background_family", "bosons", "registry.json");
        var families = Path.Combine(repoRoot, "studies", "phase12_joined_calculation_001", "output", "background_family", "modes", "mode_families.json");
        var spectraRoot = Path.Combine(repoRoot, "studies", "phase12_joined_calculation_001", "output", "background_family", "spectra");

        var table = InternalVectorBosonSourceCandidateAdapter.GenerateFromPhase12(
            registry,
            families,
            spectraRoot,
            MakeProvenance());

        Assert.Equal("source-blocked", table.TerminalStatus);
        Assert.NotEmpty(table.Candidates);
        Assert.All(table.Candidates, candidate =>
        {
            Assert.Equal(InternalVectorBosonSourceCandidateAdapter.SourceOrigin, candidate.SourceOrigin);
            Assert.Equal(InternalVectorBosonSourceCandidateAdapter.ModeRole, candidate.ModeRole);
            Assert.NotEqual("w-boson", candidate.SourceCandidateId);
            Assert.NotEqual("z-boson", candidate.SourceCandidateId);
            Assert.Equal("source-blocked", candidate.Status);
            Assert.NotEmpty(candidate.ClosureRequirements);
            Assert.DoesNotContain(candidate.SourceArtifactPaths, p => p.Contains("physical_targets", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void GenerateFromPhase12_ExternalTargetPath_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            InternalVectorBosonSourceCandidateAdapter.GenerateFromPhase12(
                "studies/phase19_dimensionless_wz_candidate_001/physical_targets.json",
                "families.json",
                "spectra",
                MakeProvenance()));

        Assert.Contains("external target", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BlockedSourceCandidate_BridgesToBlockedCandidateModeExtraction()
    {
        var candidate = new InternalVectorBosonSourceCandidate
        {
            SourceCandidateId = "phase12-candidate-0",
            SourceOrigin = InternalVectorBosonSourceCandidateAdapter.SourceOrigin,
            ModeRole = InternalVectorBosonSourceCandidateAdapter.ModeRole,
            SourceArtifactPaths = ["studies/phase12/example.json"],
            SourceModeIds = ["mode-0"],
            SourceFamilyId = "family-0",
            MassLikeValue = 1.0e-6,
            Uncertainty = new QuantitativeUncertainty { ExtractionError = 1.0e-7 },
            BranchSelectors = [],
            EnvironmentSelectors = ["bg-a"],
            RefinementLevels = [],
            Status = "source-blocked",
            Assumptions = ["identity-neutral source"],
            ClosureRequirements = ["source uncertainty budget is incomplete"],
            Provenance = MakeProvenance(),
        };
        var source = InternalVectorBosonSourceCandidateAdapter.ToCandidateModeSourceRecord(candidate, MakeProvenance());

        var extraction = CandidateModeExtractor.ExtractCandidateMode(
            source,
            extractionId: "extract-source",
            modeId: "candidate-mode",
            particleId: "identity-neutral-vector-boson-candidate",
            modeKind: "vector-boson-mass-mode",
            provenance: MakeProvenance());

        Assert.Equal("blocked", extraction.Status);
        Assert.Null(extraction.CandidateMode);
        Assert.Contains(extraction.ClosureRequirements, r => r.Contains("uncertainty", StringComparison.OrdinalIgnoreCase));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "studies")) &&
                Directory.Exists(Path.Combine(current.FullName, "src")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
