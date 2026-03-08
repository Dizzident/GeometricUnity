/*
 * Geometric Unity - Native Vulkan Bridge (stub implementation)
 *
 * All functions are stubs that return success without actual Vulkan calls.
 * Real implementation will use Vulkan 1.4 for mesh rendering, convergence
 * plot overlays, and offscreen image export.
 */

#include "gu_vulkan_bridge.h"
#include <string.h>
#include <stdio.h>

/* --- Internal state --- */

static int s_initialized = 0;
static int s_wireframe_enabled = 1;
static int s_width = 1920;
static int s_height = 1080;
static char s_last_error[256] = "";

static void set_error(const char* msg)
{
    strncpy(s_last_error, msg, sizeof(s_last_error) - 1);
    s_last_error[sizeof(s_last_error) - 1] = '\0';
}

/* --- Lifecycle --- */

int gu_vk_initialize(int enable_validation, int width, int height)
{
    if (s_initialized)
    {
        set_error("Already initialized");
        return GU_VK_ERROR;
    }

    /* STUB: Real implementation would:
     * 1. vkCreateInstance with VK_API_VERSION_1_4
     * 2. Setup debug messenger if enable_validation
     * 3. Select physical device with graphics queue
     * 4. Create logical device
     * 5. Create swapchain or offscreen framebuffer
     * 6. Create render pass, pipeline, command pool
     */

    s_width = width > 0 ? width : 1920;
    s_height = height > 0 ? height : 1080;
    s_initialized = 1;
    s_last_error[0] = '\0';

    return GU_VK_OK;
}

void gu_vk_shutdown(void)
{
    /* STUB: Real implementation would destroy all Vulkan resources
     * in reverse creation order, then destroy device and instance. */

    s_initialized = 0;
}

int gu_vk_is_initialized(void)
{
    return s_initialized;
}

/* --- Mesh upload --- */

int gu_vk_upload_mesh(
    const float* positions,
    const float* colors,
    int vertex_count,
    const uint32_t* indices,
    int index_count)
{
    if (!s_initialized)
    {
        set_error("Not initialized");
        return GU_VK_NOT_INIT;
    }

    if (!positions || !colors || !indices)
    {
        set_error("Null buffer pointer");
        return GU_VK_ERROR;
    }

    if (vertex_count <= 0 || index_count <= 0)
    {
        set_error("Invalid vertex/index count");
        return GU_VK_ERROR;
    }

    /* STUB: Real implementation would:
     * 1. Create staging buffers
     * 2. Copy vertex (position + color) data
     * 3. Transfer to device-local vertex buffer
     * 4. Create index buffer
     * 5. Update draw commands
     */

    return GU_VK_OK;
}

/* --- Rendering --- */

int gu_vk_render_frame(void)
{
    if (!s_initialized)
    {
        set_error("Not initialized");
        return GU_VK_NOT_INIT;
    }

    /* STUB: Real implementation would:
     * 1. Acquire next swapchain image
     * 2. Record render pass (mesh + wireframe + overlays)
     * 3. Submit command buffer
     * 4. Present
     */

    return GU_VK_OK;
}

int gu_vk_render_to_file(const char* output_path, int width, int height)
{
    if (!s_initialized)
    {
        set_error("Not initialized");
        return GU_VK_NOT_INIT;
    }

    if (!output_path || output_path[0] == '\0')
    {
        set_error("Invalid output path");
        return GU_VK_ERROR;
    }

    /* STUB: Real implementation would:
     * 1. Create offscreen framebuffer at specified resolution
     * 2. Record and submit render pass
     * 3. Read back pixels
     * 4. Encode as PNG and write to output_path
     */

    return GU_VK_OK;
}

/* --- Wireframe overlay --- */

void gu_vk_set_wireframe(int enabled)
{
    s_wireframe_enabled = enabled ? 1 : 0;
}

/* --- 2D overlay for convergence plots --- */

int gu_vk_upload_plot_series(
    const double* x_values,
    const double* y_values,
    int point_count,
    int series_index,
    int log_scale)
{
    if (!s_initialized)
    {
        set_error("Not initialized");
        return GU_VK_NOT_INIT;
    }

    if (!x_values || !y_values)
    {
        set_error("Null plot data pointer");
        return GU_VK_ERROR;
    }

    if (point_count <= 0)
    {
        set_error("Invalid point count");
        return GU_VK_ERROR;
    }

    if (series_index < 0 || series_index > 5)
    {
        set_error("Series index out of range [0,5]");
        return GU_VK_ERROR;
    }

    /* STUB: Real implementation would:
     * 1. Store series data in host-side buffer
     * 2. Generate 2D line geometry for the plot
     * 3. Upload as overlay vertex buffer
     */

    (void)log_scale;
    return GU_VK_OK;
}

/* --- Camera control --- */

void gu_vk_set_camera(
    float eye_x, float eye_y, float eye_z,
    float target_x, float target_y, float target_z,
    float up_x, float up_y, float up_z)
{
    /* STUB: Real implementation would compute view matrix from
     * eye, target, and up vectors and upload as push constant. */
    (void)eye_x; (void)eye_y; (void)eye_z;
    (void)target_x; (void)target_y; (void)target_z;
    (void)up_x; (void)up_y; (void)up_z;
}

/* --- Error reporting --- */

const char* gu_vk_get_last_error(void)
{
    return s_last_error;
}
