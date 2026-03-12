using System.Numerics;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Computes chirality decompositions for fermionic modes.
///
/// Algorithm (ARCH_P4.md §7.5):
///   For a mode with eigenvector phi (length = cellCount * spinorDim * dimG * 2, complex interleaved):
///
///   1. Apply P_L and P_R to the spinor part of each cell's degrees of freedom.
///      P_L = (I + s * Gamma_chi) / 2  where s = sign from convention
///      P_R = (I - s * Gamma_chi) / 2
///      Sign convention "left-is-minus": s = -1  => P_L = (I - Gamma_chi)/2
///      Sign convention "left-is-plus":  s = +1  => P_L = (I + Gamma_chi)/2
///
///   2. The projectors act only on the spinorDim indices (the Clifford index).
///      The gauge index (dimG) is untouched: P acts as P ⊗ I_dimG.
///
///   3. For each cell c and gauge component g, apply the spinorDim x spinorDim P_L
///      to the spinorDim complex components of phi[c, :, g].
///
///   4. Compute left/right fractions as squared norms.
///
/// Odd-dimensional Y: chirality is not defined. Return trivial decomposition
/// (leftFraction = rightFraction = 0.5, chiralityTag = "trivial").
/// </summary>
public sealed class ChiralityAnalyzer
{
    // Threshold for "definite" chirality classification
    private const double DefiniteThreshold = 0.9;

    /// <summary>
    /// Analyze the chirality of a single mode.
    ///
    /// phi = mode.EigenvectorCoefficients, length = cellCount * spinorDim * dimG * 2.
    /// If eigenvector is null, returns a "not-applicable" decomposition.
    /// </summary>
    public ChiralityDecomposition Analyze(
        FermionModeRecord mode,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        FermionFieldLayout layout,
        int cellCount)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(chiralityConvention);
        ArgumentNullException.ThrowIfNull(layout);

        // Odd-dimensional Y: trivial
        if (!chiralityConvention.HasChirality || gammas.ChiralityMatrix is null)
        {
            return new ChiralityDecomposition
            {
                ModeId = mode.ModeId,
                LeftFraction = 0.5,
                RightFraction = 0.5,
                MixedFraction = 0.0,
                ChiralityTag = "trivial",
                ChiralityStatus = "trivial",
                LeakageDiagnostic = 0.0,
                SignConvention = chiralityConvention.SignConvention,
                DiagnosticNotes = new List<string>
                {
                    $"Y-chirality undefined for odd dim(Y); trivial decomposition reported.",
                },
            };
        }

        if (mode.EigenvectorCoefficients is null)
        {
            return new ChiralityDecomposition
            {
                ModeId = mode.ModeId,
                LeftFraction = 0.0,
                RightFraction = 0.0,
                MixedFraction = 0.0,
                ChiralityTag = "not-applicable",
                ChiralityStatus = "not-applicable",
                LeakageDiagnostic = 0.0,
                SignConvention = chiralityConvention.SignConvention,
                DiagnosticNotes = new List<string> { "No eigenvector coefficients available." },
            };
        }

        var primalBlock = layout.SpinorBlocks.FirstOrDefault(b => b.Role == "primal")
            ?? throw new InvalidOperationException("Layout has no primal block.");
        int spinorDim = primalBlock.SpinorDimension;
        int dimG = primalBlock.GaugeDimension;
        int dofsPerCell = spinorDim * dimG;
        // Derive the actual node count from eigenvector length.
        // The solver may be cell-based or vertex-based; we accept either.
        int phi_len = mode.EigenvectorCoefficients.Length;
        if (phi_len == 0 || phi_len % (dofsPerCell * 2) != 0)
            throw new ArgumentException(
                $"EigenvectorCoefficients length {phi_len} is not a multiple of " +
                $"dofsPerCell*2 = {dofsPerCell * 2} (dofsPerCell={dofsPerCell}).");
        int nodeCount = phi_len / (dofsPerCell * 2);
        // cellCount parameter is kept for API compatibility but nodeCount is used internally.
        _ = cellCount;

