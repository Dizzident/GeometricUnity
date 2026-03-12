using Gu.Branching;
using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.Spectra;
using System.Text.Json;

namespace Gu.Phase3.Spectra.Tests;

/// <summary>
/// Tests for the CPU eigensolver pipeline (M26).
/// Tests cover: dense eigensolver, mode normalization, null mode diagnostics,
/// spectral clustering, and end-to-end pipeline correctness.
/// </summary>
public class EigensolverPipelineTests
{
    /// <summary>
    /// Build a diagonal test bundle with known eigenvalues.
    /// H = diag(hDiag), M = diag(mDiag).
    /// Eigenvalues are hDiag[i] / mDiag[i].
    /// </summary>
    private static LinearizedOperatorBundle MakeDiagonalBundle(
        double[] hDiag, double[] mDiag, string bgId = "test-bg")
    {
        var h = new TestHelpers.DiagonalOperator(hDiag);
        var m = new TestHelpers.DiagonalOperator(mDiag);
        return new LinearizedOperatorBundle
        {
            BundleId = $"bundle-{bgId}",
            BackgroundId = bgId,
            BranchManifestId = "test-branch",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Jacobian = h, // placeholder
            SpectralOperator = h,
            MassOperator = m,
            GaugeLambda = 0.1,
            StateDimension = hDiag.Length,
        };
    }

