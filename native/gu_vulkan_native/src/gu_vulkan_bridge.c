/*
 * Geometric Unity - Native Vulkan Bridge
 *
 * Real Vulkan 1.x implementation for offscreen mesh rendering,
 * wireframe overlay, convergence plot overlay, and image export.
 *
 * Uses headless offscreen rendering (no window/surface/swapchain required).
 * Renders triangle meshes with per-vertex color, optional wireframe overlay,
 * and 2D line-chart convergence plots.
 *
 * Image export writes raw PPM (Portable PixMap) files for simplicity --
 * no external PNG library dependency required.
 */

#include "gu_vulkan_bridge.h"
#include <string.h>
#include <stdio.h>

#ifdef GU_HAS_VULKAN

#include <vulkan/vulkan.h>
#include <stdlib.h>
#include <math.h>

/* ================================================================
 *  Constants
 * ================================================================ */

#define MAX_PLOT_SERIES   6
#define MAX_PLOT_POINTS   8192
#define MAX_VERTICES      (1 << 20)   /* 1M vertices max */
#define MAX_INDICES       (3 << 20)   /* 3M indices max  */

/* ================================================================
 *  Internal vertex layout -- position (vec3) + color (vec4)
 * ================================================================ */

typedef struct {
    float px, py, pz;
    float cr, cg, cb, ca;
} GU_Vertex;

/* ================================================================
 *  Plot series storage (host side)
 * ================================================================ */

typedef struct {
    double x[MAX_PLOT_POINTS];
    double y[MAX_PLOT_POINTS];
    int    point_count;
    int    log_scale;
    int    active;
} GU_PlotSeries;

/* ================================================================
 *  Vulkan state
 * ================================================================ */

static struct {
    int initialized;
    int enable_validation;
    int fb_width;
    int fb_height;
    int wireframe_enabled;

    /* Vulkan core */
    VkInstance              instance;
    VkDebugUtilsMessengerEXT debug_messenger;
    VkPhysicalDevice        physical_device;
    VkDevice                device;
    uint32_t                graphics_queue_family;
    VkQueue                 graphics_queue;

    /* Memory properties */
    VkPhysicalDeviceMemoryProperties mem_props;

    /* Render pass & framebuffer (offscreen) */
    VkRenderPass            render_pass;
    VkImage                 color_image;
    VkDeviceMemory          color_memory;
    VkImageView             color_view;
    VkImage                 depth_image;
    VkDeviceMemory          depth_memory;
    VkImageView             depth_view;
    VkFramebuffer           framebuffer;

    /* Pipelines */
    VkPipelineLayout        pipeline_layout;
    VkShaderModule          vert_shader;
    VkShaderModule          frag_shader;
    VkPipeline              fill_pipeline;
    VkPipeline              wire_pipeline;
    VkPipeline              line_pipeline;     /* for 2D overlays */

    /* Command pool & buffers */
    VkCommandPool           cmd_pool;
    VkCommandBuffer         cmd_buf;

    /* Fence for synchronization */
    VkFence                 fence;

    /* Readback buffer (for render-to-file) */
    VkBuffer                readback_buffer;
    VkDeviceMemory          readback_memory;
    VkDeviceSize            readback_size;

    /* Mesh data */
    VkBuffer                vertex_buffer;
    VkDeviceMemory          vertex_memory;
    VkBuffer                index_buffer;
    VkDeviceMemory          index_memory;
    int                     vertex_count;
    int                     index_count;
    int                     mesh_uploaded;

    /* Camera - stored as a view-projection matrix (column major) */
    float                   view_proj[16];

    /* Plot overlay data */
    GU_PlotSeries           plot_series[MAX_PLOT_SERIES];

    /* Plot overlay GPU buffers */
    VkBuffer                overlay_vertex_buffer;
    VkDeviceMemory          overlay_vertex_memory;
    int                     overlay_vertex_count;
    int                     overlay_uploaded;
} s_ctx;

static char s_last_error[256] = "";

/* ================================================================
 *  Error helpers
 * ================================================================ */

static void set_error(const char* msg)
{
    strncpy(s_last_error, msg, sizeof(s_last_error) - 1);
    s_last_error[sizeof(s_last_error) - 1] = '\0';
}

static void set_errorf(const char* fmt, int code)
{
    snprintf(s_last_error, sizeof(s_last_error), fmt, code);
}

/* ================================================================
 *  Memory type helpers
 * ================================================================ */

static uint32_t find_memory_type(uint32_t type_filter, VkMemoryPropertyFlags props)
{
    for (uint32_t i = 0; i < s_ctx.mem_props.memoryTypeCount; i++)
    {
        if ((type_filter & (1u << i)) &&
            (s_ctx.mem_props.memoryTypes[i].propertyFlags & props) == props)
        {
            return i;
        }
    }
    return UINT32_MAX;
}

/* ================================================================
 *  Buffer creation helper
 * ================================================================ */

static int create_buffer(VkDeviceSize size, VkBufferUsageFlags usage,
                         VkMemoryPropertyFlags mem_flags,
                         VkBuffer* out_buf, VkDeviceMemory* out_mem)
{
    VkBufferCreateInfo buf_info = {
        .sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,
        .size  = size,
        .usage = usage,
        .sharingMode = VK_SHARING_MODE_EXCLUSIVE,
    };

    if (vkCreateBuffer(s_ctx.device, &buf_info, NULL, out_buf) != VK_SUCCESS)
    {
        set_error("vkCreateBuffer failed");
        return -1;
    }

    VkMemoryRequirements mem_req;
    vkGetBufferMemoryRequirements(s_ctx.device, *out_buf, &mem_req);

    uint32_t mt = find_memory_type(mem_req.memoryTypeBits, mem_flags);
    if (mt == UINT32_MAX)
    {
        set_error("No suitable memory type for buffer");
        vkDestroyBuffer(s_ctx.device, *out_buf, NULL);
        *out_buf = VK_NULL_HANDLE;
        return -1;
    }

    VkMemoryAllocateInfo alloc_info = {
        .sType           = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
        .allocationSize  = mem_req.size,
        .memoryTypeIndex = mt,
    };

    if (vkAllocateMemory(s_ctx.device, &alloc_info, NULL, out_mem) != VK_SUCCESS)
    {
        set_error("vkAllocateMemory failed for buffer");
        vkDestroyBuffer(s_ctx.device, *out_buf, NULL);
        *out_buf = VK_NULL_HANDLE;
        return -1;
    }

    vkBindBufferMemory(s_ctx.device, *out_buf, *out_mem, 0);
    return 0;
}

/* ================================================================
 *  Image creation helper
 * ================================================================ */

