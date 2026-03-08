using Gu.Core;
using Gu.Geometry;
using Gu.Observation;
using Gu.Observation.Transforms;

namespace Gu.Observation.Tests;

public class ObservationPipelineTests
{
    /// <summary>
    /// Creates a toy fiber bundle: X_h has 2 vertices, Y_h has 4 vertices.
    /// sigma_h maps: x=0 -> y=0, x=1 -> y=2.
    /// </summary>
    private static FiberBundleMesh CreateToyBundle()
    {
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,
                0, 1,
                1, 0,
                1, 1,
            },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 2, 3 },
            });

        return new FiberBundleMesh
        {
            BaseMesh = xMesh,
            AmbientMesh = yMesh,
            YVertexToXVertex = new[] { 0, 0, 1, 1 },
            FiberVerticesPerXVertex = new[]
            {
                new[] { 0, 1 },
                new[] { 2, 3 },
            },
            XVertexToYVertex = new[] { 0, 2 },
            XCellToYCell = new[] { 0 },
            SectionCoefficients = new[]
            {
                new[] { 1.0, 0.0, 0.0 },
            },
        };
    }

    private static TensorSignature MakeScalarSig(string ambientSpaceId = "Y_h") => new()
    {
        AmbientSpaceId = ambientSpaceId,
        CarrierType = "scalar",
        Degree = "0",
        LieAlgebraBasisId = "trivial",
        ComponentOrderId = "natural",
        MemoryLayout = "dense-row-major",
    };

    private static TensorSignature MakeMultiComponentSig(string ambientSpaceId = "Y_h") => new()
    {
        AmbientSpaceId = ambientSpaceId,
        CarrierType = "vector",
        Degree = "0",
        LieAlgebraBasisId = "su2-standard",
        ComponentOrderId = "natural",
        MemoryLayout = "dense-row-major",
    };

    private static DerivedState CreateToyDerivedState()
    {
        // Y_h has 4 vertices, scalar fields
        return new DerivedState
        {
            CurvatureF = new FieldTensor
            {
                Label = "F_h",
                Signature = MakeScalarSig(),
                Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0 },
                Shape = new[] { 4 },
            },
            TorsionT = new FieldTensor
            {
                Label = "T_h",
                Signature = MakeScalarSig(),
                Coefficients = new double[] { 0.1, 0.2, 0.3, 0.4 },
                Shape = new[] { 4 },
            },
            ShiabS = new FieldTensor
            {
                Label = "S_h",
                Signature = MakeScalarSig(),
                Coefficients = new double[] { 0.5, 0.6, 0.7, 0.8 },
                Shape = new[] { 4 },
            },
            ResidualUpsilon = new FieldTensor
            {
                Label = "Upsilon_h",
                Signature = MakeScalarSig(),
                Coefficients = new double[] { 0.4, 0.4, 0.4, 0.4 },
                Shape = new[] { 4 },
            },
        };
    }

    private static DerivedState CreateMultiComponentDerivedState()
    {
        // Y_h has 4 vertices, 3 components each (e.g., su(2))
        return new DerivedState
        {
            CurvatureF = new FieldTensor
            {
                Label = "F_h",
                Signature = MakeMultiComponentSig(),
                Coefficients = new double[]
                {
                    1, 2, 3,   // y=0
                    4, 5, 6,   // y=1
                    7, 8, 9,   // y=2
                    10, 11, 12 // y=3
                },
                Shape = new[] { 4, 3 },
            },
            TorsionT = new FieldTensor
            {
                Label = "T_h",
                Signature = MakeMultiComponentSig(),
                Coefficients = new double[]
                {
                    0.1, 0.2, 0.3,
                    0.4, 0.5, 0.6,
                    0.7, 0.8, 0.9,
                    1.0, 1.1, 1.2,
                },
                Shape = new[] { 4, 3 },
            },
            ShiabS = new FieldTensor
            {
                Label = "S_h",
                Signature = MakeMultiComponentSig(),
                Coefficients = new double[12],
                Shape = new[] { 4, 3 },
            },
            ResidualUpsilon = new FieldTensor
            {
                Label = "Upsilon_h",
                Signature = MakeMultiComponentSig(),
                Coefficients = new double[]
                {
                    1, 0, 0,   // y=0
                    0, 1, 0,   // y=1
                    0, 0, 1,   // y=2
                    1, 1, 1,   // y=3
                },
                Shape = new[] { 4, 3 },
            },
        };
    }

    private static DiscreteState CreateToyDiscreteState(FiberBundleMesh bundle)
    {
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        return new DiscreteState
        {
            Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            Geometry = ctx,
            Omega = new FieldTensor
            {
                Label = "omega_h",
                Signature = MakeScalarSig(),
                Coefficients = new double[] { 0, 0, 0, 0 },
                Shape = new[] { 4 },
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "test-rev",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            },
        };
    }

    private static BranchManifest CreateToyManifest() => new()
    {
        BranchId = "test-branch",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "test-rev",
        ActiveGeometryBranch = "simplicial-1d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 1,
        AmbientDimension = 2,
        LieAlgebraId = "u1",
        BasisConventionId = "basis-standard",
        ComponentOrderId = "order-row-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-killing",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = new[] { "IA-6" },
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static ObservationPipeline CreatePipeline(FiberBundleMesh bundle)
    {
        var pullback = new PullbackOperator(bundle);
        var transforms = new IDerivedObservableTransform[]
        {
            new ResidualNormTransform(),
            new CurvatureNormTransform(),
            new TopologicalChargeTransform(),
            new ObjectivePassthroughTransform(),
        };
        var normalization = new DimensionlessNormalizationPolicy();
        return new ObservationPipeline(pullback, transforms, normalization);
    }

    [Fact]
    public void Extract_DirectCurvature_PullsBackCorrectly()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);

        Assert.Single(observed.Observables);
        Assert.True(observed.Observables.ContainsKey("curvature"));

        var snap = observed.Observables["curvature"];
        // sigma: x=0 -> y=0 (val 1.0), x=1 -> y=2 (val 3.0)
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(1.0, snap.Values[0]);
        Assert.Equal(3.0, snap.Values[1]);
    }

    [Fact]
    public void Extract_DirectResidual_PullsBackCorrectly()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "residual",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);

        var snap = observed.Observables["residual"];
        // Residual is constant 0.4 on Y_h
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(0.4, snap.Values[0]);
        Assert.Equal(0.4, snap.Values[1]);
    }

    [Fact]
    public void Extract_AllProvenanceFieldsSet()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["curvature"];

        // ObservationProvenance must be set and verified
        Assert.NotNull(snap.Provenance);
        Assert.True(snap.Provenance!.IsVerified);
        Assert.Equal("sigma_h_star", snap.Provenance.PullbackOperatorId);
        Assert.Equal("sigma-pullback", snap.Provenance.ObservationBranchId);
        Assert.Null(snap.Provenance.TransformId); // direct field, no transform

        // ObservedState-level provenance
        Assert.Equal("sigma-pullback", observed.ObservationBranchId);
        Assert.Equal("test-rev", observed.Provenance.CodeRevision);
    }

    [Fact]
    public void Extract_DerivedTransform_SetsTransformId()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "residual-norm-squared",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["residual-norm-squared"];

        Assert.NotNull(snap.Provenance);
        Assert.Equal("residual-norm-squared-v1", snap.Provenance!.TransformId);
        Assert.True(snap.Provenance.IsVerified);
    }

    [Fact]
    public void Extract_ResidualNormSquared_ScalarField()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "residual-norm-squared",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["residual-norm-squared"];

        // Residual is constant 0.4 on Y_h, pulled back: x=0->0.4, x=1->0.4
        // For scalar fields, norm squared = val^2
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(0.16, snap.Values[0], 1e-12);
        Assert.Equal(0.16, snap.Values[1], 1e-12);
    }

    [Fact]
    public void Extract_ResidualNormSquared_MultiComponent()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateMultiComponentDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "residual-norm-squared",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["residual-norm-squared"];

        // Multi-component: Upsilon at y=0 is (1,0,0), y=2 is (0,0,1)
        // sigma: x=0 -> y=0, x=1 -> y=2
        // ||Upsilon(x=0)||^2 = 1^2 + 0^2 + 0^2 = 1.0
        // ||Upsilon(x=1)||^2 = 0^2 + 0^2 + 1^2 = 1.0
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(1.0, snap.Values[0], 1e-12);
        Assert.Equal(1.0, snap.Values[1], 1e-12);
    }

    [Fact]
    public void Extract_TopologicalCharge_SumsCorrectly()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "topological-charge",
                OutputType = OutputType.ExactStructural,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["topological-charge"];

        // Curvature pulled back: x=0->1.0, x=1->3.0; sum = 4.0
        // values[0] = charge, values[1] = integrality deviation
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(4.0, snap.Values[0], 1e-12);
        Assert.Equal(0.0, snap.Values[1], 1e-12); // 4.0 is already integer
        // v1 proxy is ExactStructural (placeholder, not a true FEM integral)
        Assert.Equal(OutputType.ExactStructural, snap.OutputType);
    }

    [Fact]
    public void Extract_ObjectivePassthrough_CopiesAllValues()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "residual-passthrough",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["residual-passthrough"];

        // Residual pulled back: x=0->0.4, x=1->0.4
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(0.4, snap.Values[0], 1e-12);
        Assert.Equal(0.4, snap.Values[1], 1e-12);
    }

    [Fact]
    public void Extract_WithNormalization_AppliesScaleFactor()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
                Normalization = new NormalizationMeta
                {
                    SchemeId = "scale-by-2",
                    ScaleFactor = 2.0,
                },
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["curvature"];

        // Curvature pulled back: x=0->1.0, x=1->3.0, then *2
        Assert.Equal(2.0, snap.Values[0], 1e-12);
        Assert.Equal(6.0, snap.Values[1], 1e-12);
        Assert.NotNull(snap.Normalization);
        Assert.Equal(2.0, snap.Normalization!.ScaleFactor);
    }

    [Fact]
    public void Extract_WithoutNormalization_NoMetaAttached()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        Assert.Null(observed.Observables["curvature"].Normalization);
    }

    [Fact]
    public void Extract_MultipleRequests_AllPresent()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new ObservableRequest[]
        {
            new() { ObservableId = "curvature", OutputType = OutputType.Quantitative },
            new() { ObservableId = "residual-norm-squared", OutputType = OutputType.Quantitative },
            new() { ObservableId = "topological-charge", OutputType = OutputType.ExactStructural },
            new() { ObservableId = "residual-passthrough", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);

        Assert.Equal(4, observed.Observables.Count);
        Assert.True(observed.Observables.ContainsKey("curvature"));
        Assert.True(observed.Observables.ContainsKey("residual-norm-squared"));
        Assert.True(observed.Observables.ContainsKey("topological-charge"));
        Assert.True(observed.Observables.ContainsKey("residual-passthrough"));

        // All must have verified provenance
        foreach (var (_, snap) in observed.Observables)
        {
            Assert.NotNull(snap.Provenance);
            Assert.True(snap.Provenance!.IsVerified);
        }
    }

    [Fact]
    public void Extract_UnknownObservableId_ThrowsArgumentException()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "nonexistent-observable",
                OutputType = OutputType.Quantitative,
            },
        };

        Assert.Throws<ArgumentException>(() =>
            pipeline.Extract(derived, source, ctx, requests, manifest));
    }

    [Fact]
    public void Extract_NullDerivedState_Throws()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        Assert.Throws<ArgumentNullException>(() =>
            pipeline.Extract(null!, source, ctx, new ObservableRequest[0], manifest));
    }

    [Fact]
    public void Extract_SignatureOnDirectField_HasXhAmbientSpace()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateToyDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["curvature"];

        // Direct field should carry the pulled-back signature (X_h, not Y_h)
        Assert.NotNull(snap.Signature);
        Assert.Equal("X_h", snap.Signature!.AmbientSpaceId);
    }

    [Fact]
    public void Extract_CurvatureNormSquared_MultiComponent()
    {
        var bundle = CreateToyBundle();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateMultiComponentDerivedState();
        var source = CreateToyDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateToyManifest();

        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature-norm-squared",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["curvature-norm-squared"];

        // Curvature at y=0: (1,2,3), y=2: (7,8,9)
        // sigma: x=0 -> y=0, x=1 -> y=2
        // ||F(x=0)||^2 = 1+4+9 = 14
        // ||F(x=1)||^2 = 49+64+81 = 194
        Assert.Equal(2, snap.Values.Length);
        Assert.Equal(14.0, snap.Values[0], 1e-12);
        Assert.Equal(194.0, snap.Values[1], 1e-12);
    }

    [Fact]
    public void PipelineConstructor_NullPullback_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ObservationPipeline(null!, Array.Empty<IDerivedObservableTransform>(),
                new DimensionlessNormalizationPolicy()));
    }

    [Fact]
    public void PipelineConstructor_NullTransforms_Throws()
    {
        var bundle = CreateToyBundle();
        var pullback = new PullbackOperator(bundle);
        Assert.Throws<ArgumentNullException>(() =>
            new ObservationPipeline(pullback, null!, new DimensionlessNormalizationPolicy()));
    }

    [Fact]
    public void PipelineConstructor_NullNormalization_Throws()
    {
        var bundle = CreateToyBundle();
        var pullback = new PullbackOperator(bundle);
        Assert.Throws<ArgumentNullException>(() =>
            new ObservationPipeline(pullback, Array.Empty<IDerivedObservableTransform>(), null!));
    }

    [Fact]
    public void DimensionlessNormalization_NoRequest_PassesThrough()
    {
        var policy = new DimensionlessNormalizationPolicy();
        var values = new double[] { 1.0, 2.0, 3.0 };
        var request = new ObservableRequest
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
        };

        var (result, meta) = policy.Normalize(values, request);

        Assert.Same(values, result); // no copy, same reference
        Assert.Null(meta);
    }

    [Fact]
    public void DimensionlessNormalization_ScaleFactorOne_ReturnsSame()
    {
        var policy = new DimensionlessNormalizationPolicy();
        var values = new double[] { 1.0, 2.0 };
        var request = new ObservableRequest
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
            Normalization = new NormalizationMeta { SchemeId = "unity", ScaleFactor = 1.0 },
        };

        var (result, meta) = policy.Normalize(values, request);

        Assert.Same(values, result);
        Assert.NotNull(meta);
    }

    [Fact]
    public void DimensionlessNormalization_NonTrivialScale_AppliesCorrectly()
    {
        var policy = new DimensionlessNormalizationPolicy();
        var values = new double[] { 2.0, 4.0, 6.0 };
        var request = new ObservableRequest
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
            Normalization = new NormalizationMeta { SchemeId = "half", ScaleFactor = 0.5 },
        };

        var (result, meta) = policy.Normalize(values, request);

        Assert.NotSame(values, result); // new array
        Assert.Equal(1.0, result[0], 1e-12);
        Assert.Equal(2.0, result[1], 1e-12);
        Assert.Equal(3.0, result[2], 1e-12);
    }
}

