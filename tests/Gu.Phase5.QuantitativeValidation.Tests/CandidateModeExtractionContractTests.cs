using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class CandidateModeExtractionContractTests
{
    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "p19-wz-candidate", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static CandidateModeSourceRecord MakeInternalSource() => new()
    {
        SourceId = "su2-vector-mode-17",
        SourceOrigin = CandidateModeExtractor.InternalComputedOrigin,
        SourceArtifactKind = CandidateModeExtractor.ComputedObservableArtifactKind,
        SourceArtifactPath = "study-runs/phase19_internal_modes/computed_observables.json",
        SourceObservableId = "internal-vector-mode-17",
        Value = 80.0,
        Uncertainty = 0.4,
        UnitFamily = "mass-energy",
        Unit = "internal-mass-unit",
        EnvironmentId = "env-physical-candidate",
        BranchId = "su2-branch",
        RefinementLevel = "L1",
        SourceExtractionMethod = "spectral-mode-solve:v1",
        Provenance = MakeProvenance(),
    };

    [Fact]
    public void ExtractCandidateMode_FromInternalComputedSource_ProducesProvisionalModeWithSourceSelectors()
    {
        var source = MakeInternalSource();

        var extraction = CandidateModeExtractor.ExtractCandidateMode(
            source,
            extractionId: "extract-w",
            modeId: "candidate-w-mode",
            particleId: "w-boson",
            modeKind: "vector-boson-mass-mode",
            provenance: MakeProvenance());

        Assert.Equal("provisional", extraction.Status);
        Assert.NotNull(extraction.CandidateMode);
        Assert.Equal(["internal-vector-mode-17"], extraction.SourceObservableIds);
        Assert.Equal("env-physical-candidate", extraction.EnvironmentId);
        Assert.Equal("su2-branch", extraction.BranchId);
        Assert.Equal("L1", extraction.RefinementLevel);
        Assert.Equal(80.0, extraction.CandidateMode.Value);
        Assert.Equal("provisional", extraction.CandidateMode.Status);
        Assert.Contains(CandidateModeExtractor.AlgorithmId, extraction.CandidateMode.ExtractionMethod);
    }

    [Fact]
    public void ExtractCandidateMode_FromExternalTargetOrigin_ReturnsBlockedWithoutCandidateMode()
    {
        var targetCopyThroughSource = new CandidateModeSourceRecord
        {
            SourceId = "pdg-w-mass-target",
            SourceOrigin = "external-target",
            SourceArtifactKind = "external-target-table",
            SourceArtifactPath = "studies/phase19_dimensionless_wz_candidate_001/config/external_targets.json",
            SourceObservableId = "physical-w-mass",
            Value = 80.377,
            Uncertainty = 0.012,
            UnitFamily = "mass-energy",
            Unit = "GeV",
            EnvironmentId = "env-physical-candidate",
            BranchId = "su2-branch",
            RefinementLevel = "L1",
            SourceExtractionMethod = "target-table-copy-through",
            Provenance = MakeProvenance(),
        };

        var extraction = CandidateModeExtractor.ExtractCandidateMode(
            targetCopyThroughSource,
            extractionId: "extract-w",
            modeId: "candidate-w-mode",
            particleId: "w-boson",
            modeKind: "vector-boson-mass-mode",
            provenance: MakeProvenance());

        Assert.Equal("blocked", extraction.Status);
        Assert.Null(extraction.CandidateMode);
        Assert.Contains(extraction.ClosureRequirements, r => r.Contains("external targets are comparison data only", StringComparison.Ordinal));
        Assert.Contains(extraction.ClosureRequirements, r => r.Contains("external target table", StringComparison.Ordinal));
    }

    [Fact]
    public void ExtractCandidateMode_FromComputedLabelButExternalTargetPath_ReturnsBlocked()
    {
        var internalSource = MakeInternalSource();
        var disguisedTargetSource = new CandidateModeSourceRecord
        {
            SourceId = internalSource.SourceId,
            SourceOrigin = internalSource.SourceOrigin,
            SourceArtifactKind = internalSource.SourceArtifactKind,
            SourceArtifactPath = "studies/phase19_dimensionless_wz_candidate_001/config/external_targets.json",
            SourceObservableId = "physical-z-mass",
            Value = 91.1876,
            Uncertainty = internalSource.Uncertainty,
            UnitFamily = internalSource.UnitFamily,
            Unit = internalSource.Unit,
            EnvironmentId = internalSource.EnvironmentId,
            BranchId = internalSource.BranchId,
            RefinementLevel = internalSource.RefinementLevel,
            SourceExtractionMethod = internalSource.SourceExtractionMethod,
            Provenance = internalSource.Provenance,
        };

        var extraction = CandidateModeExtractor.ExtractCandidateMode(
            disguisedTargetSource,
            extractionId: "extract-z",
            modeId: "candidate-z-mode",
            particleId: "z-boson",
            modeKind: "vector-boson-mass-mode",
            provenance: MakeProvenance());

        Assert.Equal("blocked", extraction.Status);
        Assert.Null(extraction.CandidateMode);
        Assert.Contains(extraction.ClosureRequirements, r => r.Contains("sourceArtifactPath appears to reference an external target table", StringComparison.Ordinal));
    }
}
