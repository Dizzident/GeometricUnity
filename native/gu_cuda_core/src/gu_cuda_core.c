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
#include "gu_cuda_kernels.h"
#include <stdlib.h>
#include <string.h>
#include <math.h>

/* -------------------------------------------------------------------------
 * CUDA error checking helper
 * ------------------------------------------------------------------------- */

#ifdef GU_HAS_CUDA
#define CUDA_CHECK(call, func_name) do { \
    int _err = (call); \
    if (_err != 0) { \
        set_error(GU_ERROR_CUDA_ERROR, \
                  gu_cuda_get_last_error_string(), func_name); \
        return GU_ERROR_CUDA_ERROR; \
    } \
} while(0)

#define CUDA_CHECK_HANDLE(call, func_name) do { \
    int _err = (call); \
    if (_err != 0) { \
        set_error(GU_ERROR_CUDA_ERROR, \
                  gu_cuda_get_last_error_string(), func_name); \
        return -1; \
    } \
} while(0)
#endif

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

/* Physics data storage -- mesh topology and Lie algebra */
typedef struct {
    /* Mesh topology */
    gu_mesh_topology_header_t header;
    int32_t* face_boundary_edges;       /* [face_count * max_edges_per_face] */
    int32_t* face_boundary_orientations;/* [face_count * max_edges_per_face] */
    int32_t* edge_vertices;             /* [edge_count * 2] */
    double* vertex_coords;              /* [vertex_count * embedding_dim] */

    /* Lie algebra */
    double* structure_constants;        /* [dim_g^3] f^c_{ab} */
    double* invariant_metric;           /* [dim_g^2] g_{ab} */

    /* Background connection */
    double* a0_coefficients;            /* [edge_count * dim_g] */

    /* Flags */
    int has_topology;
    int has_vertices;
    int has_structure_constants;
    int has_metric;
    int has_a0;
} physics_data_t;

static struct {
    int initialized;
    gu_manifest_snapshot_t manifest;
    buffer_entry_t buffers[MAX_BUFFERS];
    int32_t next_handle;
    gu_error_packet_t last_error;
    int has_error;
    physics_data_t physics;
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
    CUDA_CHECK(gu_cuda_device_init(), "gu_initialize");
#endif

    return GU_SUCCESS;
}

static void free_physics_data(void) {
    physics_data_t* p = &g_state.physics;
    free(p->face_boundary_edges);       p->face_boundary_edges = NULL;
    free(p->face_boundary_orientations);p->face_boundary_orientations = NULL;
    free(p->edge_vertices);             p->edge_vertices = NULL;
    free(p->vertex_coords);             p->vertex_coords = NULL;
    free(p->structure_constants);       p->structure_constants = NULL;
    free(p->invariant_metric);          p->invariant_metric = NULL;
    free(p->a0_coefficients);           p->a0_coefficients = NULL;
    memset(p, 0, sizeof(*p));
}

void gu_shutdown(void) {
    if (!g_state.initialized) return;

    /* Free all allocated buffers */
    for (int i = 0; i < MAX_BUFFERS; i++) {
        if (g_state.buffers[i].active && g_state.buffers[i].data) {
#ifdef GU_HAS_CUDA
            gu_cuda_free(g_state.buffers[i].data);
#else
            free(g_state.buffers[i].data);
#endif
            g_state.buffers[i].data = NULL;
            g_state.buffers[i].active = 0;
        }
    }

    /* Free physics data (always host memory) */
    free_physics_data();

#ifdef GU_HAS_CUDA
    gu_cuda_device_reset();
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
    double* data = NULL;
    CUDA_CHECK_HANDLE(gu_cuda_malloc((void**)&data, byte_count), "gu_allocate_buffer");
    /* Zero the device memory to match CPU calloc behavior */
    CUDA_CHECK_HANDLE(gu_cuda_memset(data, 0, byte_count), "gu_allocate_buffer");
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
    CUDA_CHECK(gu_cuda_memcpy_h2d(buf->data, data, byte_count), "gu_upload_buffer");
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
    CUDA_CHECK(gu_cuda_memcpy_d2h(data, buf->data, copy_bytes), "gu_download_buffer");
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
    CUDA_CHECK(gu_cuda_free(g_state.buffers[handle].data), "gu_free_buffer");
#else
    free(g_state.buffers[handle].data);
#endif

    g_state.buffers[handle].data = NULL;
    g_state.buffers[handle].active = 0;

    clear_error();
    return GU_SUCCESS;
}

