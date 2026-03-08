namespace Gu.VulkanViewer;

/// <summary>
/// Maps scalar field values to RGBA colors using configurable color maps.
/// Supports scientific visualization color schemes including viridis, plasma,
/// coolwarm, and diverging palettes.
/// </summary>
public sealed class ColorMapper
{
    /// <summary>Known color scheme names.</summary>
    public static readonly IReadOnlyList<string> AvailableSchemes = new[]
    {
        "viridis", "plasma", "coolwarm", "diverging",
    };

    private readonly string _schemeName;

    /// <summary>
    /// Initializes a new <see cref="ColorMapper"/> with the specified color scheme.
    /// </summary>
    /// <param name="schemeName">
    /// Name of the color scheme. Supported values: "viridis", "plasma", "coolwarm", "diverging".
    /// Defaults to "viridis" if not specified.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemeName"/> is not a recognized scheme.</exception>
    public ColorMapper(string schemeName = "viridis")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemeName);

        if (!AvailableSchemes.Contains(schemeName))
        {
            throw new ArgumentException(
                $"Unknown color scheme '{schemeName}'. Available: {string.Join(", ", AvailableSchemes)}",
                nameof(schemeName));
        }

        _schemeName = schemeName;
    }

    /// <summary>The active color scheme name.</summary>
    public string SchemeName => _schemeName;

    /// <summary>
    /// Maps a normalized parameter t in [0, 1] to an RGBA color tuple.
    /// Values outside [0, 1] are clamped.
    /// </summary>
    /// <param name="t">Normalized parameter in [0, 1].</param>
    /// <returns>RGBA components each in [0, 1].</returns>
    public (float R, float G, float B, float A) Map(double t)
    {
        float tc = Math.Clamp((float)t, 0f, 1f);

        return _schemeName switch
        {
            "viridis" => MapViridis(tc),
            "plasma" => MapPlasma(tc),
            "coolwarm" => MapCoolWarm(tc),
            "diverging" => MapDiverging(tc),
            _ => MapViridis(tc),
        };
    }

    /// <summary>
    /// Maps an array of scalar field values to a flat RGBA array.
    /// Values are linearly normalized from [minValue, maxValue] to [0, 1],
    /// then mapped through the active color scheme.
    /// </summary>
    /// <param name="values">Scalar field values, one per vertex.</param>
    /// <param name="minValue">Value mapped to t=0.</param>
    /// <param name="maxValue">Value mapped to t=1.</param>
    /// <returns>
    /// Flat RGBA array of length values.Length * 4. Each group of 4 floats
    /// represents one vertex color in [0, 1].
    /// </returns>
    public float[] MapArray(ReadOnlySpan<double> values, double minValue, double maxValue)
    {
        float[] colors = new float[values.Length * 4];
        double range = maxValue - minValue;

        // Avoid division by zero when all values are identical.
        bool constantField = Math.Abs(range) < double.Epsilon;

        for (int i = 0; i < values.Length; i++)
        {
            double t = constantField ? 0.5 : (values[i] - minValue) / range;
            var (r, g, b, a) = Map(t);
            int offset = i * 4;
            colors[offset] = r;
            colors[offset + 1] = g;
            colors[offset + 2] = b;
            colors[offset + 3] = a;
        }

        return colors;
    }

    /// <summary>
    /// Computes the min and max of a span of values, optionally centering
    /// around zero for diverging color maps.
    /// </summary>
    /// <param name="values">Scalar field values.</param>
    /// <param name="centerAtZero">
    /// If true, the range is symmetric around zero: [-maxAbs, +maxAbs].
    /// Useful for diverging color maps on fields with negative and positive values.
    /// </param>
    /// <returns>The (min, max) range to use for normalization.</returns>
    public static (double Min, double Max) ComputeRange(ReadOnlySpan<double> values, bool centerAtZero = false)
    {
        if (values.IsEmpty)
        {
            return (0.0, 1.0);
        }

        double min = double.MaxValue;
        double max = double.MinValue;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] < min) min = values[i];
            if (values[i] > max) max = values[i];
        }

        if (centerAtZero)
        {
            double absMax = Math.Max(Math.Abs(min), Math.Abs(max));
            return (-absMax, absMax);
        }

        return (min, max);
    }

    // ---- Color scheme implementations ----
    // These are piecewise-linear approximations of the matplotlib color maps.
    // Each uses 5 control points for reasonable visual fidelity.

    private static (float R, float G, float B, float A) MapViridis(float t)
    {
        // Viridis: dark purple -> blue -> teal -> green -> yellow
        float r = Lerp3(t, 0.267f, 0.127f, 0.204f, 0.565f, 0.993f);
        float g = Lerp3(t, 0.005f, 0.214f, 0.553f, 0.812f, 0.906f);
        float b = Lerp3(t, 0.329f, 0.563f, 0.593f, 0.246f, 0.144f);
        return (r, g, b, 1f);
    }

    private static (float R, float G, float B, float A) MapPlasma(float t)
    {
        // Plasma: dark blue/purple -> magenta -> orange -> yellow
        float r = Lerp3(t, 0.050f, 0.467f, 0.830f, 0.961f, 0.940f);
        float g = Lerp3(t, 0.030f, 0.010f, 0.160f, 0.530f, 0.975f);
        float b = Lerp3(t, 0.530f, 0.660f, 0.320f, 0.080f, 0.131f);
        return (r, g, b, 1f);
    }

    private static (float R, float G, float B, float A) MapCoolWarm(float t)
    {
        // Cool-warm diverging: blue (cold) -> white (center) -> red (warm)
        float r = Lerp3(t, 0.230f, 0.456f, 0.865f, 0.929f, 0.706f);
        float g = Lerp3(t, 0.299f, 0.535f, 0.865f, 0.430f, 0.016f);
        float b = Lerp3(t, 0.754f, 0.878f, 0.865f, 0.345f, 0.150f);
        return (r, g, b, 1f);
    }

    private static (float R, float G, float B, float A) MapDiverging(float t)
    {
        // Diverging blue-white-red (symmetric, suitable for residuals)
        float r = Lerp3(t, 0.019f, 0.353f, 0.969f, 0.929f, 0.647f);
        float g = Lerp3(t, 0.188f, 0.553f, 0.969f, 0.373f, 0.059f);
        float b = Lerp3(t, 0.380f, 0.859f, 0.969f, 0.318f, 0.082f);
        return (r, g, b, 1f);
    }

    /// <summary>
    /// Piecewise linear interpolation across 5 control points at t = 0, 0.25, 0.5, 0.75, 1.0.
    /// </summary>
    private static float Lerp3(float t, float v0, float v1, float v2, float v3, float v4)
    {
        if (t <= 0.25f)
        {
            float s = t / 0.25f;
            return v0 + s * (v1 - v0);
        }
        else if (t <= 0.5f)
        {
            float s = (t - 0.25f) / 0.25f;
            return v1 + s * (v2 - v1);
        }
        else if (t <= 0.75f)
        {
            float s = (t - 0.5f) / 0.25f;
            return v2 + s * (v3 - v2);
        }
        else
        {
            float s = (t - 0.75f) / 0.25f;
            return v3 + s * (v4 - v3);
        }
    }
}
