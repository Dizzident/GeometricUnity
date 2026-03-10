using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;

namespace Gu.Phase3.Spectra.Tests;

internal static class TestHelpers
{
    public static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    public static LieAlgebra TracePairingSu2() =>
        LieAlgebraFactory.CreateSu2WithTracePairing();

    public static BranchManifest TestManifest() => new()
    {
        BranchId = "test-spectra",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "coulomb",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    public static GeometryContext DummyGeometry() => new()
    {
        BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        },
        ObservationBinding = new GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        },
        Patches = Array.Empty<PatchInfo>(),
    };

    public static TensorSignature ConnectionSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "standard",
        ComponentOrderId = "edge-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    public static FieldTensor MakeField(int length, double fillValue = 0.0)
    {
        var coeffs = new double[length];
        if (fillValue != 0.0)
            Array.Fill(coeffs, fillValue);
        return new FieldTensor
        {
            Label = "test-field",
            Signature = ConnectionSignature(),
            Coefficients = coeffs,
            Shape = new[] { length },
        };
    }

    public static FieldTensor MakeRandomField(int length, Random rng)
    {
        var coeffs = new double[length];
        for (int i = 0; i < length; i++)
            coeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        return new FieldTensor
        {
            Label = "random",
            Signature = ConnectionSignature(),
            Coefficients = coeffs,
            Shape = new[] { length },
        };
    }

    public static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    /// <summary>
    /// Build a GaugeProjector from a flat background (omega=0, a0=0).
    /// </summary>
    public static GaugeProjector BuildGaugeProjector(SimplicialMesh mesh, LieAlgebra algebra)
    {
        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var linearization = new GaugeActionLinearization(
            mesh, algebra, a0.ToFieldTensor(), omega.ToFieldTensor(), "test-bg");
        return new GaugeProjector(GaugeBasis.Build(linearization));
    }

    /// <summary>
    /// Minimal ILinearOperator that applies a diagonal operator.
    /// </summary>
    public sealed class DiagonalOperator : ILinearOperator
    {
        private readonly double[] _diag;

        public DiagonalOperator(double[] diag)
        {
            _diag = diag;
        }

        public TensorSignature InputSignature => ConnectionSignature();
        public TensorSignature OutputSignature => ConnectionSignature();
        public int InputDimension => _diag.Length;
        public int OutputDimension => _diag.Length;

        public FieldTensor Apply(FieldTensor v)
        {
            var result = new double[_diag.Length];
            for (int i = 0; i < _diag.Length; i++)
                result[i] = _diag[i] * v.Coefficients[i];
            return new FieldTensor
            {
                Label = "diag*v",
                Signature = OutputSignature,
                Coefficients = result,
                Shape = new[] { _diag.Length },
            };
        }

        public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
    }
}