/* -------------------------------------------------------------------------
 * Compute kernels
 *
 * When GU_HAS_CUDA is defined, these dispatch to GPU kernel launches
 * via the functions in gu_cuda_kernels.h. Physics kernels (curvature,
 * torsion, shiab) use the CPU-side approach for correctness: copy omega
 * from device to host, run the validated CPU physics kernel, copy the
 * result back to device.
 *
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
    physics_data_t* p = &g_state.physics;

    if (p->has_topology && p->has_structure_constants) {
#ifdef GU_HAS_CUDA
        /* Copy omega from device to host, run CPU physics kernel, copy result back */
        size_t omega_bytes = (size_t)omega_buf->element_count * sizeof(double);
        size_t curv_bytes = (size_t)curv_buf->element_count * sizeof(double);
        double* h_omega = (double*)malloc(omega_bytes);
        double* h_curv = (double*)malloc(curv_bytes);
        if (!h_omega || !h_curv) {
            free(h_omega); free(h_curv);
            set_error(GU_ERROR_ALLOCATION_FAILED, "Host staging alloc failed", "gu_evaluate_curvature");
            return GU_ERROR_ALLOCATION_FAILED;
        }
        int cuda_rc = gu_cuda_memcpy_d2h(h_omega, omega_buf->data, omega_bytes);
        if (cuda_rc != 0) {
            free(h_omega); free(h_curv);
            set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_curvature");
            return GU_ERROR_CUDA_ERROR;
        }
        int rc = gu_curvature_assemble_physics(
            h_omega, h_curv,
            p->face_boundary_edges, p->face_boundary_orientations,
            p->structure_constants,
            p->header.face_count, p->header.edge_count,
            p->header.dim_g, p->header.max_edges_per_face);
        if (rc != 0) {
            free(h_omega); free(h_curv);
            set_error(GU_ERROR_CUDA_ERROR, "Curvature kernel failed", "gu_evaluate_curvature");
            return GU_ERROR_CUDA_ERROR;
        }
        cuda_rc = gu_cuda_memcpy_h2d(curv_buf->data, h_curv, curv_bytes);
        free(h_omega); free(h_curv);
        if (cuda_rc != 0) {
            set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_curvature");
            return GU_ERROR_CUDA_ERROR;
        }
#else
        /* Real physics: F = d(omega) + (1/2)[omega, omega] */
        int rc = gu_curvature_assemble_physics(
            omega_buf->data, curv_buf->data,
            p->face_boundary_edges, p->face_boundary_orientations,
            p->structure_constants,
            p->header.face_count, p->header.edge_count,
            p->header.dim_g, p->header.max_edges_per_face);
        if (rc != 0) {
            set_error(GU_ERROR_CUDA_ERROR, "Curvature kernel failed", "gu_evaluate_curvature");
            return GU_ERROR_CUDA_ERROR;
        }
