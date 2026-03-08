using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Matrix-free linear operator interface. Used for Jacobian J_h and related operators.
/// Implementations may store local element data internally.
/// </summary>
public interface ILinearOperator
{
    /// <summary>Forward action: J * v.</summary>
    FieldTensor Apply(FieldTensor v);

    /// <summary>Transpose action: J^T * v.</summary>
    FieldTensor ApplyTranspose(FieldTensor v);

    /// <summary>Input tensor signature (omega domain).</summary>
    TensorSignature InputSignature { get; }

    /// <summary>Output tensor signature (residual codomain).</summary>
    TensorSignature OutputSignature { get; }

    /// <summary>Number of input DOFs.</summary>
    int InputDimension { get; }

    /// <summary>Number of output DOFs.</summary>
    int OutputDimension { get; }
}
