using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class SpectrumObservableExtractorTests
{
    private static QuantitativeObservableRecord MakeMode(
        string observableId,
        double value,
        double uncertainty,
        string environmentId = "env-physical-candidate",
        string branchId = "branch",
        string? refinementLevel = "L1")
    {
        return new QuantitativeObservableRecord
        {
            ObservableId = observableId,
            Value = value,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = -1,
                RefinementError = -1,
                ExtractionError = uncertainty,
                EnvironmentSensitivity = -1,
                TotalUncertainty = uncertainty,
            },
            BranchId = branchId,
            EnvironmentId = environmentId,
            RefinementLevel = refinementLevel,
            ExtractionMethod = "identified-mode-mass",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = branchId, SchemaVersion = "1.0" },
                Backend = "cpu",
            },
        };
    }

    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "wz-candidate", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static IdentifiedPhysicalModeRecord MakePhysicalMode(
        string modeId,
        string particleId,
        string observableId,
        double value,
        double uncertainty,
        string status = "validated",
        string unitFamily = "mass-energy",
        string unit = "internal-mass-unit",
        string environmentId = "env-physical-candidate",
        string branchId = "branch")
    {
        return new IdentifiedPhysicalModeRecord
        {
            ModeId = modeId,
            ParticleId = particleId,
            ModeKind = "vector-boson-mass-mode",
            ObservableId = observableId,
            Value = value,
            Uncertainty = uncertainty,
            UnitFamily = unitFamily,
            Unit = unit,
            Status = status,
            EnvironmentId = environmentId,
            BranchId = branchId,
            RefinementLevel = "L1",
            ExtractionMethod = "identified-physical-mode:test",
            Assumptions = ["unit test mode"],
            ClosureRequirements = status == "validated" ? [] : ["validate mode identification"],
            Provenance = MakeProvenance(),
        };
    }

    private static ModeIdentificationEvidenceRecord MakeEvidence(
        string modeId,
        string particleId,
        string status = "validated",
        string environmentId = "env-physical-candidate",
        string branchId = "branch")
    {
        return new ModeIdentificationEvidenceRecord
        {
            EvidenceId = $"evidence-{modeId}",
            ModeId = modeId,
            ParticleId = particleId,
            ModeKind = "vector-boson-mass-mode",
            SourceObservableIds = [$"{modeId}-source"],
            EnvironmentId = environmentId,
            BranchId = branchId,
            RefinementLevel = "L1",
            DerivationId = $"derive-{modeId}",
            Status = status,
            Assumptions = ["unit test evidence"],
            ClosureRequirements = status == "validated" ? [] : ["validate evidence"],
            Provenance = MakeProvenance(),
        };
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_WithValidatedEvidence_ProducesRatio()
    {
        var wMode = MakePhysicalMode("mode-w", "w-boson", "physical-mode-w", 80.0, 0.4);
        var zMode = MakePhysicalMode("mode-z", "z-boson", "physical-mode-z", 100.0, 0.3);
        var evidence = new[] { MakeEvidence("mode-w", "w-boson"), MakeEvidence("mode-z", "z-boson") };

        var record = SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
            wMode,
            zMode,
            "physical-w-z-mass-ratio",
            MakeProvenance(),
            evidence,
            requireValidatedEvidence: true);

        Assert.Equal(0.8, record.Value, precision: 15);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_WithMissingEvidence_Throws()
    {
        var wMode = MakePhysicalMode("mode-w", "w-boson", "physical-mode-w", 80.0, 0.4);
        var zMode = MakePhysicalMode("mode-z", "z-boson", "physical-mode-z", 100.0, 0.3);

        var ex = Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                wMode,
                zMode,
                "physical-w-z-mass-ratio",
                MakeProvenance(),
                evidenceRecords: [],
                requireValidatedEvidence: true));

        Assert.Contains("no mode-identification evidence", ex.Message);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_WithProvisionalEvidence_Throws()
    {
        var wMode = MakePhysicalMode("mode-w", "w-boson", "physical-mode-w", 80.0, 0.4);
        var zMode = MakePhysicalMode("mode-z", "z-boson", "physical-mode-z", 100.0, 0.3);
        var evidence = new[] { MakeEvidence("mode-w", "w-boson", status: "provisional"), MakeEvidence("mode-z", "z-boson") };

        var ex = Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                wMode,
                zMode,
                "physical-w-z-mass-ratio",
                MakeProvenance(),
                evidence,
                requireValidatedEvidence: true));

        Assert.Contains("provisional mode-identification evidence", ex.Message);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_FromValidatedPhysicalModes_ProducesRatio()
    {
        var wMode = MakePhysicalMode("mode-w", "w-boson", "physical-mode-w", 80.0, 0.4);
        var zMode = MakePhysicalMode("mode-z", "z-boson", "physical-mode-z", 100.0, 0.3);

        var record = SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
            wMode,
            zMode,
            "physical-w-z-mass-ratio",
            MakeProvenance());

        Assert.Equal(0.8, record.Value, precision: 15);
        Assert.Equal("positive-mode-ratio:physical-mode-w/physical-mode-z", record.ExtractionMethod);
        Assert.Equal("env-physical-candidate", record.EnvironmentId);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_FromProvisionalPhysicalMode_Throws()
    {
        var wMode = MakePhysicalMode("mode-w", "w-boson", "physical-mode-w", 80.0, 0.4, status: "provisional");
        var zMode = MakePhysicalMode("mode-z", "z-boson", "physical-mode-z", 100.0, 0.3);

        var ex = Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                wMode,
                zMode,
                "physical-w-z-mass-ratio",
                MakeProvenance()));
        Assert.Contains("must be validated", ex.Message);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_FromDifferentPhysicalModeUnits_Throws()
    {
        var wMode = MakePhysicalMode("mode-w", "w-boson", "physical-mode-w", 80.0, 0.4, unit: "internal-a");
        var zMode = MakePhysicalMode("mode-z", "z-boson", "physical-mode-z", 100.0, 0.3, unit: "internal-b");

        var ex = Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                wMode,
                zMode,
                "physical-w-z-mass-ratio",
                MakeProvenance()));
        Assert.Contains("share unit", ex.Message);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_FromModeRecords_RequiresSharedSelectors()
    {
        var numerator = MakeMode("w-vector-mode", 80.0, 0.4);
        var denominator = MakeMode("z-vector-mode", 100.0, 0.3);

        var record = SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
            numerator,
            denominator,
            "candidate-w-z-vector-mode-ratio",
            MakeProvenance());

        Assert.Equal(0.8, record.Value, precision: 15);
        Assert.Equal("env-physical-candidate", record.EnvironmentId);
        Assert.Equal("branch", record.BranchId);
        Assert.Equal("L1", record.RefinementLevel);
        Assert.Equal("positive-mode-ratio:w-vector-mode/z-vector-mode", record.ExtractionMethod);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_FromDifferentEnvironments_Throws()
    {
        var numerator = MakeMode("w-vector-mode", 80.0, 0.4, environmentId: "env-a");
        var denominator = MakeMode("z-vector-mode", 100.0, 0.3, environmentId: "env-b");

        var ex = Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                numerator,
                denominator,
                "candidate-w-z-vector-mode-ratio",
                MakeProvenance()));
        Assert.Contains("same environment", ex.Message);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_FromUnestimatedUncertainty_Throws()
    {
        var numerator = MakeMode("w-vector-mode", 80.0, -1);
        var denominator = MakeMode("z-vector-mode", 100.0, 0.3);

        var ex = Assert.Throws<ArgumentException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                numerator,
                denominator,
                "candidate-w-z-vector-mode-ratio",
                MakeProvenance()));
        Assert.Contains("total uncertainty must be estimated", ex.Message);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_PropagatesIndependentUncertainty()
    {
        var provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "wz-candidate", SchemaVersion = "1.0" },
            Backend = "cpu",
        };

        var record = SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
            numeratorModeValue: 80.0,
            numeratorModeUncertainty: 0.4,
            numeratorModeId: "w-vector-mode",
            denominatorModeValue: 100.0,
            denominatorModeUncertainty: 0.3,
            denominatorModeId: "z-vector-mode",
            observableId: "candidate-w-z-vector-mode-ratio",
            environmentId: "env-physical-candidate",
            branchId: "branch",
            refinementLevel: "L1",
            provenance);

        var expectedUncertainty = 0.8 * System.Math.Sqrt(System.Math.Pow(0.4 / 80.0, 2) + System.Math.Pow(0.3 / 100.0, 2));
        Assert.Equal(0.8, record.Value, precision: 15);
        Assert.Equal(expectedUncertainty, record.Uncertainty.TotalUncertainty, precision: 15);
        Assert.Equal("positive-mode-ratio:w-vector-mode/z-vector-mode", record.ExtractionMethod);
        Assert.Equal("candidate-w-z-vector-mode-ratio", record.ObservableId);
        Assert.Same(provenance, record.Provenance);
    }

    [Fact]
    public void CreatePositiveModeRatioRecord_NonPositiveMode_Throws()
    {
        var provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "wz-candidate", SchemaVersion = "1.0" },
            Backend = "cpu",
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
                numeratorModeValue: 0.0,
                numeratorModeUncertainty: 0.1,
                numeratorModeId: "w-vector-mode",
                denominatorModeValue: 100.0,
                denominatorModeUncertainty: 0.1,
                denominatorModeId: "z-vector-mode",
                observableId: "candidate-w-z-vector-mode-ratio",
                environmentId: "env",
                branchId: "branch",
                refinementLevel: null,
                provenance));
    }

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
