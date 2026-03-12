using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// CPU/GPU parity checker for Phase IV Dirac operator kernels.
///
/// Mirrors SpectralParityChecker (Phase III) for Phase IV fermionic operators.
/// Uses error formula: |a-b| / (1 + |a|) per component (same as SpectralParityChecker).
///
/// Applied element-wise to flat interleaved double arrays — no special handling
/// needed for complex data (real and imaginary parts are checked independently).
/// </summary>
public sealed class DiracParityChecker
{
    // Tolerances confirmed by cuda-guy and architect
    public const double GammaTolerance = 1e-10;
    public const double DiracTolerance = 1e-8;
    public const double MassTolerance = 1e-12;
    public const double ChiralityTolerance = 1e-10;
    public const double CouplingTolerance = 1e-8;

    /// <summary>Result of a single parity check operation.</summary>
    public sealed class ParityResult
    {
        public required string OperationName { get; init; }
        public required bool Passed { get; init; }
        public required double MaxRelativeError { get; init; }
        public required double Tolerance { get; init; }
        public required int ComponentsCompared { get; init; }
        public required int MaxErrorIndex { get; init; }
    }

    /// <summary>Full parity report across all tested operations.</summary>
    public sealed class ParityReport
    {
        public required List<ParityResult> Results { get; init; }
        public bool AllPassed => Results.All(r => r.Passed);
        public double WorstError => Results.Count > 0 ? Results.Max(r => r.MaxRelativeError) : 0.0;
    }

    /// <summary>
    /// Compare two span outputs and compute maximum relative error.
    /// Error formula: |a-b| / (1 + |a|) per component.
    /// </summary>
    public static ParityResult Compare(
        string operationName,
        ReadOnlySpan<double> cpuOutput,
        ReadOnlySpan<double> gpuOutput,
        double tolerance)
    {
        if (cpuOutput.Length != gpuOutput.Length)
            throw new ArgumentException(
                $"Output lengths differ: CPU={cpuOutput.Length}, GPU={gpuOutput.Length}");

        double maxRelErr = 0.0;
        int maxIdx = 0;
        for (int i = 0; i < cpuOutput.Length; i++)
        {
            double denom = 1.0 + System.Math.Abs(cpuOutput[i]);
            double relErr = System.Math.Abs(cpuOutput[i] - gpuOutput[i]) / denom;
            if (relErr > maxRelErr)
            {
                maxRelErr = relErr;
                maxIdx = i;
            }
        }

        return new ParityResult
        {
            OperationName = operationName,
            Passed = maxRelErr < tolerance,
            MaxRelativeError = maxRelErr,
            Tolerance = tolerance,
            ComponentsCompared = cpuOutput.Length,
            MaxErrorIndex = maxIdx,
        };
    }

    /// <summary>
    /// Compare two complex scalar coupling proxies.
    /// Real and imaginary parts are checked separately.
    /// </summary>
    public static (ParityResult Re, ParityResult Im) CompareCoupling(
        string operationName,
        (double Real, double Imag) cpuResult,
        (double Real, double Imag) gpuResult,
        double tolerance)
    {
        var reResult = Compare(
            operationName + "[Re]",
            new[] { cpuResult.Real },
            new[] { gpuResult.Real },
            tolerance);
        var imResult = Compare(
            operationName + "[Im]",
            new[] { cpuResult.Imag },
            new[] { gpuResult.Imag },
            tolerance);
        return (reResult, imResult);
    }

