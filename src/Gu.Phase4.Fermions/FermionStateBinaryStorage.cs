namespace Gu.Phase4.Fermions;

/// <summary>
/// Binary storage helpers for large fermionic state coefficient arrays.
///
/// Format: little-endian IEEE 754 doubles, in the same order as the
/// Coefficients array (interleaved Re/Im). A 4-byte magic header is
/// prepended for format verification.
///
/// Magic: 0x46 0x53 0x34 0x00 ("FS4\0").
///
/// This format is separate from the JSON metadata: JSON stores the
/// provenance and shape; the binary file stores the raw numbers.
/// The separation allows large states to be loaded efficiently in
/// native code without JSON parsing.
/// </summary>
public static class FermionStateBinaryStorage
{
    private static readonly byte[] Magic = { 0x46, 0x53, 0x34, 0x00 }; // "FS4\0"

    /// <summary>
    /// Write the coefficients of a DiscreteFermionState to a binary stream.
    /// </summary>
    public static void WriteCoefficients(Stream stream, DiscreteFermionState state)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(state);

        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(Magic);
        writer.Write(state.CellCount);
        writer.Write(state.DofsPerCell);
        foreach (var v in state.Coefficients)
            writer.Write(v);
    }

    /// <summary>
    /// Read coefficients from a binary stream. Returns the flat double[] array.
    /// </summary>
    public static double[] ReadCoefficients(Stream stream, out int cellCount, out int dofsPerCell)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual(Magic))
            throw new InvalidDataException("Binary fermion state: invalid magic header.");

        cellCount = reader.ReadInt32();
        dofsPerCell = reader.ReadInt32();

        int count = 2 * cellCount * dofsPerCell;
        var coefficients = new double[count];
        for (int i = 0; i < count; i++)
            coefficients[i] = reader.ReadDouble();

        return coefficients;
    }

    /// <summary>
    /// Write the coefficients of a DiscreteFermionState to a file.
    /// </summary>
    public static void WriteCoefficients(string path, DiscreteFermionState state)
    {
        using var fs = File.Create(path);
        WriteCoefficients(fs, state);
    }

    /// <summary>
    /// Read coefficients from a file. Returns the flat double[] array.
    /// </summary>
    public static double[] ReadCoefficients(string path, out int cellCount, out int dofsPerCell)
    {
        using var fs = File.OpenRead(path);
        return ReadCoefficients(fs, out cellCount, out dofsPerCell);
    }

    /// <summary>
    /// Write eigenvector coefficients for a FermionModeRecord to a binary stream.
    /// Writes a zero-length marker if EigenvectorCoefficients is null.
    /// </summary>
    public static void WriteEigenvector(Stream stream, FermionModeRecord mode)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(mode);

        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(Magic);
        var v = mode.EigenvectorCoefficients;
        int len = v?.Length ?? 0;
        writer.Write(len);
        if (v != null)
            foreach (var x in v)
                writer.Write(x);
    }

    /// <summary>
    /// Read eigenvector coefficients from a binary stream.
    /// Returns null if the stored length is 0 (mode without stored eigenvector).
    /// </summary>
    public static double[]? ReadEigenvector(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual(Magic))
            throw new InvalidDataException("Binary fermion eigenvector: invalid magic header.");

        int len = reader.ReadInt32();
        if (len == 0) return null;

        var v = new double[len];
        for (int i = 0; i < len; i++)
            v[i] = reader.ReadDouble();
        return v;
    }
}