static int create_image(uint32_t w, uint32_t h, VkFormat fmt,
                        VkImageUsageFlags usage, VkImageAspectFlags aspect,
                        VkImage* out_img, VkDeviceMemory* out_mem,
                        VkImageView* out_view)
{
    VkImageCreateInfo img_info = {
        .sType         = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO,
        .imageType     = VK_IMAGE_TYPE_2D,
        .format        = fmt,
        .extent        = { w, h, 1 },
        .mipLevels     = 1,
        .arrayLayers   = 1,
        .samples       = VK_SAMPLE_COUNT_1_BIT,
        .tiling        = VK_IMAGE_TILING_OPTIMAL,
        .usage         = usage,
        .sharingMode   = VK_SHARING_MODE_EXCLUSIVE,
        .initialLayout = VK_IMAGE_LAYOUT_UNDEFINED,
    };

    if (vkCreateImage(s_ctx.device, &img_info, NULL, out_img) != VK_SUCCESS)
    {
        set_error("vkCreateImage failed");
        return -1;
    }

    VkMemoryRequirements mem_req;
    vkGetImageMemoryRequirements(s_ctx.device, *out_img, &mem_req);

    uint32_t mt = find_memory_type(mem_req.memoryTypeBits,
                                   VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
    if (mt == UINT32_MAX)
    {
        set_error("No device-local memory for image");
        vkDestroyImage(s_ctx.device, *out_img, NULL);
        return -1;
    }

    VkMemoryAllocateInfo alloc_info = {
        .sType           = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
        .allocationSize  = mem_req.size,
        .memoryTypeIndex = mt,
    };

    if (vkAllocateMemory(s_ctx.device, &alloc_info, NULL, out_mem) != VK_SUCCESS)
    {
        set_error("vkAllocateMemory failed for image");
        vkDestroyImage(s_ctx.device, *out_img, NULL);
        return -1;
    }

    vkBindImageMemory(s_ctx.device, *out_img, *out_mem, 0);

    VkImageViewCreateInfo view_info = {
        .sType    = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
        .image    = *out_img,
        .viewType = VK_IMAGE_VIEW_TYPE_2D,
        .format   = fmt,
        .subresourceRange = {
            .aspectMask     = aspect,
            .baseMipLevel   = 0,
            .levelCount     = 1,
            .baseArrayLayer = 0,
            .layerCount     = 1,
        },
    };

    if (vkCreateImageView(s_ctx.device, &view_info, NULL, out_view) != VK_SUCCESS)
    {
        set_error("vkCreateImageView failed");
        vkFreeMemory(s_ctx.device, *out_mem, NULL);
        vkDestroyImage(s_ctx.device, *out_img, NULL);
        return -1;
    }

    return 0;
}

/* ================================================================
 *  Single-shot command buffer helpers
 * ================================================================ */

static VkCommandBuffer begin_single_command(void)
{
    VkCommandBufferAllocateInfo alloc = {
        .sType              = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
        .commandPool        = s_ctx.cmd_pool,
        .level              = VK_COMMAND_BUFFER_LEVEL_PRIMARY,
        .commandBufferCount = 1,
    };
    VkCommandBuffer cb;
    vkAllocateCommandBuffers(s_ctx.device, &alloc, &cb);

    VkCommandBufferBeginInfo begin = {
        .sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
        .flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT,
    };
    vkBeginCommandBuffer(cb, &begin);
    return cb;
}

static void end_single_command(VkCommandBuffer cb)
{
    vkEndCommandBuffer(cb);

    VkSubmitInfo submit = {
        .sType              = VK_STRUCTURE_TYPE_SUBMIT_INFO,
        .commandBufferCount = 1,
        .pCommandBuffers    = &cb,
    };

    vkResetFences(s_ctx.device, 1, &s_ctx.fence);
    vkQueueSubmit(s_ctx.graphics_queue, 1, &submit, s_ctx.fence);
    vkWaitForFences(s_ctx.device, 1, &s_ctx.fence, VK_TRUE, UINT64_MAX);
    vkFreeCommandBuffers(s_ctx.device, s_ctx.cmd_pool, 1, &cb);
}

/* ================================================================
 *  Inline SPIR-V shaders
 *
 *  Vertex shader: passthrough position + color, apply push-constant
 *  view-projection matrix.
 *
 *  Fragment shader: output interpolated vertex color.
 *
 *  These are minimal SPIR-V binaries generated from GLSL:
 *
 *  --- vertex ---
 *  #version 450
 *  layout(push_constant) uniform PC { mat4 mvp; };
 *  layout(location=0) in vec3 inPos;
 *  layout(location=1) in vec4 inColor;
 *  layout(location=0) out vec4 fragColor;
 *  void main() {
 *      gl_Position = mvp * vec4(inPos, 1.0);
 *      fragColor = inColor;
 *  }
 *
 *  --- fragment ---
 *  #version 450
 *  layout(location=0) in vec4 fragColor;
 *  layout(location=0) out vec4 outColor;
 *  void main() { outColor = fragColor; }
 * ================================================================ */

/* Pre-compiled SPIR-V for the vertex shader (compiled from GLSL 450) */
static const uint32_t s_vert_spirv[] = {
    0x07230203, 0x00010000, 0x000d000b, 0x00000027,
    0x00000000, 0x00020011, 0x00000001, 0x0006000b,
    0x00000001, 0x4c534c47, 0x6474732e, 0x3035342e,
    0x00000000, 0x0003000e, 0x00000000, 0x00000001,
    0x0009000f, 0x00000000, 0x00000004, 0x6e69616d,
    0x00000000, 0x0000000d, 0x00000019, 0x00000023,
    0x00000025, 0x00030003, 0x00000002, 0x000001c2,
    0x000a0004, 0x475f4c47, 0x4c474f4f, 0x70635f45,
    0x74735f70, 0x5f656c79, 0x656e696c, 0x7269645f,
    0x69746365, 0x00006576, 0x00080004, 0x475f4c47,
    0x4c474f4f, 0x6e695f45, 0x64756c63, 0x69645f65,
    0x74636572, 0x00657669, 0x00040005, 0x00000004,
    0x6e69616d, 0x00000000, 0x00060005, 0x0000000b,
    0x505f6c67, 0x65567265, 0x78657472, 0x00000000,
    0x00060006, 0x0000000b, 0x00000000, 0x505f6c67,
    0x7469736f, 0x006e6f69, 0x00070006, 0x0000000b,
    0x00000001, 0x505f6c67, 0x746e696f, 0x657a6953,
    0x00000000, 0x00070006, 0x0000000b, 0x00000002,
    0x435f6c67, 0x4470696c, 0x61747369, 0x0065636e,
    0x00070006, 0x0000000b, 0x00000003, 0x435f6c67,
    0x446c6c75, 0x61747369, 0x0065636e, 0x00030005,
    0x0000000d, 0x00000000, 0x00030005, 0x00000011,
    0x00004350, 0x00040006, 0x00000011, 0x00000000,
    0x0070766d, 0x00030005, 0x00000013, 0x00000000,
    0x00040005, 0x00000019, 0x6f506e69, 0x00000073,
    0x00050005, 0x00000023, 0x67617266, 0x6f6c6f43,
    0x00000072, 0x00040005, 0x00000025, 0x6f436e69,
    0x00726f6c, 0x00030047, 0x0000000b, 0x00000002,
    0x00050048, 0x0000000b, 0x00000000, 0x0000000b,
    0x00000000, 0x00050048, 0x0000000b, 0x00000001,
    0x0000000b, 0x00000001, 0x00050048, 0x0000000b,
    0x00000002, 0x0000000b, 0x00000003, 0x00050048,
    0x0000000b, 0x00000003, 0x0000000b, 0x00000004,
    0x00030047, 0x00000011, 0x00000002, 0x00040048,
    0x00000011, 0x00000000, 0x00000005, 0x00050048,
    0x00000011, 0x00000000, 0x00000007, 0x00000010,
    0x00050048, 0x00000011, 0x00000000, 0x00000023,
    0x00000000, 0x00040047, 0x00000019, 0x0000001e,
    0x00000000, 0x00040047, 0x00000023, 0x0000001e,
    0x00000000, 0x00040047, 0x00000025, 0x0000001e,
    0x00000001, 0x00020013, 0x00000002, 0x00030021,
    0x00000003, 0x00000002, 0x00030016, 0x00000006,
    0x00000020, 0x00040017, 0x00000007, 0x00000006,
    0x00000004, 0x00040015, 0x00000008, 0x00000020,
    0x00000000, 0x0004002b, 0x00000008, 0x00000009,
    0x00000001, 0x0004001c, 0x0000000a, 0x00000006,
    0x00000009, 0x0006001e, 0x0000000b, 0x00000007,
    0x00000006, 0x0000000a, 0x0000000a, 0x00040020,
    0x0000000c, 0x00000003, 0x0000000b, 0x0004003b,
    0x0000000c, 0x0000000d, 0x00000003, 0x00040015,
    0x0000000e, 0x00000020, 0x00000001, 0x0004002b,
    0x0000000e, 0x0000000f, 0x00000000, 0x00040018,
    0x00000010, 0x00000007, 0x00000004, 0x0003001e,
    0x00000011, 0x00000010, 0x00040020, 0x00000012,
    0x00000009, 0x00000011, 0x0004003b, 0x00000012,
    0x00000013, 0x00000009, 0x00040020, 0x00000014,
    0x00000009, 0x00000010, 0x00040017, 0x00000017,
    0x00000006, 0x00000003, 0x00040020, 0x00000018,
    0x00000001, 0x00000017, 0x0004003b, 0x00000018,
    0x00000019, 0x00000001, 0x0004002b, 0x00000006,
    0x0000001b, 0x3f800000, 0x00040020, 0x00000021,
    0x00000003, 0x00000007, 0x0004003b, 0x00000021,
    0x00000023, 0x00000003, 0x00040020, 0x00000024,
    0x00000001, 0x00000007, 0x0004003b, 0x00000024,
    0x00000025, 0x00000001, 0x00050036, 0x00000002,
    0x00000004, 0x00000000, 0x00000003, 0x000200f8,
    0x00000005, 0x00050041, 0x00000014, 0x00000015,
    0x00000013, 0x0000000f, 0x0004003d, 0x00000010,
    0x00000016, 0x00000015, 0x0004003d, 0x00000017,
    0x0000001a, 0x00000019, 0x00050051, 0x00000006,
    0x0000001c, 0x0000001a, 0x00000000, 0x00050051,
    0x00000006, 0x0000001d, 0x0000001a, 0x00000001,
    0x00050051, 0x00000006, 0x0000001e, 0x0000001a,
    0x00000002, 0x00070050, 0x00000007, 0x0000001f,
    0x0000001c, 0x0000001d, 0x0000001e, 0x0000001b,
    0x00050091, 0x00000007, 0x00000020, 0x00000016,
    0x0000001f, 0x00050041, 0x00000021, 0x00000022,
    0x0000000d, 0x0000000f, 0x0003003e, 0x00000022,
    0x00000020, 0x0004003d, 0x00000007, 0x00000026,
    0x00000025, 0x0003003e, 0x00000023, 0x00000026,
    0x000100fd, 0x00010038,
};

/* Pre-compiled SPIR-V for the fragment shader (compiled from GLSL 450) */
static const uint32_t s_frag_spirv[] = {
    0x07230203, 0x00010000, 0x000d000b, 0x0000000d,
    0x00000000, 0x00020011, 0x00000001, 0x0006000b,
    0x00000001, 0x4c534c47, 0x6474732e, 0x3035342e,
    0x00000000, 0x0003000e, 0x00000000, 0x00000001,
    0x0007000f, 0x00000004, 0x00000004, 0x6e69616d,
    0x00000000, 0x00000009, 0x0000000b, 0x00030010,
    0x00000004, 0x00000007, 0x00030003, 0x00000002,
    0x000001c2, 0x000a0004, 0x475f4c47, 0x4c474f4f,
    0x70635f45, 0x74735f70, 0x5f656c79, 0x656e696c,
    0x7269645f, 0x69746365, 0x00006576, 0x00080004,
    0x475f4c47, 0x4c474f4f, 0x6e695f45, 0x64756c63,
    0x69645f65, 0x74636572, 0x00657669, 0x00040005,
    0x00000004, 0x6e69616d, 0x00000000, 0x00050005,
    0x00000009, 0x4374756f, 0x726f6c6f, 0x00000000,
    0x00050005, 0x0000000b, 0x67617266, 0x6f6c6f43,
    0x00000072, 0x00040047, 0x00000009, 0x0000001e,
    0x00000000, 0x00040047, 0x0000000b, 0x0000001e,
    0x00000000, 0x00020013, 0x00000002, 0x00030021,
    0x00000003, 0x00000002, 0x00030016, 0x00000006,
    0x00000020, 0x00040017, 0x00000007, 0x00000006,
    0x00000004, 0x00040020, 0x00000008, 0x00000003,
    0x00000007, 0x0004003b, 0x00000008, 0x00000009,
    0x00000003, 0x00040020, 0x0000000a, 0x00000001,
    0x00000007, 0x0004003b, 0x0000000a, 0x0000000b,
    0x00000001, 0x00050036, 0x00000002, 0x00000004,
    0x00000000, 0x00000003, 0x000200f8, 0x00000005,
    0x0004003d, 0x00000007, 0x0000000c, 0x0000000b,
    0x0003003e, 0x00000009, 0x0000000c, 0x000100fd,
    0x00010038,
};

/* ================================================================
 *  Shader module creation
 * ================================================================ */

static VkShaderModule create_shader_module(const uint32_t* code, size_t code_size)
{
    VkShaderModuleCreateInfo ci = {
        .sType    = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO,
        .codeSize = code_size,
        .pCode    = code,
    };
    VkShaderModule module = VK_NULL_HANDLE;
    vkCreateShaderModule(s_ctx.device, &ci, NULL, &module);
    return module;
}

/* ================================================================
 *  View/projection matrix math
 * ================================================================ */

static void mat4_identity(float m[16])
{
    memset(m, 0, 16 * sizeof(float));
    m[0] = m[5] = m[10] = m[15] = 1.0f;
}

static void mat4_look_at(float m[16],
                         float ex, float ey, float ez,
                         float tx, float ty, float tz,
                         float ux, float uy, float uz)
{
    float fx = tx - ex, fy = ty - ey, fz = tz - ez;
    float fl = sqrtf(fx*fx + fy*fy + fz*fz);
    if (fl < 1e-12f) { mat4_identity(m); return; }
    fx /= fl; fy /= fl; fz /= fl;

    /* right = normalize(cross(forward, up)) */
    float rx = fy*uz - fz*uy;
    float ry = fz*ux - fx*uz;
    float rz = fx*uy - fy*ux;
    float rl = sqrtf(rx*rx + ry*ry + rz*rz);
    if (rl < 1e-12f) { mat4_identity(m); return; }
    rx /= rl; ry /= rl; rz /= rl;

    /* recalc up = cross(right, forward) */
    float u2x = ry*fz - rz*fy;
    float u2y = rz*fx - rx*fz;
    float u2z = rx*fy - ry*fx;

    /* Column-major for Vulkan push constants */
    m[0]  = rx;  m[1]  = u2x; m[2]  = -fx; m[3]  = 0.0f;
    m[4]  = ry;  m[5]  = u2y; m[6]  = -fy; m[7]  = 0.0f;
    m[8]  = rz;  m[9]  = u2z; m[10] = -fz; m[11] = 0.0f;
    m[12] = -(rx*ex + ry*ey + rz*ez);
    m[13] = -(u2x*ex + u2y*ey + u2z*ez);
    m[14] =  (fx*ex + fy*ey + fz*ez);
    m[15] = 1.0f;
}

static void mat4_perspective(float m[16], float fov_rad,
                             float aspect, float near, float far)
{
    float f = 1.0f / tanf(fov_rad * 0.5f);
    memset(m, 0, 16 * sizeof(float));
    m[0]  = f / aspect;
    m[5]  = -f;  /* Vulkan Y-axis is flipped */
    m[10] = far / (near - far);
    m[11] = -1.0f;
    m[14] = (near * far) / (near - far);
}

static void mat4_mul(float out[16], const float a[16], const float b[16])
{
    for (int c = 0; c < 4; c++)
    {
        for (int r = 0; r < 4; r++)
        {
            float sum = 0.0f;
            for (int k = 0; k < 4; k++)
                sum += a[k*4 + r] * b[c*4 + k];
            out[c*4 + r] = sum;
        }
    }
}

/* ================================================================
 *  Depth format selection
 * ================================================================ */

static VkFormat find_depth_format(void)
{
    VkFormat candidates[] = {
        VK_FORMAT_D32_SFLOAT,
        VK_FORMAT_D32_SFLOAT_S8_UINT,
        VK_FORMAT_D24_UNORM_S8_UINT,
    };
    for (int i = 0; i < 3; i++)
    {
        VkFormatProperties props;
        vkGetPhysicalDeviceFormatProperties(s_ctx.physical_device,
                                            candidates[i], &props);
        if (props.optimalTilingFeatures &
            VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT)
        {
            return candidates[i];
        }
    }
    return VK_FORMAT_D32_SFLOAT; /* fallback */
}

/* ================================================================
 *  Debug messenger callback
 * ================================================================ */

static VKAPI_ATTR VkBool32 VKAPI_CALL debug_callback(
    VkDebugUtilsMessageSeverityFlagBitsEXT severity,
    VkDebugUtilsMessageTypeFlagsEXT type,
    const VkDebugUtilsMessengerCallbackDataEXT* data,
    void* user_data)
{
    (void)severity; (void)type; (void)user_data;
    fprintf(stderr, "[GU Vulkan] %s\n", data->pMessage);
    return VK_FALSE;
}

/* ================================================================
 *  Create offscreen framebuffer at given dimensions
 * ================================================================ */

static int create_offscreen_framebuffer(int width, int height)
{
    VkFormat color_format = VK_FORMAT_R8G8B8A8_UNORM;
    VkFormat depth_format = find_depth_format();

    /* Color attachment */
    if (create_image((uint32_t)width, (uint32_t)height, color_format,
                     VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT |
                     VK_IMAGE_USAGE_TRANSFER_SRC_BIT,
                     VK_IMAGE_ASPECT_COLOR_BIT,
                     &s_ctx.color_image, &s_ctx.color_memory,
                     &s_ctx.color_view) != 0)
        return -1;

    /* Depth attachment */
    if (create_image((uint32_t)width, (uint32_t)height, depth_format,
                     VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT,
                     VK_IMAGE_ASPECT_DEPTH_BIT,
                     &s_ctx.depth_image, &s_ctx.depth_memory,
                     &s_ctx.depth_view) != 0)
        return -1;

    /* Render pass */
    VkAttachmentDescription attachments[2] = {
        {   /* color */
            .format         = color_format,
            .samples        = VK_SAMPLE_COUNT_1_BIT,
            .loadOp         = VK_ATTACHMENT_LOAD_OP_CLEAR,
            .storeOp        = VK_ATTACHMENT_STORE_OP_STORE,
            .stencilLoadOp  = VK_ATTACHMENT_LOAD_OP_DONT_CARE,
            .stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE,
            .initialLayout  = VK_IMAGE_LAYOUT_UNDEFINED,
            .finalLayout    = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        },
        {   /* depth */
            .format         = depth_format,
            .samples        = VK_SAMPLE_COUNT_1_BIT,
            .loadOp         = VK_ATTACHMENT_LOAD_OP_CLEAR,
            .storeOp        = VK_ATTACHMENT_STORE_OP_DONT_CARE,
            .stencilLoadOp  = VK_ATTACHMENT_LOAD_OP_DONT_CARE,
            .stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE,
            .initialLayout  = VK_IMAGE_LAYOUT_UNDEFINED,
            .finalLayout    = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL,
        },
    };

    VkAttachmentReference color_ref = {
        .attachment = 0,
        .layout     = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL,
    };
    VkAttachmentReference depth_ref = {
        .attachment = 1,
        .layout     = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL,
    };

    VkSubpassDescription subpass = {
        .pipelineBindPoint       = VK_PIPELINE_BIND_POINT_GRAPHICS,
        .colorAttachmentCount    = 1,
        .pColorAttachments       = &color_ref,
        .pDepthStencilAttachment = &depth_ref,
    };

    VkSubpassDependency dep = {
        .srcSubpass    = VK_SUBPASS_EXTERNAL,
        .dstSubpass    = 0,
        .srcStageMask  = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT |
                         VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT,
        .dstStageMask  = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT |
                         VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT,
        .srcAccessMask = 0,
        .dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT |
                         VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT,
    };

    VkRenderPassCreateInfo rp_info = {
        .sType           = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO,
        .attachmentCount = 2,
        .pAttachments    = attachments,
        .subpassCount    = 1,
        .pSubpasses      = &subpass,
        .dependencyCount = 1,
        .pDependencies   = &dep,
    };

    if (vkCreateRenderPass(s_ctx.device, &rp_info, NULL,
                           &s_ctx.render_pass) != VK_SUCCESS)
    {
        set_error("vkCreateRenderPass failed");
        return -1;
    }

    /* Framebuffer */
    VkImageView fb_attachments[2] = { s_ctx.color_view, s_ctx.depth_view };

    VkFramebufferCreateInfo fb_info = {
        .sType           = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO,
        .renderPass      = s_ctx.render_pass,
        .attachmentCount = 2,
        .pAttachments    = fb_attachments,
        .width           = (uint32_t)width,
        .height          = (uint32_t)height,
        .layers          = 1,
    };

    if (vkCreateFramebuffer(s_ctx.device, &fb_info, NULL,
                            &s_ctx.framebuffer) != VK_SUCCESS)
    {
        set_error("vkCreateFramebuffer failed");
        return -1;
    }

    return 0;
}

/* ================================================================
 *  Pipeline creation
 * ================================================================ */

static int create_pipelines(void)
{
    /* Push constant range: mat4 view-projection (64 bytes) */
    VkPushConstantRange pc_range = {
        .stageFlags = VK_SHADER_STAGE_VERTEX_BIT,
        .offset     = 0,
        .size       = 64,
    };

    VkPipelineLayoutCreateInfo layout_info = {
        .sType                  = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO,
        .pushConstantRangeCount = 1,
        .pPushConstantRanges    = &pc_range,
    };

    if (vkCreatePipelineLayout(s_ctx.device, &layout_info, NULL,
                               &s_ctx.pipeline_layout) != VK_SUCCESS)
    {
        set_error("vkCreatePipelineLayout failed");
        return -1;
    }

    /* Shader modules */
    s_ctx.vert_shader = create_shader_module(s_vert_spirv,
                                             sizeof(s_vert_spirv));
    s_ctx.frag_shader = create_shader_module(s_frag_spirv,
                                             sizeof(s_frag_spirv));

    if (!s_ctx.vert_shader || !s_ctx.frag_shader)
    {
        set_error("Shader module creation failed");
        return -1;
    }

    /* Shader stages */
    VkPipelineShaderStageCreateInfo stages[2] = {
        {
            .sType  = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO,
            .stage  = VK_SHADER_STAGE_VERTEX_BIT,
            .module = s_ctx.vert_shader,
            .pName  = "main",
        },
        {
            .sType  = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO,
            .stage  = VK_SHADER_STAGE_FRAGMENT_BIT,
            .module = s_ctx.frag_shader,
            .pName  = "main",
        },
    };

    /* Vertex input: position (vec3) at location 0, color (vec4) at location 1 */
    VkVertexInputBindingDescription binding = {
        .binding   = 0,
        .stride    = sizeof(GU_Vertex),
        .inputRate = VK_VERTEX_INPUT_RATE_VERTEX,
    };

    VkVertexInputAttributeDescription attrs[2] = {
        { .location = 0, .binding = 0, .format = VK_FORMAT_R32G32B32_SFLOAT,
          .offset = offsetof(GU_Vertex, px) },
        { .location = 1, .binding = 0, .format = VK_FORMAT_R32G32B32A32_SFLOAT,
          .offset = offsetof(GU_Vertex, cr) },
    };

    VkPipelineVertexInputStateCreateInfo vertex_input = {
        .sType                           = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO,
        .vertexBindingDescriptionCount   = 1,
        .pVertexBindingDescriptions      = &binding,
        .vertexAttributeDescriptionCount = 2,
        .pVertexAttributeDescriptions    = attrs,
    };

    /* Input assembly -- triangles for fill, lines for wireframe/overlay */
    VkPipelineInputAssemblyStateCreateInfo ia_tri = {
        .sType    = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO,
        .topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST,
    };

    VkPipelineInputAssemblyStateCreateInfo ia_line = {
        .sType    = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO,
        .topology = VK_PRIMITIVE_TOPOLOGY_LINE_LIST,
    };

    VkPipelineInputAssemblyStateCreateInfo ia_line_strip = {
        .sType    = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO,
        .topology = VK_PRIMITIVE_TOPOLOGY_LINE_STRIP,
    };

    /* Viewport / scissor -- dynamic */
    VkPipelineViewportStateCreateInfo viewport_state = {
        .sType         = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO,
        .viewportCount = 1,
        .scissorCount  = 1,
    };

    /* Rasterizer for fill */
    VkPipelineRasterizationStateCreateInfo raster_fill = {
        .sType       = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO,
        .polygonMode = VK_POLYGON_MODE_FILL,
        .cullMode    = VK_CULL_MODE_BACK_BIT,
        .frontFace   = VK_FRONT_FACE_COUNTER_CLOCKWISE,
        .lineWidth   = 1.0f,
    };

    /* Rasterizer for wireframe */
    VkPipelineRasterizationStateCreateInfo raster_wire = {
        .sType       = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO,
        .polygonMode = VK_POLYGON_MODE_LINE,
        .cullMode    = VK_CULL_MODE_NONE,
        .frontFace   = VK_FRONT_FACE_COUNTER_CLOCKWISE,
        .lineWidth   = 1.0f,
        .depthBiasEnable    = VK_TRUE,
        .depthBiasConstantFactor = -1.0f,
        .depthBiasSlopeFactor    = -1.0f,
    };

    /* Rasterizer for line overlay (no depth) */
    VkPipelineRasterizationStateCreateInfo raster_line = {
        .sType       = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO,
        .polygonMode = VK_POLYGON_MODE_FILL,
        .cullMode    = VK_CULL_MODE_NONE,
        .frontFace   = VK_FRONT_FACE_COUNTER_CLOCKWISE,
        .lineWidth   = 2.0f,
    };

    /* Multisampling -- no MSAA */
    VkPipelineMultisampleStateCreateInfo msaa = {
        .sType                = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO,
        .rasterizationSamples = VK_SAMPLE_COUNT_1_BIT,
    };

    /* Depth/stencil */
    VkPipelineDepthStencilStateCreateInfo depth_stencil = {
        .sType            = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO,
        .depthTestEnable  = VK_TRUE,
        .depthWriteEnable = VK_TRUE,
        .depthCompareOp   = VK_COMPARE_OP_LESS_OR_EQUAL,
    };

    VkPipelineDepthStencilStateCreateInfo depth_off = {
        .sType            = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO,
        .depthTestEnable  = VK_FALSE,
        .depthWriteEnable = VK_FALSE,
    };

    /* Color blending */
    VkPipelineColorBlendAttachmentState blend_att = {
        .blendEnable         = VK_TRUE,
        .srcColorBlendFactor = VK_BLEND_FACTOR_SRC_ALPHA,
        .dstColorBlendFactor = VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA,
        .colorBlendOp        = VK_BLEND_OP_ADD,
        .srcAlphaBlendFactor = VK_BLEND_FACTOR_ONE,
        .dstAlphaBlendFactor = VK_BLEND_FACTOR_ZERO,
        .alphaBlendOp        = VK_BLEND_OP_ADD,
        .colorWriteMask      = VK_COLOR_COMPONENT_R_BIT |
                               VK_COLOR_COMPONENT_G_BIT |
                               VK_COLOR_COMPONENT_B_BIT |
                               VK_COLOR_COMPONENT_A_BIT,
    };

    VkPipelineColorBlendStateCreateInfo blend = {
        .sType           = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO,
        .attachmentCount = 1,
        .pAttachments    = &blend_att,
    };

    /* Dynamic state */
    VkDynamicState dyn_states[] = {
        VK_DYNAMIC_STATE_VIEWPORT,
        VK_DYNAMIC_STATE_SCISSOR,
    };
    VkPipelineDynamicStateCreateInfo dyn = {
        .sType             = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO,
        .dynamicStateCount = 2,
        .pDynamicStates    = dyn_states,
    };

    /* --- Fill pipeline (triangle mesh) --- */
    VkGraphicsPipelineCreateInfo fill_ci = {
        .sType               = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO,
        .stageCount          = 2,
        .pStages             = stages,
        .pVertexInputState   = &vertex_input,
        .pInputAssemblyState = &ia_tri,
        .pViewportState      = &viewport_state,
        .pRasterizationState = &raster_fill,
        .pMultisampleState   = &msaa,
        .pDepthStencilState  = &depth_stencil,
        .pColorBlendState    = &blend,
        .pDynamicState       = &dyn,
        .layout              = s_ctx.pipeline_layout,
        .renderPass          = s_ctx.render_pass,
        .subpass             = 0,
    };

    if (vkCreateGraphicsPipelines(s_ctx.device, VK_NULL_HANDLE, 1,
                                  &fill_ci, NULL,
                                  &s_ctx.fill_pipeline) != VK_SUCCESS)
    {
        set_error("Failed to create fill pipeline");
        return -1;
    }

    /* --- Wireframe pipeline --- */
    VkGraphicsPipelineCreateInfo wire_ci = fill_ci;
    wire_ci.pRasterizationState = &raster_wire;

    if (vkCreateGraphicsPipelines(s_ctx.device, VK_NULL_HANDLE, 1,
                                  &wire_ci, NULL,
                                  &s_ctx.wire_pipeline) != VK_SUCCESS)
    {
        set_error("Failed to create wireframe pipeline");
        return -1;
    }

    /* --- Line overlay pipeline (2D convergence plots) --- */
    VkGraphicsPipelineCreateInfo line_ci = fill_ci;
    line_ci.pInputAssemblyState = &ia_line_strip;
    line_ci.pRasterizationState = &raster_line;
    line_ci.pDepthStencilState  = &depth_off;

    if (vkCreateGraphicsPipelines(s_ctx.device, VK_NULL_HANDLE, 1,
                                  &line_ci, NULL,
                                  &s_ctx.line_pipeline) != VK_SUCCESS)
    {
        set_error("Failed to create line overlay pipeline");
        return -1;
    }

    return 0;
}

/* ================================================================
 *  Destroy offscreen resources
 * ================================================================ */

static void destroy_offscreen(void)
{
    VkDevice d = s_ctx.device;
    if (!d) return;

    if (s_ctx.framebuffer)    vkDestroyFramebuffer(d, s_ctx.framebuffer, NULL);
    if (s_ctx.render_pass)    vkDestroyRenderPass(d, s_ctx.render_pass, NULL);
    if (s_ctx.color_view)     vkDestroyImageView(d, s_ctx.color_view, NULL);
    if (s_ctx.color_image)    vkDestroyImage(d, s_ctx.color_image, NULL);
    if (s_ctx.color_memory)   vkFreeMemory(d, s_ctx.color_memory, NULL);
    if (s_ctx.depth_view)     vkDestroyImageView(d, s_ctx.depth_view, NULL);
    if (s_ctx.depth_image)    vkDestroyImage(d, s_ctx.depth_image, NULL);
    if (s_ctx.depth_memory)   vkFreeMemory(d, s_ctx.depth_memory, NULL);

    s_ctx.framebuffer  = VK_NULL_HANDLE;
    s_ctx.render_pass  = VK_NULL_HANDLE;
    s_ctx.color_view   = VK_NULL_HANDLE;
    s_ctx.color_image  = VK_NULL_HANDLE;
    s_ctx.color_memory = VK_NULL_HANDLE;
    s_ctx.depth_view   = VK_NULL_HANDLE;
    s_ctx.depth_image  = VK_NULL_HANDLE;
    s_ctx.depth_memory = VK_NULL_HANDLE;
}

/* ================================================================
 *  Record and submit a full render pass
 * ================================================================ */

static int record_and_submit_render(int width, int height)
{
    VkCommandBufferBeginInfo begin = {
        .sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
        .flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT,
    };

    vkResetCommandBuffer(s_ctx.cmd_buf, 0);
    vkBeginCommandBuffer(s_ctx.cmd_buf, &begin);

    VkClearValue clears[2] = {
        { .color = {{ 0.12f, 0.12f, 0.15f, 1.0f }} },  /* dark background */
        { .depthStencil = { 1.0f, 0 } },
    };

    VkRenderPassBeginInfo rp_begin = {
        .sType       = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO,
        .renderPass  = s_ctx.render_pass,
        .framebuffer = s_ctx.framebuffer,
        .renderArea  = { {0, 0}, {(uint32_t)width, (uint32_t)height} },
        .clearValueCount = 2,
        .pClearValues    = clears,
    };

    vkCmdBeginRenderPass(s_ctx.cmd_buf, &rp_begin, VK_SUBPASS_CONTENTS_INLINE);

    /* Set viewport and scissor */
    VkViewport vp = {
        .x = 0, .y = 0,
        .width  = (float)width,
        .height = (float)height,
        .minDepth = 0.0f, .maxDepth = 1.0f,
    };
    vkCmdSetViewport(s_ctx.cmd_buf, 0, 1, &vp);

    VkRect2D scissor = { {0, 0}, {(uint32_t)width, (uint32_t)height} };
    vkCmdSetScissor(s_ctx.cmd_buf, 0, 1, &scissor);

    /* Push view-projection matrix */
    vkCmdPushConstants(s_ctx.cmd_buf, s_ctx.pipeline_layout,
                       VK_SHADER_STAGE_VERTEX_BIT, 0, 64, s_ctx.view_proj);

    /* Draw mesh if uploaded */
    if (s_ctx.mesh_uploaded && s_ctx.vertex_count > 0 && s_ctx.index_count > 0)
    {
        VkDeviceSize offset = 0;

        /* Filled triangles */
        vkCmdBindPipeline(s_ctx.cmd_buf, VK_PIPELINE_BIND_POINT_GRAPHICS,
                          s_ctx.fill_pipeline);
        vkCmdBindVertexBuffers(s_ctx.cmd_buf, 0, 1,
                               &s_ctx.vertex_buffer, &offset);
        vkCmdBindIndexBuffer(s_ctx.cmd_buf, s_ctx.index_buffer, 0,
                             VK_INDEX_TYPE_UINT32);
        vkCmdDrawIndexed(s_ctx.cmd_buf, (uint32_t)s_ctx.index_count,
                         1, 0, 0, 0);

        /* Wireframe overlay */
        if (s_ctx.wireframe_enabled)
        {
            vkCmdBindPipeline(s_ctx.cmd_buf, VK_PIPELINE_BIND_POINT_GRAPHICS,
                              s_ctx.wire_pipeline);
            vkCmdDrawIndexed(s_ctx.cmd_buf, (uint32_t)s_ctx.index_count,
                             1, 0, 0, 0);
        }
    }

    /* Draw 2D convergence overlay if present */
    if (s_ctx.overlay_uploaded && s_ctx.overlay_vertex_count > 0)
    {
        /* Use an identity matrix for 2D overlay (NDC space) */
        float identity[16];
        mat4_identity(identity);
        vkCmdPushConstants(s_ctx.cmd_buf, s_ctx.pipeline_layout,
                           VK_SHADER_STAGE_VERTEX_BIT, 0, 64, identity);

        VkDeviceSize offset = 0;
        vkCmdBindPipeline(s_ctx.cmd_buf, VK_PIPELINE_BIND_POINT_GRAPHICS,
                          s_ctx.line_pipeline);
        vkCmdBindVertexBuffers(s_ctx.cmd_buf, 0, 1,
                               &s_ctx.overlay_vertex_buffer, &offset);
        vkCmdDraw(s_ctx.cmd_buf, (uint32_t)s_ctx.overlay_vertex_count,
                  1, 0, 0);
    }

    vkCmdEndRenderPass(s_ctx.cmd_buf);
    vkEndCommandBuffer(s_ctx.cmd_buf);

    /* Submit */
    VkSubmitInfo submit = {
        .sType              = VK_STRUCTURE_TYPE_SUBMIT_INFO,
        .commandBufferCount = 1,
        .pCommandBuffers    = &s_ctx.cmd_buf,
    };

    vkResetFences(s_ctx.device, 1, &s_ctx.fence);
    if (vkQueueSubmit(s_ctx.graphics_queue, 1, &submit,
                      s_ctx.fence) != VK_SUCCESS)
    {
        set_error("vkQueueSubmit failed");
        return -1;
    }

    vkWaitForFences(s_ctx.device, 1, &s_ctx.fence, VK_TRUE, UINT64_MAX);
    return 0;
}

/* ================================================================
 *  Write PPM image file
 * ================================================================ */

static int write_ppm(const char* path, const uint8_t* rgba,
                     int width, int height)
{
    FILE* f = fopen(path, "wb");
    if (!f)
    {
        set_error("Failed to open output file for writing");
        return -1;
    }

    fprintf(f, "P6\n%d %d\n255\n", width, height);
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            int idx = (y * width + x) * 4;
            fputc(rgba[idx + 0], f);  /* R */
            fputc(rgba[idx + 1], f);  /* G */
            fputc(rgba[idx + 2], f);  /* B */
        }
    }

    fclose(f);
    return 0;
}

