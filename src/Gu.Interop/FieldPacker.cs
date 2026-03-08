using Gu.Core;

namespace Gu.Interop;

/// <summary>
/// Converts between semantic FieldTensor types and packed GPU buffers.
/// Handles SoA layout creation and data packing/unpacking.
/// This is the bridge between the C# solver world and the native GPU world.
/// </summary>
public static class FieldPacker
{
    /// <summary>
    /// Create a SoA buffer layout for a FieldTensor based on its shape.
    /// For shape [n, c], creates c components each with n elements.
    /// For shape [n], creates 1 component with n elements.
    /// </summary>
    public static BufferLayoutDescriptor CreateLayout(FieldTensor field, string layoutId)
    {
        ArgumentNullException.ThrowIfNull(field);

        int elementsPerComponent;
        int componentCount;

        if (field.Shape.Count == 1)
        {
            elementsPerComponent = field.Shape[0];
            componentCount = 1;
        }
        else if (field.Shape.Count == 2)
        {
            elementsPerComponent = field.Shape[0];
            componentCount = field.Shape[1];
        }
        else
        {
            // Higher rank: first dimension is spatial, rest are flattened
            elementsPerComponent = field.Shape[0];
            componentCount = 1;
            for (int i = 1; i < field.Shape.Count; i++)
                componentCount *= field.Shape[i];
        }

        var componentNames = new string[componentCount];
        for (int i = 0; i < componentCount; i++)
            componentNames[i] = $"{field.Label}_{i}";

        return BufferLayoutDescriptor.CreateSoA(
            layoutId,
            componentNames,
            elementsPerComponent,
            field.Signature.NumericPrecision ?? "float64");
    }

    /// <summary>
    /// Pack a FieldTensor's coefficients into SoA order for GPU upload.
    /// Row-major FieldTensor [n, c] -> SoA: [comp0_0..comp0_n, comp1_0..comp1_n, ...]
    /// </summary>
    public static double[] PackToSoA(FieldTensor field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field.Shape.Count <= 1)
        {
            // Scalar field or 1D -- no reordering needed
            var copy = new double[field.Coefficients.Length];
            Array.Copy(field.Coefficients, copy, copy.Length);
            return copy;
        }

        int n = field.Shape[0]; // spatial dimension
        int c = 1;
        for (int i = 1; i < field.Shape.Count; i++)
            c *= field.Shape[i];

        // Input: row-major [n, c] -> element[i,j] = coefficients[i*c + j]
        // Output: SoA [c, n] -> element[j,i] = packed[j*n + i]
        var packed = new double[n * c];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < c; j++)
            {
                packed[j * n + i] = field.Coefficients[i * c + j];
            }
        }
        return packed;
    }

    /// <summary>
    /// Unpack SoA data from GPU download back into a FieldTensor.
    /// SoA: [comp0_0..comp0_n, comp1_0..comp1_n, ...] -> row-major [n, c]
    /// </summary>
    public static FieldTensor UnpackFromSoA(
        ReadOnlySpan<double> soaData,
        string label,
        TensorSignature signature,
        IReadOnlyList<int> shape)
    {
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(shape);

        if (shape.Count <= 1)
        {
            var coefficients = new double[soaData.Length];
            soaData.CopyTo(coefficients);
            return new FieldTensor
            {
                Label = label,
                Signature = signature,
                Coefficients = coefficients,
                Shape = shape,
            };
        }

        int n = shape[0];
        int c = 1;
        for (int i = 1; i < shape.Count; i++)
            c *= shape[i];

        // Input: SoA [c, n] -> element[j,i] = soaData[j*n + i]
        // Output: row-major [n, c] -> element[i,j] = coefficients[i*c + j]
        var unpacked = new double[n * c];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < c; j++)
            {
                unpacked[i * c + j] = soaData[j * n + i];
            }
        }

        return new FieldTensor
        {
            Label = label,
            Signature = signature,
            Coefficients = unpacked,
            Shape = shape,
        };
    }

    /// <summary>
    /// Upload a FieldTensor to a GPU buffer via SoA packing.
    /// </summary>
    public static void UploadField(INativeBackend backend, FieldTensor field, PackedBuffer buffer)
    {
        var packed = PackToSoA(field);
        backend.UploadBuffer(buffer, packed);
    }

    /// <summary>
    /// Download a FieldTensor from a GPU buffer via SoA unpacking.
    /// </summary>
    public static FieldTensor DownloadField(
        INativeBackend backend,
        PackedBuffer buffer,
        string label,
        TensorSignature signature,
        IReadOnlyList<int> shape)
    {
        var soaData = new double[buffer.Layout.TotalElements];
        backend.DownloadBuffer(buffer, soaData);
        return UnpackFromSoA(soaData, label, signature, shape);
    }
}
