namespace Gu.Interop;

/// <summary>
/// Runs the same computation on two INativeBackend instances and
/// compares results element-by-element. Produces ParityRecord artifacts.
/// </summary>
public sealed class ParityChecker
{
    private readonly INativeBackend _reference;
    private readonly INativeBackend _target;

    public ParityChecker(INativeBackend reference, INativeBackend target)
    {
        _reference = reference ?? throw new ArgumentNullException(nameof(reference));
        _target = target ?? throw new ArgumentNullException(nameof(target));
    }

    /// <summary>
    /// Compare two result arrays element-by-element and produce a ParityRecord.
    /// </summary>
    public static ParityRecord CompareResults(
        string kernelName,
        ReadOnlySpan<double> referenceData,
        ReadOnlySpan<double> targetData,
        string cpuBackendId,
        string gpuBackendId,
        double tolerance)
    {
        if (referenceData.Length != targetData.Length)
        {
            return new ParityRecord
            {
                RecordId = Guid.NewGuid().ToString("N"),
                KernelName = kernelName,
                CpuBackendId = cpuBackendId,
                GpuBackendId = gpuBackendId,
                ElementCount = System.Math.Max(referenceData.Length, targetData.Length),
                MaxAbsoluteError = double.PositiveInfinity,
                MaxRelativeError = double.PositiveInfinity,
                L2ErrorNorm = double.PositiveInfinity,
                Tolerance = tolerance,
                Passed = false,
                Message = $"Length mismatch: reference={referenceData.Length}, target={targetData.Length}",
            };
        }

        double maxAbs = 0.0;
        double maxRel = 0.0;
        double l2Sum = 0.0;

        for (int i = 0; i < referenceData.Length; i++)
        {
            double diff = System.Math.Abs(referenceData[i] - targetData[i]);
            double refMag = System.Math.Abs(referenceData[i]);

            if (diff > maxAbs) maxAbs = diff;

            double rel = refMag > 1e-15 ? diff / refMag : diff;
            if (rel > maxRel) maxRel = rel;

            l2Sum += diff * diff;
        }

        double l2Norm = System.Math.Sqrt(l2Sum);
        bool passed = maxRel <= tolerance;

        return new ParityRecord
        {
            RecordId = Guid.NewGuid().ToString("N"),
            KernelName = kernelName,
            CpuBackendId = cpuBackendId,
            GpuBackendId = gpuBackendId,
            ElementCount = referenceData.Length,
            MaxAbsoluteError = maxAbs,
            MaxRelativeError = maxRel,
            L2ErrorNorm = l2Norm,
            Tolerance = tolerance,
            Passed = passed,
            Message = passed ? "Parity check passed" : $"Max relative error {maxRel:E3} exceeds tolerance {tolerance:E3}",
        };
    }

    /// <summary>
    /// Compare a scalar result (e.g. objective value) between two backends.
    /// </summary>
    public static ParityRecord CompareScalar(
        string kernelName,
        double referenceValue,
        double targetValue,
        string cpuBackendId,
        string gpuBackendId,
        double tolerance)
    {
        double diff = System.Math.Abs(referenceValue - targetValue);
        double refMag = System.Math.Abs(referenceValue);
        double rel = refMag > 1e-15 ? diff / refMag : diff;
        bool passed = rel <= tolerance;

        return new ParityRecord
        {
            RecordId = Guid.NewGuid().ToString("N"),
            KernelName = kernelName,
            CpuBackendId = cpuBackendId,
            GpuBackendId = gpuBackendId,
            ElementCount = 1,
            MaxAbsoluteError = diff,
            MaxRelativeError = rel,
            L2ErrorNorm = diff,
            Tolerance = tolerance,
            Passed = passed,
            Message = passed
                ? $"Parity check passed: ref={referenceValue:E6}, target={targetValue:E6}"
                : $"Parity check failed: ref={referenceValue:E6}, target={targetValue:E6}, relError={rel:E3}",
        };
    }