/* ================================================================
 *  Generate overlay vertex data from plot series
 * ================================================================ */

static void generate_overlay_vertices(void)
{
    /* Plot occupies bottom-right quadrant in NDC:
     * x: [-0.9, -0.1] mapped to data x range
     * y: [0.1, 0.9]   mapped to data y range
     * (bottom-left of screen in Vulkan coords)
     */

    /* Series colors */
    static const float series_colors[][4] = {
        { 1.0f, 0.4f, 0.4f, 1.0f },  /* red    - Objective */
        { 0.4f, 1.0f, 0.4f, 1.0f },  /* green  - Residual Norm */
        { 0.4f, 0.4f, 1.0f, 1.0f },  /* blue   - Gradient Norm */
        { 1.0f, 1.0f, 0.4f, 1.0f },  /* yellow - Gauge Violation */
        { 1.0f, 0.4f, 1.0f, 1.0f },  /* magenta- Step Size */
        { 0.4f, 1.0f, 1.0f, 1.0f },  /* cyan   - Gauge/Physics */
    };

    /* Count total vertices needed */
    int total = 0;
    for (int s = 0; s < MAX_PLOT_SERIES; s++)
    {
        if (s_ctx.plot_series[s].active)
            total += s_ctx.plot_series[s].point_count;
    }

    if (total == 0)
    {
        s_ctx.overlay_uploaded = 0;
        s_ctx.overlay_vertex_count = 0;
        return;
    }

    /* Destroy old buffer if needed */
    if (s_ctx.overlay_vertex_buffer)
    {
        vkDestroyBuffer(s_ctx.device, s_ctx.overlay_vertex_buffer, NULL);
        s_ctx.overlay_vertex_buffer = VK_NULL_HANDLE;
    }
    if (s_ctx.overlay_vertex_memory)
    {
        vkFreeMemory(s_ctx.device, s_ctx.overlay_vertex_memory, NULL);
        s_ctx.overlay_vertex_memory = VK_NULL_HANDLE;
    }

    VkDeviceSize buf_size = (VkDeviceSize)total * sizeof(GU_Vertex);
    if (create_buffer(buf_size,
                      VK_BUFFER_USAGE_VERTEX_BUFFER_BIT,
                      VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                      VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                      &s_ctx.overlay_vertex_buffer,
                      &s_ctx.overlay_vertex_memory) != 0)
        return;

    GU_Vertex* verts;
    vkMapMemory(s_ctx.device, s_ctx.overlay_vertex_memory, 0,
                buf_size, 0, (void**)&verts);

    int vi = 0;
    for (int s = 0; s < MAX_PLOT_SERIES; s++)
    {
        GU_PlotSeries* ps = &s_ctx.plot_series[s];
        if (!ps->active || ps->point_count <= 0)
            continue;

        /* Find data range */
        double x_min = ps->x[0], x_max = ps->x[0];
        double y_min = ps->y[0], y_max = ps->y[0];
        for (int i = 1; i < ps->point_count; i++)
        {
            if (ps->x[i] < x_min) x_min = ps->x[i];
            if (ps->x[i] > x_max) x_max = ps->x[i];
            double yv = ps->log_scale && ps->y[i] > 0 ? log10(ps->y[i]) : ps->y[i];
            if (yv < y_min) y_min = yv;
            if (yv > y_max) y_max = yv;
        }

        double x_range = x_max - x_min;
        double y_range = y_max - y_min;
        if (x_range < 1e-30) x_range = 1.0;
        if (y_range < 1e-30) y_range = 1.0;

        /* Map to NDC: plot region in bottom-right */
        float plot_x0 = 0.1f, plot_x1 = 0.9f;
        float plot_y0 = 0.1f, plot_y1 = 0.9f;

        for (int i = 0; i < ps->point_count; i++)
        {
            double xn = (ps->x[i] - x_min) / x_range;
            double yv = ps->log_scale && ps->y[i] > 0 ? log10(ps->y[i]) : ps->y[i];
            double yn = (yv - y_min) / y_range;

            verts[vi].px = plot_x0 + (float)xn * (plot_x1 - plot_x0);
            verts[vi].py = plot_y0 + (float)yn * (plot_y1 - plot_y0);
            verts[vi].pz = 0.0f;
            verts[vi].cr = series_colors[s][0];
            verts[vi].cg = series_colors[s][1];
            verts[vi].cb = series_colors[s][2];
            verts[vi].ca = series_colors[s][3];
            vi++;
        }
    }

    vkUnmapMemory(s_ctx.device, s_ctx.overlay_vertex_memory);
    s_ctx.overlay_vertex_count = vi;
    s_ctx.overlay_uploaded = 1;
}

