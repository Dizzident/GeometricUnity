using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Stability.Tests;

public class PrincipalSymbolSamplerTests
{
    [Fact]
    public void Sample_IdentityOperator_EllipticLike()
    {
        var op = new ScaledIdentityOperator(3, 2.0);
        var sampler = new PrincipalSymbolSampler();

        var record = sampler.Sample(
            op,
            cellIndex: 0,
            covector: new[] { 1.0, 0.0, 0.0 },
            localDim: 3,
            branchManifestId: "test-branch",
            gaugeStudyMode: GaugeStudyMode.GaugeFree,
            operatorId: "J");

        Assert.Equal(0, record.CellIndex);
        Assert.Equal(PdeClassification.EllipticLike, record.Classification);
        Assert.Equal("positive-definite", record.DefinitenessIndicator);
        Assert.Equal(0, record.RankDeficiency);
        Assert.True(record.IsSymmetric);
        Assert.Equal(3, record.Eigenvalues.Length);
    }

    [Fact]
    public void Sample_ZeroOperator_Degenerate()
    {
        var op = new ScaledIdentityOperator(3, 0.0);
        var sampler = new PrincipalSymbolSampler();

        var record = sampler.Sample(
            op, cellIndex: 0,
            covector: new[] { 1.0 },
            localDim: 3,
            branchManifestId: "test-branch",
            gaugeStudyMode: GaugeStudyMode.GaugeFree,
            operatorId: "J");

        Assert.Equal(PdeClassification.Degenerate, record.Classification);
        Assert.Equal("zero", record.DefinitenessIndicator);
        Assert.Equal(3, record.RankDeficiency);
    }

    [Fact]
    public void Sample_IndefiniteOperator_HyperbolicLike()
    {
        var op = new DiagonalOperator(new[] { 1.0, -1.0, 1.0 });
        var sampler = new PrincipalSymbolSampler();

        var record = sampler.Sample(
            op, cellIndex: 0,
            covector: new[] { 1.0 },
            localDim: 3,
            branchManifestId: "test-branch",
            gaugeStudyMode: GaugeStudyMode.GaugeFixed,
            operatorId: "H");

        Assert.Equal(PdeClassification.HyperbolicLike, record.Classification);
        Assert.Equal("indefinite", record.DefinitenessIndicator);
        Assert.Equal(0, record.RankDeficiency);
    }

    [Fact]
    public void Sample_SingularOperator_Degenerate()
    {
        var op = new DiagonalOperator(new[] { 1.0, 0.0, 1.0 });
        var sampler = new PrincipalSymbolSampler();

        var record = sampler.Sample(
            op, cellIndex: 0,
            covector: new[] { 1.0 },
            localDim: 3,
            branchManifestId: "test-branch",
            gaugeStudyMode: GaugeStudyMode.GaugeFree,
            operatorId: "L_tilde");

        Assert.Equal(PdeClassification.Degenerate, record.Classification);
        Assert.Equal(1, record.RankDeficiency);
    }

    [Fact]
    public void Sample_NegativeDefinite_EllipticLike()
    {
        var op = new ScaledIdentityOperator(2, -3.0);
        var sampler = new PrincipalSymbolSampler();

        var record = sampler.Sample(
            op, cellIndex: 0,
            covector: new[] { 1.0 },
            localDim: 2,
            branchManifestId: "test-branch",
            gaugeStudyMode: GaugeStudyMode.GaugeFree,
            operatorId: "J");

        // Negative definite is still "elliptic-like" (all same sign)
        Assert.Equal(PdeClassification.EllipticLike, record.Classification);
        Assert.Equal("negative-definite", record.DefinitenessIndicator);
    }

    [Fact]
    public void Sample_PreservesMetadata()
    {
        var op = new ScaledIdentityOperator(2, 1.0);
        var sampler = new PrincipalSymbolSampler();
        var covector = new[] { 0.5, 0.5 };

        var record = sampler.Sample(
            op, cellIndex: 3,
            covector: covector,
            localDim: 2,
            branchManifestId: "branch-42",
            gaugeStudyMode: GaugeStudyMode.GaugeFixed,
            operatorId: "H");

        Assert.Equal(3, record.CellIndex);
        Assert.Equal(covector, record.Covector);
        Assert.Equal("branch-42", record.BranchManifestId);
        Assert.Equal(GaugeStudyMode.GaugeFixed, record.GaugeStudyMode);
        Assert.Equal("H", record.OperatorId);
    }

    [Fact]
    public void Sample_NullOperator_Throws()
    {
        var sampler = new PrincipalSymbolSampler();
        Assert.Throws<ArgumentNullException>(() =>
            sampler.Sample(null!, 0, new[] { 1.0 }, 1, "b", GaugeStudyMode.GaugeFree, "J"));
    }

    [Fact]
    public void Sample_EmptyBranchId_Throws()
    {
        var op = new ScaledIdentityOperator(1, 1.0);
        var sampler = new PrincipalSymbolSampler();
        Assert.Throws<ArgumentException>(() =>
            sampler.Sample(op, 0, new[] { 1.0 }, 1, "", GaugeStudyMode.GaugeFree, "J"));
    }

    [Fact]
    public void Sample_ZeroLocalDim_Throws()
    {
        var op = new ScaledIdentityOperator(1, 1.0);
        var sampler = new PrincipalSymbolSampler();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sampler.Sample(op, 0, new[] { 1.0 }, 0, "b", GaugeStudyMode.GaugeFree, "J"));
    }
}

