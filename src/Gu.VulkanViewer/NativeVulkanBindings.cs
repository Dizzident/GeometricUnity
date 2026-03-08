using System.Runtime.InteropServices;

namespace Gu.VulkanViewer;

/// <summary>
/// P/Invoke bindings for the native Vulkan rendering bridge.
/// One-directional: C# -> native only. The native side never modifies artifact data (IX-5).
/// </summary>
internal static partial class NativeVulkanBindings
{
    private const string LibName = "gu_vulkan_native";

    [LibraryImport(LibName, EntryPoint = "gu_vk_initialize")]
    internal static partial int Initialize(int enableValidation, int width, int height);

    [LibraryImport(LibName, EntryPoint = "gu_vk_shutdown")]
    internal static partial void Shutdown();

    [LibraryImport(LibName, EntryPoint = "gu_vk_is_initialized")]
    internal static partial int IsInitialized();

    [LibraryImport(LibName, EntryPoint = "gu_vk_upload_mesh")]
    internal static partial int UploadMesh(
        [In] float[] positions,
        [In] float[] colors,
        int vertexCount,
        [In] uint[] indices,
        int indexCount);

    [LibraryImport(LibName, EntryPoint = "gu_vk_render_frame")]
    internal static partial int RenderFrame();

    [LibraryImport(LibName, EntryPoint = "gu_vk_render_to_file", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int RenderToFile(string outputPath, int width, int height);

    [LibraryImport(LibName, EntryPoint = "gu_vk_set_wireframe")]
    internal static partial void SetWireframe(int enabled);

    [LibraryImport(LibName, EntryPoint = "gu_vk_upload_plot_series")]
    internal static partial int UploadPlotSeries(
        [In] double[] xValues,
        [In] double[] yValues,
        int pointCount,
        int seriesIndex,
        int logScale);

    [LibraryImport(LibName, EntryPoint = "gu_vk_set_camera")]
    internal static partial void SetCamera(
        float eyeX, float eyeY, float eyeZ,
        float targetX, float targetY, float targetZ,
        float upX, float upY, float upZ);

    [LibraryImport(LibName, EntryPoint = "gu_vk_get_last_error", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial string GetLastError();
}
