using System.Text.Json.Serialization;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Linearized gauge action Gamma_* at a background state z_*.
///
/// Maps infinitesimal gauge parameters eta (vertex-valued, dimG per vertex)
/// to connection perturbations delta_z = Gamma_* eta (edge-valued, dimG per edge).
///
/// Wraps the Phase 2 InfinitesimalGaugeMap R_{z_*} but provides Phase 3
/// gauge-reduction-specific metadata and diagnostics.
/// </summary>
public sealed class GaugeActionLinearization
{
    private readonly InfinitesimalGaugeMap _gaugeMap;

    /// <summary>Background state ID this linearization is taken at.</summary>
    public string BackgroundId { get; }

    /// <summary>Number of gauge parameter DOFs (VertexCount * dimG).</summary>
    public int GaugeParameterDimension => _gaugeMap.InputDimension;

    /// <summary>Number of connection DOFs (EdgeCount * dimG).</summary>
    public int ConnectionDimension => _gaugeMap.OutputDimension;

    /// <summary>Expected gauge rank = dimG * (VertexCount - 1) for connected mesh.</summary>
    public int ExpectedGaugeRank { get; }

    /// <summary>The underlying linear operator.</summary>
    public ILinearOperator Operator => _gaugeMap;

    /// <summary>
    /// Create a gauge action linearization from mesh, algebra, and background fields.
    /// </summary>
    public GaugeActionLinearization(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        FieldTensor a0,
        FieldTensor omegaStar,
        string backgroundId)
    {
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        if (algebra == null) throw new ArgumentNullException(nameof(algebra));
        if (string.IsNullOrEmpty(backgroundId))
            throw new ArgumentException("Background ID must not be empty.", nameof(backgroundId));

        _gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omegaStar);
        BackgroundId = backgroundId;

        // For a connected mesh, constant gauge transforms are in the kernel,
        // so expected rank = dimG * (VertexCount - 1).
        ExpectedGaugeRank = algebra.Dimension * (mesh.VertexCount - 1);
    }

    /// <summary>
    /// Create from an existing Phase 2 BackgroundStateRecord.
    /// </summary>
    public GaugeActionLinearization(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        BackgroundStateRecord background)
        : this(mesh, algebra, background.A0, background.Omega, background.Id)
    {
    }

    /// <summary>
    /// Apply the linearized gauge action: delta_z = Gamma_* eta.
    /// </summary>
    public FieldTensor Apply(FieldTensor eta) => _gaugeMap.Apply(eta);

    /// <summary>
    /// Apply the transpose: Gamma_*^T w.
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor w) => _gaugeMap.ApplyTranspose(w);
}
