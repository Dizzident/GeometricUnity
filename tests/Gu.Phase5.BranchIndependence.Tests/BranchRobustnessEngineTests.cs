using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.BranchIndependence;

namespace Gu.Phase5.BranchIndependence.Tests;

/// <summary>
/// Tests for M46: BranchRobustnessEngine, distance matrices, equivalence classes,
/// fragility scoring, and invariance candidates.
/// </summary>
public sealed class BranchRobustnessEngineTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-rev",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "reference-cpu",
    };

    private static BranchRobustnessStudySpec MakeSpec(
        double absTol = 1e-4,
        double relTol = 1e-3) => new()
    {
        StudyId = "test-study-001",
        BranchVariantIds = new List<string> { "branch-A", "branch-B", "branch-C" },
        TargetQuantityIds = new List<string> { "eigenvalue-0", "eigenvalue-1" },
        AbsoluteTolerance = absTol,
        RelativeTolerance = relTol,
    };

    // ── Distance matrix ─────────────────────────────────────────────────────────

    [Fact]
    public void DistanceMatrix_Symmetric_DiagonalZero()
    {
        var vals = new double[] { 1.0, 2.0, 3.0 };
        var matrix = BranchDistanceMatrix.Build(
            "eigenvalue-0",
            new List<string> { "A", "B", "C" },
            vals);

        for (int i = 0; i < 3; i++)
            Assert.Equal(0.0, matrix.GetDistance(i, i));

        // symmetric
        Assert.Equal(matrix.GetDistance(0, 1), matrix.GetDistance(1, 0));
        Assert.Equal(matrix.GetDistance(0, 2), matrix.GetDistance(2, 0));
    }

    [Fact]
    public void DistanceMatrix_MaxDistance_IsLargestPairwiseDiff()
    {
        var vals = new double[] { 0.0, 1.0, 5.0 };
        var matrix = BranchDistanceMatrix.Build(
            "q",
            new List<string> { "A", "B", "C" },
            vals);

        Assert.Equal(5.0, matrix.MaxDistance, precision: 10);
    }

    [Fact]
    public void DistanceMatrix_MeanDistance_CorrectOffDiagonalAverage()
    {
        // vals: 0, 2, 4 → distances: |0-2|=2, |0-4|=4, |2-4|=2
        // off-diagonal pairs (counting both directions): 2,4,2,4,2,2 sum=16, count=6, mean=16/6≈2.667
        var vals = new double[] { 0.0, 2.0, 4.0 };
        var matrix = BranchDistanceMatrix.Build(
            "q",
            new List<string> { "A", "B", "C" },
            vals);

        Assert.Equal(16.0 / 6.0, matrix.MeanDistance, precision: 10);
    }

    // ── Engine: invariant quantities ─────────────────────────────────────────────

    [Fact]
    public void Engine_AllBranchesIdentical_ClassifiesRobust()
    {
        var spec = MakeSpec(absTol: 1e-4);
        var engine = new BranchRobustnessEngine(spec);

        // All three branches produce the same value for both quantities
        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.0, 1.0 },
            ["eigenvalue-1"] = new double[] { 2.0, 2.0, 2.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.Equal("robust", record.OverallSummary);
        Assert.Equal(2, record.InvarianceCandidates.Count);
        Assert.All(record.FragilityRecords.Values,
            f => Assert.True(f.Classification is "invariant" or "robust"));
    }

    [Fact]
    public void Engine_BranchesVarySignificantly_ClassifiesFragile()
    {
        var spec = MakeSpec(absTol: 1e-4, relTol: 1e-3);
        var engine = new BranchRobustnessEngine(spec);

        // Large variation — eigenvalue-0 changes by 50%
        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.5, 2.0 },
            ["eigenvalue-1"] = new double[] { 2.0, 2.5, 3.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.Equal("fragile", record.OverallSummary);
        Assert.Empty(record.InvarianceCandidates);
        Assert.All(record.FragilityRecords.Values,
            f => Assert.Equal("fragile", f.Classification));
    }

    [Fact]
    public void Engine_MixedVariation_ClassifiesMixed()
    {
        var spec = MakeSpec(absTol: 1e-4, relTol: 1e-3);
        var engine = new BranchRobustnessEngine(spec);

        var values = new Dictionary<string, double[]>
        {
            // Very small variation — invariant
            ["eigenvalue-0"] = new double[] { 1.0, 1.0 + 1e-6, 1.0 + 2e-6 },
            // Large variation — fragile
            ["eigenvalue-1"] = new double[] { 1.0, 2.0, 3.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.Equal("mixed", record.OverallSummary);
        var singleCandidate = Assert.Single(record.InvarianceCandidates);
        Assert.Equal("eigenvalue-0", singleCandidate.TargetQuantityId);
    }

    // ── Engine: single variant ───────────────────────────────────────────────────

    [Fact]
    public void Engine_SingleVariant_ClassifiesInconclusive()
    {
        var spec = new BranchRobustnessStudySpec
        {
            StudyId = "single-test",
            BranchVariantIds = new List<string> { "branch-A" },
            TargetQuantityIds = new List<string> { "eigenvalue-0" },
        };
        var engine = new BranchRobustnessEngine(spec);

        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.Equal("inconclusive", record.OverallSummary);
        Assert.Contains(record.DiagnosticNotes, n => n.Contains("inconclusive") || n.Contains("fewer than 2"));
    }

    // ── Equivalence classes ──────────────────────────────────────────────────────

    [Fact]
    public void Engine_TwoCloseOneFar_ProducesTwoEquivalenceClasses()
    {
        var spec = new BranchRobustnessStudySpec
        {
            StudyId = "eq-test",
            BranchVariantIds = new List<string> { "A", "B", "C" },
            TargetQuantityIds = new List<string> { "q" },
            AbsoluteTolerance = 0.01,
            RelativeTolerance = 1e-3,
        };
        var engine = new BranchRobustnessEngine(spec);

        // A and B are close; C is far
        var values = new Dictionary<string, double[]>
        {
            ["q"] = new double[] { 1.0, 1.001, 5.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.True(record.EquivalenceClasses.ContainsKey("q"));
        var classes = record.EquivalenceClasses["q"];
        Assert.Equal(2, classes.Count); // {A,B} and {C}

        var bigClass = classes.Single(c => c.Size == 2);
        Assert.Contains("A", bigClass.MemberBranchVariantIds);
        Assert.Contains("B", bigClass.MemberBranchVariantIds);
    }

    // ── Fragility score ──────────────────────────────────────────────────────────

    [Fact]
    public void FragilityScore_IsMaxDistanceOverMeanPlusEpsilon()
    {
        var spec = MakeSpec();
        var engine = new BranchRobustnessEngine(spec);

        // values: [2.0, 3.0, 4.0]
        // pairwise |diffs|: |2-3|=1, |2-4|=2, |3-4|=1 (upper triangle)
        // NxN off-diagonal mean = (1+2+1+2+1+1)/6 = 8/6 = 4/3
        // maxDist = 2.0
        // fragilityScore = 2.0 / (4/3 + 1e-14) ≈ 1.5
        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 2.0, 3.0, 4.0 },
            ["eigenvalue-1"] = new double[] { 1.0, 1.0, 1.0 },
        };

        var record = engine.Run(values, TestProvenance());
        var fragility = record.FragilityRecords["eigenvalue-0"];

        double meanDist = 8.0 / 6.0;
        double expectedScore = 2.0 / (meanDist + 1e-14);
        Assert.Equal(expectedScore, fragility.FragilityScore, precision: 10);
        Assert.Equal("fragile", fragility.Classification); // > 0.5
    }

    [Fact]
    public void FragilityRecord_MaxDistancePair_CorrectlyIdentified()
    {
        var spec = MakeSpec();
        var engine = new BranchRobustnessEngine(spec);

        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.1, 10.0 },
            ["eigenvalue-1"] = new double[] { 1.0, 1.0, 1.0 },
        };

        var record = engine.Run(values, TestProvenance());
        var fragility = record.FragilityRecords["eigenvalue-0"];

        // Maximum distance is between branch-A (1.0) and branch-C (10.0)
        Assert.Contains("branch-A", fragility.MaxDistancePair);
        Assert.Contains("branch-C", fragility.MaxDistancePair);
    }

    // ── Invariance candidates ────────────────────────────────────────────────────

    [Fact]
    public void InvarianceCandidates_RecordCorrectMeanAndDeviation()
    {
        var spec = MakeSpec(absTol: 0.1, relTol: 0.1);
        var engine = new BranchRobustnessEngine(spec);

        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.001, 0.999 },
            ["eigenvalue-1"] = new double[] { 2.0, 5.0, 8.0 }, // fragile
        };

        var record = engine.Run(values, TestProvenance());

        var candidate = Assert.Single(record.InvarianceCandidates);
        Assert.Equal("eigenvalue-0", candidate.TargetQuantityId);
        Assert.Equal(3, candidate.BranchFamilySize);
        Assert.Equal(1.0, candidate.MeanValue, precision: 2);
        Assert.NotEmpty(candidate.InvarianceNote);
    }

    // ── Missing quantity ─────────────────────────────────────────────────────────

    [Fact]
    public void Engine_MissingQuantity_RecordsDiagnosticNote()
    {
        var spec = MakeSpec();
        var engine = new BranchRobustnessEngine(spec);

        // Only supply eigenvalue-0; eigenvalue-1 is missing
        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.0, 1.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.Contains(record.DiagnosticNotes, n => n.Contains("eigenvalue-1"));
        Assert.False(record.DistanceMatrices.ContainsKey("eigenvalue-1"));
    }

    // ── Record ID and study ID ───────────────────────────────────────────────────

    [Fact]
    public void Engine_RecordId_ContainsStudyId()
    {
        var spec = MakeSpec();
        var engine = new BranchRobustnessEngine(spec);

        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.0, 1.0 },
            ["eigenvalue-1"] = new double[] { 2.0, 2.0, 2.0 },
        };

        var record = engine.Run(values, TestProvenance());

        Assert.Equal("test-study-001", record.StudyId);
        Assert.Contains("test-study-001", record.RecordId);
    }

    // ── Serialization round-trip ─────────────────────────────────────────────────

    [Fact]
    public void BranchRobustnessRecord_SerializesAndDeserializes()
    {
        var spec = MakeSpec();
        var engine = new BranchRobustnessEngine(spec);

        var values = new Dictionary<string, double[]>
        {
            ["eigenvalue-0"] = new double[] { 1.0, 1.5, 2.0 },
            ["eigenvalue-1"] = new double[] { 3.0, 3.0, 3.0 },
        };

        var record = engine.Run(values, TestProvenance());

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BranchRobustnessRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(record.StudyId, deserialized!.StudyId);
        Assert.Equal(record.OverallSummary, deserialized.OverallSummary);
        Assert.Equal(record.BranchVariantIds.Count, deserialized.BranchVariantIds.Count);
        Assert.Equal(record.InvarianceCandidates.Count, deserialized.InvarianceCandidates.Count);
    }

    [Fact]
    public void BranchRobustnessStudySpec_SerializesAndDeserializes()
    {
        var spec = MakeSpec();
        var json = GuJsonDefaults.Serialize(spec);
        var roundtrip = GuJsonDefaults.Deserialize<BranchRobustnessStudySpec>(json);

        Assert.NotNull(roundtrip);
        Assert.Equal(spec.StudyId, roundtrip!.StudyId);
        Assert.Equal(spec.BranchVariantIds, roundtrip.BranchVariantIds);
        Assert.Equal(spec.TargetQuantityIds, roundtrip.TargetQuantityIds);
    }

    [Fact]
    public void InvarianceCandidateRecord_SerializesAndDeserializes()
    {
        var candidate = new InvarianceCandidateRecord
        {
            TargetQuantityId = "eigenvalue-0",
            BranchFamilySize = 4,
            MeanValue = 1.5,
            MaxAbsDeviation = 0.001,
            FragilityScore = 0.0005,
            AbsoluteTolerance = 1e-4,
            RelativeTolerance = 1e-3,
            SourceStudyId = "study-001",
        };

        var json = GuJsonDefaults.Serialize(candidate);
        var roundtrip = GuJsonDefaults.Deserialize<InvarianceCandidateRecord>(json);

        Assert.NotNull(roundtrip);
        Assert.Equal("eigenvalue-0", roundtrip!.TargetQuantityId);
        Assert.Equal(4, roundtrip.BranchFamilySize);
        Assert.Equal(1.5, roundtrip.MeanValue);
        Assert.NotEmpty(roundtrip.InvarianceNote);
    }
}
