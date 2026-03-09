namespace Gu.VulkanViewer;

/// <summary>
/// High-level Vulkan rendering context for the Geometric Unity workbench.
/// Delegates to <see cref="NativeVulkanBridge"/> for actual Vulkan operations
/// via the native gu_vulkan_native library.
///
/// Provides a unified API that accepts <see cref="VisualizationData"/> from
/// <see cref="IFieldVisualizer"/> implementations and <see cref="ViewPayload"/>
/// objects from <see cref="ViewPayloadBuilder"/>, rendering to screen or
/// exporting to image files.
///
/// The native bridge handles:
/// 1. VkInstance creation with validation layers (VK_LAYER_KHRONOS_validation).
/// 2. Physical device selection with graphics queue family.
/// 3. Logical device creation with Vulkan 1.4 baseline features.
/// 4. Swapchain or offscreen framebuffer setup.
/// 5. Render passes for mesh visualization with per-vertex color.
/// 6. Graphics pipelines for triangle mesh, wireframe overlay, and 2D plot overlay.
/// 7. Vertex/index buffer allocation and upload.
/// 8. Command buffer recording and submission.
/// </summary>
public sealed class VulkanContext : IDisposable
{
    private readonly NativeVulkanBridge _bridge = new();
    private bool _disposed;

    /// <summary>
    /// Whether the Vulkan context has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Target Vulkan API version. The project targets Vulkan 1.4.
    /// </summary>
    public static Version TargetApiVersion { get; } = new(1, 4, 0);

    /// <summary>
    /// Required validation layers for debug builds.
    /// </summary>
    public static IReadOnlyList<string> ValidationLayers { get; } = new[]
    {
        "VK_LAYER_KHRONOS_validation",
    };

    /// <summary>
    /// Required device extensions.
    /// </summary>
    public static IReadOnlyList<string> DeviceExtensions { get; } = new[]
    {
        "VK_KHR_swapchain",
    };

    /// <summary>
    /// Initializes the Vulkan context via the native bridge.
    /// Creates the Vulkan instance, selects a physical device, creates logical device,
    /// and sets up the rendering pipeline.
    /// </summary>
    /// <param name="enableValidation">Whether to enable Vulkan validation layers.</param>
    /// <param name="width">Framebuffer width in pixels.</param>
    /// <param name="height">Framebuffer height in pixels.</param>
    /// <returns>True if initialization succeeded.</returns>
    public bool Initialize(bool enableValidation = true, int width = 1920, int height = 1080)
    {
        EnsureNotDisposed();

        if (IsInitialized)
            return true;

        try
        {
            _bridge.Initialize(width, height, enableValidation);
            IsInitialized = true;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>
    /// Uploads visualization data (positions, colors, indices) to GPU buffers.
    /// </summary>
    /// <param name="data">The prepared visualization data.</param>
    public void UploadVisualizationData(VisualizationData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        EnsureNotDisposed();
        EnsureInitialized();

        var payload = new FieldViewPayload
        {
            FieldData = data,
            FieldMode = "raw",
            FieldLabel = "uploaded",
            CoefficientCount = data.VertexCount,
            FieldNorm = 0.0,
        };
        _bridge.UploadPayload(payload);
    }

    /// <summary>
    /// Uploads a view payload to the native renderer.
    /// Supports geometry, field, residual, and convergence payloads.
    /// </summary>
    /// <param name="payload">The view payload to upload.</param>
    public void UploadPayload(ViewPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        EnsureNotDisposed();
        EnsureInitialized();

        _bridge.UploadPayload(payload);
    }

    /// <summary>
    /// Renders the current frame to the display.
    /// </summary>
    public void RenderFrame()
    {
        EnsureNotDisposed();
        EnsureInitialized();

        _bridge.RenderFrame();
    }

    /// <summary>
    /// Renders the current scene to an image file.
    /// </summary>
    /// <param name="outputPath">Path for the output image (PNG).</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    public void RenderToFile(string outputPath, int width = 1920, int height = 1080)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        EnsureNotDisposed();
        EnsureInitialized();

        _bridge.RenderToFile(outputPath, width, height);
    }

    /// <summary>
    /// Sets the camera view parameters.
    /// </summary>
    public void SetCamera(
        float eyeX, float eyeY, float eyeZ,
        float targetX, float targetY, float targetZ,
        float upX = 0f, float upY = 1f, float upZ = 0f)
    {
        EnsureNotDisposed();

        _bridge.SetCamera(eyeX, eyeY, eyeZ, targetX, targetY, targetZ, upX, upY, upZ);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _bridge.Dispose();
        IsInitialized = false;
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "VulkanContext must be initialized before use. Call Initialize() first.");
        }
    }
}
