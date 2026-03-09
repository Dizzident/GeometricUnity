namespace Gu.Interop;

/// <summary>
/// CPU reference implementation of INativeBackend.
/// Used for parity testing: CUDA results must match this within tolerance.
/// Operates on packed buffers in the same format as the GPU backend.
/// </summary>
public sealed class CpuReferenceBackend : INativeBackend
{
    private int _nextBufferId;
    private ManifestSnapshot? _manifest;
    private readonly Dictionary<int, double[]> _buffers = new();
    private ErrorPacket? _lastError = null;
    private bool _disposed;

    public InteropVersion Version { get; } = new()
    {
        Major = 1,
        Minor = 0,
        Patch = 0,
        BackendId = "cpu-reference",
    };

    public void Initialize(ManifestSnapshot manifest)
    {
        _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
    }

    public PackedBuffer AllocateBuffer(BufferLayoutDescriptor layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        EnsureNotDisposed();

        int id = _nextBufferId++;
        _buffers[id] = new double[layout.TotalElements];

        return new PackedBuffer
        {
            BufferId = id,
            Layout = layout,
        };
    }

    public void UploadBuffer(PackedBuffer buffer, ReadOnlySpan<double> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        EnsureNotDisposed();

        if (!_buffers.TryGetValue(buffer.BufferId, out var storage))
            throw new InvalidOperationException($"Buffer {buffer.BufferId} not found.");

        if (data.Length > storage.Length)
            throw new ArgumentException($"Data length {data.Length} exceeds buffer size {storage.Length}.");

        data.CopyTo(storage);
    }

    public void DownloadBuffer(PackedBuffer buffer, Span<double> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        EnsureNotDisposed();

        if (!_buffers.TryGetValue(buffer.BufferId, out var storage))
            throw new InvalidOperationException($"Buffer {buffer.BufferId} not found.");

        storage.AsSpan(0, System.Math.Min(storage.Length, data.Length)).CopyTo(data);
    }

    public void FreeBuffer(PackedBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _buffers.Remove(buffer.BufferId);
        buffer.Dispose();
    }

    public void EvaluateCurvature(PackedBuffer omega, PackedBuffer curvatureOut)
    {
        // F = d(omega) + (1/2)[omega, omega]
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(curvatureOut.BufferId, out var curvatureData))
            throw new InvalidOperationException("Curvature output buffer not found.");

        if (_topology is null || _algebra is null)
        {
            // Fallback: copy omega -> curvatureOut when no physics data uploaded
            int count = System.Math.Min(omegaData.Length, curvatureData.Length);
            Array.Copy(omegaData, curvatureData, count);
            return;
        }

