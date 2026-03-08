/**
 * gu_cuda_core.c
 *
 * Implementation of the native backend API defined in gu_interop_types.h.
 * When compiled without CUDA (GU_HAS_CUDA=0), all compute kernels run on
 * CPU as a reference fallback. When CUDA is available, compute kernels
 * dispatch to GPU.
 *
 * This file implements:
 * - Lifecycle management (initialize/shutdown)
 * - Buffer management (allocate/upload/download/free)
 * - Compute kernel dispatch (curvature/torsion/shiab/residual/objective)
 * - Error reporting
 */

#include "gu_interop_types.h"
#include <stdlib.h>
#include <string.h>
#include <math.h>

/* -------------------------------------------------------------------------
 * Internal state
 * ------------------------------------------------------------------------- */

#define MAX_BUFFERS 4096

typedef struct {
    double* data;
    int32_t element_count;
    int32_t bytes_per_element;
    int active;
} buffer_entry_t;

static struct {
    int initialized;
    gu_manifest_snapshot_t manifest;
    buffer_entry_t buffers[MAX_BUFFERS];
    int32_t next_handle;
    gu_error_packet_t last_error;
    int has_error;
} g_state = {0};

/* -------------------------------------------------------------------------
 * Error helpers
 * ------------------------------------------------------------------------- */

static void set_error(gu_error_code_t code, const char* message, const char* source) {
    g_state.last_error.code = code;
    strncpy(g_state.last_error.message, message, sizeof(g_state.last_error.message) - 1);
    g_state.last_error.message[sizeof(g_state.last_error.message) - 1] = '\0';
    strncpy(g_state.last_error.source, source, sizeof(g_state.last_error.source) - 1);
    g_state.last_error.source[sizeof(g_state.last_error.source) - 1] = '\0';
    g_state.has_error = 1;
}

static void clear_error(void) {
    g_state.has_error = 0;
    memset(&g_state.last_error, 0, sizeof(g_state.last_error));
}

/* -------------------------------------------------------------------------
 * Lifecycle
 * ------------------------------------------------------------------------- */

gu_error_code_t gu_initialize(const gu_manifest_snapshot_t* manifest) {
    if (!manifest) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "manifest is NULL", "gu_initialize");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    /* Clean up any previous state */
    if (g_state.initialized) {
        gu_shutdown();
    }

    memset(&g_state, 0, sizeof(g_state));
    g_state.manifest = *manifest;
    g_state.initialized = 1;
    g_state.next_handle = 1; /* handle 0 is reserved/invalid */

#ifdef GU_HAS_CUDA
    /* TODO: CUDA device initialization, context creation */
#endif

    return GU_SUCCESS;
}

void gu_shutdown(void) {
    if (!g_state.initialized) return;

    /* Free all allocated buffers */
    for (int i = 0; i < MAX_BUFFERS; i++) {
        if (g_state.buffers[i].active && g_state.buffers[i].data) {
            free(g_state.buffers[i].data);
            g_state.buffers[i].data = NULL;
            g_state.buffers[i].active = 0;
        }
    }

#ifdef GU_HAS_CUDA
    /* TODO: CUDA cleanup, context destruction */
#endif

    g_state.initialized = 0;
}

gu_interop_version_t gu_get_version(void) {
    gu_interop_version_t v;
    v.major = 1;
    v.minor = 0;
    v.patch = 0;
    return v;
}

/* -------------------------------------------------------------------------
 * Buffer management
 * ------------------------------------------------------------------------- */

gu_buffer_handle_t gu_allocate_buffer(int32_t total_elements, int32_t bytes_per_element) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_allocate_buffer");
        return -1;
    }
    if (total_elements <= 0 || bytes_per_element <= 0) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid buffer dimensions", "gu_allocate_buffer");
        return -1;
    }
    if (g_state.next_handle >= MAX_BUFFERS) {
        set_error(GU_ERROR_ALLOCATION_FAILED, "Buffer handle limit reached", "gu_allocate_buffer");
        return -1;
    }

    size_t byte_count = (size_t)total_elements * (size_t)bytes_per_element;

#ifdef GU_HAS_CUDA
    /* TODO: cudaMalloc */
    double* data = NULL;
    set_error(GU_ERROR_CUDA_ERROR, "CUDA allocation not implemented", "gu_allocate_buffer");
    return -1;