        // Sign factor from convention
        // "left-is-minus": P_L = (I - Gamma_chi)/2 => s = -1 for left projector
        // "left-is-plus":  P_L = (I + Gamma_chi)/2 => s = +1
        double signForLeft = chiralityConvention.SignConvention == "left-is-plus" ? +1.0 : -1.0;

        var chiMat = gammas.ChiralityMatrix;
        var phi = mode.EigenvectorCoefficients;

        double normPhiSq = 0.0;
        double normPLphiSq = 0.0;
        double normPRphiSq = 0.0;

        // Process each node
        for (int c = 0; c < nodeCount; c++)
        {
            int cellBase = c * dofsPerCell * 2; // base index in phi (complex interleaved)

            // For each gauge component g, apply P_L and P_R to spinor indices
            for (int g = 0; g < dimG; g++)
            {
                // Extract spinor vector for this (cell, gauge) slice
                var spinorSlice = new Complex[spinorDim];
                for (int s = 0; s < spinorDim; s++)
                {
                    int idx = cellBase + (s * dimG + g) * 2;
                    spinorSlice[s] = new Complex(phi[idx], phi[idx + 1]);
                }

                // Apply P_L = (I + signForLeft * Gamma_chi) / 2
                var plPhi = ApplyProjector(chiMat, spinorSlice, signForLeft, spinorDim);
                // Apply P_R = (I - signForLeft * Gamma_chi) / 2
                var prPhi = ApplyProjector(chiMat, spinorSlice, -signForLeft, spinorDim);

                // Accumulate squared norms
                for (int s = 0; s < spinorDim; s++)
                {
                    double re = spinorSlice[s].Real, im = spinorSlice[s].Imaginary;
                    normPhiSq += re * re + im * im;
                    double plRe = plPhi[s].Real, plIm = plPhi[s].Imaginary;
                    normPLphiSq += plRe * plRe + plIm * plIm;
                    double prRe = prPhi[s].Real, prIm = prPhi[s].Imaginary;
                    normPRphiSq += prRe * prRe + prIm * prIm;
                }
            }
        }

        if (normPhiSq < 1e-30)
        {
            return new ChiralityDecomposition
            {
                ModeId = mode.ModeId,
                LeftFraction = 0.0,
                RightFraction = 0.0,
                MixedFraction = 0.0,
                ChiralityTag = "not-applicable",
                ChiralityStatus = "not-applicable",
                LeakageDiagnostic = 0.0,
                SignConvention = chiralityConvention.SignConvention,
                DiagnosticNotes = new List<string> { "Zero norm eigenvector." },
            };
        }

        double leftFrac  = normPLphiSq / normPhiSq;
        double rightFrac = normPRphiSq / normPhiSq;
        double leakage   = System.Math.Abs(leftFrac + rightFrac - 1.0);
        double mixedFrac = System.Math.Max(0.0, 1.0 - leftFrac - rightFrac);

        string tag, status;
        if (leftFrac > DefiniteThreshold)
        {
            tag = "left"; status = "definite-left";
        }
        else if (rightFrac > DefiniteThreshold)
        {
            tag = "right"; status = "definite-right";
        }
        else
        {
            tag = "mixed"; status = "mixed";
        }

