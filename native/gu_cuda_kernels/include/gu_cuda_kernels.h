/**
 * gu_cuda_kernels.h
 *
 * Declarations for GPU kernel dispatch functions.
 * These are implemented in gu_cuda_kernels.cu and called from
 * gu_cuda_core.c when GU_HAS_CUDA is defined.
 *
 * All functions return 0 on success, -1 on failure.
 * Device pointers are managed by the caller (gu_cuda_core buffer system).
 */

#ifndef GU_CUDA_KERNELS_H
#define GU_CUDA_KERNELS_H

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/* =========================================================================
 * Physics kernel dispatch functions (GAP-2)
 *
 * These implement the real discrete physics operators using mesh topology
 * and Lie algebra data uploaded via GAP-9.
 * ========================================================================= */

/**
 * Compute curvature F = d(omega) + (1/2)[omega, omega].
 *
 * omega lives on edges: [edge_count * dim_g], indexed as [edge * dim_g + a].
 * F lives on faces: [face_count * dim_g], indexed as [face * dim_g + a].
 *
 * d(omega)[face,a] = sum_i sign_i * omega[edge_i, a]
 * [omega,omega][face,a] = sum_{i<j} sum_c f^a_{bc} * (si*omega_i_b) * (sj*omega_j_c)
 *                       (summed over i<j boundary edge pairs)
 *
 * @param omega                  Connection coefficients [edge_count * dim_g]
 * @param curvature_out          Output curvature [face_count * dim_g]
 * @param face_boundary_edges    [face_count * max_edges_per_face], padded with -1
 * @param face_boundary_orientations [face_count * max_edges_per_face], +1/-1
 * @param structure_constants    f^c_{ab} as [dim_g^3]
 * @param face_count             Number of faces
 * @param edge_count             Number of edges
 * @param dim_g                  Lie algebra dimension
 * @param max_edges_per_face     Max boundary edges per face (typically 3)
 * @return 0 on success, -1 on failure
 */
int gu_curvature_assemble_physics(
    const double* omega,
    double* curvature_out,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count,
    int edge_count,
    int dim_g,
    int max_edges_per_face);

/**
 * Compute augmented torsion T^aug = d_{A0}(omega - A0).
 *
 * T^aug[face,a] = d(alpha)[face,a] + [A0 wedge alpha][face,a]
 * where alpha = omega - A0.
 *
 * @param omega                  Connection [edge_count * dim_g]
 * @param a0                     Background connection [edge_count * dim_g]
 * @param torsion_out            Output torsion [face_count * dim_g]
 * @param face_boundary_edges    [face_count * max_edges_per_face]
 * @param face_boundary_orientations [face_count * max_edges_per_face]
 * @param structure_constants    f^c_{ab} [dim_g^3]
 * @param face_count             Number of faces
 * @param edge_count             Number of edges
 * @param dim_g                  Lie algebra dimension
 * @param max_edges_per_face     Max boundary edges per face
 * @return 0 on success, -1 on failure
 */
int gu_torsion_assemble_physics(
    const double* omega,
    const double* a0,
    double* torsion_out,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count,
    int edge_count,
    int dim_g,
    int max_edges_per_face);

/**
 * Identity Shiab: S = F (copy curvature to shiab output).
 *
 * @param curvature_in   Input curvature [face_count * dim_g]
 * @param shiab_out      Output shiab [face_count * dim_g]
 * @param n              Total elements (face_count * dim_g)
 * @return 0 on success, -1 on failure
 */
int gu_shiab_identity(
    const double* curvature_in,
    double* shiab_out,
    int n);

/* =========================================================================
 * Legacy stub dispatch functions (backward compatible)
 * ========================================================================= */

int gu_curvature_assemble_gpu(const double* d_omega, double* d_curvature, int n);
int gu_torsion_assemble_gpu(const double* d_omega, double* d_torsion, int n);
int gu_shiab_assemble_gpu(const double* d_omega, double* d_shiab, int n);

/**
 * Compute residual Upsilon = S - T on GPU.
 */
int gu_residual_assemble_gpu(
    const double* d_shiab, const double* d_torsion, double* d_residual, int n);

/**
 * Compute objective I2_h = (1/2) sum(r_i^2) on GPU.
 */
int gu_objective_assemble_gpu(const double* d_residual, int n, double* objective_out);

/* =========================================================================
 * Solver primitive dispatch functions (M10)
 * ========================================================================= */

int gu_axpy_gpu(double* d_y, double alpha, const double* d_x, int n);
int gu_inner_product_gpu(const double* d_u, const double* d_v, int n, double* result_out);
int gu_scale_gpu(double* d_x, double alpha, int n);
int gu_copy_gpu(double* d_dst, const double* d_src, int n);

#ifdef __cplusplus
}
#endif

#endif /* GU_CUDA_KERNELS_H */
