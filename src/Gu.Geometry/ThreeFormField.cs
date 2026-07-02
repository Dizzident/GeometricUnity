namespace Gu.Geometry;

/// <summary>
/// An ad-valued (or scalar) discrete 3-form sampled on the volumes (3-subsimplices)
/// of a <see cref="SimplicialMesh"/>. Coefficients use the flat carrier layout
/// [volumeIdx * ComponentsPerVolume + component], matching the ad-valued layout
/// used elsewhere in the platform (dimG passed as <see cref="ComponentsPerVolume"/>;
/// 1 for a scalar 3-form).
/// </summary>
public sealed class ThreeFormField
{
    /// <summary>The mesh whose volumes carry this 3-form.</summary>
    public SimplicialMesh Mesh { get; }

    /// <summary>Number of components per volume (e.g. dim(g); 1 for a scalar 3-form).</summary>
    public int ComponentsPerVolume { get; }

    /// <summary>
    /// Flat coefficient array of length Mesh.VolumeCount * ComponentsPerVolume,
    /// laid out as [volumeIdx * ComponentsPerVolume + component].
    /// </summary>
    public double[] Coefficients { get; }

    /// <summary>
    /// Constructs a 3-form field, validating that the coefficient array length
    /// equals Mesh.VolumeCount * ComponentsPerVolume.
    /// </summary>
    public ThreeFormField(SimplicialMesh mesh, int componentsPerVolume, double[] coefficients)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(coefficients);
        if (componentsPerVolume < 1)
            throw new ArgumentOutOfRangeException(nameof(componentsPerVolume), "Must be >= 1.");

        int expected = mesh.VolumeCount * componentsPerVolume;
        if (coefficients.Length != expected)
            throw new ArgumentException(
                $"Coefficient array length {coefficients.Length} does not match VolumeCount * ComponentsPerVolume = {expected}.",
                nameof(coefficients));

        Mesh = mesh;
        ComponentsPerVolume = componentsPerVolume;
        Coefficients = coefficients;
    }
}
