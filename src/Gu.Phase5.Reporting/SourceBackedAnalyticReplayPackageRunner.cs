using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase5.Reporting;

public sealed class SourceBackedAnalyticReplayPackageRunResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("bosonPerturbationVectorMaterialization")]
    public required BosonPerturbationVectorMaterializationResult BosonPerturbationVectorMaterialization { get; init; }

    [JsonPropertyName("fullReplayPackage")]
    public FullAnalyticWeakCouplingReplayPackage? FullReplayPackage { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class SourceBackedAnalyticReplayPackageRunner
{
    public const string AlgorithmId = "phase83-source-backed-analytic-replay-package-runner-v1";

    public static SourceBackedAnalyticReplayPackageRunResult Run(
        string packageId,
        string sourceArtifactId,
        string bosonModeJson,
        GammaOperatorBundle gammas,
        int cellCount,
        int spinorDim,
        int dimG,
        double[] edgeLengths,
        int[][] cellsPerEdge,
        double[][] edgeDirections,
        FermionModeRecord modeI,
        FermionModeRecord modeJ,
        string provenanceId,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceArtifactId);
        ArgumentException.ThrowIfNullOrWhiteSpace(bosonModeJson);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(edgeLengths);
        ArgumentNullException.ThrowIfNull(cellsPerEdge);
        ArgumentNullException.ThrowIfNull(edgeDirections);
        ArgumentNullException.ThrowIfNull(modeI);
        ArgumentNullException.ThrowIfNull(modeJ);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenanceId);
        ArgumentNullException.ThrowIfNull(provenance);

        int expectedVectorLength = cellsPerEdge.Length * dimG;
        var vectorMaterialization = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
            sourceArtifactId,
            bosonModeJson,
            expectedVectorLength);

        var closure = new List<string>(vectorMaterialization.ClosureRequirements);
        ValidateVariationInputs(
            cellCount,
            spinorDim,
            dimG,
            edgeLengths,
            cellsPerEdge,
            edgeDirections,
            modeI,
            modeJ,
            closure);

        if (closure.Count != 0)
            return Blocked(vectorMaterialization, closure);

        double[,] variationRe;
        double[,] variationIm;
        try
        {
            (variationRe, variationIm) = DiracVariationComputer.ComputeAnalytical(
                vectorMaterialization.PerturbationVector.ToArray(),
                gammas,
                cellCount,
                spinorDim,
                dimG,
                edgeLengths,
                cellsPerEdge,
                edgeDirections);
        }
        catch (Exception ex)
        {
            closure.Add($"analytic variation matrix computation failed: {ex.Message}");
            return Blocked(vectorMaterialization, closure);
        }

        string variationEvidenceId = $"analytic-variation:{vectorMaterialization.ModeId}:{sourceArtifactId}";
        var replay = AnalyticWeakCouplingReplayHarness.ReplayFromAnalyticVariation(
            modeI,
            modeJ,
            vectorMaterialization.ModeId,
            variationRe,
            variationIm,
            variationEvidenceId,
            provenance);

        var package = FullAnalyticWeakCouplingReplayPackageBuilder.Build(
            packageId,
            ProductionAnalyticReplayInputMaterializationAuditor.AcceptedBosonModeSourceKind,
            vectorMaterialization.PerturbationVector,
            variationRe,
            variationIm,
            modeI,
            modeJ,
            replay,
            provenanceId);

        closure.AddRange(package.ClosureRequirements);
        return new SourceBackedAnalyticReplayPackageRunResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "source-backed-analytic-replay-package-built"
                : "source-backed-analytic-replay-package-blocked",
            BosonPerturbationVectorMaterialization = vectorMaterialization,
            FullReplayPackage = package,
            ClosureRequirements = closure,
        };
    }

    private static void ValidateVariationInputs(
        int cellCount,
        int spinorDim,
        int dimG,
        double[] edgeLengths,
        int[][] cellsPerEdge,
        double[][] edgeDirections,
        FermionModeRecord modeI,
        FermionModeRecord modeJ,
        List<string> closure)
    {
        if (cellCount <= 0)
            closure.Add("cell count must be positive");
        if (spinorDim <= 0)
            closure.Add("spinor dimension must be positive");
        if (dimG <= 0)
            closure.Add("gauge algebra dimension must be positive");
        if (edgeLengths.Length != cellsPerEdge.Length)
            closure.Add("edge length count must match cells-per-edge count");
        if (edgeDirections.Length != cellsPerEdge.Length)
            closure.Add("edge direction count must match cells-per-edge count");
        if (edgeLengths.Any(v => !double.IsFinite(v) || v < 0.0))
            closure.Add("edge lengths must be finite and non-negative");
        if (edgeDirections.SelectMany(v => v).Any(v => !double.IsFinite(v)))
            closure.Add("edge directions must be finite");

        int expectedEigenvectorLength = cellCount * spinorDim * dimG * 2;
        if (modeI.EigenvectorCoefficients is null || modeI.EigenvectorCoefficients.Length != expectedEigenvectorLength)
            closure.Add($"fermion mode I eigenvector length must be {expectedEigenvectorLength}");
        if (modeJ.EigenvectorCoefficients is null || modeJ.EigenvectorCoefficients.Length != expectedEigenvectorLength)
            closure.Add($"fermion mode J eigenvector length must be {expectedEigenvectorLength}");
    }

    private static SourceBackedAnalyticReplayPackageRunResult Blocked(
        BosonPerturbationVectorMaterializationResult vectorMaterialization,
        IReadOnlyList<string> closure) => new()
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = "source-backed-analytic-replay-package-blocked",
            BosonPerturbationVectorMaterialization = vectorMaterialization,
            FullReplayPackage = null,
            ClosureRequirements = closure,
        };
}
