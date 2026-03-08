using Gu.Artifacts;
using Gu.Core;

namespace Gu.Artifacts.Tests;

public class ReplayContractValidatorTests : IDisposable
{
    private readonly string _originalDir;
    private readonly string _replayDir;

    public ReplayContractValidatorTests()
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

    private void PopulateRunFolder(string path, string backendId = "cpu-reference", double objectiveValue = 1.5e-6)
    {
        var writer = new RunFolderWriter(path);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateTestManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract(backendId: backendId));
        writer.WriteResidualBundle(TestHelpers.CreateTestResidualBundle(objectiveValue));
        writer.WriteValidationBundle(TestHelpers.CreateTestValidationBundle());
    }

    [Fact]
    public void IdenticalFolders_R1_Pass()
    {
        PopulateRunFolder(_originalDir);
        PopulateRunFolder(_replayDir);

        var origWriter = new RunFolderWriter(_originalDir);
        var replayWriter = new RunFolderWriter(_replayDir);

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R1);
        Assert.Equal(ReplayOutcome.Pass, report.Outcome);
        Assert.Equal(ReplayTiers.R1, report.ReplayTier);
        Assert.All(report.Checks, c => Assert.True(c.Passed, $"Check '{c.CheckName}' failed: {c.Detail}"));
    }

    [Fact]
    public void IdenticalFolders_R2_Pass()
    {
        PopulateRunFolder(_originalDir);
        PopulateRunFolder(_replayDir);

        var origWriter = new RunFolderWriter(_originalDir);
        var replayWriter = new RunFolderWriter(_replayDir);

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R2);
        Assert.Equal(ReplayOutcome.Pass, report.Outcome);
    }

    [Fact]
    public void DifferentObjectiveValue_R2_Fails()
    {
        PopulateRunFolder(_originalDir, objectiveValue: 1.0);
        PopulateRunFolder(_replayDir, objectiveValue: 2.0);

        var origWriter = new RunFolderWriter(_originalDir);
        var replayWriter = new RunFolderWriter(_replayDir);

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R2);
        Assert.Equal(ReplayOutcome.Fail, report.Outcome);
        Assert.Contains(report.Checks, c => !c.Passed && c.CheckName == "objective-value-match");
    }

    [Fact]
    public void MismatchedBranchId_R1_Fails()
    {
        PopulateRunFolder(_originalDir);

        // Write a different manifest in replay folder
        var writer = new RunFolderWriter(_replayDir);
        writer.CreateDirectories();
        var differentManifest = new BranchManifest
        {
            BranchId = "different-branch",
            SchemaVersion = "1.0.0",
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
        writer.WriteBranchManifest(differentManifest);
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());

        var origWriter = new RunFolderWriter(_originalDir);
        var report = ReplayContractValidator.Validate(origWriter, writer, ReplayTiers.R1);

        Assert.Equal(ReplayOutcome.Fail, report.Outcome);
        Assert.Contains(report.Checks, c => !c.Passed && c.CheckName == "manifest-branch-id");
    }

    [Fact]
    public void MissingReplayContract_ReturnsInvalid()
    {
        PopulateRunFolder(_originalDir);

        // Create replay folder without replay contract
        var replayWriter = new RunFolderWriter(_replayDir);
        replayWriter.CreateDirectories();
        replayWriter.WriteBranchManifest(TestHelpers.CreateTestManifest());
        // Deliberately no replay contract

        var origWriter = new RunFolderWriter(_originalDir);

        // Remove the original's replay contract to test invalid case
        File.Delete(Path.Combine(_originalDir, RunFolderLayout.ReplayContractFile));

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R1);
        Assert.Equal(ReplayOutcome.Invalid, report.Outcome);
    }

    [Fact]
    public void R3_RequiresDifferentBackends()
    {
        PopulateRunFolder(_originalDir, backendId: "cpu-reference");
        PopulateRunFolder(_replayDir, backendId: "cpu-reference");

        var origWriter = new RunFolderWriter(_originalDir);
        var replayWriter = new RunFolderWriter(_replayDir);

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R3);

        Assert.Contains(report.Checks, c => c.CheckName == "cross-backend-different" && !c.Passed);
    }

    [Fact]
    public void R3_DifferentBackends_PassesCrossBackendCheck()
    {
        PopulateRunFolder(_originalDir, backendId: "cpu-reference");
        PopulateRunFolder(_replayDir, backendId: "cuda");

        var origWriter = new RunFolderWriter(_originalDir);
        var replayWriter = new RunFolderWriter(_replayDir);

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R3);

        Assert.Contains(report.Checks, c => c.CheckName == "cross-backend-different" && c.Passed);
    }

    [Fact]
    public void R0_OnlyChecksStructure()
    {
        PopulateRunFolder(_originalDir);
        PopulateRunFolder(_replayDir);

        var origWriter = new RunFolderWriter(_originalDir);
        var replayWriter = new RunFolderWriter(_replayDir);

        var report = ReplayContractValidator.Validate(origWriter, replayWriter, ReplayTiers.R0);
        Assert.Equal(ReplayOutcome.Pass, report.Outcome);

        // R0 should not have branch coherence checks
        Assert.DoesNotContain(report.Checks, c => c.CheckName.StartsWith("manifest-"));
    }

    [Fact]
    public void ValidateContract_ValidContract_NoErrors()
    {
        var contract = TestHelpers.CreateTestReplayContract();
        var errors = ReplayContractValidator.ValidateContract(contract);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateContract_EmptyBackendId_ReportsError()
    {
        var contract = new ReplayContract
        {
            BranchManifest = TestHelpers.CreateTestManifest(),
            Deterministic = true,
            BackendId = "",
            ReplayTier = "R1",
        };
        var errors = ReplayContractValidator.ValidateContract(contract);
        Assert.Contains(errors, e => e.Contains("BackendId"));
    }

    [Fact]
    public void ValidateContract_InvalidTier_ReportsError()
    {
        var contract = new ReplayContract
        {
            BranchManifest = TestHelpers.CreateTestManifest(),
            Deterministic = true,
            BackendId = "cpu-reference",
            ReplayTier = "R99",
        };
        var errors = ReplayContractValidator.ValidateContract(contract);
        Assert.Contains(errors, e => e.Contains("ReplayTier"));
    }

    [Fact]
    public void ValidateContract_NonDeterministicWithoutDeclaration_ReportsError()
    {
        var contract = new ReplayContract
        {
            BranchManifest = TestHelpers.CreateTestManifest(),
            Deterministic = false,
            BackendId = "cpu-reference",
            ReplayTier = "R1",
        };
        var errors = ReplayContractValidator.ValidateContract(contract);
        Assert.Contains(errors, e => e.Contains("NonDeterminismDeclaration"));
    }

    [Fact]
    public void ReplayTiers_MeetsTier_CorrectlyCompares()
    {
        Assert.True(ReplayTiers.MeetsTier("R3", "R2"));
        Assert.True(ReplayTiers.MeetsTier("R2", "R2"));
        Assert.False(ReplayTiers.MeetsTier("R1", "R2"));
        Assert.True(ReplayTiers.MeetsTier("R0", "R0"));
    }

    [Fact]
    public void ReplayTiers_TierLevel_ReturnsCorrectValues()
    {
        Assert.Equal(0, ReplayTiers.TierLevel("R0"));
        Assert.Equal(1, ReplayTiers.TierLevel("R1"));
        Assert.Equal(2, ReplayTiers.TierLevel("R2"));
        Assert.Equal(3, ReplayTiers.TierLevel("R3"));
    }

    [Fact]
    public void ReplayTiers_InvalidTier_Throws()
    {
        Assert.Throws<ArgumentException>(() => ReplayTiers.TierLevel("R99"));
    }
}