/* ================================================================
 *  PUBLIC API: Lifecycle
 * ================================================================ */

int gu_vk_initialize(int enable_validation, int width, int height)
{
    if (s_ctx.initialized)
    {
        set_error("Already initialized");
        return GU_VK_ERROR;
    }

    memset(&s_ctx, 0, sizeof(s_ctx));
    s_ctx.fb_width  = width  > 0 ? width  : 1920;
    s_ctx.fb_height = height > 0 ? height : 1080;
    s_ctx.wireframe_enabled = 1;
    s_ctx.enable_validation = enable_validation;
    s_last_error[0] = '\0';

    /* Set default camera (looking at origin from z=3) */
    {
        float view[16], proj[16];
        mat4_look_at(view, 0.0f, 0.0f, 3.0f, 0.0f, 0.0f, 0.0f,
                     0.0f, 1.0f, 0.0f);
        float aspect = (float)s_ctx.fb_width / (float)s_ctx.fb_height;
        mat4_perspective(proj, 1.0472f /* 60 deg */, aspect, 0.01f, 100.0f);
        mat4_mul(s_ctx.view_proj, proj, view);
    }

    /* --- 1. Create VkInstance --- */
    {
        VkApplicationInfo app_info = {
            .sType              = VK_STRUCTURE_TYPE_APPLICATION_INFO,
            .pApplicationName   = "Geometric Unity Workbench",
            .applicationVersion = VK_MAKE_VERSION(1, 0, 0),
            .pEngineName        = "GU",
            .engineVersion      = VK_MAKE_VERSION(1, 0, 0),
            .apiVersion         = VK_API_VERSION_1_0,
        };

        const char* layers[] = { "VK_LAYER_KHRONOS_validation" };
        const char* exts[]   = { VK_EXT_DEBUG_UTILS_EXTENSION_NAME };

        VkInstanceCreateInfo ci = {
            .sType                   = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
            .pApplicationInfo        = &app_info,
            .enabledLayerCount       = enable_validation ? 1 : 0,
            .ppEnabledLayerNames     = enable_validation ? layers : NULL,
            .enabledExtensionCount   = enable_validation ? 1 : 0,
            .ppEnabledExtensionNames = enable_validation ? exts : NULL,
        };

        VkResult res = vkCreateInstance(&ci, NULL, &s_ctx.instance);
        if (res != VK_SUCCESS)
        {
            /* If validation layers not available, retry without */
            if (enable_validation && res == VK_ERROR_LAYER_NOT_PRESENT)
            {
                ci.enabledLayerCount     = 0;
                ci.ppEnabledLayerNames   = NULL;
                ci.enabledExtensionCount = 0;
                ci.ppEnabledExtensionNames = NULL;
                s_ctx.enable_validation = 0;
                res = vkCreateInstance(&ci, NULL, &s_ctx.instance);
            }
            if (res != VK_SUCCESS)
            {
                set_errorf("vkCreateInstance failed (%d)", (int)res);
                return GU_VK_ERROR;
            }
        }
    }

    /* --- 2. Debug messenger --- */
    if (s_ctx.enable_validation)
    {
        PFN_vkCreateDebugUtilsMessengerEXT func =
            (PFN_vkCreateDebugUtilsMessengerEXT)
            vkGetInstanceProcAddr(s_ctx.instance,
                                 "vkCreateDebugUtilsMessengerEXT");
        if (func)
        {
            VkDebugUtilsMessengerCreateInfoEXT ci = {
                .sType           = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT,
                .messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT |
                                   VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT,
                .messageType     = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT |
                                   VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT |
                                   VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT,
                .pfnUserCallback = debug_callback,
            };
            func(s_ctx.instance, &ci, NULL, &s_ctx.debug_messenger);
        }
    }

    /* --- 3. Select physical device --- */
    {
        uint32_t dev_count = 0;
        vkEnumeratePhysicalDevices(s_ctx.instance, &dev_count, NULL);
        if (dev_count == 0)
        {
            set_error("No Vulkan-capable GPU found");
            vkDestroyInstance(s_ctx.instance, NULL);
            memset(&s_ctx, 0, sizeof(s_ctx));
            return GU_VK_ERROR;
        }

        VkPhysicalDevice* devs = calloc(dev_count, sizeof(VkPhysicalDevice));
        vkEnumeratePhysicalDevices(s_ctx.instance, &dev_count, devs);
        s_ctx.physical_device = devs[0]; /* Pick first device */

        /* Prefer discrete GPU */
        for (uint32_t i = 0; i < dev_count; i++)
        {
            VkPhysicalDeviceProperties props;
            vkGetPhysicalDeviceProperties(devs[i], &props);
            if (props.deviceType == VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU)
            {
                s_ctx.physical_device = devs[i];
                break;
            }
        }
        free(devs);

        vkGetPhysicalDeviceMemoryProperties(s_ctx.physical_device,
                                            &s_ctx.mem_props);
    }

    /* --- 4. Find graphics queue family --- */
    {
        uint32_t qf_count = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(s_ctx.physical_device,
                                                 &qf_count, NULL);
        VkQueueFamilyProperties* qf_props =
            calloc(qf_count, sizeof(VkQueueFamilyProperties));
        vkGetPhysicalDeviceQueueFamilyProperties(s_ctx.physical_device,
                                                 &qf_count, qf_props);

        s_ctx.graphics_queue_family = UINT32_MAX;
        for (uint32_t i = 0; i < qf_count; i++)
        {
            if (qf_props[i].queueFlags & VK_QUEUE_GRAPHICS_BIT)
            {
                s_ctx.graphics_queue_family = i;
                break;
            }
        }
        free(qf_props);

        if (s_ctx.graphics_queue_family == UINT32_MAX)
        {
            set_error("No graphics queue family found");
            vkDestroyInstance(s_ctx.instance, NULL);
            memset(&s_ctx, 0, sizeof(s_ctx));
            return GU_VK_ERROR;
        }
    }

    /* --- 5. Create logical device --- */
    {
        float priority = 1.0f;
        VkDeviceQueueCreateInfo queue_ci = {
            .sType            = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO,
            .queueFamilyIndex = s_ctx.graphics_queue_family,
            .queueCount       = 1,
            .pQueuePriorities = &priority,
        };

        /* Enable fillModeNonSolid for wireframe rendering */
        VkPhysicalDeviceFeatures features = { 0 };
        features.fillModeNonSolid = VK_TRUE;
        features.wideLines = VK_FALSE;

        /* Check if fillModeNonSolid is supported */
        VkPhysicalDeviceFeatures supported;
        vkGetPhysicalDeviceFeatures(s_ctx.physical_device, &supported);
        if (!supported.fillModeNonSolid)
            features.fillModeNonSolid = VK_FALSE;

        VkDeviceCreateInfo dev_ci = {
            .sType                = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,
            .queueCreateInfoCount = 1,
            .pQueueCreateInfos    = &queue_ci,
            .pEnabledFeatures     = &features,
        };

        VkResult res = vkCreateDevice(s_ctx.physical_device, &dev_ci, NULL,
                                      &s_ctx.device);
        if (res != VK_SUCCESS)
        {
            set_errorf("vkCreateDevice failed (%d)", (int)res);
            vkDestroyInstance(s_ctx.instance, NULL);
            memset(&s_ctx, 0, sizeof(s_ctx));
            return GU_VK_ERROR;
        }

        vkGetDeviceQueue(s_ctx.device, s_ctx.graphics_queue_family,
                         0, &s_ctx.graphics_queue);
    }

    /* --- 6. Command pool & buffer --- */
    {
        VkCommandPoolCreateInfo pool_ci = {
            .sType            = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
            .flags            = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
            .queueFamilyIndex = s_ctx.graphics_queue_family,
        };

        if (vkCreateCommandPool(s_ctx.device, &pool_ci, NULL,
                                &s_ctx.cmd_pool) != VK_SUCCESS)
        {
            set_error("vkCreateCommandPool failed");
            goto fail_cleanup;
        }

        VkCommandBufferAllocateInfo alloc = {
            .sType              = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
            .commandPool        = s_ctx.cmd_pool,
            .level              = VK_COMMAND_BUFFER_LEVEL_PRIMARY,
            .commandBufferCount = 1,
        };

        if (vkAllocateCommandBuffers(s_ctx.device, &alloc,
                                     &s_ctx.cmd_buf) != VK_SUCCESS)
        {
            set_error("vkAllocateCommandBuffers failed");
            goto fail_cleanup;
        }
    }

    /* --- 7. Fence --- */
    {
        VkFenceCreateInfo fence_ci = {
            .sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO,
            .flags = VK_FENCE_CREATE_SIGNALED_BIT,
        };

        if (vkCreateFence(s_ctx.device, &fence_ci, NULL,
                          &s_ctx.fence) != VK_SUCCESS)
        {
            set_error("vkCreateFence failed");
            goto fail_cleanup;
        }
    }

    /* --- 8. Offscreen framebuffer --- */
    if (create_offscreen_framebuffer(s_ctx.fb_width, s_ctx.fb_height) != 0)
        goto fail_cleanup;

    /* --- 9. Graphics pipelines --- */
    if (create_pipelines() != 0)
        goto fail_cleanup;

    s_ctx.initialized = 1;
    return GU_VK_OK;

fail_cleanup:
    gu_vk_shutdown();
    return GU_VK_ERROR;
}

