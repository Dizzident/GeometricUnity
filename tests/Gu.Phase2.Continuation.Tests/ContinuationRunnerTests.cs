using Gu.Core;
using Gu.Phase2.Continuation;

namespace Gu.Phase2.Continuation.Tests;

public class ContinuationRunnerTests
{
    [Fact]
    public void Run_SimpleLinearProblem_ReachesEnd()
    {
        // G(u, lambda) = u - lambda: solution u = lambda
        var runner = new ContinuationRunner(
            evaluateResidual: (state, lambda) =>
            {
                double residual = state.Coefficients[0] - lambda;
                return (MakeTensor(residual), System.Math.Abs(residual));
            },
            solveCorrector: (predicted, lambda, tol, maxIter) =>
            {
                // Exact solve: u = lambda
                return (MakeTensor(lambda), 1, true);
            });

        var spec = MakeSpec(lambdaStart: 0.0, lambdaEnd: 1.0, maxSteps: 20);
        var result = runner.Run(spec, MakeTensor(0.0));

        Assert.Equal("reached-end", result.TerminationReason);
        Assert.True(result.Path.Count >= 2);
        Assert.Equal(0.0, result.LambdaMin, 5);
        Assert.Equal(1.0, result.LambdaMax, 5);
    }

    [Fact]
    public void Run_BackwardDirection_Works()
    {
        var runner = new ContinuationRunner(
            evaluateResidual: (state, lambda) =>
            {
                double r = state.Coefficients[0] - lambda;
                return (MakeTensor(r), System.Math.Abs(r));
            },
            solveCorrector: (predicted, lambda, tol, maxIter) =>
                (MakeTensor(lambda), 1, true));

        var spec = MakeSpec(lambdaStart: 1.0, lambdaEnd: 0.0, maxSteps: 20);
        var result = runner.Run(spec, MakeTensor(1.0));

        Assert.Equal("reached-end", result.TerminationReason);
        Assert.Equal(0.0, result.LambdaMin, 5);
        Assert.Equal(1.0, result.LambdaMax, 5);
    }

    [Fact]
    public void Run_CorrectorFails_HalvesStepAndTerminates()
    {
        int callCount = 0;
        var runner = new ContinuationRunner(
            evaluateResidual: (state, lambda) => (MakeTensor(0.0), 0.0),
            solveCorrector: (predicted, lambda, tol, maxIter) =>
            {
                callCount++;
                // Always fail
                return (predicted, maxIter, false);
            });

        var spec = MakeSpec(lambdaStart: 0.0, lambdaEnd: 1.0, maxSteps: 50,
            minStepSize: 0.01);
        var result = runner.Run(spec, MakeTensor(0.0));

        Assert.Equal("min-step-size", result.TerminationReason);
        // Should have detected step rejection burst
    }

    [Fact]
    public void Run_MaxStepsReached_Terminates()
    {
        var runner = new ContinuationRunner(
            evaluateResidual: (state, lambda) => (MakeTensor(0.0), 0.0),
            solveCorrector: (predicted, lambda, tol, maxIter) =>
                (MakeTensor(lambda), 5, true));

        var spec = MakeSpec(lambdaStart: 0.0, lambdaEnd: 100.0, maxSteps: 3,
            stepSize: 0.1);
        var result = runner.Run(spec, MakeTensor(0.0));

        Assert.Equal("max-steps", result.TerminationReason);
        Assert.Equal(4, result.TotalSteps); // initial + 3 steps
    }

    [Fact]
    public void Run_AdaptiveStepGrows_WhenCorrectorFast()
    {
        var runner = new ContinuationRunner(
            evaluateResidual: (state, lambda) => (MakeTensor(0.0), 0.0),
            solveCorrector: (predicted, lambda, tol, maxIter) =>
                // Fast convergence (1 iteration out of max 10)
                (MakeTensor(lambda), 1, true));

        var spec = MakeSpec(lambdaStart: 0.0, lambdaEnd: 10.0, maxSteps: 100,
            stepSize: 0.1, maxStepSize: 1.0);
        var result = runner.Run(spec, MakeTensor(0.0));

        Assert.Equal("reached-end", result.TerminationReason);
        // With adaptive step growth, should reach end in fewer than 100 steps
        Assert.True(result.TotalSteps < 100);
    }

    [Fact]
    public void Run_RecordsEvents()
    {
        int step = 0;
        var runner = new ContinuationRunner(
            evaluateResidual: (state, lambda) => (MakeTensor(0.0), 0.0),
            solveCorrector: (predicted, lambda, tol, maxIter) =>
            {
                step++;
                // Fail first 3 times, then succeed
                if (step <= 3)
                    return (predicted, maxIter, false);
                return (MakeTensor(lambda), 1, true);
            });

        var spec = MakeSpec(lambdaStart: 0.0, lambdaEnd: 1.0, maxSteps: 20);
        var result = runner.Run(spec, MakeTensor(0.0));

        Assert.Contains(result.AllEvents, e => e.Kind == ContinuationEventKind.StepRejectionBurst);
    }

