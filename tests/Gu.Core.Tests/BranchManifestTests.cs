using System.Text.Json;
using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class BranchManifestTests
{
    private static BranchManifest CreateMinimalV1() => new()
    {
        BranchId = "minimal-gu-v1",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "manuscript-r1",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "simplicial-patch-v1",
        ActiveObservationBranch = "sigma-pullback-v1",
        ActiveTorsionBranch = "torsion-local-v1",
        ActiveShiabBranch = "shiab-mc-v1",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "standard-basis",
        ComponentOrderId = "lexicographic",
        AdjointConventionId = "killing-form",
        PairingConventionId = "trace-pairing",
        NormConventionId = "L2-norm",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = new[] { "IA-1", "IA-2", "IA-3", "IA-4", "IA-5", "IA-6" },
        InsertedChoiceIds = new[] { "IX-1", "IX-2", "IX-3", "IX-4", "IX-5" },
        Parameters = new Dictionary<string, string>
        {
            ["gaugePenaltyWeight"] = "1.0",
            ["solverMaxIter"] = "100"
        }
    };

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = CreateMinimalV1();
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<BranchManifest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.BranchId, deserialized.BranchId);
        Assert.Equal(original.SchemaVersion, deserialized.SchemaVersion);
        Assert.Equal(original.SourceEquationRevision, deserialized.SourceEquationRevision);
        Assert.Equal(original.CodeRevision, deserialized.CodeRevision);
        Assert.Equal(original.ActiveGeometryBranch, deserialized.ActiveGeometryBranch);
        Assert.Equal(original.ActiveObservationBranch, deserialized.ActiveObservationBranch);
        Assert.Equal(original.ActiveTorsionBranch, deserialized.ActiveTorsionBranch);
        Assert.Equal(original.ActiveShiabBranch, deserialized.ActiveShiabBranch);
        Assert.Equal(original.ActiveGaugeStrategy, deserialized.ActiveGaugeStrategy);
        Assert.Equal(original.BaseDimension, deserialized.BaseDimension);
        Assert.Equal(original.AmbientDimension, deserialized.AmbientDimension);
        Assert.Equal(original.LieAlgebraId, deserialized.LieAlgebraId);
        Assert.Equal(original.BasisConventionId, deserialized.BasisConventionId);
        Assert.Equal(original.ComponentOrderId, deserialized.ComponentOrderId);
        Assert.Equal(original.AdjointConventionId, deserialized.AdjointConventionId);
        Assert.Equal(original.PairingConventionId, deserialized.PairingConventionId);
        Assert.Equal(original.NormConventionId, deserialized.NormConventionId);
        Assert.Equal(original.InsertedAssumptionIds, deserialized.InsertedAssumptionIds);
        Assert.Equal(original.InsertedChoiceIds, deserialized.InsertedChoiceIds);
        Assert.NotNull(deserialized.Parameters);
        Assert.Equal("1.0", deserialized.Parameters["gaugePenaltyWeight"]);
    }

    [Fact]
    public void BaseDimension_DefaultsTo4()
    {
        // Per IA-1: active branch metadata must default to dim(X) = 4
        var manifest = BranchManifestFactory.CreateEmpty();
        Assert.Equal(4, manifest.BaseDimension);
    }

    [Fact]
    public void Factory_CreateEmpty_ProducesValidManifest()
    {
        var manifest = BranchManifestFactory.CreateEmpty("test-branch");

        Assert.Equal("test-branch", manifest.BranchId);
        Assert.Equal(BranchManifestFactory.CurrentSchemaVersion, manifest.SchemaVersion);
        Assert.Equal(4, manifest.BaseDimension);
        Assert.Equal(14, manifest.AmbientDimension);
        Assert.Equal("penalty", manifest.ActiveGaugeStrategy);
        Assert.NotNull(manifest.InsertedAssumptionIds);
        Assert.NotNull(manifest.InsertedChoiceIds);
    }

    [Fact]
    public void Factory_CreateEmpty_RoundTrips()
    {
        var manifest = BranchManifestFactory.CreateEmpty();
        var json = GuJsonDefaults.Serialize(manifest);
        var deserialized = GuJsonDefaults.Deserialize<BranchManifest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(manifest.BranchId, deserialized.BranchId);
        Assert.Equal(manifest.SchemaVersion, deserialized.SchemaVersion);
    }

    [Fact]
    public void Parameters_IsOptionalAndNullable()
    {
        var manifest = new BranchManifest
        {
            BranchId = "no-params",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "abc",
            ActiveGeometryBranch = "geom",
            ActiveObservationBranch = "obs",
            ActiveTorsionBranch = "torsion",
            ActiveShiabBranch = "shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "std",
            ComponentOrderId = "lex",
            AdjointConventionId = "killing",
            PairingConventionId = "trace",
            NormConventionId = "L2",
            DifferentialFormMetricId = "hodge",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>()
        };

        Assert.Null(manifest.Parameters);

        // With GuJsonDefaults (WhenWritingNull), null Parameters should be omitted from JSON
        var json = GuJsonDefaults.Serialize(manifest);
        Assert.DoesNotContain("\"parameters\"", json);
    }

    [Fact]
    public void BranchId_IsWrittenToAllOutputs()
    {
        // Per Milestone 1 acceptance: branch ID is written to all outputs
        var manifest = CreateMinimalV1();
        var json = GuJsonDefaults.Serialize(manifest);
        Assert.Contains("\"branchId\"", json);
        Assert.Contains("minimal-gu-v1", json);
    }
}
