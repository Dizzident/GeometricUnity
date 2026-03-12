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
#include <stddef.h>

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
 * Jacobian/adjoint kernel dispatch functions (CUDA Stage 2)
 * ========================================================================= */

/**
 * Compute Jacobian-vector product: Jv = (dUpsilon/domega) * delta.
 * For identity Shiab + augmented torsion:
 *   J*delta = dF/domega(delta) - dT/domega(delta)
 *
 * Curvature linearization: dF/domega(delta) = d(delta) + 0.5*sum_{i<j}([omega_i,delta_j]+[delta_i,omega_j])
 * Torsion linearization: dT/domega(delta) = d_{A0}(delta) = d(delta) + [A0 wedge delta]
 *
 * @param omega       Connection [edge_count * dim_g]
 * @param delta       Perturbation [edge_count * dim_g]
 * @param jv_out      Output J*delta [face_count * dim_g]
 * @param a0          Background connection [edge_count * dim_g] (may be NULL for zero A0)
 * @param face_boundary_edges    [face_count * max_edges_per_face]
 * @param face_boundary_orientations [face_count * max_edges_per_face]
 * @param structure_constants    f^c_{ab} [dim_g^3]
 * @param face_count  Number of faces
 * @param edge_count  Number of edges
 * @param dim_g       Lie algebra dimension
 * @param max_edges_per_face  Max boundary edges per face
 * @return 0 on success, -1 on failure
 */
int gu_jacobian_action_physics(
    const double* omega,
    const double* delta,
    double* jv_out,
    const double* a0,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count,
    int edge_count,
    int dim_g,
    int max_edges_per_face);

/**
 * Compute adjoint action: J^T*v.
 * Uses column-by-column Jacobian application (basis vector approach).
 *
 * @param omega       Connection [edge_count * dim_g]
 * @param v           Input vector [face_count * dim_g]
 * @param jtv_out     Output J^T*v [edge_count * dim_g]
 * @param a0          Background connection [edge_count * dim_g] (may be NULL)
 * @param face_boundary_edges    [face_count * max_edges_per_face]
 * @param face_boundary_orientations [face_count * max_edges_per_face]
 * @param structure_constants    f^c_{ab} [dim_g^3]
 * @param face_count  Number of faces
 * @param edge_count  Number of edges
 * @param dim_g       Lie algebra dimension
 * @param max_edges_per_face  Max boundary edges per face
 * @return 0 on success, -1 on failure
 */
int gu_adjoint_action_physics(
    const double* omega,
    const double* v,
    double* jtv_out,
    const double* a0,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count,
    int edge_count,
    int dim_g,
    int max_edges_per_face);

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

/* =========================================================================
 * Phase IV Dirac kernel dispatch functions (M44)
 *
 * Fermionic operator kernels for the discrete Dirac operator on Y_h.
 *
 * All spinor vectors are interleaved complex: [Re_0, Im_0, Re_1, Im_1, ...].
 * spinor_dof = 2 * cell_count * spinor_dim * gauge_dim  (interleaved real length).
 *
 * Gamma matrices are uploaded once at initialization via gu_dirac_upload_gammas.
 * They are stored as device constants for the lifetime of the kernel context.
 * ========================================================================= */

/**
 * Upload gamma matrices to device constants.
 * Must be called once before any gu_dirac_* kernel invocations.
 *
 * @param gamma_re       Flat row-major real parts: [mu * spinor_dim^2 + row * spinor_dim + col]
 * @param gamma_im       Flat row-major imag parts: same layout
 * @param chirality_re   Chirality matrix real part [spinor_dim^2], NULL for odd dimY
 * @param chirality_im   Chirality matrix imag part [spinor_dim^2], NULL for odd dimY
 * @param spacetime_dim  Number of gamma matrices (dimY)
 * @param spinor_dim     Spinor dimension (2^floor(dimY/2))
 * @return 0 on success, -1 on failure
 */
int gu_dirac_upload_gammas(
    const double* gamma_re,
    const double* gamma_im,
    const double* chirality_re,
    const double* chirality_im,
    int spacetime_dim,
    int spinor_dim);

/**
 * Apply a single gamma matrix: result = Gamma_mu * spinor (per cell).
 *
 * @param spinor_in      Input spinor [spinor_dof]
 * @param result_out     Output [spinor_dof]
 * @param mu             Gamma index in [0, spacetime_dim)
 * @param cell_count     Number of cells
 * @param spinor_dim     Spinor dimension
 * @param gauge_dim      Gauge representation dimension
 * @return 0 on success, -1 on failure
 */
int gu_dirac_gamma_action_gpu(
    const double* spinor_in,
    double* result_out,
    int mu,
    int cell_count,
    int spinor_dim,
    int gauge_dim);