#endif
    } else {
        /* Fallback: identity stub (F = omega) */
        int count = omega_buf->element_count < curv_buf->element_count
            ? omega_buf->element_count : curv_buf->element_count;
#ifdef GU_HAS_CUDA
        CUDA_CHECK(gu_cuda_memcpy_d2d(curv_buf->data, omega_buf->data,
                                       (size_t)count * sizeof(double)),
                   "gu_evaluate_curvature");
#else
        memcpy(curv_buf->data, omega_buf->data, (size_t)count * sizeof(double));
#endif
    }

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

    buffer_entry_t* omega_buf = &g_state.buffers[omega];
    buffer_entry_t* torsion_buf = &g_state.buffers[torsion_out];
    physics_data_t* p = &g_state.physics;

    if (p->has_topology && p->has_structure_constants && p->has_a0) {
#ifdef GU_HAS_CUDA
        /* Copy omega from device to host, run CPU physics kernel, copy result back */
        size_t omega_bytes = (size_t)omega_buf->element_count * sizeof(double);
        size_t torsion_bytes = (size_t)torsion_buf->element_count * sizeof(double);
        double* h_omega = (double*)malloc(omega_bytes);
        double* h_torsion = (double*)malloc(torsion_bytes);
        if (!h_omega || !h_torsion) {
            free(h_omega); free(h_torsion);
            set_error(GU_ERROR_ALLOCATION_FAILED, "Host staging alloc failed", "gu_evaluate_torsion");
            return GU_ERROR_ALLOCATION_FAILED;
        }
        int cuda_rc = gu_cuda_memcpy_d2h(h_omega, omega_buf->data, omega_bytes);
        if (cuda_rc != 0) {
            free(h_omega); free(h_torsion);
            set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_torsion");
            return GU_ERROR_CUDA_ERROR;
        }
        int rc = gu_torsion_assemble_physics(
            h_omega, p->a0_coefficients, h_torsion,
            p->face_boundary_edges, p->face_boundary_orientations,
            p->structure_constants,
            p->header.face_count, p->header.edge_count,
            p->header.dim_g, p->header.max_edges_per_face);
        if (rc != 0) {
            free(h_omega); free(h_torsion);
            set_error(GU_ERROR_CUDA_ERROR, "Torsion kernel failed", "gu_evaluate_torsion");
            return GU_ERROR_CUDA_ERROR;
        }
        cuda_rc = gu_cuda_memcpy_h2d(torsion_buf->data, h_torsion, torsion_bytes);
        free(h_omega); free(h_torsion);
        if (cuda_rc != 0) {
            set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_torsion");
            return GU_ERROR_CUDA_ERROR;
        }
#else
        /* Real physics: T^aug = d_{A0}(omega - A0) */
        int rc = gu_torsion_assemble_physics(
            omega_buf->data, p->a0_coefficients, torsion_buf->data,
            p->face_boundary_edges, p->face_boundary_orientations,
            p->structure_constants,
            p->header.face_count, p->header.edge_count,
            p->header.dim_g, p->header.max_edges_per_face);
        if (rc != 0) {
            set_error(GU_ERROR_CUDA_ERROR, "Torsion kernel failed", "gu_evaluate_torsion");
            return GU_ERROR_CUDA_ERROR;
        }
#endif
    } else {
        /* Fallback: trivial torsion T = 0 */
#ifdef GU_HAS_CUDA
        CUDA_CHECK(gu_cuda_memset(torsion_buf->data, 0,
                                   (size_t)torsion_buf->element_count * sizeof(double)),
                   "gu_evaluate_torsion");
#else
        memset(torsion_buf->data, 0, (size_t)torsion_buf->element_count * sizeof(double));
#endif
    }

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
    physics_data_t* p = &g_state.physics;

    if (p->has_topology && p->has_structure_constants) {
#ifdef GU_HAS_CUDA
        /*
         * Identity Shiab: S = F = d(omega) + (1/2)[omega, omega].
         * Copy omega D->H, compute curvature on CPU, copy result H->D.
         */
        size_t omega_bytes = (size_t)omega_buf->element_count * sizeof(double);
        size_t shiab_bytes = (size_t)shiab_buf->element_count * sizeof(double);
        double* h_omega = (double*)malloc(omega_bytes);
        double* h_shiab = (double*)malloc(shiab_bytes);
        if (!h_omega || !h_shiab) {
            free(h_omega); free(h_shiab);
            set_error(GU_ERROR_ALLOCATION_FAILED, "Host staging alloc failed", "gu_evaluate_shiab");
            return GU_ERROR_ALLOCATION_FAILED;
        }
        int cuda_rc = gu_cuda_memcpy_d2h(h_omega, omega_buf->data, omega_bytes);
        if (cuda_rc != 0) {
            free(h_omega); free(h_shiab);
            set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_shiab");
            return GU_ERROR_CUDA_ERROR;
        }
        int rc = gu_curvature_assemble_physics(
            h_omega, h_shiab,
            p->face_boundary_edges, p->face_boundary_orientations,
            p->structure_constants,
            p->header.face_count, p->header.edge_count,
            p->header.dim_g, p->header.max_edges_per_face);
        if (rc != 0) {
            free(h_omega); free(h_shiab);
            set_error(GU_ERROR_CUDA_ERROR, "Shiab kernel failed", "gu_evaluate_shiab");
            return GU_ERROR_CUDA_ERROR;
        }
        cuda_rc = gu_cuda_memcpy_h2d(shiab_buf->data, h_shiab, shiab_bytes);
        free(h_omega); free(h_shiab);
        if (cuda_rc != 0) {
            set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_shiab");
            return GU_ERROR_CUDA_ERROR;
        }
#else
        /*
         * Identity Shiab: S = F = d(omega) + (1/2)[omega, omega].
         * Compute curvature directly into the shiab output buffer.
         */
        int rc = gu_curvature_assemble_physics(
            omega_buf->data, shiab_buf->data,
            p->face_boundary_edges, p->face_boundary_orientations,
            p->structure_constants,
            p->header.face_count, p->header.edge_count,
            p->header.dim_g, p->header.max_edges_per_face);
        if (rc != 0) {
            set_error(GU_ERROR_CUDA_ERROR, "Shiab kernel failed", "gu_evaluate_shiab");
            return GU_ERROR_CUDA_ERROR;
        }
#endif
    } else {
        /* Fallback: identity Shiab S = omega (stub) */
        int count = omega_buf->element_count < shiab_buf->element_count
            ? omega_buf->element_count : shiab_buf->element_count;
#ifdef GU_HAS_CUDA
        CUDA_CHECK(gu_cuda_memcpy_d2d(shiab_buf->data, omega_buf->data,
                                       (size_t)count * sizeof(double)),
                   "gu_evaluate_shiab");
#else
        memcpy(shiab_buf->data, omega_buf->data, (size_t)count * sizeof(double));
#endif
    }

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

    int count = s_buf->element_count;
    if (t_buf->element_count < count) count = t_buf->element_count;
    if (r_buf->element_count < count) count = r_buf->element_count;