    [Fact]
    public void Run_NullSpec_Throws()
    {
        var runner = new ContinuationRunner(
            evaluateResidual: (s, l) => (MakeTensor(0), 0),
            solveCorrector: (s, l, t, m) => (s, 0, true));

        Assert.Throws<ArgumentNullException>(() => runner.Run(null!, MakeTensor(0)));
    }

    [Fact]
    public void Run_NullInitialState_Throws()
    {
        var runner = new ContinuationRunner(
            evaluateResidual: (s, l) => (MakeTensor(0), 0),
            solveCorrector: (s, l, t, m) => (s, 0, true));

        Assert.Throws<ArgumentNullException>(() => runner.Run(MakeSpec(0, 1, 10), null!));
    }

    [Fact]
    public void ContinuationRunner_NullEvaluator_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ContinuationRunner(null!, (s, l, t, m) => (s, 0, true)));
    }

    private static FieldTensor MakeTensor(double value)
    {
        return new FieldTensor
        {
            Label = "test",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "test",
                CarrierType = "scalar",
                Degree = "0",
                LieAlgebraBasisId = "trivial",
                ComponentOrderId = "scalar",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new[] { value },
            Shape = new[] { 1 },
        };
    }

    private static ContinuationSpec MakeSpec(
        double lambdaStart, double lambdaEnd, int maxSteps,
        double stepSize = 0.1, double minStepSize = 0.001, double maxStepSize = 0.5)
    {
        return new ContinuationSpec
        {
            ParameterName = "test-param",
            LambdaStart = lambdaStart,
            LambdaEnd = lambdaEnd,
            InitialStepSize = stepSize,
            MaxSteps = maxSteps,
            MinStepSize = minStepSize,
            MaxStepSize = maxStepSize,
            CorrectorTolerance = 1e-8,
            MaxCorrectorIterations = 10,
            BranchManifestId = "test-branch",
        };
    }
}

public class ContinuationEventTests
{
    [Fact]
    public void ContinuationEventKind_HasSixValues()
    {
        Assert.Equal(6, Enum.GetValues<ContinuationEventKind>().Length);
    }

    [Fact]
    public void CanConstructEvent()
    {
        var ev = new ContinuationEvent
        {
            Kind = ContinuationEventKind.HessianSignChange,
            Lambda = 0.5,
            Description = "Sign change",
            Severity = "critical",
        };
        Assert.Equal(ContinuationEventKind.HessianSignChange, ev.Kind);
    }
}

public class StabilityAtlasTests
{
    [Fact]
    public void CanConstructEmptyAtlas()
    {
        var atlas = new StabilityAtlas
        {
            AtlasId = "atlas-1",
            BranchManifestId = "branch-1",
            FamilyDescription = "Test family",
            Paths = Array.Empty<ContinuationResult>(),
            HessianRecords = Array.Empty<Gu.Phase2.Stability.HessianRecord>(),
            SymbolSamples = Array.Empty<Gu.Phase2.Stability.PrincipalSymbolRecord>(),
            LinearizationRecords = Array.Empty<Gu.Phase2.Stability.LinearizationRecord>(),
            BifurcationIndicators = Array.Empty<ContinuationEvent>(),
            Timestamp = DateTimeOffset.UtcNow,
        };

        Assert.Equal("atlas-1", atlas.AtlasId);
        Assert.Empty(atlas.Paths);
    }

    [Fact]
    public void CanConstructAtlasWithPath()
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

        var path = new ContinuationResult
        {
            Spec = new ContinuationSpec
            {
                ParameterName = "lambda",
                LambdaStart = 0,
                LambdaEnd = 1,
                InitialStepSize = 0.1,
                MaxSteps = 10,
                MinStepSize = 0.01,
                MaxStepSize = 0.5,
                CorrectorTolerance = 1e-8,
                MaxCorrectorIterations = 10,
                BranchManifestId = "branch-1",
            },
            Path = new[] { step },
            TerminationReason = "reached-end",
            TotalSteps = 1,
            TotalArclength = 0.0,
            LambdaMin = 0.0,
            LambdaMax = 0.0,
            AllEvents = Array.Empty<ContinuationEvent>(),
            Timestamp = DateTimeOffset.UtcNow,
        };

        var atlas = new StabilityAtlas
        {
            AtlasId = "atlas-2",
            BranchManifestId = "branch-1",
            FamilyDescription = "Single path test",
            Paths = new[] { path },
            HessianRecords = Array.Empty<Gu.Phase2.Stability.HessianRecord>(),
            SymbolSamples = Array.Empty<Gu.Phase2.Stability.PrincipalSymbolRecord>(),
            LinearizationRecords = Array.Empty<Gu.Phase2.Stability.LinearizationRecord>(),
            BifurcationIndicators = Array.Empty<ContinuationEvent>(),
            Timestamp = DateTimeOffset.UtcNow,
        };

        Assert.Single(atlas.Paths);
        Assert.Equal("reached-end", atlas.Paths[0].TerminationReason);
    }
}

