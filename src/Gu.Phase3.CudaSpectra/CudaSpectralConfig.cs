namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Configuration for CUDA spectral acceleration operations.
/// Controls tolerances, block sizes, and backend selection for
/// GPU-accelerated spectral solves.
/// </summary>
public sealed class CudaSpectralConfig
{
    /// <summary>Relative tolerance for CPU/GPU parity checks.</summary>
    public double ParityTolerance { get; init; } = 1e-9;

    /// <summary>CUDA thread block size for operator kernels.</summary>
    public int CudaBlockSize { get; init; } = 256;

    /// <summary>Number of CUDA streams for concurrent operations.</summary>
    public int NumStreams { get; init; } = 2;

    /// <summary>Whether to force CPU-only execution (no GPU).</summary>
    public bool ForceCpu { get; init; }

    /// <summary>Whether to run parity checks before trusting GPU results.</summary>
    public bool EnableParityChecks { get; init; } = true;

    /// <summary>Number of random test vectors for parity validation.</summary>
    public int ParityTestVectors { get; init; } = 5;

    /// <summary>Random seed for parity test vector generation.</summary>
    public int ParitySeed { get; init; } = 42;

    /// <summary>Maximum GPU memory budget in bytes (0 = unlimited).</summary>
    public long MaxGpuMemoryBytes { get; init; }

    /// <summary>LOBPCG eigensolver configuration.</summary>
    public LobpcgConfig EigensolverConfig { get; init; } = new();

    /// <summary>
    /// Create a config that forces CPU-only execution.
    /// Used for environments without GPU hardware.
    /// </summary>
    public static CudaSpectralConfig CpuOnly() => new()
    {
        ForceCpu = true,
        EnableParityChecks = false,
    };
}
