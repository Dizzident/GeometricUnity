using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Execution;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.GoldenArtifacts;

/// <summary>
/// Golden artifact regression tests.
/// Serializes known-good results and verifies deserialization reproduces
/// identical values. Catches serialization schema changes and silent regressions.
/// Per IMPLEMENTATION_PLAN_P2.md Section 10.4 and GAP-14.
/// </summary>
public class GoldenArtifactTests
{
    [Fact]
    public void CanonicityEvidenceRecord_RoundTrip_PreservesAllFields()
    {
        var evidence = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-golden-1",
            StudyId = "sweep-env-2d",
            Verdict = "consistent",
            MaxObservedDeviation = 1.5e-9,
            Tolerance = 1e-6,
            Timestamp = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero),
        };

        var json = GuJsonDefaults.Serialize(evidence);
        var deserialized = GuJsonDefaults.Deserialize<CanonicityEvidenceRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("ev-golden-1", deserialized!.EvidenceId);
        Assert.Equal("sweep-env-2d", deserialized.StudyId);
        Assert.Equal("consistent", deserialized.Verdict);
        Assert.Equal(1.5e-9, deserialized.MaxObservedDeviation, precision: 15);
        Assert.Equal(1e-6, deserialized.Tolerance);
    }

    [Fact]
    public void PairwiseDistanceMatrix_Construction_PreservesDistancesAndSymmetry()
    {
        // System.Text.Json does not support double[,] serialization,
        // so we verify construction and computed properties directly.
        var matrix = new PairwiseDistanceMatrix
        {
            MetricId = "D_obs_max",
            BranchIds = new[] { "v1", "v2", "v3" },
            Distances = new double[,]
            {
                { 0.0, 1.5e-9, 2.3e-8 },
                { 1.5e-9, 0.0, 3.1e-8 },
                { 2.3e-8, 3.1e-8, 0.0 },
            },
        };

        Assert.Equal("D_obs_max", matrix.MetricId);
        Assert.Equal(3, matrix.BranchIds.Count);
        Assert.Equal(0.0, matrix.Distances[0, 0]);
        Assert.Equal(1.5e-9, matrix.Distances[0, 1], precision: 15);
        // Symmetric: D[0,2] == D[2,0]
        Assert.Equal(matrix.Distances[0, 2], matrix.Distances[2, 0]);
        Assert.Equal(3.1e-8, matrix.MaxDistance, precision: 15);
    }

    [Fact]
    public void FailureModeMatrix_Construction_PreservesNullModes()
    {
        // System.Text.Json does not support bool[,] serialization,
        // so we verify construction directly.
        var matrix = new FailureModeMatrix
        {
            BranchIds = new[] { "v1", "v2" },
            PrimaryFailureModes = new string?[] { null, "solver-diverged" },
            SameFailureMode = new bool[,]
            {
                { true, false },
                { false, true },
            },
        };

        Assert.Null(matrix.PrimaryFailureModes[0]);
        Assert.Equal("solver-diverged", matrix.PrimaryFailureModes[1]);
        Assert.True(matrix.SameFailureMode[0, 0]);
        Assert.False(matrix.SameFailureMode[0, 1]);
        Assert.False(matrix.SameFailureMode[1, 0]);
        Assert.True(matrix.SameFailureMode[1, 1]);
    }

    [Fact]
    public void CanonicityDocket_RoundTrip_PreservesStatus()
    {
        var docket = new CanonicityDocket
        {
            ObjectClass = "shiab",
            ActiveRepresentative = "identity-shiab",
            EquivalenceRelationId = "output-eq",
            AdmissibleComparisonClass = "identity-shiab",
            DownstreamClaimsBlockedUntilClosure = new[] { "claim-1" },
            CurrentEvidence = new[]
            {
                new CanonicityEvidenceRecord
                {
                    EvidenceId = "ev-1",
                    StudyId = "study-1",
                    Verdict = "consistent",
                    MaxObservedDeviation = 1e-9,
                    Tolerance = 1e-6,
                    Timestamp = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero),
                },
            },
            KnownCounterexamples = Array.Empty<string>(),
            PendingTheorems = new[] { "ellipticity-conjecture" },
            StudyReports = new[] { "study-1" },
            Status = DocketStatus.EvidenceAccumulating,
        };

        var json = GuJsonDefaults.Serialize(docket);
        var deserialized = GuJsonDefaults.Deserialize<CanonicityDocket>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("shiab", deserialized!.ObjectClass);
        Assert.Equal(DocketStatus.EvidenceAccumulating, deserialized.Status);
        Assert.Single(deserialized.CurrentEvidence);
        Assert.Equal("consistent", deserialized.CurrentEvidence[0].Verdict);
        Assert.Single(deserialized.PendingTheorems);
    }

    [Fact]
    public void BranchRunRecord_RoundTrip_PreservesConvergenceData()
    {
        var variant = new BranchVariantManifest
        {
            Id = "v-golden",
            ParentFamilyId = "fam-1",
            A0Variant = "zero",
            BiConnectionVariant = "simple",
            TorsionVariant = "augmented",
            ShiabVariant = "identity",
            ObservationVariant = "default",
            ExtractionVariant = "default",
            GaugeVariant = "coulomb",
            RegularityVariant = "smooth",
            PairingVariant = "trace",
            ExpectedClaimCeiling = "branch-local-numerical",
        };

        var manifest = new BranchManifest
        {
            BranchId = "v-golden",
            SchemaVersion = "1.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-2d",
            ActiveObservationBranch = "default",
            ActiveTorsionBranch = "augmented",
            ActiveShiabBranch = "identity",
            ActiveGaugeStrategy = "coulomb",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "canonical",
            ComponentOrderId = "edge-major",
            AdjointConventionId = "default",
            PairingConventionId = "trace",
            NormConventionId = "l2",
            DifferentialFormMetricId = "euclidean",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };

        var record = new BranchRunRecord
        {
            Variant = variant,
            Manifest = manifest,
            Converged = true,
            TerminationReason = "converged",
            FinalObjective = 1.23e-10,
            FinalResidualNorm = 4.56e-9,
            Iterations = 42,
            SolveMode = SolveMode.ObjectiveMinimization,
            ExtractionSucceeded = true,
            ComparisonAdmissible = true,
            ArtifactBundle = new ArtifactBundle
            {
                ArtifactId = "art-golden",
                Branch = new BranchRef { BranchId = "v-golden", SchemaVersion = "1.0" },
                ReplayContract = new ReplayContract
                {
                    BranchManifest = manifest,
                    Deterministic = true,
                    BackendId = "cpu-reference",
                    ReplayTier = "R2",
                },
                Provenance = new ProvenanceMeta
                {
                    CreatedAt = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero),
                    CodeRevision = "abc123",
                    Branch = new BranchRef { BranchId = "v-golden", SchemaVersion = "1.0" },
                },
                CreatedAt = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero),
            },
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BranchRunRecord>(json);

        Assert.NotNull(deserialized);
        Assert.True(deserialized!.Converged);
        Assert.Equal(1.23e-10, deserialized.FinalObjective, precision: 15);
        Assert.Equal(4.56e-9, deserialized.FinalResidualNorm, precision: 15);
        Assert.Equal(42, deserialized.Iterations);
        Assert.True(deserialized.ExtractionSucceeded);
        Assert.True(deserialized.ComparisonAdmissible);
    }
}
