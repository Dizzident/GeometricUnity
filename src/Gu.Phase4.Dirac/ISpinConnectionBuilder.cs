using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Builds a SpinConnectionBundle from a bosonic background and spinor specification.
///
/// The spin connection has two parts (§11.4):
/// 1. Levi-Civita part from Y_h metric geometry.
/// 2. Gauge coupling part from the bosonic omega field via representation rho.
/// </summary>
public interface ISpinConnectionBuilder
{
    /// <summary>
    /// Build the spin connection from a background and spinor spec.
    /// </summary>
    /// <param name="background">Bosonic background record (Phase III output).</param>
    /// <param name="bosonicState">
    /// The resolved connection field omega (FieldTensor from the background state artifact).
    /// Shape: edgeCount * dimG.
    /// </param>
    /// <param name="spinorSpec">Spinor representation specification.</param>
    /// <param name="layout">Fermionic field layout.</param>
    /// <param name="mesh">Y_h mesh (provides edge/vertex geometry).</param>
    /// <param name="provenance">Provenance metadata.</param>
    SpinConnectionBundle Build(
        BackgroundRecord background,
        double[] bosonicState,
        SpinorRepresentationSpec spinorSpec,
        FermionFieldLayout layout,
        Gu.Geometry.SimplicialMesh mesh,
        ProvenanceMeta provenance);
}