#ifdef GU_HAS_CUDA
    /* Launch GPU kernel: r[i] = s[i] - t[i] */
    int rc = gu_residual_assemble_gpu(s_buf->data, t_buf->data, r_buf->data, count);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "CUDA residual kernel failed", "gu_evaluate_residual");
        return GU_ERROR_CUDA_ERROR;
    }
#else
    /* CPU reference: Upsilon = S - T */
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
    /* GPU parallel reduction: I2_h = (1/2) sum(r_i^2) */
    int rc = gu_objective_assemble_gpu(r_buf->data, r_buf->element_count, objective_out);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "CUDA objective kernel failed", "gu_evaluate_objective");
        return GU_ERROR_CUDA_ERROR;
    }
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

    int count = n < y_buf->element_count ? n : y_buf->element_count;
    if (x_buf->element_count < count) count = x_buf->element_count;

#ifdef GU_HAS_CUDA
    int rc = gu_axpy_gpu(y_buf->data, alpha, x_buf->data, count);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "CUDA axpy kernel failed", "gu_axpy");
        return GU_ERROR_CUDA_ERROR;
    }
#else
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

    int count = n < u_buf->element_count ? n : u_buf->element_count;
    if (v_buf->element_count < count) count = v_buf->element_count;

#ifdef GU_HAS_CUDA
    int rc = gu_inner_product_gpu(u_buf->data, v_buf->data, count, result_out);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "CUDA inner_product kernel failed", "gu_inner_product");
        return GU_ERROR_CUDA_ERROR;
    }
