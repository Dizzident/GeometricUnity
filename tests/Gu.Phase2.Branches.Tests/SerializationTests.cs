using Gu.Core.Serialization;
using Gu.Phase2.Branches;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches.Tests;

public class SerializationTests
{
    [Fact]
    public void BranchVariantManifest_RoundTrips()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var json = GuJsonDefaults.Serialize(variant);
        var deserialized = GuJsonDefaults.Deserialize<BranchVariantManifest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("v1", deserialized!.Id);
        Assert.Equal("fam-1", deserialized.ParentFamilyId);
        Assert.Equal("local-algebraic", deserialized.TorsionVariant);
        Assert.Equal("identity-shiab", deserialized.ShiabVariant);
    }

    [Fact]
    public void BranchFamilyManifest_RoundTrips()
    {
        var family = BranchVariantManifestTests.MakeFamily("fam-1",
            BranchVariantManifestTests.MakeVariant("v1"),
            BranchVariantManifestTests.MakeVariant("v2"));

        var json = GuJsonDefaults.Serialize(family);
        var deserialized = GuJsonDefaults.Deserialize<BranchFamilyManifest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("fam-1", deserialized!.FamilyId);
        Assert.Equal(2, deserialized.Variants.Count);
        Assert.NotNull(deserialized.DefaultEquivalence);
    }

    [Fact]
    public void EquivalenceSpec_RoundTrips()
    {
        var spec = BranchVariantManifestTests.MakeEquivalence();

        var json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<EquivalenceSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("eq-default", deserialized!.Id);
        Assert.Equal(1e-6, deserialized.Tolerances["l2-norm"]);
    }

    [Fact]
    public void CanonicityDocket_RoundTrips()
    {
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-equivalence", "identity-shiab,covariant-shiab");

        var json = GuJsonDefaults.Serialize(docket);
        var deserialized = GuJsonDefaults.Deserialize<CanonicityDocket>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("shiab", deserialized!.ObjectClass);
        Assert.Equal(DocketStatus.Open, deserialized.Status);
    }

    [Fact]
    public void CanonicityEvidenceRecord_RoundTrips()
    {
        var record = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-1",
            StudyId = "study-1",
            Verdict = "consistent",
            MaxObservedDeviation = 0.01,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<CanonicityEvidenceRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("ev-1", deserialized!.EvidenceId);
        Assert.Equal(0.01, deserialized.MaxObservedDeviation);
    }

    [Fact]
    public void BranchFieldLayout_RoundTrips()
    {
        var layout = BranchFieldLayout.CreatePhase1Compatible();

        var json = GuJsonDefaults.Serialize(layout);
        var deserialized = GuJsonDefaults.Deserialize<BranchFieldLayout>(json);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized!.ConnectionBlocks);
        Assert.Equal("omega", deserialized.ConnectionBlocks[0].BlockId);
        Assert.Equal("connection", deserialized.ConnectionBlocks[0].FieldType);
    }
}
