using Gu.Core;

namespace Gu.ReferenceCpu;

/// <summary>
/// Static arithmetic operations on FieldTensor. Creates new immutable instances.
/// Validates carrier type compatibility before operations.
/// </summary>
public static class FieldTensorOps
{
    /// <summary>Element-wise subtraction. Validates matching signatures.</summary>
    public static FieldTensor Subtract(FieldTensor a, FieldTensor b)
    {
        ValidateCompatible(a, b);
        var result = new double[a.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coefficients[i] - b.Coefficients[i];

        return new FieldTensor
        {
            Label = $"({a.Label} - {b.Label})",
            Signature = a.Signature,
            Coefficients = result,
            Shape = a.Shape,
        };
    }

    /// <summary>Element-wise addition. Validates matching signatures.</summary>
    public static FieldTensor Add(FieldTensor a, FieldTensor b)
    {
        ValidateCompatible(a, b);
        var result = new double[a.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coefficients[i] + b.Coefficients[i];

        return new FieldTensor
        {
            Label = $"({a.Label} + {b.Label})",
            Signature = a.Signature,
            Coefficients = result,
            Shape = a.Shape,
        };
    }

    /// <summary>Scalar multiplication.</summary>
    public static FieldTensor Scale(FieldTensor a, double scalar)
    {
        var result = new double[a.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coefficients[i] * scalar;

        return new FieldTensor
        {
            Label = a.Label,
            Signature = a.Signature,
            Coefficients = result,
            Shape = a.Shape,
        };
    }

    /// <summary>a + alpha * b (fused add-scale). Validates matching signatures.</summary>
    public static FieldTensor AddScaled(FieldTensor a, FieldTensor b, double alpha)
    {
        ValidateCompatible(a, b);
        var result = new double[a.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coefficients[i] + alpha * b.Coefficients[i];

        return new FieldTensor
        {
            Label = a.Label,
            Signature = a.Signature,
            Coefficients = result,
            Shape = a.Shape,
        };
    }

    /// <summary>Flat dot product of coefficient arrays (no mass matrix).</summary>
    public static double Dot(FieldTensor a, FieldTensor b)
    {
        ValidateCompatible(a, b);
        double sum = 0;
        for (int i = 0; i < a.Coefficients.Length; i++)
            sum += a.Coefficients[i] * b.Coefficients[i];
        return sum;
    }

    /// <summary>L2 norm of coefficients (no mass matrix).</summary>
    public static double L2Norm(FieldTensor a)
    {
        double sum = 0;
        for (int i = 0; i < a.Coefficients.Length; i++)
            sum += a.Coefficients[i] * a.Coefficients[i];
        return System.Math.Sqrt(sum);
    }

    /// <summary>Create zero tensor with same shape/signature.</summary>
    public static FieldTensor ZerosLike(FieldTensor template)
    {
        return new FieldTensor
        {
            Label = "zero",
            Signature = template.Signature,
            Coefficients = new double[template.Coefficients.Length],
            Shape = template.Shape,
        };
    }

    /// <summary>
    /// Create zero tensor with a specified label, shape, and signature.
    /// </summary>
    public static FieldTensor Zeros(string label, TensorSignature signature, IReadOnlyList<int> shape)
    {
        int size = 1;
        foreach (var dim in shape)
            size *= dim;

        return new FieldTensor
        {
            Label = label,
            Signature = signature,
            Coefficients = new double[size],
            Shape = shape,
        };
    }

    /// <summary>
    /// Validate that two FieldTensors have strictly identical TensorSignatures.
    /// All signature fields must match (not just CarrierType) because the coefficient
    /// arrays must represent the same basis, ordering, and layout for element-wise
    /// arithmetic to be physically meaningful. Per physicist confirmation: strict
    /// TensorSignature identity is required for Upsilon = S - T.
    /// </summary>
    public static void ValidateCompatible(FieldTensor a, FieldTensor b)
    {
        if (a.Coefficients.Length != b.Coefficients.Length)
            throw new InvalidOperationException(
                $"Cannot operate on fields '{a.Label}' and '{b.Label}': " +
                $"coefficient lengths differ ({a.Coefficients.Length} vs {b.Coefficients.Length}).");

        var sa = a.Signature;
        var sb = b.Signature;
        var mismatches = new List<string>();

        if (sa.CarrierType != sb.CarrierType)
            mismatches.Add($"CarrierType ('{sa.CarrierType}' vs '{sb.CarrierType}')");
        if (sa.AmbientSpaceId != sb.AmbientSpaceId)
            mismatches.Add($"AmbientSpaceId ('{sa.AmbientSpaceId}' vs '{sb.AmbientSpaceId}')");
        if (sa.Degree != sb.Degree)
            mismatches.Add($"Degree ('{sa.Degree}' vs '{sb.Degree}')");
        if (sa.LieAlgebraBasisId != sb.LieAlgebraBasisId)
            mismatches.Add($"LieAlgebraBasisId ('{sa.LieAlgebraBasisId}' vs '{sb.LieAlgebraBasisId}')");
        if (sa.ComponentOrderId != sb.ComponentOrderId)
            mismatches.Add($"ComponentOrderId ('{sa.ComponentOrderId}' vs '{sb.ComponentOrderId}')");
        if (sa.NumericPrecision != sb.NumericPrecision)
            mismatches.Add($"NumericPrecision ('{sa.NumericPrecision}' vs '{sb.NumericPrecision}')");
        if (sa.MemoryLayout != sb.MemoryLayout)
            mismatches.Add($"MemoryLayout ('{sa.MemoryLayout}' vs '{sb.MemoryLayout}')");

        if (mismatches.Count > 0)
            throw new InvalidOperationException(
                $"Cannot operate on fields '{a.Label}' and '{b.Label}': " +
                $"TensorSignature mismatch in {string.Join(", ", mismatches)}. " +
                $"Strict signature identity is required for arithmetic operations.");
    }
}
