using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records metadata about a linearization computation at a background state.
/// Per IMPLEMENTATION_PLAN_P2.md section 8.2.
/// </summary>
public sealed class LinearizationRecord
{
    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Branch manifest ID.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Linearized operator definition identifier.</summary>
    [JsonPropertyName("operatorDefinitionId")]
    public required string OperatorDefinitionId { get; init; }

    /// <summary>
    /// Derivative mode: "analytic", "AD", "finite-difference", "hybrid".
    /// </summary>
    [JsonPropertyName("derivativeMode")]
    public required string DerivativeMode { get; init; }

    /// <summary>Input (domain) dimension.</summary>
    [JsonPropertyName("inputDimension")]
    public required int InputDimension { get; init; }

    /// <summary>Output (codomain) dimension.</summary>
    [JsonPropertyName("outputDimension")]
    public required int OutputDimension { get; init; }

    /// <summary>Domain (input) tensor signature.</summary>
    [JsonPropertyName("domainSignature")]
    public TensorSignature? DomainSignature { get; init; }

    /// <summary>Codomain (output) tensor signature.</summary>
    [JsonPropertyName("codomainSignature")]
    public TensorSignature? CodomainSignature { get; init; }

    /// <summary>
    /// Gauge handling mode: "raw" (no gauge fixing), "coulomb-slice", "penalty-augmented".
    /// </summary>
    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }

    /// <summary>
    /// Assembly mode: "matrix-free", "explicit-sparse", "block-hybrid".
    /// </summary>
    [JsonPropertyName("assemblyMode")]
    public required string AssemblyMode { get; init; }

    /// <summary>
    /// Validation status: "verified", "unverified", "failed".
    /// </summary>
    [JsonPropertyName("validationStatus")]
    public required string ValidationStatus { get; init; }

    /// <summary>Finite-difference verification max absolute error (if verified).</summary>
    [JsonPropertyName("fdMaxAbsoluteError")]
    public double? FdMaxAbsoluteError { get; init; }

    /// <summary>Finite-difference verification max relative error (if verified).</summary>
    [JsonPropertyName("fdMaxRelativeError")]
    public double? FdMaxRelativeError { get; init; }
}
