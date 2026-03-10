using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.CudaSpectra;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.CudaSpectra.Tests;

internal static class TestHelpers
{
    public static LieAlgebra TracePairingSu2 => LieAlgebraFactory.CreateSu2WithTracePairing();

    public static BranchManifest TestManifest => new()
    {
        BranchId = "branch-1",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test-rev",
        LieAlgebraId = "su2",
        BaseDimension = 2,
        AmbientDimension = 5,
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        PairingConventionId = "pairing-trace",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    public static FieldTensor MakeField(int length, double fillValue = 0.0)
    {
        var coeffs = new double[length];
        if (fillValue != 0.0)
            Array.Fill(coeffs, fillValue);
        return new FieldTensor
        {
            Label = "test-field",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { length },
        };
    }

    public static FieldTensor MakeRandomField(int length, int seed = 42)
    {
        var rng = new Random(seed);
        var coeffs = new double[length];
        for (int i = 0; i < length; i++)
            coeffs[i] = rng.NextDouble() - 0.5;
        return new FieldTensor
        {
            Label = "random-field",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
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
    /// Simple diagonal operator for testing: multiplies each component by a constant.
    /// </summary>
    public sealed class DiagonalOperator : ILinearOperator
    {
        private readonly double _scale;
        private readonly int _dim;
        private readonly TensorSignature _sig;

        public DiagonalOperator(double scale, int dim)
        {
            _scale = scale;
            _dim = dim;
            _sig = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            };
        }

        public TensorSignature InputSignature => _sig;
        public TensorSignature OutputSignature => _sig;
        public int InputDimension => _dim;
        public int OutputDimension => _dim;

        public FieldTensor Apply(FieldTensor v)
        {
            var result = new double[_dim];
            for (int i = 0; i < _dim; i++)
                result[i] = _scale * v.Coefficients[i];
            return new FieldTensor
            {
                Label = "scaled",
                Signature = _sig,
                Coefficients = result,
                Shape = new[] { _dim },
            };
        }

        public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
    }

    /// <summary>
    /// A simple ISpectralKernel with known eigenvalues for testing LOBPCG.
    /// Spectral operator: diag(1, 2, 3, ..., n).
    /// Mass operator: identity.
    /// Eigenvalues should be 1, 2, 3, ...
    /// </summary>
    public sealed class DiagonalSpectralKernel : ISpectralKernel
    {
        private readonly int _n;

        public DiagonalSpectralKernel(int n)
        {
            _n = n;
        }

        public int StateDimension => _n;
        public int ResidualDimension => _n;

        public void ApplySpectral(ReadOnlySpan<double> v, Span<double> result)
        {
            for (int i = 0; i < _n; i++)
                result[i] = (i + 1.0) * v[i];
        }

        public void ApplyMass(ReadOnlySpan<double> v, Span<double> result)
        {
            v[.._n].CopyTo(result);
        }

        public void ApplyJacobian(ReadOnlySpan<double> v, Span<double> result)
        {
            for (int i = 0; i < _n; i++)
                result[i] = System.Math.Sqrt(i + 1.0) * v[i];
        }

        public void ApplyAdjoint(ReadOnlySpan<double> w, Span<double> result)
        {
            for (int i = 0; i < _n; i++)
                result[i] = System.Math.Sqrt(i + 1.0) * w[i];
        }
    }
}
