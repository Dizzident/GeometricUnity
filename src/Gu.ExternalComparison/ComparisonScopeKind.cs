using System.Text.Json.Serialization;

namespace Gu.ExternalComparison;

/// <summary>
/// Classification of what a comparison is testing (Section 18.3).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComparisonScopeKind
{
    /// <summary>Tests raw branch structure before observation.</summary>
    RawBranchStructure,

    /// <summary>Tests the observation map itself (sigma_h^* consistency).</summary>
    ObservationMap,

    /// <summary>Tests against an auxiliary effective model.</summary>
    AuxiliaryEffectiveModel,

    /// <summary>Tests numerical implementation correctness.</summary>
    NumericalImplementation,

    /// <summary>Combined scope.</summary>
    Combination,
}
