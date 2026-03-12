using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Fermionic spectral solver for D_h phi_i = lambda_i phi_i (M_psi = I).
///
/// Requires the DiracOperatorBundle to have an explicit matrix assembled
/// (HasExplicitMatrix = true). Uses a self-contained Lanczos iteration
/// on D^T*D to find the modes with smallest |eigenvalue|.
///
/// Returns modes sorted by ascending |lambda_i|.
///
/// M38 extension: accepts an optional chiralityPostProcessor delegate that maps
/// the raw mode array to modes with chirality decompositions applied.
/// This allows higher-level projects (e.g. Gu.Phase4.Chirality-based pipeline)
/// to inject real chirality analysis without creating a circular dependency.
/// </summary>
public sealed class FermionSpectralSolver
{
    private readonly IDiracOperatorAssembler _assembler;

    public FermionSpectralSolver(IDiracOperatorAssembler assembler)
    {
        ArgumentNullException.ThrowIfNull(assembler);
        _assembler = assembler;
    }

    /// <summary>
    /// Solve the fermionic spectral problem.
    ///
    /// The optional <paramref name="chiralityPostProcessor"/> is called after the raw
    /// modes are computed. It should return a new array with ChiralityDecomposition and
    /// ConjugationPairing populated using real M37 analysis. If null, modes retain the
    /// "no-chirality-analysis" placeholder convention.
    /// </summary>
    public FermionSpectralResult Solve(
        DiracOperatorBundle diracBundle,
        FermionFieldLayout layout,
        FermionSpectralConfig config,
        ProvenanceMeta provenance,
        Func<FermionModeRecord[], FermionModeRecord[]>? chiralityPostProcessor = null)
    {
        ArgumentNullException.ThrowIfNull(diracBundle);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(provenance);

        if (!diracBundle.HasExplicitMatrix || diracBundle.ExplicitMatrix is null)
            throw new InvalidOperationException(
                "FermionSpectralSolver requires DiracOperatorBundle with HasExplicitMatrix=true.");

        int totalDof = diracBundle.TotalDof;

        int realDim = 2 * totalDof;

        // Validate MassPsiWeights length if provided
        double[]? massPsiWeights = config.MassPsiWeights;
        if (massPsiWeights is not null && massPsiWeights.Length != realDim)
            throw new ArgumentException(
                $"MassPsiWeights length {massPsiWeights.Length} does not match " +
                $"expected realDim {realDim} (totalDof={totalDof}).");

        // Build D^T * D (symmetric positive semi-definite)
        var DtD = BuildDtD(diracBundle.ExplicitMatrix, totalDof);

        // Cap nModes at realDim since that is the actual number of real DOFs.
        int nModes = System.Math.Min(config.ModeCount, realDim);
        var (lambdaSq, vecs, iters) = LanczosSmallest(DtD, totalDof, nModes,
            config.ConvergenceTolerance, config.MaxIterations, config.Seed,
            massPsiWeights);

        bool converged = iters < config.MaxIterations;
        var notes = new List<string>();
        if (!converged)
            notes.Add($"Reached max iterations ({config.MaxIterations}).");

        int actualModes = lambdaSq.Length;

        // Build preliminary mode records
        var modeRecords = new FermionModeRecord[actualModes];
        for (int i = 0; i < actualModes; i++)
        {
            double lambda = System.Math.Sqrt(System.Math.Max(lambdaSq[i], 0.0));

            // Determine sign from <v, D*v>
            double[] v = vecs[i];
            double[] Dv = ApplyD(diracBundle.ExplicitMatrix, totalDof, v);
            double vDv = 0;
            for (int k = 0; k < totalDof * 2; k++) vDv += v[k] * Dv[k];
            double evRe = vDv < 0 ? -lambda : lambda;

            double residualNorm = ComputeResidual(diracBundle.ExplicitMatrix, totalDof, v, evRe);

            modeRecords[i] = new FermionModeRecord
            {
                ModeId = $"mode-{diracBundle.FermionBackgroundId}-{i:D3}",
                BackgroundId = diracBundle.FermionBackgroundId,
                BranchVariantId = "default",
                LayoutId = diracBundle.LayoutId,
                ModeIndex = i,
                EigenvalueRe = evRe,
                EigenvalueIm = 0.0,
                ResidualNorm = residualNorm,
                EigenvectorCoefficients = v,
                ChiralityDecomposition = new ChiralityDecompositionRecord
                {
                    LeftFraction = 0.5,
                    RightFraction = 0.5,
                    MixedFraction = 0.0,
                    SignConvention = "no-chirality-analysis",
                },
                ConjugationPairing = new ConjugationPairingRecord
                {
                    HasPair = false,
                    ConjugationType = "hermitian",
                },
                GaugeLeakScore = 0.0,
                GaugeReductionApplied = false,
                Backend = "cpu-reference",
                ComputedWithUnverifiedGpu = false,
                BranchStabilityScore = 0.0,
                RefinementStabilityScore = 0.0,
                ReplayTier = "R0",
                Provenance = provenance,
            };
        }

        // Apply chirality/conjugation post-processing (M37 wiring injected from caller)
        if (chiralityPostProcessor is not null)
            modeRecords = chiralityPostProcessor(modeRecords);

        // Sort by ascending |eigenvalue| and re-index
        var sortedModes = modeRecords
            .OrderBy(m => m.EigenvalueMagnitude)
            .ToArray();

        var finalModes = new FermionModeRecord[sortedModes.Length];
        for (int i = 0; i < sortedModes.Length; i++)
            finalModes[i] = WithModeIndex(sortedModes[i], i);

        double maxRes = finalModes.Length > 0 ? finalModes.Max(m => m.ResidualNorm) : 0.0;
        double meanRes = finalModes.Length > 0 ? finalModes.Average(m => m.ResidualNorm) : 0.0;

        var obsSummary = BuildObservationSummary(finalModes, diracBundle.FermionBackgroundId);

        return new FermionSpectralResult
        {
            ResultId = $"fermion-spectral-{diracBundle.FermionBackgroundId}",
            FermionBackgroundId = diracBundle.FermionBackgroundId,
            OperatorId = diracBundle.OperatorId,
            Modes = new List<FermionModeRecord>(finalModes),
            Diagnostics = new FermionSpectralDiagnostics
            {
                SolverName = massPsiWeights is not null
                    ? "cpu-lanczos-dtd-mpsi-v1"
                    : "cpu-lanczos-dtd-v1",
                Iterations = iters,
                Converged = converged,
                MaxResidual = maxRes,
                MeanResidual = meanRes,
                Notes = notes,
            },
            Provenance = provenance,
            ObservationSummary = obsSummary,
        };
    }

