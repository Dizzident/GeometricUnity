namespace Gu.Phase3.Properties.Tests;

public class BosonPropertyVectorTests
{
    private static BosonPropertyVector MakeVector(
        string modeId, double mass, double gaugeLeak, int mult,
        string dominantClass = "mixed",
        Dictionary<string, double>? blockFractions = null)
    {
        blockFractions ??= new Dictionary<string, double> { ["connection"] = 1.0 };
        return new BosonPropertyVector
        {
            ModeId = modeId,
            BackgroundId = "bg-1",
            MassLikeScale = new MassLikeScaleRecord
            {
                ModeId = modeId,
                Eigenvalue = mass * mass,
                MassLikeScale = mass,
                ExtractionMethod = "eigenvalue",
                OperatorType = "GaussNewton",
                BackgroundId = "bg-1",
            },
            Polarization = new PolarizationDescriptor
            {
                ModeId = modeId,
                BlockEnergyFractions = blockFractions,
                DominantClass = dominantClass,
                DominanceFraction = blockFractions.Values.Max(),
                BackgroundId = "bg-1",
            },
            Symmetry = new SymmetryDescriptor
            {
                ModeId = modeId,
                BackgroundId = "bg-1",
            },
            GaugeLeakScore = gaugeLeak,
            Multiplicity = mult,
        };
    }

    [Fact]
    public void Distance_IdenticalVectors_IsZero()
    {
        var a = MakeVector("m-1", 2.0, 0.01, 1);
        var b = MakeVector("m-2", 2.0, 0.01, 1);

        double dist = BosonPropertyVector.Distance(a, b);
        Assert.Equal(0.0, dist, 10);
    }

    [Fact]
    public void Distance_DifferentMass_IsPositive()
    {
        var a = MakeVector("m-1", 1.0, 0.01, 1);
        var b = MakeVector("m-2", 2.0, 0.01, 1);

        double dist = BosonPropertyVector.Distance(a, b);
        Assert.True(dist > 0);
    }

    [Fact]
    public void Distance_DifferentLeak_IsPositive()
    {
        var a = MakeVector("m-1", 1.0, 0.01, 1);
        var b = MakeVector("m-2", 1.0, 0.99, 1);

        double dist = BosonPropertyVector.Distance(a, b);
        Assert.True(dist > 0);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        var a = MakeVector("m-1", 1.0, 0.01, 1);
        var b = MakeVector("m-2", 3.0, 0.5, 2);

        double dAB = BosonPropertyVector.Distance(a, b);
        double dBA = BosonPropertyVector.Distance(b, a);
        Assert.Equal(dAB, dBA, 12);
    }

    [Fact]
    public void Distance_DifferentPolarization_IsPositive()
    {
        var a = MakeVector("m-1", 1.0, 0.01, 1,
            blockFractions: new Dictionary<string, double> { ["conn"] = 0.8, ["aux"] = 0.2 });
        var b = MakeVector("m-2", 1.0, 0.01, 1,
            blockFractions: new Dictionary<string, double> { ["conn"] = 0.2, ["aux"] = 0.8 });

        double dist = BosonPropertyVector.Distance(a, b);
        Assert.True(dist > 0);
    }

    [Fact]
    public void Distance_NullA_Throws()
    {
        var b = MakeVector("m-2", 1.0, 0.01, 1);
        Assert.Throws<ArgumentNullException>(() =>
            BosonPropertyVector.Distance(null!, b));
    }
}