void gu_vk_shutdown(void)
{
    if (!s_ctx.device)
    {
        memset(&s_ctx, 0, sizeof(s_ctx));
        return;
    }

    vkDeviceWaitIdle(s_ctx.device);

    VkDevice d = s_ctx.device;

    /* Destroy overlay buffers */
    if (s_ctx.overlay_vertex_buffer)
        vkDestroyBuffer(d, s_ctx.overlay_vertex_buffer, NULL);
    if (s_ctx.overlay_vertex_memory)
        vkFreeMemory(d, s_ctx.overlay_vertex_memory, NULL);

    /* Destroy mesh buffers */
    if (s_ctx.vertex_buffer) vkDestroyBuffer(d, s_ctx.vertex_buffer, NULL);
    if (s_ctx.vertex_memory) vkFreeMemory(d, s_ctx.vertex_memory, NULL);
    if (s_ctx.index_buffer)  vkDestroyBuffer(d, s_ctx.index_buffer, NULL);
    if (s_ctx.index_memory)  vkFreeMemory(d, s_ctx.index_memory, NULL);

    /* Destroy readback buffer */
    if (s_ctx.readback_buffer) vkDestroyBuffer(d, s_ctx.readback_buffer, NULL);
    if (s_ctx.readback_memory) vkFreeMemory(d, s_ctx.readback_memory, NULL);

    /* Destroy pipelines */
    if (s_ctx.fill_pipeline) vkDestroyPipeline(d, s_ctx.fill_pipeline, NULL);
    if (s_ctx.wire_pipeline) vkDestroyPipeline(d, s_ctx.wire_pipeline, NULL);
    if (s_ctx.line_pipeline) vkDestroyPipeline(d, s_ctx.line_pipeline, NULL);
    if (s_ctx.pipeline_layout)
        vkDestroyPipelineLayout(d, s_ctx.pipeline_layout, NULL);
    if (s_ctx.vert_shader) vkDestroyShaderModule(d, s_ctx.vert_shader, NULL);
    if (s_ctx.frag_shader) vkDestroyShaderModule(d, s_ctx.frag_shader, NULL);

    /* Destroy offscreen resources */
    destroy_offscreen();

    /* Destroy fence */
    if (s_ctx.fence) vkDestroyFence(d, s_ctx.fence, NULL);

    /* Destroy command pool */
    if (s_ctx.cmd_pool) vkDestroyCommandPool(d, s_ctx.cmd_pool, NULL);

    /* Destroy device */
    vkDestroyDevice(d, NULL);

    /* Destroy debug messenger */
    if (s_ctx.debug_messenger)
    {
        PFN_vkDestroyDebugUtilsMessengerEXT func =
            (PFN_vkDestroyDebugUtilsMessengerEXT)
            vkGetInstanceProcAddr(s_ctx.instance,
                                 "vkDestroyDebugUtilsMessengerEXT");
        if (func)
            func(s_ctx.instance, s_ctx.debug_messenger, NULL);
    }

    /* Destroy instance */
    if (s_ctx.instance)
        vkDestroyInstance(s_ctx.instance, NULL);

    memset(&s_ctx, 0, sizeof(s_ctx));
}