    private static FermionObservationSummary BuildObservationSummary(
        FermionModeRecord[] modes, string backgroundId)
    {
        int leftCount = 0, rightCount = 0, mixedCount = 0;
        const double threshold = 0.9;
        var pairedKeys = new HashSet<string>();

        foreach (var m in modes)
        {
            double left = m.ChiralityDecomposition.LeftFraction;
            double right = m.ChiralityDecomposition.RightFraction;
            if (left > threshold) leftCount++;
            else if (right > threshold) rightCount++;
            else mixedCount++;

            if (m.ConjugationPairing.HasPair && m.ConjugationPairing.PartnerModeId is { } partnerId)
            {
                string key = string.CompareOrdinal(m.ModeId, partnerId) < 0
                    ? $"{m.ModeId}:{partnerId}"
                    : $"{partnerId}:{m.ModeId}";
                pairedKeys.Add(key);
            }
        }

        return new FermionObservationSummary
        {
            BackgroundId = backgroundId,
            TotalModes = modes.Length,
            LeftChiralCount = leftCount,
            RightChiralCount = rightCount,
            MixedOrTrivialCount = mixedCount,
            ConjugationPairCount = pairedKeys.Count,
        };
    }

    // --- Linear algebra helpers ---

    private static double[] BuildDtD(double[] D, int totalDof)
    {
        // D is complex totalDof x totalDof stored as [i*totalDof+j][re/im]
        // Returned as flat array of length (2*totalDof)^2 (real representation).
        int realDim = 2 * totalDof;
        var DtD = new double[realDim * realDim];

        for (int i = 0; i < totalDof; i++)
            for (int j = 0; j < totalDof; j++)
            {
                double re = 0, im = 0;
                for (int k = 0; k < totalDof; k++)
                {
                    // D†[i,k] = conj(D[k,i])
                    double dkiRe = D[(k * totalDof + i) * 2];
                    double dkiIm = -D[(k * totalDof + i) * 2 + 1];
                    double dkjRe = D[(k * totalDof + j) * 2];
                    double dkjIm = D[(k * totalDof + j) * 2 + 1];
                    re += dkiRe * dkjRe - dkiIm * dkjIm;
                    im += dkiRe * dkjIm + dkiIm * dkjRe;
                }
                // Store in real (2*totalDof) x (2*totalDof) representation
                DtD[(2 * i) * realDim + 2 * j]         = re;
                DtD[(2 * i) * realDim + 2 * j + 1]     = -im;
                DtD[(2 * i + 1) * realDim + 2 * j]     = im;
                DtD[(2 * i + 1) * realDim + 2 * j + 1] = re;
            }

        return DtD;
    }

