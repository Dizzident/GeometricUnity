using Gu.Core.Serialization;
using Gu.Phase2.Continuation;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Continuation.Tests;

public class ContinuationSerializationTests
{
    [Fact]
    public void ContinuationSpec_RoundTrips()
    {
        var spec = MakeSpec();

        var json = GuJsonDefaults.Serialize(spec);
        var deserialized = GuJsonDefaults.Deserialize<ContinuationSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("gauge-lambda", deserialized!.ParameterName);
        Assert.Equal(0.0, deserialized.LambdaStart);
        Assert.Equal(1.0, deserialized.LambdaEnd);
        Assert.Equal(0.1, deserialized.InitialStepSize);
        Assert.Equal(20, deserialized.MaxSteps);
        Assert.Equal(0.001, deserialized.MinStepSize);
        Assert.Equal(0.5, deserialized.MaxStepSize);
        Assert.Equal(1e-8, deserialized.CorrectorTolerance);
        Assert.Equal(10, deserialized.MaxCorrectorIterations);
        Assert.Equal("branch-1", deserialized.BranchManifestId);
    }

    [Fact]
    public void ContinuationEvent_RoundTrips()
    {
        var ev = new ContinuationEvent
        {
            Kind = ContinuationEventKind.HessianSignChange,
            Lambda = 0.5,
            Description = "Eigenvalue sign change",
            Severity = "critical",
        };

        var json = GuJsonDefaults.Serialize(ev);
        var deserialized = GuJsonDefaults.Deserialize<ContinuationEvent>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(ContinuationEventKind.HessianSignChange, deserialized!.Kind);
        Assert.Equal(0.5, deserialized.Lambda);
        Assert.Equal("critical", deserialized.Severity);
    }

    [Fact]
    public void ContinuationEventKind_SerializesAsString()
    {
        var ev = new ContinuationEvent
        {
            Kind = ContinuationEventKind.SingularValueCollapse,
            Lambda = 0.3,
            Description = "Near-zero eigenvalue",
            Severity = "critical",
        };

        var json = GuJsonDefaults.Serialize(ev);
        Assert.Contains("\"SingularValueCollapse\"", json);
    }

    [Fact]
    public void ContinuationStep_RoundTrips()
    {
        var step = new ContinuationStep
        {
            StepIndex = 5,
            Lambda = 0.5,
            Arclength = 0.55,
            StepSize = 0.1,
            ResidualNorm = 1e-10,
            CorrectorIterations = 3,
            CorrectorConverged = true,
            Events = Array.Empty<ContinuationEvent>(),
        };

        var json = GuJsonDefaults.Serialize(step);
        var deserialized = GuJsonDefaults.Deserialize<ContinuationStep>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(5, deserialized!.StepIndex);
        Assert.Equal(0.5, deserialized.Lambda);
        Assert.Equal(0.55, deserialized.Arclength);
        Assert.True(deserialized.CorrectorConverged);
        Assert.Empty(deserialized.Events);
    }

    [Fact]
    public void ContinuationResult_RoundTrips()
    {
        var result = MakeResult();

        var json = GuJsonDefaults.Serialize(result);
        var deserialized = GuJsonDefaults.Deserialize<ContinuationResult>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("reached-end", deserialized!.TerminationReason);
        Assert.Equal(2, deserialized.TotalSteps);
        Assert.Equal(0.0, deserialized.LambdaMin);
        Assert.Equal(1.0, deserialized.LambdaMax);
        Assert.Single(deserialized.Path);
        Assert.Empty(deserialized.AllEvents);
    }

    [Fact]
    public void StabilityAtlas_RoundTrips()
    {
        var atlas = new StabilityAtlas
        {
            AtlasId = "atlas-1",
            BranchManifestId = "branch-1",
            FamilyDescription = "Test family",
            Paths = new[] { MakeResult() },
            HessianRecords = Array.Empty<HessianRecord>(),
            SymbolSamples = Array.Empty<PrincipalSymbolRecord>(),
            LinearizationRecords = Array.Empty<LinearizationRecord>(),
            GaugeFixedLinearizationRecords = Array.Empty<GaugeFixedLinearizationRecord>(),
            BifurcationIndicators = Array.Empty<BifurcationIndicatorRecord>(),
            DiscretizationNotes = "h=0.1 uniform mesh",
            TheoremStatusNotes = "Ellipticity unproven",
            Timestamp = DateTimeOffset.UtcNow,
        };

        var json = GuJsonDefaults.Serialize(atlas);
        var deserialized = GuJsonDefaults.Deserialize<StabilityAtlas>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("atlas-1", deserialized!.AtlasId);
        Assert.Equal("branch-1", deserialized.BranchManifestId);
        Assert.Equal("Test family", deserialized.FamilyDescription);
        Assert.Single(deserialized.Paths);
        Assert.Equal("h=0.1 uniform mesh", deserialized.DiscretizationNotes);
        Assert.Equal("Ellipticity unproven", deserialized.TheoremStatusNotes);
    }

    [Fact]
    public void ContinuationResult_WithEvents_RoundTrips()
    {
        var ev = new ContinuationEvent
        {
            Kind = ContinuationEventKind.StepRejectionBurst,
            Lambda = 0.7,
            Description = "3 consecutive rejections",
            Severity = "warning",
        };

        var step = new ContinuationStep
        {
            StepIndex = 0,
            Lambda = 0.0,
            Arclength = 0.0,
            StepSize = 0.1,
            ResidualNorm = 0.0,
            CorrectorIterations = 0,
            CorrectorConverged = true,
            Events = new[] { ev },
        };

        var result = new ContinuationResult
        {
            Spec = MakeSpec(),
            Path = new[] { step },
            TerminationReason = "min-step-size",
            TotalSteps = 1,
            TotalArclength = 0.0,
            LambdaMin = 0.0,
            LambdaMax = 0.0,
            AllEvents = new[] { ev },
            Timestamp = DateTimeOffset.UtcNow,
        };

        var json = GuJsonDefaults.Serialize(result);
        var deserialized = GuJsonDefaults.Deserialize<ContinuationResult>(json);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized!.AllEvents);
        Assert.Equal(ContinuationEventKind.StepRejectionBurst, deserialized.AllEvents[0].Kind);
        Assert.Single(deserialized.Path[0].Events);
    }

    private static ContinuationSpec MakeSpec()
    {
        return new ContinuationSpec
        {
            ParameterName = "gauge-lambda",
            LambdaStart = 0.0,
            LambdaEnd = 1.0,
            InitialStepSize = 0.1,
            MaxSteps = 20,
            MinStepSize = 0.001,
            MaxStepSize = 0.5,
            CorrectorTolerance = 1e-8,
            MaxCorrectorIterations = 10,
            BranchManifestId = "branch-1",
        };
    }

    private static ContinuationResult MakeResult()
    {
        var step = new ContinuationStep
        {
            StepIndex = 0,
            Lambda = 0.0,
            Arclength = 0.0,
            StepSize = 0.1,
            ResidualNorm = 0.0,
            CorrectorIterations = 0,
            CorrectorConverged = true,
            Events = Array.Empty<ContinuationEvent>(),
        };

        return new ContinuationResult
        {
            Spec = MakeSpec(),
            Path = new[] { step },
            TerminationReason = "reached-end",
            TotalSteps = 2,
            TotalArclength = 1.0,
            LambdaMin = 0.0,
            LambdaMax = 1.0,
            AllEvents = Array.Empty<ContinuationEvent>(),
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}
