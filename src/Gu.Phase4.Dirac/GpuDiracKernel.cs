using Gu.Geometry;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Managed GPU Dirac kernel wrapper.
///
/// When the native library is available, operations dispatch through the
/// `gu_dirac_*` surface. Otherwise the CPU reference kernel remains the
/// authoritative fallback.
/// </summary>
public sealed class GpuDiracKernel : IDiracKernel, IDisposable
{
    private readonly CpuDiracKernel _fallback;
    private readonly double[] _gammaRe;
    private readonly double[] _gammaIm;
    private readonly double[]? _chiralityRe;
    private readonly double[]? _chiralityIm;
    private readonly int[] _vertexEdgeIncidence;
    private readonly int[] _vertexEdgeOrient;
    private readonly int[] _edgeVertices;
    private readonly double[] _edgeDirectionCoeff;
    private readonly int _cellCount;
    private readonly int _spinorDim;
    private readonly int _gaugeDim;
    private readonly int _edgeCount;
    private readonly int _vertexCount;
    private readonly int _maxEdgesPerVertex;
    private bool _disposed;
    private bool _nativeActive;

    public GpuDiracKernel(CpuDiracKernel cpuFallback)
    {
        _fallback = cpuFallback ?? throw new ArgumentNullException(nameof(cpuFallback));
        _cellCount = cpuFallback.CellCount;
        _spinorDim = cpuFallback.SpinorBlockDimension;
        _gaugeDim = cpuFallback.GaugeDimension;
        _edgeCount = cpuFallback.Mesh.EdgeCount;
        _vertexCount = cpuFallback.Mesh.VertexCount;

        (_gammaRe, _gammaIm, _chiralityRe, _chiralityIm) = FlattenGammas(cpuFallback.Gammas);
        (_vertexEdgeIncidence, _vertexEdgeOrient, _maxEdgesPerVertex) = BuildVertexIncidence(cpuFallback.Mesh);
        _edgeVertices = BuildEdgeVertices(cpuFallback.Mesh);
        _edgeDirectionCoeff = BuildEdgeDirectionCoefficients(cpuFallback.Mesh, cpuFallback.SpacetimeDimension);

        _nativeActive = TryActivateNative();
    }

    public int SpinorDimension => _fallback.SpinorDimension;

    public int SpacetimeDimension => _fallback.SpacetimeDimension;

    public bool IsCudaActive => _nativeActive;

    public void ApplyGamma(int mu, ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        if (!_nativeActive)
        {
            _fallback.ApplyGamma(mu, spinor, result);
            return;
        }

        unsafe
        {
            fixed (double* spinorPtr = spinor, resultPtr = result)
            {
                CheckNativeResult(
                    DiracNativeBindings.ApplyGamma(
                        (nint)spinorPtr,
                        (nint)resultPtr,
                        mu,
                        _cellCount,
                        _spinorDim,
                        _gaugeDim),
                    nameof(DiracNativeBindings.ApplyGamma));
            }
        }
    }

    public void ApplyDirac(ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        if (!_nativeActive)
        {
            _fallback.ApplyDirac(spinor, result);
            return;
        }

        unsafe
        {
            fixed (double* spinorPtr = spinor, resultPtr = result, coeffPtr = _edgeDirectionCoeff)
            fixed (int* incidencePtr = _vertexEdgeIncidence, orientPtr = _vertexEdgeOrient, edgeVerticesPtr = _edgeVertices)
            {
                CheckNativeResult(
                    DiracNativeBindings.ApplyDirac(
                        (nint)spinorPtr,
                        (nint)resultPtr,
                        (nint)coeffPtr,
                        (nint)incidencePtr,
                        (nint)orientPtr,
                        (nint)edgeVerticesPtr,
                        _vertexCount,
                        _edgeCount,
                        _cellCount,
                        _spinorDim,
                        _gaugeDim,
                        _maxEdgesPerVertex,
                        SpacetimeDimension),
                    nameof(DiracNativeBindings.ApplyDirac));
            }
        }
    }

