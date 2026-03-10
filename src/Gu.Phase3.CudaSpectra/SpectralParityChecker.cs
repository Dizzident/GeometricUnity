namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Validates CPU/GPU parity for spectral operator actions.
/// Compares outputs from CpuSpectralKernel against GpuSpectralKernel
/// and reports maximum relative error per operation.
///
/// Extends the Phase2ParityChecker pattern for Phase III spectral operations.
/// </summary>
public sealed class SpectralParityChecker
{
    /// <summary>Default relative tolerance for parity checks.</summary>
    public const double DefaultTolerance = 1e-9;

    /// <summary>
    /// Result of a single spectral parity check.
    /// </summary>
    public sealed class ParityResult
    {
        /// <summary>Name of the kernel operation tested.</summary>
        public required string OperationName { get; init; }

        /// <summary>Whether the parity check passed.</summary>
        public required bool Passed { get; init; }

        /// <summary>Maximum relative error across all output components.</summary>
        public required double MaxRelativeError { get; init; }

        /// <summary>Tolerance used for the check.</summary>
        public required double Tolerance { get; init; }

        /// <summary>Number of output components compared.</summary>
        public required int ComponentsCompared { get; init; }

        /// <summary>Index of the component with maximum error.</summary>
        public required int MaxErrorIndex { get; init; }
    }

    /// <summary>
    /// Full parity report across all spectral operations.
    /// </summary>
    public sealed class ParityReport
    {
        /// <summary>Individual results per operation.</summary>
        public required IReadOnlyList<ParityResult> Results { get; init; }

        /// <summary>Whether all operations passed.</summary>
        public bool AllPassed => Results.All(r => r.Passed);

        /// <summary>Maximum relative error across all operations.</summary>
        public double WorstError => Results.Count > 0 ? Results.Max(r => r.MaxRelativeError) : 0.0;
    }

    /// <summary>
    /// Compare two span outputs and compute maximum relative error.
    /// Uses the same error formula as Phase2ParityChecker: |a-b| / (1 + |a|).
    /// </summary>
    public static ParityResult Compare(
        string operationName,
        ReadOnlySpan<double> cpuOutput,
        ReadOnlySpan<double> gpuOutput,
        double tolerance = DefaultTolerance)
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
    /// Run full parity check between CPU and GPU spectral kernels.
    /// Tests all four operations: ApplySpectral, ApplyMass, ApplyJacobian, ApplyAdjoint.
    /// </summary>
    /// <param name="cpuKernel">CPU reference kernel.</param>
    /// <param name="gpuKernel">GPU kernel to validate.</param>
    /// <param name="numTestVectors">Number of random test vectors to use.</param>
    /// <param name="tolerance">Relative error tolerance.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    public static ParityReport RunFullCheck(
        ISpectralKernel cpuKernel,
        ISpectralKernel gpuKernel,
        int numTestVectors = 5,
        double tolerance = DefaultTolerance,
        int seed = 42)
    {
        if (cpuKernel.StateDimension != gpuKernel.StateDimension)
            throw new ArgumentException("State dimensions do not match.");
        if (cpuKernel.ResidualDimension != gpuKernel.ResidualDimension)
            throw new ArgumentException("Residual dimensions do not match.");

        int n = cpuKernel.StateDimension;
        int m = cpuKernel.ResidualDimension;
        var rng = new Random(seed);
        var results = new List<ParityResult>();

        for (int t = 0; t < numTestVectors; t++)
        {
            // State-space test vector
            var v = RandomVector(rng, n);

            // Test ApplySpectral
            var cpuHv = new double[n];
            var gpuHv = new double[n];
            cpuKernel.ApplySpectral(v, cpuHv);
            gpuKernel.ApplySpectral(v, gpuHv);
            results.Add(Compare($"ApplySpectral[{t}]", cpuHv, gpuHv, tolerance));

            // Test ApplyMass
            var cpuMv = new double[n];
            var gpuMv = new double[n];
            cpuKernel.ApplyMass(v, cpuMv);
            gpuKernel.ApplyMass(v, gpuMv);
            results.Add(Compare($"ApplyMass[{t}]", cpuMv, gpuMv, tolerance));

            // Test ApplyJacobian
            var cpuJv = new double[m];
            var gpuJv = new double[m];
            cpuKernel.ApplyJacobian(v, cpuJv);
            gpuKernel.ApplyJacobian(v, gpuJv);
            results.Add(Compare($"ApplyJacobian[{t}]", cpuJv, gpuJv, tolerance));

            // Test ApplyAdjoint (input from residual space)
            var w = RandomVector(rng, m);
            var cpuJtw = new double[n];
            var gpuJtw = new double[n];
            cpuKernel.ApplyAdjoint(w, cpuJtw);
            gpuKernel.ApplyAdjoint(w, gpuJtw);
            results.Add(Compare($"ApplyAdjoint[{t}]", cpuJtw, gpuJtw, tolerance));
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
