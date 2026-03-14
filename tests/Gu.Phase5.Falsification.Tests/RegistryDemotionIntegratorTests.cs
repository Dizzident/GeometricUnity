using Gu.Core;
using Gu.Phase4.Registry;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// Tests for RegistryDemotionIntegrator (M50).
/// </summary>
public sealed class RegistryDemotionIntegratorTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    private static UnifiedParticleRecord MakeParticle(string id, string claimClass)
        => new UnifiedParticleRecord
        {
            ParticleId = id,
            ParticleType = UnifiedParticleType.Boson,
            PrimarySourceId = $"src-{id}",
            ContributingSourceIds = [$"src-{id}"],
            BranchVariantSet = ["v1"],
            BackgroundSet = ["bg1"],
            MassLikeEnvelope = [1.0, 1.0, 1.0],
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
            Provenance = MakeProvenance(),
        };

    private static FalsifierSummary MakeSummary(string targetId, string severity, bool active = true)
    {
        var falsifier = new FalsifierRecord
        {
            FalsifierId = "f-001",
            FalsifierType = FalsifierTypes.BranchFragility,
            Severity = severity,
            TargetId = targetId,
            BranchId = "b1",
            Description = "test",
            Evidence = "test",
            Active = active,
            Provenance = MakeProvenance(),
        };
        return new FalsifierSummary
        {
            StudyId = "study-test",
            Falsifiers = [falsifier],
            ActiveFatalCount = severity == FalsifierSeverity.Fatal && active ? 1 : 0,
            ActiveHighCount = severity == FalsifierSeverity.High && active ? 1 : 0,
            TotalActiveCount = active ? 1 : 0,
            Provenance = MakeProvenance(),
        };
    }

    [Fact]
    public void ApplyDemotions_NoFalsifiers_ReturnsUnchangedRegistry()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C3_BranchStableCandidate"));

        var summary = new FalsifierSummary
        {
            StudyId = "study",
            Falsifiers = Array.Empty<FalsifierRecord>(),
            ActiveFatalCount = 0,
            ActiveHighCount = 0,
            TotalActiveCount = 0,
            Provenance = MakeProvenance(),
        };

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Same(registry, result); // no change → same instance
        Assert.Equal("C3_BranchStableCandidate", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_Fatal_CapsAtC0()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C4_ObservationConsistentCandidate"));
        var summary = MakeSummary("p1", FalsifierSeverity.Fatal);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Equal("C0_NumericalMode", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_High_DemotesBy2()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C4_ObservationConsistentCandidate"));
        var summary = MakeSummary("p1", FalsifierSeverity.High);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Equal("C2_ReproducibleMode", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_Medium_DemotesByOne()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C3_BranchStableCandidate"));
        var summary = MakeSummary("p1", FalsifierSeverity.Medium);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Equal("C2_ReproducibleMode", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_Low_NoChange()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C3_BranchStableCandidate"));
        var summary = MakeSummary("p1", FalsifierSeverity.Low);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Equal("C3_BranchStableCandidate", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_InactiveFalsifier_NoChange()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C3_BranchStableCandidate"));
        var summary = MakeSummary("p1", FalsifierSeverity.Fatal, active: false);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Equal("C3_BranchStableCandidate", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_CannotDropBelowC0()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C0_NumericalMode"));
        var summary = MakeSummary("p1", FalsifierSeverity.Fatal);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        Assert.Equal("C0_NumericalMode", result.Candidates[0].ClaimClass);
    }

    [Fact]
    public void ApplyDemotions_UnrelatedParticle_NotAffected()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C3_BranchStableCandidate"));
        registry.Register(MakeParticle("p2", "C3_BranchStableCandidate"));
        // falsifier only targets p1
        var summary = MakeSummary("p1", FalsifierSeverity.High);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        var p1 = result.Candidates.First(c => c.ParticleId == "p1");
        var p2 = result.Candidates.First(c => c.ParticleId == "p2");
        Assert.Equal("C1_NumericalHint", p1.ClaimClass);      // demoted by 2
        Assert.Equal("C3_BranchStableCandidate", p2.ClaimClass); // unchanged
    }

    [Fact]
    public void ApplyDemotions_RecordsDemotionHistory()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C3_BranchStableCandidate"));
        var summary = MakeSummary("p1", FalsifierSeverity.High);

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        var p1 = result.Candidates.First(c => c.ParticleId == "p1");
        Assert.Single(p1.Demotions);
        var demotion = p1.Demotions[0];
        Assert.Equal("C3_BranchStableCandidate", demotion.FromClaimClass);
        Assert.Equal("C1_NumericalHint", demotion.ToClaimClass);
        Assert.Contains("Phase5Falsifier", demotion.Reason);
    }

    [Fact]
    public void ApplyDemotions_MultipleFalsifiersOnSameParticle_Cumulates()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeParticle("p1", "C4_ObservationConsistentCandidate"));

        // Two High falsifiers: demote by 2, then by 2 again → C0
        var falsifiers = new[]
        {
            new FalsifierRecord
            {
                FalsifierId = "f-001", FalsifierType = FalsifierTypes.BranchFragility,
                Severity = FalsifierSeverity.High, TargetId = "p1", BranchId = "b1",
                Description = "f1", Evidence = "e1", Active = true,
                Provenance = MakeProvenance(),
            },
            new FalsifierRecord
            {
                FalsifierId = "f-002", FalsifierType = FalsifierTypes.NonConvergence,
                Severity = FalsifierSeverity.High, TargetId = "p1", BranchId = "b1",
                Description = "f2", Evidence = "e2", Active = true,
                Provenance = MakeProvenance(),
            },
        };
        var summary = new FalsifierSummary
        {
            StudyId = "study",
            Falsifiers = falsifiers,
            ActiveFatalCount = 0, ActiveHighCount = 2, TotalActiveCount = 2,
            Provenance = MakeProvenance(),
        };

        var result = RegistryDemotionIntegrator.ApplyDemotions(registry, summary);

        var p1 = result.Candidates[0];
        // C4 → C2 (high) → C0 (high again)
        Assert.Equal("C0_NumericalMode", p1.ClaimClass);
        Assert.Equal(2, p1.Demotions.Count);
    }
}
