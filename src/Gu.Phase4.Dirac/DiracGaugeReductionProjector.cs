using Gu.Core;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Applies an already-materialized fermion-space projector to an explicit Dirac operator.
/// The projector is stored as a real dense totalDof x totalDof matrix and acts on the
/// real and imaginary components of each complex fermion coefficient identically.
/// </summary>
public sealed class DiracGaugeReductionProjector
{
    private const double DefaultTolerance = 1e-10;

    public DiracOperatorBundle Project(
        DiracOperatorBundle source,
        double[] projectorMatrix,
        string projectorId,
        ProvenanceMeta provenance,
        double tolerance = DefaultTolerance)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(projectorMatrix);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectorId);
        ArgumentNullException.ThrowIfNull(provenance);

        if (!source.HasExplicitMatrix || source.ExplicitMatrix is null)
            throw new InvalidOperationException("Gauge projection requires an explicit Dirac matrix.");

        int n = source.TotalDof;
        if (projectorMatrix.Length != n * n)
            throw new ArgumentException(
                $"Projector length {projectorMatrix.Length} does not match expected {n * n}.",
                nameof(projectorMatrix));

        double symmetryResidual = ComputeSymmetryResidual(projectorMatrix, n);
        if (symmetryResidual > tolerance)
            throw new ArgumentException(
                $"Projector must be symmetric within tolerance {tolerance:R}; residual={symmetryResidual:R}.",
                nameof(projectorMatrix));

        double idempotenceResidual = ComputeIdempotenceResidual(projectorMatrix, n);
        if (idempotenceResidual > tolerance)
            throw new ArgumentException(
                $"Projector must be idempotent within tolerance {tolerance:R}; residual={idempotenceResidual:R}.",
                nameof(projectorMatrix));

        var projected = MultiplyProjectorSandwich(source.ExplicitMatrix, projectorMatrix, n);
        double hermiticityResidual = ComputeHermiticityResidual(projected, n);

        var notes = new List<string>(source.DiagnosticNotes ?? Enumerable.Empty<string>())
        {
            $"Applied fermion-space gauge projector {projectorId}.",
            $"Projector symmetry residual={symmetryResidual:R}.",
            $"Projector idempotence residual={idempotenceResidual:R}.",
        };

        return new DiracOperatorBundle
        {
            OperatorId = $"{source.OperatorId}-gauge-reduced-{projectorId}",
            FermionBackgroundId = source.FermionBackgroundId,
            LayoutId = source.LayoutId,
            SpinConnectionId = source.SpinConnectionId,
            MatrixShape = new[] { n, n },
            HasExplicitMatrix = true,
            ExplicitMatrix = projected,
            ExplicitMatrixRef = null,
            IsHermitian = hermiticityResidual <= source.HermiticityTolerance,
            HermiticityResidual = hermiticityResidual,
            HermiticityTolerance = source.HermiticityTolerance,
            MassBranchTermIncluded = source.MassBranchTermIncluded,
            CorrectionTermIncluded = source.CorrectionTermIncluded,
            GaugeReductionApplied = true,
            CellCount = source.CellCount,
            DofsPerCell = source.DofsPerCell,
            DiagnosticNotes = notes,
            Provenance = provenance,
        };
    }

    public static double[] IdentityProjector(int dimension)
    {
        if (dimension <= 0)
            throw new ArgumentOutOfRangeException(nameof(dimension), "Dimension must be positive.");

        var projector = new double[dimension * dimension];
        for (int i = 0; i < dimension; i++)
            projector[i * dimension + i] = 1.0;
        return projector;
    }

    private static double[] MultiplyProjectorSandwich(double[] matrix, double[] projector, int n)
    {
        var temp = new double[n * n * 2];
        var output = new double[n * n * 2];

        // temp = D * P. P is real, D is complex.
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
            {
                double re = 0.0;
                double im = 0.0;
                for (int k = 0; k < n; k++)
                {
                    double p = projector[k * n + col];
                    re += matrix[(row * n + k) * 2] * p;
                    im += matrix[(row * n + k) * 2 + 1] * p;
                }

                temp[(row * n + col) * 2] = re;
                temp[(row * n + col) * 2 + 1] = im;
            }

        // output = P * temp.
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
            {
                double re = 0.0;
                double im = 0.0;
                for (int k = 0; k < n; k++)
                {
                    double p = projector[row * n + k];
                    re += p * temp[(k * n + col) * 2];
                    im += p * temp[(k * n + col) * 2 + 1];
                }

                output[(row * n + col) * 2] = re;
                output[(row * n + col) * 2 + 1] = im;
            }

        return output;
    }

    private static double ComputeSymmetryResidual(double[] matrix, int n)
    {
        double diff = 0.0;
        double norm = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
            {
                double value = matrix[row * n + col];
                double delta = value - matrix[col * n + row];
                diff += delta * delta;
                norm += value * value;
            }

        return norm > 1e-30 ? System.Math.Sqrt(diff / norm) : System.Math.Sqrt(diff);
    }

    private static double ComputeIdempotenceResidual(double[] matrix, int n)
    {
        double diff = 0.0;
        double norm = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
            {
                double product = 0.0;
                for (int k = 0; k < n; k++)
                    product += matrix[row * n + k] * matrix[k * n + col];

                double value = matrix[row * n + col];
                double delta = product - value;
                diff += delta * delta;
                norm += value * value;
            }

        return norm > 1e-30 ? System.Math.Sqrt(diff / norm) : System.Math.Sqrt(diff);
    }

    private static double ComputeHermiticityResidual(double[] matrix, int n)
    {
        double diff = 0.0;
        double norm = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
            {
                double re = matrix[(row * n + col) * 2];
                double im = matrix[(row * n + col) * 2 + 1];
                double adjRe = matrix[(col * n + row) * 2];
                double adjIm = -matrix[(col * n + row) * 2 + 1];
                double dRe = re - adjRe;
                double dIm = im - adjIm;
                diff += dRe * dRe + dIm * dIm;
                norm += re * re + im * im;
            }

        return norm > 1e-30 ? System.Math.Sqrt(diff / norm) : System.Math.Sqrt(diff);
    }
}
