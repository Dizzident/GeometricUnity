using System.Numerics;
using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Couplings;

/// <summary>
/// Computes boson-fermion coupling proxies g_ijk = &lt;phi_i^†, delta_D[b_k] phi_j&gt;
/// via finite-difference variation of the Dirac operator.
///
/// Algorithm (P4-IA §8):
///   1. Given background z_*, compute D(z_*) (already in diracBundle).
///   2. For each boson mode b_k with eigenvector coefficients:
///        a. Build perturbed connection omega_eps = omega_* + eps * b_k_connection
///        b. Assemble D(omega_eps) via assembler
///        c. Compute delta_D = (D(omega_eps) - D(z_*)) / eps  (finite difference)
///   3. For each pair (phi_i, phi_j) of fermionic modes:
///        g_ijk = &lt;phi_i, delta_D phi_j&gt; (complex inner product)
///
/// Since this is a coupling PROXY (not a derived constant), the result depends
/// on normalization conventions. See NormalizationConvention in CouplingAtlas.
///
/// Normalization conventions:
///   "raw"           : no normalization beyond the eigenvector norms
///   "unit-modes"    : phi_i and phi_j are unit-normalized before inner product
///   "unit-boson"    : additionally normalize the boson variation by its L2 norm
/// </summary>
public sealed class CouplingProxyEngine
{
    private readonly IDiracOperatorAssembler _assembler;
    private const double DefaultEpsilon = 1e-5;

    public CouplingProxyEngine(IDiracOperatorAssembler assembler)
    {
        ArgumentNullException.ThrowIfNull(assembler);
        _assembler = assembler;
    }

