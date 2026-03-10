using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties.Tests;

public class MassLikeScaleExtractorTests
{
    [Fact]
    public void Extract_PositiveEigenvalue_ReturnsSqrt()
    {
        var mode = TestHelpers.MakeMode("m-1", 4.0, new double[] { 1, 0, 0 });
        var record = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal("m-1", record.ModeId);
        Assert.Equal(4.0, record.Eigenvalue);
        Assert.Equal(2.0, record.MassLikeScale, 12);
        Assert.Equal("eigenvalue", record.ExtractionMethod);
        Assert.Equal("GaussNewton", record.OperatorType);
    }

    [Fact]
    public void Extract_NegativeEigenvalue_ReturnsNegativeSqrt()
    {
        var mode = TestHelpers.MakeMode("m-neg", -9.0, new double[] { 1, 0, 0 });
        var record = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal(-3.0, record.MassLikeScale, 12);
    }

    [Fact]
    public void Extract_ZeroEigenvalue_ReturnsZero()
    {
        var mode = TestHelpers.MakeMode("m-zero", 0.0, new double[] { 1, 0, 0 });
        var record = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal(0.0, record.MassLikeScale);
    }

    [Fact]
    public void Extract_SmallEigenvalue_ReturnsSmallScale()
    {
        var mode = TestHelpers.MakeMode("m-small", 1e-10, new double[] { 1, 0, 0 });
        var record = MassLikeScaleExtractor.Extract(mode);

        Assert.Equal(1e-5, record.MassLikeScale, 10);
    }

    [Fact]
    public void ExtractAll_MultipleModesReturnsAll()
    {
        var modes = new[]
        {
            TestHelpers.MakeMode("m-1", 1.0, new double[] { 1, 0, 0 }),
            TestHelpers.MakeMode("m-2", 4.0, new double[] { 0, 1, 0 }),
            TestHelpers.MakeMode("m-3", 9.0, new double[] { 0, 0, 1 }),
        };

        var records = MassLikeScaleExtractor.ExtractAll(modes);
        Assert.Equal(3, records.Count);
        Assert.Equal(1.0, records[0].MassLikeScale, 12);
        Assert.Equal(2.0, records[1].MassLikeScale, 12);
        Assert.Equal(3.0, records[2].MassLikeScale, 12);
    }

    [Fact]
    public void Extract_IncludesBackgroundId()
    {
        var mode = TestHelpers.MakeMode("m-1", 1.0, new double[] { 1 }, backgroundId: "bg-42");
        var record = MassLikeScaleExtractor.Extract(mode);
        Assert.Equal("bg-42", record.BackgroundId);
    }

    [Fact]
    public void Extract_NullMode_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            MassLikeScaleExtractor.Extract(null!));
    }
}