    /// <summary>
    /// Run a full CPU/GPU parity check across all IDiracKernel operations.
    ///
    /// Tests:
    ///   - ApplyGamma[mu] for all mu in [0, dimY)
    ///   - ApplyDirac
    ///   - ApplyMass
    ///   - ApplyChiralityProjector (left and right), skipped for odd dimY
    ///   - ComputeCouplingProxy: 3 random (spinorI, spinorJ, bosonK) triples
    ///
    /// Uses numTestVectors random spinors for Gamma/Dirac/Mass/Chirality tests.
    /// Uses 3 fixed random triples for coupling tests (per spec).
    ///
    /// <param name="cpuKernel">CPU reference kernel.</param>
    /// <param name="gpuKernel">GPU kernel to validate.</param>
    /// <param name="spec">Spinor representation spec (drives gamma count and chirality check).</param>
    /// <param name="bosonDimension">edgeCount * dimG — dimension of bosonK vectors.</param>
    /// <param name="numTestVectors">Number of random spinor test vectors per operation.</param>
    /// <param name="gammaTolerance">Override tolerance for ApplyGamma.</param>
    /// <param name="diracTolerance">Override tolerance for ApplyDirac.</param>
    /// <param name="massTolerance">Override tolerance for ApplyMass.</param>
    /// <param name="chiralityTolerance">Override tolerance for ApplyChiralityProjector.</param>
    /// <param name="couplingTolerance">Override tolerance for ComputeCouplingProxy.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// </summary>
    public static ParityReport RunFullCheck(
        IDiracKernel cpuKernel,
        IDiracKernel gpuKernel,
        SpinorRepresentationSpec spec,
        int bosonDimension,
        int numTestVectors = 5,
        double? gammaTolerance = null,
        double? diracTolerance = null,
        double? massTolerance = null,
        double? chiralityTolerance = null,
        double? couplingTolerance = null,
        int seed = 42)
    {
        ArgumentNullException.ThrowIfNull(cpuKernel);
        ArgumentNullException.ThrowIfNull(gpuKernel);
        ArgumentNullException.ThrowIfNull(spec);

        if (cpuKernel.SpinorDimension != gpuKernel.SpinorDimension)
            throw new ArgumentException(
                $"SpinorDimension mismatch: CPU={cpuKernel.SpinorDimension}, GPU={gpuKernel.SpinorDimension}");
        if (cpuKernel.SpacetimeDimension != gpuKernel.SpacetimeDimension)
            throw new ArgumentException(
                $"SpacetimeDimension mismatch: CPU={cpuKernel.SpacetimeDimension}, GPU={gpuKernel.SpacetimeDimension}");

        double gTol = gammaTolerance ?? GammaTolerance;
        double dTol = diracTolerance ?? DiracTolerance;
        double mTol = massTolerance ?? MassTolerance;
        double cTol = chiralityTolerance ?? ChiralityTolerance;
        double kTol = couplingTolerance ?? CouplingTolerance;

        int n = cpuKernel.SpinorDimension;
        int dim = cpuKernel.SpacetimeDimension;
        bool hasChirality = spec.GammaConvention.HasChirality;

        var rng = new Random(seed);
        var results = new List<ParityResult>();

        // --- ApplyGamma for each mu ---
        for (int mu = 0; mu < dim; mu++)
        {
            for (int t = 0; t < numTestVectors; t++)
            {
                var spinor = RandomVector(rng, n);
                var cpuOut = new double[n];
                var gpuOut = new double[n];
                cpuKernel.ApplyGamma(mu, spinor, cpuOut);
                gpuKernel.ApplyGamma(mu, spinor, gpuOut);
                results.Add(Compare($"ApplyGamma[mu={mu},t={t}]", cpuOut, gpuOut, gTol));
            }
        }

        // --- ApplyDirac ---
        for (int t = 0; t < numTestVectors; t++)
        {
            var spinor = RandomVector(rng, n);
            var cpuOut = new double[n];
            var gpuOut = new double[n];
            cpuKernel.ApplyDirac(spinor, cpuOut);
            gpuKernel.ApplyDirac(spinor, gpuOut);
            results.Add(Compare($"ApplyDirac[t={t}]", cpuOut, gpuOut, dTol));
        }

        // --- ApplyMass ---
        for (int t = 0; t < numTestVectors; t++)
        {
            var spinor = RandomVector(rng, n);
            var cpuOut = new double[n];
            var gpuOut = new double[n];
            cpuKernel.ApplyMass(spinor, cpuOut);
            gpuKernel.ApplyMass(spinor, gpuOut);
            results.Add(Compare($"ApplyMass[t={t}]", cpuOut, gpuOut, mTol));
        }

        // --- ApplyChiralityProjector (skip for odd dimY) ---
        if (hasChirality)
        {
            foreach (bool left in new[] { true, false })
            {
                string side = left ? "L" : "R";
                for (int t = 0; t < numTestVectors; t++)
                {
                    var spinor = RandomVector(rng, n);
                    var cpuOut = new double[n];
                    var gpuOut = new double[n];
                    cpuKernel.ApplyChiralityProjector(left, spinor, cpuOut);
                    gpuKernel.ApplyChiralityProjector(left, spinor, gpuOut);
                    results.Add(Compare($"ApplyChiralityProjector[{side},t={t}]", cpuOut, gpuOut, cTol));
                }
            }
        }

        // --- ComputeCouplingProxy: 3 random (spinorI, spinorJ, bosonK) triples ---
        for (int trial = 0; trial < 3; trial++)
        {
            var spinorI = RandomVector(rng, n);
            var spinorJ = RandomVector(rng, n);
            var bosonK = RandomVector(rng, bosonDimension);

            var cpuCoupling = cpuKernel.ComputeCouplingProxy(spinorI, spinorJ, bosonK);
            var gpuCoupling = gpuKernel.ComputeCouplingProxy(spinorI, spinorJ, bosonK);

            var (reRes, imRes) = CompareCoupling(
                $"ComputeCouplingProxy[trial={trial}]",
                cpuCoupling, gpuCoupling, kTol);
            results.Add(reRes);
            results.Add(imRes);
        }

        return new ParityReport { Results = results };
    }

    private static double[] RandomVector(Random rng, int n)
    {
        var v = new double[n];
        for (int i = 0; i < n; i++)
            v[i] = rng.NextDouble() - 0.5;
        return v;
    }
}
