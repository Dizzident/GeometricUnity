using System.Numerics;
using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Fermionic spectral solver for K psi_i = lambda_i M_psi psi_i.
///
/// Requires the DiracOperatorBundle to have an explicit matrix assembled
/// (HasExplicitMatrix = true). The persisted Euclidean-Hermitian matrix is
/// treated as the stiffness/action matrix K. Weighted solves form the
/// Euclidean-Hermitian representative B = M_psi^-1/2 K M_psi^-1/2.
///
/// Uses a bounded dense Hermitian eigensolve appropriate to the current
/// explicit reference matrices. This is not a production-scale spectral path.
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
    private const int MaxDenseComplexDimension = 512;
    private const double DefaultHermiticityTolerance = 1e-10;

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
        ValidateSolveInputs(diracBundle.ExplicitMatrix, totalDof, config);

        double[]? massPsiWeights = config.MassPsiWeights;
        bool isWeighted = massPsiWeights is not null;
        double[] complexMassWeights = ValidateMassPsiWeights(massPsiWeights, totalDof);

        var stiffnessMatrix = BuildComplexMatrix(diracBundle.ExplicitMatrix, totalDof);
        double hermiticityTolerance =
            double.IsFinite(diracBundle.HermiticityTolerance) && diracBundle.HermiticityTolerance > 0.0
                ? System.Math.Max(diracBundle.HermiticityTolerance, DefaultHermiticityTolerance)
                : DefaultHermiticityTolerance;
        ValidateAndSymmetrizeHermitian(stiffnessMatrix, totalDof, hermiticityTolerance, "K");

        var hermitianRepresentative = BuildHermitianRepresentative(
            stiffnessMatrix, complexMassWeights, totalDof);
        ValidateAndSymmetrizeHermitian(
            hermitianRepresentative, totalDof, hermiticityTolerance, isWeighted ? "B" : "K");

        var (eigenvalues, representativeVectors, iters, converged) = SolveDenseHermitian(
            hermitianRepresentative,
            totalDof,
            config.ConvergenceTolerance,
            config.MaxIterations);

        int nModes = System.Math.Min(config.ModeCount, totalDof);
        var selectedIndices = Enumerable.Range(0, eigenvalues.Length)
            .OrderBy(index => System.Math.Abs(eigenvalues[index]))
            .Take(nModes)
            .ToArray();

        var notes = new List<string>();
        if (!converged)
            notes.Add($"Reached max dense Jacobi sweeps ({config.MaxIterations}).");
        notes.Add(isWeighted
            ? "Solved B=M_psi^-1/2 K M_psi^-1/2, then returned psi=M_psi^-1/2 u with M_psi normalization."
            : "Solved the persisted Euclidean-Hermitian stiffness/action matrix K directly with M_psi=I.");
        notes.Add("Residuals are ||K psi - lambda M_psi psi||_2 / ||psi||_Mpsi in the original stiffness variables.");
        notes.Add(
            $"Dense Hermitian Jacobi reference path is bounded to totalDof <= {MaxDenseComplexDimension}; " +
            "it is intended for current explicit matrices, not production-scale spectra.");

        int actualModes = selectedIndices.Length;

        // Build preliminary mode records
        var modeRecords = new FermionModeRecord[actualModes];
        for (int i = 0; i < actualModes; i++)
        {
            int selectedIndex = selectedIndices[i];
            double eigenvalue = eigenvalues[selectedIndex];
            Complex[] psi = BackTransformAndNormalize(
                representativeVectors[selectedIndex], complexMassWeights);
            double residualNorm = ComputeGeneralizedResidual(
                stiffnessMatrix, psi, eigenvalue, complexMassWeights, totalDof);

            modeRecords[i] = new FermionModeRecord
            {
                ModeId = $"mode-{diracBundle.FermionBackgroundId}-{i:D3}",
                BackgroundId = diracBundle.FermionBackgroundId,
                BranchVariantId = "default",
                LayoutId = diracBundle.LayoutId,
                ModeIndex = i,
                EigenvalueRe = eigenvalue,
                EigenvalueIm = 0.0,
                ResidualNorm = residualNorm,
                EigenvectorCoefficients = ToInterleavedReal(psi),
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
                GaugeReductionApplied = diracBundle.GaugeReductionApplied,
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
                SolverName = isWeighted
                    ? "cpu-dense-hermitian-b-mpsi-v2"
                    : "cpu-dense-hermitian-k-v2",
                Iterations = iters,
                Converged = converged,
                MaxResidual = maxRes,
                MeanResidual = meanRes,
                GaugeReductionApplied = diracBundle.GaugeReductionApplied,
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

    private static void ValidateSolveInputs(
        double[] explicitMatrix,
        int totalDof,
        FermionSpectralConfig config)
    {
        if (totalDof <= 0)
            throw new ArgumentException("DiracOperatorBundle TotalDof must be positive.");
        if (totalDof > MaxDenseComplexDimension)
            throw new InvalidOperationException(
                $"FermionSpectralSolver dense Hermitian reference path is bounded to " +
                $"totalDof <= {MaxDenseComplexDimension}; received {totalDof}. " +
                "A separate scalable Hermitian eigensolver is required for larger explicit matrices.");

        int expectedLength = checked(totalDof * totalDof * 2);
        if (explicitMatrix.Length != expectedLength)
            throw new ArgumentException(
                $"ExplicitMatrix length {explicitMatrix.Length} does not match expected " +
                $"{expectedLength} for totalDof={totalDof}.");

        if (config.ModeCount < 0)
            throw new ArgumentOutOfRangeException(nameof(config), "ModeCount must be non-negative.");
        if (!double.IsFinite(config.ConvergenceTolerance) || config.ConvergenceTolerance <= 0.0)
            throw new ArgumentOutOfRangeException(
                nameof(config), "ConvergenceTolerance must be finite and positive.");
        if (config.MaxIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(config), "MaxIterations must be positive.");
    }

    private static double[] ValidateMassPsiWeights(double[]? weights, int totalDof)
    {
        int realDim = 2 * totalDof;
        var complexWeights = new double[totalDof];
        if (weights is null)
        {
            Array.Fill(complexWeights, 1.0);
            return complexWeights;
        }

        if (weights.Length != realDim)
            throw new ArgumentException(
                $"MassPsiWeights length {weights.Length} does not match " +
                $"expected realDim {realDim} (totalDof={totalDof}).");

        for (int i = 0; i < totalDof; i++)
        {
            double realWeight = weights[2 * i];
            double imaginaryWeight = weights[2 * i + 1];
            if (!double.IsFinite(realWeight) || realWeight <= 0.0)
                throw new ArgumentException(
                    $"MassPsiWeights[{2 * i}]={realWeight} must be finite and positive.",
                    nameof(weights));
            if (!double.IsFinite(imaginaryWeight) || imaginaryWeight <= 0.0)
                throw new ArgumentException(
                    $"MassPsiWeights[{2 * i + 1}]={imaginaryWeight} must be finite and positive.",
                    nameof(weights));
            if (realWeight != imaginaryWeight)
                throw new ArgumentException(
                    $"MassPsiWeights real/imaginary pair at complex DOF {i} must match; " +
                    $"received {realWeight} and {imaginaryWeight}.",
                    nameof(weights));

            complexWeights[i] = realWeight;
        }

        return complexWeights;
    }

    private static Complex[] BuildComplexMatrix(double[] interleavedMatrix, int n)
    {
        var matrix = new Complex[n * n];
        for (int i = 0; i < matrix.Length; i++)
        {
            double real = interleavedMatrix[2 * i];
            double imaginary = interleavedMatrix[2 * i + 1];
            if (!double.IsFinite(real) || !double.IsFinite(imaginary))
                throw new ArgumentException(
                    $"ExplicitMatrix complex entry {i} must contain finite coefficients.");
            matrix[i] = new Complex(real, imaginary);
        }
        return matrix;
    }

    private static void ValidateAndSymmetrizeHermitian(
        Complex[] matrix,
        int n,
        double tolerance,
        string representationName)
    {
        double normSquared = 0.0;
        double residualSquared = 0.0;
        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                Complex entry = matrix[row * n + col];
                Complex difference = entry - Complex.Conjugate(matrix[col * n + row]);
                normSquared += entry.Magnitude * entry.Magnitude;
                residualSquared += difference.Magnitude * difference.Magnitude;
            }
        }

        double relativeResidual =
            System.Math.Sqrt(residualSquared / System.Math.Max(normSquared, 1e-300));
        if (relativeResidual > tolerance)
            throw new InvalidOperationException(
                $"FermionSpectralSolver requires Euclidean-Hermitian {representationName}; " +
                $"relative residual {relativeResidual:E6} exceeds tolerance {tolerance:E6}.");

        for (int row = 0; row < n; row++)
        {
            matrix[row * n + row] = new Complex(matrix[row * n + row].Real, 0.0);
            for (int col = row + 1; col < n; col++)
            {
                Complex average = 0.5 * (
                    matrix[row * n + col] +
                    Complex.Conjugate(matrix[col * n + row]));
                matrix[row * n + col] = average;
                matrix[col * n + row] = Complex.Conjugate(average);
            }
        }
    }

    private static Complex[] BuildHermitianRepresentative(
        Complex[] stiffnessMatrix,
        double[] massWeights,
        int n)
    {
        var representative = new Complex[stiffnessMatrix.Length];
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
                representative[row * n + col] =
                    stiffnessMatrix[row * n + col] /
                    System.Math.Sqrt(massWeights[row] * massWeights[col]);
        return representative;
    }

    private static (double[] Eigenvalues, Complex[][] Eigenvectors, int Sweeps, bool Converged)
        SolveDenseHermitian(
            Complex[] hermitianMatrix,
            int n,
            double tolerance,
            int maxSweeps)
    {
        var matrix = (Complex[])hermitianMatrix.Clone();
        var vectors = new Complex[n * n];
        for (int i = 0; i < n; i++)
            vectors[i * n + i] = Complex.One;

        double matrixNorm = FrobeniusNorm(matrix);
        double relativeTolerance = System.Math.Min(
            System.Math.Max(tolerance, 1e-14),
            1e-12);
        double convergenceThreshold = relativeTolerance * System.Math.Max(matrixNorm, 1e-300);
        double rotationThreshold = convergenceThreshold / System.Math.Max(1, n);

        bool converged = false;
        int sweeps;
        for (sweeps = 1; sweeps <= maxSweeps; sweeps++)
        {
            if (UpperOffDiagonalNorm(matrix, n) <= convergenceThreshold)
            {
                converged = true;
                break;
            }

            for (int row = 0; row < n; row++)
                for (int col = row + 1; col < n; col++)
                    if (matrix[row * n + col].Magnitude > rotationThreshold)
                        ApplyHermitianJacobiRotation(matrix, vectors, n, row, col);
        }

        if (!converged)
            converged = UpperOffDiagonalNorm(matrix, n) <= convergenceThreshold;

        var eigenvalues = new double[n];
        var eigenvectors = new Complex[n][];
        for (int i = 0; i < n; i++)
        {
            eigenvalues[i] = matrix[i * n + i].Real;
            eigenvectors[i] = new Complex[n];
            for (int row = 0; row < n; row++)
                eigenvectors[i][row] = vectors[row * n + i];
        }

        return (eigenvalues, eigenvectors, System.Math.Min(sweeps, maxSweeps), converged);
    }

    private static void ApplyHermitianJacobiRotation(
        Complex[] matrix,
        Complex[] vectors,
        int n,
        int p,
        int q)
    {
        Complex apq = matrix[p * n + q];
        double magnitude = apq.Magnitude;
        if (magnitude == 0.0)
            return;

        Complex phase = Complex.Conjugate(apq) / magnitude;
        for (int row = 0; row < n; row++)
        {
            if (row != q)
            {
                matrix[row * n + q] *= phase;
                matrix[q * n + row] = Complex.Conjugate(matrix[row * n + q]);
            }
            vectors[row * n + q] *= phase;
        }

        double app = matrix[p * n + p].Real;
        double aqq = matrix[q * n + q].Real;
        double tau = (aqq - app) / (2.0 * magnitude);
        double t = tau >= 0.0
            ? 1.0 / (tau + System.Math.Sqrt(1.0 + tau * tau))
            : -1.0 / (-tau + System.Math.Sqrt(1.0 + tau * tau));
        double cosine = 1.0 / System.Math.Sqrt(1.0 + t * t);
        double sine = t * cosine;

        matrix[p * n + p] = new Complex(app - t * magnitude, 0.0);
        matrix[q * n + q] = new Complex(aqq + t * magnitude, 0.0);
        matrix[p * n + q] = Complex.Zero;
        matrix[q * n + p] = Complex.Zero;

        for (int row = 0; row < n; row++)
        {
            if (row == p || row == q)
                continue;

            Complex arp = matrix[row * n + p];
            Complex arq = matrix[row * n + q];
            matrix[row * n + p] = cosine * arp - sine * arq;
            matrix[p * n + row] = Complex.Conjugate(matrix[row * n + p]);
            matrix[row * n + q] = sine * arp + cosine * arq;
            matrix[q * n + row] = Complex.Conjugate(matrix[row * n + q]);
        }

        for (int row = 0; row < n; row++)
        {
            Complex vrp = vectors[row * n + p];
            Complex vrq = vectors[row * n + q];
            vectors[row * n + p] = cosine * vrp - sine * vrq;
            vectors[row * n + q] = sine * vrp + cosine * vrq;
        }
    }

    private static double FrobeniusNorm(Complex[] matrix)
    {
        double sumSquared = 0.0;
        foreach (Complex entry in matrix)
            sumSquared += entry.Magnitude * entry.Magnitude;
        return System.Math.Sqrt(sumSquared);
    }

    private static double UpperOffDiagonalNorm(Complex[] matrix, int n)
    {
        double sumSquared = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = row + 1; col < n; col++)
            {
                double magnitude = matrix[row * n + col].Magnitude;
                sumSquared += magnitude * magnitude;
            }
        return System.Math.Sqrt(sumSquared);
    }

    private static Complex[] BackTransformAndNormalize(
        Complex[] representativeVector,
        double[] massWeights)
    {
        var psi = new Complex[representativeVector.Length];
        for (int i = 0; i < psi.Length; i++)
            psi[i] = representativeVector[i] / System.Math.Sqrt(massWeights[i]);

        double normSquared = 0.0;
        for (int i = 0; i < psi.Length; i++)
            normSquared += massWeights[i] * psi[i].Magnitude * psi[i].Magnitude;

        double norm = System.Math.Sqrt(normSquared);
        if (norm > 1e-14)
            for (int i = 0; i < psi.Length; i++)
                psi[i] /= norm;
        return psi;
    }

    private static double ComputeGeneralizedResidual(
        Complex[] stiffnessMatrix,
        Complex[] psi,
        double eigenvalue,
        double[] massWeights,
        int n)
    {
        Complex[] kPsi = MatVec(stiffnessMatrix, psi, n);
        double residualSquared = 0.0;
        double mNormSquared = 0.0;
        for (int i = 0; i < n; i++)
        {
            Complex residual = kPsi[i] - eigenvalue * massWeights[i] * psi[i];
            residualSquared += residual.Magnitude * residual.Magnitude;
            mNormSquared += massWeights[i] * psi[i].Magnitude * psi[i].Magnitude;
        }

        double mNorm = System.Math.Sqrt(mNormSquared);
        return mNorm > 1e-14
            ? System.Math.Sqrt(residualSquared) / mNorm
            : System.Math.Sqrt(residualSquared);
    }

    private static Complex[] MatVec(Complex[] matrix, Complex[] vector, int n)
    {
        var result = new Complex[n];
        for (int row = 0; row < n; row++)
            for (int col = 0; col < n; col++)
                result[row] += matrix[row * n + col] * vector[col];
        return result;
    }

    private static double[] ToInterleavedReal(Complex[] vector)
    {
        var interleaved = new double[2 * vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            interleaved[2 * i] = vector[i].Real;
            interleaved[2 * i + 1] = vector[i].Imaginary;
        }
        return interleaved;
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
