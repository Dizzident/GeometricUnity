using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.CudaInterop;

/// <summary>
/// Arguments for a Phase II branch-parameterized residual kernel launch.
/// Extends Phase I residual evaluation with branch variant dispatch.
/// </summary>
public sealed class Phase2ResidualKernelArgs
{
    /// <summary>Branch variant manifest defining the branch parameters.</summary>
    [JsonPropertyName("variant")]
    public required BranchVariantManifest Variant { get; init; }

    /// <summary>Total field DOFs (edge count * dim G).</summary>
    [JsonPropertyName("fieldDof")]
    public required int FieldDof { get; init; }

    /// <summary>Total residual DOFs (face count * dim G).</summary>
    [JsonPropertyName("residualDof")]
    public required int ResidualDof { get; init; }

    /// <summary>Dimension of the Lie algebra.</summary>
    [JsonPropertyName("dimG")]
    public required int DimG { get; init; }

    /// <summary>Number of mesh edges.</summary>
    [JsonPropertyName("edgeCount")]
    public required int EdgeCount { get; init; }

    /// <summary>Number of mesh faces.</summary>
    [JsonPropertyName("faceCount")]
    public required int FaceCount { get; init; }
}
