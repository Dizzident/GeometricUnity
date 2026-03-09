using System.Text.Json.Serialization;

namespace Gu.Phase2.Recovery;

/// <summary>
/// Kind of node in the recovery DAG.
/// The mandatory pipeline order is: Native -> Observation -> Extraction -> Interpretation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecoveryNodeKind
{
    /// <summary>Native Y-space source object.</summary>
    Native,

    /// <summary>Observation map / sigma_h descent step.</summary>
    Observation,

    /// <summary>Typed projector/extractor step.</summary>
    Extraction,

    /// <summary>Gate-evaluated final interpretation/classification.</summary>
    Interpretation,
}
