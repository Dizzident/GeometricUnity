using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

/// <summary>
/// Tests for P11-M7: GeometryEvidenceClassifier and the observerse-recovery blocking contract.
///
/// Verifies:
/// - Current repo geometry (dim=2 base, dim=7 ambient) is classified as "toy-control"
/// - Only dim(X)=4, dim(Y)=14 qualifies as "draft-aligned"
/// - The block statement is emitted for "toy-control" and null for "draft-aligned"
/// - Phase5Report carries geometryEvidenceLabel and observerseRecoveryBlock
/// - Phase5ReportGenerator defaults to "toy-control" when label is not provided
/// - ToMarkdown includes the "Geometry Evidence" section and block statement
/// - D-P11-007: X^4/Observerse recovery claims are mechanically blocked for toy-control
/// </summary>
public sealed class GeometryEvidenceClassifierTests
{
    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase11-m7-v1",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    // ===== GeometryEvidenceClassifier.Classify =====

    [Fact]
    public void Classify_CurrentRepoDimensions_IsToyControl()
    {
        // Current repo: dim(X)=2, dim(Y)=7 (2D base, 5D fiber)
        var label = GeometryEvidenceClassifier.Classify(baseDimension: 2, ambientDimension: 7);
        Assert.Equal(GeometryEvidenceClassifier.ToyControl, label);
    }

    [Fact]
    public void Classify_DraftDimensions_IsDraftAligned()
    {
        // Draft: dim(X)=4, dim(Y)=14
        var label = GeometryEvidenceClassifier.Classify(baseDimension: 4, ambientDimension: 14);
        Assert.Equal(GeometryEvidenceClassifier.DraftAligned, label);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 5)]
    [InlineData(3, 10)]
    [InlineData(4, 10)]   // correct base but wrong ambient
    [InlineData(5, 14)]   // wrong base but correct ambient
    [InlineData(3, 14)]
    public void Classify_NonDraftDimensions_IsToyControl(int baseDim, int ambientDim)
    {
        var label = GeometryEvidenceClassifier.Classify(baseDim, ambientDim);
        Assert.Equal(GeometryEvidenceClassifier.ToyControl, label);
    }

    // ===== GetBlockStatement =====

    [Fact]
    public void GetBlockStatement_ToyControl_ReturnsNonNull()
    {
        var block = GeometryEvidenceClassifier.GetBlockStatement(GeometryEvidenceClassifier.ToyControl);
        Assert.NotNull(block);
    }

    [Fact]
    public void GetBlockStatement_ToyControl_StartsWithObserverseRecoveryBlocked()
    {
        var block = GeometryEvidenceClassifier.GetBlockStatement(GeometryEvidenceClassifier.ToyControl);
        Assert.NotNull(block);
        Assert.StartsWith("OBSERVERSE-RECOVERY-BLOCKED:", block);
    }

    [Fact]
    public void GetBlockStatement_ToyControl_MentionsDim2()
    {
        var block = GeometryEvidenceClassifier.GetBlockStatement(GeometryEvidenceClassifier.ToyControl);
        Assert.NotNull(block);
        Assert.Contains("dim(X)=2", block);
    }

    [Fact]
    public void GetBlockStatement_DraftAligned_ReturnsNull()
    {
        var block = GeometryEvidenceClassifier.GetBlockStatement(GeometryEvidenceClassifier.DraftAligned);
        Assert.Null(block);
    }

    // ===== Phase5Report carries geometryEvidenceLabel =====

    [Fact]
    public void Phase5ReportGenerator_DefaultsToToyControl()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());

        Assert.Equal(GeometryEvidenceClassifier.ToyControl, report.GeometryEvidenceLabel);
    }

    [Fact]
    public void Phase5ReportGenerator_DefaultsToyControl_HasObserverseBlock()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());

        Assert.NotNull(report.ObserverseRecoveryBlock);
        Assert.StartsWith("OBSERVERSE-RECOVERY-BLOCKED:", report.ObserverseRecoveryBlock);
    }

    [Fact]
    public void Phase5ReportGenerator_ExplicitToyControl_HasObserverseBlock()
    {
        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            geometryEvidenceLabel: GeometryEvidenceClassifier.ToyControl);

        Assert.Equal(GeometryEvidenceClassifier.ToyControl, report.GeometryEvidenceLabel);
        Assert.NotNull(report.ObserverseRecoveryBlock);
    }

    [Fact]
    public void Phase5ReportGenerator_DraftAligned_NoObserverseBlock()
    {
        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            geometryEvidenceLabel: GeometryEvidenceClassifier.DraftAligned);

        Assert.Equal(GeometryEvidenceClassifier.DraftAligned, report.GeometryEvidenceLabel);
        Assert.Null(report.ObserverseRecoveryBlock);
    }

    // ===== ToMarkdown includes the Geometry Evidence section =====

    [Fact]
    public void ToMarkdown_ToyControl_IncludesGeometryEvidenceSection()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());
        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("## Geometry Evidence", md);
        Assert.Contains("toy-control", md);
    }

    [Fact]
    public void ToMarkdown_ToyControl_IncludesObserverseRecoveryBlock()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());
        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("OBSERVERSE-RECOVERY-BLOCKED:", md);
    }

    [Fact]
    public void ToMarkdown_DraftAligned_NoObserverseRecoveryBlock()
    {
        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            geometryEvidenceLabel: GeometryEvidenceClassifier.DraftAligned);
        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("## Geometry Evidence", md);
        Assert.Contains("draft-aligned", md);
        Assert.DoesNotContain("OBSERVERSE-RECOVERY-BLOCKED:", md);
    }

    // ===== D-P11-007 contract: X^4 recovery claims mechanically blocked =====

    [Fact]
    public void D_P11_007_ToyControl_BlockStatement_MentionsX4()
    {
        // The block statement must make clear that X^4 recovery is not evidenced.
        var block = GeometryEvidenceClassifier.GetBlockStatement(GeometryEvidenceClassifier.ToyControl);
        Assert.NotNull(block);
        Assert.Contains("X^4", block);
    }

    [Fact]
    public void D_P11_007_ToyControl_BlockStatement_MentionsObserverse()
    {
        var block = GeometryEvidenceClassifier.GetBlockStatement(GeometryEvidenceClassifier.ToyControl);
        Assert.NotNull(block);
        Assert.Contains("Observerse", block);
    }

    [Fact]
    public void D_P11_007_Report_For_CurrentRepoGeometry_IsAlwaysToyControl()
    {
        // Current repo manifests use baseDimension=2, ambientDimension=7.
        // This test confirms that generating a report without an explicit label
        // always defaults to toy-control, blocking any X^4 recovery inference.
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());

        Assert.Equal(GeometryEvidenceClassifier.ToyControl, report.GeometryEvidenceLabel);
        Assert.NotNull(report.ObserverseRecoveryBlock);
    }

    // ===== ClassifyFromManifest =====

    [Fact]
    public void ClassifyFromManifest_SameBehaviorAsClassify()
    {
        Assert.Equal(
            GeometryEvidenceClassifier.Classify(2, 7),
            GeometryEvidenceClassifier.ClassifyFromManifest(2, 7));
        Assert.Equal(
            GeometryEvidenceClassifier.Classify(4, 14),
            GeometryEvidenceClassifier.ClassifyFromManifest(4, 14));
    }
}
