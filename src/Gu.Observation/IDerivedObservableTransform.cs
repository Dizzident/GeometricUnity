using Gu.Core;

namespace Gu.Observation;

/// <summary>
/// A transform that computes a derived observable from pulled-back fields.
/// Each implementation maps a specific ObservableRequest.ObservableId
/// to extracted values from the pulled-back DerivedState fields.
/// </summary>
public interface IDerivedObservableTransform
{
    /// <summary>The observable ID this transform handles.</summary>
    string ObservableId { get; }

    /// <summary>The output type classification of the derived observable.</summary>
    OutputType OutputType { get; }

    /// <summary>A stable identifier for provenance tracking.</summary>
    string TransformId { get; }

    /// <summary>
    /// Compute the derived observable values from pulled-back fields.
    /// </summary>
    /// <param name="pulledBackFields">Fields already pulled back to X_h via sigma_h^*.</param>
    /// <param name="request">The observable request being fulfilled.</param>
    /// <returns>The computed observable values on X_h.</returns>
    double[] Compute(PulledBackFields pulledBackFields, ObservableRequest request);
}
