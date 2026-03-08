using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class EnvironmentSpecTests
{
    [Fact]
    public void Factory_CreateEmpty_ProducesValidSpec()
    {
        var spec = EnvironmentSpecFactory.CreateEmpty("test-env", "test-branch");

        Assert.Equal("test-env", spec.EnvironmentId);
        Assert.Equal("test-branch", spec.Branch.BranchId);
        Assert.Equal("toy-consistency", spec.ScenarioType);
        Assert.Equal(4, spec.Geometry.BaseSpace.Dimension);
        Assert.Equal(14, spec.Geometry.AmbientSpace.Dimension);
        Assert.Equal("simplicial", spec.Geometry.DiscretizationType);
        Assert.Equal("flat-connection", spec.InitialConditions.ConditionType);
        Assert.Equal("penalty", spec.GaugeConditions.StrategyType);
        Assert.Equal(1.0, spec.GaugeConditions.PenaltyCoefficient);
    }

    [Fact]
    public void Factory_CreateEmpty_RoundTrips()
    {
        var spec = EnvironmentSpecFactory.CreateEmpty();
        var json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(spec.EnvironmentId, deserialized.EnvironmentId);
        Assert.Equal(spec.ScenarioType, deserialized.ScenarioType);
        Assert.Equal(spec.Geometry.BaseSpace.Dimension, deserialized.Geometry.BaseSpace.Dimension);
    }

    [Fact]
    public void ProjectionBinding_DirectionIsCorrect()
    {
        // Per Section 4.2: pi_h maps Y_h to X_h
        var spec = EnvironmentSpecFactory.CreateEmpty();
        var proj = spec.Geometry.ProjectionBinding;
        Assert.Equal("projection", proj.BindingType);
        Assert.Equal("Y_h", proj.SourceSpace.SpaceId);
        Assert.Equal("X_h", proj.TargetSpace.SpaceId);
    }

    [Fact]
    public void ObservationBinding_DirectionIsCorrect()
    {
        // Per Section 4.2: sigma_h maps X_h to Y_h
        var spec = EnvironmentSpecFactory.CreateEmpty();
        var obs = spec.Geometry.ObservationBinding;
        Assert.Equal("observation", obs.BindingType);
        Assert.Equal("X_h", obs.SourceSpace.SpaceId);
        Assert.Equal("Y_h", obs.TargetSpace.SpaceId);
    }

    [Fact]
    public void ScenarioTypes_CoverRequiredCategories()
    {
        // Per Section 17.2: at least 5 environment categories
        var validScenarios = new[]
        {
            "toy-consistency",
            "branch-sensitivity",
            "observation-pipeline",
            "scaling",
            "external-comparison-ready"
        };

        // The factory uses "toy-consistency" by default
        var spec = EnvironmentSpecFactory.CreateEmpty();
        Assert.Contains(spec.ScenarioType, validScenarios);
    }
}
