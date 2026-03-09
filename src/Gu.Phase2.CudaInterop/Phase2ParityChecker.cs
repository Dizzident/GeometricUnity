namespace Gu.Phase2.CudaInterop;

/// <summary>
/// Validates CPU-GPU parity for Phase II kernel operations.
/// Compares GPU kernel outputs against CPU reference implementations
/// and reports maximum relative error.
/// </summary>
public sealed class Phase2ParityChecker
{
    /// <summary>Default relative tolerance for parity checks.</summary>
    public const double DefaultTolerance = 1e-9;

    /// <summary>
    /// Result of a parity check between CPU and GPU outputs.
    /// </summary>
    public sealed class ParityResult
    {
        /// <summary>Name of the kernel operation tested.</summary>
        public required string KernelName { get; init; }

        /// <summary>Whether the parity check passed.</summary>
        public required bool Passed { get; init; }

        /// <summary>Maximum relative error across all output components.</summary>
        public required double MaxRelativeError { get; init; }

        /// <summary>Tolerance used for the check.</summary>
        public required double Tolerance { get; init; }

        /// <summary>Number of output components compared.</summary>
        public required int ComponentsCompared { get; init; }
    }

    /// <summary>
    /// Compare two output arrays and compute maximum relative error.
    /// </summary>
    public static ParityResult Compare(
        string kernelName,
        ReadOnlySpan<double> cpuOutput,
        ReadOnlySpan<double> gpuOutput,
        double tolerance = DefaultTolerance)
    {
        if (cpuOutput.Length != gpuOutput.Length)
            throw new ArgumentException(
                $"Output lengths differ: CPU={cpuOutput.Length}, GPU={gpuOutput.Length}");

        double maxRelErr = 0.0;
        for (int i = 0; i < cpuOutput.Length; i++)
        {
            double denom = 1.0 + System.Math.Abs(cpuOutput[i]);
            double relErr = System.Math.Abs(cpuOutput[i] - gpuOutput[i]) / denom;
            maxRelErr = System.Math.Max(maxRelErr, relErr);
        }

        return new ParityResult
        {
            KernelName = kernelName,
            Passed = maxRelErr < tolerance,
            MaxRelativeError = maxRelErr,
            Tolerance = tolerance,
            ComponentsCompared = cpuOutput.Length,
        };
    }
}
