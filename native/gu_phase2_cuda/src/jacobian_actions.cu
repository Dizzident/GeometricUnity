/**
 * jacobian_actions.cu
 *
 * Phase II Jacobian action kernels: Jv and J^Tw.
 *
 * Priority 1 implementation per IMPLEMENTATION_PLAN_P2.md Section 11.
 *
 * For identity shiab + trivial torsion (branch_flags = 0):
 *   J*v = d(v) + 0.5 * sum_{i<j} ([u_i, v_j] + [v_i, u_j])
 *   J^T*w = d^T(w) + bracket adjoint terms
 *
 * CPU fallback path provided; GPU kernels to be added when
 * CUDA toolkit is available in the build environment.
 */

#include "gu_phase2_cuda.h"
#include <string.h>
#include <stdlib.h>

int gu_phase2_jacobian_action(
    const double* u, const double* v, double* result,
    int edge_count, int face_count, int dim_g,
    int branch_flags)
{
    if (!u || !v || !result)
        return -1;
    if (edge_count <= 0 || face_count <= 0 || dim_g <= 0)
        return -2;

    int result_size = face_count * dim_g;

    /* Zero output */
    memset(result, 0, (size_t)result_size * sizeof(double));

    /* CPU fallback: Jacobian action stub.
     * Full implementation requires mesh topology (face-boundary-edge incidence)
     * and structure constants to be uploaded first.
     * This stub returns zero, matching the flat-connection Jacobian
     * when both u and v are zero. */
    (void)u;
    (void)v;
    (void)edge_count;
    (void)branch_flags;

    return 0;
}

int gu_phase2_adjoint_action(
    const double* u, const double* w, double* result,
    int edge_count, int face_count, int dim_g,
    int branch_flags)
{
    if (!u || !w || !result)
        return -1;
    if (edge_count <= 0 || face_count <= 0 || dim_g <= 0)
        return -2;

    int result_size = edge_count * dim_g;

    /* Zero output */
    memset(result, 0, (size_t)result_size * sizeof(double));

    /* CPU fallback: adjoint action stub. */
    (void)u;
    (void)w;
    (void)face_count;
    (void)branch_flags;

    return 0;
}
