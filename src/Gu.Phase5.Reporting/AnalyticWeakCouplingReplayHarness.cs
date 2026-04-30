using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;

namespace Gu.Phase5.Reporting;

public sealed class AnalyticWeakCouplingReplayResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("couplingRecord")]
    public BosonFermionCouplingRecord? CouplingRecord { get; init; }

    [JsonPropertyName("evidenceBuild")]
    public ReplayedRawWeakCouplingMatrixElementEvidenceBuildResult? EvidenceBuild { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class AnalyticWeakCouplingReplayHarness
{
    public const string AlgorithmId = "phase79-analytic-weak-coupling-replay-harness-v1";

    public static AnalyticWeakCouplingReplayResult ReplayFromAnalyticVariation(
        FermionModeRecord modeI,
        FermionModeRecord modeJ,
        string bosonModeId,
        double[,] analyticVariationMatrixRe,
        double[,]? analyticVariationMatrixIm,
        string variationEvidenceId,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(modeI);
        ArgumentNullException.ThrowIfNull(modeJ);
        ArgumentNullException.ThrowIfNull(bosonModeId);
        ArgumentNullException.ThrowIfNull(analyticVariationMatrixRe);
        ArgumentNullException.ThrowIfNull(variationEvidenceId);
        ArgumentNullException.ThrowIfNull(provenance);

        var closure = new List<string>();
        if (modeI.EigenvectorCoefficients is null)
            closure.Add("fermion mode I eigenvector coefficients are missing");
        if (modeJ.EigenvectorCoefficients is null)
            closure.Add("fermion mode J eigenvector coefficients are missing");
        if (string.IsNullOrWhiteSpace(bosonModeId))
            closure.Add("boson mode id is missing");
        if (string.IsNullOrWhiteSpace(variationEvidenceId))
            closure.Add("variation evidence id is missing");
        if (analyticVariationMatrixRe.GetLength(0) != analyticVariationMatrixRe.GetLength(1))
            closure.Add("analytic variation real matrix must be square");
        if (analyticVariationMatrixIm is not null &&
            (analyticVariationMatrixIm.GetLength(0) != analyticVariationMatrixRe.GetLength(0) ||
             analyticVariationMatrixIm.GetLength(1) != analyticVariationMatrixRe.GetLength(1)))
        {
            closure.Add("analytic variation imaginary matrix dimensions must match real matrix");
        }

        if (closure.Count != 0)
        {
            return new AnalyticWeakCouplingReplayResult
            {
                AlgorithmId = AlgorithmId,
                TerminalStatus = "analytic-weak-coupling-replay-blocked",
                CouplingRecord = null,
                EvidenceBuild = null,
                ClosureRequirements = closure,
            };
        }

        var coupling = new CouplingProxyEngine(new CpuDiracOperatorAssembler()).ComputeCoupling(
            modeI,
            modeJ,
            bosonModeId,
            analyticVariationMatrixRe,
            analyticVariationMatrixIm,
            RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
            provenance,
            RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
            variationEvidenceId,
            ["replayed from analytic Dirac-variation matrix"]);

        var evidenceBuild = ReplayedRawWeakCouplingMatrixElementEvidenceBuilder.Build(coupling);
        closure.AddRange(evidenceBuild.ClosureRequirements);

        return new AnalyticWeakCouplingReplayResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "analytic-weak-coupling-replay-validated"
                : "analytic-weak-coupling-replay-blocked",
            CouplingRecord = coupling,
            EvidenceBuild = evidenceBuild,
            ClosureRequirements = closure,
        };
    }
}
