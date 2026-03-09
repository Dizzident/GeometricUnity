using System.Text.Json;
using Gu.Core;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Semantics.Tests;

public class BranchFieldLayoutTests
{
    [Fact]
    public void CreatePhase1Compatible_HasSingleConnectionBlock()
    {
        var layout = BranchFieldLayout.CreatePhase1Compatible();
        Assert.Single(layout.ConnectionBlocks);
        Assert.Equal("omega", layout.ConnectionBlocks[0].BlockId);
        Assert.Equal("connection", layout.ConnectionBlocks[0].FieldType);
    }

    [Fact]
    public void CreatePhase1Compatible_HasNoAuxiliaryBlocks()
    {
        var layout = BranchFieldLayout.CreatePhase1Compatible();
        Assert.Empty(layout.AuxiliaryBosonicBlocks);
    }

    [Fact]
    public void CreatePhase1Compatible_CustomBlockId()
    {
        var layout = BranchFieldLayout.CreatePhase1Compatible("A");
        Assert.Equal("A", layout.ConnectionBlocks[0].BlockId);
        Assert.Contains("A", layout.GaugeActionRules[0]);
        Assert.Contains("A", layout.ObservationEligibility);
    }

    [Fact]
    public void CreatePhase1Compatible_HasSignature()
    {
        var layout = BranchFieldLayout.CreatePhase1Compatible();
        var sig = layout.ConnectionBlocks[0].Signature;
        Assert.Equal("Y_h", sig.AmbientSpaceId);
        Assert.Equal("connection-1form", sig.CarrierType);
    }

    [Fact]
    public void TwoBlockLayout_CanBeConstructed()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "edge-major",
            MemoryLayout = "dense-row-major",
        };

        var layout = new BranchFieldLayout
        {
            ConnectionBlocks = new[]
            {
                new FieldBlockDescriptor { BlockId = "A", FieldType = "connection", DofCount = 30, Signature = sig },
            },
            AuxiliaryBosonicBlocks = new[]
            {
                new FieldBlockDescriptor { BlockId = "omega-aux", FieldType = "auxiliary-scalar", DofCount = 10, Signature = sig },
            },
            GaugeActionRules = new[] { "full-gauge-on-A", "inert-on-omega-aux" },
            ObservationEligibility = new[] { "A", "omega-aux" },
        };

        Assert.Single(layout.ConnectionBlocks);
        Assert.Single(layout.AuxiliaryBosonicBlocks);
        Assert.Equal(2, layout.GaugeActionRules.Count);
    }
}

public class EquivalenceSpecTests
{
    [Fact]
    public void CanConstructWithAllFields()
    {
        var spec = new EquivalenceSpec
        {
            Id = "eq-1",
            Name = "Gauge equivalence",
            ComparedObjectClasses = new[] { "observed-output" },
            NormalizationProcedure = "gauge-normalize",
            AllowedTransformations = new[] { "gauge-transform" },
            Metrics = new[] { "l2-norm" },
            Tolerances = new Dictionary<string, double> { ["l2-norm"] = 1e-6 },
            InterpretationRule = "equivalent-if-within-tolerance",
        };

        Assert.Equal("eq-1", spec.Id);
        Assert.Single(spec.Tolerances);
        Assert.Equal(1e-6, spec.Tolerances["l2-norm"]);
    }
}

public class CanonicityEvidenceRecordTests
{
    [Fact]
    public void CanConstructWithRequiredFields()
    {
        var record = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-1",
            StudyId = "study-1",
            Verdict = "consistent",
            MaxObservedDeviation = 1e-8,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        Assert.Equal("ev-1", record.EvidenceId);
        Assert.Equal("consistent", record.Verdict);
        Assert.True(record.MaxObservedDeviation < record.Tolerance);
    }

    [Fact]
    public void InconsistentVerdict_DeviationExceedsTolerance()
    {
        var record = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-2",
            StudyId = "study-2",
            Verdict = "inconsistent",
            MaxObservedDeviation = 0.5,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        Assert.Equal("inconsistent", record.Verdict);
        Assert.True(record.MaxObservedDeviation > record.Tolerance);
    }
}

public class ClaimClassTests
{
    [Fact]
    public void AllValuesExist()
    {
        Assert.Equal(5, Enum.GetValues<ClaimClass>().Length);
    }
}

public class ComparisonModeTests
{
    [Fact]
    public void AllValuesExist()
    {
        Assert.Equal(3, Enum.GetValues<ComparisonMode>().Length);
    }
}

public class BranchVariantManifestSemanticTests
{
    [Fact]
    public void CanConstructWithAllFields()
    {
        var variant = new BranchVariantManifest
        {
            Id = "v1",
            ParentFamilyId = "fam-1",
            A0Variant = "flat-A0",
            BiConnectionVariant = "simple-a0-omega",
            TorsionVariant = "local-algebraic",
            ShiabVariant = "identity-shiab",
            ObservationVariant = "sigma-pullback",
            ExtractionVariant = "standard-extraction",
            GaugeVariant = "penalty",
            RegularityVariant = "standard-regularity",
            PairingVariant = "pairing-trace",
            ExpectedClaimCeiling = "branch-local-numerical",
        };

        Assert.Equal("v1", variant.Id);
        Assert.Null(variant.Notes);
    }
}

