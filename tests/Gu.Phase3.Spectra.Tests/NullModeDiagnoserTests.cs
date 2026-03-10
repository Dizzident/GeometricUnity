namespace Gu.Phase3.Spectra.Tests;

public class NullModeDiagnoserTests
{
    private static ModeRecord MakeMode(int index, double eigenvalue, double gaugeLeakScore, double residualNorm = 1e-12)
    {
        var v = new double[9];
        v[index % 9] = 1.0;
        return new ModeRecord
        {
            ModeId = $"mode-{index}",
            BackgroundId = "test-bg",
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = eigenvalue,
            ResidualNorm = residualNorm,
            NormalizationConvention = "unit-L2-norm",
            GaugeLeakScore = gaugeLeakScore,
            ModeVector = v,
            ModeIndex = index,
        };
    }

    [Fact]
    public void Diagnose_NoNullModes_ReturnsNull()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-8);
        var modes = new[] { MakeMode(0, 1.0, 0.01), MakeMode(1, 2.5, 0.02) };
        Assert.Null(diagnoser.Diagnose(modes));
    }

    [Fact]
    public void Diagnose_GaugeNullMode_Classified()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        var modes = new[]
        {
            MakeMode(0, 1e-10, 0.98), // gauge artifact
            MakeMode(1, 1.5, 0.01),   // physical
        };

        var diagnosis = diagnoser.Diagnose(modes);
        Assert.NotNull(diagnosis);
        Assert.Equal(1, diagnosis!.NullModeCount);
        Assert.Equal(1, diagnosis.GaugeNullCount);
        Assert.Equal(0, diagnosis.PhysicalNullCount);
    }

    [Fact]
    public void Diagnose_PhysicalNullMode_Classified()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        var modes = new[]
        {
            MakeMode(0, 1e-10, 0.01), // physical zero mode (Goldstone)
            MakeMode(1, 1.5, 0.01),
        };

        var diagnosis = diagnoser.Diagnose(modes);
        Assert.NotNull(diagnosis);
        Assert.Equal(1, diagnosis!.NullModeCount);
        Assert.Equal(0, diagnosis.GaugeNullCount);
        Assert.Equal(1, diagnosis.PhysicalNullCount);
    }

    [Fact]
    public void Diagnose_MixedNullModes()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-4, gaugeLeakThreshold: 0.8);
        var modes = new[]
        {
            MakeMode(0, 1e-10, 0.95), // gauge
            MakeMode(1, 5e-6, 0.05),  // physical
            MakeMode(2, 1e-5, 0.85),  // gauge
            MakeMode(3, 2.0, 0.01),   // not null
        };

        var diagnosis = diagnoser.Diagnose(modes);
        Assert.NotNull(diagnosis);
        Assert.Equal(3, diagnosis!.NullModeCount);
        Assert.Equal(2, diagnosis.GaugeNullCount);
        Assert.Equal(1, diagnosis.PhysicalNullCount);
        // Eigenvalues sorted by |lambda|
        Assert.True(System.Math.Abs(diagnosis.NullEigenvalues[0]) <= System.Math.Abs(diagnosis.NullEigenvalues[1]));
    }

    [Fact]
    public void Classify_SingleMode_GaugeArtifact()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        var mode = MakeMode(0, 1e-10, 0.95);
        Assert.Equal(NullModeClassification.GaugeArtifact, diagnoser.Classify(mode));
    }

    [Fact]
    public void Classify_SingleMode_ExactSymmetry()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        var mode = MakeMode(0, 1e-10, 0.01);
        Assert.Equal(NullModeClassification.ExactSymmetry, diagnoser.Classify(mode));
    }

    [Fact]
    public void Classify_SingleMode_NotNull()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6);
        var mode = MakeMode(0, 1.0, 0.01);
        Assert.Equal(NullModeClassification.NotNull, diagnoser.Classify(mode));
    }

    [Fact]
    public void Classify_SingleMode_DiscretizationArtifact()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        // High residual, moderate gauge leak → discretization artifact
        var mode = MakeMode(0, 1e-10, 0.5, residualNorm: 1e-3);
        Assert.Equal(NullModeClassification.DiscretizationArtifact, diagnoser.Classify(mode));
    }

    [Fact]
    public void Classify_SingleMode_Unresolved()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        // Moderate gauge leak, low residual → unresolved
        var mode = MakeMode(0, 1e-10, 0.5, residualNorm: 1e-12);
        Assert.Equal(NullModeClassification.Unresolved, diagnoser.Classify(mode));
    }
}