#else
    double* data = (double*)calloc((size_t)total_elements, (size_t)bytes_per_element);
#endif

    if (!data) {
        set_error(GU_ERROR_ALLOCATION_FAILED, "Memory allocation failed", "gu_allocate_buffer");
        return -1;
    }

    int32_t handle = g_state.next_handle++;
    g_state.buffers[handle].data = data;
    g_state.buffers[handle].element_count = total_elements;
    g_state.buffers[handle].bytes_per_element = bytes_per_element;
    g_state.buffers[handle].active = 1;

    clear_error();
    return handle;
}

gu_error_code_t gu_upload_buffer(gu_buffer_handle_t handle, const void* data, size_t byte_count) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_upload_buffer");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (handle <= 0 || handle >= MAX_BUFFERS || !g_state.buffers[handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid buffer handle", "gu_upload_buffer");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (!data) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "data is NULL", "gu_upload_buffer");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* buf = &g_state.buffers[handle];
    size_t max_bytes = (size_t)buf->element_count * (size_t)buf->bytes_per_element;
    if (byte_count > max_bytes) {
        set_error(GU_ERROR_BUFFER_OVERFLOW, "Upload exceeds buffer size", "gu_upload_buffer");
        return GU_ERROR_BUFFER_OVERFLOW;
    }

#ifdef GU_HAS_CUDA
    /* TODO: cudaMemcpy(buf->data, data, byte_count, cudaMemcpyHostToDevice) */
#else
    memcpy(buf->data, data, byte_count);
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_download_buffer(gu_buffer_handle_t handle, void* data, size_t byte_count) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_download_buffer");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (handle <= 0 || handle >= MAX_BUFFERS || !g_state.buffers[handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid buffer handle", "gu_download_buffer");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (!data) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "data is NULL", "gu_download_buffer");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* buf = &g_state.buffers[handle];
    size_t max_bytes = (size_t)buf->element_count * (size_t)buf->bytes_per_element;
    size_t copy_bytes = byte_count < max_bytes ? byte_count : max_bytes;

#ifdef GU_HAS_CUDA
    /* TODO: cudaMemcpy(data, buf->data, copy_bytes, cudaMemcpyDeviceToHost) */