/**
 * Apply the full discrete Dirac operator: result = D_h * spinor.
 *
 * D_h = sum_mu Gamma_mu * nabla_spin_mu (vertex-based finite difference).
 * Gauge coupling enters through the uploaded spin connection coefficients.
 *
 * @param spinor_in            Input spinor [spinor_dof]
 * @param result_out           Output [spinor_dof]
 * @param gauge_coupling_coeff Gauge coupling part of spin connection [edge_count * spinor_dim * spinor_dim * 2]
 * @param vertex_edge_incidence Vertex-edge incidence [vertex_count * max_edges_per_vertex]
 * @param vertex_edge_orient    Vertex-edge orientations [vertex_count * max_edges_per_vertex]
 * @param vertex_count          Number of mesh vertices
 * @param edge_count            Number of mesh edges
 * @param cell_count            Number of mesh cells (= vertex_count for vertex-based)
 * @param spinor_dim            Spinor dimension
 * @param gauge_dim             Gauge representation dimension
 * @param max_edges_per_vertex  Padding width for incidence arrays
 * @return 0 on success, -1 on failure
 */
int gu_dirac_apply_gpu(
    const double* spinor_in,
    double* result_out,
    const double* gauge_coupling_coeff,
    const int32_t* vertex_edge_incidence,
    const int32_t* vertex_edge_orient,
    int vertex_count,
    int edge_count,
    int cell_count,
    int spinor_dim,
    int gauge_dim,
    int max_edges_per_vertex);

/**
 * Apply the fermionic mass operator: result = M_psi * spinor.
 * M_psi = diag(vol_cell * I_spinor). For uniform vol=1.0 this is identity.
 *
 * @param spinor_in    Input spinor [spinor_dof]
 * @param result_out   Output [spinor_dof]
 * @param cell_volumes Cell volumes [cell_count]. NULL => uniform volume 1.0.
 * @param spinor_dof   Total interleaved spinor array length
 * @param cell_count   Number of cells
 * @param spinor_dim   Spinor dimension per cell
 * @param gauge_dim    Gauge dimension per cell
 * @return 0 on success, -1 on failure
 */
int gu_dirac_mass_apply_gpu(
    const double* spinor_in,
    double* result_out,
    const double* cell_volumes,
    int spinor_dof,
    int cell_count,
    int spinor_dim,
    int gauge_dim);

/**
 * Apply chirality projector: result = P_L * spinor (left=1) or P_R * spinor (left=0).
 * P_L = (I - Gamma_chi) / 2, P_R = (I + Gamma_chi) / 2.
 * Requires gu_dirac_upload_gammas to have been called with non-NULL chirality matrices.
 *
 * @param spinor_in   Input spinor [spinor_dof]
 * @param result_out  Output [spinor_dof]
 * @param left        1 for P_L, 0 for P_R
 * @param cell_count  Number of cells
 * @param spinor_dim  Spinor dimension
 * @param gauge_dim   Gauge dimension
 * @return 0 on success, -1 on failure (-2 if chirality matrix not uploaded)
 */
int gu_dirac_chirality_project_gpu(
    const double* spinor_in,
    double* result_out,
    int left,
    int cell_count,
    int spinor_dim,
    int gauge_dim);

/**
 * Compute boson-fermion coupling proxy scalar:
 *   g = <spinorI, delta_D[bosonK] spinorJ>
 * where delta_D[b_k] = Gamma^mu * b_k_mu^a * rho(T_a) (analytical variation).
 *
 * Returns real and imaginary parts of the scalar g.
 *
 * @param spinor_i       Left spinor [spinor_dof]
 * @param spinor_j       Right spinor [spinor_dof]
 * @param boson_k        Boson mode vector [edge_count * gauge_dim]
 * @param result_re_out  Output real part (scalar)
 * @param result_im_out  Output imaginary part (scalar)
 * @param spinor_dof     Total interleaved spinor array length
 * @param edge_count     Number of mesh edges
 * @param cell_count     Number of cells
 * @param spinor_dim     Spinor dimension
 * @param gauge_dim      Gauge dimension
 * @param spacetime_dim  Number of gamma matrices
 * @return 0 on success, -1 on failure
 */
int gu_dirac_coupling_proxy_gpu(
    const double* spinor_i,
    const double* spinor_j,
    const double* boson_k,
    double* result_re_out,
    double* result_im_out,
    int spinor_dof,
    int edge_count,
    int cell_count,
    int spinor_dim,
    int gauge_dim,
    int spacetime_dim);

/* =========================================================================
 * CUDA runtime wrappers
 *
 * These wrap cudaMalloc/cudaFree/cudaMemcpy so that gu_cuda_core.c (compiled
 * by gcc, not nvcc) can perform device memory management without including
 * <cuda_runtime.h>.
 *
 * Return 0 on success, non-zero CUDA error code on failure.
 * On failure, gu_cuda_get_last_error_string() returns the error description.
 * ========================================================================= */

int gu_cuda_device_init(void);
int gu_cuda_device_reset(void);

int gu_cuda_malloc(void** devptr, size_t size);
int gu_cuda_free(void* devptr);
int gu_cuda_memcpy_h2d(void* dst, const void* src, size_t count);
int gu_cuda_memcpy_d2h(void* dst, const void* src, size_t count);
int gu_cuda_memcpy_d2d(void* dst, const void* src, size_t count);
int gu_cuda_memset(void* devptr, int value, size_t count);

const char* gu_cuda_get_last_error_string(void);

#ifdef __cplusplus
}
#endif

#endif /* GU_CUDA_KERNELS_H */
