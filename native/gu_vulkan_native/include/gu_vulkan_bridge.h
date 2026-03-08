/*
 * Geometric Unity - Native Vulkan Bridge
 *
 * One-directional C# -> native bridge for the Vulkan workbench.
 * C# prepares view payloads and passes packed data to native for rendering.
 * The native side is read-only: it receives data but never modifies artifact state (IX-5).
 *
 * Stub implementation: all functions are no-ops or return success.
 * Real implementation will use Vulkan 1.4 for rendering.
 */

#ifndef GU_VULKAN_BRIDGE_H
#define GU_VULKAN_BRIDGE_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

/* --- Result codes --- */
#define GU_VK_OK          0
#define GU_VK_ERROR       (-1)
#define GU_VK_NOT_INIT    (-2)

/* --- Lifecycle --- */

/**
 * Initialize the Vulkan rendering context.
 * @param enable_validation  Non-zero to enable Vulkan validation layers.
 * @param width              Window/framebuffer width in pixels.
 * @param height             Window/framebuffer height in pixels.
 * @return GU_VK_OK on success.
 */
int gu_vk_initialize(int enable_validation, int width, int height);

/**
 * Shutdown and release all Vulkan resources.
 */
void gu_vk_shutdown(void);

/**
 * Check if the Vulkan context is initialized.
 * @return Non-zero if initialized.
 */
int gu_vk_is_initialized(void);

/* --- Mesh upload --- */

/**
 * Upload mesh geometry to GPU vertex/index buffers.
 * @param positions     Flat float array: [x0,y0,z0, x1,y1,z1, ...]. Length = vertex_count * 3.
 * @param colors        Flat float array: [r0,g0,b0,a0, ...]. Length = vertex_count * 4.
 * @param vertex_count  Number of vertices.
 * @param indices       Triangle index buffer. Length = triangle_count * 3.
 * @param index_count   Total number of indices (triangle_count * 3).
 * @return GU_VK_OK on success.
 */
int gu_vk_upload_mesh(
    const float* positions,
    const float* colors,
    int vertex_count,
    const uint32_t* indices,
    int index_count);

/* --- Rendering --- */

/**
 * Render the current scene to the display or offscreen framebuffer.
 * @return GU_VK_OK on success.
 */
int gu_vk_render_frame(void);

/**
 * Render the current scene to an image file.
 * @param output_path   Path for the output image (PNG).
 * @param width         Image width in pixels.
 * @param height        Image height in pixels.
 * @return GU_VK_OK on success.
 */
int gu_vk_render_to_file(const char* output_path, int width, int height);

/* --- Wireframe overlay --- */

/**
 * Enable or disable wireframe overlay rendering.
 * @param enabled  Non-zero to enable.
 */
void gu_vk_set_wireframe(int enabled);

/* --- 2D overlay for convergence plots --- */

/**
 * Upload convergence plot data for 2D overlay rendering.
 * @param x_values       X-axis values (iteration numbers).
 * @param y_values       Y-axis values (objective/norm/etc).
 * @param point_count    Number of data points.
 * @param series_index   Which series slot to upload to (0-5).
 * @param log_scale      Non-zero for logarithmic Y-axis.
 * @return GU_VK_OK on success.
 */
int gu_vk_upload_plot_series(
    const double* x_values,
    const double* y_values,
    int point_count,
    int series_index,
    int log_scale);

/* --- Camera control --- */

/**
 * Set the camera view matrix parameters.
 * @param eye_x, eye_y, eye_z       Camera position.
 * @param target_x, target_y, target_z  Look-at target.
 * @param up_x, up_y, up_z          Up vector.
 */
void gu_vk_set_camera(
    float eye_x, float eye_y, float eye_z,
    float target_x, float target_y, float target_z,
    float up_x, float up_y, float up_z);

/* --- Error reporting --- */

/**
 * Get the last error message.
 * @return Pointer to a static string (do not free).
 */
const char* gu_vk_get_last_error(void);

#ifdef __cplusplus
}
#endif

#endif /* GU_VULKAN_BRIDGE_H */
