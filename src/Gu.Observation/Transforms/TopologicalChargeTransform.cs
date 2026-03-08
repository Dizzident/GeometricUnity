using Gu.Core;

namespace Gu.Observation.Transforms;

/// <summary>
/// Computes a topological charge proxy from the curvature on X_h.
///
/// Physics: Q = (1/8pi^2) * integral_X tr(F wedge F).
/// This is the second Chern number -- an integer for smooth connections on compact manifolds.
///
/// In FEM discretization (Lie-algebra-valued 1-form coefficients), the discrete Q_h is
/// approximately integer with deviation O(h^p) controlled by mesh spacing.
/// Exact integrality requires lattice gauge theory (Wilson plaquettes), not available in v1.
///
/// Output type: ExactStructural because this is a placeholder proxy (coefficient sum),
/// not a true semi-quantitative FEM integral. The v1 proxy does not approximate the
/// actual topological charge, so SemiQuantitative would overstate its fidelity.
///
/// Note: tr(F wedge F) is identically zero for dim(X) &lt; 4 because wedging two 2-forms
/// on a manifold of dimension less than 4 produces a form of degree > dim, which vanishes.
/// The topological charge is only meaningful for dim(X) >= 4.
///
/// v1 implementation: sum of curvature coefficient squares as a proxy.
/// A proper implementation requires quadrature over cells with the full
/// tr(F wedge F) computation using the Lie algebra pairing.
/// </summary>
public sealed class TopologicalChargeTransform : IDerivedObservableTransform
{
    public string ObservableId => "topological-charge";
    public OutputType OutputType => OutputType.ExactStructural;
    public string TransformId => "topological-charge-v1";

    public double[] Compute(PulledBackFields pulledBackFields, ObservableRequest request)
    {
        var curvature = pulledBackFields.CurvatureF;

        // v1 proxy: sum all curvature coefficients.
        // A proper implementation would compute:
        //   Q_h = (1/8pi^2) * sum_cells sum_qp tr(F(qp) ^ F(qp)) * w_qp * sqrt(det(g))
        // with the wedge product antisymmetrization and Lie algebra trace.
        // This requires quadrature infrastructure not yet available.
        double charge = 0.0;
        for (int i = 0; i < curvature.Coefficients.Length; i++)
        {
            charge += curvature.Coefficients[i];
        }

        // Return both the raw charge and the integrality deviation as diagnostics.
        // values[0] = Q_h (the charge proxy)
        // values[1] = |Q_h - round(Q_h)| (integrality deviation diagnostic)
        double rounded = Math.Round(charge);
        double deviation = Math.Abs(charge - rounded);

        return new[] { charge, deviation };
    }
}
