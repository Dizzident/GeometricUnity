using System.Text.Json.Serialization;
using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Serializable artifact record for a linearized operator bundle.
/// Contains metadata about the bundle (not the operators themselves,
/// which are runtime objects). Used for provenance and replay.
/// </summary>
public sealed class OperatorBundleArtifact
{
    [JsonPropertyName("bundleId")]
    public required string BundleId { get; init; }

    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("operatorType")]
    public required SpectralOperatorType OperatorType { get; init; }

    [JsonPropertyName("formulation")]
    public required PhysicalModeFormulation Formulation { get; init; }

    [JsonPropertyName("backgroundAdmissibility")]
    public required AdmissibilityLevel BackgroundAdmissibility { get; init; }

    [JsonPropertyName("gaugeLambda")]
    public required double GaugeLambda { get; init; }

    [JsonPropertyName("stateDimension")]
    public required int StateDimension { get; init; }

    [JsonPropertyName("physicalDimension")]
    public int? PhysicalDimension { get; init; }

    [JsonPropertyName("gaugeRank")]
    public int? GaugeRank { get; init; }

    [JsonPropertyName("hasResidualCorrection")]
    public bool HasResidualCorrection { get; init; }

    /// <summary>
    /// Create an artifact record from a bundle.
    /// </summary>
    public static OperatorBundleArtifact FromBundle(LinearizedOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        bool hasCorrection = bundle.SpectralOperator is FullHessianOperator fh
            && fh.HasResidualCorrection;

        return new OperatorBundleArtifact
        {
            BundleId = bundle.BundleId,
            BackgroundId = bundle.BackgroundId,
            BranchManifestId = bundle.BranchManifestId,
            OperatorType = bundle.OperatorType,
            Formulation = bundle.Formulation,
            BackgroundAdmissibility = bundle.BackgroundAdmissibility,
            GaugeLambda = bundle.GaugeLambda,
            StateDimension = bundle.StateDimension,
            PhysicalDimension = bundle.PhysicalDimension,
            GaugeRank = bundle.GaugeRank,
            HasResidualCorrection = hasCorrection,
        };
    }
}