    private static double[] ApplyD(double[] D, int totalDof, double[] v)
    {
        // D is complex stored as flat array; v is flat real array of length 2*totalDof
        int realDim = 2 * totalDof;
        var result = new double[realDim];
        for (int i = 0; i < totalDof; i++)
        {
            double re = 0, im = 0;
            for (int j = 0; j < totalDof; j++)
            {
                double dRe = D[(i * totalDof + j) * 2];
                double dIm = D[(i * totalDof + j) * 2 + 1];
                double vRe = v[j * 2];
                double vIm = v[j * 2 + 1];
                re += dRe * vRe - dIm * vIm;
                im += dRe * vIm + dIm * vRe;
            }
            result[i * 2] = re;
            result[i * 2 + 1] = im;
        }
        return result;
    }

    private static double ComputeResidual(double[] D, int totalDof, double[] v, double evRe)
    {
        var Dv = ApplyD(D, totalDof, v);
        int n = 2 * totalDof;
        double sumSq = 0, normSq = 0;
        for (int i = 0; i < n; i++)
        {
            double r = Dv[i] - evRe * v[i];
            sumSq += r * r;
            normSq += v[i] * v[i];
        }
        double vNorm = System.Math.Sqrt(normSq);
        return vNorm > 1e-14 ? System.Math.Sqrt(sumSq) / vNorm : System.Math.Sqrt(sumSq);
    }

    private static (double[], double[][], int) LanczosSmallest(
        double[] A, int origDim, int nModes, double tol, int maxIter, int seed,
        double[]? massPsiWeights = null)
    {
        // massPsiWeights[k] are diagonal entries of M_psi (real representation).
        // When provided: apply M_psi^{-1} to the Lanczos residual at each step,
        // equivalent to solving M_psi^{-1} * A * v = lambda * v (standard eigenvalue problem
        // with eigenvalues equal to those of the generalized problem A v = lambda M_psi v).
        int realDim = 2 * origDim; // A is (2*totalDof)^2
        int krylov = System.Math.Min(System.Math.Max(2 * nModes + 4, 20), realDim);
        var rng = new Random(seed);

        var Q = new double[krylov][];
        var alpha = new double[krylov];
        var beta = new double[krylov + 1];

        // Random start
        var q0 = new double[realDim];
        for (int i = 0; i < realDim; i++) q0[i] = rng.NextDouble() - 0.5;
        Normalize(q0, realDim);
        Q[0] = q0;

        int actual = krylov;
        int totalIter = 0;

        for (int j = 0; j < krylov; j++)
        {
            totalIter++;
            var z = MatVec(A, realDim, Q[j]);
            // Apply M_psi^{-1}: z[i] /= massPsiWeights[i]
            if (massPsiWeights is not null)
                for (int i = 0; i < realDim; i++) z[i] /= massPsiWeights[i];

            if (j > 0)
                for (int i = 0; i < realDim; i++) z[i] -= beta[j] * Q[j - 1][i];

            alpha[j] = Dot(Q[j], z, realDim);
            for (int i = 0; i < realDim; i++) z[i] -= alpha[j] * Q[j][i];

            if (j < krylov - 1)
            {
                beta[j + 1] = Norm(z, realDim);
                if (beta[j + 1] < tol) { actual = j + 1; break; }
                Q[j + 1] = new double[realDim];
                for (int i = 0; i < realDim; i++) Q[j + 1][i] = z[i] / beta[j + 1];
            }
        }

        // Solve tridiagonal eigenvalue problem (size actual)
        var (triEvals, triEvecs) = SolveTridiagonal(alpha, beta, actual);

        // Find nModes smallest
        int take = System.Math.Min(nModes, actual);
        var order = Enumerable.Range(0, actual)
            .OrderBy(i => System.Math.Abs(triEvals[i]))
            .Take(take)
            .ToArray();

        var eigenvalues = new double[take];
        var eigenvectors = new double[take][];

        for (int m = 0; m < take; m++)
        {
            int idx = order[m];
            eigenvalues[m] = System.Math.Max(triEvals[idx], 0.0);

            var v = new double[realDim];
            for (int j = 0; j < actual && Q[j] != null; j++)
            {
                double c = triEvecs[j][idx];
                for (int i = 0; i < realDim; i++) v[i] += c * Q[j][i];
            }
            Normalize(v, realDim);
            eigenvectors[m] = v;
        }

        return (eigenvalues, eigenvectors, totalIter);
    }

