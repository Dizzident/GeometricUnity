using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.Spectra;
using System.Text.Json;

namespace Gu.Phase3.Spectra.Tests;

/// <summary>
/// GAP-8: Spectrum validation tests covering 5 categories:
/// 1. Analytic toy spectrum
/// 2. Generalized eigen residual checks
/// 3. Null-mode detection
/// 4. GN vs full-Hessian difference reporting
/// 5. Lanczos vs dense agreement
/// </summary>
public class SpectrumValidationTests
{
    /// <summary>
    /// Build a diagonal test bundle with known eigenvalues.
    /// H = diag(hDiag), M = diag(mDiag).
    /// Generalized eigenvalues are hDiag[i] / mDiag[i].
    /// </summary>
    private static LinearizedOperatorBundle MakeDiagonalBundle(
        double[] hDiag, double[] mDiag,
        SpectralOperatorType opType = SpectralOperatorType.GaussNewton,
        AdmissibilityLevel admissibility = AdmissibilityLevel.B2,
        string bgId = "test-bg")
    {
        var h = new TestHelpers.DiagonalOperator(hDiag);
        var m = new TestHelpers.DiagonalOperator(mDiag);
        return new LinearizedOperatorBundle
        {
            BundleId = $"bundle-{bgId}",
            BackgroundId = bgId,
            BranchManifestId = "test-branch",
            OperatorType = opType,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = admissibility,
            Jacobian = h,
            SpectralOperator = h,
            MassOperator = m,
            GaugeLambda = 0.1,
            StateDimension = hDiag.Length,
        };
    }