#else
    memcpy(data, buf->data, copy_bytes);
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_free_buffer(gu_buffer_handle_t handle) {
    if (handle <= 0 || handle >= MAX_BUFFERS || !g_state.buffers[handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid buffer handle", "gu_free_buffer");
        return GU_ERROR_INVALID_ARGUMENT;
    }

#ifdef GU_HAS_CUDA
    /* TODO: cudaFree(g_state.buffers[handle].data) */
#else
    free(g_state.buffers[handle].data);
#endif

    g_state.buffers[handle].data = NULL;
    g_state.buffers[handle].active = 0;

    clear_error();
    return GU_SUCCESS;
}

/* -------------------------------------------------------------------------
 * Compute kernels (CPU reference fallback)
 *
 * When GU_HAS_CUDA is defined, these dispatch to GPU kernel launches.
 * Otherwise they execute directly on CPU using the same algorithm.
 * ------------------------------------------------------------------------- */

gu_error_code_t gu_evaluate_curvature(gu_buffer_handle_t omega, gu_buffer_handle_t curvature_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_curvature");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (omega <= 0 || omega >= MAX_BUFFERS || !g_state.buffers[omega].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid omega handle", "gu_evaluate_curvature");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (curvature_out <= 0 || curvature_out >= MAX_BUFFERS || !g_state.buffers[curvature_out].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid curvature_out handle", "gu_evaluate_curvature");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* omega_buf = &g_state.buffers[omega];
    buffer_entry_t* curv_buf = &g_state.buffers[curvature_out];

#ifdef GU_HAS_CUDA
    /* TODO: launch curvature CUDA kernel */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA curvature kernel not implemented", "gu_evaluate_curvature");
    return GU_ERROR_CUDA_ERROR;
#else
    /*
     * CPU reference curvature: F = d(omega) + (1/2)[omega, omega]
     * For the reference stub, copy omega -> curvature (identity map).
     * The full implementation requires mesh topology and Lie algebra structure constants.
     */
    int count = omega_buf->element_count < curv_buf->element_count
        ? omega_buf->element_count : curv_buf->element_count;
    memcpy(curv_buf->data, omega_buf->data, (size_t)count * sizeof(double));
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_evaluate_torsion(gu_buffer_handle_t omega, gu_buffer_handle_t torsion_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_torsion");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (omega <= 0 || omega >= MAX_BUFFERS || !g_state.buffers[omega].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid omega handle", "gu_evaluate_torsion");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (torsion_out <= 0 || torsion_out >= MAX_BUFFERS || !g_state.buffers[torsion_out].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid torsion_out handle", "gu_evaluate_torsion");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* torsion_buf = &g_state.buffers[torsion_out];

#ifdef GU_HAS_CUDA
    /* TODO: launch torsion CUDA kernel */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA torsion kernel not implemented", "gu_evaluate_torsion");
    return GU_ERROR_CUDA_ERROR;
#else
    /* CPU reference: trivial torsion T = 0 */
    memset(torsion_buf->data, 0, (size_t)torsion_buf->element_count * sizeof(double));
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_evaluate_shiab(gu_buffer_handle_t omega, gu_buffer_handle_t shiab_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_shiab");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (omega <= 0 || omega >= MAX_BUFFERS || !g_state.buffers[omega].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid omega handle", "gu_evaluate_shiab");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (shiab_out <= 0 || shiab_out >= MAX_BUFFERS || !g_state.buffers[shiab_out].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid shiab_out handle", "gu_evaluate_shiab");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* omega_buf = &g_state.buffers[omega];
    buffer_entry_t* shiab_buf = &g_state.buffers[shiab_out];

#ifdef GU_HAS_CUDA
    /* TODO: launch shiab CUDA kernel */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA Shiab kernel not implemented", "gu_evaluate_shiab");
    return GU_ERROR_CUDA_ERROR;
#else
    /* CPU reference: identity Shiab S = F (copy omega, matching CpuReferenceBackend) */
    int count = omega_buf->element_count < shiab_buf->element_count
        ? omega_buf->element_count : shiab_buf->element_count;
    memcpy(shiab_buf->data, omega_buf->data, (size_t)count * sizeof(double));
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_evaluate_residual(gu_buffer_handle_t shiab, gu_buffer_handle_t torsion,
                                      gu_buffer_handle_t residual_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_residual");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (shiab <= 0 || shiab >= MAX_BUFFERS || !g_state.buffers[shiab].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid shiab handle", "gu_evaluate_residual");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (torsion <= 0 || torsion >= MAX_BUFFERS || !g_state.buffers[torsion].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid torsion handle", "gu_evaluate_residual");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (residual_out <= 0 || residual_out >= MAX_BUFFERS || !g_state.buffers[residual_out].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid residual_out handle", "gu_evaluate_residual");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* s_buf = &g_state.buffers[shiab];
    buffer_entry_t* t_buf = &g_state.buffers[torsion];
    buffer_entry_t* r_buf = &g_state.buffers[residual_out];

#ifdef GU_HAS_CUDA
    /* TODO: launch residual CUDA kernel */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA residual kernel not implemented", "gu_evaluate_residual");
    return GU_ERROR_CUDA_ERROR;
#else
    /* CPU reference: Upsilon = S - T */
    int count = s_buf->element_count;
    if (t_buf->element_count < count) count = t_buf->element_count;
    if (r_buf->element_count < count) count = r_buf->element_count;

    for (int i = 0; i < count; i++) {
        r_buf->data[i] = s_buf->data[i] - t_buf->data[i];
    }
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_evaluate_objective(gu_buffer_handle_t residual, double* objective_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_objective");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (residual <= 0 || residual >= MAX_BUFFERS || !g_state.buffers[residual].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid residual handle", "gu_evaluate_objective");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (!objective_out) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "objective_out is NULL", "gu_evaluate_objective");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* r_buf = &g_state.buffers[residual];

#ifdef GU_HAS_CUDA
    /* TODO: parallel reduction on GPU */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA objective kernel not implemented", "gu_evaluate_objective");
    return GU_ERROR_CUDA_ERROR;
#else
    /* CPU reference: I2_h = (1/2) sum(r_i^2) */
    double sum = 0.0;
    for (int i = 0; i < r_buf->element_count; i++) {
        sum += r_buf->data[i] * r_buf->data[i];
    }
    *objective_out = 0.5 * sum;
#endif

    clear_error();
    return GU_SUCCESS;
}

/* -------------------------------------------------------------------------
 * Solver primitives (M10)
 * These are BLAS-like operations needed by the solver iteration loop.
 * On GPU these map to cuBLAS or custom kernels.
 * ------------------------------------------------------------------------- */

gu_error_code_t gu_axpy(gu_buffer_handle_t y_handle, double alpha, gu_buffer_handle_t x_handle, int32_t n) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_axpy");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (y_handle <= 0 || y_handle >= MAX_BUFFERS || !g_state.buffers[y_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid y handle", "gu_axpy");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (x_handle <= 0 || x_handle >= MAX_BUFFERS || !g_state.buffers[x_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid x handle", "gu_axpy");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* y_buf = &g_state.buffers[y_handle];
    buffer_entry_t* x_buf = &g_state.buffers[x_handle];

#ifdef GU_HAS_CUDA
    /* TODO: cublasDaxpy */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA axpy not implemented", "gu_axpy");
    return GU_ERROR_CUDA_ERROR;
#else
    int count = n < y_buf->element_count ? n : y_buf->element_count;
    if (x_buf->element_count < count) count = x_buf->element_count;
    for (int i = 0; i < count; i++) {
        y_buf->data[i] += alpha * x_buf->data[i];
    }
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_inner_product(gu_buffer_handle_t u_handle, gu_buffer_handle_t v_handle,
                                  int32_t n, double* result_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_inner_product");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (u_handle <= 0 || u_handle >= MAX_BUFFERS || !g_state.buffers[u_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid u handle", "gu_inner_product");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (v_handle <= 0 || v_handle >= MAX_BUFFERS || !g_state.buffers[v_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid v handle", "gu_inner_product");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (!result_out) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "result_out is NULL", "gu_inner_product");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* u_buf = &g_state.buffers[u_handle];
    buffer_entry_t* v_buf = &g_state.buffers[v_handle];

#ifdef GU_HAS_CUDA
    /* TODO: cublasDdot or custom parallel reduction */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA inner_product not implemented", "gu_inner_product");
    return GU_ERROR_CUDA_ERROR;
#else
    int count = n < u_buf->element_count ? n : u_buf->element_count;
    if (v_buf->element_count < count) count = v_buf->element_count;
    double sum = 0.0;
    for (int i = 0; i < count; i++) {
        sum += u_buf->data[i] * v_buf->data[i];
    }
    *result_out = sum;
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_scale(gu_buffer_handle_t x_handle, double alpha, int32_t n) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_scale");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (x_handle <= 0 || x_handle >= MAX_BUFFERS || !g_state.buffers[x_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid x handle", "gu_scale");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* x_buf = &g_state.buffers[x_handle];

#ifdef GU_HAS_CUDA
    /* TODO: cublasDscal */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA scale not implemented", "gu_scale");
    return GU_ERROR_CUDA_ERROR;
#else
    int count = n < x_buf->element_count ? n : x_buf->element_count;
    for (int i = 0; i < count; i++) {
        x_buf->data[i] *= alpha;
    }
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_copy(gu_buffer_handle_t dst_handle, gu_buffer_handle_t src_handle, int32_t n) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_copy");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (dst_handle <= 0 || dst_handle >= MAX_BUFFERS || !g_state.buffers[dst_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid dst handle", "gu_copy");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (src_handle <= 0 || src_handle >= MAX_BUFFERS || !g_state.buffers[src_handle].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid src handle", "gu_copy");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* dst_buf = &g_state.buffers[dst_handle];
    buffer_entry_t* src_buf = &g_state.buffers[src_handle];

#ifdef GU_HAS_CUDA
    /* TODO: cudaMemcpy device-to-device */
    set_error(GU_ERROR_CUDA_ERROR, "CUDA copy not implemented", "gu_copy");
    return GU_ERROR_CUDA_ERROR;
#else
    int count = n < dst_buf->element_count ? n : dst_buf->element_count;
    if (src_buf->element_count < count) count = src_buf->element_count;
    memcpy(dst_buf->data, src_buf->data, (size_t)count * sizeof(double));
#endif

    clear_error();
    return GU_SUCCESS;
}

/* -------------------------------------------------------------------------
 * Error reporting
 * ------------------------------------------------------------------------- */

const gu_error_packet_t* gu_get_last_error(void) {
    if (!g_state.has_error) return NULL;
    return &g_state.last_error;
}
