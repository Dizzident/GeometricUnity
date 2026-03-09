/**
 * spectrum_probes.cu
 *
 * GPU-accelerated spectral probe kernels for Phase II.
 *
 * Provides GPU-side matrix-vector products for Lanczos/LOBPCG iterations
 * running on the host. The host drives the iteration; these kernels
 * provide the inner Hv or Jv products.
 *
 * Priority 4 per IMPLEMENTATION_PLAN_P2.md Section 11.
 *
 * Currently implemented via the Jacobian and Hessian action kernels
 * in jacobian_actions.cu and hessian_actions.cu. This file is reserved
 * for future GPU-native Lanczos/LOBPCG loop implementations that
 * keep all vectors on-device to minimize PCI-e transfer overhead.
 */

#include "gu_phase2_cuda.h"

/* Future: GPU-resident Lanczos iteration
 *
 * int gu_phase2_lanczos_gpu(
 *     const double* u_device,
 *     int edge_count, int face_count, int dim_g,
 *     int num_eigenvalues, double tolerance,
 *     double* eigenvalues_out, double* eigenvectors_out,
 *     int branch_flags);
 */
