using System.Numerics;

namespace Gu.Phase4.Spin;

/// <summary>
/// Builds gamma matrices for Cl(p,q) using the recursive Pauli tensor-product construction.
///
/// Algorithm (Riemannian Cl(n,0)):
///   Base case n=2: Gamma_1 = sigma_1, Gamma_2 = sigma_2 (2x2 complex)
///   Recursive step Cl(n) -> Cl(n+2):
///     Gamma'_mu = Gamma_mu ⊗ sigma_3         (mu = 1..n)
///     Gamma'_{n+1} = I_s ⊗ sigma_1
///     Gamma'_{n+2} = I_s ⊗ sigma_2
///   For odd n: take Cl(n-1) gammas, then Gamma_n = i^{(n-1)/2} * prod(all previous)
///
/// For Lorentzian Cl(p,q) with q>0: negative-signature gammas become i * Gamma_riemannian.
/// Only Cl(p,0) is fully validated in this first pass (P4-IA: Riemannian default).
///
/// GammaMatrices is Complex[][,]: index [direction][row, col].
/// SpinorDimension = 2^floor(n/2).
/// </summary>
public sealed class GammaMatrixBuilder
{
    // Pauli matrices (2x2)
    private static readonly Complex[,] Sigma1 = { { Complex.Zero, Complex.One }, { Complex.One, Complex.Zero } };
    private static readonly Complex[,] Sigma2 = { { Complex.Zero, new Complex(0, -1) }, { new Complex(0, 1), Complex.Zero } };
    private static readonly Complex[,] Sigma3 = { { Complex.One, Complex.Zero }, { Complex.Zero, new Complex(-1, 0) } };

    /// <summary>
    /// Build gamma matrices for the given signature and convention.
    /// Returns a GammaOperatorBundle with all gamma matrices and optional chirality matrix.
    /// </summary>
    public GammaOperatorBundle Build(CliffordSignature signature, GammaConventionSpec convention, Gu.Core.ProvenanceMeta provenance)
    {
        int n = signature.Dimension;
        int spinorDim = SpinorDimension(n);

        var gammaMatrices = BuildRiemannianGammas(n);

        // For Lorentzian or mixed: multiply negative-signature gammas by i (anti-Hermitian)
        if (signature.Negative > 0)
        {
            for (int mu = n - signature.Negative; mu < n; mu++)
            {
                var mat = gammaMatrices[mu];
                var scaled = new Complex[spinorDim, spinorDim];
                for (int r = 0; r < spinorDim; r++)
                    for (int c = 0; c < spinorDim; c++)
                        scaled[r, c] = Complex.ImaginaryOne * mat[r, c];
                gammaMatrices[mu] = scaled;
            }
        }

        // Build chirality matrix for even dimensions
        Complex[,]? chiralityMatrix = null;
        if (n % 2 == 0)
            chiralityMatrix = BuildChiralityMatrix(gammaMatrices, n);

        return new GammaOperatorBundle
        {
            ConventionId = convention.ConventionId,
            Signature = signature,
            SpinorDimension = spinorDim,
            GammaMatrices = gammaMatrices,
            ChiralityMatrix = chiralityMatrix,
            Provenance = provenance,
        };
    }

    /// <summary>Spinor dimension for Cl(n): 2^floor(n/2).</summary>
    public static int SpinorDimension(int n) => 1 << (n / 2);

