using Gu.Core;

namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// Configuration for Dirac kernel CPU/CUDA parity checks.
/// </summary>
public sealed class DiracParityConfig
{
    /// <summary>Maximum allowed absolute per-element error for parity tests.</summary>
    public double MaxAbsoluteError { get; init; } = 1e-10;

    /// <summary>Number of random spinor vectors to test per operator.</summary>
    public int NumTestVectors { get; init; } = 8;

    /// <summary>Random seed for reproducible parity tests.</summary>
    public int RandomSeed { get; init; } = 42;

    public static DiracParityConfig Default { get; } = new();
}

/// <summary>
/// Result of a single parity check between CPU and GPU Dirac kernels.
/// </summary>
public sealed class DiracParityCheckResult
{
    public required string OperationName { get; init; }
    public required bool Passed { get; init; }
    public required double MaxAbsoluteError { get; init; }
    public required double MaxRelativeError { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Parity check report for all operator actions.
/// </summary>
public sealed class DiracParityReport
{
    public required string ReportId { get; init; }
    public required IReadOnlyList<DiracParityCheckResult> Checks { get; init; }
    public required bool AllPassed { get; init; }
    public required string GpuVerificationStatus { get; init; }
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Checks CPU/GPU parity for all IDiracKernel operations.
///
/// M44 parity closure: for each operation, applies both CPU and GPU kernels
/// to the same random test vectors and verifies the outputs agree within tolerance.
///
/// If the GPU kernel is a stub, the test should always pass (both use the same
/// CPU code), but the report records VerificationStatus = "stub-unverified".
/// </summary>
public static class DiracParityChecker
{
    /// <summary>
    /// Run all parity checks and return a report.
    /// </summary>
    public static DiracParityReport Check(
        IDiracKernel cpu,
        IDiracKernel gpu,
        string reportId,
        ProvenanceMeta provenance,
        DiracParityConfig? config = null)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        ArgumentNullException.ThrowIfNull(gpu);
        ArgumentNullException.ThrowIfNull(reportId);
        ArgumentNullException.ThrowIfNull(provenance);

        config ??= DiracParityConfig.Default;

        if (cpu.TotalDof != gpu.TotalDof)
            throw new InvalidOperationException(
                $"CPU TotalDof {cpu.TotalDof} != GPU TotalDof {gpu.TotalDof}.");

        int n = cpu.TotalDof;
        var rng = new Random(config.RandomSeed);
        var checks = new List<DiracParityCheckResult>();

        // Generate test vectors
        var testVectors = new List<double[]>(config.NumTestVectors);
        for (int v = 0; v < config.NumTestVectors; v++)
        {
            var vec = new double[n];
            for (int i = 0; i < n; i++) vec[i] = rng.NextDouble() * 2.0 - 1.0;
            testVectors.Add(vec);
        }

        checks.Add(CheckOperation("ApplyDirac", n, testVectors, config,
            (psi, res) => cpu.ApplyDirac(psi, res),
            (psi, res) => gpu.ApplyDirac(psi, res),
            provenance));

        checks.Add(CheckOperation("ApplyMassPsi", n, testVectors, config,
            (psi, res) => cpu.ApplyMassPsi(psi, res),
            (psi, res) => gpu.ApplyMassPsi(psi, res),
            provenance));

        checks.Add(CheckOperation("ProjectLeft", n, testVectors, config,
            (psi, res) => cpu.ProjectLeft(psi, res),
            (psi, res) => gpu.ProjectLeft(psi, res),
            provenance));

        checks.Add(CheckOperation("ProjectRight", n, testVectors, config,
            (psi, res) => cpu.ProjectRight(psi, res),
            (psi, res) => gpu.ProjectRight(psi, res),
            provenance));

        // Coupling proxy parity: test with a diagonal boson perturbation
        var bosonPert = new double[n];
        for (int i = 0; i < n; i++) bosonPert[i] = rng.NextDouble() * 0.1;
        var modePairs = new List<(int, int)> { (0, 0), (0, 1), (1, 0) }
            .Where(p => p.Item1 < n / 2 && p.Item2 < n / 2)
            .ToList();
        checks.Add(CheckCouplingProxy("AccumulateCouplingProxy", bosonPert, modePairs, config,
            cpu, gpu, provenance));

        string gpuStatus = gpu is GpuDiracKernelStub stub
            ? stub.VerificationStatus
            : "production";

        return new DiracParityReport
        {
            ReportId = reportId,
            Checks = checks,
            AllPassed = checks.All(c => c.Passed),
            GpuVerificationStatus = gpuStatus,
            Provenance = provenance,
        };
    }

    private static DiracParityCheckResult CheckOperation(
        string name,
        int n,
        IReadOnlyList<double[]> testVectors,
        DiracParityConfig config,
        Action<ReadOnlySpan<double>, Span<double>> cpuOp,
        Action<ReadOnlySpan<double>, Span<double>> gpuOp,
        ProvenanceMeta provenance)
    {
        double maxAbsErr = 0.0;
        double maxRelErr = 0.0;
        var notes = new List<string>();

        var cpuResult = new double[n];
        var gpuResult = new double[n];

        foreach (var psi in testVectors)
        {
            cpuOp(psi, cpuResult);
            gpuOp(psi, gpuResult);

            double norm = 0.0;
            for (int i = 0; i < n; i++) norm += cpuResult[i] * cpuResult[i];
            norm = System.Math.Sqrt(norm);

            for (int i = 0; i < n; i++)
            {
                double absErr = System.Math.Abs(cpuResult[i] - gpuResult[i]);
                double relErr = norm > 1e-300 ? absErr / norm : absErr;
                if (absErr > maxAbsErr) maxAbsErr = absErr;
                if (relErr > maxRelErr) maxRelErr = relErr;
            }
        }

        bool passed = maxAbsErr <= config.MaxAbsoluteError;
        if (!passed)
            notes.Add($"Max absolute error {maxAbsErr:E3} exceeds tolerance {config.MaxAbsoluteError:E3}.");

        return new DiracParityCheckResult
        {
            OperationName = name,
            Passed = passed,
            MaxAbsoluteError = maxAbsErr,
            MaxRelativeError = maxRelErr,
            Notes = notes,
            Provenance = provenance,
        };
    }

    private static DiracParityCheckResult CheckCouplingProxy(
        string name,
        double[] bosonPert,
        List<(int, int)> modePairs,
        DiracParityConfig config,
        IDiracKernel cpu,
        IDiracKernel gpu,
        ProvenanceMeta provenance)
    {
        double cpuVal = cpu.AccumulateCouplingProxy(bosonPert, modePairs);
        double gpuVal = gpu.AccumulateCouplingProxy(bosonPert, modePairs);
        double absErr = System.Math.Abs(cpuVal - gpuVal);
        double relErr = System.Math.Abs(cpuVal) > 1e-300 ? absErr / System.Math.Abs(cpuVal) : absErr;
        bool passed = absErr <= config.MaxAbsoluteError;

        var notes = new List<string>();
        if (!passed)
            notes.Add($"Coupling proxy abs error {absErr:E3} exceeds tolerance {config.MaxAbsoluteError:E3}.");

        return new DiracParityCheckResult
        {
            OperationName = name,
            Passed = passed,
            MaxAbsoluteError = absErr,
            MaxRelativeError = relErr,
            Notes = notes,
            Provenance = provenance,
        };
    }
}
