namespace Gu.VulkanViewer;

/// <summary>
/// Managed wrapper around the native Vulkan rendering bridge.
/// Provides a safe, disposable interface for uploading view data
/// and triggering renders. One-directional: C# pushes data to native.
/// </summary>
public sealed class NativeVulkanBridge : IDisposable
{
    private bool _disposed;
    private bool _initialized;

    /// <summary>
    /// Initialize the native Vulkan renderer.
    /// </summary>
    public void Initialize(int width = 1920, int height = 1080, bool enableValidation = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int result = NativeVulkanBindings.Initialize(enableValidation ? 1 : 0, width, height);
        if (result != 0)
        {
            throw new InvalidOperationException(
                $"Failed to initialize Vulkan context: {NativeVulkanBindings.GetLastError()}");
        }
        _initialized = true;
    }

    /// <summary>
    /// Upload a view payload to the native renderer.
    /// Dispatches to the appropriate upload method based on payload type.
    /// </summary>
    public void UploadPayload(ViewPayload payload)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(payload);

        switch (payload)
        {
            case GeometryViewPayload geo:
                UploadMeshData(geo.MeshData);
                NativeVulkanBindings.SetWireframe(geo.ShowWireframe ? 1 : 0);
                break;

            case FieldViewPayload field:
                UploadMeshData(field.FieldData);
                break;

            case ResidualViewPayload residual:
                UploadMeshData(residual.ResidualData);
                break;

            case ConvergenceViewPayload convergence:
                UploadConvergenceData(convergence);
                break;

            // ComparisonOverlayPayload and SpectrumViewPayload are
            // text-based overlays, not mesh data -- handled by the app layer.
        }
    }

    /// <summary>
    /// Render the current frame.
    /// </summary>
    public void RenderFrame()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();

        int result = NativeVulkanBindings.RenderFrame();
        if (result != 0)
        {
            throw new InvalidOperationException(
                $"Render failed: {NativeVulkanBindings.GetLastError()}");
        }
    }

    /// <summary>
    /// Render the current scene to an image file.
    /// </summary>
    public void RenderToFile(string outputPath, int width = 1920, int height = 1080)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        int result = NativeVulkanBindings.RenderToFile(outputPath, width, height);
        if (result != 0)
        {
            throw new InvalidOperationException(
                $"Render to file failed: {NativeVulkanBindings.GetLastError()}");
        }
    }

    /// <summary>
    /// Set the camera view parameters.
    /// </summary>
    public void SetCamera(
        float eyeX, float eyeY, float eyeZ,
        float targetX, float targetY, float targetZ,
        float upX = 0f, float upY = 1f, float upZ = 0f)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        NativeVulkanBindings.SetCamera(
            eyeX, eyeY, eyeZ,
            targetX, targetY, targetZ,
            upX, upY, upZ);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_initialized)
        {
            NativeVulkanBindings.Shutdown();
            _initialized = false;
        }
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException(
                "NativeVulkanBridge must be initialized before use.");
        }
    }

    private static void UploadMeshData(VisualizationData data)
    {
        int result = NativeVulkanBindings.UploadMesh(
            data.Positions, data.Colors, data.VertexCount,
            data.Indices, data.Indices.Length);

        if (result != 0)
        {
            throw new InvalidOperationException(
                $"Mesh upload failed: {NativeVulkanBindings.GetLastError()}");
        }
    }

    private static void UploadConvergenceData(ConvergenceViewPayload convergence)
    {
        var plotData = convergence.PlotData;
        for (int i = 0; i < plotData.Series.Count && i < 6; i++)
        {
            var series = plotData.Series[i];
            int result = NativeVulkanBindings.UploadPlotSeries(
                series.X, series.Y, series.X.Length,
                i, series.LogScaleRecommended ? 1 : 0);

            if (result != 0)
            {
                throw new InvalidOperationException(
                    $"Plot series upload failed (series {i}): {NativeVulkanBindings.GetLastError()}");
            }
        }
    }
}