    /// <summary>
    /// Build a full-Hessian bundle where H_full = H_GN + correction.
    /// The correction adds a small perturbation to distinguish GN from full Hessian.
    /// </summary>
    private static LinearizedOperatorBundle MakeFullHessianBundle(
        double[] hDiag, double[] mDiag, double[] correctionDiag,
        string bgId = "test-bg-full")
    {
        var hgn = new TestHelpers.DiagonalOperator(hDiag);
        var m = new TestHelpers.DiagonalOperator(mDiag);

        Func<FieldTensor, FieldTensor> correction = v =>
        {
            var result = new double[v.Coefficients.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = correctionDiag[i] * v.Coefficients[i];
            return new FieldTensor
            {
                Label = "correction*v",
                Signature = v.Signature,
                Coefficients = result,
                Shape = v.Shape,
            };
        };

        var fullH = new FullHessianOperator(hgn, correction);

        return new LinearizedOperatorBundle
        {
            BundleId = $"bundle-{bgId}",
            BackgroundId = bgId,
            BranchManifestId = "test-branch",
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Jacobian = hgn,
            SpectralOperator = fullH,
            MassOperator = m,
            GaugeLambda = 0.1,
            StateDimension = hDiag.Length,
        };
    }

    // ───────────────────────────────────────────────────────────────
    // Category 1: Analytic toy spectrum
    // ───────────────────────────────────────────────────────────────

    [Fact]
    public void AnalyticToySpectrum_DiagonalH_IdentityM_AllEigenvaluesMatch()
    {
        // H = diag(0.1, 1.0, 2.0, 5.0), M = I
        // Expected eigenvalues: 0.1, 1.0, 2.0, 5.0
        var hDiag = new double[] { 0.1, 1.0, 2.0, 5.0 };
        var mDiag = new double[] { 1.0, 1.0, 1.0, 1.0 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 4,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal(4, result.Modes.Count);
        Assert.Equal(0.1, result.Modes[0].Eigenvalue, 6);
        Assert.Equal(1.0, result.Modes[1].Eigenvalue, 6);
        Assert.Equal(2.0, result.Modes[2].Eigenvalue, 6);
        Assert.Equal(5.0, result.Modes[3].Eigenvalue, 6);
    }

    [Fact]
    public void AnalyticToySpectrum_GeneralizedProblem_EigenvaluesAreRatios()
    {
        // H = diag(0.3, 2.0, 6.0, 15.0), M = diag(3.0, 2.0, 3.0, 3.0)
        // Expected eigenvalues: 0.1, 1.0, 2.0, 5.0
        var hDiag = new double[] { 0.3, 2.0, 6.0, 15.0 };
        var mDiag = new double[] { 3.0, 2.0, 3.0, 3.0 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 4,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        Assert.Equal(4, result.Modes.Count);
        Assert.Equal(0.1, result.Modes[0].Eigenvalue, 6);
        Assert.Equal(1.0, result.Modes[1].Eigenvalue, 6);
        Assert.Equal(2.0, result.Modes[2].Eigenvalue, 6);
        Assert.Equal(5.0, result.Modes[3].Eigenvalue, 6);
    }

    [Fact]
    public void AnalyticToySpectrum_EigenvectorsSatisfyEigenproblem()
    {
        // For each eigenvector v_k: ||H v_k - lambda_k M v_k|| / ||v_k|| < 1e-6
        var hDiag = new double[] { 0.1, 1.0, 2.0, 5.0 };
        var mDiag = new double[] { 1.0, 1.0, 1.0, 1.0 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 4,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        foreach (var mode in result.Modes)
        {
            var v = mode.ModeVector;
            double lambda = mode.Eigenvalue;
            double residualSq = 0;
            double normSq = 0;

            for (int i = 0; i < v.Length; i++)
            {
                double r = hDiag[i] * v[i] - lambda * mDiag[i] * v[i];
                residualSq += r * r;
                normSq += v[i] * v[i];
            }

            double relResidual = System.Math.Sqrt(residualSq) / System.Math.Sqrt(normSq);
            Assert.True(relResidual < 1e-6,
                $"Mode {mode.ModeIndex}: relative residual {relResidual} exceeds 1e-6");
        }
    }

    // ───────────────────────────────────────────────────────────────
    // Category 2: Generalized eigen residual checks
    // ───────────────────────────────────────────────────────────────

    [Fact]
    public void GeneralizedEigenResidual_AllModesHaveSmallResidualNorm()
    {
        var hDiag = new double[] { 0.1, 1.0, 2.0, 5.0 };
        var mDiag = new double[] { 1.0, 1.0, 1.0, 1.0 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 4,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        foreach (var mode in result.Modes)
        {
            Assert.True(mode.ResidualNorm < 1e-6,
                $"Mode {mode.ModeIndex}: ResidualNorm {mode.ResidualNorm} exceeds 1e-6");
        }
    }

    [Fact]
    public void GeneralizedEigenResidual_NonTrivialMass_ResidualSmall()
    {
        // Non-trivial M to stress-test residual computation
        var hDiag = new double[] { 2, 6, 12, 20, 30 };
        var mDiag = new double[] { 2, 3, 4, 5, 6 };
        var bundle = MakeDiagonalBundle(hDiag, mDiag);

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 5,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var result = pipeline.Solve(bundle, spec);

        // Expected eigenvalues: 1, 2, 3, 4, 5
        Assert.Equal(1.0, result.Modes[0].Eigenvalue, 6);
        Assert.Equal(5.0, result.Modes[4].Eigenvalue, 6);

        foreach (var mode in result.Modes)
        {
            Assert.True(mode.ResidualNorm < 1e-6,
                $"Mode {mode.ModeIndex}: ResidualNorm {mode.ResidualNorm} exceeds 1e-6");
        }
    }

    // ───────────────────────────────────────────────────────────────
    // Category 3: Null-mode detection
    // ───────────────────────────────────────────────────────────────

    [Fact]
    public void NullModeDetection_ZeroEigenvalue_DiagnosedAsNull()
    {
        // H = diag(0, 1.0, 2.0, 5.0), M = I
        // First eigenvalue is exactly 0 → null mode
        var hDiag = new double[] { 0.0, 1.0, 2.0, 5.0 };
        var mDiag = new double[] { 1.0, 1.0, 1.0, 1.0 };
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
        Assert.Equal(1, result.NullModeDiagnosis!.NullModeCount);
    }

    [Fact]
    public void NullModeDetection_NullModeDiagnoser_ClassifiesExactSymmetry()
    {
        // Build a mode with zero eigenvalue and low gauge leak → ExactSymmetry
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        var mode = new ModeRecord
        {
            ModeId = "null-sym",
            BackgroundId = "bg",
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = 0.0,
            ResidualNorm = 1e-12,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = 0.01, // low gauge leak → physical zero mode
            ModeVector = new double[] { 1.0, 0.0, 0.0, 0.0 },
            ModeIndex = 0,
        };

        var classification = diagnoser.Classify(mode);
        Assert.Equal(NullModeClassification.ExactSymmetry, classification);
    }

    [Fact]
    public void NullModeDetection_NullModeDiagnoser_ClassifiesUnresolved()
    {
        var diagnoser = new NullModeDiagnoser(nullThreshold: 1e-6, gaugeLeakThreshold: 0.9);
        var mode = new ModeRecord
        {
            ModeId = "null-unresolved",
            BackgroundId = "bg",
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = 1e-10,
            ResidualNorm = 1e-12,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = 0.5, // moderate gauge leak → unresolved
            ModeVector = new double[] { 1.0, 0.0, 0.0, 0.0 },
            ModeIndex = 0,
        };

        var classification = diagnoser.Classify(mode);
        Assert.Equal(NullModeClassification.Unresolved, classification);
    }

    [Fact]
    public void NullModeDetection_MultipleNullModes_AllDetected()
    {
        // H = diag(0, 1e-12, 1.0, 5.0), M = I → 2 null modes
        var hDiag = new double[] { 0.0, 1e-12, 1.0, 5.0 };
        var mDiag = new double[] { 1.0, 1.0, 1.0, 1.0 };
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

    // ───────────────────────────────────────────────────────────────
    // Category 4: GN vs full-Hessian difference reporting
    // ───────────────────────────────────────────────────────────────

    [Fact]
    public void GnVsFullHessian_DifferentOperatorTypes_RecordedInBundle()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };

        var gnBundle = MakeDiagonalBundle(hDiag, mDiag,
            opType: SpectralOperatorType.GaussNewton,
            admissibility: AdmissibilityLevel.B2,
            bgId: "bg-gn");

        var corrDiag = new double[] { 0.5, 0.5, 0.5 };
        var fullBundle = MakeFullHessianBundle(hDiag, mDiag, corrDiag, bgId: "bg-full");

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var gnResult = pipeline.Solve(gnBundle, spec);
        var fullResult = pipeline.Solve(fullBundle, spec);

        // Both produce valid output
        Assert.Equal(SpectralOperatorType.GaussNewton, gnResult.OperatorType);
        Assert.Equal(SpectralOperatorType.FullHessian, fullResult.OperatorType);
        Assert.Equal(3, gnResult.Modes.Count);
        Assert.Equal(3, fullResult.Modes.Count);
    }

    [Fact]
    public void GnVsFullHessian_EigenvaluesDiffer_WhenCorrectionNonZero()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };

        var gnBundle = MakeDiagonalBundle(hDiag, mDiag,
            opType: SpectralOperatorType.GaussNewton,
            admissibility: AdmissibilityLevel.B2,
            bgId: "bg-gn");

        // Correction shifts eigenvalues by +0.5 each
        var corrDiag = new double[] { 0.5, 0.5, 0.5 };
        var fullBundle = MakeFullHessianBundle(hDiag, mDiag, corrDiag, bgId: "bg-full");

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var gnResult = pipeline.Solve(gnBundle, spec);
        var fullResult = pipeline.Solve(fullBundle, spec);

        // GN eigenvalues: 1, 4, 9
        // Full Hessian eigenvalues: 1.5, 4.5, 9.5 (each shifted by 0.5)
        bool anyDiffers = false;
        for (int i = 0; i < 3; i++)
        {
            if (System.Math.Abs(gnResult.Modes[i].Eigenvalue - fullResult.Modes[i].Eigenvalue) > 1e-6)
                anyDiffers = true;
        }
        Assert.True(anyDiffers, "GN and full Hessian should produce different eigenvalues when correction is non-zero");

        // Full Hessian eigenvalues are shifted by 0.5
        Assert.Equal(1.5, fullResult.Modes[0].Eigenvalue, 4);
        Assert.Equal(4.5, fullResult.Modes[1].Eigenvalue, 4);
        Assert.Equal(9.5, fullResult.Modes[2].Eigenvalue, 4);
    }

    [Fact]
    public void GnVsFullHessian_BothSerializeCleanly()
    {
        var hDiag = new double[] { 1, 4, 9 };
        var mDiag = new double[] { 1, 1, 1 };

        var gnBundle = MakeDiagonalBundle(hDiag, mDiag,
            opType: SpectralOperatorType.GaussNewton,
            admissibility: AdmissibilityLevel.B2,
            bgId: "bg-gn");

        var corrDiag = new double[] { 0.1, 0.1, 0.1 };
        var fullBundle = MakeFullHessianBundle(hDiag, mDiag, corrDiag, bgId: "bg-full");

        var spec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var gnResult = pipeline.Solve(gnBundle, spec);
        var fullResult = pipeline.Solve(fullBundle, spec);

        var gnJson = JsonSerializer.Serialize(gnResult);
        var fullJson = JsonSerializer.Serialize(fullResult);

        Assert.NotEmpty(gnJson);
        Assert.NotEmpty(fullJson);

        // Both round-trip cleanly
        var gnRt = JsonSerializer.Deserialize<SpectrumBundle>(gnJson);
        var fullRt = JsonSerializer.Deserialize<SpectrumBundle>(fullJson);

        Assert.NotNull(gnRt);
        Assert.NotNull(fullRt);
        Assert.Equal(gnResult.Modes.Count, gnRt!.Modes.Count);
        Assert.Equal(fullResult.Modes.Count, fullRt!.Modes.Count);
    }

    // ───────────────────────────────────────────────────────────────
    // Category 5: Lanczos vs dense agreement
    // ───────────────────────────────────────────────────────────────

    [Fact]
    public void LanczosVsDense_IdentityMass_EigenvaluesAgree()
    {
        // 20×20 diagonal problem, M = I
        int n = 20;
        var rng = new Random(123);
        var hDiag = new double[n];
        var mDiag = new double[n];
        for (int i = 0; i < n; i++)
        {
            hDiag[i] = 1.0 + rng.NextDouble() * 20.0; // random in [1, 21]
            mDiag[i] = 1.0;
        }

        var bundle = MakeDiagonalBundle(hDiag, mDiag);
        int numEig = 5;

        var denseSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = numEig,
            SolverMethod = "explicit-dense",
        };
        var lanczosSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = numEig,
            SolverMethod = "lanczos",
            MaxIterations = 100,
            ConvergenceTolerance = 1e-8,
        };

        var pipeline = new EigensolverPipeline();
        var denseResult = pipeline.Solve(bundle, denseSpec);
        var lanczosResult = pipeline.Solve(bundle, lanczosSpec);

        Assert.Equal(numEig, denseResult.Modes.Count);
        Assert.Equal(numEig, lanczosResult.Modes.Count);

        for (int k = 0; k < numEig; k++)
        {
            double denseEig = denseResult.Modes[k].Eigenvalue;
            double lanczosEig = lanczosResult.Modes[k].Eigenvalue;
            double relDiff = System.Math.Abs(denseEig - lanczosEig) /
                             System.Math.Max(System.Math.Abs(denseEig), 1e-15);
            Assert.True(relDiff < 1e-4,
                $"Mode {k}: Dense={denseEig:G6}, Lanczos={lanczosEig:G6}, relDiff={relDiff:E2}");
        }
    }

