namespace Gu.Phase4.Dirac;

/// <summary>
/// Configuration for the fermionic spectral solver.
/// </summary>
public sealed class FermionSpectralConfig
{
    /// <summary>
    /// Target region for eigenvalue computation.
    /// "lowest-magnitude": modes closest to zero (near-zero mass candidates),
    /// "near-zero": synonym for lowest-magnitude,
    /// "lowest-positive": smallest positive eigenvalues only.
    /// </summary>
    public required string TargetRegion { get; init; }

    /// <summary>Number of fermion modes to compute.</summary>
    public required int ModeCount { get; init; }

    /// <summary>Whether to apply gauge/redundancy reduction before solving.</summary>
    public bool GaugeReduction { get; init; } = true;

    /// <summary>Whether to deflate the null space before solving.</summary>
    public bool NullspaceDeflation { get; init; } = true;

    /// <summary>Convergence tolerance for the eigensolver.</summary>
    public double ConvergenceTolerance { get; init; } = 1e-10;

    /// <summary>Maximum Lanczos/Arnoldi iterations.</summary>
    public int MaxIterations { get; init; } = 1000;

    /// <summary>Random seed for reproducible initial vectors. -1 = random.</summary>
    public int Seed { get; init; } = 42;

    /// <summary>
    /// Optional per-DOF diagonal weights for M_psi (the fermionic inner-product / mass matrix).
    ///
    /// M_psi is block-diagonal: one vol(c) * I_{spinorDim * gaugeDim} block per cell.
    /// This array should have length = totalDof * 2 (matching the real representation used
    /// by the solver). If null, M_psi = I (identity, all weights = 1).
    ///
    /// To build this from cell volumes: use MassPsiWeightsBuilder.BuildFromCellVolumes.
    /// </summary>
    public double[]? MassPsiWeights { get; init; }
}
