using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class RawWeakCouplingMatrixElementEvidenceValidatorTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Validate_WithReplayedAnalyticEvidence_AcceptsRawMagnitude()
    {
        var result = RawWeakCouplingMatrixElementEvidenceValidator.Validate(MakeAcceptedEvidence(0.8125));

        Assert.Equal("raw-weak-coupling-matrix-element-evidence-validated", result.TerminalStatus);
        Assert.Equal(0.8125, result.AcceptedRawMatrixElementMagnitude);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Validate_WithProvisionalScalarInput_BlocksPromotion()
    {
        var result = RawWeakCouplingMatrixElementEvidenceValidator.Validate(new RawWeakCouplingMatrixElementEvidenceRecord
        {
            EvidenceId = "phase65-provisional-scalar-raw-matrix-element",
            SourceKind = "scalar-study-input",
            VariationMethod = "manual-scalar:v1",
            NormalizationConvention = "unknown",
            RawMatrixElementMagnitude = 0.8,
            UsesFiniteDifferenceProxy = false,
            UsesCouplingProxyMagnitude = false,
            ReplayedFromCouplingRecordId = null,
            VariationEvidenceId = null,
            ProvenanceId = "phase65-study-artifact",
        });

        Assert.Equal("raw-weak-coupling-matrix-element-evidence-blocked", result.TerminalStatus);
        Assert.Null(result.AcceptedRawMatrixElementMagnitude);
        Assert.Contains($"matrix-element source kind must be {RawWeakCouplingMatrixElementEvidenceValidator.AcceptedSourceKind}", result.ClosureRequirements);
        Assert.Contains("replayed coupling record id is missing", result.ClosureRequirements);
    }

    [Fact]
    public void ExtractFromEvidence_WithValidatedEvidence_UsesAcceptedRawMagnitude()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());
        var matrixElement = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: false,
            usesCouplingProxyMagnitude: false);
        var evidence = RawWeakCouplingMatrixElementEvidenceValidator.Validate(MakeAcceptedEvidence(0.8125));

        var extraction = DimensionlessWeakCouplingAmplitudeExtractor.ExtractFromEvidence(
            matrixElement,
            normalization,
            evidence,
            TargetObservableIds);

        Assert.Equal("dimensionless-weak-coupling-amplitude-extracted", extraction.TerminalStatus);
        Assert.Equal(0.8125, extraction.RawMatrixElementMagnitude);
        Assert.Equal(0.8125 * System.Math.Sqrt(0.5), extraction.Candidate!.CouplingValue);
    }

    [Fact]
    public void ExtractFromEvidence_WithBlockedEvidence_DoesNotEmitCandidate()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());
        var matrixElement = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: false,
            usesCouplingProxyMagnitude: false);
        var evidence = RawWeakCouplingMatrixElementEvidenceValidator.Validate(MakeAcceptedEvidence(double.NaN));

        var extraction = DimensionlessWeakCouplingAmplitudeExtractor.ExtractFromEvidence(
            matrixElement,
            normalization,
            evidence,
            TargetObservableIds);

        Assert.Equal("dimensionless-weak-coupling-amplitude-blocked", extraction.TerminalStatus);
        Assert.Null(extraction.Candidate);
        Assert.Contains("raw matrix-element magnitude must be finite and positive", extraction.ClosureRequirements);
    }

    private static RawWeakCouplingMatrixElementEvidenceRecord MakeAcceptedEvidence(double magnitude) => new()
    {
        EvidenceId = "phase77-replayed-analytic-matrix-element",
        SourceKind = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedSourceKind,
        VariationMethod = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
        NormalizationConvention = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
        RawMatrixElementMagnitude = magnitude,
        UsesFiniteDifferenceProxy = false,
        UsesCouplingProxyMagnitude = false,
        ReplayedFromCouplingRecordId = "coupling-w-mode-lepton-current",
        VariationEvidenceId = "variation-phase4-analytic-dirac-w-mode",
        ProvenanceId = "test",
    };
}
