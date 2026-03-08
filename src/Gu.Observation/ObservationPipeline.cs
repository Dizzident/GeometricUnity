using Gu.Core;
using Gu.Geometry;

namespace Gu.Observation;

/// <summary>
/// The sigma_h^* extraction pipeline: sole factory for verified ObservedState.
/// Enforces observation discipline (IA-6): no Y-quantity reaches comparison
/// without passing through this pipeline.
///
/// Flow per request:
///   DerivedState fields (Y_h)
///     -> PullbackOperator (sigma_h^*) -> fields on X_h
///     -> IDerivedObservableTransform (optional)
///     -> INormalizationPolicy
///     -> ObservableSnapshot with ObservationProvenance(IsVerified=true)
///   Collect all -> ObservedState
/// </summary>
public sealed class ObservationPipeline
{
    private readonly PullbackOperator _pullback;
    private readonly IReadOnlyDictionary<string, IDerivedObservableTransform> _transforms;
    private readonly INormalizationPolicy _normalization;

    public ObservationPipeline(
        PullbackOperator pullback,
        IReadOnlyList<IDerivedObservableTransform> transforms,
        INormalizationPolicy normalization)
    {
        _pullback = pullback ?? throw new ArgumentNullException(nameof(pullback));
        _normalization = normalization ?? throw new ArgumentNullException(nameof(normalization));

        var dict = new Dictionary<string, IDerivedObservableTransform>();
        foreach (var t in transforms ?? throw new ArgumentNullException(nameof(transforms)))
        {
            dict[t.ObservableId] = t;
        }
        _transforms = dict;
    }

    /// <summary>
    /// Extract observed quantities from the derived state through sigma_h^*.
    /// This is the only path to create a verified ObservedState.
    /// </summary>
    public ObservedState Extract(
        DerivedState derived,
        DiscreteState sourceState,
        GeometryContext ctx,
        IReadOnlyList<ObservableRequest> requests,
        BranchManifest branch)
    {
        ArgumentNullException.ThrowIfNull(derived);
        ArgumentNullException.ThrowIfNull(sourceState);
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(requests);
        ArgumentNullException.ThrowIfNull(branch);

        // Step 1: Pull back all DerivedState fields from Y_h to X_h via sigma_h^*
        var pulledBack = PullBackDerivedState(derived);

        // Step 2: For each request, compute the observable
        var pipelineTimestamp = DateTimeOffset.UtcNow;
        var observables = new Dictionary<string, ObservableSnapshot>();

        foreach (var request in requests)
        {
            var snapshot = ProcessRequest(request, pulledBack, branch, pipelineTimestamp);
            observables[request.ObservableId] = snapshot;
        }

        // Step 3: Assemble ObservedState
        return new ObservedState
        {
            ObservationBranchId = branch.ActiveObservationBranch,
            Observables = observables,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = pipelineTimestamp,
                CodeRevision = branch.CodeRevision,
                Branch = new BranchRef
                {
                    BranchId = branch.BranchId,
                    SchemaVersion = branch.SchemaVersion,
                },
                Notes = $"Extracted via ObservationPipeline with {requests.Count} observables",
            },
        };
    }

    private PulledBackFields PullBackDerivedState(DerivedState derived)
    {
        // Determine components per vertex from the shape of each field.
        // Fields with shape [n] are scalar; fields with shape [n, c] are multi-component.
        return new PulledBackFields
        {
            CurvatureF = PullBackField(derived.CurvatureF),
            TorsionT = PullBackField(derived.TorsionT),
            ShiabS = PullBackField(derived.ShiabS),
            ResidualUpsilon = PullBackField(derived.ResidualUpsilon),
        };
    }

    private FieldTensor PullBackField(FieldTensor yField)
    {
        // Dispatch pullback based on the field's differential form degree.
        // Degree "0" -> vertex-based, "1" -> edge-based, "2" -> face-based.
        var degree = yField.Signature.Degree;

        if (degree == "2")
        {
            // Face-based 2-form (curvature F_h, torsion T_h, shiab S_h, residual Upsilon_h).
            // Shape: [FaceCount, componentsPerFace] or [FaceCount] for scalar-valued 2-forms.
            int componentsPerFace = yField.Shape.Count >= 2 ? yField.Shape[1] : 1;
            return componentsPerFace == 1 && yField.Shape.Count == 1
                ? _pullback.ApplyFaceField(yField, 1)
                : _pullback.ApplyFaceField(yField, componentsPerFace);
        }

        if (degree == "1")
        {
            // Edge-based 1-form (connection omega_h).
            int componentsPerEdge = yField.Shape.Count >= 2 ? yField.Shape[1] : 1;
            return _pullback.ApplyEdgeField(yField, componentsPerEdge);
        }

        // Degree "0" or other: vertex-based field.
        if (yField.Shape.Count == 1)
        {
            return _pullback.ApplyVertexScalar(yField);
        }

        if (yField.Shape.Count == 2)
        {
            int componentsPerVertex = yField.Shape[1];
            return _pullback.ApplyVertexMultiComponent(yField, componentsPerVertex);
        }

        // For higher-rank tensors, flatten to multi-component vertex field
        int totalComponents = 1;
        for (int i = 1; i < yField.Shape.Count; i++)
            totalComponents *= yField.Shape[i];

        return _pullback.ApplyVertexMultiComponent(yField, totalComponents);
    }

    private ObservableSnapshot ProcessRequest(
        ObservableRequest request,
        PulledBackFields pulledBack,
        BranchManifest branch,
        DateTimeOffset pipelineTimestamp)
    {
        double[] values;
        string? transformId = null;
        OutputType outputType = request.OutputType;
        TensorSignature? signature = null;

        if (_transforms.TryGetValue(request.ObservableId, out var transform))
        {
            // Derived observable: compute via transform
            values = transform.Compute(pulledBack, request);
            transformId = transform.TransformId;
            outputType = transform.OutputType;
        }
        else
        {
            // Direct field passthrough based on well-known observable IDs
            var field = ResolveDirectField(request.ObservableId, pulledBack);
            values = new double[field.Coefficients.Length];
            Array.Copy(field.Coefficients, values, values.Length);
            signature = field.Signature;
        }

        // Normalization
        var (normalizedValues, normMeta) = _normalization.Normalize(values, request);

        // Provenance: pipeline has verified this observable
        var provenance = new ObservationProvenance
        {
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = branch.ActiveObservationBranch,
            TransformId = transformId,
            IsVerified = true,
            PipelineTimestamp = pipelineTimestamp,
        };

        return new ObservableSnapshot
        {
            ObservableId = request.ObservableId,
            OutputType = outputType,
            Values = normalizedValues,
            Normalization = normMeta,
            Signature = signature,
            Provenance = provenance,
        };
    }

    private static FieldTensor ResolveDirectField(string observableId, PulledBackFields pulledBack)
    {
        return observableId switch
        {
            "curvature" => pulledBack.CurvatureF,
            "torsion" => pulledBack.TorsionT,
            "shiab" => pulledBack.ShiabS,
            "residual" => pulledBack.ResidualUpsilon,
            _ => throw new ArgumentException(
                $"Unknown observable '{observableId}'. Register an IDerivedObservableTransform for custom observables."),
        };
    }
}
