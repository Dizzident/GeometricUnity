/**
 * continuation_kernels.cu
 *
 * Phase II continuation inner-solve kernels.
 *
 * Provides GPU-accelerated inner linear solves for the predictor-corrector
 * continuation loop. Each continuation step requires solving:
 *   J^T J delta = -J^T r (Newton correction)
 * where J and r are evaluated at the current (omega, lambda) pair.
 *
 * Priority 5 per IMPLEMENTATION_PLAN_P2.md Section 11.
 *
 * Currently, continuation inner solves use the host-side Krylov solver
 * with GPU Hv actions. This file is reserved for future GPU-native
 * CG/GMRES implementations that keep the Krylov basis on-device.
 */

#include "gu_phase2_cuda.h"

/* Future: GPU-resident conjugate gradient for Hessian system
 *
 * int gu_phase2_cg_solve_gpu(
 *     const double* u_device,
 *     const double* rhs_device,
 *     double* solution_device,
 *     int field_dof, double tolerance, int max_iter,
 *     double lambda, int branch_flags);
 */
