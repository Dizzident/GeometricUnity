using System.Numerics;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Finds conjugation pairs among fermionic modes.
///
/// For a Hermitian D_h (Riemannian signature), eigenvalues come in pairs (lambda, -lambda)
/// for the charge-conjugation-symmetric case. For real eigenvalues:
///   - A conjugation pair (A, B) has eigenvalueB ≈ -eigenvalueA
///   - Their eigenvectors are related by the conjugation map C: phi_B ≈ C * phi_A
///
/// For "hermitian" conjugation (simplest case):
///   C * phi = phi*  (complex conjugate)
///   Expected conjugate eigenvalue = -lambda_A (for off-diagonal charge conjugation)
///   OR lambda_A (for zero modes, which are self-conjugate)
///
/// The overlap score for a candidate pair (A, B) is:
///   score = |<phi_B, C*phi_A>|^2 / (||phi_A||^2 * ||phi_B||^2)
/// For "hermitian" conjugation: C*phi_A = phi_A* (complex conjugate).
/// </summary>
public sealed class ConjugationAnalyzer
{
    /// <summary>
    /// Find conjugation pairs in a list of modes.
    ///
    /// Searches for pairs (A, B) where:
    ///   1. |eigenvalueB - expectedConjugateEigenvalue(A)| < eigenvalueTolerance
    ///   2. overlapScore(A, B) >= overlapThreshold
    ///
    /// Returns the best pairing (each mode appears in at most one pair).
    /// </summary>
    public IReadOnlyList<ConjugationPairRecord> FindPairs(
        IReadOnlyList<FermionModeRecord> modes,
        ConjugationConventionSpec convention,
        GammaOperatorBundle gammas,
        double overlapThreshold = 0.8,
        double eigenvalueTolerance = 0.1)
    {
        ArgumentNullException.ThrowIfNull(modes);
        ArgumentNullException.ThrowIfNull(convention);

        var pairs = new List<ConjugationPairRecord>();
        var paired = new HashSet<int>();
        string conjType = convention.ConjugationType;

        for (int i = 0; i < modes.Count; i++)
        {
            if (paired.Contains(i)) continue;
            var modeA = modes[i];
            if (modeA.EigenvectorCoefficients is null) continue;

            double expectedConjEig = ComputeExpectedConjugateEigenvalue(modeA.EigenvalueRe, conjType);

            // Find best candidate j != i
            int bestJ = -1;
            double bestScore = -1.0;

            for (int j = i + 1; j < modes.Count; j++)
            {
                if (paired.Contains(j)) continue;
                var modeB = modes[j];
                if (modeB.EigenvectorCoefficients is null) continue;

                // Check eigenvalue proximity
                if (System.Math.Abs(modeB.EigenvalueRe - expectedConjEig) > eigenvalueTolerance)
                    continue;

                // Compute overlap
                double score = ComputeOverlapScore(
                    modeA.EigenvectorCoefficients,
                    modeB.EigenvectorCoefficients,
                    conjType);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestJ = j;
                }
            }

            if (bestJ >= 0 && bestScore >= overlapThreshold)
            {
                paired.Add(i);
                paired.Add(bestJ);
                var modeB = modes[bestJ];

                pairs.Add(new ConjugationPairRecord
                {
                    PairId = $"pair-{modeA.ModeId}-{modeB.ModeId}",
                    ModeIdA = modeA.ModeId,
                    ModeIdB = modeB.ModeId,
                    OverlapScore = bestScore,
                    ConjugationType = conjType,
                    IsConfident = bestScore >= overlapThreshold,
                    EigenvalueA = modeA.EigenvalueRe,
                    EigenvalueB = modeB.EigenvalueRe,
                    ExpectedConjugateEigenvalue = expectedConjEig,
                });
            }
        }

        return pairs;
    }

    /// <summary>
    /// Expected conjugate eigenvalue for a mode with eigenvalue lambda.
    /// "hermitian": -lambda (charge-conjugate eigenvalue flips sign for Dirac)
    /// "majorana":  lambda  (self-conjugate)
    /// </summary>
    private static double ComputeExpectedConjugateEigenvalue(double lambda, string conjType)
        => conjType == "majorana" ? lambda : -lambda;

    /// <summary>
    /// Compute overlap score between phi_B and C(phi_A).
    /// For "hermitian" conjugation: C(phi) = phi* (complex conjugate).
    /// Returns |<phi_B, C(phi_A)>|^2 / (||phi_A||^2 * ||phi_B||^2).
    /// </summary>
    private static double ComputeOverlapScore(
        double[] phiA, double[] phiB, string conjType)
    {
        if (phiA.Length != phiB.Length || phiA.Length % 2 != 0)
            return 0.0;

        int n = phiA.Length / 2;
        double normASq = 0.0, normBSq = 0.0;
        double overlapRe = 0.0, overlapIm = 0.0;

        for (int k = 0; k < n; k++)
        {
            double aRe = phiA[k * 2], aIm = phiA[k * 2 + 1];
            double bRe = phiB[k * 2], bIm = phiB[k * 2 + 1];

            normASq += aRe * aRe + aIm * aIm;
            normBSq += bRe * bRe + bIm * bIm;

            // C(phi_A) for "hermitian": complex conjugate => (aRe, -aIm)
            double cARe = conjType == "majorana" ? aRe : aRe;
            double cAIm = conjType == "majorana" ? aIm : -aIm;

            // <phi_B, C(phi_A)> += conj(phi_B[k]) * C(phi_A)[k] = (bRe-i*bIm)*(cARe+i*cAIm)
            overlapRe += bRe * cARe + bIm * cAIm;
            overlapIm += bRe * cAIm - bIm * cARe;
        }

        double denom = normASq * normBSq;
        if (denom < 1e-30) return 0.0;
        return (overlapRe * overlapRe + overlapIm * overlapIm) / denom;
    }
}
