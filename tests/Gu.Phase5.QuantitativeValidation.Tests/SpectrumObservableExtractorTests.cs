using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class SpectrumObservableExtractorTests
{
    [Fact]
    public void ComputeAdjacentGapRatio_UsesMinOverMaxOfNeighboringGaps()
    {
        var ratio = SpectrumObservableExtractor.ComputeAdjacentGapRatio(
            [-2.0762941435550895, 1.839998261954932, 3.3631147376181514],
            gapIndex: 0);

        Assert.Equal(0.3889179657576827, ratio, precision: 15);
    }

    [Fact]
    public void CreateAdjacentGapRatioRecord_CarriesProvenanceAndUncertainty()
    {
        var provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-24T00:00:00Z"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "zenodo-su2lgt", SchemaVersion = "1.0" },
            Backend = "external-python",
        };

        var record = SpectrumObservableExtractor.CreateAdjacentGapRatioRecord(
            [-2.0, 1.0, 2.0],
            gapIndex: 0,
            observableId: "obs",
            environmentId: "env",
            branchId: "branch",
            refinementLevel: "P=4",
            extractionUncertainty: 0.001,
            provenance);

        Assert.Equal(1.0 / 3.0, record.Value, precision: 15);
        Assert.Equal(0.001, record.Uncertainty.TotalUncertainty);
        Assert.Equal("adjacent-gap-ratio:index-0", record.ExtractionMethod);
        Assert.Equal("env", record.EnvironmentId);
        Assert.Same(provenance, record.Provenance);
    }

    [Fact]
    public void ComputeAdjacentGapRatio_UnsortedEigenvalues_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.ComputeAdjacentGapRatio([0.0, 2.0, 1.0], gapIndex: 0));
    }
}