    public void ApplyMass(ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        if (!_nativeActive)
        {
            _fallback.ApplyMass(spinor, result);
            return;
        }

        unsafe
        {
            fixed (double* spinorPtr = spinor, resultPtr = result)
            {
                CheckNativeResult(
                    DiracNativeBindings.ApplyMass(
                        (nint)spinorPtr,
                        (nint)resultPtr,
                        nint.Zero,
                        SpinorDimension,
                        _cellCount,
                        _spinorDim,
                        _gaugeDim),
                    nameof(DiracNativeBindings.ApplyMass));
            }
        }
    }

    public void ApplyChiralityProjector(bool left, ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        if (!_nativeActive || _chiralityRe is null || _chiralityIm is null)
        {
            _fallback.ApplyChiralityProjector(left, spinor, result);
            return;
        }

        unsafe
        {
            fixed (double* spinorPtr = spinor, resultPtr = result)
            {
                int rc = DiracNativeBindings.ApplyChiralityProjector(
                    (nint)spinorPtr,
                    (nint)resultPtr,
                    left ? 1 : 0,
                    _cellCount,
                    _spinorDim,
                    _gaugeDim);

                if (rc == -2)
                {
                    _fallback.ApplyChiralityProjector(left, spinor, result);
                    return;
                }

                CheckNativeResult(rc, nameof(DiracNativeBindings.ApplyChiralityProjector));
            }
        }
    }

