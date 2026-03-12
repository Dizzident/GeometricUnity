using System.Text.Json;
using Gu.Core;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;
using Gu.Phase4.Reporting;

namespace Gu.Phase4.Reporting.Tests;

/// <summary>
/// Targeted coverage tests for Phase4ReportGenerator.Generate() (ADVISORY-REPORTING-1).
///
/// Requirements:
///   1. Generate() returns a non-null Phase4Report with all required fields populated.
///   2. The generated report survives a JSON round-trip via Phase4ReportSerializer.
/// </summary>
public sealed class Phase4ReportGeneratorCoverageTests
{
    [Fact]
    public void Generate_ReturnsNonNullReport_WithRequiredFieldsPopulated()
    {
        var generator = new Phase4ReportGenerator();
        var report = generator.Generate(
            "coverage-study-nonnull",
            TestHelpers.MakeRegistry(TestHelpers.MakeParticle("p1", UnifiedParticleType.Fermion)),
            TestHelpers.MakeAtlas(TestHelpers.MakeFamily("fam-x", "left", branchScore: 0.75)),
            TestHelpers.MakeCouplingAtlas(TestHelpers.MakeCoupling("boson-x", "ferm-0", "ferm-1", 0.6)),
            TestHelpers.MakeProvenance());

        Assert.NotNull(report);
        Assert.Equal("coverage-study-nonnull", report.StudyId);
        Assert.StartsWith("phase4-report-coverage-study-nonnull-", report.ReportId);
        Assert.NotNull(report.FermionAtlas);
        Assert.NotNull(report.CouplingAtlas);
        Assert.NotNull(report.RegistrySummary);
        Assert.NotNull(report.NegativeResults);
        Assert.NotNull(report.Provenance);
    }

    [Fact]
    public void Generate_JsonRoundTrip_PreservesKeyFields()
    {
        var generator = new Phase4ReportGenerator();
        var original = generator.Generate(
            "coverage-study-roundtrip",
            TestHelpers.MakeRegistry(
                TestHelpers.MakeParticle("b1", UnifiedParticleType.Boson),
                TestHelpers.MakeParticle("f1", UnifiedParticleType.Fermion)),
            TestHelpers.MakeAtlas(
                TestHelpers.MakeFamily("fam-rt1", "right", branchScore: 0.9),
                TestHelpers.MakeFamily("fam-rt2", "left", branchScore: 0.4)),
            TestHelpers.MakeCouplingAtlas(
                TestHelpers.MakeCoupling("boson-y", "ferm-0", "ferm-1", 0.3)),
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(original);
        var restored = Phase4ReportSerializer.Deserialize(json);

        Assert.Equal(original.ReportId, restored.ReportId);
        Assert.Equal(original.StudyId, restored.StudyId);
        Assert.Equal(original.FermionAtlas.TotalFamilies, restored.FermionAtlas.TotalFamilies);
        Assert.Equal(original.CouplingAtlas.TotalCouplings, restored.CouplingAtlas.TotalCouplings);
        Assert.Equal(original.RegistrySummary.TotalBosons, restored.RegistrySummary.TotalBosons);
        Assert.Equal(original.RegistrySummary.TotalFermions, restored.RegistrySummary.TotalFermions);
    }
}