public class StabilityAtlasBuilderTests
{
    [Fact]
    public void Build_EmptyAtlas_HasCorrectMetadata()
    {
        var builder = new StabilityAtlasBuilder("atlas-1", "branch-1", "Test family");
        var atlas = builder.Build();

        Assert.Equal("atlas-1", atlas.AtlasId);
        Assert.Equal("branch-1", atlas.BranchManifestId);
        Assert.Equal("Test family", atlas.FamilyDescription);
        Assert.Empty(atlas.Paths);
        Assert.Empty(atlas.HessianRecords);
        Assert.Empty(atlas.BifurcationIndicators);
    }

    [Fact]
    public void Build_WithPath_IncludesPath()
    {
        var path = MakeContinuationResult("reached-end");
        var builder = new StabilityAtlasBuilder("atlas-1", "branch-1", "Test family")
            .AddPath(path);

        var atlas = builder.Build();

        Assert.Single(atlas.Paths);
        Assert.Equal("reached-end", atlas.Paths[0].TerminationReason);
    }

    [Fact]
    public void Build_WithBifurcationEvents_ExtractsBifurcationIndicators()
    {
        var bifurcationEvent = new ContinuationEvent
        {
            Kind = ContinuationEventKind.HessianSignChange,
            Lambda = 0.5,
            Description = "Sign change at 0.5",
            Severity = "critical",
        };
        var nonBifurcationEvent = new ContinuationEvent
        {
            Kind = ContinuationEventKind.StepRejectionBurst,
            Lambda = 0.3,
            Description = "Rejections",
            Severity = "warning",
        };

        var step = new ContinuationStep
        {
            StepIndex = 0,
            Lambda = 0.5,
            Arclength = 0.5,
            StepSize = 0.1,
            ResidualNorm = 0.0,
            CorrectorIterations = 1,
            CorrectorConverged = true,
            Events = new[] { bifurcationEvent, nonBifurcationEvent },
        };

        var path = new ContinuationResult
        {
            Spec = MakeSpec(),
            Path = new[] { step },
            TerminationReason = "reached-end",
            TotalSteps = 1,
            TotalArclength = 0.5,
            LambdaMin = 0.0,
            LambdaMax = 0.5,
            AllEvents = new[] { bifurcationEvent, nonBifurcationEvent },
            Timestamp = DateTimeOffset.UtcNow,
        };

        var builder = new StabilityAtlasBuilder("atlas-1", "branch-1", "Test")
            .AddPath(path);
        var atlas = builder.Build();

        // Only bifurcation-relevant events should be in bifurcationIndicators
        Assert.Single(atlas.BifurcationIndicators);
        Assert.Equal(ContinuationEventKind.HessianSignChange, atlas.BifurcationIndicators[0].Kind);
    }

    [Fact]
    public void Build_WithHessianAndLinearization_IncludesThem()
    {
        var hessian = new Gu.Phase2.Stability.HessianRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 1.0,
            Dimension = 6,
            AssemblyMode = "matrix-free",
            SymmetryVerified = true,
            SymmetryError = 1e-12,
        };

        var linearization = new Gu.Phase2.Stability.LinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            OperatorDefinitionId = "J",
            DerivativeMode = "analytic",
            InputDimension = 6,
            OutputDimension = 4,
            GaugeHandlingMode = "raw",
            AssemblyMode = "matrix-free",
            ValidationStatus = "verified",
        };

        var builder = new StabilityAtlasBuilder("atlas-1", "branch-1", "Test")
            .AddHessianRecord(hessian)
            .AddLinearizationRecord(linearization)
            .WithDiscretizationNotes("Test mesh 4 cells")
            .WithTheoremStatusNotes("Numerical only");

        var atlas = builder.Build();

        Assert.Single(atlas.HessianRecords);
        Assert.Single(atlas.LinearizationRecords);
        Assert.Equal("Test mesh 4 cells", atlas.DiscretizationNotes);
        Assert.Equal("Numerical only", atlas.TheoremStatusNotes);
    }

    [Fact]
    public void Build_MultiplePaths_AllIncluded()
    {
        var path1 = MakeContinuationResult("reached-end");
        var path2 = MakeContinuationResult("max-steps");

        var builder = new StabilityAtlasBuilder("atlas-1", "branch-1", "Multi-path")
            .AddPath(path1)
            .AddPath(path2);

        var atlas = builder.Build();

        Assert.Equal(2, atlas.Paths.Count);
    }

    private static ContinuationResult MakeContinuationResult(string terminationReason)
    {
        return new ContinuationResult
        {
            Spec = MakeSpec(),
            Path = Array.Empty<ContinuationStep>(),
            TerminationReason = terminationReason,
            TotalSteps = 0,
            TotalArclength = 0,
            LambdaMin = 0,
            LambdaMax = 1,
            AllEvents = Array.Empty<ContinuationEvent>(),
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private static ContinuationSpec MakeSpec()
    {
        return new ContinuationSpec
        {
            ParameterName = "test",
            LambdaStart = 0,
            LambdaEnd = 1,
            InitialStepSize = 0.1,
            MaxSteps = 10,
            MinStepSize = 0.01,
            MaxStepSize = 0.5,
            CorrectorTolerance = 1e-8,
            MaxCorrectorIterations = 10,
            BranchManifestId = "branch-1",
        };
    }
}
