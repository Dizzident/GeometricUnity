using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class PhysicalObservableContractTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    [Fact]
    public void PhysicalObservableMapping_JsonRoundTrip_PreservesRequiredFields()
    {
        var table = new PhysicalObservableMappingTable
        {
            TableId = "mappings",
            Mappings =
            [
                new PhysicalObservableMapping
                {
                    MappingId = "map-photon",
                    ParticleId = "photon",
                    PhysicalObservableType = "masslessness",
                    SourceComputedObservableId = "bosonic-eigenvalue-ratio-1",
                    TargetPhysicalObservableId = "physical-photon-masslessness",
                    RequiredEnvironmentTier = "structured",
                    UnitFamily = "dimensionless",
                    Status = "blocked",
                    Assumptions = ["low mode is a gauge candidate"],
                    ClosureRequirements = ["prove protected gauge-mode identification"],
                },
            ],
        };

        var json = GuJsonDefaults.Serialize(table);
        var roundTrip = GuJsonDefaults.Deserialize<PhysicalObservableMappingTable>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("map-photon", roundTrip!.Mappings[0].MappingId);
        Assert.Equal("blocked", roundTrip.Mappings[0].Status);
        Assert.Single(roundTrip.Mappings[0].ClosureRequirements);
    }

    [Fact]
    public void ObservableClassification_JsonRoundTrip_PreservesClassification()
    {
        var table = new ObservableClassificationTable
        {
            TableId = "classifications",
            Classifications =
            [
                new ObservableClassification
                {
                    ObservableId = "bosonic-eigenvalue-ratio-1",
                    Classification = "internal-benchmark",
                    Rationale = "benchmark ratio, not a particle property",
                    PhysicalClaimAllowed = false,
                },
            ],
        };

        var json = GuJsonDefaults.Serialize(table);
        var roundTrip = GuJsonDefaults.Deserialize<ObservableClassificationTable>(json);

        Assert.NotNull(roundTrip);
        Assert.False(roundTrip!.Classifications[0].PhysicalClaimAllowed);
        Assert.Equal("internal-benchmark", roundTrip.Classifications[0].Classification);
    }

    [Fact]
    public void PhysicalCalibration_JsonRoundTrip_PreservesScaleAndStatus()
    {
        var table = new PhysicalCalibrationTable
        {
            TableId = "calibrations",
            Calibrations =
            [
                new PhysicalCalibrationRecord
                {
                    CalibrationId = "cal-z-mass",
                    MappingId = "map-z-mass",
                    SourceComputedObservableId = "z-mass-internal",
                    SourceUnitFamily = "dimensionless",
                    TargetUnitFamily = "mass-energy",
                    TargetUnit = "GeV",
                    ScaleFactor = 91.1876,
                    ScaleUncertainty = 0.0021,
                    Status = "validated",
                    Method = "test-fixture",
                    Source = "unit-test",
                    Assumptions = ["scale is externally supplied for the test"],
                    ClosureRequirements = [],
                },
            ],
        };

        var json = GuJsonDefaults.Serialize(table);
        var roundTrip = GuJsonDefaults.Deserialize<PhysicalCalibrationTable>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("cal-z-mass", roundTrip!.Calibrations[0].CalibrationId);
        Assert.Equal(91.1876, roundTrip.Calibrations[0].ScaleFactor);
        Assert.Equal("validated", roundTrip.Calibrations[0].Status);
    }

    [Fact]
    public void ToMarkdown_BlockedGate_AllowsBenchmarkLanguageButBlocksBosonPrediction()
    {
        var classifications = new ObservableClassificationTable
        {
            TableId = "classifications",
            Classifications =
            [
                new ObservableClassification
                {
                    ObservableId = "bosonic-eigenvalue-ratio-1",
                    Classification = "internal-benchmark",
                    Rationale = "benchmark comparison quantity",
                    PhysicalClaimAllowed = false,
                },
            ],
        };

        var report = Phase5ReportGenerator.Generate(
            "study-1",
            [],
            MakeProvenance(),
            observableClassifications: classifications);

        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("Observable Classifications", md);
        Assert.Contains("bosonic-eigenvalue-ratio-1: internal-benchmark", md);
        Assert.Contains("Physical boson prediction: blocked", md);
    }

    [Fact]
    public void PhysicalClaimGate_AllowsPredictionOnlyWhenAllInputsPass()
    {
        var mappings = new[]
        {
            new PhysicalObservableMapping
            {
                MappingId = "map-z-mass",
                ParticleId = "z-boson",
                PhysicalObservableType = "mass",
                SourceComputedObservableId = "z-mass-in-gev",
                TargetPhysicalObservableId = "physical-z-boson-mass-gev",
                UnitFamily = "mass-energy",
                Status = "validated",
                Assumptions = ["validated test mapping"],
                ClosureRequirements = [],
            },
        };
        var classifications = new ObservableClassificationTable
        {
            TableId = "classifications",
            Classifications =
            [
                new ObservableClassification
                {
                    ObservableId = "z-mass-in-gev",
                    Classification = "physical-observable",
                    Rationale = "validated mass observable",
                    PhysicalClaimAllowed = true,
                },
            ],
        };
        var falsifiers = new FalsifierSummary
        {
            StudyId = "study-1",
            Falsifiers = [],
            ActiveFatalCount = 0,
            ActiveHighCount = 0,
            TotalActiveCount = 0,
            Provenance = MakeProvenance(),
        };

        var gate = PhysicalClaimGate.Evaluate(
            mappings,
            classifications,
            falsifiers,
            calibrationAvailable: true,
            physicalTargetEvidenceAvailable: true);

        Assert.True(gate.PhysicalBosonPredictionAllowed);
        Assert.Contains(gate.SummaryLines, line => line.Contains("Physical boson prediction: passed"));
    }

    [Fact]
    public void PhysicalPredictionProjector_ValidatedInputs_EmitsPrediction()
    {
        var observables = new[]
        {
            new QuantitativeObservableRecord
            {
                ObservableId = "z-mass-internal",
                Value = 1.0,
                Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.01 },
                BranchId = "branch-a",
                EnvironmentId = "env-physical",
                RefinementLevel = "L1",
                ExtractionMethod = "unit-test",
                Provenance = MakeProvenance(),
            },
        };
        var mappings = new[]
        {
            new PhysicalObservableMapping
            {
                MappingId = "map-z-mass",
                ParticleId = "z-boson",
                PhysicalObservableType = "mass",
                SourceComputedObservableId = "z-mass-internal",
                TargetPhysicalObservableId = "physical-z-boson-mass-gev",
                UnitFamily = "mass-energy",
                Status = "validated",
                Assumptions = ["test mapping"],
                ClosureRequirements = [],
            },
        };
        var classifications = new ObservableClassificationTable
        {
            TableId = "classifications",
            Classifications =
            [
                new ObservableClassification
                {
                    ObservableId = "z-mass-internal",
                    Classification = "physical-observable",
                    Rationale = "validated test observable",
                    PhysicalClaimAllowed = true,
                },
            ],
        };
        var calibrations = new PhysicalCalibrationTable
        {
            TableId = "calibrations",
            Calibrations =
            [
                new PhysicalCalibrationRecord
                {
                    CalibrationId = "cal-z-mass",
                    MappingId = "map-z-mass",
                    SourceComputedObservableId = "z-mass-internal",
                    SourceUnitFamily = "dimensionless",
                    TargetUnitFamily = "mass-energy",
                    TargetUnit = "GeV",
                    ScaleFactor = 91.188,
                    ScaleUncertainty = 0.002,
                    Status = "validated",
                    Method = "unit-test",
                    Source = "unit-test",
                    Assumptions = ["test calibration"],
                    ClosureRequirements = [],
                },
            ],
        };

        var predictions = PhysicalPredictionProjector.Project(observables, mappings, classifications, calibrations);

        var prediction = Assert.Single(predictions);
        Assert.Equal("predicted", prediction.Status);
        Assert.Equal("physical-z-boson-mass-gev", prediction.TargetPhysicalObservableId);
        Assert.Equal(91.188, prediction.Value);
        Assert.Equal("GeV", prediction.Unit);
        Assert.Empty(prediction.BlockReasons);
    }

    [Fact]
    public void PhysicalPredictionProjector_BenchmarkClassification_BlocksPrediction()
    {
        var observables = new[]
        {
            new QuantitativeObservableRecord
            {
                ObservableId = "bosonic-eigenvalue-ratio-1",
                Value = 0.1,
                Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.01 },
                BranchId = "branch-a",
                EnvironmentId = "env-structured",
                ExtractionMethod = "unit-test",
                Provenance = MakeProvenance(),
            },
        };
        var mappings = new[]
        {
            new PhysicalObservableMapping
            {
                MappingId = "map-photon",
                ParticleId = "photon",
                PhysicalObservableType = "masslessness",
                SourceComputedObservableId = "bosonic-eigenvalue-ratio-1",
                TargetPhysicalObservableId = "physical-photon-masslessness",
                UnitFamily = "dimensionless",
                Status = "validated",
                Assumptions = ["test mapping"],
                ClosureRequirements = [],
            },
        };
        var classifications = new ObservableClassificationTable
        {
            TableId = "classifications",
            Classifications =
            [
                new ObservableClassification
                {
                    ObservableId = "bosonic-eigenvalue-ratio-1",
                    Classification = "internal-benchmark",
                    Rationale = "benchmark only",
                    PhysicalClaimAllowed = false,
                },
            ],
        };
        var calibrations = new PhysicalCalibrationTable
        {
            TableId = "calibrations",
            Calibrations =
            [
                new PhysicalCalibrationRecord
                {
                    CalibrationId = "cal-photon",
                    MappingId = "map-photon",
                    SourceComputedObservableId = "bosonic-eigenvalue-ratio-1",
                    SourceUnitFamily = "dimensionless",
                    TargetUnitFamily = "dimensionless",
                    TargetUnit = "normalized-zero-mode-indicator",
                    ScaleFactor = 1.0,
                    ScaleUncertainty = 0.0,
                    Status = "validated",
                    Method = "unit-test",
                    Source = "unit-test",
                    Assumptions = ["test calibration"],
                    ClosureRequirements = [],
                },
            ],
        };

        var predictions = PhysicalPredictionProjector.Project(observables, mappings, classifications, calibrations);

        var prediction = Assert.Single(predictions);
        Assert.Equal("blocked", prediction.Status);
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("not classified for physical claims"));
    }

    [Fact]
    public void Phase18PhysicalTargets_AreCitedAndInactive()
    {
        var repoRoot = FindRepoRoot();
        var targetPath = Path.Combine(repoRoot, "studies", "phase18_experimental_targets_001", "physical_targets.json");
        var table = GuJsonDefaults.Deserialize<ExternalTargetTable>(File.ReadAllText(targetPath));

        Assert.NotNull(table);
        Assert.All(table!.Targets, target =>
        {
            Assert.Equal("physical-prediction", target.EvidenceTier);
            Assert.Equal("physical-observable", target.BenchmarkClass);
            Assert.False(string.IsNullOrWhiteSpace(target.ParticleId));
            Assert.False(string.IsNullOrWhiteSpace(target.PhysicalObservableType));
            Assert.False(string.IsNullOrWhiteSpace(target.UnitFamily));
            Assert.False(string.IsNullOrWhiteSpace(target.Unit));
            Assert.False(string.IsNullOrWhiteSpace(target.Citation));
            Assert.False(string.IsNullOrWhiteSpace(target.SourceUrl));
            Assert.False(string.IsNullOrWhiteSpace(target.RetrievedAt));
        });
    }

    [Fact]
    public void Phase19WzCandidate_IsInactiveAndBlockedByContract()
    {
        var repoRoot = FindRepoRoot();
        var studyDir = Path.Combine(repoRoot, "studies", "phase19_dimensionless_wz_candidate_001");
        var observables = GuJsonDefaults.Deserialize<List<QuantitativeObservableRecord>>(
            File.ReadAllText(Path.Combine(studyDir, "candidate_observables.json")));
        var modes = GuJsonDefaults.Deserialize<List<IdentifiedPhysicalModeRecord>>(
            File.ReadAllText(Path.Combine(studyDir, "candidate_modes.json")));
        var classifications = GuJsonDefaults.Deserialize<ObservableClassificationTable>(
            File.ReadAllText(Path.Combine(studyDir, "observable_classifications.json")));
        var mappings = GuJsonDefaults.Deserialize<PhysicalObservableMappingTable>(
            File.ReadAllText(Path.Combine(studyDir, "physical_observable_mappings.json")));
        var calibrations = GuJsonDefaults.Deserialize<PhysicalCalibrationTable>(
            File.ReadAllText(Path.Combine(studyDir, "physical_calibrations.json")));
        var targets = GuJsonDefaults.Deserialize<ExternalTargetTable>(
            File.ReadAllText(Path.Combine(studyDir, "physical_targets.json")));

        Assert.NotNull(observables);
        Assert.NotNull(modes);
        Assert.NotNull(classifications);
        Assert.NotNull(mappings);
        Assert.NotNull(calibrations);
        Assert.NotNull(targets);
        Assert.All(modes!, mode =>
        {
            Assert.Equal("provisional", mode.Status);
            Assert.True(mode.Uncertainty < 0);
            Assert.NotEmpty(mode.ClosureRequirements);
        });
        Assert.Equal("physical-candidate", classifications!.Classifications[0].Classification);
        Assert.False(classifications.Classifications[0].PhysicalClaimAllowed);
        Assert.Equal("provisional", mappings!.Mappings[0].Status);
        Assert.Equal("provisional", calibrations!.Calibrations[0].Status);
        Assert.Equal("physical-w-z-mass-ratio", targets!.Targets[0].ObservableId);

        var predictions = PhysicalPredictionProjector.Project(
            observables!,
            mappings.Mappings,
            classifications,
            calibrations);

        var prediction = Assert.Single(predictions);
        Assert.Equal("blocked", prediction.Status);
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("provisional"));
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("not classified for physical claims"));
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("no validated calibration"));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "studies")) &&
                Directory.Exists(Path.Combine(current.FullName, "src")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
