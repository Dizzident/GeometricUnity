using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Stability.Tests;

public class RandomizedSvdProbeTests
{
    [Fact]
    public void MethodId_IsRandomizedSvd()
    {
        var probe = new RandomizedSvdProbe();
        Assert.Equal("randomized-svd", probe.MethodId);
    }

    [Fact]
    public void SingularValues_OfKnownDiagonalMatrix_AreCorrect()
    {
        // A diagonal matrix has singular values = |diagonal entries|
        var diag = new[] { 3.0, 1.0, 4.0, 1.5, 2.0 };
        var op = new RsvdTestOperator(diag);
        var probe = new RandomizedSvdProbe(oversampling: 3, powerIterations: 2);

        var result = probe.ComputeSmallestSingularValues(op, k: 3);

        Assert.True(result.Values.Length >= 3, $"Expected at least 3 values, got {result.Values.Length}");
        // Smallest 3 singular values should be 1.0, 1.5, 2.0 (sorted ascending)
        Assert.True(System.Math.Abs(result.Values[0] - 1.0) < 0.1,
            $"Smallest singular value should be ~1.0, got {result.Values[0]:E6}");
        Assert.True(System.Math.Abs(result.Values[1] - 1.5) < 0.1,
            $"Second singular value should be ~1.5, got {result.Values[1]:E6}");
        Assert.True(System.Math.Abs(result.Values[2] - 2.0) < 0.1,
            $"Third singular value should be ~2.0, got {result.Values[2]:E6}");
    }

    [Fact]
    public void SigmaMin_DetectsNearlyRankDeficient()
    {
        // Nearly rank-deficient: one very small singular value
        var diag = new[] { 1.0, 1.0, 1e-10, 1.0, 1.0 };
        var op = new RsvdTestOperator(diag);
        var probe = new RandomizedSvdProbe(oversampling: 3, powerIterations: 3);

        var result = probe.ComputeSmallestSingularValues(op, k: 1);

        Assert.True(result.Values.Length >= 1, "Should compute at least 1 singular value");
        Assert.True(result.Values[0] < 1e-6,
            $"Smallest singular value should be near 0 (got {result.Values[0]:E6})");
    }

    [Fact]
    public void SingularValues_NonSquareOperator_Works()
    {
        // Non-square: 3x5 operator with singular values [0.5, 2.0, 5.0]
        // The operator maps R^5 -> R^3, so it has a 2D null space.
        // The full singular value list is [0, 0, 0.5, 2.0, 5.0].
        // Requesting k=3 smallest should give [0, 0, 0.5].
        var op = new RectangularTestOperator(
            rows: 3, cols: 5,
            singularValues: new[] { 0.5, 2.0, 5.0 });
        var probe = new RandomizedSvdProbe(oversampling: 3, powerIterations: 2);

        var result = probe.ComputeSmallestSingularValues(op, k: 3);

        Assert.True(result.Values.Length >= 3, $"Expected at least 3 values, got {result.Values.Length}");
        // First two should be ~0 (null space)
        Assert.True(result.Values[0] < 0.1,
            $"First singular value should be ~0 (null space), got {result.Values[0]:E6}");
        Assert.True(result.Values[1] < 0.1,
            $"Second singular value should be ~0 (null space), got {result.Values[1]:E6}");
        // Third should be the smallest non-zero singular value ~0.5
        Assert.True(System.Math.Abs(result.Values[2] - 0.5) < 0.2,
            $"Third singular value should be ~0.5, got {result.Values[2]:E6}");
    }

    [Fact]
    public void AgreesWithLanczos_OnSquareOperator()
    {
        var diag = new[] { 0.1, 0.5, 1.0, 2.0, 5.0 };
        var op = new RsvdTestOperator(diag);

        var rsvd = new RandomizedSvdProbe(oversampling: 3, powerIterations: 2);
        var lanczos = new LanczosSpectrumProbe();

        var rsvdResult = rsvd.ComputeSmallestSingularValues(op, k: 3);
        var lanczosResult = lanczos.ComputeSmallestSingularValues(op, k: 3);

        int common = System.Math.Min(rsvdResult.Values.Length, lanczosResult.Values.Length);
        Assert.True(common >= 3, "Both should produce at least 3 singular values");

        for (int i = 0; i < common; i++)
        {
            double relDiff = System.Math.Abs(rsvdResult.Values[i] - lanczosResult.Values[i]) /
                (1.0 + System.Math.Abs(lanczosResult.Values[i]));
            Assert.True(relDiff < 0.1,
                $"Singular value {i}: RSVD={rsvdResult.Values[i]:E6}, " +
                $"Lanczos={lanczosResult.Values[i]:E6}, relDiff={relDiff:E6}");
        }
    }