        return new ChiralityDecomposition
        {
            ModeId = mode.ModeId,
            LeftFraction = leftFrac,
            RightFraction = rightFrac,
            MixedFraction = mixedFrac,
            ChiralityTag = tag,
            ChiralityStatus = status,
            LeakageDiagnostic = leakage,
            SignConvention = chiralityConvention.SignConvention,
        };
    }

    /// <summary>
    /// Analyze a single mode and return a FermionChiralityRecord with X, Y, and F decompositions.
    ///
    /// X-chirality uses gammas [0..baseDimension-1].
    /// Y-chirality uses all gammas (same as Analyze); null when dimY is odd.
    /// F-chirality uses gammas [baseDimension..dimY-1]; null when dimF is odd or not set.
    ///
    /// baseDimension is read from chiralityConvention.BaseDimension.
    /// When BaseDimension is null, only Y-chirality is computed (XChirality uses Y-result).
    /// </summary>
    public FermionChiralityRecord AnalyzeTriple(
        FermionModeRecord mode,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        FermionFieldLayout layout,
        int cellCount)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(chiralityConvention);
        ArgumentNullException.ThrowIfNull(layout);

        int dimY = gammas.GammaMatrices.Length;
        int? baseDim = chiralityConvention.BaseDimension;
        int? fiberDim = chiralityConvention.FiberDimension;

        // Y-chirality: use existing Analyze (handles trivial/odd case)
        var yChirality = chiralityConvention.HasChirality && gammas.ChiralityMatrix is not null
            ? Analyze(mode, gammas, chiralityConvention, layout, cellCount)
            : null;
        // Trivial Y still produced for odd dim (ChiralityStatus="trivial") but we store null here
        // per the architect ruling: null = undefined, not trivial placeholder
        if (yChirality is not null && yChirality.ChiralityStatus == "trivial")
            yChirality = null;

        // X-chirality: partial from first baseDim gammas
        ChiralityDecomposition xChirality;
        if (baseDim is null || baseDim <= 0 || baseDim > dimY)
        {
            // No decomposition — fall back to Y-chirality result (or trivial)
            xChirality = yChirality ?? BuildTrivialDecomposition(mode.ModeId,
                chiralityConvention.SignConvention,
                $"BaseDimension not set; X-chirality unavailable.");
        }
        else if (baseDim % 2 != 0)
        {
            // Odd dimX — X-chirality undefined
            xChirality = BuildTrivialDecomposition(mode.ModeId,
                chiralityConvention.SignConvention,
                $"X-chirality undefined for odd dim(X)={baseDim}; trivial decomposition.");
        }
        else
        {
            var xChiMat = BuildPartialChiralityMatrix(gammas.GammaMatrices, 0, baseDim.Value);
            xChirality = AnalyzeWithMatrix(mode, xChiMat, chiralityConvention, layout, cellCount);
        }

        // F-chirality: partial from gammas [baseDim..dimY-1]
        ChiralityDecomposition? fChirality = null;
        if (baseDim is not null && fiberDim is not null && fiberDim > 0)
        {
            if (fiberDim % 2 != 0)
            {
                // Odd dimF — F-chirality undefined, leave null
            }
            else
            {
                var fChiMat = BuildPartialChiralityMatrix(gammas.GammaMatrices, baseDim.Value, dimY);
                fChirality = AnalyzeWithMatrix(mode, fChiMat, chiralityConvention, layout, cellCount);
            }
        }

        return new FermionChiralityRecord
        {
            FermionModeId = mode.ModeId,
            XChirality = xChirality,
            YChirality = yChirality,
            FChirality = fChirality,
        };
    }

    /// <summary>
    /// Analyze all modes and return FermionChiralityRecord per mode.
    /// </summary>
    public List<FermionChiralityRecord> AnalyzeTripleAll(
        IReadOnlyList<FermionModeRecord> modes,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        FermionFieldLayout layout,
        int cellCount)
    {
        ArgumentNullException.ThrowIfNull(modes);
        var results = new List<FermionChiralityRecord>(modes.Count);
        foreach (var mode in modes)
            results.Add(AnalyzeTriple(mode, gammas, chiralityConvention, layout, cellCount));
        return results;
    }

    /// <summary>
    /// Build chirality matrix from a contiguous slice of gamma matrices:
    /// Gamma_partial = i^(count/2) * gammas[start] * ... * gammas[end-1]
    /// where count = end - start must be even.
    /// </summary>
    private static System.Numerics.Complex[,] BuildPartialChiralityMatrix(
        System.Numerics.Complex[][,] allGammas, int start, int end)
    {
        int count = end - start;
        int spinorDim = allGammas[start].GetLength(0);
        int k = count / 2;
        var phase = ComplexIPower(k);

        // Identity matrix
        var result = new System.Numerics.Complex[spinorDim, spinorDim];
        for (int i = 0; i < spinorDim; i++) result[i, i] = System.Numerics.Complex.One;

        for (int mu = start; mu < end; mu++)
            result = MatMulLocal(result, allGammas[mu]);

        return ScaleMatrixLocal(phase, result);
    }

    /// <summary>
    /// Run the projection analysis using a pre-built chirality matrix.
    /// </summary>
    private ChiralityDecomposition AnalyzeWithMatrix(
        FermionModeRecord mode,
        System.Numerics.Complex[,] chiMat,
        ChiralityConventionSpec chiralityConvention,
        FermionFieldLayout layout,
        int cellCount)
    {
        if (mode.EigenvectorCoefficients is null)
        {
            return new ChiralityDecomposition
            {
                ModeId = mode.ModeId,
                LeftFraction = 0.0,
                RightFraction = 0.0,
                MixedFraction = 0.0,
                ChiralityTag = "not-applicable",
                ChiralityStatus = "not-applicable",
                LeakageDiagnostic = 0.0,
                SignConvention = chiralityConvention.SignConvention,
                DiagnosticNotes = new List<string> { "No eigenvector coefficients available." },
            };
        }

        var primalBlock = layout.SpinorBlocks.FirstOrDefault(b => b.Role == "primal")
            ?? throw new InvalidOperationException("Layout has no primal block.");
        int spinorDim = primalBlock.SpinorDimension;
        int dimG = primalBlock.GaugeDimension;
        int dofsPerCell = spinorDim * dimG;
        int phi_len = mode.EigenvectorCoefficients.Length;
        if (phi_len == 0 || phi_len % (dofsPerCell * 2) != 0)
            throw new ArgumentException(
                $"EigenvectorCoefficients length {phi_len} is not a multiple of " +
                $"dofsPerCell*2 = {dofsPerCell * 2}.");
        int nodeCount = phi_len / (dofsPerCell * 2);
        _ = cellCount;

        double signForLeft = chiralityConvention.SignConvention == "left-is-plus" ? +1.0 : -1.0;
        var phi = mode.EigenvectorCoefficients;

        double normPhiSq = 0.0, normPLphiSq = 0.0, normPRphiSq = 0.0;
        for (int c = 0; c < nodeCount; c++)
        {
            int cellBase = c * dofsPerCell * 2;
            for (int g = 0; g < dimG; g++)
            {
                var spinorSlice = new System.Numerics.Complex[spinorDim];
                for (int s = 0; s < spinorDim; s++)
                {
                    int idx = cellBase + (s * dimG + g) * 2;
                    spinorSlice[s] = new System.Numerics.Complex(phi[idx], phi[idx + 1]);
                }
                var plPhi = ApplyProjector(chiMat, spinorSlice, signForLeft, spinorDim);
                var prPhi = ApplyProjector(chiMat, spinorSlice, -signForLeft, spinorDim);
                for (int s = 0; s < spinorDim; s++)
                {
                    double re = spinorSlice[s].Real, im = spinorSlice[s].Imaginary;
                    normPhiSq += re * re + im * im;
                    double plRe = plPhi[s].Real, plIm = plPhi[s].Imaginary;
                    normPLphiSq += plRe * plRe + plIm * plIm;
                    double prRe = prPhi[s].Real, prIm = prPhi[s].Imaginary;
                    normPRphiSq += prRe * prRe + prIm * prIm;
                }
            }
        }

        if (normPhiSq < 1e-30)
        {
            return new ChiralityDecomposition
            {
                ModeId = mode.ModeId,
                LeftFraction = 0.0,
                RightFraction = 0.0,
                MixedFraction = 0.0,
                ChiralityTag = "not-applicable",
                ChiralityStatus = "not-applicable",
                LeakageDiagnostic = 0.0,
                SignConvention = chiralityConvention.SignConvention,
                DiagnosticNotes = new List<string> { "Zero norm eigenvector." },
            };
        }

        double leftFrac = normPLphiSq / normPhiSq;
        double rightFrac = normPRphiSq / normPhiSq;
        double leakage = System.Math.Abs(leftFrac + rightFrac - 1.0);
        double mixedFrac = System.Math.Max(0.0, 1.0 - leftFrac - rightFrac);

        string tag, status;
        if (leftFrac > DefiniteThreshold) { tag = "left"; status = "definite-left"; }
        else if (rightFrac > DefiniteThreshold) { tag = "right"; status = "definite-right"; }
        else { tag = "mixed"; status = "mixed"; }

        return new ChiralityDecomposition
        {
            ModeId = mode.ModeId,
            LeftFraction = leftFrac,
            RightFraction = rightFrac,
            MixedFraction = mixedFrac,
            ChiralityTag = tag,
            ChiralityStatus = status,
            LeakageDiagnostic = leakage,
            SignConvention = chiralityConvention.SignConvention,
        };
    }

    private static ChiralityDecomposition BuildTrivialDecomposition(
        string modeId, string signConvention, string note) =>
        new ChiralityDecomposition
        {
            ModeId = modeId,
            LeftFraction = 0.5,
            RightFraction = 0.5,
            MixedFraction = 0.0,
            ChiralityTag = "trivial",
            ChiralityStatus = "trivial",
            LeakageDiagnostic = 0.0,
            SignConvention = signConvention,
            DiagnosticNotes = new List<string> { note },
        };

    // i^k cycles: i^0=1, i^1=i, i^2=-1, i^3=-i
    private static System.Numerics.Complex ComplexIPower(int k)
    {
        return (((k % 4) + 4) % 4) switch
        {
            0 => System.Numerics.Complex.One,
            1 => System.Numerics.Complex.ImaginaryOne,
            2 => new System.Numerics.Complex(-1, 0),
            3 => new System.Numerics.Complex(0, -1),
            _ => System.Numerics.Complex.One,
        };
    }

    private static System.Numerics.Complex[,] MatMulLocal(
        System.Numerics.Complex[,] a, System.Numerics.Complex[,] b)
    {
        int n = a.GetLength(0);
        var result = new System.Numerics.Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                System.Numerics.Complex s = System.Numerics.Complex.Zero;
                for (int k = 0; k < n; k++)
                    s += a[i, k] * b[k, j];
                result[i, j] = s;
            }
        return result;
    }

    private static System.Numerics.Complex[,] ScaleMatrixLocal(
        System.Numerics.Complex scale, System.Numerics.Complex[,] m)
    {
        int n = m.GetLength(0);
        var result = new System.Numerics.Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i, j] = scale * m[i, j];
        return result;
    }

    /// <summary>
    /// Analyze all modes in a list.
    /// </summary>
    public IReadOnlyList<ChiralityDecomposition> AnalyzeAll(
        IReadOnlyList<FermionModeRecord> modes,
        GammaOperatorBundle gammas,
        ChiralityConventionSpec chiralityConvention,
        FermionFieldLayout layout,
        int cellCount)
    {
        ArgumentNullException.ThrowIfNull(modes);
        var results = new List<ChiralityDecomposition>(modes.Count);
        foreach (var mode in modes)
            results.Add(Analyze(mode, gammas, chiralityConvention, layout, cellCount));
        return results;
    }

    /// <summary>
    /// Apply projector P = (I + sign * Gamma_chi) / 2 to spinor vector.
    /// </summary>
    private static Complex[] ApplyProjector(
        Complex[,] chiMat,
        Complex[] spinor,
        double sign,
        int spinorDim)
    {
        var result = new Complex[spinorDim];
        for (int i = 0; i < spinorDim; i++)
        {
            // (I + sign * chiMat) spinor / 2
            Complex sum = spinor[i]; // identity contribution
            for (int j = 0; j < spinorDim; j++)
                sum += sign * chiMat[i, j] * spinor[j];
            result[i] = sum * 0.5;
        }
        return result;
    }
}
