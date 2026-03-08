using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Enforces tensor signature consistency (Section 11.1).
/// Prevents silent changes in component order between CPU and GPU.
/// Every tensor-like object must declare its signature and signatures must match
/// when tensors interact.
/// </summary>
public static class TensorSignatureEnforcer
{
    /// <summary>
    /// Verify that a FieldTensor's coefficient count matches the product of its shape.
    /// </summary>
    public static IReadOnlyList<string> ValidateField(FieldTensor field)
    {
        var errors = new List<string>();

        if (field.Signature == null)
        {
            errors.Add($"Field '{field.Label}' has no tensor signature.");
            return errors;
        }

        int expectedSize = 1;
        foreach (var dim in field.Shape)
            expectedSize *= dim;

        if (field.Coefficients.Length != expectedSize)
        {
            errors.Add(
                $"Field '{field.Label}' has {field.Coefficients.Length} coefficients " +
                $"but shape {string.Join("x", field.Shape)} implies {expectedSize}.");
        }

        if (string.IsNullOrWhiteSpace(field.Signature.AmbientSpaceId))
            errors.Add($"Field '{field.Label}' signature has no ambient space ID.");
        if (string.IsNullOrWhiteSpace(field.Signature.CarrierType))
            errors.Add($"Field '{field.Label}' signature has no carrier type.");
        if (string.IsNullOrWhiteSpace(field.Signature.LieAlgebraBasisId))
            errors.Add($"Field '{field.Label}' signature has no Lie algebra basis ID.");
        if (string.IsNullOrWhiteSpace(field.Signature.ComponentOrderId))
            errors.Add($"Field '{field.Label}' signature has no component order ID.");
        if (string.IsNullOrWhiteSpace(field.Signature.MemoryLayout))
            errors.Add($"Field '{field.Label}' signature has no memory layout.");

        return errors;
    }

    /// <summary>
    /// Verify that two tensor signatures are compatible for operations that combine them
    /// (e.g., subtraction S_h - T_h to form Upsilon_h).
    /// </summary>
    public static IReadOnlyList<string> ValidateCompatibility(
        TensorSignature a, string labelA,
        TensorSignature b, string labelB)
    {
        var errors = new List<string>();

        if (a.AmbientSpaceId != b.AmbientSpaceId)
            errors.Add($"Ambient space mismatch: '{labelA}' uses '{a.AmbientSpaceId}' but '{labelB}' uses '{b.AmbientSpaceId}'.");
        if (a.CarrierType != b.CarrierType)
            errors.Add($"Carrier type mismatch: '{labelA}' uses '{a.CarrierType}' but '{labelB}' uses '{b.CarrierType}'.");
        if (a.Degree != b.Degree)
            errors.Add($"Degree mismatch: '{labelA}' has degree '{a.Degree}' but '{labelB}' has degree '{b.Degree}'.");
        if (a.LieAlgebraBasisId != b.LieAlgebraBasisId)
            errors.Add($"Lie algebra basis mismatch: '{labelA}' uses '{a.LieAlgebraBasisId}' but '{labelB}' uses '{b.LieAlgebraBasisId}'.");
        if (a.ComponentOrderId != b.ComponentOrderId)
            errors.Add($"Component order mismatch: '{labelA}' uses '{a.ComponentOrderId}' but '{labelB}' uses '{b.ComponentOrderId}'.");
        if (a.NumericPrecision != b.NumericPrecision)
            errors.Add($"Numeric precision mismatch: '{labelA}' uses '{a.NumericPrecision}' but '{labelB}' uses '{b.NumericPrecision}'.");
        if (a.MemoryLayout != b.MemoryLayout)
            errors.Add($"Memory layout mismatch: '{labelA}' uses '{a.MemoryLayout}' but '{labelB}' uses '{b.MemoryLayout}'.");

        return errors;
    }

    /// <summary>
    /// Verify that a tensor signature is consistent with a branch manifest's conventions.
    /// </summary>
    public static IReadOnlyList<string> ValidateAgainstManifest(
        TensorSignature signature, string label, BranchManifest manifest)
    {
        var errors = new List<string>();

        if (signature.LieAlgebraBasisId != manifest.BasisConventionId &&
            !string.IsNullOrWhiteSpace(manifest.BasisConventionId))
        {
            errors.Add(
                $"Field '{label}' uses Lie algebra basis '{signature.LieAlgebraBasisId}' " +
                $"but branch manifest requires '{manifest.BasisConventionId}'.");
        }

        if (signature.ComponentOrderId != manifest.ComponentOrderId &&
            !string.IsNullOrWhiteSpace(manifest.ComponentOrderId))
        {
            errors.Add(
                $"Field '{label}' uses component order '{signature.ComponentOrderId}' " +
                $"but branch manifest requires '{manifest.ComponentOrderId}'.");
        }

        return errors;
    }

    /// <summary>
    /// Validate a field and throw if invalid.
    /// </summary>
    public static void ValidateFieldOrThrow(FieldTensor field)
    {
        var errors = ValidateField(field);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"FieldTensor '{field.Label}' is invalid. " +
                $"Errors: {string.Join("; ", errors)}");
        }
    }
}
