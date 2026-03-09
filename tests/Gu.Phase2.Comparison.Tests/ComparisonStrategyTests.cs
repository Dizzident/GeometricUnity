using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison.Tests;

public sealed class ComparisonStrategyTests
{
    private static ComparisonAsset MakeAsset(
        string id = "asset-001",
        Dictionary<string, string>? variables = null) => new()
    {
        AssetId = id,
        SourceCitation = "Test citation",
        AcquisitionDate = DateTimeOffset.UtcNow,
        PreprocessingDescription = "None",
        AdmissibleUseStatement = "Any",
        DomainOfValidity = "All",
        UncertaintyModel = new UncertaintyRecord
        {
            Discretization = 0.01,
            Solver = 0.02,
            Branch = 0.03,
            Extraction = 0.04,
            Calibration = 0.05,
            DataAsset = 0.06,
        },
        ComparisonVariables = variables ?? new Dictionary<string, string>
        {
            ["energy"] = "Total energy",
        },
    };

    private static PredictionTestRecord MakeRecord(
        ClaimClass claimClass = ClaimClass.ExactStructuralConsequence,
        string numericalStatus = "converged") => new()
    {
        TestId = "test-001",
        ClaimClass = claimClass,
        FormalSource = "Theorem 3.1",
        BranchManifestId = "manifest-001",
        ObservableMapId = "obs-map-001",
        TheoremDependencyStatus = "closed",
        NumericalDependencyStatus = numericalStatus,
        ApproximationStatus = "exact",
        Falsifier = "Observable diverges by >5 sigma",
        ArtifactLinks = ["artifact-001"],
    };

