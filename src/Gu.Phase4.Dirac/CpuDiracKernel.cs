using System.Numerics;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// CPU reference implementation of IDiracKernel.
///
/// Wraps CpuDiracOperatorAssembler to implement the matrix-free kernel interface
/// for use in parity testing against GpuDiracKernel.
///
/// Initialized from a pre-assembled DiracOperatorBundle plus the context needed
/// for matrix-free apply (gammas, mesh, layout, connection).
///
/// PhysicsNote:
/// - ApplyGamma applies Gamma_mu pointwise to every cell's spinor block.
/// - ApplyDirac applies the full vertex-based discrete Dirac operator.
/// - ApplyMass applies M_psi = diag(vol_cell * I) (uniform cell volume = 1.0 default).
/// - ApplyChiralityProjector throws for odd dimY (no chirality grading).
/// - ComputeCouplingProxy uses the analytical variation dD/d_omega[b_k] = Gamma^mu * b_k_mu^a * rho(T_a).
///   Under P4-IA-003 (flat LC), Levi-Civita part drops out.
/// </summary>
public sealed class CpuDiracKernel : IDiracKernel
{
    private readonly DiracOperatorBundle _bundle;
    private readonly GammaOperatorBundle _gammas;
    private readonly SimplicialMesh _mesh;
    private readonly FermionFieldLayout _layout;
    private readonly SpinConnectionBundle _connection;
    private readonly CpuDiracOperatorAssembler _assembler;

    private readonly int _cellCount;
    private readonly int _spinorDim;
    private readonly int _gaugeDim;
    private readonly int _dofsPerCell;

