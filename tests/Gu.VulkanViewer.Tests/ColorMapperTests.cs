namespace Gu.VulkanViewer.Tests;

public class ColorMapperTests
{
    [Theory]
    [InlineData("viridis")]
    [InlineData("plasma")]
    [InlineData("coolwarm")]
    [InlineData("diverging")]
    public void Constructor_AcceptsValidSchemes(string scheme)
    {
        var mapper = new ColorMapper(scheme);
        Assert.Equal(scheme, mapper.SchemeName);
    }

    [Fact]
    public void Constructor_DefaultsToViridis()
    {
        var mapper = new ColorMapper();
        Assert.Equal("viridis", mapper.SchemeName);
    }

    [Fact]
    public void Constructor_ThrowsOnUnknownScheme()
    {
        Assert.Throws<ArgumentException>(() => new ColorMapper("unknown"));
    }

    [Fact]
    public void Constructor_ThrowsOnNullScheme()
    {
        Assert.ThrowsAny<ArgumentException>(() => new ColorMapper(null!));
    }

    [Theory]
    [InlineData("viridis")]
    [InlineData("plasma")]
    [InlineData("coolwarm")]
    [InlineData("diverging")]
    public void Map_ProducesValidRgbaInRange(string scheme)
    {
        var mapper = new ColorMapper(scheme);

        // Test at several points across [0, 1].
        double[] testPoints = { 0.0, 0.1, 0.25, 0.5, 0.75, 0.9, 1.0 };

        foreach (double t in testPoints)
        {
            var (r, g, b, a) = mapper.Map(t);

            Assert.InRange(r, 0f, 1f);
            Assert.InRange(g, 0f, 1f);
            Assert.InRange(b, 0f, 1f);
            Assert.Equal(1f, a); // All schemes produce full alpha.
        }
    }

    [Fact]
    public void Map_ClampsValuesOutsideRange()
    {
        var mapper = new ColorMapper("viridis");

        var (r1, g1, b1, a1) = mapper.Map(-0.5);
        var (r2, g2, b2, a2) = mapper.Map(0.0);
        Assert.Equal(r1, r2);
        Assert.Equal(g1, g2);
        Assert.Equal(b1, b2);

        var (r3, g3, b3, a3) = mapper.Map(1.5);
        var (r4, g4, b4, a4) = mapper.Map(1.0);
        Assert.Equal(r3, r4);
        Assert.Equal(g3, g4);
        Assert.Equal(b3, b4);
    }

    [Fact]
    public void Map_ProducesDifferentColorsAtExtremes()
    {
        var mapper = new ColorMapper("viridis");

        var (r0, g0, b0, _) = mapper.Map(0.0);
        var (r1, g1, b1, _) = mapper.Map(1.0);

        // Viridis should produce different colors at 0 and 1.
        bool different = Math.Abs(r0 - r1) > 0.01f
            || Math.Abs(g0 - g1) > 0.01f
            || Math.Abs(b0 - b1) > 0.01f;
        Assert.True(different, "Color at t=0 and t=1 should be different.");
    }

    [Fact]
    public void MapArray_ProducesCorrectLength()
    {
        var mapper = new ColorMapper("viridis");
        double[] values = { 0.0, 0.5, 1.0, 1.5, 2.0 };

        float[] colors = mapper.MapArray(values, 0.0, 2.0);

        Assert.Equal(5 * 4, colors.Length);
    }

    [Fact]
    public void MapArray_AllValuesInRange()
    {
        var mapper = new ColorMapper("plasma");
        double[] values = { -10, -5, 0, 5, 10, 100 };

        float[] colors = mapper.MapArray(values, -10.0, 100.0);

        for (int i = 0; i < colors.Length; i++)
        {
            Assert.InRange(colors[i], 0f, 1f);
        }
    }

    [Fact]
    public void MapArray_HandlesConstantField()
    {
        var mapper = new ColorMapper("viridis");
        double[] values = { 5.0, 5.0, 5.0 };

        // When all values are the same, range is zero.
        float[] colors = mapper.MapArray(values, 5.0, 5.0);

        // Should not produce NaN or throw.
        for (int i = 0; i < colors.Length; i++)
        {
            Assert.False(float.IsNaN(colors[i]));
            Assert.InRange(colors[i], 0f, 1f);
        }
    }

    [Fact]
    public void ComputeRange_ReturnsMinMax()
    {
        double[] values = { -3.0, 1.0, 5.0, -1.0, 2.0 };

        var (min, max) = ColorMapper.ComputeRange(values);

        Assert.Equal(-3.0, min);
        Assert.Equal(5.0, max);
    }

    [Fact]
    public void ComputeRange_CenterAtZero_ProducesSymmetricRange()
    {
        double[] values = { -2.0, 1.0, 5.0 };

        var (min, max) = ColorMapper.ComputeRange(values, centerAtZero: true);

        Assert.Equal(-5.0, min);
        Assert.Equal(5.0, max);
    }

    [Fact]
    public void ComputeRange_EmptySpan_ReturnsDefault()
    {
        var (min, max) = ColorMapper.ComputeRange(ReadOnlySpan<double>.Empty);

        Assert.Equal(0.0, min);
        Assert.Equal(1.0, max);
    }

    [Fact]
    public void AvailableSchemes_ContainsAllFour()
    {
        Assert.Contains("viridis", ColorMapper.AvailableSchemes);
        Assert.Contains("plasma", ColorMapper.AvailableSchemes);
        Assert.Contains("coolwarm", ColorMapper.AvailableSchemes);
        Assert.Contains("diverging", ColorMapper.AvailableSchemes);
        Assert.Equal(4, ColorMapper.AvailableSchemes.Count);
    }
}
