using Gu.Core;
using Gu.Phase3.GaugeReduction;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Orchestrates the full spectral solve pipeline:
/// 1. Select solver method (dense for small, Lanczos for large).
/// 2. Solve the generalized eigenproblem H v = lambda M v.
/// 3. M-normalize eigenvectors.
/// 4. Compute residual norms and orthogonality diagnostics.
/// 5. Compute gauge leak scores.
/// 6. Cluster near-degenerate eigenvalues.
/// 7. Diagnose null modes.
/// 8. Assemble SpectrumBundle.
/// </summary>
public sealed class EigensolverPipeline
{
    private readonly GaugeProjector? _gaugeProjector;

    public EigensolverPipeline(GaugeProjector? gaugeProjector = null)
    {
        _gaugeProjector = gaugeProjector;
    }

    /// <summary>
    /// Solve the spectral problem and return a complete SpectrumBundle.
    /// </summary>
    public SpectrumBundle Solve(
        LinearizedOperatorBundle bundle,
        GeneralizedEigenproblemSpec spec)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(spec);

        int n = bundle.StateDimension;
        int numEig = System.Math.Min(spec.NumEigenvalues, n);

        // Select solver method
        string method = spec.SolverMethod;
        if (method == "auto")
            method = n <= 200 ? "explicit-dense" : "lanczos";

        // Solve
        double[] eigenvalues;
        double[][] eigenvectors;
        int iterations;
        string convergenceStatus;

        switch (method)
        {
            case "explicit-dense":
                (eigenvalues, eigenvectors) = DenseEigensolver.Solve(bundle, numEig);
                iterations = 0; // dense solve is direct
                convergenceStatus = "converged";
                break;

            case "lanczos":
                (eigenvalues, eigenvectors, iterations, convergenceStatus) =
                    LanczosSolver.Solve(bundle, numEig, spec.MaxIterations, spec.ConvergenceTolerance);
                break;

            default:
                throw new ArgumentException($"Unknown solver method: {method}");
        }

        // Compute residual norms: ||H v - lambda M v|| / ||v||_M
        var residualNorms = ComputeResidualNorms(bundle, eigenvalues, eigenvectors);

        // Compute gauge leak scores
        var gaugeLeakScores = ComputeGaugeLeakScores(eigenvectors);

        // Compute orthogonality diagnostics
        double maxOrthDefect = ComputeMaxOrthogonalityDefect(bundle, eigenvectors);

        // Build mode records with energy fractions
        var modes = new ModeRecord[eigenvalues.Length];
        for (int k = 0; k < eigenvalues.Length; k++)
        {
            var blockFractions = ComputeBlockEnergyFractions(eigenvectors[k]);
            var tensorFractions = ComputeTensorEnergyFractions(bundle, eigenvectors[k]);

            modes[k] = new ModeRecord
            {
                ModeId = $"{bundle.BackgroundId}-mode-{k}",
                BackgroundId = bundle.BackgroundId,
                OperatorType = bundle.OperatorType,
                Eigenvalue = eigenvalues[k],
                ResidualNorm = residualNorms[k],
                NormalizationConvention = "unit-M-norm",
                GaugeLeakScore = gaugeLeakScores[k],
                ModeVector = eigenvectors[k],
                ModeIndex = k,
                BlockEnergyFractions = blockFractions,
                TensorEnergyFractions = tensorFractions,
            };
        }

        // Spectral clustering
        var clusters = SpectralClustering.Cluster(
            eigenvalues,
            spec.ClusterRelativeTolerance,
            spec.ClusterAbsoluteTolerance,
            bundle.BackgroundId);

        // Assign cluster IDs to modes
        foreach (var cluster in clusters)
        {
            foreach (int idx in cluster.ModeIndices)
            {
                if (idx < modes.Length)
                {
                    modes[idx] = new ModeRecord
                    {
                        ModeId = modes[idx].ModeId,
                        BackgroundId = modes[idx].BackgroundId,
                        OperatorType = modes[idx].OperatorType,
                        Eigenvalue = modes[idx].Eigenvalue,
                        ResidualNorm = modes[idx].ResidualNorm,
                        NormalizationConvention = modes[idx].NormalizationConvention,
                        MultiplicityClusterId = cluster.ClusterId,
                        GaugeLeakScore = modes[idx].GaugeLeakScore,
                        ModeVector = modes[idx].ModeVector,
                        ModeIndex = modes[idx].ModeIndex,
                        BlockEnergyFractions = modes[idx].BlockEnergyFractions,
                        TensorEnergyFractions = modes[idx].TensorEnergyFractions,
                        ModeVectorArtifactRef = modes[idx].ModeVectorArtifactRef,
                        ObservedSignatureRef = modes[idx].ObservedSignatureRef,
                    };
                }
            }
        }

        // Null mode diagnosis
        var nullDiagnosis = DiagnoseNullModes(
            eigenvalues, gaugeLeakScores,
            spec.NullModeThreshold, spec.GaugeLeakThreshold);