#else
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

    int count = n < x_buf->element_count ? n : x_buf->element_count;

#ifdef GU_HAS_CUDA
    int rc = gu_scale_gpu(x_buf->data, alpha, count);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "CUDA scale kernel failed", "gu_scale");
        return GU_ERROR_CUDA_ERROR;
    }
#else
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

    int count = n < dst_buf->element_count ? n : dst_buf->element_count;
    if (src_buf->element_count < count) count = src_buf->element_count;

#ifdef GU_HAS_CUDA
    int rc = gu_copy_gpu(dst_buf->data, src_buf->data, count);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "CUDA copy failed", "gu_copy");
        return GU_ERROR_CUDA_ERROR;
    }
#else
    memcpy(dst_buf->data, src_buf->data, (size_t)count * sizeof(double));
#endif

    clear_error();
    return GU_SUCCESS;
}

/* -------------------------------------------------------------------------
 * Jacobian/adjoint operations (CUDA Stage 2)
 * ------------------------------------------------------------------------- */

gu_error_code_t gu_evaluate_jacobian_action(gu_buffer_handle_t omega, gu_buffer_handle_t delta,
                                             gu_buffer_handle_t jv_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_jacobian_action");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (omega <= 0 || omega >= MAX_BUFFERS || !g_state.buffers[omega].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid omega handle", "gu_evaluate_jacobian_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (delta <= 0 || delta >= MAX_BUFFERS || !g_state.buffers[delta].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid delta handle", "gu_evaluate_jacobian_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (jv_out <= 0 || jv_out >= MAX_BUFFERS || !g_state.buffers[jv_out].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid jv_out handle", "gu_evaluate_jacobian_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* omega_buf = &g_state.buffers[omega];
    buffer_entry_t* delta_buf = &g_state.buffers[delta];
    buffer_entry_t* jv_buf = &g_state.buffers[jv_out];
    physics_data_t* p = &g_state.physics;

    if (!p->has_topology || !p->has_structure_constants) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Physics data not uploaded", "gu_evaluate_jacobian_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }

#ifdef GU_HAS_CUDA
    /* Stage D->H, compute on CPU, stage H->D */
    size_t omega_bytes = (size_t)omega_buf->element_count * sizeof(double);
    size_t delta_bytes = (size_t)delta_buf->element_count * sizeof(double);
    size_t jv_bytes = (size_t)jv_buf->element_count * sizeof(double);
    double* h_omega = (double*)malloc(omega_bytes);
    double* h_delta = (double*)malloc(delta_bytes);
    double* h_jv = (double*)malloc(jv_bytes);
    if (!h_omega || !h_delta || !h_jv) {
        free(h_omega); free(h_delta); free(h_jv);
        set_error(GU_ERROR_ALLOCATION_FAILED, "Host staging alloc failed", "gu_evaluate_jacobian_action");
        return GU_ERROR_ALLOCATION_FAILED;
    }
    int cuda_rc = gu_cuda_memcpy_d2h(h_omega, omega_buf->data, omega_bytes);
    if (cuda_rc != 0) { free(h_omega); free(h_delta); free(h_jv);
        set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_jacobian_action");
        return GU_ERROR_CUDA_ERROR; }
    cuda_rc = gu_cuda_memcpy_d2h(h_delta, delta_buf->data, delta_bytes);
    if (cuda_rc != 0) { free(h_omega); free(h_delta); free(h_jv);
        set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_jacobian_action");
        return GU_ERROR_CUDA_ERROR; }
    int rc = gu_jacobian_action_physics(
        h_omega, h_delta, h_jv, p->a0_coefficients,
        p->face_boundary_edges, p->face_boundary_orientations,
        p->structure_constants,
        p->header.face_count, p->header.edge_count,
        p->header.dim_g, p->header.max_edges_per_face);
    if (rc != 0) { free(h_omega); free(h_delta); free(h_jv);
        set_error(GU_ERROR_CUDA_ERROR, "Jacobian action kernel failed", "gu_evaluate_jacobian_action");
        return GU_ERROR_CUDA_ERROR; }
    cuda_rc = gu_cuda_memcpy_h2d(jv_buf->data, h_jv, jv_bytes);
    free(h_omega); free(h_delta); free(h_jv);
    if (cuda_rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_jacobian_action");
        return GU_ERROR_CUDA_ERROR; }
