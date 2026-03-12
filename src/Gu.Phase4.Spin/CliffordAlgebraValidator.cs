using System.Numerics;

namespace Gu.Phase4.Spin;

/// <summary>
/// Validates a GammaOperatorBundle against the Clifford algebra identities:
///   1. {Gamma_a, Gamma_b} = 2 * eta_{ab} * I
///   2. Gamma_chi^2 = I (for even dimensions)
///   3. {Gamma_chi, Gamma_mu} = 0 for all mu (for even dimensions)
///   4. Each Gamma_mu is Hermitian (for Riemannian signature, q=0)
///
/// Uses Frobenius norm to measure errors.
/// </summary>
public sealed class CliffordAlgebraValidator
{
    /// <summary>Validate a GammaOperatorBundle and return a CliffordValidationResult.</summary>
    public CliffordValidationResult Validate(GammaOperatorBundle bundle, double tolerance = 1e-12)
    {
        var gammas = bundle.GammaMatrices; // Complex[][,]
        int n = bundle.Signature.Dimension;
        int s = bundle.SpinorDimension;
        var sig = bundle.Signature;
        var notes = new List<string>();

        // 1. Anticommutation: {Gamma_a, Gamma_b} = 2 * eta_{ab} * I
        double maxAnticommError = 0.0;
        var ident = Identity(s);
        for (int a = 0; a < n; a++)
        {
            for (int b = 0; b < n; b++)
            {
                // {Gamma_a, Gamma_b} = Gamma_a*Gamma_b + Gamma_b*Gamma_a
                var comm = MatAdd(MatMul(gammas[a], gammas[b]), MatMul(gammas[b], gammas[a]));

                // Expected: 2 * eta_{ab} * I
                Complex expected = Complex.Zero;
                if (a == b)
                    expected = a < sig.Positive ? new Complex(2.0, 0) : new Complex(-2.0, 0);

                var diff = MatSubScaled(comm, expected, ident);
                double err = FrobeniusNorm(diff);
                if (err > maxAnticommError) maxAnticommError = err;
            }
        }

        if (maxAnticommError > tolerance)
            notes.Add($"Anticommutation max error {maxAnticommError:E3} exceeds tolerance {tolerance:E3}");

        // 2+3. Chirality checks (only for even n)
        double chiralitySquareError = 0.0;
        double chiralityAnticommError = 0.0;
        if (bundle.ChiralityMatrix != null)
        {
            var gc = bundle.ChiralityMatrix;

            // Gamma_chi^2 = I
            var gcSq = MatMul(gc, gc);
            chiralitySquareError = FrobeniusNorm(MatSubScaled(gcSq, Complex.One, ident));

            if (chiralitySquareError > tolerance)
                notes.Add($"Gamma_chi^2 != I: error {chiralitySquareError:E3}");

            // {Gamma_chi, Gamma_mu} = 0
            for (int mu = 0; mu < n; mu++)
            {
                var anticomm = MatAdd(MatMul(gc, gammas[mu]), MatMul(gammas[mu], gc));
                double err = FrobeniusNorm(anticomm);
                if (err > chiralityAnticommError) chiralityAnticommError = err;
            }

            if (chiralityAnticommError > tolerance)
                notes.Add($"{{Gamma_chi, Gamma_mu}} != 0: max error {chiralityAnticommError:E3}");
        }

        // 4. Hermiticity (Riemannian only: all gammas should be Hermitian)
        double conjugationError = 0.0;
        if (sig.IsRiemannian)
        {
            for (int mu = 0; mu < n; mu++)
            {
                var diff = MatSub(gammas[mu], ConjugateTranspose(gammas[mu]));
                double err = FrobeniusNorm(diff);
                if (err > conjugationError) conjugationError = err;
            }

            if (conjugationError > tolerance)
                notes.Add($"Hermiticity max error {conjugationError:E3} (Riemannian)");
        }

        bool passed = maxAnticommError <= tolerance
                   && chiralitySquareError <= tolerance
                   && chiralityAnticommError <= tolerance
                   && (!sig.IsRiemannian || conjugationError <= tolerance);

        return new CliffordValidationResult
        {
            ConventionId = bundle.ConventionId,
            AnticommutationMaxError = maxAnticommError,
            ChiralitySquareError = chiralitySquareError,
            ChiralityAnticommutationMaxError = chiralityAnticommError,
            ConjugationConsistencyError = conjugationError,
            Passed = passed,
            Tolerance = tolerance,
            DiagnosticNotes = notes,
        };
    }

    // --- Matrix utilities ---

    private static Complex[,] MatMul(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex s = Complex.Zero;
                for (int k = 0; k < n; k++) s += a[i, k] * b[k, j];
                r[i, j] = s;
            }
        return r;
    }

    private static Complex[,] MatAdd(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] + b[i, j];
        return r;
    }

    private static Complex[,] MatSub(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] - b[i, j];
        return r;
    }

    // result = a - scale * b
    private static Complex[,] MatSubScaled(Complex[,] a, Complex scale, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] - scale * b[i, j];
        return r;
    }

    private static Complex[,] Identity(int n)
    {
        var m = new Complex[n, n];
        for (int i = 0; i < n; i++) m[i, i] = Complex.One;
        return m;
    }

    private static Complex[,] ConjugateTranspose(Complex[,] a)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = Complex.Conjugate(a[j, i]);
        return r;
    }

    private static double FrobeniusNorm(Complex[,] m)
    {
        int n = m.GetLength(0);
        double sum = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                sum += m[i, j].Real * m[i, j].Real + m[i, j].Imaginary * m[i, j].Imaginary;
        return System.Math.Sqrt(sum);
    }
}