/// <summary>
/// Tests for face-based 2-form field pullback through the observation pipeline.
/// Uses ToyGeometryFactory.CreateToy2D() which provides a 2D X_h mesh with faces,
/// matching the physical reality that curvature/torsion/shiab/residual are 2-forms.
/// </summary>
public class ObservationPipelineFaceFieldTests
{
    private static TensorSignature Make2FormSig(string ambientSpaceId = "Y_h") => new()
    {
        AmbientSpaceId = ambientSpaceId,
        CarrierType = "curvature-2form",
        Degree = "2",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "natural",
        MemoryLayout = "dense-row-major",
    };

    private static DerivedState CreateFaceBasedDerivedState(FiberBundleMesh bundle)
    {
        int yFaceCount = bundle.AmbientMesh.FaceCount;
        // Scalar-valued 2-forms: one value per face on Y_h
        var fCoeffs = new double[yFaceCount];
        var tCoeffs = new double[yFaceCount];
        var sCoeffs = new double[yFaceCount];
        var uCoeffs = new double[yFaceCount];
        for (int i = 0; i < yFaceCount; i++)
        {
            fCoeffs[i] = (i + 1) * 1.0;
            tCoeffs[i] = (i + 1) * 0.1;
            sCoeffs[i] = (i + 1) * 0.5;
            uCoeffs[i] = (i + 1) * 0.01;
        }

        return new DerivedState
        {
            CurvatureF = new FieldTensor
            {
                Label = "F_h",
                Signature = Make2FormSig(),
                Coefficients = fCoeffs,
                Shape = new[] { yFaceCount },
            },
            TorsionT = new FieldTensor
            {
                Label = "T_h",
                Signature = Make2FormSig(),
                Coefficients = tCoeffs,
                Shape = new[] { yFaceCount },
            },
            ShiabS = new FieldTensor
            {
                Label = "S_h",
                Signature = Make2FormSig(),
                Coefficients = sCoeffs,
                Shape = new[] { yFaceCount },
            },
            ResidualUpsilon = new FieldTensor
            {
                Label = "Upsilon_h",
                Signature = Make2FormSig(),
                Coefficients = uCoeffs,
                Shape = new[] { yFaceCount },
            },
        };
    }

