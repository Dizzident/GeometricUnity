using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.GaugeReduction;

namespace Gu.Phase3.Spectra;

/// <summary>
/// High-level orchestrator that builds a <see cref="LinearizedOperatorBundle"/>
/// and runs self-checks (symmetry, PSD) before returning it.
///
/// Wraps <see cref="OperatorBundleBuilder"/> with validation.
/// </summary>
public sealed class SpectralOperatorBundleBuilder
{
    private readonly OperatorBundleBuilder _inner;
    private readonly int _probeCount;
    private readonly double _selfCheckTol;

    /// <param name="inner">The underlying operator bundle builder.</param>
    /// <param name="probeCount">Number of random probes for self-checks (default 10).</param>
    /// <param name="selfCheckTol">Tolerance for symmetry/PSD checks (default 1e-8).</param>
    public SpectralOperatorBundleBuilder(
        OperatorBundleBuilder inner,
        int probeCount = 10,
        double selfCheckTol = 1e-8)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _probeCount = probeCount;
        _selfCheckTol = selfCheckTol;
    }

    /// <summary>
    /// Build a linearized operator bundle and run self-checks.
    /// </summary>
    /// <returns>The bundle and its self-check report.</returns>
    public (LinearizedOperatorBundle Bundle, OperatorSelfCheckReport Report) BuildWithChecks(
        LinearizedOperatorSpec spec,
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry,
        GaugeProjector? gaugeProjector = null)
    {
        var bundle = _inner.Build(spec, omega, a0, manifest, geometry, gaugeProjector);
        var report = RunSelfChecks(bundle);
        return (bundle, report);
    }

    /// <summary>
    /// Run symmetry and PSD self-checks on a bundle.
    /// </summary>
    public OperatorSelfCheckReport RunSelfChecks(LinearizedOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        int n = bundle.StateDimension;
        var rng = new Random(42);

        double maxSpectralSymErr = 0;
        double maxMassSymErr = 0;
        double minMassQuad = double.MaxValue;

        for (int t = 0; t < _probeCount; t++)
        {
            var u = MakeRandomField(n, rng);
            var v = MakeRandomField(n, rng);

            // Spectral symmetry: |u^T H v - v^T H u|
            var hu = bundle.ApplySpectral(u);
            var hv = bundle.ApplySpectral(v);
            double uHv = Dot(u.Coefficients, hv.Coefficients);
            double vHu = Dot(v.Coefficients, hu.Coefficients);
            double symErr = System.Math.Abs(uHv - vHu);
            if (symErr > maxSpectralSymErr) maxSpectralSymErr = symErr;

            // Mass symmetry: |u^T M v - v^T M u|
            var mu = bundle.ApplyMass(u);
            var mv = bundle.ApplyMass(v);
            double uMv = Dot(u.Coefficients, mv.Coefficients);
            double vMu = Dot(v.Coefficients, mu.Coefficients);
            double massSymErr = System.Math.Abs(uMv - vMu);
            if (massSymErr > maxMassSymErr) maxMassSymErr = massSymErr;

            // Mass PSD: v^T M v >= 0
            double vMv = Dot(v.Coefficients, mv.Coefficients);
            if (vMv < minMassQuad) minMassQuad = vMv;
        }

        bool passed = maxSpectralSymErr < _selfCheckTol
                   && maxMassSymErr < _selfCheckTol
                   && minMassQuad >= -_selfCheckTol;

        return new OperatorSelfCheckReport
        {
            BundleId = bundle.BundleId,
            SpectralSymmetryError = maxSpectralSymErr,
            MassSymmetryError = maxMassSymErr,
            MassMinQuadratic = minMassQuad,
            Passed = passed,
            Tolerance = _selfCheckTol,
            ProbeCount = _probeCount,
        };
    }

    private static FieldTensor MakeRandomField(int length, Random rng)
    {
        var coeffs = new double[length];
        for (int i = 0; i < length; i++)
            coeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        return new FieldTensor
        {
            Label = "probe",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "standard",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { length },
        };
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