    /// <summary>
    /// Run the full residual pipeline on both backends and compare all intermediate results.
    /// Returns a list of ParityRecords, one per kernel stage.
    /// </summary>
    public IReadOnlyList<ParityRecord> RunFullResidualParity(
        ManifestSnapshot manifest,
        double[] omegaData,
        double tolerance = 1e-12)
    {
        var records = new List<ParityRecord>();
        string refId = _reference.Version.BackendId;
        string tgtId = _target.Version.BackendId;

        // Initialize both backends
        _reference.Initialize(manifest);
        _target.Initialize(manifest);

        int n = omegaData.Length;

        // Allocate buffers
        var layout = BufferLayoutDescriptor.CreateSoA("parity", new[] { "c" }, n);

        var refOmega = _reference.AllocateBuffer(layout);
        var tgtOmega = _target.AllocateBuffer(layout);
        var refCurv = _reference.AllocateBuffer(layout);
        var tgtCurv = _target.AllocateBuffer(layout);
        var refTorsion = _reference.AllocateBuffer(layout);
        var tgtTorsion = _target.AllocateBuffer(layout);
        var refShiab = _reference.AllocateBuffer(layout);
        var tgtShiab = _target.AllocateBuffer(layout);
        var refResidual = _reference.AllocateBuffer(layout);
        var tgtResidual = _target.AllocateBuffer(layout);

        // Upload omega
        _reference.UploadBuffer(refOmega, omegaData);
        _target.UploadBuffer(tgtOmega, omegaData);

        // Stage 1: Curvature
        _reference.EvaluateCurvature(refOmega, refCurv);
        _target.EvaluateCurvature(tgtOmega, tgtCurv);
        var refCurvData = new double[n];
        var tgtCurvData = new double[n];
        _reference.DownloadBuffer(refCurv, refCurvData);
        _target.DownloadBuffer(tgtCurv, tgtCurvData);
        records.Add(CompareResults("curvature", refCurvData, tgtCurvData, refId, tgtId, tolerance));

        // Stage 2: Torsion
        _reference.EvaluateTorsion(refOmega, refTorsion);
        _target.EvaluateTorsion(tgtOmega, tgtTorsion);
        var refTorsionData = new double[n];
        var tgtTorsionData = new double[n];
        _reference.DownloadBuffer(refTorsion, refTorsionData);
        _target.DownloadBuffer(tgtTorsion, tgtTorsionData);
        records.Add(CompareResults("torsion", refTorsionData, tgtTorsionData, refId, tgtId, tolerance));

        // Stage 3: Shiab
        _reference.EvaluateShiab(refOmega, refShiab);
        _target.EvaluateShiab(tgtOmega, tgtShiab);
        var refShiabData = new double[n];
        var tgtShiabData = new double[n];
        _reference.DownloadBuffer(refShiab, refShiabData);
        _target.DownloadBuffer(tgtShiab, tgtShiabData);
        records.Add(CompareResults("shiab", refShiabData, tgtShiabData, refId, tgtId, tolerance));

        // Stage 4: Residual
        _reference.EvaluateResidual(refShiab, refTorsion, refResidual);
        _target.EvaluateResidual(tgtShiab, tgtTorsion, tgtResidual);
        var refResidualData = new double[n];
        var tgtResidualData = new double[n];
        _reference.DownloadBuffer(refResidual, refResidualData);
        _target.DownloadBuffer(tgtResidual, tgtResidualData);
        records.Add(CompareResults("residual", refResidualData, tgtResidualData, refId, tgtId, tolerance));

        // Stage 5: Objective
        double refObj = _reference.EvaluateObjective(refResidual);
        double tgtObj = _target.EvaluateObjective(tgtResidual);
        records.Add(CompareScalar("objective", refObj, tgtObj, refId, tgtId, tolerance));

        // Cleanup
        _reference.FreeBuffer(refOmega);
        _reference.FreeBuffer(refCurv);
        _reference.FreeBuffer(refTorsion);
        _reference.FreeBuffer(refShiab);
        _reference.FreeBuffer(refResidual);
        _target.FreeBuffer(tgtOmega);
        _target.FreeBuffer(tgtCurv);
        _target.FreeBuffer(tgtTorsion);
        _target.FreeBuffer(tgtShiab);
        _target.FreeBuffer(tgtResidual);

        return records;
    }
}
