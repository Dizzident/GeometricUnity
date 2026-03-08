using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class LinearizationStateTests
{
    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new LinearizationState
        {
            Jacobian = new LinearOperatorModel
            {
                Label = "jacobian",
                RealizationType = "sparse-csr",
                Rows = 10,
                Cols = 6,
                Values = new double[] { 1.0, 2.0, 3.0 },
                RowPointers = new int[] { 0, 1, 2, 3 },
                ColIndices = new int[] { 0, 1, 2 }
            },
            Adjoint = new LinearOperatorModel
            {
                Label = "adjoint",
                RealizationType = "sparse-csr",
                Rows = 6,
                Cols = 10
            },
            GradientLikeResidual = new FieldTensor
            {
                Label = "J^T M Upsilon",
                Signature = new TensorSignature
                {
                    AmbientSpaceId = "Y_h",
                    CarrierType = "connection-1form",
                    Degree = "1",
                    LieAlgebraBasisId = "su2-standard",
                    ComponentOrderId = "lexicographic",
                    MemoryLayout = "dense-row-major"
                },
                Coefficients = new double[] { 0.1, 0.2, 0.3 },
                Shape = new[] { 3 }
            },
            SpectralDiagnostics = new Dictionary<string, double>
            {
                ["conditionNumber"] = 42.5,
                ["smallestSingularValue"] = 0.01
            }
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<LinearizationState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("jacobian", deserialized.Jacobian.Label);
        Assert.Equal(10, deserialized.Jacobian.Rows);
        Assert.Equal(6, deserialized.Jacobian.Cols);
        Assert.NotNull(deserialized.Adjoint);
        Assert.Equal(6, deserialized.Adjoint.Rows);
        Assert.NotNull(deserialized.SpectralDiagnostics);
        Assert.Equal(42.5, deserialized.SpectralDiagnostics["conditionNumber"]);
    }

    [Fact]
    public void Adjoint_IsOptional()
    {
        // Per Section 11.2: operator must provide explicit adjoint or declare it approximated
        var state = new LinearizationState
        {
            Jacobian = new LinearOperatorModel
            {
                Label = "jacobian",
                RealizationType = "matrix-free",
                Rows = 10,
                Cols = 6
            },
            GradientLikeResidual = new FieldTensor
            {
                Label = "gradient",
                Signature = new TensorSignature
                {
                    AmbientSpaceId = "Y_h",
                    CarrierType = "connection-1form",
                    Degree = "1",
                    LieAlgebraBasisId = "su2-standard",
                    ComponentOrderId = "lexicographic",
                    MemoryLayout = "dense-row-major"
                },
                Coefficients = new[] { 0.0 },
                Shape = new[] { 1 }
            }
        };

        Assert.Null(state.Adjoint);
    }

    [Fact]
    public void MatrixFreeOperator_HasNoSparseData()
    {
        var op = new LinearOperatorModel
        {
            Label = "jacobian-mf",
            RealizationType = "matrix-free",
            Rows = 100,
            Cols = 60
        };

        Assert.Null(op.Values);
        Assert.Null(op.RowPointers);
        Assert.Null(op.ColIndices);
    }
}