    /// <summary>
    /// Recursive tensor-product construction of Riemannian gamma matrices.
    /// Returns Complex[][,] with n entries; result[mu] is spinorDim x spinorDim.
    /// </summary>
    private static Complex[][,] BuildRiemannianGammas(int n)
    {
        if (n == 0)
            return Array.Empty<Complex[,]>();

        if (n == 1)
        {
            // Cl(1,0): single gamma Gamma_1 = [[1]] (1x1), spinorDim = 1
            return new Complex[][,] { new Complex[1, 1] { { Complex.One } } };
        }

        if (n == 2)
        {
            // Base case: Cl(2,0), spinorDim=2
            return new Complex[][,]
            {
                CloneMatrix(Sigma1),
                CloneMatrix(Sigma2),
            };
        }

        if (n % 2 == 0)
        {
            // Build Cl(n) from Cl(n-2) by adding two new gammas
            var prev = BuildRiemannianGammas(n - 2);
            int s = SpinorDimension(n - 2);

            var result = new Complex[n][,];

            // Extend existing gammas: Gamma'_mu = Gamma_mu ⊗ sigma_3
            for (int mu = 0; mu < n - 2; mu++)
                result[mu] = KroneckerProduct(prev[mu], Sigma3);

            // Add two new gammas using identity ⊗ Pauli
            result[n - 2] = KroneckerProduct(Identity(s), Sigma1);
            result[n - 1] = KroneckerProduct(Identity(s), Sigma2);

            return result;
        }
        else
        {
            // Odd n: start from n-1 (even), add Gamma_n = i^k * Gamma_1 * ... * Gamma_{n-1}
            var evenGammas = BuildRiemannianGammas(n - 1);
            int s = SpinorDimension(n - 1); // spinorDim stays same for odd extension

            var result = new Complex[n][,];
            for (int mu = 0; mu < n - 1; mu++)
                result[mu] = evenGammas[mu];

            // Gamma_n = i^k * product of all previous, where k = (n-1)/2
            int k = (n - 1) / 2;
            Complex phaseFactor = ComplexIPower(k);
            var product = Identity(s);
            for (int mu = 0; mu < n - 1; mu++)
                product = MatMul(product, evenGammas[mu]);

            result[n - 1] = ScaleMatrix(phaseFactor, product);
            return result;
        }
    }

    /// <summary>
    /// Build the chirality matrix for even n:
    /// Gamma_chi = i^(n/2) * Gamma_1 * Gamma_2 * ... * Gamma_n
    /// </summary>
    private static Complex[,] BuildChiralityMatrix(Complex[][,] gammas, int n)
    {
        // Gamma_chi = i^(n/2) * Gamma_1 * ... * Gamma_n
        // For n=2: i^1 * sigma_1 * sigma_2 = i*(i*sigma_3) = -sigma_3 = diag(-1,+1)
        // [1,0] is in the -1 eigenspace => left under "left-is-minus".
        int k = n / 2;
        Complex phase = ComplexIPower(k); // i^k
        int s = gammas[0].GetLength(0);
        var result = Identity(s);
        for (int mu = 0; mu < n; mu++)
            result = MatMul(result, gammas[mu]);
        return ScaleMatrix(phase, result);
    }

    // --- Matrix utilities ---

    internal static Complex[,] KroneckerProduct(Complex[,] a, Complex[,] b)
    {
        int ra = a.GetLength(0), ca = a.GetLength(1);
        int rb = b.GetLength(0), cb = b.GetLength(1);
        var result = new Complex[ra * rb, ca * cb];
        for (int i = 0; i < ra; i++)
            for (int j = 0; j < ca; j++)
                for (int p = 0; p < rb; p++)
                    for (int q = 0; q < cb; q++)
                        result[i * rb + p, j * cb + q] = a[i, j] * b[p, q];
        return result;
    }

    internal static Complex[,] MatMul(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var result = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex s = Complex.Zero;
                for (int k = 0; k < n; k++)
                    s += a[i, k] * b[k, j];
                result[i, j] = s;
            }
        return result;
    }

    internal static Complex[,] Identity(int n)
    {
        var m = new Complex[n, n];
        for (int i = 0; i < n; i++) m[i, i] = Complex.One;
        return m;
    }

    private static Complex[,] ScaleMatrix(Complex scale, Complex[,] m)
    {
        int n = m.GetLength(0);
        var result = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = scale * m[i, j];
        return result;
    }

    private static Complex[,] CloneMatrix(Complex[,] m)
    {
        int rows = m.GetLength(0), cols = m.GetLength(1);
        var result = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = m[i, j];
        return result;
    }

    // i^k cycles: i^0=1, i^1=i, i^2=-1, i^3=-i
    private static Complex ComplexIPower(int k)
    {
        return (((k % 4) + 4) % 4) switch
        {
            0 => Complex.One,
            1 => Complex.ImaginaryOne,
            2 => new Complex(-1, 0),
            3 => new Complex(0, -1),
            _ => Complex.One
        };
    }
}