    private static DerivedState CreateMultiComponentFaceDerivedState(FiberBundleMesh bundle, int componentsPerFace)
    {
        int yFaceCount = bundle.AmbientMesh.FaceCount;
        int total = yFaceCount * componentsPerFace;
        var fCoeffs = new double[total];
        var tCoeffs = new double[total];
        var sCoeffs = new double[total];
        var uCoeffs = new double[total];
        for (int i = 0; i < total; i++)
        {
            fCoeffs[i] = i + 1.0;
            tCoeffs[i] = (i + 1.0) * 0.1;
            sCoeffs[i] = (i + 1.0) * 0.5;
            uCoeffs[i] = (i + 1.0) * 0.01;
        }

        return new DerivedState
        {
            CurvatureF = new FieldTensor
            {
                Label = "F_h",
                Signature = Make2FormSig(),
                Coefficients = fCoeffs,
                Shape = new[] { yFaceCount, componentsPerFace },
            },
            TorsionT = new FieldTensor
            {
                Label = "T_h",
                Signature = Make2FormSig(),
                Coefficients = tCoeffs,
                Shape = new[] { yFaceCount, componentsPerFace },
            },
            ShiabS = new FieldTensor
            {
                Label = "S_h",
                Signature = Make2FormSig(),
                Coefficients = sCoeffs,
                Shape = new[] { yFaceCount, componentsPerFace },
            },
            ResidualUpsilon = new FieldTensor
            {
                Label = "Upsilon_h",
                Signature = Make2FormSig(),
                Coefficients = uCoeffs,
                Shape = new[] { yFaceCount, componentsPerFace },
            },
        };
    }