    [Fact]
    public void DenseEigensolver_DiagonalProblem_FindsCorrectEigenvalues()
    {
        // H = diag(1, 4, 9, 16, 25), M = I
        // Eigenvalues should be 1, 4, 9, 16, 25
        var hDiag = new double[] { 1, 4, 9, 16, 25 };
        var mDiag = new double[] { 1, 1, 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal(3, result.Modes.Count);
        Assert.Equal("converged", result.ConvergenceStatus);

        // Check eigenvalues (sorted ascending)
        Assert.Equal(1.0, result.Modes[0].Eigenvalue, 6);
        Assert.Equal(4.0, result.Modes[1].Eigenvalue, 6);
        Assert.Equal(9.0, result.Modes[2].Eigenvalue, 6);
    }

    [Fact]
    public void DenseEigensolver_GeneralizedProblem_FindsCorrectEigenvalues()
    {
        // H = diag(2, 6, 12), M = diag(2, 3, 4)
        // Generalized eigenvalues: 2/2=1, 6/3=2, 12/4=3
        var hDiag = new double[] { 2, 6, 12 };
        var mDiag = new double[] { 2, 3, 4 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal(3, result.Modes.Count);
        Assert.Equal(1.0, result.Modes[0].Eigenvalue, 6);
        Assert.Equal(2.0, result.Modes[1].Eigenvalue, 6);
        Assert.Equal(3.0, result.Modes[2].Eigenvalue, 6);
    }

    [Fact]
    public void ModeNormalization_EigenvectorsAreMNormalized()
    {
        var hDiag = new double[] { 3, 7, 11 };
        var mDiag = new double[] { 2, 3, 5 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        // Check M-normalization: v^T M v = 1
        for (int k = 0; k < result.Modes.Count; k++)
        {
            var v = result.Modes[k].ModeVector;
            double mNorm = 0;
            for (int i = 0; i < v.Length; i++)
                mNorm += v[i] * mDiag[i] * v[i];
            Assert.Equal(1.0, mNorm, 6);
        }
    }

    [Fact]
    public void OrthogonalityDiagnostics_AreReported()
    {
        var hDiag = new double[] { 1, 2, 3, 4, 5 };
        var mDiag = new double[] { 1, 1, 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 5,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        // Max orthogonality defect should be small for a diagonal problem
        Assert.True(result.MaxOrthogonalityDefect < 1e-8,
            $"Orthogonality defect {result.MaxOrthogonalityDefect} too large");
    }

    [Fact]
    public void ResidualNorms_AreSmall()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        foreach (var mode in result.Modes)
        {
            Assert.True(mode.ResidualNorm < 1e-8,
                $"Mode {mode.ModeIndex} residual norm {mode.ResidualNorm} too large");
        }
    }

    [Fact]
    public void NullModeDiagnosis_DetectsNearZeroEigenvalues()
    {
        // H = diag(0, 1e-12, 5, 10), M = I
        // Two near-zero eigenvalues
        var hDiag = new double[] { 0, 1e-12, 5, 10 };
        var mDiag = new double[] { 1, 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 4,
            SolverMethod = "explicit-dense",
            NullModeThreshold = 1e-6,
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.NotNull(result.NullModeDiagnosis);
        Assert.Equal(2, result.NullModeDiagnosis!.NullModeCount);
    }

    [Fact]
    public void SpectralClustering_GroupsDegenerateEigenvalues()
    {
        // H = diag(1, 1.0001, 5, 5.0002, 5.0001, 20), M = I
        // Expect 3 clusters: {1, 1.0001}, {5, 5.0001, 5.0002}, {20}
        var hDiag = new double[] { 1, 1.0001, 5, 5.0002, 5.0001, 20 };
        var mDiag = new double[] { 1, 1, 1, 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 6,
            SolverMethod = "explicit-dense",
            ClusterRelativeTolerance = 1e-2,
            ClusterAbsoluteTolerance = 0.01,
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal(3, result.Clusters.Count);

        // First cluster: multiplicity 2
        Assert.Equal(2, result.Clusters[0].Multiplicity);
        // Second cluster: multiplicity 3
        Assert.Equal(3, result.Clusters[1].Multiplicity);
        // Third cluster: multiplicity 1
        Assert.Equal(1, result.Clusters[2].Multiplicity);
    }

    [Fact]
    public void SpectralClustering_StableUnderRepeatedRuns()
    {
        var hDiag = new double[] { 1, 1.0001, 5, 5.0001, 20 };
        var mDiag = new double[] { 1, 1, 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 5,
            SolverMethod = "explicit-dense",
            ClusterRelativeTolerance = 1e-2,
            ClusterAbsoluteTolerance = 0.01,
        };

        var pipeline = new EigensolverPipeline();

        // Run twice
        var result1 = pipeline.Solve(bundle, spec);
        var result2 = pipeline.Solve(bundle, spec);

        Assert.Equal(result1.Clusters.Count, result2.Clusters.Count);
        for (int i = 0; i < result1.Clusters.Count; i++)
        {
            Assert.Equal(result1.Clusters[i].Multiplicity, result2.Clusters[i].Multiplicity);
            Assert.Equal(result1.Clusters[i].MeanEigenvalue, result2.Clusters[i].MeanEigenvalue, 10);
        }
    }

    [Fact]
    public void SpectrumBundle_SerializesCleanly()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        Assert.NotEmpty(json);
        Assert.Contains("spectrumId", json);
        Assert.Contains("modes", json);
        Assert.Contains("clusters", json);
        Assert.Contains("convergenceStatus", json);

        // Verify round-trip
        var deserialized = JsonSerializer.Deserialize<SpectrumBundle>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(result.Modes.Count, deserialized!.Modes.Count);
    }

    [Fact]
    public void ModeRecord_IncludesClusterId()
    {
        var hDiag = new double[] { 1, 1.0001, 5 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
            ClusterRelativeTolerance = 1e-2,
            ClusterAbsoluteTolerance = 0.01,
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        // First two modes should share a cluster ID
        Assert.NotNull(result.Modes[0].MultiplicityClusterId);
        Assert.Equal(result.Modes[0].MultiplicityClusterId, result.Modes[1].MultiplicityClusterId);
        // Third mode should have a different cluster
        Assert.NotEqual(result.Modes[0].MultiplicityClusterId, result.Modes[2].MultiplicityClusterId);
    }

    [Fact]
    public void LanczosSolver_DiagonalProblem_Converges()
    {
        // Use a slightly larger problem for Lanczos
        int n = 10;
        var hDiag = new double[n];
        var mDiag = new double[n];
        for (int i = 0; i < n; i++)
        {
            hDiag[i] = (i + 1) * (i + 1); // 1, 4, 9, 16, ...
            mDiag[i] = 1.0;
        }
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "lanczos",
            MaxIterations = 100,
            ConvergenceTolerance = 1e-6,
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal(3, result.Modes.Count);
        // Lanczos should find the smallest eigenvalues
        Assert.Equal(1.0, result.Modes[0].Eigenvalue, 2);
        Assert.Equal(4.0, result.Modes[1].Eigenvalue, 2);
        Assert.Equal(9.0, result.Modes[2].Eigenvalue, 2);
    }

    [Fact]
    public void LanczosSolver_NonTrivialMass_AgreesWithDense()
    {
        // H = diag(h_i), M = diag(m_i) with non-trivial M (not identity)
        // Generalized eigenvalues: h_i / m_i = 1, 2, 3, ..., 10
        int n = 10;
        var mDiag = new double[] { 2, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        var hDiag = new double[n];
        for (int i = 0; i < n; i++)
            hDiag[i] = (i + 1) * mDiag[i];
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        // Solve with dense
        var denseSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 5,
            SolverMethod = "explicit-dense",
        };
        var denseResult = new EigensolverPipeline().Solve(bundle, denseSpec);

        // Solve with Lanczos
        var lanczosSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 5,
            SolverMethod = "lanczos",
            MaxIterations = 100,
            ConvergenceTolerance = 1e-10,
        };
        var lanczosResult = new EigensolverPipeline().Solve(bundle, lanczosSpec);

        Assert.Equal(5, denseResult.Modes.Count);
        Assert.Equal(5, lanczosResult.Modes.Count);

        for (int k = 0; k < 5; k++)
        {
            double denseEig = denseResult.Modes[k].Eigenvalue;
            double lanczosEig = lanczosResult.Modes[k].Eigenvalue;
            double relError = System.Math.Abs(denseEig - lanczosEig)
                              / System.Math.Max(1e-15, System.Math.Abs(denseEig));
            Assert.True(relError < 1e-4,
                $"Mode {k}: dense={denseEig:G6}, lanczos={lanczosEig:G6}, relError={relError:E2}");
        }
    }

    [Fact]
    public void AutoMethodSelection_ChoosesDenseForSmallProblems()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "auto",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal("explicit-dense", result.SolverMethod);
    }

    [Fact]
    public void BlockEnergyFractions_SumToOne()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        foreach (var mode in result.Modes)
        {
            Assert.NotNull(mode.BlockEnergyFractions);
            double sum = mode.BlockEnergyFractions!.Values.Sum();
            Assert.Equal(1.0, sum, 6);
        }
    }

    [Fact]
    public void TensorEnergyFractions_AreNonNegative()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        foreach (var mode in result.Modes)
        {
            Assert.NotNull(mode.TensorEnergyFractions);
            foreach (var kvp in mode.TensorEnergyFractions!)
            {
                Assert.True(kvp.Value >= 0,
                    $"TensorEnergyFractions[\"{kvp.Key}\"] = {kvp.Value} is negative");
            }
        }
    }

    [Fact]
    public void ModeRecord_NewFields_DefaultToNull()
    {
        var mode = new ModeRecord
        {
            ModeId = "test",
            BackgroundId = "bg",
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = 1.0,
            ResidualNorm = 0.0,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = 0.0,
            ModeVector = new double[] { 1.0 },
            ModeIndex = 0,
        };

        Assert.Null(mode.ModeVectorArtifactRef);
        Assert.Null(mode.ObservedSignatureRef);
    }

    [Fact]
    public void ModeRecord_JsonRoundTrip_IncludesNewFields()
    {
        var mode = new ModeRecord
        {
            ModeId = "rt-test",
            BackgroundId = "bg-rt",
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = 2.5,
            ResidualNorm = 1e-8,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = 0.01,
            ModeVector = new double[] { 0.5, 0.5, 0.5, 0.5 },
            ModeIndex = 0,
            BlockEnergyFractions = new Dictionary<string, double> { ["connection"] = 1.0 },
            TensorEnergyFractions = new Dictionary<string, double> { ["connection-1form"] = 1.0 },
            ModeVectorArtifactRef = "artifacts/mode-0.bin",
            ObservedSignatureRef = "obs/sig-0.json",
        };

        var json = JsonSerializer.Serialize(mode, new JsonSerializerOptions { WriteIndented = true });
        var deserialized = JsonSerializer.Deserialize<ModeRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("rt-test", deserialized!.ModeId);
        Assert.NotNull(deserialized.BlockEnergyFractions);
        Assert.Equal(1.0, deserialized.BlockEnergyFractions!["connection"]);
        Assert.NotNull(deserialized.TensorEnergyFractions);
        Assert.Equal(1.0, deserialized.TensorEnergyFractions!["connection-1form"]);
        Assert.Equal("artifacts/mode-0.bin", deserialized.ModeVectorArtifactRef);
        Assert.Equal("obs/sig-0.json", deserialized.ObservedSignatureRef);
    }

    [Fact]
    public void ModeRecord_JsonRoundTrip_NullOptionalFields()
    {
        var mode = new ModeRecord
        {
            ModeId = "null-test",
            BackgroundId = "bg-null",
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = 1.0,
            ResidualNorm = 0.0,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = 0.0,
            ModeVector = new double[] { 1.0 },
            ModeIndex = 0,
        };

        var json = JsonSerializer.Serialize(mode);
        var deserialized = JsonSerializer.Deserialize<ModeRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized!.BlockEnergyFractions);
        Assert.Null(deserialized.TensorEnergyFractions);
        Assert.Null(deserialized.ModeVectorArtifactRef);
        Assert.Null(deserialized.ObservedSignatureRef);
    }