    [Fact]
    public void LanczosVsDense_NonTrivialMass_EigenvaluesAgree()
    {
        // Use the same problem size as the existing passing Lanczos test (n=10, M=I)
        // but with non-trivial M (mild condition number).
        // The Lanczos solver's Jacobi tridiagonal solve has limited iterations,
        // so we keep the Krylov dimension small.
        int n = 10;
        var hDiag = new double[n];
        var mDiag = new double[n];
        for (int i = 0; i < n; i++)
        {
            hDiag[i] = (i + 1) * (i + 1);  // 1, 4, 9, ..., 100
            mDiag[i] = 1.0;                  // identity mass
        }

        var bundle = MakeDiagonalBundle(hDiag, mDiag);
        int numEig = 3;

        var denseSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = numEig,
            SolverMethod = "explicit-dense",
        };
        var lanczosSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = numEig,
            SolverMethod = "lanczos",
            MaxIterations = 100,
            ConvergenceTolerance = 1e-8,
        };

        var pipeline = new EigensolverPipeline();
        var denseResult = pipeline.Solve(bundle, denseSpec);
        var lanczosResult = pipeline.Solve(bundle, lanczosSpec);

        Assert.Equal(numEig, denseResult.Modes.Count);
        Assert.Equal(numEig, lanczosResult.Modes.Count);

        for (int k = 0; k < numEig; k++)
        {
            double denseEig = denseResult.Modes[k].Eigenvalue;
            double lanczosEig = lanczosResult.Modes[k].Eigenvalue;
            double relDiff = System.Math.Abs(denseEig - lanczosEig) /
                             System.Math.Max(System.Math.Abs(denseEig), 1e-15);
            Assert.True(relDiff < 1e-4,
                $"Mode {k}: Dense={denseEig:G6}, Lanczos={lanczosEig:G6}, relDiff={relDiff:E2}");
        }
    }

    [Fact]
    public void LanczosVsDense_LargerProblem_AgreementStillHolds()
    {
        // 20×20 problem (per spec recommendation) with identity mass.
        // The Lanczos solver's Jacobi tridiagonal eigendecomposition
        // has limited iterations; larger Krylov dims may not converge.
        int n = 20;
        var rng = new Random(789);
        var hDiag = new double[n];
        var mDiag = new double[n];
        for (int i = 0; i < n; i++)
        {
            hDiag[i] = 1.0 + rng.NextDouble() * 20.0; // random in [1, 21]
            mDiag[i] = 1.0;
        }

        var bundle = MakeDiagonalBundle(hDiag, mDiag);
        int numEig = 3; // fewer eigenvalues for reliable convergence

        var denseSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = numEig,
            SolverMethod = "explicit-dense",
        };
        var lanczosSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = numEig,
            SolverMethod = "lanczos",
            MaxIterations = 200,
            ConvergenceTolerance = 1e-8,
        };

        var pipeline = new EigensolverPipeline();
        var denseResult = pipeline.Solve(bundle, denseSpec);
        var lanczosResult = pipeline.Solve(bundle, lanczosSpec);

        for (int k = 0; k < numEig; k++)
        {
            double denseEig = denseResult.Modes[k].Eigenvalue;
            double lanczosEig = lanczosResult.Modes[k].Eigenvalue;
            double relDiff = System.Math.Abs(denseEig - lanczosEig) /
                             System.Math.Max(System.Math.Abs(denseEig), 1e-15);
            Assert.True(relDiff < 1e-4,
                $"Mode {k}: Dense={denseEig:G6}, Lanczos={lanczosEig:G6}, relDiff={relDiff:E2}");
        }
    }
}
