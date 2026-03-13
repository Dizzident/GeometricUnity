/**
 * hessian_actions.cu
 *
 * Phase II Hessian-style operator action: H*v = J^T M J v + lambda C^T M C v.
 *
 * Priority 2 implementation per IMPLEMENTATION_PLAN_P2.md Section 11.
 *
 * CPU fallback path provided; GPU kernels to be added when
 * CUDA toolkit is available in the build environment.
 */

#include "gu_phase2_cuda.h"
#include <string.h>
#include <stdlib.h>

int gu_phase2_hessian_action(
    const double* u, const double* v, double* result,
    int edge_count, int face_count, int dim_g,
    double lambda, int branch_flags)
{
    double* jv;
    double* jtjv;
    int field_size;
    int residual_size;
    int i;
    int rc;

    if (!u || !v || !result)
        return -1;
    if (edge_count <= 0 || face_count <= 0 || dim_g <= 0)
        return -2;
    if (!gu_phase2_is_initialized())
        return -3;

    field_size = edge_count * dim_g;
    residual_size = face_count * dim_g;
    memset(result, 0, (size_t)field_size * sizeof(double));

    jv = (double*)calloc((size_t)residual_size, sizeof(double));
    jtjv = (double*)calloc((size_t)field_size, sizeof(double));
    if (!jv || !jtjv)
    {
        free(jv);
        free(jtjv);
        return -4;
    }

    rc = gu_phase2_jacobian_action(u, v, jv, edge_count, face_count, dim_g, branch_flags);
    if (rc != 0)
    {
        free(jv);
        free(jtjv);
        return rc;
    }

    rc = gu_phase2_adjoint_action(u, jv, jtjv, edge_count, face_count, dim_g, branch_flags);
    if (rc != 0)
    {
        free(jv);
        free(jtjv);
        return rc;
    }

    for (i = 0; i < field_size; i++)
        result[i] = jtjv[i] + lambda * v[i];

    free(jv);
    free(jtjv);

    return 0;
}