public class SymbolStudyReportTests
{
    [Fact]
    public void FromSamples_UnanimousElliptic_ReportsElliptic()
    {
        var samples = new[]
        {
            MakeSample(PdeClassification.EllipticLike),
            MakeSample(PdeClassification.EllipticLike),
            MakeSample(PdeClassification.EllipticLike),
        };

        var report = SymbolStudyReport.FromSamples(
            "study-1", "branch-1", "bg-1", "J", GaugeStudyMode.GaugeFree, samples);

        Assert.Equal(PdeClassification.EllipticLike, report.SummaryClassification);
        Assert.Equal(3, report.EllipticCount);
        Assert.Equal(0, report.HyperbolicCount);
        Assert.Equal(3, report.TotalSamples);
    }

    [Fact]
    public void FromSamples_MixedClassifications_ReportsMixed()
    {
        var samples = new[]
        {
            MakeSample(PdeClassification.EllipticLike),
            MakeSample(PdeClassification.HyperbolicLike),
        };

        var report = SymbolStudyReport.FromSamples(
            "study-2", "branch-1", "bg-1", "H", GaugeStudyMode.GaugeFixed, samples);

        Assert.Equal(PdeClassification.Mixed, report.SummaryClassification);
        Assert.Equal(1, report.EllipticCount);
        Assert.Equal(1, report.HyperbolicCount);
        Assert.Equal(2, report.TotalSamples);
    }

    [Fact]
    public void FromSamples_EmptySamples_Unresolved()
    {
        var report = SymbolStudyReport.FromSamples(
            "study-3", "branch-1", "bg-1", "J", GaugeStudyMode.GaugeFree,
            Array.Empty<PrincipalSymbolRecord>());

        Assert.Equal(PdeClassification.Unresolved, report.SummaryClassification);
        Assert.Equal(0, report.TotalSamples);
    }

    [Fact]
    public void FromSamples_PreservesMetadata()
    {
        var samples = new[] { MakeSample(PdeClassification.Degenerate) };

        var report = SymbolStudyReport.FromSamples(
            "study-4", "branch-99", "bg-5", "L_tilde", GaugeStudyMode.GaugeFixed, samples);

        Assert.Equal("study-4", report.StudyId);
        Assert.Equal("branch-99", report.BranchManifestId);
        Assert.Equal("bg-5", report.BackgroundStateId);
        Assert.Equal("L_tilde", report.OperatorId);
        Assert.Equal(GaugeStudyMode.GaugeFixed, report.GaugeStudyMode);
        Assert.Equal(1, report.DegenerateCount);
    }

    [Fact]
    public void FromSamples_AllDegenerate_ReportsDegenerate()
    {
        var samples = new[]
        {
            MakeSample(PdeClassification.Degenerate),
            MakeSample(PdeClassification.Degenerate),
        };

        var report = SymbolStudyReport.FromSamples(
            "study-5", "branch-1", "bg-1", "J", GaugeStudyMode.GaugeFree, samples);

        Assert.Equal(PdeClassification.Degenerate, report.SummaryClassification);
    }

    private static PrincipalSymbolRecord MakeSample(PdeClassification classification)
    {
        return new PrincipalSymbolRecord
        {
            CellIndex = 0,
            Covector = new[] { 1.0 },
            SymbolMatrix = new[] { new double[] { 0.0 } },
            Eigenvalues = new[] { 1.0 },
            IsSymmetric = true,
            SymmetryError = 0,
            DefinitenessIndicator = "positive-definite",
            RankDeficiency = 0,
            GaugeNullDimension = 0,
            Classification = classification,
            BranchManifestId = "test-branch",
            GaugeStudyMode = GaugeStudyMode.GaugeFree,
            OperatorId = "J",
        };
    }
}

public class PdeClassificationEnumTests
{
    [Fact]
    public void HasFiveValues()
    {
        Assert.Equal(5, Enum.GetValues<PdeClassification>().Length);
    }

    [Fact]
    public void GaugeStudyMode_HasTwoValues()
    {
        Assert.Equal(2, Enum.GetValues<GaugeStudyMode>().Length);
    }
}

/// <summary>
/// Simple scaled identity operator for testing: Apply(v) = scale * v.
/// </summary>
internal sealed class ScaledIdentityOperator : ILinearOperator
{
    private readonly int _dim;
    private readonly double _scale;

    public ScaledIdentityOperator(int dim, double scale)
    {
        _dim = dim;
        _scale = scale;
    }

    public TensorSignature InputSignature => new()
    {
        AmbientSpaceId = "test",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "face-major",
        MemoryLayout = "dense-row-major",
    };

    public TensorSignature OutputSignature => InputSignature;
    public int InputDimension => _dim;
    public int OutputDimension => _dim;

    public FieldTensor Apply(FieldTensor v)
    {
        var result = new double[_dim];
        for (int i = 0; i < _dim && i < v.Coefficients.Length; i++)
            result[i] = _scale * v.Coefficients[i];
        return new FieldTensor
        {
            Label = "scaled",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _dim },
        };
    }

    public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
}

/// <summary>
/// Diagonal operator with specified eigenvalues for testing.
/// </summary>
internal sealed class DiagonalOperator : ILinearOperator
{
    private readonly double[] _diag;

    public DiagonalOperator(double[] diagonal)
    {
        _diag = diagonal;
    }

    public TensorSignature InputSignature => new()
    {
        AmbientSpaceId = "test",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "face-major",
        MemoryLayout = "dense-row-major",
    };

    public TensorSignature OutputSignature => InputSignature;
    public int InputDimension => _diag.Length;
    public int OutputDimension => _diag.Length;

    public FieldTensor Apply(FieldTensor v)
    {
        var result = new double[_diag.Length];
        for (int i = 0; i < _diag.Length && i < v.Coefficients.Length; i++)
            result[i] = _diag[i] * v.Coefficients[i];
        return new FieldTensor
        {
            Label = "diag",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _diag.Length },
        };
    }

    public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
}