int gu_vk_is_initialized(void)
{
    return s_ctx.initialized;
}

/* ================================================================
 *  PUBLIC API: Mesh upload
 * ================================================================ */

int gu_vk_upload_mesh(
    const float* positions,
    const float* colors,
    int vertex_count,
    const uint32_t* indices,
    int index_count)
{
    if (!s_ctx.initialized)
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

    if (vertex_count > MAX_VERTICES || index_count > MAX_INDICES)
    {
        set_error("Mesh exceeds maximum vertex/index count");
        return GU_VK_ERROR;
    }

    /* Destroy old buffers */
    vkDeviceWaitIdle(s_ctx.device);

    if (s_ctx.vertex_buffer)
    {
        vkDestroyBuffer(s_ctx.device, s_ctx.vertex_buffer, NULL);
        s_ctx.vertex_buffer = VK_NULL_HANDLE;
    }
    if (s_ctx.vertex_memory)
    {
        vkFreeMemory(s_ctx.device, s_ctx.vertex_memory, NULL);
        s_ctx.vertex_memory = VK_NULL_HANDLE;
    }
    if (s_ctx.index_buffer)
    {
        vkDestroyBuffer(s_ctx.device, s_ctx.index_buffer, NULL);
        s_ctx.index_buffer = VK_NULL_HANDLE;
    }
    if (s_ctx.index_memory)
    {
        vkFreeMemory(s_ctx.device, s_ctx.index_memory, NULL);
        s_ctx.index_memory = VK_NULL_HANDLE;
    }

    /* Create vertex buffer (host-visible for simplicity) */
    VkDeviceSize vb_size = (VkDeviceSize)vertex_count * sizeof(GU_Vertex);
    if (create_buffer(vb_size,
                      VK_BUFFER_USAGE_VERTEX_BUFFER_BIT,
                      VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                      VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                      &s_ctx.vertex_buffer,
                      &s_ctx.vertex_memory) != 0)
        return GU_VK_ERROR;

    /* Interleave position + color data */
    GU_Vertex* verts;
    vkMapMemory(s_ctx.device, s_ctx.vertex_memory, 0, vb_size, 0,
                (void**)&verts);
    for (int i = 0; i < vertex_count; i++)
    {
        verts[i].px = positions[i * 3 + 0];
        verts[i].py = positions[i * 3 + 1];
        verts[i].pz = positions[i * 3 + 2];
        verts[i].cr = colors[i * 4 + 0];
        verts[i].cg = colors[i * 4 + 1];
        verts[i].cb = colors[i * 4 + 2];
        verts[i].ca = colors[i * 4 + 3];
    }
    vkUnmapMemory(s_ctx.device, s_ctx.vertex_memory);

    /* Create index buffer */
    VkDeviceSize ib_size = (VkDeviceSize)index_count * sizeof(uint32_t);
    if (create_buffer(ib_size,
                      VK_BUFFER_USAGE_INDEX_BUFFER_BIT,
                      VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                      VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                      &s_ctx.index_buffer,
                      &s_ctx.index_memory) != 0)
        return GU_VK_ERROR;

    void* idx_data;
    vkMapMemory(s_ctx.device, s_ctx.index_memory, 0, ib_size, 0, &idx_data);
    memcpy(idx_data, indices, (size_t)ib_size);
    vkUnmapMemory(s_ctx.device, s_ctx.index_memory);

    s_ctx.vertex_count = vertex_count;
    s_ctx.index_count  = index_count;
    s_ctx.mesh_uploaded = 1;

    return GU_VK_OK;
}

