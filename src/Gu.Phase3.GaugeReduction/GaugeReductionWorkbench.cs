using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Orchestrates gauge reduction for a background state.
///
/// Builds the gauge action linearization, gauge basis, projectors,
/// and diagnostic reports. This is the primary entry point for
/// Phase III gauge reduction.
/// </summary>
public sealed class GaugeReductionWorkbench
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public GaugeReductionWorkbench(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    /// <summary>
    /// Build the linearized gauge action at a background state.
    /// </summary>
    public GaugeActionLinearization BuildLinearization(BackgroundStateRecord background)
    {
        return new GaugeActionLinearization(_mesh, _algebra, background);
    }

    /// <summary>
    /// Build a gauge basis from a background state (Euclidean inner product).
    /// </summary>
    public GaugeBasis BuildGaugeBasis(
        BackgroundStateRecord background,
        double svdCutoff = 1e-10)
    {
        var linearization = BuildLinearization(background);
        return GaugeBasis.Build(linearization, svdCutoff);
    }

    /// <summary>
    /// Build an M_state-orthonormal gauge basis from a background state.
    /// Use this when M_state != I to ensure P_phys is M_state-self-adjoint (constraint #8).
    /// </summary>
    public GaugeBasis BuildGaugeBasisWithMass(
        BackgroundStateRecord background,
        double[] massWeights,
        double svdCutoff = 1e-10)
    {
        var linearization = BuildLinearization(background);
        return GaugeBasis.BuildWithMass(linearization, massWeights, svdCutoff);
    }

    /// <summary>
    /// Build a gauge projector from a background state (Euclidean inner product).
    /// </summary>
    public GaugeProjector BuildGaugeProjector(
        BackgroundStateRecord background,
        double svdCutoff = 1e-10)
    {
        var basis = BuildGaugeBasis(background, svdCutoff);
        return new GaugeProjector(basis);
    }

    /// <summary>
    /// Build an M_state-aware gauge projector.
    /// The gauge basis is M_state-orthonormalized and the projector uses
    /// M_state inner product, ensuring P_phys is M_state-self-adjoint.
    /// </summary>
    public GaugeProjector BuildGaugeProjectorWithMass(
        BackgroundStateRecord background,
        double[] massWeights,
        double svdCutoff = 1e-10)
    {
        var basis = BuildGaugeBasisWithMass(background, massWeights, svdCutoff);
        return new GaugeProjector(basis, massWeights);
    }

    /// <summary>
    /// Build a physical projector (ILinearOperator) from a background state.
    /// </summary>
    public PhysicalProjector BuildPhysicalProjector(
        BackgroundStateRecord background,
        double svdCutoff = 1e-10)
    {
        var gaugeProjector = BuildGaugeProjector(background, svdCutoff);
        return new PhysicalProjector(gaugeProjector);
    }

    /// <summary>
    /// Generate a constraint defect report for a background state.
    /// </summary>
    public ConstraintDefectReport GenerateConstraintDefectReport(
        BackgroundStateRecord background,
        double svdCutoff = 1e-10)
    {
        var basis = BuildGaugeBasis(background, svdCutoff);
        return ConstraintDefectReport.FromGaugeBasis(basis);
    }

    /// <summary>
    /// Generate a gauge leak report for a set of trial vectors.
    /// </summary>
    public GaugeLeakReport GenerateGaugeLeakReport(
        BackgroundStateRecord background,
        double[][] trialVectors,
        string[]? vectorLabels = null,
        double svdCutoff = 1e-10)
    {
        if (trialVectors == null) throw new ArgumentNullException(nameof(trialVectors));
        if (trialVectors.Length == 0)
            throw new ArgumentException("At least one trial vector is required.", nameof(trialVectors));

        var projector = BuildGaugeProjector(background, svdCutoff);

        var entries = new GaugeLeakEntry[trialVectors.Length];
        double maxLeak = 0;
        double sumLeak = 0;

        for (int i = 0; i < trialVectors.Length; i++)
        {
            double score = projector.GaugeLeakScore(trialVectors[i]);
            double totalNorm = L2Norm(trialVectors[i]);
            var gaugeComp = projector.ApplyGauge(trialVectors[i]);
            double gaugeNorm = L2Norm(gaugeComp);

            string label = vectorLabels != null && i < vectorLabels.Length
                ? vectorLabels[i]
                : $"trial-{i}";

            entries[i] = new GaugeLeakEntry
            {
                VectorLabel = label,
                LeakScore = score,
                GaugeNorm = gaugeNorm,
                TotalNorm = totalNorm,
            };

            sumLeak += score;
            if (score > maxLeak) maxLeak = score;
        }

        // Compute spectral gap from gauge basis singular values (constraint #5)
        double? spectralGap = ComputeSpectralGap(projector.Basis);

        return new GaugeLeakReport
        {
            BackgroundId = background.Id,
            GaugeRank = projector.GaugeRank,
            Entries = entries,
            MaxLeakScore = maxLeak,
            MeanLeakScore = trialVectors.Length > 0 ? sumLeak / trialVectors.Length : 0.0,
            SpectralGap = spectralGap,
        };
    }

    /// <summary>
    /// Perform full gauge reduction: build all components and generate reports.
    /// </summary>
    public GaugeReductionResult PerformFullReduction(
        BackgroundStateRecord background,
        double svdCutoff = 1e-10)
    {
        var linearization = BuildLinearization(background);
        var basis = GaugeBasis.Build(linearization, svdCutoff);
        var gaugeProjector = new GaugeProjector(basis);
        var physicalProjector = new PhysicalProjector(gaugeProjector);
        var defectReport = ConstraintDefectReport.FromGaugeBasis(basis);

        return new GaugeReductionResult
        {
            Linearization = linearization,
            Basis = basis,
            GaugeProjector = gaugeProjector,
            PhysicalProjector = physicalProjector,
            DefectReport = defectReport,
        };
    }

    /// <summary>
    /// Wrap an operator H into the physical subspace: H_phys = P_phys H P_phys.
    /// Use this to project a HessianOperator (or any ILinearOperator) into
    /// the gauge-reduced physical subspace.
    /// </summary>
    public PhysicalProjectedOperator WrapOperator(
        ILinearOperator op,
        GaugeProjector projector)
    {
        if (op == null) throw new ArgumentNullException(nameof(op));
        if (projector == null) throw new ArgumentNullException(nameof(projector));
        return new PhysicalProjectedOperator(op, projector);
    }

    /// <summary>
    /// Compute the spectral gap between gauge and physical sectors.
    ///
    /// The spectral gap is the ratio of the smallest retained singular value
    /// (weakest gauge direction) to the largest discarded singular value
    /// (strongest non-gauge direction from the SVD). A large gap indicates
    /// clean separation; a small gap warns of numerical mixing.
    ///
    /// Returns null if not enough singular values to compute.
    /// </summary>
    private static double? ComputeSpectralGap(GaugeBasis basis)
    {
        if (basis.Rank == 0 || basis.SingularValues.Count == 0)
            return null;

        double minRetained = basis.SingularValues[basis.SingularValues.Count - 1];

        // Find the largest discarded singular value: first SV in AllSingularValues strictly below minRetained
        double maxDiscarded = 0;
        foreach (double sv in basis.AllSingularValues)
        {
            if (sv < minRetained - 1e-15)
            {
                maxDiscarded = sv;
                break;
            }
        }

        // No discarded singular values — cannot compute ratio
        if (maxDiscarded < 1e-30)
            return null;

        return minRetained / maxDiscarded;
    }

    /// <summary>
    /// Compute the old proxy-based spectral gap (kept for comparison).
    /// </summary>
    private static double? ComputeSpectralGapApproximate(GaugeBasis basis)
    {
        if (basis.Rank == 0 || basis.SingularValues.Count == 0)
            return null;

        double smallestRetained = basis.SingularValues[basis.SingularValues.Count - 1];
        double cutoffValue = basis.SingularValues[0] * basis.SvdCutoff;

        if (cutoffValue < 1e-30)
            return smallestRetained;

        return smallestRetained / cutoffValue;
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }
}
