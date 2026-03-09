using Gu.Core;
using Gu.Core.Serialization;
using Gu.Validation;

namespace Gu.Validation.Tests;

public class SchemaValidatorTests
{
    [Theory]
    [InlineData("branch")]
    [InlineData("geometry")]
    [InlineData("artifact")]
    [InlineData("observed")]
    [InlineData("validation")]
    public void LoadEmbeddedSchema_AllKnownSchemas_Load(string schemaName)
    {
        var schema = SchemaValidator.LoadEmbeddedSchema(schemaName);
        Assert.NotNull(schema);
    }

    [Fact]
    public void LoadEmbeddedSchema_UnknownSchema_Throws()
    {
        Assert.Throws<ArgumentException>(() => SchemaValidator.LoadEmbeddedSchema("nonexistent"));
    }

    [Fact]
    public void Validate_ValidBranchManifest_Passes()
    {
        var json = """
        {
            "branchId": "test-branch",
            "schemaVersion": "1.0",
            "sourceEquationRevision": "rev1",
            "codeRevision": "abc123",
            "activeGeometryBranch": "geo-1",
            "activeObservationBranch": "obs-1",
            "activeTorsionBranch": "torsion-1",
            "activeShiabBranch": "shiab-1",
            "activeGaugeStrategy": "coulomb",
            "baseDimension": 4,
            "ambientDimension": 14,
            "differentialFormMetricId": "flat",
            "lieAlgebraId": "su2",
            "basisConventionId": "standard",
            "componentOrderId": "lexicographic",
            "adjointConventionId": "physicist",
            "pairingConventionId": "trace",
            "normConventionId": "l2",
            "insertedAssumptionIds": ["IA-1"],
            "insertedChoiceIds": ["IX-1"]
        }
        """;

        var result = SchemaValidator.Validate(json, "branch");

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        Assert.Equal("branch", result.SchemaName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MissingRequiredField_Fails()
    {
        var json = """
        {
            "branchId": "test-branch"
        }
        """;

        var result = SchemaValidator.Validate(json, "branch");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_InvalidJson_Fails()
    {
        var result = SchemaValidator.Validate("not valid json {{{", "branch");

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Invalid JSON"));
    }

    [Fact]
    public void Validate_AdditionalProperty_Fails()
    {
        var json = """
        {
            "branchId": "test-branch",
            "schemaVersion": "1.0",
            "sourceEquationRevision": "rev1",
            "codeRevision": "abc123",
            "activeGeometryBranch": "geo-1",
            "activeObservationBranch": "obs-1",
            "activeTorsionBranch": "torsion-1",
            "activeShiabBranch": "shiab-1",
            "activeGaugeStrategy": "coulomb",
            "baseDimension": 4,
            "ambientDimension": 14,
            "differentialFormMetricId": "flat",
            "lieAlgebraId": "su2",
            "basisConventionId": "standard",
            "componentOrderId": "lexicographic",
            "adjointConventionId": "physicist",
            "pairingConventionId": "trace",
            "normConventionId": "l2",
            "insertedAssumptionIds": ["IA-1"],
            "insertedChoiceIds": ["IX-1"],
            "unknownField": "should fail"
        }
        """;

        var result = SchemaValidator.Validate(json, "branch");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidValidationBundle_Passes()
    {
        var json = """
        {
            "branch": {
                "branchId": "test",
                "schemaVersion": "1.0"
            },
            "records": [
                {
                    "ruleId": "jacobi-identity",
                    "category": "algebraic",
                    "passed": true,
                    "timestamp": "2026-01-01T00:00:00Z"
                }
            ],
            "allPassed": true
        }
        """;

        var result = SchemaValidator.Validate(json, "validation");

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_ValidGeometry_Passes()
    {
        var json = """
        {
            "baseSpace": { "spaceId": "X", "dimension": 4 },
            "ambientSpace": { "spaceId": "Y", "dimension": 14 },
            "discretizationType": "simplicial",
            "quadratureRuleId": "midpoint",
            "basisFamilyId": "whitney",
            "projectionBinding": {
                "bindingType": "pullback",
                "sourceSpace": { "spaceId": "Y", "dimension": 14 },
                "targetSpace": { "spaceId": "X", "dimension": 4 }
            },
            "observationBinding": {
                "bindingType": "section",
                "sourceSpace": { "spaceId": "X", "dimension": 4 },
                "targetSpace": { "spaceId": "Y", "dimension": 14 }
            },
            "patches": [
                { "patchId": "patch-0", "elementCount": 100 }
            ]
        }
        """;

        var result = SchemaValidator.Validate(json, "geometry");

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void ValidateWithSchemaFile_BranchSchema_Works()
    {
        var schemaPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "..", "schemas", "branch.schema.json");

        // Resolve to absolute path
        schemaPath = Path.GetFullPath(schemaPath);

        if (!File.Exists(schemaPath))
        {
            // Try from the repo root
            schemaPath = "/home/josh/Documents/GitHub/GeometricUnity/schemas/branch.schema.json";
        }

        if (!File.Exists(schemaPath))
            return; // Skip if running in CI without schemas

        var json = """
        {
            "branchId": "test",
            "schemaVersion": "1.0",
            "sourceEquationRevision": "rev1",
            "codeRevision": "abc",
            "activeGeometryBranch": "g",
            "activeObservationBranch": "o",
            "activeTorsionBranch": "t",
            "activeShiabBranch": "s",
            "activeGaugeStrategy": "coulomb",
            "baseDimension": 4,
            "ambientDimension": 14,
            "differentialFormMetricId": "flat",
            "lieAlgebraId": "su2",
            "basisConventionId": "standard",
            "componentOrderId": "lex",
            "adjointConventionId": "physicist",
            "pairingConventionId": "trace",
            "normConventionId": "l2",
            "insertedAssumptionIds": [],
            "insertedChoiceIds": []
        }
        """;

        var result = SchemaValidator.ValidateWithSchemaFile(json, schemaPath);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void KnownSchemas_ContainsAllFive()
    {
        Assert.Equal(5, SchemaValidator.KnownSchemas.Count);
        Assert.Contains("branch", SchemaValidator.KnownSchemas);
        Assert.Contains("geometry", SchemaValidator.KnownSchemas);
        Assert.Contains("artifact", SchemaValidator.KnownSchemas);
        Assert.Contains("observed", SchemaValidator.KnownSchemas);
        Assert.Contains("validation", SchemaValidator.KnownSchemas);
    }
}
