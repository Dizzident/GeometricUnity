using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Predictions.Tests;

public sealed class PredictionValidatorTests
{
    private static ComparisonAsset MakeAsset() => new()
    {
        AssetId = "asset-001",
        SourceCitation = "Test citation",
        AcquisitionDate = DateTimeOffset.UtcNow,
        PreprocessingDescription = "None",
        AdmissibleUseStatement = "Any",
        DomainOfValidity = "All",
        UncertaintyModel = UncertaintyRecord.Unestimated(),
        ComparisonVariables = new Dictionary<string, string> { ["energy"] = "Total energy" },
    };

    private static PredictionTestRecord MakeRecord(
        ClaimClass claimClass = ClaimClass.ExactStructuralConsequence,
        string theoremStatus = "closed",
        string numericalStatus = "converged",
        string approximation = "exact",
        string? falsifier = "Observable diverges from prediction by >5 sigma",
        string? branchManifestId = "manifest-001",
        ComparisonAsset? asset = null,
        bool includeAsset = true) => new()
    {
        TestId = "test-001",
        ClaimClass = claimClass,
        FormalSource = "Theorem 3.1",
        BranchManifestId = branchManifestId ?? "",
        ObservableMapId = "obs-map-001",
        TheoremDependencyStatus = theoremStatus,
        NumericalDependencyStatus = numericalStatus,
        ApproximationStatus = approximation,
        ExternalComparisonAsset = includeAsset ? (asset ?? MakeAsset()) : null,
        Falsifier = falsifier ?? "",
        ArtifactLinks = ["artifact-001"],
    };

    [Fact]
    public void FullyValid_Record_PassesAllRules()
    {
        var record = MakeRecord();
        var result = PredictionValidator.Validate(record);

        Assert.True(result.IsValid);
        Assert.Empty(result.Violations);
        Assert.Equal(ClaimClass.ExactStructuralConsequence, result.Record.ClaimClass);
    }

    [Fact]
    public void Rule3_MissingFalsifier_BecomesInadmissible()
    {
        var record = MakeRecord(falsifier: "");
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Equal(ClaimClass.Inadmissible, result.Record.ClaimClass);
        Assert.Contains(result.Violations, v => v.Contains("Rule 3"));
    }

    [Fact]
    public void Rule3_WhitespaceFalsifier_BecomesInadmissible()
    {
        var record = MakeRecord(falsifier: "   ");
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Equal(ClaimClass.Inadmissible, result.Record.ClaimClass);
    }

    [Fact]
    public void Rule4_MissingBranchManifest_BecomesInadmissible()
    {
        var record = MakeRecord(branchManifestId: "");
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Equal(ClaimClass.Inadmissible, result.Record.ClaimClass);
        Assert.Contains(result.Violations, v => v.Contains("Rule 4"));
    }

    [Fact]
    public void Rule1_OpenTheorem_DemotesExactToApproximate()
    {
        var record = MakeRecord(
            claimClass: ClaimClass.ExactStructuralConsequence,
            theoremStatus: "open");
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Equal(ClaimClass.ApproximateStructuralSurrogate, result.Record.ClaimClass);
        Assert.Contains(result.Violations, v => v.Contains("Rule 1"));
    }

    [Fact]
    public void Rule1_OpenTheorem_DoesNotDemoteNonExact()
    {
        var record = MakeRecord(
            claimClass: ClaimClass.ApproximateStructuralSurrogate,
            theoremStatus: "open");
        var result = PredictionValidator.Validate(record);

        // Rule 1 only fires for ExactStructuralConsequence
        Assert.DoesNotContain(result.Violations, v => v.Contains("Rule 1"));
    }

    [Fact]
    public void Rule2_MissingExternalAsset_NotesViolation()
    {
        var record = MakeRecord(includeAsset: false);
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Contains(result.Violations, v => v.Contains("Rule 2"));
        // Rule 2 does not demote -- just notes
        Assert.Equal(ClaimClass.ExactStructuralConsequence, result.Record.ClaimClass);
    }

    [Fact]
    public void Rule5_ExploratoryNumerical_DemotesToPostdiction()
    {
        var record = MakeRecord(numericalStatus: "exploratory");
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Equal(ClaimClass.PostdictionTarget, result.Record.ClaimClass);
        Assert.Contains(result.Violations, v => v.Contains("Rule 5"));
    }

    [Fact]
    public void Rule5_FailedNumerical_DemotesToPostdiction()
    {
        var record = MakeRecord(numericalStatus: "failed");
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.Equal(ClaimClass.PostdictionTarget, result.Record.ClaimClass);
    }

    [Fact]
    public void Rule5_DoesNotPromote_AlreadyLowerClass()
    {
        var record = MakeRecord(
            claimClass: ClaimClass.SpeculativeInterpretation,
            numericalStatus: "exploratory");
        var result = PredictionValidator.Validate(record);

        // SpeculativeInterpretation > PostdictionTarget in enum, so no demotion
        Assert.Equal(ClaimClass.SpeculativeInterpretation, result.Record.ClaimClass);
    }

    [Fact]
    public void Rule3_TakesPrecedence_OverOtherRules()
    {
        // Missing falsifier AND open theorem -- Rule 3 should fire first
        var record = MakeRecord(falsifier: "", theoremStatus: "open");
        var result = PredictionValidator.Validate(record);

        Assert.Equal(ClaimClass.Inadmissible, result.Record.ClaimClass);
        Assert.Single(result.Violations); // Only Rule 3
    }

    [Fact]
    public void MultipleViolations_AllReported()
    {
        // Open theorem + missing asset + exploratory numerical
        var record = MakeRecord(
            theoremStatus: "open",
            numericalStatus: "exploratory",
            includeAsset: false);
        var result = PredictionValidator.Validate(record);

        Assert.False(result.IsValid);
        Assert.True(result.Violations.Count >= 3);
        Assert.Contains(result.Violations, v => v.Contains("Rule 1"));
        Assert.Contains(result.Violations, v => v.Contains("Rule 2"));
        Assert.Contains(result.Violations, v => v.Contains("Rule 5"));
    }

    [Fact]
    public void Validate_NullRecord_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PredictionValidator.Validate(null!));
    }

    [Fact]
    public void UncertaintyRecord_Unestimated_AllNegativeOne()
    {
        var u = UncertaintyRecord.Unestimated();
        Assert.Equal(-1, u.Discretization);
        Assert.Equal(-1, u.Solver);
        Assert.Equal(-1, u.Branch);
        Assert.Equal(-1, u.Extraction);
        Assert.Equal(-1, u.Calibration);
        Assert.Equal(-1, u.DataAsset);
    }

    [Fact]
    public void ComparisonAsset_RequiredFields_RoundTrip()
    {
        var asset = MakeAsset();
        Assert.Equal("asset-001", asset.AssetId);
        Assert.Equal("Test citation", asset.SourceCitation);
        Assert.NotNull(asset.UncertaintyModel);
        Assert.Single(asset.ComparisonVariables);
    }
}