#else
    int rc = gu_jacobian_action_physics(
        omega_buf->data, delta_buf->data, jv_buf->data, p->a0_coefficients,
        p->face_boundary_edges, p->face_boundary_orientations,
        p->structure_constants,
        p->header.face_count, p->header.edge_count,
        p->header.dim_g, p->header.max_edges_per_face);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "Jacobian action kernel failed", "gu_evaluate_jacobian_action");
        return GU_ERROR_CUDA_ERROR;
    }
#endif

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_evaluate_adjoint_action(gu_buffer_handle_t omega, gu_buffer_handle_t v,
                                            gu_buffer_handle_t jtv_out) {
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_evaluate_adjoint_action");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (omega <= 0 || omega >= MAX_BUFFERS || !g_state.buffers[omega].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid omega handle", "gu_evaluate_adjoint_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (v <= 0 || v >= MAX_BUFFERS || !g_state.buffers[v].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid v handle", "gu_evaluate_adjoint_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (jtv_out <= 0 || jtv_out >= MAX_BUFFERS || !g_state.buffers[jtv_out].active) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid jtv_out handle", "gu_evaluate_adjoint_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    buffer_entry_t* omega_buf = &g_state.buffers[omega];
    buffer_entry_t* v_buf = &g_state.buffers[v];
    buffer_entry_t* jtv_buf = &g_state.buffers[jtv_out];
    physics_data_t* p = &g_state.physics;

    if (!p->has_topology || !p->has_structure_constants) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Physics data not uploaded", "gu_evaluate_adjoint_action");
        return GU_ERROR_INVALID_ARGUMENT;
    }

#ifdef GU_HAS_CUDA
    size_t omega_bytes = (size_t)omega_buf->element_count * sizeof(double);
    size_t v_bytes = (size_t)v_buf->element_count * sizeof(double);
    size_t jtv_bytes = (size_t)jtv_buf->element_count * sizeof(double);
    double* h_omega = (double*)malloc(omega_bytes);
    double* h_v = (double*)malloc(v_bytes);
    double* h_jtv = (double*)malloc(jtv_bytes);
    if (!h_omega || !h_v || !h_jtv) {
        free(h_omega); free(h_v); free(h_jtv);
        set_error(GU_ERROR_ALLOCATION_FAILED, "Host staging alloc failed", "gu_evaluate_adjoint_action");
        return GU_ERROR_ALLOCATION_FAILED;
    }
    int cuda_rc = gu_cuda_memcpy_d2h(h_omega, omega_buf->data, omega_bytes);
    if (cuda_rc != 0) { free(h_omega); free(h_v); free(h_jtv);
        set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_adjoint_action");
        return GU_ERROR_CUDA_ERROR; }
    cuda_rc = gu_cuda_memcpy_d2h(h_v, v_buf->data, v_bytes);
    if (cuda_rc != 0) { free(h_omega); free(h_v); free(h_jtv);
        set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_adjoint_action");
        return GU_ERROR_CUDA_ERROR; }
    int rc = gu_adjoint_action_physics(
        h_omega, h_v, h_jtv, p->a0_coefficients,
        p->face_boundary_edges, p->face_boundary_orientations,
        p->structure_constants,
        p->header.face_count, p->header.edge_count,
        p->header.dim_g, p->header.max_edges_per_face);
    if (rc != 0) { free(h_omega); free(h_v); free(h_jtv);
        set_error(GU_ERROR_CUDA_ERROR, "Adjoint action kernel failed", "gu_evaluate_adjoint_action");
        return GU_ERROR_CUDA_ERROR; }
    cuda_rc = gu_cuda_memcpy_h2d(jtv_buf->data, h_jtv, jtv_bytes);
    free(h_omega); free(h_v); free(h_jtv);
    if (cuda_rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, gu_cuda_get_last_error_string(), "gu_evaluate_adjoint_action");
        return GU_ERROR_CUDA_ERROR; }
