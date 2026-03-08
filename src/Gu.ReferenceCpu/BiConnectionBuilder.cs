using Gu.Core;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Constructs the bi-connection pair (A_omega, B_omega) = Mu_A0(omega, branchParameters, geometryContext).
/// This is a branch-defined construction per Section 4.6.
/// The implementation must record the A0 dependency in provenance.
/// </summary>
public sealed class BiConnectionBuilder
{
    private readonly ConnectionField _a0;
    private readonly string _branchId;

    /// <summary>
    /// Creates a bi-connection builder with a distinguished connection A0.
    /// A0 must be represented explicitly (Section 4.5) -- never treated as an implicit global.
    /// </summary>
    /// <param name="a0">The distinguished connection A0.</param>
    /// <param name="branchId">Branch identifier for provenance tracking.</param>
    public BiConnectionBuilder(ConnectionField a0, string branchId)
    {
        _a0 = a0 ?? throw new ArgumentNullException(nameof(a0));
        _branchId = branchId ?? throw new ArgumentNullException(nameof(branchId));
    }

    /// <summary>The distinguished connection A0.</summary>
    public ConnectionField A0 => _a0;

    /// <summary>Branch identifier used for provenance.</summary>
    public string BranchId => _branchId;

    /// <summary>
    /// Constructs the bi-connection pair from omega_h.
    /// Default branch construction:
    ///   A_omega = A0 + omega
    ///   B_omega = A0 - omega
    ///
    /// This is a branch-defined map. Alternative constructions can be implemented
    /// by creating different BiConnectionBuilder variants, with the choice
    /// recorded in the branch manifest.
    /// </summary>
    /// <param name="omega">The dynamical connection field.</param>
    /// <returns>Tuple of (A_omega, B_omega) connection fields.</returns>
    public (ConnectionField AConnection, ConnectionField BConnection) Build(ConnectionField omega)
    {
        if (omega.Mesh != _a0.Mesh)
            throw new ArgumentException("omega and A0 must live on the same mesh.");
        if (omega.Algebra != _a0.Algebra)
            throw new ArgumentException("omega and A0 must use the same Lie algebra.");

        int n = omega.Coefficients.Length;
        var aCoeffs = new double[n];
        var bCoeffs = new double[n];

        for (int i = 0; i < n; i++)
        {
            aCoeffs[i] = _a0.Coefficients[i] + omega.Coefficients[i];
            bCoeffs[i] = _a0.Coefficients[i] - omega.Coefficients[i];
        }

        var aConnection = new ConnectionField(omega.Mesh, omega.Algebra, aCoeffs);
        var bConnection = new ConnectionField(omega.Mesh, omega.Algebra, bCoeffs);

        return (aConnection, bConnection);
    }

    /// <summary>
    /// Creates a BiConnectionBuilder with A0 = 0 (flat distinguished connection).
    /// Suitable for toy cases where A0 is trivial.
    /// </summary>
    public static BiConnectionBuilder WithFlatA0(
        Gu.Geometry.SimplicialMesh mesh, LieAlgebra algebra, string branchId)
    {
        var a0 = ConnectionField.Zero(mesh, algebra);
        return new BiConnectionBuilder(a0, branchId);
    }
}
