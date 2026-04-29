using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class NonProxyFermionCurrentMatrixElementRecord
{
    [JsonPropertyName("matrixElementId")]
    public required string MatrixElementId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("sourceKind")]
    public required string SourceKind { get; init; }

    [JsonPropertyName("derivationMethod")]
    public required string DerivationMethod { get; init; }

    [JsonPropertyName("normalizationConventionId")]
    public string? NormalizationConventionId { get; init; }

    [JsonPropertyName("operatorSource")]
    public required string OperatorSource { get; init; }

    [JsonPropertyName("matrixElementFormula")]
    public required string MatrixElementFormula { get; init; }

    [JsonPropertyName("usesFiniteDifferenceProxy")]
    public required bool UsesFiniteDifferenceProxy { get; init; }

    [JsonPropertyName("usesCouplingProxyMagnitude")]
    public required bool UsesCouplingProxyMagnitude { get; init; }

    [JsonPropertyName("producesDimensionlessCouplingValue")]
    public required bool ProducesDimensionlessCouplingValue { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class NonProxyFermionCurrentMatrixElementDeriver
{
    public const string MatrixElementId = "phase64-non-proxy-fermion-current-matrix-element";
    public const string SourceKind = "non-proxy-fermion-current-matrix-element";
    public const string DerivationMethod = "analytic-dirac-variation-matrix-element:v1";
    public const string OperatorSource = "Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical";
    public const string MatrixElementFormula = "<phi_i, delta_D[b_k] phi_j>";

    public static NonProxyFermionCurrentMatrixElementRecord Derive(
        Su2GeneratorNormalizationDerivationResult generatorNormalization,
        bool analyticDiracVariationAvailable,
        bool usesFiniteDifferenceProxy,
        bool usesCouplingProxyMagnitude)
    {
        ArgumentNullException.ThrowIfNull(generatorNormalization);

        var closure = new List<string>();
        if (!string.Equals(
                generatorNormalization.TerminalStatus,
                "su2-generator-normalization-derived",
                StringComparison.Ordinal))
            closure.Add("canonical SU(2) generator normalization has not been derived");
        if (string.IsNullOrWhiteSpace(generatorNormalization.NormalizationConventionId))
            closure.Add("normalization convention id is missing");
        if (!analyticDiracVariationAvailable)
            closure.Add("analytical Dirac variation operator source is missing");
        if (usesFiniteDifferenceProxy)
            closure.Add("matrix element source must not use finite-difference coupling proxies");
        if (usesCouplingProxyMagnitude)
            closure.Add("matrix element source must not use coupling proxy magnitudes");

        return new NonProxyFermionCurrentMatrixElementRecord
        {
            MatrixElementId = MatrixElementId,
            TerminalStatus = closure.Count == 0
                ? "non-proxy-fermion-current-matrix-element-derived"
                : "non-proxy-fermion-current-matrix-element-blocked",
            SourceKind = SourceKind,
            DerivationMethod = DerivationMethod,
            NormalizationConventionId = generatorNormalization.NormalizationConventionId,
            OperatorSource = OperatorSource,
            MatrixElementFormula = MatrixElementFormula,
            UsesFiniteDifferenceProxy = usesFiniteDifferenceProxy,
            UsesCouplingProxyMagnitude = usesCouplingProxyMagnitude,
            ProducesDimensionlessCouplingValue = false,
            ClosureRequirements = closure,
        };
    }
}