    /// <summary>
    /// Compute one DiracVariationBundle and coupling record for a single
    /// (bosonMode, fermionModeI, fermionModeJ) triple.
    ///
    /// The bosonVariationMatrix must be supplied externally (e.g., pre-assembled
    /// finite-difference delta_D). If null, a zero coupling is returned with a
    /// diagnostic note.
    /// </summary>
    public BosonFermionCouplingRecord ComputeCoupling(
        FermionModeRecord modeI,
        FermionModeRecord modeJ,
        string bosonModeId,
        double[,]? bosonVariationMatrixRe,
        double[,]? bosonVariationMatrixIm,
        string normalizationConvention,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(modeI);
        ArgumentNullException.ThrowIfNull(modeJ);
        ArgumentNullException.ThrowIfNull(bosonModeId);
        ArgumentNullException.ThrowIfNull(normalizationConvention);
        ArgumentNullException.ThrowIfNull(provenance);

        string couplingId = $"coupling-{bosonModeId}-{modeI.ModeId}-{modeJ.ModeId}";

        if (modeI.EigenvectorCoefficients is null || modeJ.EigenvectorCoefficients is null
            || bosonVariationMatrixRe is null)
        {
            return new BosonFermionCouplingRecord
            {
                CouplingId = couplingId,
                BosonModeId = bosonModeId,
                FermionModeIdI = modeI.ModeId,
                FermionModeIdJ = modeJ.ModeId,
                CouplingProxyReal = 0.0,
                CouplingProxyImaginary = 0.0,
                CouplingProxyMagnitude = 0.0,
                NormalizationConvention = normalizationConvention,
                SelectionRuleNotes = new List<string> { "Missing eigenvector or variation matrix; zero coupling returned." },
                Provenance = provenance,
            };
        }

        int n = bosonVariationMatrixRe.GetLength(0);
        double[] phiI = modeI.EigenvectorCoefficients;
        double[] phiJ = modeJ.EigenvectorCoefficients;

        if (phiI.Length != n * 2 || phiJ.Length != n * 2)
            throw new ArgumentException(
                $"Eigenvector length mismatch: phiI={phiI.Length}, phiJ={phiJ.Length}, " +
                $"expected {n * 2} (2*{n} for complex interleaved).");

        // Optionally unit-normalize the mode vectors
        double[] piNorm = phiI;
        double[] pjNorm = phiJ;
        if (normalizationConvention is "unit-modes" or "unit-boson")
        {
            piNorm = NormalizeVector(phiI);
            pjNorm = NormalizeVector(phiJ);
        }

        // Compute delta_D * phi_j
        // delta_D is stored as (n x n) complex matrix in two real arrays
        var deltaPhiJ = new double[n * 2];
        for (int i = 0; i < n; i++)
        {
            double sumRe = 0.0, sumIm = 0.0;
            for (int j = 0; j < n; j++)
            {
                double aRe = bosonVariationMatrixRe[i, j];
                double aIm = bosonVariationMatrixIm is not null ? bosonVariationMatrixIm[i, j] : 0.0;
                double bRe = pjNorm[j * 2];
                double bIm = pjNorm[j * 2 + 1];
                // (aRe + i*aIm)(bRe + i*bIm) = (aRe*bRe - aIm*bIm) + i*(aRe*bIm + aIm*bRe)
                sumRe += aRe * bRe - aIm * bIm;
                sumIm += aRe * bIm + aIm * bRe;
            }
            deltaPhiJ[i * 2] = sumRe;
            deltaPhiJ[i * 2 + 1] = sumIm;
        }

        // Compute g = <phi_i, delta_D phi_j> = sum_k conj(phi_i[k]) * (delta_D phi_j)[k]
        double gRe = 0.0, gIm = 0.0;
        for (int i = 0; i < n; i++)
        {
            double piRe = piNorm[i * 2];
            double piIm = piNorm[i * 2 + 1];
            double djRe = deltaPhiJ[i * 2];
            double djIm = deltaPhiJ[i * 2 + 1];
            // conj(piRe + i*piIm) * (djRe + i*djIm) = (piRe*djRe + piIm*djIm) + i*(piRe*djIm - piIm*djRe)
            gRe += piRe * djRe + piIm * djIm;
            gIm += piRe * djIm - piIm * djRe;
        }

        double gMag = System.Math.Sqrt(gRe * gRe + gIm * gIm);

        return new BosonFermionCouplingRecord
        {
            CouplingId = couplingId,
            BosonModeId = bosonModeId,
            FermionModeIdI = modeI.ModeId,
            FermionModeIdJ = modeJ.ModeId,
            CouplingProxyReal = gRe,
            CouplingProxyImaginary = gIm,
            CouplingProxyMagnitude = gMag,
            NormalizationConvention = normalizationConvention,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Build a full CouplingAtlas for all (bosonMode, fermionI, fermionJ) combinations
    /// where a variation matrix is provided.
    ///
    /// variationMatrices: maps bosonModeId -> (Re matrix, Im matrix). If Im is null, treated as zero.
    /// </summary>
    public CouplingAtlas BuildAtlas(
        string atlasId,
        string fermionBackgroundId,
        IReadOnlyList<FermionModeRecord> fermionModes,
        IReadOnlyDictionary<string, (double[,] Re, double[,]? Im)> variationMatrices,
        string normalizationConvention,
        string bosonRegistryVersion,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(atlasId);
        ArgumentNullException.ThrowIfNull(fermionBackgroundId);
        ArgumentNullException.ThrowIfNull(fermionModes);
        ArgumentNullException.ThrowIfNull(variationMatrices);
        ArgumentNullException.ThrowIfNull(normalizationConvention);
        ArgumentNullException.ThrowIfNull(bosonRegistryVersion);
        ArgumentNullException.ThrowIfNull(provenance);

        var couplings = new List<BosonFermionCouplingRecord>();

        foreach (var (bosonModeId, (reMatrix, imMatrix)) in variationMatrices)
        {
            for (int i = 0; i < fermionModes.Count; i++)
            {
                for (int j = 0; j < fermionModes.Count; j++)
                {
                    var record = ComputeCoupling(
                        fermionModes[i],
                        fermionModes[j],
                        bosonModeId,
                        reMatrix,
                        imMatrix,
                        normalizationConvention,
                        provenance);
                    couplings.Add(record);
                }
            }
        }

        return new CouplingAtlas
        {
            AtlasId = atlasId,
            FermionBackgroundId = fermionBackgroundId,
            BosonRegistryVersion = bosonRegistryVersion,
            Couplings = couplings,
            NormalizationConvention = normalizationConvention,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Compute finite-difference variation matrix delta_D = (D(omega + eps*dOmega) - D(omega)) / eps.
    ///
    /// Returns (Re, Im) parts of the complex variation matrix as (n x n) arrays,
    /// where n = totalDof of the Dirac operator.
    /// </summary>
    public static (double[,] Re, double[,] Im) ComputeVariationMatrix(
        double[,] dBaseRe,
        double[,] dBaseIm,
        double[,] dPerturbedRe,
        double[,] dPerturbedIm,
        double epsilon)
    {
        if (epsilon <= 0.0)
            throw new ArgumentException("epsilon must be positive.", nameof(epsilon));

        int n = dBaseRe.GetLength(0);
        var reResult = new double[n, n];
        var imResult = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                reResult[i, j] = (dPerturbedRe[i, j] - dBaseRe[i, j]) / epsilon;
                imResult[i, j] = (dPerturbedIm[i, j] - dBaseIm[i, j]) / epsilon;
            }
        }

        return (reResult, imResult);
    }

    // --- Utilities ---

    private static double[] NormalizeVector(double[] v)
    {
        double norm2 = 0.0;
        for (int i = 0; i < v.Length; i++)
            norm2 += v[i] * v[i];
        double norm = System.Math.Sqrt(norm2);
        if (norm < 1e-30)
            return v; // zero vector: return as-is
        var result = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
            result[i] = v[i] / norm;
        return result;
    }
}