#else
    int rc = gu_adjoint_action_physics(
        omega_buf->data, v_buf->data, jtv_buf->data, p->a0_coefficients,
        p->face_boundary_edges, p->face_boundary_orientations,
        p->structure_constants,
        p->header.face_count, p->header.edge_count,
        p->header.dim_g, p->header.max_edges_per_face);
    if (rc != 0) {
        set_error(GU_ERROR_CUDA_ERROR, "Adjoint action kernel failed", "gu_evaluate_adjoint_action");
        return GU_ERROR_CUDA_ERROR;
    }
#endif

    clear_error();
    return GU_SUCCESS;
}

/* -------------------------------------------------------------------------
 * Extended data upload (GAP-9)
 *
 * Upload mesh topology, Lie algebra, and background connection data
 * required by real physics kernels.
 * ------------------------------------------------------------------------- */

gu_error_code_t gu_upload_mesh_topology(
    const gu_mesh_topology_header_t* header,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const int32_t* edge_vertices)
{
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_upload_mesh_topology");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (!header || !face_boundary_edges || !face_boundary_orientations || !edge_vertices) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "NULL argument", "gu_upload_mesh_topology");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (header->face_count <= 0 || header->edge_count <= 0 || header->max_edges_per_face <= 0) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid topology dimensions", "gu_upload_mesh_topology");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    physics_data_t* p = &g_state.physics;

    /* Free any previously uploaded topology */
    free(p->face_boundary_edges);
    free(p->face_boundary_orientations);
    free(p->edge_vertices);

    p->header = *header;

    size_t face_edge_count = (size_t)header->face_count * (size_t)header->max_edges_per_face;
    size_t edge_vert_count = (size_t)header->edge_count * 2;

    p->face_boundary_edges = (int32_t*)malloc(face_edge_count * sizeof(int32_t));
    p->face_boundary_orientations = (int32_t*)malloc(face_edge_count * sizeof(int32_t));
    p->edge_vertices = (int32_t*)malloc(edge_vert_count * sizeof(int32_t));

    if (!p->face_boundary_edges || !p->face_boundary_orientations || !p->edge_vertices) {
        free(p->face_boundary_edges);       p->face_boundary_edges = NULL;
        free(p->face_boundary_orientations);p->face_boundary_orientations = NULL;
        free(p->edge_vertices);             p->edge_vertices = NULL;
        set_error(GU_ERROR_ALLOCATION_FAILED, "Failed to allocate topology arrays", "gu_upload_mesh_topology");
        return GU_ERROR_ALLOCATION_FAILED;
    }

    memcpy(p->face_boundary_edges, face_boundary_edges, face_edge_count * sizeof(int32_t));
    memcpy(p->face_boundary_orientations, face_boundary_orientations, face_edge_count * sizeof(int32_t));
    memcpy(p->edge_vertices, edge_vertices, edge_vert_count * sizeof(int32_t));

    p->has_topology = 1;
    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_upload_vertex_coordinates(
    const double* vertex_coords,
    int32_t vertex_count,
    int32_t embedding_dim)
{
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_upload_vertex_coordinates");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (!vertex_coords) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "vertex_coords is NULL", "gu_upload_vertex_coordinates");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (vertex_count <= 0 || embedding_dim <= 0) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid vertex dimensions", "gu_upload_vertex_coordinates");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    physics_data_t* p = &g_state.physics;
    free(p->vertex_coords);

    size_t count = (size_t)vertex_count * (size_t)embedding_dim;
    p->vertex_coords = (double*)malloc(count * sizeof(double));
    if (!p->vertex_coords) {
        set_error(GU_ERROR_ALLOCATION_FAILED, "Failed to allocate vertex array", "gu_upload_vertex_coordinates");
        return GU_ERROR_ALLOCATION_FAILED;
    }

    memcpy(p->vertex_coords, vertex_coords, count * sizeof(double));
    p->header.vertex_count = vertex_count;
    p->header.embedding_dimension = embedding_dim;
    p->has_vertices = 1;

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_upload_structure_constants(
    const double* structure_constants,
    int32_t dim)
{
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_upload_structure_constants");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (!structure_constants) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "structure_constants is NULL", "gu_upload_structure_constants");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (dim <= 0) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "dim must be positive", "gu_upload_structure_constants");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    physics_data_t* p = &g_state.physics;
    free(p->structure_constants);

    size_t count = (size_t)dim * (size_t)dim * (size_t)dim;
    p->structure_constants = (double*)malloc(count * sizeof(double));
    if (!p->structure_constants) {
        set_error(GU_ERROR_ALLOCATION_FAILED, "Failed to allocate structure constants", "gu_upload_structure_constants");
        return GU_ERROR_ALLOCATION_FAILED;
    }

    memcpy(p->structure_constants, structure_constants, count * sizeof(double));
    p->header.dim_g = dim;
    p->has_structure_constants = 1;

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_upload_invariant_metric(
    const double* metric,
    int32_t dim)
{
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_upload_invariant_metric");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (!metric) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "metric is NULL", "gu_upload_invariant_metric");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (dim <= 0) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "dim must be positive", "gu_upload_invariant_metric");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    physics_data_t* p = &g_state.physics;
    free(p->invariant_metric);

    size_t count = (size_t)dim * (size_t)dim;
    p->invariant_metric = (double*)malloc(count * sizeof(double));
    if (!p->invariant_metric) {
        set_error(GU_ERROR_ALLOCATION_FAILED, "Failed to allocate metric array", "gu_upload_invariant_metric");
        return GU_ERROR_ALLOCATION_FAILED;
    }

    memcpy(p->invariant_metric, metric, count * sizeof(double));
    p->has_metric = 1;

    clear_error();
    return GU_SUCCESS;
}

gu_error_code_t gu_upload_background_connection(
    const double* a0_coefficients,
    int32_t edge_count,
    int32_t dim_g)
{
    if (!g_state.initialized) {
        set_error(GU_ERROR_NOT_INITIALIZED, "Backend not initialized", "gu_upload_background_connection");
        return GU_ERROR_NOT_INITIALIZED;
    }
    if (!a0_coefficients) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "a0_coefficients is NULL", "gu_upload_background_connection");
        return GU_ERROR_INVALID_ARGUMENT;
    }
    if (edge_count <= 0 || dim_g <= 0) {
        set_error(GU_ERROR_INVALID_ARGUMENT, "Invalid A0 dimensions", "gu_upload_background_connection");
        return GU_ERROR_INVALID_ARGUMENT;
    }

    physics_data_t* p = &g_state.physics;
    free(p->a0_coefficients);

    size_t count = (size_t)edge_count * (size_t)dim_g;
    p->a0_coefficients = (double*)malloc(count * sizeof(double));
    if (!p->a0_coefficients) {
        set_error(GU_ERROR_ALLOCATION_FAILED, "Failed to allocate A0 array", "gu_upload_background_connection");
        return GU_ERROR_ALLOCATION_FAILED;
    }

    memcpy(p->a0_coefficients, a0_coefficients, count * sizeof(double));
    p->has_a0 = 1;

    clear_error();
    return GU_SUCCESS;
}

int32_t gu_has_physics_data(void) {
    physics_data_t* p = &g_state.physics;
    return p->has_topology && p->has_structure_constants && p->has_metric;
}

/* -------------------------------------------------------------------------
 * Error reporting
 * ------------------------------------------------------------------------- */

const gu_error_packet_t* gu_get_last_error(void) {
    if (!g_state.has_error) return NULL;
    return &g_state.last_error;
}
