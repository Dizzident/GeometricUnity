using Gu.Phase2.Recovery;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Recovery.Tests;

public sealed class IdentificationGateTests
{
    private static PhysicalIdentificationRecord MakeRecord(
        string formalSource = "F_{A_omega} on Y",
        string observationExtractionMap = "sigma_h_star -> trace-projector",
        string supportStatus = "theorem-supported",
        string approximationStatus = "flat-background, dim=4, su(2)",
        string comparisonTarget = "gravitational-coupling",
        string falsifier = "wrong sign of trace under su(2) Killing pairing")
    {
        return new PhysicalIdentificationRecord
        {
            IdentificationId = "test-id",
            FormalSource = formalSource,
            ObservationExtractionMap = observationExtractionMap,
            SupportStatus = supportStatus,
            ApproximationStatus = approximationStatus,
            ComparisonTarget = comparisonTarget,
            Falsifier = falsifier,
            ResolvedClaimClass = ClaimClass.Inadmissible, // placeholder, gate will compute
        };
    }

    [Fact]
    public void AllFieldsPresent_TheoremSupported_ExactStructural()
    {
        var record = MakeRecord(supportStatus: "theorem-supported");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.ExactStructuralConsequence, result.ResolvedClaimClass);
    }

    [Fact]
    public void AllFieldsPresent_NumericalOnly_ApproximateSurrogate()
    {
        var record = MakeRecord(supportStatus: "numerical-only");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.ApproximateStructuralSurrogate, result.ResolvedClaimClass);
    }

    [Fact]
    public void AllFieldsPresent_Conjectural_SpeculativeInterpretation()
    {
        var record = MakeRecord(supportStatus: "conjectural");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.SpeculativeInterpretation, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingFalsifier_Inadmissible()
    {
        var record = MakeRecord(falsifier: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void WhitespaceFalsifier_Inadmissible()
    {
        var record = MakeRecord(falsifier: "   ");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingFormalSource_Inadmissible()
    {
        var record = MakeRecord(formalSource: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingObservationExtractionMap_Inadmissible()
    {
        var record = MakeRecord(observationExtractionMap: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingSupportStatus_Inadmissible()
    {
        var record = MakeRecord(supportStatus: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingComparisonTarget_Inadmissible()
    {
        var record = MakeRecord(comparisonTarget: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingApproximationStatus_TheoremSupported_DemotesToApproximateSurrogate()
    {
        var record = MakeRecord(
            supportStatus: "theorem-supported",
            approximationStatus: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.ApproximateStructuralSurrogate, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingApproximationStatus_NumericalOnly_DemotesToPostdiction()
    {
        var record = MakeRecord(
            supportStatus: "numerical-only",
            approximationStatus: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.PostdictionTarget, result.ResolvedClaimClass);
    }

    [Fact]
    public void MissingApproximationStatus_Conjectural_DemotesToInadmissible()
    {
        var record = MakeRecord(
            supportStatus: "conjectural",
            approximationStatus: "");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void UnrecognizedSupportStatus_PostdictionTarget()
    {
        var record = MakeRecord(supportStatus: "heuristic-argument");
        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.PostdictionTarget, result.ResolvedClaimClass);
    }

    [Fact]
    public void NullRecord_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => IdentificationGate.Evaluate(null!));
    }

    [Fact]
    public void Evaluate_ReturnsNewRecord_PreservesFields()
    {
        var record = MakeRecord();
        var result = IdentificationGate.Evaluate(record);

        Assert.NotSame(record, result);
        Assert.Equal(record.IdentificationId, result.IdentificationId);
        Assert.Equal(record.FormalSource, result.FormalSource);
        Assert.Equal(record.ObservationExtractionMap, result.ObservationExtractionMap);
        Assert.Equal(record.SupportStatus, result.SupportStatus);
        Assert.Equal(record.ApproximationStatus, result.ApproximationStatus);
        Assert.Equal(record.ComparisonTarget, result.ComparisonTarget);
        Assert.Equal(record.Falsifier, result.Falsifier);
    }

    [Fact]
    public void Evaluate_NeverMutatesInput()
    {
        var record = MakeRecord(supportStatus: "conjectural");
        var originalClass = record.ResolvedClaimClass;
        _ = IdentificationGate.Evaluate(record);
        Assert.Equal(originalClass, record.ResolvedClaimClass);
    }
}
