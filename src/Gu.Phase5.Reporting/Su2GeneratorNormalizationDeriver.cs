using System.Text.Json.Serialization;
using Gu.Math;

namespace Gu.Phase5.Reporting;

public sealed class Su2GeneratorNormalizationDerivationResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("normalizationConventionId")]
    public string? NormalizationConventionId { get; init; }

    [JsonPropertyName("algebraId")]
    public required string AlgebraId { get; init; }

    [JsonPropertyName("basisOrderId")]
    public required string BasisOrderId { get; init; }

    [JsonPropertyName("pairingId")]
    public required string PairingId { get; init; }

    [JsonPropertyName("generatorLabels")]
    public required IReadOnlyList<string> GeneratorLabels { get; init; }

    [JsonPropertyName("structureConstantConvention")]
    public string? StructureConstantConvention { get; init; }

    [JsonPropertyName("physicalTraceNormalization")]
    public string? PhysicalTraceNormalization { get; init; }

    [JsonPropertyName("internalTraceMetricDiagonal")]
    public double? InternalTraceMetricDiagonal { get; init; }

    [JsonPropertyName("physicalTraceMetricDiagonal")]
    public double? PhysicalTraceMetricDiagonal { get; init; }

    [JsonPropertyName("internalToPhysicalGeneratorScale")]
    public double? InternalToPhysicalGeneratorScale { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class Su2GeneratorNormalizationDeriver
{
    public const string AlgorithmId = "phase63-su2-generator-normalization-deriver-v1";
    public const string NormalizationConventionId = "physical-weak-coupling-normalization:su2-canonical-trace-half-v1";

    public static Su2GeneratorNormalizationDerivationResult Derive(LieAlgebra algebra, double tolerance = 1e-12)
    {
        ArgumentNullException.ThrowIfNull(algebra);

        var closure = new List<string>();

        if (!string.Equals(algebra.AlgebraId, "su2", StringComparison.Ordinal))
            closure.Add("algebraId must be su2");
        if (algebra.Dimension != 3)
            closure.Add("SU(2) generator normalization requires dimension 3");
        if (!string.Equals(algebra.BasisOrderId, "canonical", StringComparison.Ordinal))
            closure.Add("basisOrderId must be canonical");
        if (!string.Equals(algebra.PairingId, "trace", StringComparison.Ordinal))
            closure.Add("pairingId must be trace");

        if (algebra.Dimension == 3)
        {
            if (!MetricIsPositiveIdentity(algebra, tolerance))
                closure.Add("trace pairing metric must be positive identity in the canonical generator basis");
            if (!StructureConstantsAreCanonicalSu2(algebra, tolerance))
                closure.Add("structure constants must match epsilon_abc in the canonical SU(2) basis");
        }

        var internalDiagonal = TryGetSharedDiagonal(algebra, tolerance);
        var scale = closure.Count == 0 && internalDiagonal is { } d && d > 0.0
            ? System.Math.Sqrt(0.5 / d)
            : (double?)null;

        return new Su2GeneratorNormalizationDerivationResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "su2-generator-normalization-derived"
                : "su2-generator-normalization-blocked",
            NormalizationConventionId = closure.Count == 0 ? NormalizationConventionId : null,
            AlgebraId = algebra.AlgebraId,
            BasisOrderId = algebra.BasisOrderId,
            PairingId = algebra.PairingId,
            GeneratorLabels = algebra.BasisLabels,
            StructureConstantConvention = closure.Count == 0 ? "[T_a, T_b] = epsilon_abc T_c" : null,
            PhysicalTraceNormalization = closure.Count == 0 ? "tr(t_a t_b) = 1/2 delta_ab" : null,
            InternalTraceMetricDiagonal = internalDiagonal,
            PhysicalTraceMetricDiagonal = closure.Count == 0 ? 0.5 : null,
            InternalToPhysicalGeneratorScale = scale,
            ClosureRequirements = closure,
        };
    }

    private static bool MetricIsPositiveIdentity(LieAlgebra algebra, double tolerance)
    {
        for (var a = 0; a < algebra.Dimension; a++)
        {
            for (var b = 0; b < algebra.Dimension; b++)
            {
                var expected = a == b ? 1.0 : 0.0;
                if (System.Math.Abs(algebra.GetMetricComponent(a, b) - expected) > tolerance)
                    return false;
            }
        }

        return true;
    }

    private static bool StructureConstantsAreCanonicalSu2(LieAlgebra algebra, double tolerance)
    {
        for (var a = 0; a < 3; a++)
        {
            for (var b = 0; b < 3; b++)
            {
                for (var c = 0; c < 3; c++)
                {
                    if (System.Math.Abs(algebra.GetStructureConstant(a, b, c) - Epsilon(a, b, c)) > tolerance)
                        return false;
                }
            }
        }

        return true;
    }

    private static double? TryGetSharedDiagonal(LieAlgebra algebra, double tolerance)
    {
        if (algebra.Dimension <= 0)
            return null;

        var diagonal = algebra.GetMetricComponent(0, 0);
        for (var i = 1; i < algebra.Dimension; i++)
        {
            if (System.Math.Abs(algebra.GetMetricComponent(i, i) - diagonal) > tolerance)
                return null;
        }

        return diagonal;
    }

    private static int Epsilon(int a, int b, int c)
    {
        if (a == b || b == c || a == c)
            return 0;
        if ((a, b, c) is (0, 1, 2) or (1, 2, 0) or (2, 0, 1))
            return 1;
        return -1;
    }
}
