using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase4.Registry;
using Gu.Phase5.Environments;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Falsification.Tests;

public sealed class DerivedSidecarGeneratorTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 3, 16, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "derived-sidecar-test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu",
    };

    private static UnifiedParticleRegistry MakeRegistry()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(new UnifiedParticleRecord
        {
            ParticleId = "cand-001",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "cluster-001",
            ContributingSourceIds = ["ferm-family-001"],
            BranchVariantSet = [],
            BackgroundSet = [],
            Chirality = "mixed",
            MassLikeEnvelope = [1.0, 1.2, 1.4],
            BranchStabilityScore = 0.95,
            ObservationConfidence = 0.4,
            ComparisonEvidenceScore = 0.3,
            ClaimClass = "C3_BranchStableCandidate",
            RegistryVersion = "1.0.0",
            Provenance = MakeProvenance(),
        });
        return registry;
    }

    private static IReadOnlyList<QuantitativeObservableRecord> MakeObservables() =>
    [
        new QuantitativeObservableRecord
        {
            ObservableId = "obs-001",
            Value = 1.0,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = 0.02,
                RefinementError = 0.02,
                ExtractionError = 0.01,
                EnvironmentSensitivity = 0.01,
                TotalUncertainty = 0.03,
            },
            BranchId = "V1",
            EnvironmentId = "env-toy",
            ExtractionMethod = "ratio",
            Provenance = MakeProvenance(),
        },
        new QuantitativeObservableRecord
        {
            ObservableId = "obs-001",
            Value = 1.08,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = 0.02,
                RefinementError = 0.02,
                ExtractionError = 0.01,
                EnvironmentSensitivity = 0.01,
                TotalUncertainty = 0.03,
            },
            BranchId = "V1",
            EnvironmentId = "env-structured",
            ExtractionMethod = "ratio",
            Provenance = MakeProvenance(),
        },
    ];

    private static IReadOnlyList<EnvironmentRecord> MakeEnvironmentRecords() =>
    [
        new EnvironmentRecord
        {
            EnvironmentId = "env-toy",
            GeometryTier = "toy",
            GeometryFingerprint = "toy-001",
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = 4,
            FaceCount = 2,
            Admissibility = new EnvironmentAdmissibilityReport
            {
                Level = "toy",
                Checks = [],
                Passed = true,
            },
            Provenance = MakeProvenance(),
        },
        new EnvironmentRecord
        {
            EnvironmentId = "env-structured",
            GeometryTier = "structured",
            GeometryFingerprint = "structured-001",
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = 16,
            FaceCount = 8,
            Admissibility = new EnvironmentAdmissibilityReport
            {
                Level = "structured",
                Checks = [],
                Passed = true,
            },
            Provenance = MakeProvenance(),
        },
    ];

    [Fact]
    public void GenerateSidecars_WithoutExplicitRecords_DerivesEvaluatedChannels()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), $"derived-sidecars-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmpDir);
        try
        {
            var summary = SidecarGenerator.GenerateSidecars(
                MakeRegistry(),
                MakeObservables(),
                MakeEnvironmentRecords(),
                tmpDir,
                "derived-sidecars",
                MakeProvenance());

            Assert.All(summary.Channels, ch => Assert.Equal("evaluated", ch.Status));

            var observationRecords = GuJsonDefaults.Deserialize<List<ObservationChainRecord>>(
                File.ReadAllText(Path.Combine(tmpDir, "observation_chain.json")));
            var envVarianceRecords = GuJsonDefaults.Deserialize<List<EnvironmentVarianceRecord>>(
                File.ReadAllText(Path.Combine(tmpDir, "environment_variance.json")));
            var representationRecords = GuJsonDefaults.Deserialize<List<RepresentationContentRecord>>(
                File.ReadAllText(Path.Combine(tmpDir, "representation_content.json")));
            var couplingRecords = GuJsonDefaults.Deserialize<List<CouplingConsistencyRecord>>(
                File.ReadAllText(Path.Combine(tmpDir, "coupling_consistency.json")));

            Assert.NotNull(observationRecords);
            Assert.Single(observationRecords);
            Assert.NotNull(envVarianceRecords);
            Assert.Single(envVarianceRecords);
            Assert.NotNull(representationRecords);
            Assert.Single(representationRecords);
            Assert.NotNull(couplingRecords);
            Assert.Single(couplingRecords);
            Assert.Equal(1, representationRecords[0].MissingRequiredCount);
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);
        }
    }
}
