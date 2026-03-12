using Gu.Phase4.Fermions;

namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// CPU reference implementation of IDiracKernel for M44 parity testing.
///
/// Wraps a Gu.Phase4.Dirac.DiracOperatorBundle (explicit complex matrix) and an optional set of
/// fermionic modes to implement the four operator actions needed for CPU/CUDA parity.
///
/// Convention: all vectors are real-interleaved complex, length = TotalDof.
/// TotalDof = 2 * cellCount * dofsPerCell (the "2" accounts for complex interleaving).
///
/// For the parity-test use case the DofsPerCell encodes the full spinor+gauge dimension.
/// P_L and P_R project using a block-diagonal gamma5: upper block +1, lower block sign.
/// AccumulateCouplingProxy uses the diagonal approximation: delta_D[b] ≈ diag(b).
/// </summary>
public sealed class CpuDiracKernel : IDiracKernel
{
    private readonly Gu.Phase4.Dirac.DiracOperatorBundle _opBundle;
    private readonly IReadOnlyList<FermionModeRecord>? _modes;

    /// <param name="opBundle">Assembled Dirac operator bundle (must have ExplicitMatrix if ApplyDirac is called).</param>
    /// <param name="modes">Fermionic eigenmodes for coupling proxy computation (may be null).</param>
    public CpuDiracKernel(
        Gu.Phase4.Dirac.DiracOperatorBundle opBundle,
        IReadOnlyList<FermionModeRecord>? modes = null)
    {
        ArgumentNullException.ThrowIfNull(opBundle);
        _opBundle = opBundle;
        _modes = modes;
        TotalDof = 2 * opBundle.CellCount * opBundle.DofsPerCell;
    }

    /// <inheritdoc/>
    public int TotalDof { get; }

    /// <inheritdoc/>
    public bool ComputedWithCuda => false;

    /// <inheritdoc/>
    public void ApplyDirac(ReadOnlySpan<double> psi, Span<double> result)
    {
        ValidateLength(psi, result);
        ApplyExplicitMatrix(psi, result);
    }

    /// <inheritdoc/>
    public void ApplyMassPsi(ReadOnlySpan<double> psi, Span<double> result)
    {
        ValidateLength(psi, result);
        // Unit-weight mass: identity operator
        psi.CopyTo(result);
    }

    /// <inheritdoc/>
    public void ProjectLeft(ReadOnlySpan<double> psi, Span<double> result)
    {
        ValidateLength(psi, result);
        ApplyChiralityProjector(psi, result, gammaSign: -1);
    }

    /// <inheritdoc/>
    public void ProjectRight(ReadOnlySpan<double> psi, Span<double> result)
    {
        ValidateLength(psi, result);
        ApplyChiralityProjector(psi, result, gammaSign: +1);
    }

    /// <inheritdoc/>
    public double AccumulateCouplingProxy(
        ReadOnlySpan<double> bosonPerturbation,
        IReadOnlyList<(int ModeI, int ModeJ)> modePairs)
    {
        if (bosonPerturbation.Length != TotalDof)
            throw new ArgumentException(
                $"bosonPerturbation length {bosonPerturbation.Length} != TotalDof {TotalDof}.");

        if (modePairs.Count == 0 || _modes is null || _modes.Count == 0)
            return 0.0;

        // Coupling proxy sum_{i,j} |<phi_i | delta_D[b] | phi_j>|
        // Diagonal approximation: delta_D[b] psi ≈ b * psi (component-wise)
        double total = 0.0;
        foreach (var (modeI, modeJ) in modePairs)
        {
            if (modeI < 0 || modeI >= _modes.Count) continue;
            if (modeJ < 0 || modeJ >= _modes.Count) continue;

            var phiI = _modes[modeI].EigenvectorCoefficients;
            var phiJ = _modes[modeJ].EigenvectorCoefficients;
            if (phiI is null || phiJ is null) continue;

            int n = System.Math.Min(TotalDof, System.Math.Min(phiI.Length, phiJ.Length));
            n = System.Math.Min(n, bosonPerturbation.Length);
            // Align to even (complex pairs)
            n = (n / 2) * 2;

            double re = 0.0, im = 0.0;
            for (int k = 0; k < n; k += 2)
            {
                double bRe = bosonPerturbation[k];
                double bIm = bosonPerturbation[k + 1];
                // delta_D[b] phi_j at component k: (bRe * phiJ_re - bIm * phiJ_im, bRe * phiJ_im + bIm * phiJ_re)
                double dpjRe = bRe * phiJ[k] - bIm * phiJ[k + 1];
                double dpjIm = bRe * phiJ[k + 1] + bIm * phiJ[k];
                // <phi_i | dpj> = conj(phi_i) * dpj = (phiI_re - i phiI_im)(dpjRe + i dpjIm)
                re += phiI[k] * dpjRe + phiI[k + 1] * dpjIm;
                im += phiI[k] * dpjIm - phiI[k + 1] * dpjRe;
            }
            total += System.Math.Sqrt(re * re + im * im);
        }
        return total;
    }