    private static (double[], double[][]) SolveTridiagonal(double[] diag, double[] offDiag, int n)
    {
        var d = new double[n];
        var e = new double[n + 1]; // extra slot so e[i+2] is safe when m = n-1
        for (int i = 0; i < n; i++) d[i] = diag[i];
        for (int i = 1; i < n; i++) e[i] = offDiag[i];

        var z = new double[n, n];
        for (int i = 0; i < n; i++) z[i, i] = 1.0;

        const int maxIter = 200;
        for (int l = 0; l < n; l++)
        {
            int iter = 0;
            int m;
            do
            {
                for (m = l; m < n - 1; m++)
                    if (System.Math.Abs(e[m + 1]) <= 1e-14 * (System.Math.Abs(d[m]) + System.Math.Abs(d[m + 1])))
                        break;
                if (m == l) break;
                if (++iter > maxIter) break;

                double g = (d[l + 1] - d[l]) / (2.0 * e[l + 1]);
                double r = System.Math.Sqrt(g * g + 1.0);
                g = d[m] - d[l] + e[l + 1] / (g + (g >= 0 ? r : -r));

                double s = 1.0, c2 = 1.0, p = 0.0;
                for (int i = m - 1; i >= l; i--)
                {
                    double f = s * e[i + 1];
                    double b2 = c2 * e[i + 1];
                    r = System.Math.Sqrt(f * f + g * g);
                    if (i + 2 < n) e[i + 2] = r;
                    if (r < 1e-14) { d[i + 1] -= p; if (m + 1 < n) e[m + 1] = 0.0; break; }
                    s = f / r; c2 = g / r;
                    double dl = d[i + 1]; g = dl - p; r = (d[i] - g) * s + 2.0 * c2 * b2;
                    p = s * r; d[i + 1] = g + p; g = c2 * r - b2;
                    for (int k = 0; k < n; k++)
                    {
                        double tz = z[k, i + 1];
                        z[k, i + 1] = s * z[k, i] + c2 * tz;
                        z[k, i] = c2 * z[k, i] - s * tz;
                    }
                }
                if (l + 1 < n) e[l + 1] = g;
                if (m + 1 < n) e[m + 1] = 0.0;
                d[l] -= p;
            } while (m != l);
        }

        var evecs = new double[n][];
        for (int i = 0; i < n; i++)
        {
            evecs[i] = new double[n];
            for (int j = 0; j < n; j++) evecs[i][j] = z[j, i];
        }
        return (d, evecs);
    }

    private static double[] MatVec(double[] A, int n, double[] v)
    {
        var result = new double[n];
        for (int i = 0; i < n; i++)
        {
            double s = 0;
            for (int j = 0; j < n; j++) s += A[i * n + j] * v[j];
            result[i] = s;
        }
        return result;
    }

    private static double Dot(double[] a, double[] b, int n)
    {
        double s = 0;
        for (int i = 0; i < n; i++) s += a[i] * b[i];
        return s;
    }

    private static double Norm(double[] v, int n)
    {
        double s = 0;
        for (int i = 0; i < n; i++) s += v[i] * v[i];
        return System.Math.Sqrt(s);
    }

    private static void Normalize(double[] v, int n)
    {
        double nm = Norm(v, n);
        if (nm > 1e-14)
            for (int i = 0; i < n; i++) v[i] /= nm;
    }

    private static FermionModeRecord WithModeIndex(FermionModeRecord m, int idx) =>
        new FermionModeRecord
        {
            ModeId = m.ModeId,
            BackgroundId = m.BackgroundId,
            BranchVariantId = m.BranchVariantId,
            LayoutId = m.LayoutId,
            ModeIndex = idx,
            EigenvalueRe = m.EigenvalueRe,
            EigenvalueIm = m.EigenvalueIm,
            ResidualNorm = m.ResidualNorm,
            EigenvectorCoefficients = m.EigenvectorCoefficients,
            ChiralityDecomposition = m.ChiralityDecomposition,
            ConjugationPairing = m.ConjugationPairing,
            GaugeLeakScore = m.GaugeLeakScore,
            GaugeReductionApplied = m.GaugeReductionApplied,
            Backend = m.Backend,
            ComputedWithUnverifiedGpu = m.ComputedWithUnverifiedGpu,
            BranchStabilityScore = m.BranchStabilityScore,
            RefinementStabilityScore = m.RefinementStabilityScore,
            ReplayTier = m.ReplayTier,
            AmbiguityNotes = m.AmbiguityNotes,
            Provenance = m.Provenance,
        };
}
