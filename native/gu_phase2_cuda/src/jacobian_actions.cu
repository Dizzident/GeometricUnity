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

static void gu_phase2_accumulate_bracket(
    const double* field_x, int edge_x, int sign_x,
    const double* field_y, int edge_y, int sign_y,
    const double* structure_constants,
    int dim_g,
    double* result)
{
    int a;
    int b;
    int c;

    for (a = 0; a < dim_g; a++)
    {
        double xa = (double)sign_x * field_x[edge_x * dim_g + a];
        if (xa == 0.0)
            continue;

        for (b = 0; b < dim_g; b++)
        {
            double yb = (double)sign_y * field_y[edge_y * dim_g + b];
            double xy = xa * yb;
            if (xy == 0.0)
                continue;

            for (c = 0; c < dim_g; c++)
            {
                result[c] += structure_constants[a * dim_g * dim_g + b * dim_g + c] * xy;
            }
        }
    }
}

int gu_phase2_jacobian_action(
    const double* u, const double* v, double* result,
    int edge_count, int face_count, int dim_g,
    int branch_flags)
{
    const gu_phase2_config_t* config;
    const int32_t* face_boundary_edges;
    const int32_t* face_boundary_orientations;
    const double* structure_constants;
    const double* a0;
    int fi;

    if (!u || !v || !result)
        return -1;
    if (edge_count <= 0 || face_count <= 0 || dim_g <= 0)
        return -2;
    if (!gu_phase2_is_initialized())
        return -3;

    config = gu_phase2_get_config();
    face_boundary_edges = gu_phase2_get_face_boundary_edges();
    face_boundary_orientations = gu_phase2_get_face_boundary_orientations();
    structure_constants = gu_phase2_get_structure_constants();
    a0 = gu_phase2_get_background_connection();
    if (!config || !face_boundary_edges || !face_boundary_orientations || !structure_constants)
        return -4;
    if (config->edge_count != edge_count || config->face_count != face_count || config->dim_g != dim_g)
        return -5;

    memset(result, 0, (size_t)face_count * (size_t)dim_g * sizeof(double));

    for (fi = 0; fi < face_count; fi++)
    {
        const int32_t* edges = &face_boundary_edges[fi * config->max_edges_per_face];
        const int32_t* orients = &face_boundary_orientations[fi * config->max_edges_per_face];
        double* out_face = &result[fi * dim_g];
        double* d_delta = (double*)calloc((size_t)dim_g, sizeof(double));
        double* curv_linear = (double*)calloc((size_t)dim_g, sizeof(double));
        double* a0_linear = (double*)calloc((size_t)dim_g, sizeof(double));
        int i;
        int j;

        if (!d_delta || !curv_linear || !a0_linear)
        {
            free(d_delta);
            free(curv_linear);
            free(a0_linear);
            return -6;
        }

        for (i = 0; i < config->max_edges_per_face; i++)
        {
            int edge_idx = edges[i];
            int sign = orients[i];
            int a;

            if (edge_idx < 0)
                break;

            for (a = 0; a < dim_g; a++)
                d_delta[a] += (double)sign * v[edge_idx * dim_g + a];
        }

        for (i = 0; i < config->max_edges_per_face; i++)
        {
            int ei = edges[i];
            int si = orients[i];
            if (ei < 0)
                break;

            for (j = i + 1; j < config->max_edges_per_face; j++)
            {
                int ej = edges[j];
                int sj = orients[j];
                int a;

                if (ej < 0)
                    break;

                gu_phase2_accumulate_bracket(u, ei, si, v, ej, sj, structure_constants, dim_g, curv_linear);
                gu_phase2_accumulate_bracket(v, ei, si, u, ej, sj, structure_constants, dim_g, curv_linear);

                if (a0)
                {
                    gu_phase2_accumulate_bracket(a0, ei, si, v, ej, sj, structure_constants, dim_g, a0_linear);
                    gu_phase2_accumulate_bracket(v, ei, si, a0, ej, sj, structure_constants, dim_g, a0_linear);
                }

                (void)a;
            }
        }

        for (i = 0; i < dim_g; i++)
        {
            double d_f = d_delta[i] + 0.5 * curv_linear[i];
            double d_t = d_delta[i];
            if (a0 && (branch_flags & 0x3) == 1)
                d_t += a0_linear[i];
            out_face[i] = d_f - d_t;
        }

        free(d_delta);
        free(curv_linear);
        free(a0_linear);
    }

    return 0;
}

int gu_phase2_adjoint_action(
    const double* u, const double* w, double* result,
    int edge_count, int face_count, int dim_g,
    int branch_flags)
{
    double* basis;
    double* j_basis;
    int k;
    int n_edge;
    int n_face;

    if (!u || !w || !result)
        return -1;
    if (edge_count <= 0 || face_count <= 0 || dim_g <= 0)
        return -2;
    if (!gu_phase2_is_initialized())
        return -3;

    n_edge = edge_count * dim_g;
    n_face = face_count * dim_g;
    memset(result, 0, (size_t)n_edge * sizeof(double));

    basis = (double*)calloc((size_t)n_edge, sizeof(double));
    j_basis = (double*)calloc((size_t)n_face, sizeof(double));
    if (!basis || !j_basis)
    {
        free(basis);
        free(j_basis);
        return -4;
    }

    for (k = 0; k < n_edge; k++)
    {
        double dot = 0.0;
        int i;
        int rc;

        basis[k] = 1.0;
        rc = gu_phase2_jacobian_action(u, basis, j_basis, edge_count, face_count, dim_g, branch_flags);
        basis[k] = 0.0;
        if (rc != 0)
        {
            free(basis);
            free(j_basis);
            return rc;
        }

        for (i = 0; i < n_face; i++)
            dot += j_basis[i] * w[i];
        result[k] = dot;
    }

    free(basis);
    free(j_basis);

    return 0;
}
