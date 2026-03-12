using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Interface for discrete Dirac operator kernel operations.
///
/// Mirrors ISpectralKernel (Phase III) for Phase IV fermionic operators.
/// All methods operate on flat real arrays with complex values interleaved
/// as [Re_0, Im_0, Re_1, Im_1, ...].
///
/// SpinorDimension returns the FULL interleaved array length:
///   2 * cellCount * spinorDim * gaugeDim
///
/// CPU and GPU implementations both implement this interface, enabling
/// CPU/GPU parity checks via DiracParityChecker.
/// </summary>
public interface IDiracKernel
{
    /// <summary>
    /// Apply a single gamma matrix: result = Gamma_mu * spinor (pointwise per cell).
    ///
    /// mu must be in [0, spacetimeDimension-1].
    /// </summary>
    void ApplyGamma(int mu, ReadOnlySpan<double> spinor, Span<double> result);

    /// <summary>
    /// Apply the full discrete Dirac operator: result = D_h * spinor.
    /// </summary>
    void ApplyDirac(ReadOnlySpan<double> spinor, Span<double> result);

    /// <summary>
    /// Apply the fermionic mass operator: result = M_psi * spinor.
    /// M_psi is block-diagonal: one block per cell, each = vol(cell) * I_spinor.
    /// </summary>
    void ApplyMass(ReadOnlySpan<double> spinor, Span<double> result);

    /// <summary>
    /// Apply the chirality projector: result = P_L * spinor (left=true) or P_R * spinor (left=false).
    /// P_L = (1/2)(I - Gamma_chi), P_R = (1/2)(I + Gamma_chi).
    ///
    /// Throws InvalidOperationException if the spacetime dimension is odd
    /// (chirality operator undefined for odd dimY).
    /// </summary>
    void ApplyChiralityProjector(bool left, ReadOnlySpan<double> spinor, Span<double> result);

    /// <summary>
    /// Compute the boson-fermion coupling proxy scalar:
    ///   g = &lt;spinorI, delta_D[bosonK] spinorJ&gt;
    ///
    /// where delta_D[bosonK] = Gamma^mu * bosonK_mu^a * rho(T_a) (analytical variation).
    ///
    /// bosonK has length edgeCount * dimG (algebra-valued connection coefficient per edge).
    /// spinorI and spinorJ have length SpinorDimension (interleaved complex).
    ///
    /// Returns the coupling as a (Real, Imag) pair.
    /// </summary>
    (double Real, double Imag) ComputeCouplingProxy(
        ReadOnlySpan<double> spinorI,
        ReadOnlySpan<double> spinorJ,
        ReadOnlySpan<double> bosonK);

    /// <summary>
    /// Full interleaved complex spinor array length = 2 * cellCount * spinorDim * gaugeDim.
    /// </summary>
    int SpinorDimension { get; }

    /// <summary>
    /// Spacetime dimension of Y_h (number of gamma matrices).
    /// </summary>
    int SpacetimeDimension { get; }
}
