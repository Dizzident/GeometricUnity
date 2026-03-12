using System.Numerics;
using Gu.Phase4.Dirac;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Couplings;

/// <summary>
/// Computes the analytical variation of the Dirac operator with respect to the
/// gauge connection omega.
///
/// For a bosonic mode b_k (edge perturbation delta_omega in connection space):
///
///   dD_h/domega[b_k] = sum_edges e: (1/h_e) * sum_mu (n_mu * Gamma_mu) ⊗ G^{(a)}_e
///
/// where G^{(a)}_e is the gauge representation of b_k_e^a * T_a for edge e.
///
/// The Levi-Civita spin connection does NOT depend on omega, so it drops out
/// of the variation (P4-IA-003: flat LC assumption, zero LC coefficients).
///
/// Reference: ARCH_P4.md §7.8 (analytical variation method)
/// </summary>
public sealed class DiracVariationComputer
{
    /// <summary>
    /// Compute the variation matrix delta_D for a bosonic perturbation given as
    /// a flat edge-indexed connection perturbation.
    ///
    /// deltaOmegaEdge[e * dimG + a] = b_k_e^a: the a-th gauge component on edge e.
    ///
    /// Returns (Re, Im) of the variation matrix as (totalDof x totalDof) arrays.
    /// totalDof = cellCount * spinorDim * dimG.
    ///
    /// This uses the same Kronecker structure as CpuDiracOperatorAssembler:
    ///   D contribution per edge (cA,cB): (1/h_e) * sum_mu(n_mu * Gamma_mu) ⊗ G_e
    /// The variation replaces G_e (from omega_e) with G_e^{(b_k)} (from b_k_e).
    /// </summary>
    public static (double[,] Re, double[,] Im) ComputeAnalytical(
        double[] deltaOmegaEdge,
        GammaOperatorBundle gammas,
        int cellCount,
        int spinorDim,
        int dimG,
        double[] edgeLengths,
        int[][] cellsPerEdge,
        double[][] edgeDirections)
    {
        ArgumentNullException.ThrowIfNull(deltaOmegaEdge);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(edgeLengths);
        ArgumentNullException.ThrowIfNull(cellsPerEdge);
        ArgumentNullException.ThrowIfNull(edgeDirections);

        int dim = gammas.GammaMatrices.Length;
        int dofsPerCell = spinorDim * dimG;
        int totalDof = cellCount * dofsPerCell;
        var re = new double[totalDof, totalDof];
        var im = new double[totalDof, totalDof];

        int edgeCount = cellsPerEdge.Length;
        if (deltaOmegaEdge.Length != edgeCount * dimG)
            throw new ArgumentException(
                $"deltaOmegaEdge length {deltaOmegaEdge.Length} != edgeCount*dimG={edgeCount * dimG}");

        for (int edgeIdx = 0; edgeIdx < edgeCount; edgeIdx++)
        {
            var cells = cellsPerEdge[edgeIdx];
            if (cells.Length < 2) continue;
            int cA = cells[0];
            int cB = cells[1];

            double h = edgeLengths[edgeIdx];
            if (h < 1e-30) continue;
            double invH = 1.0 / h;

            var nVec = edgeDirections[edgeIdx];
            int spaceDim = System.Math.Min(dim, nVec.Length);

            // Build gauge matrix G_e from b_k_e^a
            var gRe = new double[dimG, dimG];
            var gIm = new double[dimG, dimG];

            if (dimG == 1)
            {
                // Trivial representation: scalar
                gRe[0, 0] = deltaOmegaEdge[edgeIdx * dimG];
            }
            else if (dimG == 3)
            {
                // su(2) adjoint: [rho(T_a)]_{bc} = eps_{abc}
                for (int a = 0; a < 3; a++)
                {
                    double omega_a = deltaOmegaEdge[edgeIdx * dimG + a];
                    if (System.Math.Abs(omega_a) < 1e-30) continue;
                    // rho(T_a)_{bc} = sum_a delta * eps_{abc} added
                    for (int b = 0; b < 3; b++)
                        for (int c = 0; c < 3; c++)
                        {
                            double eps = LeviCivita3(a, b, c);
                            gRe[b, c] += omega_a * eps;
                        }
                }
            }
            // else: no gauge coupling

            // Build Gamma contraction: sum_mu n_mu * Gamma_mu (spinorDim x spinorDim complex)
            var gammaContractRe = new double[spinorDim, spinorDim];
            var gammaContractIm = new double[spinorDim, spinorDim];

            for (int mu = 0; mu < spaceDim; mu++)
            {
                double n_mu = nVec[mu];
                if (System.Math.Abs(n_mu) < 1e-30) continue;
                var gmat = gammas.GammaMatrices[mu];
                for (int r = 0; r < spinorDim; r++)
                    for (int c = 0; c < spinorDim; c++)
                    {
                        gammaContractRe[r, c] += n_mu * gmat[r, c].Real;
                        gammaContractIm[r, c] += n_mu * gmat[r, c].Imaginary;
                    }
            }

            // Accumulate Kronecker block: invH * (gammaContract ⊗ G_e)
            // for (cA -> cB) and Hermitian conjugate (cB -> cA)
            AddKroneckerBlock(re, im, cA, cB, dofsPerCell, spinorDim, dimG,
                invH, gammaContractRe, gammaContractIm, gRe, gIm, conjugate: false);
            AddKroneckerBlock(re, im, cB, cA, dofsPerCell, spinorDim, dimG,
                invH, gammaContractRe, gammaContractIm, gRe, gIm, conjugate: true);
        }

        return (re, im);
    }

    private static void AddKroneckerBlock(
        double[,] re, double[,] im,
        int cRow, int cCol,
        int dofsPerCell, int spinorDim, int dimG,
        double scale,
        double[,] gammaRe, double[,] gammaIm,
        double[,] gaugeRe, double[,] gaugeIm,
        bool conjugate)
    {
        for (int sRow = 0; sRow < spinorDim; sRow++)
            for (int sCol = 0; sCol < spinorDim; sCol++)
            {
                double gR = conjugate ? gammaRe[sCol, sRow] : gammaRe[sRow, sCol];
                double gI = conjugate ? -gammaIm[sCol, sRow] : gammaIm[sRow, sCol];

                for (int gRow = 0; gRow < dimG; gRow++)
                    for (int gCol = 0; gCol < dimG; gCol++)
                    {
                        double gauR = conjugate ? gaugeRe[gCol, gRow] : gaugeRe[gRow, gCol];
                        double gauI = conjugate ? -gaugeIm[gCol, gRow] : gaugeIm[gRow, gCol];

                        double valRe = gR * gauR - gI * gauI;
                        double valIm = gR * gauI + gI * gauR;

                        int row = cRow * dofsPerCell + sRow * dimG + gRow;
                        int col = cCol * dofsPerCell + sCol * dimG + gCol;
                        re[row, col] += scale * valRe;
                        im[row, col] += scale * valIm;
                    }
            }
    }

    private static double LeviCivita3(int a, int b, int c)
    {
        // Full antisymmetric Levi-Civita symbol for su(2) adjoint
        if (a == b || b == c || a == c) return 0.0;
        // Even permutations of (0,1,2): +1
        if ((a == 0 && b == 1 && c == 2) ||
            (a == 1 && b == 2 && c == 0) ||
            (a == 2 && b == 0 && c == 1)) return 1.0;
        return -1.0;
    }
}