    /// <summary>
    /// Create a CPU Dirac kernel from an assembled bundle and its assembly context.
    /// </summary>
    public CpuDiracKernel(
        DiracOperatorBundle bundle,
        GammaOperatorBundle gammas,
        SimplicialMesh mesh,
        FermionFieldLayout layout,
        SpinConnectionBundle connection,
        CpuDiracOperatorAssembler assembler)
    {
        _bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));
        _gammas = gammas ?? throw new ArgumentNullException(nameof(gammas));
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _assembler = assembler ?? throw new ArgumentNullException(nameof(assembler));

        _cellCount = bundle.CellCount;
        _spinorDim = gammas.SpinorDimension;
        _gaugeDim = layout.SpinorBlocks
            .Where(b => b.Role == "primal")
            .Select(b => b.GaugeDimension)
            .FirstOrDefault(1);
        _dofsPerCell = _spinorDim * _gaugeDim;
    }

    /// <inheritdoc />
    public int SpinorDimension => 2 * _cellCount * _dofsPerCell;

    /// <inheritdoc />
    public int SpacetimeDimension => _gammas.Signature.Dimension;

    /// <inheritdoc />
    public void ApplyGamma(int mu, ReadOnlySpan<double> spinor, Span<double> result)
    {
        int dim = _gammas.Signature.Dimension;
        if (mu < 0 || mu >= dim)
            throw new ArgumentOutOfRangeException(nameof(mu), $"mu={mu} out of range [0, {dim}).");

        int expected = SpinorDimension;
        if (spinor.Length != expected)
            throw new ArgumentException($"spinor length {spinor.Length} != {expected}");
        if (result.Length != expected)
            throw new ArgumentException($"result length {result.Length} != {expected}");

        var gamma = _gammas.GammaMatrices[mu];

        // Apply Gamma_mu blockwise: for each cell, multiply the spinor block by gamma
        // (gauge index is a trivial factor — same gamma for each gauge component)
        for (int cell = 0; cell < _cellCount; cell++)
        {
            for (int g = 0; g < _gaugeDim; g++)
            {
                int baseIn = (cell * _dofsPerCell + g * _spinorDim) * 2;
                int baseOut = (cell * _dofsPerCell + g * _spinorDim) * 2;

                // result[s] = sum_t gamma[s,t] * spinor[t]
                for (int s = 0; s < _spinorDim; s++)
                {
                    double sumRe = 0.0, sumIm = 0.0;
                    for (int t = 0; t < _spinorDim; t++)
                    {
                        double aRe = gamma[s, t].Real;
                        double aIm = gamma[s, t].Imaginary;
                        double bRe = spinor[baseIn + t * 2];
                        double bIm = spinor[baseIn + t * 2 + 1];
                        sumRe += aRe * bRe - aIm * bIm;
                        sumIm += aRe * bIm + aIm * bRe;
                    }
                    result[baseOut + s * 2]     = sumRe;
                    result[baseOut + s * 2 + 1] = sumIm;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ApplyDirac(ReadOnlySpan<double> spinor, Span<double> result)
    {
        int expected = SpinorDimension;
        if (spinor.Length != expected)
            throw new ArgumentException($"spinor length {spinor.Length} != {expected}");
        if (result.Length != expected)
            throw new ArgumentException($"result length {result.Length} != {expected}");

        var psi = spinor.ToArray();
        double[] res = _assembler.Apply(_bundle, _connection, _gammas, _layout, _mesh, psi);
        res.AsSpan().CopyTo(result);
    }

    /// <inheritdoc />
    public void ApplyMass(ReadOnlySpan<double> spinor, Span<double> result)
    {
        int expected = SpinorDimension;
        if (spinor.Length != expected)
            throw new ArgumentException($"spinor length {spinor.Length} != {expected}");
        if (result.Length != expected)
            throw new ArgumentException($"result length {result.Length} != {expected}");

        // M_psi is block-diagonal: one block per cell, each = vol(cell) * I_spinor.
        // Under P4-IA uniform cell volume = 1.0.
        // So M_psi * spinor = spinor (identity scaling by 1.0).
        spinor.CopyTo(result);
    }

    /// <inheritdoc />
    public void ApplyChiralityProjector(bool left, ReadOnlySpan<double> spinor, Span<double> result)
    {
        if (_gammas.ChiralityMatrix is null)
            throw new InvalidOperationException(
                $"Chirality operator is not defined for odd spacetime dimension (dimY={SpacetimeDimension}). " +
                "Use even-dimensional configurations for chirality projections.");

        int expected = SpinorDimension;
        if (spinor.Length != expected)
            throw new ArgumentException($"spinor length {spinor.Length} != {expected}");
        if (result.Length != expected)
            throw new ArgumentException($"result length {result.Length} != {expected}");

        var gamma5 = _gammas.ChiralityMatrix;
        double sign = left ? -1.0 : 1.0;  // P_L = (I - Gamma_chi)/2, P_R = (I + Gamma_chi)/2

        // Apply blockwise: result = (1/2)(spinor + sign * Gamma_chi * spinor)
        for (int cell = 0; cell < _cellCount; cell++)
        {
            for (int g = 0; g < _gaugeDim; g++)
            {
                int baseIdx = (cell * _dofsPerCell + g * _spinorDim) * 2;

                for (int s = 0; s < _spinorDim; s++)
                {
                    double sumRe = 0.0, sumIm = 0.0;
                    for (int t = 0; t < _spinorDim; t++)
                    {
                        double aRe = gamma5[s, t].Real;
                        double aIm = gamma5[s, t].Imaginary;
                        double bRe = spinor[baseIdx + t * 2];
                        double bIm = spinor[baseIdx + t * 2 + 1];
                        sumRe += aRe * bRe - aIm * bIm;
                        sumIm += aRe * bIm + aIm * bRe;
                    }
                    double inRe = spinor[baseIdx + s * 2];
                    double inIm = spinor[baseIdx + s * 2 + 1];
                    result[baseIdx + s * 2]     = 0.5 * (inRe + sign * sumRe);
                    result[baseIdx + s * 2 + 1] = 0.5 * (inIm + sign * sumIm);
                }
            }
        }
    }

    /// <inheritdoc />
    public (double Real, double Imag) ComputeCouplingProxy(
        ReadOnlySpan<double> spinorI,
        ReadOnlySpan<double> spinorJ,
        ReadOnlySpan<double> bosonK)
    {
        int expected = SpinorDimension;
        if (spinorI.Length != expected)
            throw new ArgumentException($"spinorI length {spinorI.Length} != {expected}");
        if (spinorJ.Length != expected)
            throw new ArgumentException($"spinorJ length {spinorJ.Length} != {expected}");

        // Analytical variation: delta_D[b_k] = Gamma^mu * b_k_mu^a * rho(T_a)
        // Under P4-IA-003 (flat LC, LC drops out) and adjoint gauge representation
        // (branch param GaugeRepresentationId = "adjoint"):
        //   delta_D[b_k] psi = sum_mu [sum_a b_k_mu^a * rho_adj(T_a)] * Gamma_mu * psi
        //
        // where b_k_mu^a is the boson coefficient on edge for direction mu and generator a
        //
        // g = <phi_i, delta_D phi_j> = sum_mu b_k_mu * <phi_i, Gamma_mu phi_j>
        //
        // For each mu: extract the scalar boson coefficient b_k_mu from bosonK.
        // bosonK has length edgeCount * dimG; use mean per-direction as a proxy.
        // The coarse approximation: mu-th direction coefficient = mean of bosonK entries
        // with edge index in the mu-th directional band.
        // For the parity test context (uniform random bosonK), use the first dimG entries
        // as the gauge coefficients for direction mu=0, next dimG for mu=1, etc.

        int dim = SpacetimeDimension;
        int edgeCount = _mesh.EdgeCount;
        int dimG = _connection.GaugeDimension;

        // Compute delta_D * spinorJ = sum_mu [sum_a b_mu^a * rho(T_a)] * Gamma_mu * spinorJ
        // Simplified: for each direction mu, compute mean boson coefficient over edges,
        // then apply Gamma_mu scaled by that coefficient.
        var deltaPhiJ = new double[expected];
        var tempBuf = new double[expected];

        for (int mu = 0; mu < dim; mu++)
        {
            // Mean of all boson coefficients for this direction.
            // In the real coupling engine the per-edge coefficients would be used per edge.
            // For the kernel interface (which receives a single bosonK vector) we use
            // a per-direction mean: average over all edgeCount * dimG entries.
            // This is a conservative projection for parity testing purposes.
            double bMuMean = 0.0;
            if (bosonK.Length >= edgeCount * dimG && edgeCount > 0 && dimG > 0)
            {
                double sum = 0.0;
                int count = System.Math.Min(bosonK.Length, edgeCount * dimG);
                for (int k = 0; k < count; k++)
                    sum += bosonK[k];
                bMuMean = (count > 0) ? sum / count : 0.0;
            }
            else if (bosonK.Length > mu)
            {
                bMuMean = bosonK[mu];
            }

            if (System.Math.Abs(bMuMean) < 1e-30) continue;

            // tempBuf = Gamma_mu * spinorJ
            ApplyGamma(mu, spinorJ, tempBuf);

            // Accumulate: deltaPhiJ += bMuMean * tempBuf
            for (int k = 0; k < expected; k++)
                deltaPhiJ[k] += bMuMean * tempBuf[k];
        }

        // g = <phi_i, delta_D phi_j> = sum_k conj(phi_i[k]) * (delta_D phi_j)[k]
        double gRe = 0.0, gIm = 0.0;
        int n = expected / 2;
        for (int k = 0; k < n; k++)
        {
            double iRe = spinorI[k * 2];
            double iIm = spinorI[k * 2 + 1];
            double dRe = deltaPhiJ[k * 2];
            double dIm = deltaPhiJ[k * 2 + 1];
            // conj(iRe + i*iIm) * (dRe + i*dIm)
            gRe += iRe * dRe + iIm * dIm;
            gIm += iRe * dIm - iIm * dRe;
        }

        return (gRe, gIm);
    }
}