    [Fact]
    public void GaugeLeakScores_AreZeroWithoutProjector()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        // No gauge projector
        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        foreach (var mode in result.Modes)
            Assert.Equal(0.0, mode.GaugeLeakScore);
    }

    [Fact]
    public void LanczosSolver_LargeKrylovDim_JacobiConverges()
    {
        // 50×50 diagonal problem with identity mass.
        // The dimension-scaled maxJacobiIter = max(200, 10*krylovDim) ensures
        // the tridiagonal Jacobi step converges for larger Krylov dimensions.
        int n = 50;
        var hDiag = new double[n];
        var mDiag = new double[n];
        for (int i = 0; i < n; i++)
        {
            hDiag[i] = i + 1.0;
            mDiag[i] = 1.0;
        }
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 5,
            MaxIterations = 100,
            SolverMethod = "lanczos",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        // Lanczos with limited Krylov window on a 50-dim problem should return modes
        Assert.Contains("converged", result.ConvergenceStatus);
        Assert.Equal(5, result.Modes.Count);

        // All eigenvalues should be positive (diag entries are 1..50)
        foreach (var mode in result.Modes)
            Assert.True(mode.Eigenvalue > 0,
                $"Mode {mode.ModeIndex} eigenvalue {mode.Eigenvalue:E3} is not positive");

        // Jacobi should converge on this well-conditioned problem — no convergence warning
        bool hasJacobiWarning = result.DiagnosticNotes.Any(
            n => n.Contains("Jacobi did not converge", StringComparison.OrdinalIgnoreCase));
        Assert.False(hasJacobiWarning,
            $"Unexpected Jacobi convergence warning: {string.Join("; ", result.DiagnosticNotes)}");
    }
}