    public (double Real, double Imag) ComputeCouplingProxy(
        ReadOnlySpan<double> spinorI,
        ReadOnlySpan<double> spinorJ,
        ReadOnlySpan<double> bosonK)
    {
        EnsureNotDisposed();
        if (!_nativeActive)
            return _fallback.ComputeCouplingProxy(spinorI, spinorJ, bosonK);

        unsafe
        {
            fixed (double* spinorIPtr = spinorI, spinorJPtr = spinorJ, bosonPtr = bosonK)
            {
                CheckNativeResult(
                    DiracNativeBindings.ComputeCouplingProxy(
                        (nint)spinorIPtr,
                        (nint)spinorJPtr,
                        (nint)bosonPtr,
                        out double real,
                        out double imag,
                        SpinorDimension,
                        _edgeCount,
                        _cellCount,
                        _spinorDim,
                        _gaugeDim,
                        SpacetimeDimension),
                    nameof(DiracNativeBindings.ComputeCouplingProxy));

                return (real, imag);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _nativeActive = false;
            _disposed = true;
        }
    }

    private bool TryActivateNative()
    {
        try
        {
            UploadGammas();
            return true;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }

    private void UploadGammas()
    {
        unsafe
        {
            fixed (double* gammaRePtr = _gammaRe, gammaImPtr = _gammaIm)
            fixed (double* chiralityRePtr = _chiralityRe, chiralityImPtr = _chiralityIm)
            {
                CheckNativeResult(
                    DiracNativeBindings.UploadGammas(
                        (nint)gammaRePtr,
                        (nint)gammaImPtr,
                        _chiralityRe is null ? nint.Zero : (nint)chiralityRePtr,
                        _chiralityIm is null ? nint.Zero : (nint)chiralityImPtr,
                        SpacetimeDimension,
                        _spinorDim),
                    nameof(DiracNativeBindings.UploadGammas));
            }
        }
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_nativeActive)
            UploadGammas();
    }

    private static void CheckNativeResult(int result, string functionName)
    {
        if (result != 0)
            throw new InvalidOperationException($"{functionName} failed with error code {result}.");
    }

    private static (double[] GammaRe, double[] GammaIm, double[]? ChiralityRe, double[]? ChiralityIm)
        FlattenGammas(GammaOperatorBundle gammas)
    {
        int spacetimeDim = gammas.Signature.Dimension;
        int spinorDim = gammas.SpinorDimension;
        var gammaRe = new double[spacetimeDim * spinorDim * spinorDim];
        var gammaIm = new double[spacetimeDim * spinorDim * spinorDim];

        for (int mu = 0; mu < spacetimeDim; mu++)
        {
            var gamma = gammas.GammaMatrices[mu];
            for (int row = 0; row < spinorDim; row++)
            {
                for (int col = 0; col < spinorDim; col++)
                {
                    int idx = mu * spinorDim * spinorDim + row * spinorDim + col;
                    gammaRe[idx] = gamma[row, col].Real;
                    gammaIm[idx] = gamma[row, col].Imaginary;
                }
            }
        }

        if (gammas.ChiralityMatrix is null)
            return (gammaRe, gammaIm, null, null);

        var chiralityRe = new double[spinorDim * spinorDim];
        var chiralityIm = new double[spinorDim * spinorDim];
        for (int row = 0; row < spinorDim; row++)
        {
            for (int col = 0; col < spinorDim; col++)
            {
                int idx = row * spinorDim + col;
                chiralityRe[idx] = gammas.ChiralityMatrix[row, col].Real;
                chiralityIm[idx] = gammas.ChiralityMatrix[row, col].Imaginary;
            }
        }

        return (gammaRe, gammaIm, chiralityRe, chiralityIm);
    }

    private static (int[] Incidence, int[] Orientation, int MaxEdgesPerVertex) BuildVertexIncidence(SimplicialMesh mesh)
    {
        var perVertex = new List<(int Edge, int Orient)>[mesh.VertexCount];
        for (int v = 0; v < mesh.VertexCount; v++)
            perVertex[v] = new List<(int Edge, int Orient)>();

        for (int edgeIdx = 0; edgeIdx < mesh.EdgeCount; edgeIdx++)
        {
            int v0 = mesh.Edges[edgeIdx][0];
            int v1 = mesh.Edges[edgeIdx][1];
            perVertex[v0].Add((edgeIdx, -1));
            perVertex[v1].Add((edgeIdx, +1));
        }

        int maxEdges = perVertex.Max(edges => edges.Count);
        var incidence = Enumerable.Repeat(-1, mesh.VertexCount * maxEdges).ToArray();
        var orientation = new int[mesh.VertexCount * maxEdges];

        for (int v = 0; v < mesh.VertexCount; v++)
        {
            for (int i = 0; i < perVertex[v].Count; i++)
            {
                incidence[v * maxEdges + i] = perVertex[v][i].Edge;
                orientation[v * maxEdges + i] = perVertex[v][i].Orient;
            }
        }

        return (incidence, orientation, maxEdges);
    }

    private static int[] BuildEdgeVertices(SimplicialMesh mesh)
    {
        var result = new int[mesh.EdgeCount * 2];
        for (int edgeIdx = 0; edgeIdx < mesh.EdgeCount; edgeIdx++)
        {
            result[edgeIdx * 2] = mesh.Edges[edgeIdx][0];
            result[edgeIdx * 2 + 1] = mesh.Edges[edgeIdx][1];
        }

        return result;
    }

    private static double[] BuildEdgeDirectionCoefficients(SimplicialMesh mesh, int spacetimeDim)
    {
        var coeff = new double[mesh.EdgeCount * spacetimeDim];
        var coords = mesh.VertexCoordinates;
        int embDim = mesh.EmbeddingDimension;

        for (int edgeIdx = 0; edgeIdx < mesh.EdgeCount; edgeIdx++)
        {
            int v = mesh.Edges[edgeIdx][0];
            int w = mesh.Edges[edgeIdx][1];
            int dominant = 0;
            double maxAbs = 0.0;
            double sumSq = 0.0;

            for (int d = 0; d < embDim; d++)
            {
                double delta = coords[w * embDim + d] - coords[v * embDim + d];
                double abs = System.Math.Abs(delta);
                sumSq += delta * delta;
                if (abs > maxAbs)
                {
                    maxAbs = abs;
                    dominant = d;
                }
            }

            double length = System.Math.Sqrt(sumSq);
            if (length < 1e-14 || dominant >= spacetimeDim)
                continue;

            coeff[edgeIdx * spacetimeDim + dominant] = 1.0 / length;
        }

        return coeff;
    }
}
