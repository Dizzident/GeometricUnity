using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase5CampaignSpecValidatorTests : IDisposable
{
    private readonly string _tempDir;

    public Phase5CampaignSpecValidatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"gu-validator-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    /// <summary>
    /// Creates a minimal valid spec referencing files that actually exist in <paramref name="dir"/>.
    /// </summary>
    private Phase5CampaignSpec MakeValidSpec(string dir, string schemaVersion = "1.0.0",
        IReadOnlyList<string>? extraEnvPaths = null)
    {
        // Create the required stub files
        var requiredFiles = new[]
        {
            "targets.json",
            "branch_quantity_values.json",
            "refinement_values.json",
            "observables.json",
            "registry.json",
            "env_toy.json",
        };
        foreach (var f in requiredFiles)
            File.WriteAllText(Path.Combine(dir, f), "{}");

        var envPaths = new List<string> { "env_toy.json" };
        if (extraEnvPaths != null)
            envPaths.AddRange(extraEnvPaths);

        return new Phase5CampaignSpec
        {
            CampaignId = "test-campaign",
            SchemaVersion = schemaVersion,
            BranchFamilySpec = new BranchRobustnessStudySpec
            {
                StudyId = "branch-study",
                BranchVariantIds = ["V1", "V2"],
                TargetQuantityIds = ["q1"],
            },
            RefinementSpec = new RefinementStudySpec
            {
                StudyId = "refine-study",
                SchemaVersion = "1.0",
                BranchManifestId = "branch-1",
                TargetQuantities = ["q1"],
                RefinementLevels = [
                    new RefinementLevel { LevelId = "L0", MeshParameterX = 1.0, MeshParameterF = 1.0 },
                    new RefinementLevel { LevelId = "L1", MeshParameterX = 0.5, MeshParameterF = 0.5 },
                ],
                Provenance = MakeProvenance(),
            },
            EnvironmentCampaignSpec = new EnvironmentCampaignSpec
            {
                CampaignId = "env-campaign",
                SchemaVersion = "1.0",
                EnvironmentIds = ["env-toy"],
                BranchManifestId = "branch-1",
                TargetQuantities = ["q1"],
                Provenance = MakeProvenance(),
            },
            ExternalTargetTablePath = "targets.json",
            BranchQuantityValuesPath = "branch_quantity_values.json",
            RefinementValuesPath = "refinement_values.json",
            ObservablesPath = "observables.json",
            EnvironmentRecordPaths = envPaths,
            RegistryPath = "registry.json",
            CalibrationPolicy = new CalibrationPolicy
            {
                PolicyId = "test-policy",
                Mode = "sigma",
                SigmaThreshold = 5.0,
            },
            FalsificationPolicy = new FalsificationPolicy(),
            Provenance = MakeProvenance(),
        };
    }

    // -----------------------------------------------------------------------
    // Tests: valid specs
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidSpec_NonReferenceMode_ReturnsValid()
    {
        var spec = MakeValidSpec(_tempDir);
        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ValidReferenceSpec_WithAllSidecars_ReturnsValid()
    {
        // Create sidecar stubs and a second env record
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        File.WriteAllText(Path.Combine(_tempDir, "obs_chain.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");

        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0",
            extraEnvPaths: ["env_structured.json"]);

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        Assert.Empty(result.Errors);
    }

    // -----------------------------------------------------------------------
    // Tests: missing required files
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_MissingTargetTableFile_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);
        File.Delete(Path.Combine(_tempDir, "targets.json"));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("externalTargetTablePath") && e.Contains("not found"));
    }

    [Fact]
    public void Validate_MissingRegistryFile_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);
        File.Delete(Path.Combine(_tempDir, "registry.json"));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("registryPath") && e.Contains("not found"));
    }

    [Fact]
    public void Validate_MissingEnvironmentRecordFile_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);
        File.Delete(Path.Combine(_tempDir, "env_toy.json"));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("environmentRecordPaths[0]") && e.Contains("not found"));
    }

    // -----------------------------------------------------------------------
    // Tests: reference mode — missing sidecars
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ReferenceModeWithNullSidecars_ReportsAllFour()
    {
        // Create a second env record so the env count check passes
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0",
            extraEnvPaths: ["env_structured.json"]);

        // Sidecars are all null (default)
        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("observationChainPath") && e.Contains("required in reference mode"));
        Assert.Contains(result.Errors, e => e.Contains("environmentVariancePath") && e.Contains("required in reference mode"));
        Assert.Contains(result.Errors, e => e.Contains("representationContentPath") && e.Contains("required in reference mode"));
        Assert.Contains(result.Errors, e => e.Contains("couplingConsistencyPath") && e.Contains("required in reference mode"));
    }

    [Fact]
    public void Validate_ReferenceModeOnlyOneEnvRecord_ReportsError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "obs_chain.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");

        // Only 1 env record (no extraEnvPaths)
        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0");

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("at least 2 environment records"));
    }

    [Fact]
    public void Validate_ReferenceModeOldSchemaVersion_ReportsError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        File.WriteAllText(Path.Combine(_tempDir, "obs_chain.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");

        // Schema version is 1.0.0, not 1.1.0
        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.0.0",
            extraEnvPaths: ["env_structured.json"]);

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("schemaVersion") && e.Contains("1.1.0"));
    }

    [Fact]
    public void Validate_ReferenceModeWithMissingSidecarFile_ReportsError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        // obs_chain.json intentionally NOT created
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");

        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0",
            extraEnvPaths: ["env_structured.json"]);

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",      // file does not exist
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("observationChainPath") && e.Contains("not found"));
    }
}
