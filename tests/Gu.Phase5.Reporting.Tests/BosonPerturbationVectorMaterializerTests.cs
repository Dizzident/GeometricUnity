using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class BosonPerturbationVectorMaterializerTests
{
    [Fact]
    public void MaterializeFromModeJson_AcceptsPersistedPhase12ModeVector()
    {
        var result = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
            "phase12-mode-0.json",
            """
            {
              "modeId": "bg-mode-0",
              "modeVector": [0.25, -0.5, 0.75]
            }
            """,
            expectedLength: 3);

        Assert.Equal("boson-perturbation-vector-materialized", result.TerminalStatus);
        Assert.Equal("modeVector", result.SourceFieldName);
        Assert.Equal("bg-mode-0", result.ModeId);
        Assert.Equal([0.25, -0.5, 0.75], result.PerturbationVector);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void MaterializeFromModeJson_AcceptsLegacyEigenvectorCoefficientsFallback()
    {
        var result = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
            "legacy-mode.json",
            """
            {
              "modeId": "legacy-mode",
              "eigenvectorCoefficients": [1.0, 0.0]
            }
            """,
            expectedLength: 2);

        Assert.Equal("boson-perturbation-vector-materialized", result.TerminalStatus);
        Assert.Equal("eigenvectorCoefficients", result.SourceFieldName);
        Assert.Equal([1.0, 0.0], result.PerturbationVector);
    }

    [Fact]
    public void MaterializeFromModeJson_BlocksWrongLength()
    {
        var result = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
            "phase12-mode-0.json",
            """
            {
              "modeId": "bg-mode-0",
              "modeVector": [0.25, -0.5]
            }
            """,
            expectedLength: 3);

        Assert.Equal("boson-perturbation-vector-blocked", result.TerminalStatus);
        Assert.Contains("perturbation vector length must be 3", result.ClosureRequirements);
    }

    [Fact]
    public void MaterializeFromModeJson_BlocksNonFiniteValues()
    {
        var result = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
            "phase12-mode-0.json",
            """
            {
              "modeId": "bg-mode-0",
              "modeVector": [0.25, 1e999]
            }
            """,
            expectedLength: 2);

        Assert.Equal("boson-perturbation-vector-blocked", result.TerminalStatus);
        Assert.Contains("perturbation vector contains a non-finite value", result.ClosureRequirements);
    }

    [Fact]
    public void MaterializeFromModeJson_BlocksMissingVectorField()
    {
        var result = BosonPerturbationVectorMaterializer.MaterializeFromModeJson(
            "phase12-mode-0.json",
            """
            {
              "modeId": "bg-mode-0"
            }
            """,
            expectedLength: 3);

        Assert.Equal("boson-perturbation-vector-blocked", result.TerminalStatus);
        Assert.Contains(
            "mode JSON must contain 'modeVector' or 'eigenvectorCoefficients'",
            result.ClosureRequirements);
    }
}
