/**
 * branch_kernels.cu
 *
 * Branch-parameterized residual assembly for Phase II.
 * Extends Phase I residual evaluation with branch variant dispatch tables.
 *
 * Each branch variant is encoded as an integer flag word:
 *   bits 0-1: torsion type (0=trivial, 1=augmented)
 *   bits 2-3: shiab type (0=identity, 1=trace-free)
 *   bits 4-5: bi-connection type (0=A0+omega, 1=A0-omega)
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
                result[c] += structure_constants[a * dim_g * dim_g + b * dim_g + c] * xy;
        }
    }
}

int gu_phase2_batch_residual(
    const double* connection_states, double* residuals_out,
    int batch_size, int field_dof, int residual_dof,
    const int* branch_flags_array)
{
    const gu_phase2_config_t* config;
    const int32_t* face_boundary_edges;
    const int32_t* face_boundary_orientations;
    const double* structure_constants;
    const double* a0;
    int b;

    if (!connection_states || !residuals_out || !branch_flags_array)
        return -1;
    if (batch_size <= 0 || field_dof <= 0 || residual_dof <= 0)
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
    if (field_dof != config->edge_count * config->dim_g || residual_dof != config->face_count * config->dim_g)
        return -5;

    for (b = 0; b < batch_size; b++)
    {
        const double* u = connection_states + b * field_dof;
        double* r = residuals_out + b * residual_dof;
        int fi;
        int torsion_variant = branch_flags_array[b] & 0x3;

        memset(r, 0, (size_t)residual_dof * sizeof(double));

        for (fi = 0; fi < config->face_count; fi++)
        {
            const int32_t* edges = &face_boundary_edges[fi * config->max_edges_per_face];
            const int32_t* orients = &face_boundary_orientations[fi * config->max_edges_per_face];
            double* face_residual = &r[fi * config->dim_g];
            double* d_omega = (double*)calloc((size_t)config->dim_g, sizeof(double));
            double* wedge = (double*)calloc((size_t)config->dim_g, sizeof(double));
            double* torsion = (double*)calloc((size_t)config->dim_g, sizeof(double));
            int i;
            int j;

            if (!d_omega || !wedge || !torsion)
            {
                free(d_omega);
                free(wedge);
                free(torsion);
                return -6;
            }

            for (i = 0; i < config->max_edges_per_face; i++)
            {
                int edge_idx = edges[i];
                int sign = orients[i];
                int a;
                if (edge_idx < 0)
                    break;

                for (a = 0; a < config->dim_g; a++)
                {
                    double omega_val = u[edge_idx * config->dim_g + a];
                    d_omega[a] += (double)sign * omega_val;
                    if (torsion_variant == 1 && a0)
                        torsion[a] += (double)sign * (omega_val - a0[edge_idx * config->dim_g + a]);
                }
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
                    if (ej < 0)
                        break;

                    gu_phase2_accumulate_bracket(u, ei, si, u, ej, sj, structure_constants, config->dim_g, wedge);

                    if (torsion_variant == 1 && a0)
                    {
                        gu_phase2_accumulate_bracket(a0, ei, si, u, ej, sj, structure_constants, config->dim_g, torsion);
                        gu_phase2_accumulate_bracket(u, ei, si, a0, ej, sj, structure_constants, config->dim_g, torsion);
                    }
                }
            }

            for (i = 0; i < config->dim_g; i++)
            {
                double curvature = d_omega[i] + 0.5 * wedge[i];
                face_residual[i] = torsion_variant == 1 ? curvature - torsion[i] : curvature;
            }

            free(d_omega);
            free(wedge);
            free(torsion);
        }
    }

    return 0;
}