public class BranchFamilyManifestSemanticTests
{
    [Fact]
    public void CanConstructWithVariants()
    {
        var variant = new BranchVariantManifest
        {
            Id = "v1",
            ParentFamilyId = "fam-1",
            A0Variant = "flat-A0",
            BiConnectionVariant = "simple-a0-omega",
            TorsionVariant = "local-algebraic",
            ShiabVariant = "identity-shiab",
            ObservationVariant = "sigma-pullback",
            ExtractionVariant = "standard-extraction",
            GaugeVariant = "penalty",
            RegularityVariant = "standard-regularity",
            PairingVariant = "pairing-trace",
            ExpectedClaimCeiling = "branch-local-numerical",
        };

        var equivalence = new EquivalenceSpec
        {
            Id = "eq-default",
            Name = "Default",
            ComparedObjectClasses = new[] { "observed-output" },
            NormalizationProcedure = "none",
            AllowedTransformations = Array.Empty<string>(),
            Metrics = new[] { "l2-norm" },
            Tolerances = new Dictionary<string, double> { ["l2-norm"] = 1e-6 },
            InterpretationRule = "equivalent-if-within-tolerance",
        };

        var family = new BranchFamilyManifest
        {
            FamilyId = "fam-1",
            Description = "Test family",
            Variants = new[] { variant },
            DefaultEquivalence = equivalence,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal("fam-1", family.FamilyId);
        Assert.Single(family.Variants);
    }
}

public class RecoveryStudySpecTests
{
    [Fact]
    public void CanConstructWithAllFields()
    {
        var spec = new RecoveryStudySpec
        {
            StudyId = "recovery-1",
            SweepResultId = "sweep-result-1",
            RecoveryGraphId = "graph-1",
            EnforceIdentificationGate = true,
            MaxAllowedClaimClass = "numerical-only",
        };

        Assert.Equal("recovery-1", spec.StudyId);
        Assert.Equal("sweep-result-1", spec.SweepResultId);
        Assert.Equal("graph-1", spec.RecoveryGraphId);
        Assert.True(spec.EnforceIdentificationGate);
        Assert.Equal("numerical-only", spec.MaxAllowedClaimClass);
    }

    [Fact]
    public void JsonRoundTrip_PreservesAllFields()
    {
        var original = new RecoveryStudySpec
        {
            StudyId = "recovery-rt",
            SweepResultId = "sweep-42",
            RecoveryGraphId = "dag-main",
            EnforceIdentificationGate = false,
            MaxAllowedClaimClass = "branch-local-numerical",
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<RecoveryStudySpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.StudyId, deserialized.StudyId);
        Assert.Equal(original.SweepResultId, deserialized.SweepResultId);
        Assert.Equal(original.RecoveryGraphId, deserialized.RecoveryGraphId);
        Assert.Equal(original.EnforceIdentificationGate, deserialized.EnforceIdentificationGate);
        Assert.Equal(original.MaxAllowedClaimClass, deserialized.MaxAllowedClaimClass);
    }

    [Fact]
    public void JsonPropertyNames_AreCorrect()
    {
        var spec = new RecoveryStudySpec
        {
            StudyId = "s1",
            SweepResultId = "sr1",
            RecoveryGraphId = "rg1",
            EnforceIdentificationGate = true,
            MaxAllowedClaimClass = "numerical-only",
        };

        var json = JsonSerializer.Serialize(spec);
        Assert.Contains("\"studyId\"", json);
        Assert.Contains("\"sweepResultId\"", json);
        Assert.Contains("\"recoveryGraphId\"", json);
        Assert.Contains("\"enforceIdentificationGate\"", json);
        Assert.Contains("\"maxAllowedClaimClass\"", json);
    }

    [Fact]
    public void ResearchBatchSpec_IncludesRecoveryStudies()
    {
        var recoverySpec = new RecoveryStudySpec
        {
            StudyId = "recovery-batch",
            SweepResultId = "sweep-1",
            RecoveryGraphId = "graph-1",
            EnforceIdentificationGate = true,
            MaxAllowedClaimClass = "numerical-only",
        };

        var batchSpec = new ResearchBatchSpec
        {
            BatchId = "batch-with-recovery",
            Sweeps = [],
            StabilityStudies = [],
            RecoveryStudies = [recoverySpec],
            ComparisonCampaignIds = [],
        };

        Assert.Single(batchSpec.RecoveryStudies);
        Assert.Equal("recovery-batch", batchSpec.RecoveryStudies[0].StudyId);
    }
}
