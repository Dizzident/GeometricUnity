namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Verification status of the CUDA GPU spectral backend.
/// Enforces IA-5 (CPU reference before CUDA trust): no high-claim
/// candidate may depend on unverified GPU-only logic.
/// </summary>
public enum CudaVerificationStatus
{
    /// <summary>CUDA hardware/library not available.</summary>
    NotAvailable,

    /// <summary>CUDA is available but parity with CPU has not been verified.</summary>
    AvailableUnverified,

    /// <summary>CUDA is available and has passed CPU/GPU parity checks.</summary>
    AvailableParityPassed,

    /// <summary>CUDA is available but failed CPU/GPU parity checks.</summary>
    AvailableParityFailed,
}
