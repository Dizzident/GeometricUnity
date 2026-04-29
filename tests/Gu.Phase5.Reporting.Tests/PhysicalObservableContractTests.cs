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
    public void PhysicalClaimGate_TargetScopedAuditAllowsScopedComparisonButKeepsUnrestrictedBlocked()
    {
        var mappings = MakeValidatedWzMappings();
        var classifications = MakePhysicalWzClassifications();
        var falsifiers = MakeGlobalSidecarFalsifiers();
        var audit = MakeTargetClearFalsifierRelevanceAudit();

        var gate = PhysicalClaimGate.Evaluate(
            mappings,
            classifications,
            falsifiers,
            calibrationAvailable: true,
            physicalTargetEvidenceAvailable: true,
            targetScopedFalsifierAudit: audit);

        Assert.False(gate.PhysicalBosonPredictionAllowed);
        Assert.True(gate.TargetScopedPhysicalComparisonAllowed);
        Assert.Equal("physical-w-z-mass-ratio", gate.TargetScopedObservableId);
        Assert.Equal(0, gate.TargetRelevantSevereFalsifierCount);
        Assert.Equal(3, gate.GlobalSidecarSevereFalsifierCount);
        Assert.Contains(gate.SummaryLines, line => line.Contains("Target-scoped physical comparison: allowed", StringComparison.Ordinal));

        var status = PhysicalPredictionTerminalStatus.Evaluate(gate, predictions: [], scoreCard: null);
        Assert.Equal("target-scoped", status.Status);
    }

    [Fact]
    public void Phase5ReportGenerator_RendersTargetScopedFalsifierRelevanceAudit()
    {
        var report = Phase5ReportGenerator.Generate(
            "study-target-scoped",
            [],
            MakeProvenance(),
            falsifiers: MakeGlobalSidecarFalsifiers(),
            observableClassifications: MakePhysicalWzClassifications(),
            physicalObservableMappings: MakeValidatedWzMappings(),
            physicalCalibrationAvailable: true,
            physicalTargetEvidenceAvailable: true,
            physicalClaimFalsifierRelevanceAudit: MakeTargetClearFalsifierRelevanceAudit());

        Assert.NotNull(report.PhysicalClaimFalsifierRelevanceAudit);
        Assert.True(report.PhysicalClaimGate!.TargetScopedPhysicalComparisonAllowed);

        var md = Phase5ReportGenerator.ToMarkdown(report);
        Assert.Contains("Physical Claim Falsifier Relevance", md);
        Assert.Contains("Target-relevant severe falsifiers: 0", md);
        Assert.Contains("Global sidecar severe falsifiers: 3", md);
    }

    [Fact]
    public void PhysicalPredictionTerminalStatus_GateBlocked_ReturnsBlocked()
    {
        var gate = PhysicalClaimGate.Evaluate(
            mappings: null,
            classifications: null,
            falsifiers: null,
            calibrationAvailable: false,
            physicalTargetEvidenceAvailable: false);

        var status = PhysicalPredictionTerminalStatus.Evaluate(gate, predictions: [], scoreCard: null);

        Assert.Equal("blocked", status.Status);
        Assert.Contains(status.SummaryLines, line => line.Contains("claim gate is blocked", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PhysicalPredictionTerminalStatus_PassingPhysicalTarget_ReturnsPredicted()
    {
        var gate = MakePassingPhysicalGate();
        var prediction = MakePredictedPhysicalRecord("physical-w-z-mass-ratio");
        var scoreCard = MakePhysicalScoreCard(passed: true);

        var status = PhysicalPredictionTerminalStatus.Evaluate(gate, [prediction], scoreCard);

        Assert.Equal("predicted", status.Status);
        Assert.Contains(status.SummaryLines, line => line.Contains("passed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PhysicalPredictionTerminalStatus_FailingPhysicalTarget_ReturnsFailed()
    {
        var gate = MakePassingPhysicalGate();
        var prediction = MakePredictedPhysicalRecord("physical-w-z-mass-ratio");
        var scoreCard = MakePhysicalScoreCard(passed: false);

        var status = PhysicalPredictionTerminalStatus.Evaluate(gate, [prediction], scoreCard);

        Assert.Equal("failed", status.Status);
        Assert.Contains(status.SummaryLines, line => line.Contains("failed", StringComparison.OrdinalIgnoreCase));
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

        var predictions = PhysicalPredictionProjector.Project(
            observables,
            mappings,
            classifications,
            calibrations,
            electroweakBridges: MakeValidElectroweakBridgeTable());

        var prediction = Assert.Single(predictions);
        Assert.Equal("predicted", prediction.Status);
        Assert.Equal("physical-z-boson-mass-gev", prediction.TargetPhysicalObservableId);
        Assert.Equal(91.188, prediction.Value);
        Assert.Equal("GeV", prediction.Unit);
        Assert.Empty(prediction.BlockReasons);
    }

    [Fact]
    public void PhysicalPredictionProjector_AbsoluteWzMassWithoutBridge_BlocksPrediction()
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

        var predictions = PhysicalPredictionProjector.Project(
            observables,
            MakeZMassMappings(),
            MakeZMassClassifications(),
            MakeZMassCalibrations());

        var prediction = Assert.Single(predictions);
        Assert.Equal("blocked", prediction.Status);
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("requires a validated electroweak bridge", StringComparison.Ordinal));
    }

    [Fact]
    public void PhysicalPredictionProjector_AbsoluteWzMassWithRejectedBridge_BlocksPrediction()
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
        var bridgeTable = new ElectroweakBridgeTable
        {
            TableId = "bridges",
            Bridges =
            [
                MakeElectroweakBridge(inputKind: "coupling-profile-mean-magnitude"),
            ],
        };

        var predictions = PhysicalPredictionProjector.Project(
            observables,
            MakeZMassMappings(),
            MakeZMassClassifications(),
            MakeZMassCalibrations(),
            electroweakBridges: bridgeTable);

        var prediction = Assert.Single(predictions);
        Assert.Equal("blocked", prediction.Status);
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("rejected for absolute W/Z projection", StringComparison.Ordinal));
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
    public void PhysicalPredictionProjector_UnestimatedSourceUncertainty_BlocksPrediction()
    {
        var observables = new[]
        {
            new QuantitativeObservableRecord
            {
                ObservableId = "z-mass-internal",
                Value = 1.0,
                Uncertainty = new QuantitativeUncertainty { TotalUncertainty = -1 },
                BranchId = "branch-a",
                EnvironmentId = "env-physical",
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
        Assert.Equal("blocked", prediction.Status);
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("total uncertainty is unestimated"));
    }

    [Fact]
    public void PhysicalPredictionProjector_WzMappingWithoutValidatedModeEvidence_BlocksPrediction()
    {
        var observables = new[]
        {
            new QuantitativeObservableRecord
            {
                ObservableId = "candidate-w-z-vector-mode-ratio",
                Value = 0.8,
                Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.01 },
                BranchId = "branch-a",
                EnvironmentId = "env-physical",
                RefinementLevel = "L1",
                ExtractionMethod = "positive-mode-ratio:w-mode/z-mode",
                Provenance = MakeProvenance(),
            },
        };
        var mappings = new[]
        {
            new PhysicalObservableMapping
            {
                MappingId = "map-wz-ratio",
                ParticleId = "electroweak-sector",
                PhysicalObservableType = "mass-ratio",
                SourceComputedObservableId = "candidate-w-z-vector-mode-ratio",
                TargetPhysicalObservableId = "physical-w-z-mass-ratio",
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
                    ObservableId = "candidate-w-z-vector-mode-ratio",
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
                    CalibrationId = "cal-wz-ratio",
                    MappingId = "map-wz-ratio",
                    SourceComputedObservableId = "candidate-w-z-vector-mode-ratio",
                    SourceUnitFamily = "dimensionless",
                    TargetUnitFamily = "dimensionless",
                    TargetUnit = "dimensionless",
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

        var predictions = PhysicalPredictionProjector.Project(
            observables,
            mappings,
            classifications,
            calibrations,
            physicalModeRecords: [],
            modeIdentificationEvidence: []);

        var prediction = Assert.Single(predictions);
        Assert.Equal("blocked", prediction.Status);
        Assert.Contains(prediction.BlockReasons, reason => reason.Contains("requires identified physical mode records"));
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
        var evidence = GuJsonDefaults.Deserialize<ModeIdentificationEvidenceTable>(
            File.ReadAllText(Path.Combine(studyDir, "mode_identification_evidence.json")));
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
        Assert.NotNull(evidence);
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
        Assert.All(evidence!.Evidence, item =>
        {
            Assert.Equal("provisional", item.Status);
            Assert.NotEmpty(item.SourceObservableIds);
            Assert.NotEmpty(item.ClosureRequirements);
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

    private static PhysicalClaimGate MakePassingPhysicalGate()
    {
        var falsifiers = new FalsifierSummary
        {
            StudyId = "study-1",
            Falsifiers = [],
            ActiveFatalCount = 0,
            ActiveHighCount = 0,
            TotalActiveCount = 0,
            Provenance = MakeProvenance(),
        };

        return PhysicalClaimGate.Evaluate(
            MakeValidatedWzMappings(),
            MakePhysicalWzClassifications(),
            falsifiers,
            calibrationAvailable: true,
            physicalTargetEvidenceAvailable: true);
    }

    private static IReadOnlyList<PhysicalObservableMapping> MakeValidatedWzMappings() =>
    [
        new PhysicalObservableMapping
        {
            MappingId = "map-wz-ratio",
            ParticleId = "electroweak-sector",
            PhysicalObservableType = "mass-ratio",
            SourceComputedObservableId = "candidate-w-z-vector-mode-ratio",
            TargetPhysicalObservableId = "physical-w-z-mass-ratio",
            UnitFamily = "dimensionless",
            Status = "validated",
            Assumptions = ["test mapping"],
            ClosureRequirements = [],
        },
    ];

    private static IReadOnlyList<PhysicalObservableMapping> MakeZMassMappings() =>
    [
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
    ];

    private static ObservableClassificationTable MakePhysicalWzClassifications() => new()
    {
        TableId = "classifications",
        Classifications =
        [
            new ObservableClassification
            {
                ObservableId = "candidate-w-z-vector-mode-ratio",
                Classification = "physical-observable",
                Rationale = "validated test ratio",
                PhysicalClaimAllowed = true,
            },
        ],
    };

    private static ObservableClassificationTable MakeZMassClassifications() => new()
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

    private static PhysicalCalibrationTable MakeZMassCalibrations() => new()
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

    private static ElectroweakBridgeTable MakeValidElectroweakBridgeTable() => new()
    {
        TableId = "bridges",
        Bridges = [MakeValidElectroweakBridge()],
    };

    private static ElectroweakBridgeRecord MakeValidElectroweakBridge()
        => MakeElectroweakBridge();

    private static ElectroweakBridgeRecord MakeElectroweakBridge(
        string inputKind = "normalized-internal-weak-coupling") => new()
    {
        BridgeObservableId = "test-electroweak-bridge",
        SourceModeIds = ["phase22-phase12-candidate-0", "phase22-phase12-candidate-2"],
        DimensionlessBridgeValue = 0.65,
        DimensionlessBridgeUncertainty = 0.01,
        InputKind = inputKind,
        WeakCouplingNormalizationConvention = "test-normalized-internal-weak-coupling",
        MassGenerationRelation = "test-mass-generation-relation",
        ExcludedTargetObservableIds = ["physical-w-boson-mass-gev", "physical-z-boson-mass-gev"],
        Status = "validated",
        Assumptions = ["test bridge"],
        ClosureRequirements = [],
    };

    private static FalsifierSummary MakeGlobalSidecarFalsifiers() => new()
    {
        StudyId = "study-1",
        ActiveFatalCount = 1,
        ActiveHighCount = 2,
        TotalActiveCount = 3,
        Falsifiers =
        [
            new()
            {
                FalsifierId = "falsifier-0001",
                FalsifierType = "branch-fragility",
                Severity = "high",
                TargetId = "gauge-violation",
                BranchId = "branch-family",
                TriggerValue = 1.8,
                Threshold = 0.5,
                Description = "branch fragility",
                Evidence = "branch",
                Active = true,
                Provenance = MakeProvenance(),
            },
            new()
            {
                FalsifierId = "falsifier-0002",
                FalsifierType = "branch-fragility",
                Severity = "high",
                TargetId = "solver-iterations",
                BranchId = "branch-family",
                TriggerValue = 2.0,
                Threshold = 0.5,
                Description = "branch fragility",
                Evidence = "branch",
                Active = true,
                Provenance = MakeProvenance(),
            },
            new()
            {
                FalsifierId = "falsifier-0003",
                FalsifierType = "representation-content",
                Severity = "fatal",
                TargetId = "fermion-registry-phase4-toy-v1-0000",
                BranchId = "unknown",
                TriggerValue = 1.0,
                Threshold = 0.0,
                Description = "representation content",
                Evidence = "representation",
                Active = true,
                Provenance = MakeProvenance(),
            },
        ],
        Provenance = MakeProvenance(),
    };

    private static WzPhysicalClaimFalsifierRelevanceAuditResult MakeTargetClearFalsifierRelevanceAudit() => new()
    {
        ResultId = "phase47-wz-physical-claim-falsifier-relevance-audit-v1",
        SchemaVersion = "1.0.0",
        TerminalStatus = "wz-physical-claim-target-clear-global-sidecars-blocked",
        AlgorithmId = WzPhysicalClaimFalsifierRelevanceAudit.AlgorithmId,
        TargetObservableId = "physical-w-z-mass-ratio",
        TargetComparisonPassed = true,
        SelectorVariationPassed = true,
        SelectedModeIds = ["phase22-phase12-candidate-0", "phase22-phase12-candidate-2"],
        SelectedSourceCandidateIds = ["phase12-candidate-0", "phase12-candidate-2"],
        ActiveSevereFalsifierCount = 3,
        TargetRelevantSevereFalsifierCount = 0,
        GlobalSidecarSevereFalsifierCount = 3,
        FalsifierAudits =
        [
            new()
            {
                FalsifierId = "falsifier-0001",
                FalsifierType = "branch-fragility",
                Severity = "high",
                TargetId = "gauge-violation",
                BranchId = "branch-family",
                Relevance = "global-sidecar",
                Scope = "branch-diagnostic",
                Reason = "test",
            },
            new()
            {
                FalsifierId = "falsifier-0002",
                FalsifierType = "branch-fragility",
                Severity = "high",
                TargetId = "solver-iterations",
                BranchId = "branch-family",
                Relevance = "global-sidecar",
                Scope = "branch-diagnostic",
                Reason = "test",
            },
            new()
            {
                FalsifierId = "falsifier-0003",
                FalsifierType = "representation-content",
                Severity = "fatal",
                TargetId = "fermion-registry-phase4-toy-v1-0000",
                BranchId = "unknown",
                Relevance = "global-sidecar",
                Scope = "fermion-registry",
                Reason = "test",
            },
        ],
        Diagnosis = ["test"],
        ClosureRequirements = ["resolve global sidecar falsifiers or adopt target-scoped policy"],
        Provenance = MakeProvenance(),
    };

    private static PhysicalPredictionRecord MakePredictedPhysicalRecord(string targetObservableId) => new()
    {
        PredictionId = $"prediction-{targetObservableId}",
        MappingId = "map-wz-ratio",
        CalibrationId = "cal-wz-ratio",
        SourceComputedObservableId = "candidate-w-z-vector-mode-ratio",
        TargetPhysicalObservableId = targetObservableId,
        ParticleId = "electroweak-sector",
        PhysicalObservableType = "mass-ratio",
        Value = 0.88136,
        Uncertainty = 0.0001,
        Unit = "dimensionless",
        UnitFamily = "dimensionless",
        Status = "predicted",
        BlockReasons = [],
    };

    private static ConsistencyScoreCard MakePhysicalScoreCard(bool passed) => new()
    {
        StudyId = "study-1",
        SchemaVersion = "1.0.0",
        Matches =
        [
            new TargetMatchRecord
            {
                ObservableId = "physical-w-z-mass-ratio",
                TargetLabel = "pdg-w-z-mass-ratio",
                TargetValue = 0.88136,
                TargetUncertainty = 0.00015,
                ComputedValue = passed ? 0.88136 : 0.9,
                ComputedUncertainty = 0.0001,
                Pull = passed ? 0.0 : 99.0,
                Passed = passed,
                TargetEvidenceTier = "physical-prediction",
                TargetBenchmarkClass = "physical-observable",
            },
        ],
        TotalTargets = 1,
        MatchedTargetCount = 1,
        MissingTargetCount = 0,
        TotalPassed = passed ? 1 : 0,
        TotalFailed = passed ? 0 : 1,
        OverallScore = passed ? 1.0 : 0.0,
        CalibrationPolicyId = "test-policy",
        Provenance = MakeProvenance(),
    };
}
