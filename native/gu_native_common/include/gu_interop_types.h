/**
 * gu_interop_types.h
 *
 * Shared interop types used by both gu_cuda_core and gu_vulkan_native.
 * Mirrors the C# types in Gu.Interop to ensure ABI compatibility.
 *
 * Convention: All structs use fixed-size types for cross-platform safety.
 * All functions use C linkage (extern "C") for P/Invoke compatibility.
 */

#ifndef GU_INTEROP_TYPES_H
#define GU_INTEROP_TYPES_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Version information -- must match InteropVersion in C# */
typedef struct {
    int32_t major;
    int32_t minor;
    int32_t patch;
    /* backend_id is not included in the native struct;
       the C# side manages it */
} gu_interop_version_t;

/* Error codes */
typedef enum {
    GU_SUCCESS = 0,
    GU_ERROR_NOT_INITIALIZED = 1,
    GU_ERROR_INVALID_ARGUMENT = 2,
    GU_ERROR_ALLOCATION_FAILED = 3,
    GU_ERROR_BUFFER_OVERFLOW = 4,
    GU_ERROR_CUDA_ERROR = 100,
    GU_ERROR_CUDA_LAUNCH_FAILED = 101,
    GU_ERROR_CUDA_OUT_OF_MEMORY = 102,
} gu_error_code_t;

/* Error packet -- returned by gu_get_last_error */
typedef struct {
    gu_error_code_t code;
    char message[256];
    char source[64];
} gu_error_packet_t;

/* Component descriptor for SoA buffer layout */
typedef struct {
    int32_t offset;       /* byte offset from buffer start */
    int32_t stride;       /* bytes between consecutive elements */
    int32_t count;        /* number of elements */
} gu_component_descriptor_t;

/* Buffer layout descriptor */
typedef struct {
    int32_t total_elements;
    int32_t component_count;
    int32_t bytes_per_element;  /* 8 for float64, 4 for float32 */
    /* components array follows; accessed via gu_layout_get_component */
} gu_buffer_layout_t;

/* Manifest snapshot -- lightweight branch info for GPU initialization */
typedef struct {
    int32_t base_dimension;
    int32_t ambient_dimension;
    int32_t lie_algebra_dimension;
    int32_t mesh_cell_count;
    int32_t mesh_vertex_count;
} gu_manifest_snapshot_t;

/* Opaque handle to a GPU buffer */
typedef int32_t gu_buffer_handle_t;

/*
 * Native backend API
 * These functions are implemented by gu_cuda_core (and optionally gu_cpu_reference).
 */

/* Lifecycle */
gu_error_code_t gu_initialize(const gu_manifest_snapshot_t* manifest);
void gu_shutdown(void);
gu_interop_version_t gu_get_version(void);

/* Buffer management */
gu_buffer_handle_t gu_allocate_buffer(int32_t total_elements, int32_t bytes_per_element);
gu_error_code_t gu_upload_buffer(gu_buffer_handle_t handle, const void* data, size_t byte_count);
gu_error_code_t gu_download_buffer(gu_buffer_handle_t handle, void* data, size_t byte_count);
gu_error_code_t gu_free_buffer(gu_buffer_handle_t handle);

/* Compute kernels -- assembly */
gu_error_code_t gu_evaluate_curvature(gu_buffer_handle_t omega, gu_buffer_handle_t curvature_out);
gu_error_code_t gu_evaluate_torsion(gu_buffer_handle_t omega, gu_buffer_handle_t torsion_out);
gu_error_code_t gu_evaluate_shiab(gu_buffer_handle_t omega, gu_buffer_handle_t shiab_out);
gu_error_code_t gu_evaluate_residual(gu_buffer_handle_t shiab, gu_buffer_handle_t torsion, gu_buffer_handle_t residual_out);
gu_error_code_t gu_evaluate_objective(gu_buffer_handle_t residual, double* objective_out);

/* Compute kernels -- solver primitives (M10) */
gu_error_code_t gu_axpy(gu_buffer_handle_t y, double alpha, gu_buffer_handle_t x, int32_t n);
gu_error_code_t gu_inner_product(gu_buffer_handle_t u, gu_buffer_handle_t v, int32_t n, double* result_out);
gu_error_code_t gu_scale(gu_buffer_handle_t x, double alpha, int32_t n);
gu_error_code_t gu_copy(gu_buffer_handle_t dst, gu_buffer_handle_t src, int32_t n);

/* Error reporting */
const gu_error_packet_t* gu_get_last_error(void);

#ifdef __cplusplus
}
#endif

#endif /* GU_INTEROP_TYPES_H */