/* ================================================================
 *  PUBLIC API: Rendering
 * ================================================================ */

int gu_vk_render_frame(void)
{
    if (!s_ctx.initialized)
    {
        set_error("Not initialized");
        return GU_VK_NOT_INIT;
    }

    return record_and_submit_render(s_ctx.fb_width, s_ctx.fb_height);
}

int gu_vk_render_to_file(const char* output_path, int width, int height)
{
    if (!s_ctx.initialized)
    {
        set_error("Not initialized");
        return GU_VK_NOT_INIT;
    }

    if (!output_path || output_path[0] == '\0')
    {
        set_error("Invalid output path");
        return GU_VK_ERROR;
    }

    if (width <= 0)  width  = s_ctx.fb_width;
    if (height <= 0) height = s_ctx.fb_height;

    /* If different dimensions, recreate offscreen resources */
    int need_recreate = (width != s_ctx.fb_width || height != s_ctx.fb_height);

    if (need_recreate)
    {
        vkDeviceWaitIdle(s_ctx.device);

        /* Destroy old pipelines (they reference render pass) */
        if (s_ctx.fill_pipeline)
        {
            vkDestroyPipeline(s_ctx.device, s_ctx.fill_pipeline, NULL);
            s_ctx.fill_pipeline = VK_NULL_HANDLE;
        }
        if (s_ctx.wire_pipeline)
        {
            vkDestroyPipeline(s_ctx.device, s_ctx.wire_pipeline, NULL);
            s_ctx.wire_pipeline = VK_NULL_HANDLE;
        }
        if (s_ctx.line_pipeline)
        {
            vkDestroyPipeline(s_ctx.device, s_ctx.line_pipeline, NULL);
            s_ctx.line_pipeline = VK_NULL_HANDLE;
        }
        if (s_ctx.pipeline_layout)
        {
            vkDestroyPipelineLayout(s_ctx.device, s_ctx.pipeline_layout, NULL);
            s_ctx.pipeline_layout = VK_NULL_HANDLE;
        }

        destroy_offscreen();

        s_ctx.fb_width  = width;
        s_ctx.fb_height = height;

        if (create_offscreen_framebuffer(width, height) != 0)
            return GU_VK_ERROR;
        if (create_pipelines() != 0)
            return GU_VK_ERROR;
    }

    /* Render */
    if (record_and_submit_render(width, height) != 0)
        return GU_VK_ERROR;

    /* Readback: create host-visible buffer */
    VkDeviceSize pixel_size = (VkDeviceSize)width * height * 4;

    /* Reuse readback buffer if large enough */
    if (s_ctx.readback_size < pixel_size)
    {
        if (s_ctx.readback_buffer)
        {
            vkDestroyBuffer(s_ctx.device, s_ctx.readback_buffer, NULL);
            s_ctx.readback_buffer = VK_NULL_HANDLE;
        }
        if (s_ctx.readback_memory)
        {
            vkFreeMemory(s_ctx.device, s_ctx.readback_memory, NULL);
            s_ctx.readback_memory = VK_NULL_HANDLE;
        }

        if (create_buffer(pixel_size,
                          VK_BUFFER_USAGE_TRANSFER_DST_BIT,
                          VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                          VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                          &s_ctx.readback_buffer,
                          &s_ctx.readback_memory) != 0)
            return GU_VK_ERROR;

        s_ctx.readback_size = pixel_size;
    }

    /* Copy image to buffer */
    VkCommandBuffer cb = begin_single_command();

    VkBufferImageCopy region = {
        .bufferOffset      = 0,
        .bufferRowLength   = 0,
        .bufferImageHeight = 0,
        .imageSubresource  = {
            .aspectMask     = VK_IMAGE_ASPECT_COLOR_BIT,
            .mipLevel       = 0,
            .baseArrayLayer = 0,
            .layerCount     = 1,
        },
        .imageOffset = { 0, 0, 0 },
        .imageExtent = { (uint32_t)width, (uint32_t)height, 1 },
    };

    vkCmdCopyImageToBuffer(cb, s_ctx.color_image,
                           VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                           s_ctx.readback_buffer, 1, &region);

    end_single_command(cb);

    /* Read pixels */
    uint8_t* pixels;
    vkMapMemory(s_ctx.device, s_ctx.readback_memory, 0,
                pixel_size, 0, (void**)&pixels);

    int result = write_ppm(output_path, pixels, width, height);

    vkUnmapMemory(s_ctx.device, s_ctx.readback_memory);

    return result == 0 ? GU_VK_OK : GU_VK_ERROR;
}

