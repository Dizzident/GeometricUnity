using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Validation.Tests;

public class ReplayValidatorTests : IDisposable
{
    private readonly string _originalDir;
    private readonly string _replayDir;

    public ReplayValidatorTests()
    {
        _originalDir = TestHelpers.CreateTempRunFolder();
        _replayDir = TestHelpers.CreateTempRunFolder();
    }

    public void Dispose()
    {
        if (Directory.Exists(_originalDir))
            Directory.Delete(_originalDir, recursive: true);
        if (Directory.Exists(_replayDir))
            Directory.Delete(_replayDir, recursive: true);
    }

    private void PopulateRunFolder(
        string path,
        string backendId = "cpu-reference",
        double objectiveValue = 1.5e-6,
        double[]? observedValues = null)
    {
        var writer = new RunFolderWriter(path);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract(backendId: backendId));
        writer.WriteValidationBundle(TestHelpers.CreateTestValidationBundle());

        var residual = new ResidualBundle
        {
            Components = Array.Empty<ResidualComponent>(),
            ObjectiveValue = objectiveValue,
            TotalNorm = System.Math.Sqrt(objectiveValue),
        };
        writer.WriteResidualBundle(residual);

        if (observedValues is not null)
        {
            writer.WriteObservedState(TestHelpers.CreateTestObservedState(observedValues));
        }
    }

    [Fact]
    public void R0_IdenticalFolders_Passes()
    {
        PopulateRunFolder(_originalDir);
        PopulateRunFolder(_replayDir);

        var validator = new ReplayValidator();
        var origReader = new RunFolderReader(_originalDir);
        var replayReader = new RunFolderReader(_replayDir);

        var report = validator.Validate(origReader, replayReader, ReplayTiers.R0);

        Assert.Equal(ReplayOutcome.Pass, report.Outcome);
        Assert.Equal(ReplayTiers.R0, report.ReplayTier);
    }

    [Fact]
    public void R0_SchemaVersionMismatch_Fails()
    {
        PopulateRunFolder(_originalDir);

        // Write a different schema version in replay
        var replayWriter = new RunFolderWriter(_replayDir);
        replayWriter.CreateDirectories();
        var differentManifest = new BranchManifest
        {
            BranchId = "test-branch",
            SchemaVersion = "2.0.0", // Different!
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-4d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic",
            ActiveShiabBranch = "first-order-curvature",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };
        replayWriter.WriteBranchManifest(differentManifest);
        replayWriter.WriteReplayContract(TestHelpers.CreateTestReplayContract());

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R0);

        Assert.Equal(ReplayOutcome.Fail, report.Outcome);
        Assert.Contains(report.Checks, c => c.CheckName == "r0-schema-version-match" && !c.Passed);
    }

    [Fact]
    public void R1_IdenticalObservedStates_Passes()
    {
        var values = new[] { 1.0, 2.0, 3.0 };
        PopulateRunFolder(_originalDir, observedValues: values);
        PopulateRunFolder(_replayDir, observedValues: values);

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R1);

        Assert.Equal(ReplayOutcome.Pass, report.Outcome);
    }

    [Fact]
    public void R1_DifferentObservedValues_Fails()
    {
        PopulateRunFolder(_originalDir, observedValues: new[] { 1.0, 2.0, 3.0 });
        PopulateRunFolder(_replayDir, observedValues: new[] { 1.0, 2.0, 999.0 });

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R1);

        Assert.Equal(ReplayOutcome.Fail, report.Outcome);
        Assert.Contains(report.Checks, c =>
            c.CheckName == "r1-observable-energy" && !c.Passed);
    }

    [Fact]
    public void R1_NoObservedStates_VacuouslyValid()
    {
        PopulateRunFolder(_originalDir);
        PopulateRunFolder(_replayDir);

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R1);

        Assert.Equal(ReplayOutcome.Pass, report.Outcome);
        Assert.Contains(report.Checks, c =>
            c.CheckName == "r1-observed-present" && c.Passed);
    }

    [Fact]
    public void R2_IdenticalResiduals_Passes()
    {
        PopulateRunFolder(_originalDir, objectiveValue: 1.5e-6);
        PopulateRunFolder(_replayDir, objectiveValue: 1.5e-6);

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R2);

        Assert.Equal(ReplayOutcome.Pass, report.Outcome);
    }

    [Fact]
    public void R2_DifferentObjectiveValues_Fails()
    {
        PopulateRunFolder(_originalDir, objectiveValue: 1.0);
        PopulateRunFolder(_replayDir, objectiveValue: 2.0);

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R2);

        Assert.Equal(ReplayOutcome.Fail, report.Outcome);
        Assert.Contains(report.Checks, c =>
            c.CheckName == "r2-objective-value" && !c.Passed);
    }

    [Fact]
    public void R3_IdenticalFiles_BitExact_Passes()
    {
        // For bit-exact R3, we must write identical content to both folders.
        // We use a fixed validation bundle and fixed timestamp to ensure identical JSON.
        var fixedTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var fixedBranch = TestHelpers.CreateTestBranchRef();
        var fixedValidation = new ValidationBundle
        {
            Branch = fixedBranch,
            Records = new[]
            {
                new ValidationRecord
                {
                    RuleId = "test-rule",
                    Category = "test",
                    Passed = true,
                    Timestamp = fixedTime,
                },
            },
            AllPassed = true,
        };
        var fixedResidual = new ResidualBundle
        {
            Components = Array.Empty<ResidualComponent>(),
            ObjectiveValue = 1.5e-6,
            TotalNorm = System.Math.Sqrt(1.5e-6),
        };

        foreach (var dir in new[] { _originalDir, _replayDir })
        {
            var writer = new RunFolderWriter(dir);
            writer.CreateDirectories();
            writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
            writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());
            writer.WriteValidationBundle(fixedValidation);
            writer.WriteResidualBundle(fixedResidual);
        }

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R3);

        // R3 checks file hashes. Since both folders have identical content,
        // the R3 checks should pass.
        var r3Checks = report.Checks.Where(c => c.CheckName.StartsWith("r3-")).ToList();
        Assert.NotEmpty(r3Checks);
        Assert.All(r3Checks, c => Assert.True(c.Passed, $"{c.CheckName}: {c.Detail}"));
    }

    [Fact]
    public void R3_DifferentFiles_Fails()
    {
        PopulateRunFolder(_originalDir, objectiveValue: 1.0);
        PopulateRunFolder(_replayDir, objectiveValue: 2.0);

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R3);

        // R3 checks should detect hash differences in residual bundle
        var r3Checks = report.Checks.Where(c => c.CheckName.StartsWith("r3-")).ToList();
        Assert.Contains(r3Checks, c => !c.Passed);
    }

    [Fact]
    public void MissingOriginalContract_ReportsFailure()
    {
        // Create folders without replay contracts
        var origWriter = new RunFolderWriter(_originalDir);
        origWriter.CreateDirectories();
        origWriter.WriteBranchManifest(TestHelpers.CreateValidManifest());

        var replayWriter = new RunFolderWriter(_replayDir);
        replayWriter.CreateDirectories();

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir));

        Assert.Equal(ReplayOutcome.Fail, report.Outcome);
        Assert.Contains(report.Checks, c =>
            c.CheckName == "original-contract-exists" && !c.Passed);
    }

    [Fact]
    public void DefaultTier_UsesContractTier()
    {
        PopulateRunFolder(_originalDir);
        PopulateRunFolder(_replayDir);

        var validator = new ReplayValidator();
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir));
        // Default tier comes from original contract = R2

        Assert.Equal("R2", report.ReplayTier);
    }

    [Fact]
    public void CustomTolerance_IsRespected()
    {
        PopulateRunFolder(_originalDir, objectiveValue: 1.0);
        PopulateRunFolder(_replayDir, objectiveValue: 1.5);

        // With very large tolerance, R2 should pass
        var validator = new ReplayValidator(tolerance: 10.0);
        var report = validator.Validate(
            new RunFolderReader(_originalDir),
            new RunFolderReader(_replayDir),
            ReplayTiers.R2);

        // The objective value difference is 0.5, well within tolerance of 10.0
        var objectiveCheck = report.Checks.FirstOrDefault(c => c.CheckName == "r2-objective-value");
        Assert.NotNull(objectiveCheck);
        Assert.True(objectiveCheck.Passed);
    }

    [Fact]
    public void ReplayValidator_DefaultTolerance_Is1eNeg10()
    {
        Assert.Equal(1e-10, ReplayValidator.DefaultTolerance);
    }
}
