using Gu.Core;
using Gu.Geometry;
using Gu.Observation;

namespace Gu.Core.Tests;

public class ObservationPipelineTests
{
    private static FiberBundleMesh CreateBundle()
    {
        return ToyGeometryFactory.CreateToy2D();
    }

    private static BranchManifest CreateBranch()
    {
        return new BranchManifest
        {
            BranchId = "test-branch",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
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
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };
    }

    private static FieldTensor MakeYField(FiberBundleMesh bundle, string label, string carrier, string degree)
    {
        // Use correct entity count based on differential form degree
        int n = degree switch
        {
            "2" => bundle.AmbientMesh.FaceCount,
            "1" => bundle.AmbientMesh.EdgeCount,
            _ => bundle.AmbientMesh.VertexCount,
        };
        var coeffs = new double[n];
        for (int i = 0; i < n; i++)
            coeffs[i] = i * 0.1;

        return new FieldTensor
        {
            Label = label,
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = carrier,
                Degree = degree,
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { n },
        };
    }

    private static (DerivedState, DiscreteState, GeometryContext) MakeTestInputs(FiberBundleMesh bundle)
    {
        var derived = new DerivedState
        {
            CurvatureF = MakeYField(bundle, "F_h", "curvature-2form", "2"),
            TorsionT = MakeYField(bundle, "T_h", "torsion-2form", "2"),
            ShiabS = MakeYField(bundle, "S_h", "torsion-2form", "2"),
            ResidualUpsilon = MakeYField(bundle, "Upsilon_h", "torsion-2form", "2"),
        };

        var ctx = bundle.ToGeometryContext("centroid", "P1");

        var state = new DiscreteState
        {
            Branch = new BranchRef
            {
                BranchId = "test-branch",
                SchemaVersion = "1.0.0",
            },
            Geometry = ctx,
            Omega = MakeYField(bundle, "omega_h", "connection-1form", "1"),
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = new BranchRef
                {
                    BranchId = "test-branch",
                    SchemaVersion = "1.0.0",
                },
            },
        };

        return (derived, state, ctx);
    }

    [Fact]
    public void Extract_DirectCurvature_ProducesVerifiedObservable()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
            },
        };

        var observed = pipeline.Extract(derived, state, ctx, requests, branch);

        Assert.Single(observed.Observables);
        Assert.True(observed.Observables.ContainsKey("curvature"));
        var snap = observed.Observables["curvature"];
        Assert.NotNull(snap.Provenance);
        Assert.True(snap.Provenance!.IsVerified,
            "IA-6: Observable must be verified as having passed through sigma_h^*");
        Assert.Equal("sigma_h_star", snap.Provenance.PullbackOperatorId);
    }

    [Fact]
    public void Extract_AllDirectFields_Available()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "torsion", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "shiab", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "residual", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, state, ctx, requests, branch);

        Assert.Equal(4, observed.Observables.Count);
        Assert.All(observed.Observables.Values, snap =>
        {
            Assert.NotNull(snap.Provenance);
            Assert.True(snap.Provenance!.IsVerified);
        });
    }

    [Fact]
    public void Extract_ValuesLiveOnXh_NotYh()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, state, ctx, requests, branch);
        var snap = observed.Observables["curvature"];

        // Curvature is a 2-form (degree "2"), so values live on X_h faces, not vertices
        Assert.Equal(bundle.BaseMesh.FaceCount, snap.Values.Length);
    }

    [Fact]
    public void Extract_UnknownObservable_Throws()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var requests = new[]
        {
            new ObservableRequest { ObservableId = "nonexistent", OutputType = OutputType.Quantitative },
        };

        Assert.Throws<ArgumentException>(() =>
            pipeline.Extract(derived, state, ctx, requests, branch));
    }

    [Fact]
    public void Extract_ObservationBranchId_CopiedFromManifest()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, state, ctx, requests, branch);

        Assert.Equal("sigma-pullback", observed.ObservationBranchId);
    }

    [Fact]
    public void Extract_Provenance_HasBranchInfo()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, state, ctx, requests, branch);

        Assert.NotNull(observed.Provenance);
        Assert.Equal("abc123", observed.Provenance.CodeRevision);
        Assert.Equal("test-branch", observed.Provenance.Branch!.BranchId);
    }

    [Fact]
    public void Extract_WithNormalization_AppliesScaleFactor()
    {
        var bundle = CreateBundle();
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var (derived, state, ctx) = MakeTestInputs(bundle);
        var branch = CreateBranch();
        var normMeta = new NormalizationMeta
        {
            SchemeId = "dimensionless",
            ScaleFactor = 2.0,
            Description = "test scaling",
        };
        var requests = new[]
        {
            new ObservableRequest
            {
                ObservableId = "curvature",
                OutputType = OutputType.Quantitative,
                Normalization = normMeta,
            },
        };

        var observed = pipeline.Extract(derived, state, ctx, requests, branch);
        var snap = observed.Observables["curvature"];

        Assert.NotNull(snap.Normalization);
        Assert.Equal(2.0, snap.Normalization!.ScaleFactor);
    }
}

public class DimensionlessNormalizationPolicyTests
{
    [Fact]
    public void NoNormalization_ReturnsValuesUnchanged()
    {
        var policy = new DimensionlessNormalizationPolicy();
        var request = new ObservableRequest
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
            Normalization = null,
        };

        var values = new double[] { 1.0, 2.0, 3.0 };
        var (result, meta) = policy.Normalize(values, request);

        Assert.Same(values, result);
        Assert.Null(meta);
    }

    [Fact]
    public void UnitScaleFactor_ReturnsValuesUnchanged()
    {
        var policy = new DimensionlessNormalizationPolicy();
        var normMeta = new NormalizationMeta { SchemeId = "dimensionless", ScaleFactor = 1.0 };
        var request = new ObservableRequest
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
            Normalization = normMeta,
        };

        var values = new double[] { 1.0, 2.0, 3.0 };
        var (result, meta) = policy.Normalize(values, request);

        Assert.Same(values, result);
        Assert.Equal(normMeta, meta);
    }

    [Fact]
    public void NonUnitScaleFactor_ScalesValues()
    {
        var policy = new DimensionlessNormalizationPolicy();
        var normMeta = new NormalizationMeta { SchemeId = "dimensionless", ScaleFactor = 0.5 };
        var request = new ObservableRequest
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
            Normalization = normMeta,
        };

        var values = new double[] { 2.0, 4.0, 6.0 };
        var (result, meta) = policy.Normalize(values, request);

        Assert.NotSame(values, result);
        Assert.Equal(new double[] { 1.0, 2.0, 3.0 }, result);
        Assert.Equal(normMeta, meta);
    }
}

public class PulledBackFieldsTests
{
    [Fact]
    public void AllFieldsRequired()
    {
        var field = new FieldTensor
        {
            Label = "test",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = "test",
                Degree = "0",
                LieAlgebraBasisId = "trivial",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[] { 1.0 },
            Shape = new[] { 1 },
        };

        var pbf = new PulledBackFields
        {
            CurvatureF = field,
            TorsionT = field,
            ShiabS = field,
            ResidualUpsilon = field,
        };

        Assert.NotNull(pbf.CurvatureF);
        Assert.NotNull(pbf.TorsionT);
        Assert.NotNull(pbf.ShiabS);
        Assert.NotNull(pbf.ResidualUpsilon);
    }
}
