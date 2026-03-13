/**
 * gu_phase2_cuda.h
 *
 * Phase II CUDA kernel API for Geometric Unity.
 *
 * Extends Phase I kernels with:
 *   - Branch-parameterized Jacobian actions (Jv, J^Tw)
 *   - Hessian-style operator actions (Hv = J^T M J v + lambda C^T M C v)
 *   - Batched multi-branch residual evaluation
 *   - Spectral probe support kernels
 *   - Continuation inner solve kernels
 *
 * All functions return 0 on success, non-zero on error.
 */

#ifndef GU_PHASE2_CUDA_H
#define GU_PHASE2_CUDA_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Kernel configuration for Phase II operations.
 */
typedef struct {
    int edge_count;
    int face_count;
    int dim_g;
    int max_edges_per_face;
} gu_phase2_config_t;

/**
 * Initialize Phase II CUDA kernels with geometry configuration.
 * Must be called before any kernel invocations.
 */
int gu_phase2_init(const gu_phase2_config_t* config);

/**
 * Shutdown Phase II CUDA resources.
 */
int gu_phase2_shutdown(void);

/**
 * Upload face-boundary topology used by Jacobian/Hessian kernels.
 */
int gu_phase2_upload_face_topology(
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    int face_count,
    int max_edges_per_face);

/**
 * Upload Lie-algebra structure constants f^c_{ab}.
 */
int gu_phase2_upload_structure_constants(
    const double* structure_constants,
    int dim_g);

/**
 * Upload the background connection A0 used by augmented torsion branches.
 */
int gu_phase2_upload_background_connection(
    const double* a0,
    int edge_count,
    int dim_g);

/* =========================================================================
 * Priority 1: Jacobian actions (Jv and J^Tw)
 * ========================================================================= */

/**
 * Compute Jacobian-vector product: result = J(u) * v.
 *
 * J is the linearization of the residual map at connection u.
 * For identity shiab + trivial torsion:
 *   J*v = d(v) + 0.5 * sum_{i<j} ([u_i, v_j] + [v_i, u_j])
 *
 * @param u            Current connection (edge_count * dim_g doubles)
 * @param v            Perturbation direction (edge_count * dim_g doubles)
 * @param result       Output J*v (face_count * dim_g doubles)
 * @param edge_count   Number of mesh edges
 * @param face_count   Number of mesh faces
 * @param dim_g        Lie algebra dimension
 * @param branch_flags Encoded branch variant parameters
 */
int gu_phase2_jacobian_action(
    const double* u, const double* v, double* result,
    int edge_count, int face_count, int dim_g,
    int branch_flags);

/**
 * Compute adjoint action: result = J(u)^T * w.
 *
 * Maps face-valued w to edge-valued result.
 *
 * @param u            Current connection (edge_count * dim_g doubles)
 * @param w            Input vector (face_count * dim_g doubles)
 * @param result       Output J^T*w (edge_count * dim_g doubles)
 * @param edge_count   Number of mesh edges
 * @param face_count   Number of mesh faces
 * @param dim_g        Lie algebra dimension
 * @param branch_flags Encoded branch variant parameters
 */
int gu_phase2_adjoint_action(
    const double* u, const double* w, double* result,
    int edge_count, int face_count, int dim_g,
    int branch_flags);

/* =========================================================================
 * Priority 2: Hessian action (H = J^T M J + lambda C^T M C)
 * ========================================================================= */

/**
 * Compute Hessian-vector product: result = H(u) * v.
 *
 * H = J^T * M_R * J + lambda * C^T * M_0 * C
 *
 * @param u            Current connection (edge_count * dim_g doubles)
 * @param v            Input vector (edge_count * dim_g doubles)
 * @param result       Output H*v (edge_count * dim_g doubles)
 * @param edge_count   Number of mesh edges
 * @param face_count   Number of mesh faces
 * @param dim_g        Lie algebra dimension
 * @param lambda       Gauge penalty weight
 * @param branch_flags Encoded branch variant parameters
 */
int gu_phase2_hessian_action(
    const double* u, const double* v, double* result,
    int edge_count, int face_count, int dim_g,
    double lambda, int branch_flags);

/* =========================================================================
 * Priority 3: Batched multi-branch residual evaluation
 * ========================================================================= */

/**
 * Evaluate residuals for multiple branch variants in a single launch.
 *
 * @param connection_states  Packed connection states (batch_size * field_dof doubles)
 * @param residuals_out      Packed output residuals (batch_size * residual_dof doubles)
 * @param batch_size         Number of branches to evaluate
 * @param field_dof          DOFs per connection state
 * @param residual_dof       DOFs per residual
 * @param branch_flags_array Branch flags for each batch item (batch_size ints)
 */
int gu_phase2_batch_residual(
    const double* connection_states, double* residuals_out,
    int batch_size, int field_dof, int residual_dof,
    const int* branch_flags_array);

/* Internal shared state accessors used across translation units. */
int gu_phase2_is_initialized(void);
const gu_phase2_config_t* gu_phase2_get_config(void);
const int32_t* gu_phase2_get_face_boundary_edges(void);
const int32_t* gu_phase2_get_face_boundary_orientations(void);
const double* gu_phase2_get_structure_constants(void);
const double* gu_phase2_get_background_connection(void);

#ifdef __cplusplus
}
#endif

#endif /* GU_PHASE2_CUDA_H */
