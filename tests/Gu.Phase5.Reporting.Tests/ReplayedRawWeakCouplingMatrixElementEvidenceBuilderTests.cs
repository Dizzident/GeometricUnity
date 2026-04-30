using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ReplayedRawWeakCouplingMatrixElementEvidenceBuilderTests
{
    [Fact]
    public void Build_WithAnalyticUnitModeCoupling_BuildsValidatedEvidence()
    {
        var result = ReplayedRawWeakCouplingMatrixElementEvidenceBuilder.Build(
            MakeCoupling(
                variationMethod: RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
                normalizationConvention: RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
                real: 0.6,
                imaginary: 0.8,
                magnitude: 1.0));

        Assert.Equal("replayed-raw-weak-coupling-matrix-element-evidence-built", result.TerminalStatus);
        Assert.Equal("raw-weak-coupling-matrix-element-evidence-validated", result.EvidenceValidation.TerminalStatus);
        Assert.Equal(1.0, result.EvidenceValidation.AcceptedRawMatrixElementMagnitude);
        Assert.False(result.EvidenceValidation.Evidence.UsesFiniteDifferenceProxy);
        Assert.False(result.EvidenceValidation.Evidence.UsesCouplingProxyMagnitude);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Build_WithFiniteDifferenceVariation_BlocksEvidence()
    {
        var result = ReplayedRawWeakCouplingMatrixElementEvidenceBuilder.Build(
            MakeCoupling(
                variationMethod: "finite-difference-dirac-variation:v1",
                normalizationConvention: RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
                real: 0.6,
                imaginary: 0.8,
                magnitude: 1.0));

        Assert.Equal("replayed-raw-weak-coupling-matrix-element-evidence-blocked", result.TerminalStatus);
        Assert.Equal("raw-weak-coupling-matrix-element-evidence-blocked", result.EvidenceValidation.TerminalStatus);
        Assert.Contains(
            $"variation method must be {RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod}",
            result.ClosureRequirements);
        Assert.Contains("raw matrix-element evidence must not use finite-difference coupling proxies", result.ClosureRequirements);
    }

    [Fact]
    public void Build_WithMismatchedMagnitude_BlocksEvidence()
    {
        var result = ReplayedRawWeakCouplingMatrixElementEvidenceBuilder.Build(
            MakeCoupling(
                variationMethod: RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
                normalizationConvention: RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
                real: 0.6,
                imaginary: 0.8,
                magnitude: 0.9));

        Assert.Equal("replayed-raw-weak-coupling-matrix-element-evidence-blocked", result.TerminalStatus);
        Assert.Contains("source coupling magnitude does not match real/imaginary matrix-element components", result.ClosureRequirements);
    }

    private static BosonFermionCouplingRecord MakeCoupling(
        string variationMethod,
        string normalizationConvention,
        double real,
        double imaginary,
        double magnitude) => new()
    {
        CouplingId = "coupling-w-mode-lepton-current",
        BosonModeId = "w-mode",
        FermionModeIdI = "lepton-left",
        FermionModeIdJ = "lepton-neutrino",
        CouplingProxyReal = real,
        CouplingProxyImaginary = imaginary,
        CouplingProxyMagnitude = magnitude,
        NormalizationConvention = normalizationConvention,
        SelectionRuleNotes = [],
        VariationMethod = variationMethod,
        VariationEvidenceId = "variation-phase4-analytic-dirac-w-mode",
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-29T00:00:00Z"),
            CodeRevision = "test-revision",
            Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
            Backend = "cpu",
        },
    };
}