        return new SpectrumBundle
        {
            SpectrumId = $"spectrum-{bundle.BackgroundId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            BackgroundId = bundle.BackgroundId,
            OperatorBundleId = bundle.BundleId,
            OperatorType = bundle.OperatorType,
            Formulation = bundle.Formulation,
            SolverMethod = method,
            StateDimension = n,
            Modes = modes,
            Clusters = clusters,
            NullModeDiagnosis = nullDiagnosis,
            ConvergenceStatus = convergenceStatus,
            IterationsUsed = iterations,
            MaxOrthogonalityDefect = maxOrthDefect,
        };
    }

    private double[] ComputeResidualNorms(
        LinearizedOperatorBundle bundle, double[] eigenvalues, double[][] eigenvectors)
    {
        int n = bundle.StateDimension;
        var norms = new double[eigenvalues.Length];

        for (int k = 0; k < eigenvalues.Length; k++)
        {
            var ft = new FieldTensor
            {
                Label = $"v_{k}",
                Signature = bundle.SpectralOperator.InputSignature,
                Coefficients = eigenvectors[k],
                Shape = new[] { n },
            };

            var hv = bundle.ApplySpectral(ft);
            var mv = bundle.ApplyMass(ft);

            // r = Hv - lambda*Mv
            double rNorm = 0;
            double mNorm = 0;
            for (int i = 0; i < n; i++)
            {
                double r = hv.Coefficients[i] - eigenvalues[k] * mv.Coefficients[i];
                rNorm += r * r;
                mNorm += eigenvectors[k][i] * mv.Coefficients[i];
            }

            norms[k] = mNorm > 1e-30
                ? System.Math.Sqrt(rNorm) / System.Math.Sqrt(mNorm)
                : System.Math.Sqrt(rNorm);
        }

        return norms;
    }

    private double[] ComputeGaugeLeakScores(double[][] eigenvectors)
    {
        var scores = new double[eigenvectors.Length];
        if (_gaugeProjector == null)
        {
            // No gauge projector: report 0 (no gauge leak info)
            return scores;
        }

        for (int k = 0; k < eigenvectors.Length; k++)
            scores[k] = _gaugeProjector.GaugeLeakScore(eigenvectors[k]);

        return scores;
    }

    private static double ComputeMaxOrthogonalityDefect(
        LinearizedOperatorBundle bundle, double[][] eigenvectors)
    {
        int count = eigenvectors.Length;
        int n = bundle.StateDimension;
        double maxDefect = 0;

        for (int i = 0; i < count; i++)
        {
            var fti = new FieldTensor
            {
                Label = $"v_{i}",
                Signature = bundle.MassOperator.InputSignature,
                Coefficients = eigenvectors[i],
                Shape = new[] { n },
            };
            var mvi = bundle.ApplyMass(fti);

            for (int j = i + 1; j < count; j++)
            {
                double dot = 0;
                for (int k = 0; k < n; k++)
                    dot += eigenvectors[j][k] * mvi.Coefficients[k];
                double defect = System.Math.Abs(dot);
                if (defect > maxDefect) maxDefect = defect;
            }
        }

        return maxDefect;
    }

    /// <summary>
    /// Compute block energy fractions from a mode vector.
    /// For single-connection-block layouts, the entire energy is in the "connection" block.
    /// </summary>
    private static Dictionary<string, double> ComputeBlockEnergyFractions(double[] eigenvector)
    {
        // Single-block case: all energy is in the connection block.
        // This matches PolarizationExtractor semantics for Phase I/III compatible layouts.
        return new Dictionary<string, double> { ["connection"] = 1.0 };
    }

    /// <summary>
    /// Compute tensor energy fractions from a mode vector, keyed by the tensor carrier type.
    /// For single-connection-block layouts, the energy is 100% in the operator's carrier type.
    /// </summary>
    private static Dictionary<string, double> ComputeTensorEnergyFractions(
        LinearizedOperatorBundle bundle, double[] eigenvector)
    {
        string carrierType = bundle.SpectralOperator.InputSignature.CarrierType;
        return new Dictionary<string, double> { [carrierType] = 1.0 };
    }

    private static NullModeDiagnosis? DiagnoseNullModes(
        double[] eigenvalues, double[] gaugeLeakScores,
        double nullThreshold, double gaugeLeakThreshold)
    {
        var nullIndices = new List<int>();
        for (int i = 0; i < eigenvalues.Length; i++)
        {
            if (System.Math.Abs(eigenvalues[i]) < nullThreshold)
                nullIndices.Add(i);
        }

        if (nullIndices.Count == 0) return null;

        int gaugeNull = 0;
        var nullEig = new double[nullIndices.Count];
        var nullLeak = new double[nullIndices.Count];
        for (int i = 0; i < nullIndices.Count; i++)
        {
            int idx = nullIndices[i];
            nullEig[i] = eigenvalues[idx];
            nullLeak[i] = gaugeLeakScores[idx];
            if (gaugeLeakScores[idx] > gaugeLeakThreshold)
                gaugeNull++;
        }

        return new NullModeDiagnosis
        {
            NullThreshold = nullThreshold,
            NullModeCount = nullIndices.Count,
            GaugeNullCount = gaugeNull,
            NullEigenvalues = nullEig,
            NullGaugeLeakScores = nullLeak,
            GaugeLeakThreshold = gaugeLeakThreshold,
        };
    }
}
