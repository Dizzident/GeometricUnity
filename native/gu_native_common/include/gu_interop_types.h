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

/* Extended mesh topology descriptor for physics kernel data */
typedef struct {
    int32_t edge_count;
    int32_t face_count;
    int32_t vertex_count;
    int32_t embedding_dimension;
    int32_t max_edges_per_face;  /* typically 3 for triangular faces */
    int32_t dim_g;               /* Lie algebra dimension */
} gu_mesh_topology_header_t;

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

/* =========================================================================
 * Extended data upload functions (GAP-9)
 *
 * These upload mesh topology and Lie algebra data required by physics kernels.
 * Must be called after gu_initialize() and before any compute kernel dispatch.
 * ========================================================================= */

/**
 * Upload mesh topology: face-boundary-edge incidence and orientations.
 *
 * @param header            Mesh topology dimensions
 * @param face_boundary_edges      Flat array [face_count * max_edges_per_face] of edge indices per face.
 *                                 Padded with -1 if a face has fewer than max_edges_per_face boundary edges.
 * @param face_boundary_orientations Flat array [face_count * max_edges_per_face] of +1/-1 signs.
 * @param edge_vertices     Flat array [edge_count * 2] of vertex indices per edge: {v0, v1}.
 * @return GU_SUCCESS or error code.
 */
gu_error_code_t gu_upload_mesh_topology(
    const gu_mesh_topology_header_t* header,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const int32_t* edge_vertices);

/**
 * Upload vertex coordinates.
 *
 * @param vertex_coords  Flat array [vertex_count * embedding_dimension] of doubles.
 * @param vertex_count   Number of vertices.
 * @param embedding_dim  Dimension of the embedding space.
 * @return GU_SUCCESS or error code.
 */
gu_error_code_t gu_upload_vertex_coordinates(
    const double* vertex_coords,
    int32_t vertex_count,
    int32_t embedding_dim);

/**
 * Upload Lie algebra structure constants f^c_{ab}.
 *
 * @param structure_constants  Flat array [dim * dim * dim] indexed as [a*dim*dim + b*dim + c].
 * @param dim                  Lie algebra dimension (e.g. 3 for su(2)).
 * @return GU_SUCCESS or error code.
 */
gu_error_code_t gu_upload_structure_constants(
    const double* structure_constants,
    int32_t dim);

/**
 * Upload Lie algebra invariant metric g_{ab}.
 *
 * @param metric  Flat array [dim * dim] row-major symmetric matrix.
 * @param dim     Lie algebra dimension.
 * @return GU_SUCCESS or error code.
 */
gu_error_code_t gu_upload_invariant_metric(
    const double* metric,
    int32_t dim);

/**
 * Upload background connection A0.
 *
 * @param a0_coefficients  Flat array [edge_count * dim_g] of connection coefficients.
 * @param edge_count       Number of edges.
 * @param dim_g            Lie algebra dimension.
 * @return GU_SUCCESS or error code.
 */
gu_error_code_t gu_upload_background_connection(
    const double* a0_coefficients,
    int32_t edge_count,
    int32_t dim_g);

/**
 * Query whether topology and algebra data have been uploaded.
 * @return 1 if all data uploaded, 0 otherwise.
 */
int32_t gu_has_physics_data(void);

#ifdef __cplusplus
}
#endif

#endif /* GU_INTEROP_TYPES_H */