/* ================================================================
 *  PUBLIC API: Wireframe overlay
 * ================================================================ */

void gu_vk_set_wireframe(int enabled)
{
    s_ctx.wireframe_enabled = enabled ? 1 : 0;
}

/* ================================================================
 *  PUBLIC API: 2D overlay for convergence plots
 * ================================================================ */

int gu_vk_upload_plot_series(
    const double* x_values,
    const double* y_values,
    int point_count,
    int series_index,
    int log_scale)
{
    if (!s_ctx.initialized)
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

    GU_PlotSeries* ps = &s_ctx.plot_series[series_index];
    int n = point_count < MAX_PLOT_POINTS ? point_count : MAX_PLOT_POINTS;
    memcpy(ps->x, x_values, (size_t)n * sizeof(double));
    memcpy(ps->y, y_values, (size_t)n * sizeof(double));
    ps->point_count = n;
    ps->log_scale   = log_scale;
    ps->active      = 1;

    /* Regenerate overlay vertex buffer */
    generate_overlay_vertices();

    return GU_VK_OK;
}

/* ================================================================
 *  PUBLIC API: Camera control
 * ================================================================ */

void gu_vk_set_camera(
    float eye_x, float eye_y, float eye_z,
    float target_x, float target_y, float target_z,
    float up_x, float up_y, float up_z)
{
    float view[16], proj[16];
    mat4_look_at(view, eye_x, eye_y, eye_z,
                 target_x, target_y, target_z,
                 up_x, up_y, up_z);

    float aspect = s_ctx.fb_width > 0 && s_ctx.fb_height > 0
        ? (float)s_ctx.fb_width / (float)s_ctx.fb_height
        : 16.0f / 9.0f;
    mat4_perspective(proj, 1.0472f /* 60 deg */, aspect, 0.01f, 100.0f);
    mat4_mul(s_ctx.view_proj, proj, view);
}

/* ================================================================
 *  PUBLIC API: Error reporting
 * ================================================================ */

const char* gu_vk_get_last_error(void)
{
    return s_last_error;
}

#else /* !GU_HAS_VULKAN -- stub fallback for builds without Vulkan SDK */

static int s_initialized = 0;
static int s_wireframe_enabled = 1;
static char s_last_error[256] = "";

static void set_error(const char* msg)
{
    strncpy(s_last_error, msg, sizeof(s_last_error) - 1);
    s_last_error[sizeof(s_last_error) - 1] = '\0';
}

int gu_vk_initialize(int enable_validation, int width, int height)
{
    (void)enable_validation; (void)width; (void)height;
    if (s_initialized) { set_error("Already initialized"); return GU_VK_ERROR; }
    s_initialized = 1;
    s_last_error[0] = '\0';
    return GU_VK_OK;
}

void gu_vk_shutdown(void) { s_initialized = 0; }

int gu_vk_is_initialized(void) { return s_initialized; }

int gu_vk_upload_mesh(const float* positions, const float* colors,
                      int vertex_count, const uint32_t* indices, int index_count)
{
    if (!s_initialized) { set_error("Not initialized"); return GU_VK_NOT_INIT; }
    if (!positions || !colors || !indices) { set_error("Null buffer pointer"); return GU_VK_ERROR; }
    if (vertex_count <= 0 || index_count <= 0) { set_error("Invalid vertex/index count"); return GU_VK_ERROR; }
    return GU_VK_OK;
}

int gu_vk_render_frame(void)
{
    if (!s_initialized) { set_error("Not initialized"); return GU_VK_NOT_INIT; }
    return GU_VK_OK;
}

int gu_vk_render_to_file(const char* output_path, int width, int height)
{
    (void)width; (void)height;
    if (!s_initialized) { set_error("Not initialized"); return GU_VK_NOT_INIT; }
    if (!output_path || output_path[0] == '\0') { set_error("Invalid output path"); return GU_VK_ERROR; }
    return GU_VK_OK;
}

void gu_vk_set_wireframe(int enabled) { s_wireframe_enabled = enabled ? 1 : 0; }

int gu_vk_upload_plot_series(const double* x_values, const double* y_values,
                             int point_count, int series_index, int log_scale)
{
    (void)log_scale;
    if (!s_initialized) { set_error("Not initialized"); return GU_VK_NOT_INIT; }
    if (!x_values || !y_values) { set_error("Null plot data pointer"); return GU_VK_ERROR; }
    if (point_count <= 0) { set_error("Invalid point count"); return GU_VK_ERROR; }
    if (series_index < 0 || series_index > 5) { set_error("Series index out of range [0,5]"); return GU_VK_ERROR; }
    return GU_VK_OK;
}

void gu_vk_set_camera(float eye_x, float eye_y, float eye_z,
                       float target_x, float target_y, float target_z,
                       float up_x, float up_y, float up_z)
{
    (void)eye_x; (void)eye_y; (void)eye_z;
    (void)target_x; (void)target_y; (void)target_z;
    (void)up_x; (void)up_y; (void)up_z;
}

const char* gu_vk_get_last_error(void) { return s_last_error; }

#endif /* GU_HAS_VULKAN */
