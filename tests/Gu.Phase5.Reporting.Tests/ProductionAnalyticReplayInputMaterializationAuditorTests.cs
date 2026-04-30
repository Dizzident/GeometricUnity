using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ProductionAnalyticReplayInputMaterializationAuditorTests
{
    [Fact]
    public void Audit_WithFullyMaterializedProductionInputs_Passes()
    {
        var result = ProductionAnalyticReplayInputMaterializationAuditor.Audit(new ProductionAnalyticReplayInputMaterializationRecord
        {
            ArtifactId = "phase80-production-replay-inputs",
            BosonModeSourceKind = ProductionAnalyticReplayInputMaterializationAuditor.AcceptedBosonModeSourceKind,
            HasBosonPerturbationVector = true,
            HasAnalyticVariationMatrix = true,
            HasFermionModeEigenvectors = true,
            HasFullCouplingRecord = true,
            IsTopCouplingSummaryOnly = false,
            VariationMethod = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
            NormalizationConvention = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
            VariationEvidenceId = "analytic-variation-w-mode",
            ProvenanceId = "phase80:test-revision:cpu",
        });

        Assert.Equal("production-analytic-replay-inputs-materialized", result.TerminalStatus);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Audit_WithPhase4TopSyntheticSummary_Blocks()
    {
        var result = ProductionAnalyticReplayInputMaterializationAuditor.Audit(new ProductionAnalyticReplayInputMaterializationRecord
        {
            ArtifactId = "studies/phase4_fermion_family_atlas_001/output/coupling_atlas.json",
            BosonModeSourceKind = "synthetic-boson-perturbation",
            HasBosonPerturbationVector = false,
            HasAnalyticVariationMatrix = false,
            HasFermionModeEigenvectors = false,
            HasFullCouplingRecord = false,
            IsTopCouplingSummaryOnly = true,
            VariationMethod = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
            NormalizationConvention = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
            VariationEvidenceId = null,
            ProvenanceId = "phase4-study-output",
        });

        Assert.Equal("production-analytic-replay-inputs-blocked", result.TerminalStatus);
        Assert.Contains($"boson mode source kind must be {ProductionAnalyticReplayInputMaterializationAuditor.AcceptedBosonModeSourceKind}", result.ClosureRequirements);
        Assert.Contains("selected boson perturbation vector is missing", result.ClosureRequirements);
        Assert.Contains("selected fermion mode eigenvectors are missing", result.ClosureRequirements);
        Assert.Contains("top-coupling summary is insufficient; full real/imaginary coupling record is required", result.ClosureRequirements);
    }
}
