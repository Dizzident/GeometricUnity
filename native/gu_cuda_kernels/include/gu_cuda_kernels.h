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

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Compute curvature F = d(omega) + (1/2)[omega, omega] on GPU.
 * Stub: copies omega -> curvature.
 *
 * @param d_omega      Device pointer to connection coefficients
 * @param d_curvature  Device pointer to curvature output
 * @param n            Number of elements
 * @return 0 on success, -1 on failure
 */
int gu_curvature_assemble_gpu(const double* d_omega, double* d_curvature, int n);

/**
 * Compute torsion T on GPU.
 * Stub: sets T = 0 (trivial torsion).
 *
 * @param d_omega    Device pointer to connection coefficients
 * @param d_torsion  Device pointer to torsion output
 * @param n          Number of elements
 * @return 0 on success, -1 on failure
 */
int gu_torsion_assemble_gpu(const double* d_omega, double* d_torsion, int n);

/**
 * Compute Shiab operator S on GPU.
 * Stub: copies omega -> shiab (identity Shiab).
 *
 * @param d_omega  Device pointer to connection coefficients
 * @param d_shiab  Device pointer to Shiab output
 * @param n        Number of elements
 * @return 0 on success, -1 on failure
 */
int gu_shiab_assemble_gpu(const double* d_omega, double* d_shiab, int n);

/**
 * Compute residual Upsilon = S - T on GPU.
 *
 * @param d_shiab     Device pointer to Shiab coefficients
 * @param d_torsion   Device pointer to torsion coefficients
 * @param d_residual  Device pointer to residual output
 * @param n           Number of elements
 * @return 0 on success, -1 on failure
 */
int gu_residual_assemble_gpu(
    const double* d_shiab, const double* d_torsion, double* d_residual, int n);

/**
 * Compute objective I2_h = (1/2) sum(r_i^2) on GPU.
 * Uses parallel reduction.
 *
 * @param d_residual    Device pointer to residual coefficients
 * @param n             Number of elements
 * @param objective_out Output: the objective value
 * @return 0 on success, -1 on failure
 */
int gu_objective_assemble_gpu(const double* d_residual, int n, double* objective_out);

/* =========================================================================
 * Solver primitive dispatch functions (M10)
 * ========================================================================= */

/**
 * BLAS daxpy: y = y + alpha * x.
 */
int gu_axpy_gpu(double* d_y, double alpha, const double* d_x, int n);

/**
 * BLAS ddot: result = dot(u, v).
 */
int gu_inner_product_gpu(const double* d_u, const double* d_v, int n, double* result_out);

/**
 * BLAS dscal: x = alpha * x.
 */
int gu_scale_gpu(double* d_x, double alpha, int n);

/**
 * Device-to-device copy: dst = src.
 */
int gu_copy_gpu(double* d_dst, const double* d_src, int n);

#ifdef __cplusplus
}
#endif

#endif /* GU_CUDA_KERNELS_H */