    // Apply the explicit complex matrix stored flat in _opBundle.ExplicitMatrix
    // Layout: [row * TotalDof + col], each entry = (re, im) pair, so stride = TotalDof*2
    private void ApplyExplicitMatrix(ReadOnlySpan<double> psi, Span<double> result)
    {
        result.Fill(0.0);
        var mat = _opBundle.ExplicitMatrix;
        if (mat is null) return; // identity fallback: result stays 0 (intentional for tests)

        int n = _opBundle.TotalDof; // = CellCount * DofsPerCell (half of real TotalDof)
        // ExplicitMatrix is complex: mat[2*(row*n + col)] = Re, mat[2*(row*n+col)+1] = Im
        // psi/result are real-interleaved length = 2*n
        for (int row = 0; row < n; row++)
        {
            double sumRe = 0.0, sumIm = 0.0;
            for (int col = 0; col < n; col++)
            {
                int matIdx = 2 * (row * n + col);
                if (matIdx + 1 >= mat.Length) break;
                double mRe = mat[matIdx];
                double mIm = mat[matIdx + 1];
                double xRe = psi[2 * col];
                double xIm = psi[2 * col + 1];
                // (mRe + i mIm)(xRe + i xIm)
                sumRe += mRe * xRe - mIm * xIm;
                sumIm += mRe * xIm + mIm * xRe;
            }
            result[2 * row] = sumRe;
            result[2 * row + 1] = sumIm;
        }
    }

    // P_{L/R} = (1 + gammaSign * gamma5) / 2, gammaSign=-1 for P_L, +1 for P_R
    // Minimal gamma5: upper half of spinor block = +eigenvalue, lower = -eigenvalue
    private void ApplyChiralityProjector(ReadOnlySpan<double> psi, Span<double> result, int gammaSign)
    {
        int dof = _opBundle.DofsPerCell;
        int half = dof / 2;
        if (half < 1) half = 1;

        for (int cell = 0; cell < _opBundle.CellCount; cell++)
        {
            int offset = cell * dof; // complex-component index
            for (int d = 0; d < dof; d++)
            {
                // gamma5 eigenvalue: +1 for upper half spinor, -1 for lower half
                double gamma5Eig = d < half ? 1.0 : -1.0;
                double factor = 0.5 * (1.0 + gammaSign * gamma5Eig);
                int idx = 2 * (offset + d);
                result[idx] = factor * psi[idx];
                result[idx + 1] = factor * psi[idx + 1];
            }
        }
    }

    private void ValidateLength(ReadOnlySpan<double> psi, Span<double> result)
    {
        if (psi.Length != TotalDof)
            throw new ArgumentException($"psi length {psi.Length} != TotalDof {TotalDof}.");
        if (result.Length != TotalDof)
            throw new ArgumentException($"result length {result.Length} != TotalDof {TotalDof}.");
    }
}