    [Fact]
    public void ComputeSmallestEigenvalues_RequiresSquareOperator()
    {
        var op = new RectangularTestOperator(rows: 3, cols: 5,
            singularValues: new[] { 1.0, 2.0, 3.0 });
        var probe = new RandomizedSvdProbe();

        Assert.Throws<ArgumentException>(() =>
            probe.ComputeSmallestEigenvalues(op, k: 2));
    }

    [Fact]
    public void InvalidK_Throws()
    {
        var op = new RsvdTestOperator(new[] { 1.0, 2.0 });
        var probe = new RandomizedSvdProbe();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            probe.ComputeSmallestSingularValues(op, k: 0));
    }
}

/// <summary>
/// Square diagonal operator for randomized SVD testing.
/// </summary>
internal sealed class RsvdTestOperator : ILinearOperator
{
    private readonly double[] _diag;
    private readonly TensorSignature _sig;

    public RsvdTestOperator(double[] diagonal)
    {
        _diag = diagonal;
        _sig = new TensorSignature
        {
            AmbientSpaceId = "test",
            CarrierType = "test-vector",
            Degree = "0",
            LieAlgebraBasisId = "test",
            ComponentOrderId = "natural",
            MemoryLayout = "dense",
        };
    }

    public TensorSignature InputSignature => _sig;
    public TensorSignature OutputSignature => _sig;
    public int InputDimension => _diag.Length;
    public int OutputDimension => _diag.Length;

    public FieldTensor Apply(FieldTensor v)
    {
        var result = new double[_diag.Length];
        for (int i = 0; i < _diag.Length; i++)
            result[i] = _diag[i] * v.Coefficients[i];
        return new FieldTensor
        {
            Label = "rsvd_test_out",
            Signature = _sig,
            Coefficients = result,
            Shape = [_diag.Length],
        };
    }

    public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v); // Diagonal is self-transpose
}

/// <summary>
/// Rectangular operator with known singular values for testing.
/// Constructs as U * Sigma * V^T where U,V are identity-like and Sigma has given singular values.
/// </summary>
internal sealed class RectangularTestOperator : ILinearOperator
{
    private readonly int _rows;
    private readonly int _cols;
    private readonly double[] _singularValues;
    private readonly TensorSignature _inputSig;
    private readonly TensorSignature _outputSig;

    public RectangularTestOperator(int rows, int cols, double[] singularValues)
    {
        _rows = rows;
        _cols = cols;
        _singularValues = singularValues;
        _inputSig = new TensorSignature
        {
            AmbientSpaceId = "test",
            CarrierType = "test-input",
            Degree = "0",
            LieAlgebraBasisId = "test",
            ComponentOrderId = "natural",
            MemoryLayout = "dense",
        };
        _outputSig = new TensorSignature
        {
            AmbientSpaceId = "test",
            CarrierType = "test-output",
            Degree = "0",
            LieAlgebraBasisId = "test",
            ComponentOrderId = "natural",
            MemoryLayout = "dense",
        };
    }

    public TensorSignature InputSignature => _inputSig;
    public TensorSignature OutputSignature => _outputSig;
    public int InputDimension => _cols;
    public int OutputDimension => _rows;

    public FieldTensor Apply(FieldTensor v)
    {
        // A = diag(sigma) padded: result[i] = sigma[i] * v[i] for i < min(rows, cols, sigmas)
        var result = new double[_rows];
        int minDim = System.Math.Min(_rows, System.Math.Min(_cols, _singularValues.Length));
        for (int i = 0; i < minDim; i++)
            result[i] = _singularValues[i] * v.Coefficients[i];
        return new FieldTensor
        {
            Label = "rect_test_out",
            Signature = _outputSig,
            Coefficients = result,
            Shape = [_rows],
        };
    }

    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        // A^T = diag(sigma) padded: result[i] = sigma[i] * v[i] for i < min(rows, cols, sigmas)
        var result = new double[_cols];
        int minDim = System.Math.Min(_rows, System.Math.Min(_cols, _singularValues.Length));
        for (int i = 0; i < minDim; i++)
            result[i] = _singularValues[i] * v.Coefficients[i];
        return new FieldTensor
        {
            Label = "rect_test_transpose_out",
            Signature = _inputSig,
            Coefficients = result,
            Shape = [_cols],
        };
    }
}