    private static DiscreteState CreateDiscreteState(FiberBundleMesh bundle)
    {
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        return new DiscreteState
        {
            Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            Geometry = ctx,
            Omega = new FieldTensor
            {
                Label = "omega_h",
                Signature = new TensorSignature
                {
                    AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                    LieAlgebraBasisId = "canonical", ComponentOrderId = "natural",
                    MemoryLayout = "dense-row-major",
                },
                Coefficients = new double[bundle.AmbientMesh.EdgeCount],
                Shape = new[] { bundle.AmbientMesh.EdgeCount },
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "test-rev",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
            },
        };
    }

    private static BranchManifest CreateManifest() => new()
    {
        BranchId = "test-branch",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "test-rev",
        ActiveGeometryBranch = "simplicial-2d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 2,
        AmbientDimension = 5,
        LieAlgebraId = "su2",
        BasisConventionId = "basis-standard",
        ComponentOrderId = "order-row-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-killing",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = new[] { "IA-6" },
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static ObservationPipeline CreatePipeline(FiberBundleMesh bundle)
    {
        var pullback = new PullbackOperator(bundle);
        var transforms = new IDerivedObservableTransform[]
        {
            new ResidualNormTransform(),
            new CurvatureNormTransform(),
            new TopologicalChargeTransform(),
            new ObjectivePassthroughTransform(),
        };
        return new ObservationPipeline(pullback, transforms, new DimensionlessNormalizationPolicy());
    }

    [Fact]
    public void Extract_FaceBased2Form_PullsBackToXhFaces()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateFaceBasedDerivedState(bundle);
        var source = CreateDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateManifest();

        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["curvature"];

        // X_h has faces; pulled-back field should have X_h face count values
        Assert.Equal(bundle.BaseMesh.FaceCount, snap.Values.Length);
        Assert.NotNull(snap.Signature);
        Assert.Equal("X_h", snap.Signature!.AmbientSpaceId);
        Assert.Equal("2", snap.Signature.Degree);
    }

