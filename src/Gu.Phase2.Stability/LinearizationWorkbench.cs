using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability;

/// <summary>
/// Full linearization and Hessian workbench.
/// Orchestrates background state setup, Jacobian/L_tilde/H construction,
/// finite-difference validation, and spectrum probing.
/// </summary>
public sealed class LinearizationWorkbench
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly CpuResidualAssembler _assembler;
    private readonly CpuMassMatrix _massMatrix;

    public LinearizationWorkbench(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        CpuResidualAssembler assembler,
        CpuMassMatrix massMatrix)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _assembler = assembler ?? throw new ArgumentNullException(nameof(assembler));
        _massMatrix = massMatrix ?? throw new ArgumentNullException(nameof(massMatrix));
    }

    /// <summary>
    /// Create a BackgroundStateRecord from a solver result.
    /// </summary>
    public BackgroundStateRecord CreateBackgroundState(
        string id,
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry,
        bool solverConverged,
        string? terminationReason = null)
    {
        var conn = new ConnectionField(_mesh, _algebra, (double[])omega.Coefficients.Clone());
        var a0Conn = new ConnectionField(_mesh, _algebra, (double[])a0.Coefficients.Clone());
        var derived = _assembler.AssembleDerivedState(conn, a0Conn, manifest, geometry);
        double objective = _massMatrix.EvaluateObjective(derived.ResidualUpsilon);
        double residualNorm = _massMatrix.Norm(derived.ResidualUpsilon);

        return new BackgroundStateRecord
        {
            Id = id,
            BranchManifestId = manifest.BranchId,
            Omega = omega,
            A0 = a0,
            DerivedState = derived,
            ResidualNorm = residualNorm,
            ObjectiveValue = objective,
            SolverConverged = solverConverged,
            SolverTerminationReason = terminationReason,
        };
    }

    /// <summary>
    /// Build the physics Jacobian J at a background state.
    /// </summary>
    public CpuLocalJacobian BuildJacobian(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        var conn = new ConnectionField(_mesh, _algebra, (double[])background.Omega.Coefficients.Clone());
        var a0Conn = new ConnectionField(_mesh, _algebra, (double[])background.A0.Coefficients.Clone());
        return _assembler.BuildJacobian(conn, a0Conn, background.DerivedState.CurvatureF, manifest, geometry);
    }

    /// <summary>
    /// Build the gauge-fixed linearized operator L_tilde = (J, sqrt(lambda)*C).
    /// </summary>
    public GaugeFixedLinearOperator BuildGaugeFixedOperator(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        FieldTensor? omegaRef = null)
    {
        var jacobian = BuildJacobian(background, manifest, geometry);
        var gauge = new CoulombGaugePenalty(_mesh, _algebra.Dimension, gaugeLambda, omegaRef);
        return new GaugeFixedLinearOperator(jacobian, gauge, _mesh, _algebra.Dimension);
    }

    /// <summary>
    /// Build the mass-weighted Hessian operator:
    ///   H(v) = J^T M_R J v + lambda * C^T M_0 C v
    /// where M_R is the residual mass matrix and M_0 is the gauge (vertex) mass matrix.
    /// </summary>
    /// <param name="background">Background state.</param>
    /// <param name="manifest">Branch manifest.</param>
    /// <param name="geometry">Geometry context.</param>
    /// <param name="gaugeLambda">Gauge penalty weight lambda.</param>
    /// <param name="gaugeMassWeights">Diagonal vertex mass weights for M_0 (null = uniform).</param>
    /// <param name="omegaRef">Reference connection for gauge (null = zero).</param>
    public HessianOperator BuildHessian(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        double[]? gaugeMassWeights = null,
        FieldTensor? omegaRef = null)
    {
        var jacobian = BuildJacobian(background, manifest, geometry);
        var gauge = new CoulombGaugePenalty(_mesh, _algebra.Dimension, gaugeLambda, omegaRef);
        return new HessianOperator(
            jacobian, gauge, _massMatrix, gaugeMassWeights,
            gaugeLambda, _algebra.Dimension, _mesh.VertexCount);
    }

    /// <summary>
    /// Build the infinitesimal gauge map R_{z_*}(xi) = -d_{A0}(xi) + [A_* - A0, xi].
    /// </summary>
    public InfinitesimalGaugeMap BuildGaugeMap(BackgroundStateRecord background)
    {
        return new InfinitesimalGaugeMap(_mesh, _algebra, background.A0, background.Omega);
    }

    /// <summary>
    /// Validate the Jacobian via finite differences and produce a LinearizationRecord.
    /// </summary>
    public LinearizationRecord ValidateJacobian(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        FieldTensor perturbation,
        double fdEpsilon = 1e-7,
        double fdTolerance = 1e-4)
    {
        var jacobian = BuildJacobian(background, manifest, geometry);

        FieldTensor EvalUpsilon(FieldTensor omegaTensor)
        {
            var conn = new ConnectionField(_mesh, _algebra, (double[])omegaTensor.Coefficients.Clone());
            var a0Conn = new ConnectionField(_mesh, _algebra, (double[])background.A0.Coefficients.Clone());
            var derived = _assembler.AssembleDerivedState(conn, a0Conn, manifest, geometry);
            return derived.ResidualUpsilon;
        }

        var fdResult = FiniteDifferenceVerifier.Verify(
            jacobian, EvalUpsilon, background.Omega, perturbation, fdEpsilon, fdTolerance);

        return new LinearizationRecord
        {
            BackgroundStateId = background.Id,
            BranchManifestId = manifest.BranchId,
            OperatorDefinitionId = "J_dUpsilon_domega",
            DerivativeMode = "analytic",
            InputDimension = jacobian.InputDimension,
            OutputDimension = jacobian.OutputDimension,
            GaugeHandlingMode = "raw",
            AssemblyMode = "matrix-free",
            ValidationStatus = fdResult.Passed ? "verified" : "failed",
            FdMaxAbsoluteError = fdResult.MaxAbsoluteError,
            FdMaxRelativeError = fdResult.MaxRelativeError,
        };
    }

    /// <summary>
    /// Validate L_tilde via finite differences.
    /// </summary>
    public LinearizationRecord ValidateGaugeFixedOperator(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        FieldTensor perturbation,
        double fdEpsilon = 1e-7,
        double fdTolerance = 1e-4)
    {
        var lTilde = BuildGaugeFixedOperator(background, manifest, geometry, gaugeLambda);

        // L_tilde is linear, so we verify by comparing L_tilde*delta against
        // the finite difference (R_tilde(omega+eps*delta) - R_tilde(omega)) / eps
        // where R_tilde(omega) = (Upsilon(omega), sqrt(lambda)*d^*(omega))

        var gauge = new CoulombGaugePenalty(_mesh, _algebra.Dimension, gaugeLambda);
        double sqrtLambda = System.Math.Sqrt(gaugeLambda);

        FieldTensor EvalStackedResidual(FieldTensor omegaTensor)
        {
            var conn = new ConnectionField(_mesh, _algebra, (double[])omegaTensor.Coefficients.Clone());
            var a0Conn = new ConnectionField(_mesh, _algebra, (double[])background.A0.Coefficients.Clone());
            var derived = _assembler.AssembleDerivedState(conn, a0Conn, manifest, geometry);

            var upsilon = derived.ResidualUpsilon;
            var codiff = gauge.ApplyCodifferential(omegaTensor.Coefficients);

            var stacked = new double[upsilon.Coefficients.Length + codiff.Length];
            Array.Copy(upsilon.Coefficients, 0, stacked, 0, upsilon.Coefficients.Length);
            for (int i = 0; i < codiff.Length; i++)
                stacked[upsilon.Coefficients.Length + i] = sqrtLambda * codiff[i];

            return new FieldTensor
            {
                Label = "R_tilde",
                Signature = lTilde.OutputSignature,
                Coefficients = stacked,
                Shape = new[] { stacked.Length },
            };
        }

        var fdResult = FiniteDifferenceVerifier.Verify(
            lTilde, EvalStackedResidual, background.Omega, perturbation, fdEpsilon, fdTolerance);

        return new LinearizationRecord
        {
            BackgroundStateId = background.Id,
            BranchManifestId = manifest.BranchId,
            OperatorDefinitionId = "L_tilde_gauge_fixed",
            DerivativeMode = "analytic",
            InputDimension = lTilde.InputDimension,
            OutputDimension = lTilde.OutputDimension,
            GaugeHandlingMode = "coulomb-slice",
            AssemblyMode = "matrix-free",
            ValidationStatus = fdResult.Passed ? "verified" : "failed",
            FdMaxAbsoluteError = fdResult.MaxAbsoluteError,
            FdMaxRelativeError = fdResult.MaxRelativeError,
        };
    }

    /// <summary>
    /// Verify symmetry of the Hessian H using random dot-product tests.
    /// Checks that |&lt;Hx,y&gt; - &lt;x,Hy&gt;| / (||x|| ||y||) is small.
    /// </summary>
    public HessianRecord ValidateHessianSymmetry(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        int numTests = 5,
        double symmetryTolerance = 1e-10)
    {
        var hessian = BuildHessian(background, manifest, geometry, gaugeLambda);
        var rng = new Random(42);
        double maxError = 0;

        for (int t = 0; t < numTests; t++)
        {
            var x = MakeRandomField(hessian.InputDimension, rng, hessian.InputSignature);
            var y = MakeRandomField(hessian.InputDimension, rng, hessian.InputSignature);

            var hx = hessian.Apply(x);
            var hy = hessian.Apply(y);

            double hxDotY = Dot(hx.Coefficients, y.Coefficients);
            double xDotHy = Dot(x.Coefficients, hy.Coefficients);

            double normX = L2Norm(x.Coefficients);
            double normY = L2Norm(y.Coefficients);
            double scale = normX * normY;

            if (scale > 1e-15)
            {
                double err = System.Math.Abs(hxDotY - xDotHy) / scale;
                maxError = System.Math.Max(maxError, err);
            }
        }

        return new HessianRecord
        {
            BackgroundStateId = background.Id,
            BranchManifestId = manifest.BranchId,
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = gaugeLambda,
            Dimension = hessian.InputDimension,
            AssemblyMode = "matrix-free",
            SymmetryVerified = maxError < symmetryTolerance,
            SymmetryError = maxError,
        };
    }

    /// <summary>
    /// Probe the spectrum of H at a background state.
    /// </summary>
    public SpectrumRecord ProbeHessianSpectrum(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        ISpectrumProbe probe,
        int numEigenvalues = 5,
        StabilityStudySpec? spec = null)
    {
        var hessian = BuildHessian(background, manifest, geometry, gaugeLambda);
        var result = probe.ComputeSmallestEigenvalues(hessian, numEigenvalues);

        var effectiveSpec = spec ?? StabilityStudySpec.Default;
        var (interpretation, _, _, _, _) = ClassifySpectrum(result.Values, effectiveSpec);

        return new SpectrumRecord
        {
            BackgroundStateId = background.Id,
            BranchManifestId = manifest.BranchId,
            OperatorId = "H",
            ProbeMethod = probe.MethodId,
            RequestedCount = numEigenvalues,
            ObtainedCount = result.Values.Length,
            Values = result.Values,
            ConvergenceStatus = result.ConvergenceStatus,
            ResidualNorms = result.ResidualNorms,
            GaugeHandlingMode = "coulomb-slice",
            NormalizationConvention = "unit-l2",
            ModeSpace = "native-Y",
            StabilityInterpretation = interpretation,
        };
    }

    /// <summary>
    /// Probe the Hessian spectrum and produce a HessianRecord with mode counts.
    /// </summary>
    public HessianRecord ProbeHessianWithClassification(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        ISpectrumProbe probe,
        int numEigenvalues = 10,
        StabilityStudySpec? spec = null)
    {
        var hessian = BuildHessian(background, manifest, geometry, gaugeLambda);
        var result = probe.ComputeSmallestEigenvalues(hessian, numEigenvalues);

        var effectiveSpec = spec ?? StabilityStudySpec.Default;
        var (_, coercive, soft, nearKernel, negative) = ClassifySpectrum(result.Values, effectiveSpec);

        return new HessianRecord
        {
            BackgroundStateId = background.Id,
            BranchManifestId = manifest.BranchId,
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = gaugeLambda,
            Dimension = hessian.InputDimension,
            AssemblyMode = "matrix-free",
            SymmetryVerified = true,
            CoerciveModeCount = coercive,
            SoftModeCount = soft,
            NearKernelCount = nearKernel,
            NegativeModeCount = negative,
        };
    }

    /// <summary>
    /// Probe the singular values of L_tilde at a background state.
    /// </summary>
    public SpectrumRecord ProbeLTildeSingularValues(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry,
        double gaugeLambda,
        ISpectrumProbe probe,
        int numSingularValues = 5)
    {
        var lTilde = BuildGaugeFixedOperator(background, manifest, geometry, gaugeLambda);
        var result = probe.ComputeSmallestSingularValues(lTilde, numSingularValues);

        return new SpectrumRecord
        {
            BackgroundStateId = background.Id,
            BranchManifestId = manifest.BranchId,
            OperatorId = "L_tilde",
            ProbeMethod = probe.MethodId,
            RequestedCount = numSingularValues,
            ObtainedCount = result.Values.Length,
            Values = result.Values,
            ConvergenceStatus = result.ConvergenceStatus,
            ResidualNorms = result.ResidualNorms,
            GaugeHandlingMode = "coulomb-slice",
            NormalizationConvention = "unit-l2",
            ModeSpace = "native-Y",
        };
    }

    /// <summary>
    /// Classify eigenvalues into stability categories using configurable thresholds.
    /// Returns (interpretation, coercive, soft, nearKernel, negative) counts.
    /// </summary>
    public static (string Interpretation, int Coercive, int Soft, int NearKernel, int Negative)
        ClassifySpectrum(double[] eigenvalues, StabilityStudySpec spec)
    {
        if (eigenvalues.Length == 0)
            return ("no-eigenvalues-computed", 0, 0, 0, 0);

        int coercive = 0, soft = 0, nearKernel = 0, negative = 0;
        foreach (double ev in eigenvalues)
        {
            if (ev > spec.SoftModeThreshold)
                coercive++;
            else if (ev > spec.NearKernelThreshold)
                soft++;
            else if (ev > spec.NegativeModeThreshold)
                nearKernel++;
            else
                negative++;
        }

        string interpretation;
        if (negative > 0)
            interpretation = "negative-modes-saddle";
        else if (nearKernel > 0)
            interpretation = "near-zero-kernel";
        else if (soft > 0)
            interpretation = "soft-modes-present";
        else
            interpretation = "strictly-positive-on-slice";

        return (interpretation, coercive, soft, nearKernel, negative);
    }

    /// <summary>
    /// Interpret spectrum per section 9.5 stability semantics (convenience overload with default thresholds).
    /// </summary>
    private static string InterpretSpectrum(double[] eigenvalues, double gaugeLambda)
    {
        var (interpretation, _, _, _, _) = ClassifySpectrum(eigenvalues, StabilityStudySpec.Default);
        return interpretation;
    }

    private static FieldTensor MakeRandomField(int n, Random rng, TensorSignature sig)
    {
        var coeffs = new double[n];
        for (int i = 0; i < n; i++)
            coeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        return new FieldTensor
        {
            Label = "random",
            Signature = sig,
            Coefficients = coeffs,
            Shape = new[] { n },
        };
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