    private static InMemoryDatasetAdapter MakeAdapter(
        ComparisonAsset asset,
        Dictionary<string, double[]> data)
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(asset, data);
        return adapter;
    }

    // --- StructuralComparisonStrategy ---

    [Fact]
    public void Structural_AllVariablesPresent_Passes()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.0, 2.0, 3.0],
        });

        var strategy = new StructuralComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.True(score.Passed);
        Assert.Equal(1.0, score.Score);
        Assert.Contains("passed", score.Summary);
    }

    [Fact]
    public void Structural_MissingVariable_Fails()
    {
        var asset = MakeAsset(variables: new()
        {
            ["energy"] = "Total energy",
            ["momentum"] = "Total momentum",
        });
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.0],
            // momentum missing
        });

        var strategy = new StructuralComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
        Assert.Equal(0.5, score.Score);
        Assert.Contains("momentum", score.Summary);
    }

    [Fact]
    public void Structural_EmptyVariableData_Fails()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = Array.Empty<double>(),
        });

        var strategy = new StructuralComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
    }

    [Fact]
    public void Structural_NoVariables_Fails()
    {
        var asset = MakeAsset(variables: new Dictionary<string, string>());
        var adapter = MakeAdapter(asset, new());

        var strategy = new StructuralComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
        Assert.Equal(0.0, score.Score);
    }

    [Fact]
    public void Structural_PreservesClaimClass()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new() { ["energy"] = [1.0] });

        var strategy = new StructuralComparisonStrategy();
        var score = strategy.Execute(
            MakeRecord(ClaimClass.ApproximateStructuralSurrogate), asset, adapter);

        Assert.Equal(ClaimClass.ApproximateStructuralSurrogate, score.ResolvedClaimClass);
    }

    // --- SemiQuantitativeComparisonStrategy ---

    [Fact]
    public void SemiQuantitative_CloseToExpected_Passes()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.0, 1.01, 0.99], // close to 1.0
        });

        var strategy = new SemiQuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.True(score.Passed);
        Assert.True(score.Score > 0.9);
        Assert.Contains("passed", score.Summary);
    }

    [Fact]
    public void SemiQuantitative_FarFromExpected_Fails()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [5.0, 6.0, 7.0], // far from 1.0
        });

        var strategy = new SemiQuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
        Assert.Contains("failed", score.Summary);
    }

    [Fact]
    public void SemiQuantitative_CustomTolerance()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.4], // deviation = 0.4 from 1.0
        });

        // Default tolerance (0.25) should fail
        var strict = new SemiQuantitativeComparisonStrategy(0.25);
        Assert.False(strict.Execute(MakeRecord(), asset, adapter).Passed);

        // Generous tolerance should pass
        var generous = new SemiQuantitativeComparisonStrategy(0.5);
        Assert.True(generous.Execute(MakeRecord(), asset, adapter).Passed);
    }

    [Fact]
    public void SemiQuantitative_NoData_Fails()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new());

        var strategy = new SemiQuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
        Assert.Equal(0.0, score.Score);
    }

    [Fact]
    public void SemiQuantitative_DemotesHighClaimOnFailure()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [10.0], // way off
        });

        var strategy = new SemiQuantitativeComparisonStrategy();
        var score = strategy.Execute(
            MakeRecord(ClaimClass.ExactStructuralConsequence), asset, adapter);

        Assert.False(score.Passed);
        Assert.True(score.ResolvedClaimClass >= ClaimClass.PostdictionTarget);
    }

    [Fact]
    public void SemiQuantitative_DoesNotPromoteLowClaim()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [10.0],
        });

        var strategy = new SemiQuantitativeComparisonStrategy();
        var score = strategy.Execute(
            MakeRecord(ClaimClass.SpeculativeInterpretation), asset, adapter);

        // Should not promote to PostdictionTarget (SpeculativeInterpretation > PostdictionTarget)
        Assert.Equal(ClaimClass.SpeculativeInterpretation, score.ResolvedClaimClass);
    }

    // --- QuantitativeComparisonStrategy ---

    [Fact]
    public void Quantitative_PerfectMatch_Passes()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.0, 1.0, 1.0], // perfect match
        });

        var strategy = new QuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.True(score.Passed);
        Assert.Equal(1.0, score.Score, 5); // perfect score
        Assert.Contains("passed", score.Summary);
    }

    [Fact]
    public void Quantitative_LargeDeviation_Fails()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [100.0, 200.0], // far from 1.0
        });

        var strategy = new QuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
        Assert.True(score.Score < 0.5);
        Assert.Contains("failed", score.Summary);
    }

    [Fact]
    public void Quantitative_ZeroUncertainty_Fails()
    {
        var asset = new ComparisonAsset
        {
            AssetId = "zero-uncertainty",
            SourceCitation = "Test",
            AcquisitionDate = DateTimeOffset.UtcNow,
            PreprocessingDescription = "None",
            AdmissibleUseStatement = "Any",
            DomainOfValidity = "All",
            UncertaintyModel = UncertaintyRecord.Unestimated(), // all -1
            ComparisonVariables = new Dictionary<string, string> { ["energy"] = "Energy" },
        };
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.0],
        });

        var strategy = new QuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
        Assert.Contains("zero or unestimated", score.Summary);
    }

    [Fact]
    public void Quantitative_CustomSigmaThreshold()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.5], // moderate deviation
        });

        // Very strict threshold
        var strict = new QuantitativeComparisonStrategy(0.001);
        Assert.False(strict.Execute(MakeRecord(), asset, adapter).Passed);

        // Very generous threshold
        var generous = new QuantitativeComparisonStrategy(1000.0);
        Assert.True(generous.Execute(MakeRecord(), asset, adapter).Passed);
    }

    [Fact]
    public void Quantitative_NoData_Fails()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new());

        var strategy = new QuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.False(score.Passed);
    }

    [Fact]
    public void Quantitative_PropagatesUncertainty()
    {
        var asset = MakeAsset();
        var adapter = MakeAdapter(asset, new()
        {
            ["energy"] = [1.0],
        });

        var strategy = new QuantitativeComparisonStrategy();
        var score = strategy.Execute(MakeRecord(), asset, adapter);

        Assert.Equal(0.01, score.Uncertainty.Discretization);
        Assert.Equal(0.06, score.Uncertainty.DataAsset);
    }

    // --- ComparisonStrategyFactory ---

    [Theory]
    [InlineData(ComparisonMode.Structural, typeof(StructuralComparisonStrategy))]
    [InlineData(ComparisonMode.SemiQuantitative, typeof(SemiQuantitativeComparisonStrategy))]
    [InlineData(ComparisonMode.Quantitative, typeof(QuantitativeComparisonStrategy))]
    public void Factory_CreatesCorrectStrategy(ComparisonMode mode, Type expectedType)
    {
        var strategy = ComparisonStrategyFactory.Create(mode);
        Assert.IsType(expectedType, strategy);
    }

    // --- Null checks ---

    [Fact]
    public void Structural_NullPrediction_Throws()
    {
        var strategy = new StructuralComparisonStrategy();
        Assert.Throws<ArgumentNullException>(() =>
            strategy.Execute(null!, MakeAsset(), new InMemoryDatasetAdapter()));
    }

    [Fact]
    public void SemiQuantitative_NullAsset_Throws()
    {
        var strategy = new SemiQuantitativeComparisonStrategy();
        Assert.Throws<ArgumentNullException>(() =>
            strategy.Execute(MakeRecord(), null!, new InMemoryDatasetAdapter()));
    }

    [Fact]
    public void Quantitative_NullAdapter_Throws()
    {
        var strategy = new QuantitativeComparisonStrategy();
        Assert.Throws<ArgumentNullException>(() =>
            strategy.Execute(MakeRecord(), MakeAsset(), null!));
    }
}