    [Fact]
    public void Extract_FaceBased2Form_AllFieldsHaveVerifiedProvenance()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateFaceBasedDerivedState(bundle);
        var source = CreateDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateManifest();

        var requests = new ObservableRequest[]
        {
            new() { ObservableId = "curvature", OutputType = OutputType.Quantitative },
            new() { ObservableId = "torsion", OutputType = OutputType.Quantitative },
            new() { ObservableId = "shiab", OutputType = OutputType.Quantitative },
            new() { ObservableId = "residual", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);

        Assert.Equal(4, observed.Observables.Count);
        foreach (var (_, snap) in observed.Observables)
        {
            Assert.NotNull(snap.Provenance);
            Assert.True(snap.Provenance!.IsVerified);
            Assert.Equal("sigma_h_star", snap.Provenance.PullbackOperatorId);
            Assert.Equal(bundle.BaseMesh.FaceCount, snap.Values.Length);
        }
    }

    [Fact]
    public void Extract_MultiComponentFace2Form_PullsBackCorrectly()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pipeline = CreatePipeline(bundle);
        int componentsPerFace = 3; // e.g., dim(su2) = 3
        var derived = CreateMultiComponentFaceDerivedState(bundle, componentsPerFace);
        var source = CreateDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateManifest();

        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["curvature"];

        // Multi-component face field: X_h faces * components per face
        Assert.Equal(bundle.BaseMesh.FaceCount * componentsPerFace, snap.Values.Length);
    }

    [Fact]
    public void Extract_FaceBasedResidualNorm_ComputesCorrectly()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pipeline = CreatePipeline(bundle);
        var derived = CreateFaceBasedDerivedState(bundle);
        var source = CreateDiscreteState(bundle);
        var ctx = bundle.ToGeometryContext("midpoint", "P1");
        var manifest = CreateManifest();

        var requests = new[]
        {
            new ObservableRequest { ObservableId = "residual-norm-squared", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, source, ctx, requests, manifest);
        var snap = observed.Observables["residual-norm-squared"];

        // Scalar face field: norm squared = value^2 per face
        Assert.Equal(bundle.BaseMesh.FaceCount, snap.Values.Length);
        // All values should be non-negative (squares)
        Assert.All(snap.Values, v => Assert.True(v >= 0));
    }
}