        EvaluateCurvatureInto(omegaData, curvatureData);
    }

    public void EvaluateTorsion(PackedBuffer omega, PackedBuffer torsionOut)
    {
        // T^aug = d_{A0}(omega - A0)
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(torsionOut.BufferId, out var torsionData))
            throw new InvalidOperationException("Torsion output buffer not found.");

        if (_topology is null || _algebra is null || _a0 is null)
        {
            // Fallback: trivial torsion when no physics data uploaded
            Array.Clear(torsionData);
            return;
        }

        int faceCount = _topology.FaceCount;
        int dimG = _algebra.Dimension;
        int edgeCount = _topology.EdgeCount;
        int stride = _topology.MaxEdgesPerFace;

        // alpha = omega - A0
        var alpha = new double[edgeCount * dimG];
        for (int i = 0; i < alpha.Length; i++)
            alpha[i] = omegaData[i] - _a0[i];

        Array.Clear(torsionData, 0, torsionData.Length);

        // d_{A0}(alpha) = d(alpha) + [A0 wedge alpha] on each face
        for (int fi = 0; fi < faceCount; fi++)
        {
            // 1. d(alpha): boundary operator
            var dAlpha = new double[dimG];
            for (int i = 0; i < stride; i++)
            {
                int edgeIdx = _topology.FaceBoundaryEdges[fi * stride + i];
                if (edgeIdx < 0) break;
                int sign = _topology.FaceBoundaryOrientations[fi * stride + i];
                for (int a = 0; a < dimG; a++)
                    dAlpha[a] += sign * alpha[edgeIdx * dimG + a];
            }

            // 2. [A0 wedge alpha]: cross-term brackets (NO 0.5 factor)
            var bracketTerm = new double[dimG];
            for (int i = 0; i < stride; i++)
            {
                int ei = _topology.FaceBoundaryEdges[fi * stride + i];
                if (ei < 0) break;
                int si = _topology.FaceBoundaryOrientations[fi * stride + i];

                for (int j = i + 1; j < stride; j++)
                {
                    int ej = _topology.FaceBoundaryEdges[fi * stride + j];
                    if (ej < 0) break;
                    int sj = _topology.FaceBoundaryOrientations[fi * stride + j];

                    // [A0_i, alpha_j] + [alpha_i, A0_j]
                    ComputeBracket(_a0, ei, si, alpha, ej, sj, dimG, bracketTerm);
                    ComputeBracket(alpha, ei, si, _a0, ej, sj, dimG, bracketTerm);
                }
            }

            // d_{A0}(alpha) = d(alpha) + [A0 wedge alpha]
            for (int a = 0; a < dimG; a++)
                torsionData[fi * dimG + a] = dAlpha[a] + bracketTerm[a];
        }
    }

    public void EvaluateShiab(PackedBuffer omega, PackedBuffer shiabOut)
    {
        // Identity Shiab: S = F (curvature pass-through)
        // First compute curvature from omega, then copy to shiab output
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(shiabOut.BufferId, out var shiabData))
            throw new InvalidOperationException("Shiab output buffer not found.");

        if (_topology is null || _algebra is null)
        {
            // Fallback: copy omega -> shiabOut when no physics data uploaded
            int count = System.Math.Min(omegaData.Length, shiabData.Length);
            Array.Copy(omegaData, shiabData, count);
            return;
        }

        // Compute curvature F into shiab output buffer, then S = F
        EvaluateCurvatureInto(omegaData, shiabData);
    }

    public void EvaluateResidual(PackedBuffer shiab, PackedBuffer torsion, PackedBuffer residualOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(shiab.BufferId, out var shiabData))
            throw new InvalidOperationException("Shiab buffer not found.");
        if (!_buffers.TryGetValue(torsion.BufferId, out var torsionData))
            throw new InvalidOperationException("Torsion buffer not found.");
        if (!_buffers.TryGetValue(residualOut.BufferId, out var residualData))
            throw new InvalidOperationException("Residual output buffer not found.");

        // Upsilon = S - T
        int count = System.Math.Min(System.Math.Min(shiabData.Length, torsionData.Length), residualData.Length);
        for (int i = 0; i < count; i++)
            residualData[i] = shiabData[i] - torsionData[i];
    }

    public double EvaluateObjective(PackedBuffer residual)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(residual.BufferId, out var residualData))
            throw new InvalidOperationException("Residual buffer not found.");

        // I2_h = (1/2) * sum(Upsilon_i^2) -- simplified, no metric/quadrature
        double sum = 0.0;
        foreach (var v in residualData)
            sum += v * v;
        return 0.5 * sum;
    }

    public ErrorPacket? GetLastError() => _lastError;

    private MeshTopologyData? _topology;
    private AlgebraUploadData? _algebra;
    private double[]? _a0;

    public void UploadMeshTopology(MeshTopologyData topology)
    {
        EnsureNotDisposed();
        _topology = topology ?? throw new ArgumentNullException(nameof(topology));
    }

    public void UploadAlgebraData(AlgebraUploadData algebra)
    {
        EnsureNotDisposed();
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    public void UploadBackgroundConnection(ReadOnlySpan<double> a0Coefficients, int edgeCount, int dimG)
    {
        EnsureNotDisposed();
        _a0 = a0Coefficients.ToArray();
    }

    public bool HasPhysicsData => _topology != null && _algebra != null;

    public void EvaluateJacobianAction(PackedBuffer omega, PackedBuffer delta, PackedBuffer jvOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(delta.BufferId, out var deltaData))
            throw new InvalidOperationException("Delta buffer not found.");
        if (!_buffers.TryGetValue(jvOut.BufferId, out var jvData))
            throw new InvalidOperationException("Jv output buffer not found.");

        if (_topology is null || _algebra is null)
        {
            Array.Clear(jvData);
            return;
        }

        EvaluateJacobianActionInto(omegaData, deltaData, jvData);
    }

    public void EvaluateAdjointAction(PackedBuffer omega, PackedBuffer v, PackedBuffer jtvOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(v.BufferId, out var vData))
            throw new InvalidOperationException("V buffer not found.");
        if (!_buffers.TryGetValue(jtvOut.BufferId, out var jtvData))
            throw new InvalidOperationException("JTv output buffer not found.");

        if (_topology is null || _algebra is null)
        {
            Array.Clear(jtvData);
            return;
        }

        EvaluateAdjointActionInto(omegaData, vData, jtvData);
    }

    public void Axpy(PackedBuffer y, double alpha, PackedBuffer x, int n)
    {
        EnsureNotDisposed();
        var yData = _buffers[y.BufferId];
        var xData = _buffers[x.BufferId];
        for (int i = 0; i < n; i++)
            yData[i] += alpha * xData[i];
    }

    public double InnerProduct(PackedBuffer u, PackedBuffer v, int n)
    {
        EnsureNotDisposed();
        var uData = _buffers[u.BufferId];
        var vData = _buffers[v.BufferId];
        double sum = 0;
        for (int i = 0; i < n; i++)
            sum += uData[i] * vData[i];
        return sum;
    }

    public void Scale(PackedBuffer x, double alpha, int n)
    {
        EnsureNotDisposed();
        var xData = _buffers[x.BufferId];
        for (int i = 0; i < n; i++)
            xData[i] *= alpha;
    }

    public void Copy(PackedBuffer dst, PackedBuffer src, int n)
    {
        EnsureNotDisposed();
        var dstData = _buffers[dst.BufferId];
        var srcData = _buffers[src.BufferId];
        Array.Copy(srcData, dstData, n);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _buffers.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Compute Lie bracket [X, Y] using structure constants and accumulate into result.
    /// X is at fieldX[edgeX * dimG ..], scaled by signX; same for Y.
    /// f^c_{ab}: StructureConstants[a * dim * dim + b * dim + c].
    /// </summary>
    private void ComputeBracket(
        double[] fieldX, int edgeX, int signX,
        double[] fieldY, int edgeY, int signY,
        int dimG, double[] result)
    {
        var sc = _algebra!.StructureConstants;
        for (int a = 0; a < dimG; a++)
        {
            double xa = signX * fieldX[edgeX * dimG + a];
            for (int b = 0; b < dimG; b++)
            {
                double yb = signY * fieldY[edgeY * dimG + b];
                double xy = xa * yb;
                if (xy == 0.0) continue;
                for (int c = 0; c < dimG; c++)
                {
                    // [X, Y]^c = sum_{a,b} f^c_{ab} * X^a * Y^b
                    result[c] += sc[a * dimG * dimG + b * dimG + c] * xy;
                }
            }
        }
    }

    /// <summary>
    /// Compute curvature F = d(omega) + (1/2)[omega,omega] into a pre-allocated output array.
    /// Shared by EvaluateCurvature and EvaluateShiab (identity Shiab).
    /// </summary>
    private void EvaluateCurvatureInto(double[] omegaData, double[] outputData)
    {
        int faceCount = _topology!.FaceCount;
        int dimG = _algebra!.Dimension;
        int stride = _topology.MaxEdgesPerFace;

        Array.Clear(outputData, 0, outputData.Length);

        for (int fi = 0; fi < faceCount; fi++)
        {
            var dOmega = new double[dimG];
            for (int i = 0; i < stride; i++)
            {
                int edgeIdx = _topology.FaceBoundaryEdges[fi * stride + i];
                if (edgeIdx < 0) break;
                int sign = _topology.FaceBoundaryOrientations[fi * stride + i];
                for (int a = 0; a < dimG; a++)
                    dOmega[a] += sign * omegaData[edgeIdx * dimG + a];
            }

            var wedgeTerm = new double[dimG];
            for (int i = 0; i < stride; i++)
            {
                int ei = _topology.FaceBoundaryEdges[fi * stride + i];
                if (ei < 0) break;
                int si = _topology.FaceBoundaryOrientations[fi * stride + i];
                for (int j = i + 1; j < stride; j++)
                {
                    int ej = _topology.FaceBoundaryEdges[fi * stride + j];
                    if (ej < 0) break;
                    int sj = _topology.FaceBoundaryOrientations[fi * stride + j];
                    ComputeBracket(omegaData, ei, si, omegaData, ej, sj, dimG, wedgeTerm);
                }
            }

            for (int a = 0; a < dimG; a++)
                outputData[fi * dimG + a] = dOmega[a] + 0.5 * wedgeTerm[a];
        }
    }

    /// <summary>
    /// Compute Jacobian-vector product J*delta where J = dUpsilon/domega.
    /// For identity Shiab + augmented torsion:
    ///   J*delta = dF/domega(delta) - dT/domega(delta)
    ///
    /// Curvature linearization: dF/domega(delta) = d(delta) + 0.5*sum_{i&lt;j}([omega_i,delta_j]+[delta_i,omega_j])
    /// Torsion linearization: dT/domega(delta) = d_{A0}(delta) = d(delta) + [A0 wedge delta]
    /// </summary>
    private void EvaluateJacobianActionInto(double[] omegaData, double[] deltaData, double[] jvData)
    {
        int faceCount = _topology!.FaceCount;
        int dimG = _algebra!.Dimension;
        int stride = _topology.MaxEdgesPerFace;

        Array.Clear(jvData, 0, jvData.Length);

        for (int fi = 0; fi < faceCount; fi++)
        {
            // --- dF/domega(delta) ---
            // 1. d(delta): boundary operator on delta
            var dDelta = new double[dimG];
            for (int i = 0; i < stride; i++)
            {
                int edgeIdx = _topology.FaceBoundaryEdges[fi * stride + i];
                if (edgeIdx < 0) break;
                int sign = _topology.FaceBoundaryOrientations[fi * stride + i];
                for (int a = 0; a < dimG; a++)
                    dDelta[a] += sign * deltaData[edgeIdx * dimG + a];
            }

            // 2. 0.5 * sum_{i<j}([omega_i, delta_j] + [delta_i, omega_j])
            var curvLinear = new double[dimG];
            for (int i = 0; i < stride; i++)
            {
                int ei = _topology.FaceBoundaryEdges[fi * stride + i];
                if (ei < 0) break;
                int si = _topology.FaceBoundaryOrientations[fi * stride + i];
                for (int j = i + 1; j < stride; j++)
                {
                    int ej = _topology.FaceBoundaryEdges[fi * stride + j];
                    if (ej < 0) break;
                    int sj = _topology.FaceBoundaryOrientations[fi * stride + j];

                    // [omega_i, delta_j] + [delta_i, omega_j]
                    ComputeBracket(omegaData, ei, si, deltaData, ej, sj, dimG, curvLinear);
                    ComputeBracket(deltaData, ei, si, omegaData, ej, sj, dimG, curvLinear);
                }
            }

            // dF/domega(delta) = d(delta) + 0.5 * cross terms
            var dFdelta = new double[dimG];
            for (int a = 0; a < dimG; a++)
                dFdelta[a] = dDelta[a] + 0.5 * curvLinear[a];

            // --- dT/domega(delta) ---
            // For augmented torsion T = d_{A0}(omega - A0), dT/domega(delta) = d_{A0}(delta)
            // d_{A0}(delta) = d(delta) + [A0 wedge delta]
            var dTdelta = new double[dimG];
            if (_a0 != null)
            {
                // d(delta) part (same as dDelta computed above)
                Array.Copy(dDelta, dTdelta, dimG);

                // [A0 wedge delta] part
                var a0BracketTerm = new double[dimG];
                for (int i = 0; i < stride; i++)
                {
                    int ei = _topology.FaceBoundaryEdges[fi * stride + i];
                    if (ei < 0) break;
                    int si = _topology.FaceBoundaryOrientations[fi * stride + i];
                    for (int j = i + 1; j < stride; j++)
                    {
                        int ej = _topology.FaceBoundaryEdges[fi * stride + j];
                        if (ej < 0) break;
                        int sj = _topology.FaceBoundaryOrientations[fi * stride + j];

                        ComputeBracket(_a0, ei, si, deltaData, ej, sj, dimG, a0BracketTerm);
                        ComputeBracket(deltaData, ei, si, _a0, ej, sj, dimG, a0BracketTerm);
                    }
                }

                for (int a = 0; a < dimG; a++)
                    dTdelta[a] += a0BracketTerm[a];
            }
            // If A0 is zero/null, dT/domega(delta) = d(delta) for trivial A0

            // J*delta = dF/domega(delta) - dT/domega(delta)
            // For identity Shiab: dS/domega = dF/domega
            for (int a = 0; a < dimG; a++)
                jvData[fi * dimG + a] = dFdelta[a] - dTdelta[a];
        }
    }

    /// <summary>
    /// Compute adjoint action J^T * v by assembling contributions edge-by-edge.
    /// (J^T v)_e = sum over faces f containing edge e of the contribution from face f.
    /// </summary>
    private void EvaluateAdjointActionInto(double[] omegaData, double[] vData, double[] jtvData)
    {
        int faceCount = _topology!.FaceCount;
        int dimG = _algebra!.Dimension;
        int edgeCount = _topology.EdgeCount;
        int stride = _topology.MaxEdgesPerFace;

        Array.Clear(jtvData, 0, jtvData.Length);

        // Compute J^T * v by applying J to each basis vector e_k and dotting with v.
        // This is O(edgeCount * dimG * faceCount * dimG) which is correct for small-medium meshes.
        // A more efficient implementation would accumulate per-face contributions to each edge,
        // but that requires more complex bookkeeping.
        int nEdge = edgeCount * dimG;
        var ek = new double[nEdge];
        var jek = new double[faceCount * dimG];

        for (int k = 0; k < nEdge; k++)
        {
            ek[k] = 1.0;
            EvaluateJacobianActionInto(omegaData, ek, jek);

            double dot = 0;
            int nFace = faceCount * dimG;
            for (int i = 0; i < nFace; i++)
                dot += jek[i] * vData[i];
            jtvData[k] = dot;

            ek[k] = 0.0;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CpuReferenceBackend));
    }

    private void EnsureInitialized()
    {
        if (_manifest is null)
            throw new InvalidOperationException("Backend not initialized. Call Initialize() first.");
    }
}
