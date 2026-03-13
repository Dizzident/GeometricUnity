/**
 * gu_phase2_init.cu
 *
 * Initialization and shutdown for Phase II CUDA kernels.
 */

#include "gu_phase2_cuda.h"
#include <stdlib.h>
#include <string.h>

static gu_phase2_config_t g_config;
static int g_initialized = 0;
static int32_t* g_face_boundary_edges = NULL;
static int32_t* g_face_boundary_orientations = NULL;
static double* g_structure_constants = NULL;
static double* g_background_connection = NULL;

static void gu_phase2_free_uploaded_data(void)
{
    free(g_face_boundary_edges);
    free(g_face_boundary_orientations);
    free(g_structure_constants);
    free(g_background_connection);
    g_face_boundary_edges = NULL;
    g_face_boundary_orientations = NULL;
    g_structure_constants = NULL;
    g_background_connection = NULL;
}

int gu_phase2_init(const gu_phase2_config_t* config)
{
    if (!config)
        return -1;
    if (config->edge_count <= 0 || config->face_count <= 0 || config->dim_g <= 0)
        return -2;

    g_config = *config;
    g_initialized = 1;

    return 0;
}

int gu_phase2_shutdown(void)
{
    if (!g_initialized)
        return -1;

    gu_phase2_free_uploaded_data();
    g_initialized = 0;
    return 0;
}

int gu_phase2_upload_face_topology(
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    int face_count,
    int max_edges_per_face)
{
    size_t count;

    if (!g_initialized)
        return -1;
    if (!face_boundary_edges || !face_boundary_orientations)
        return -2;
    if (face_count != g_config.face_count || max_edges_per_face != g_config.max_edges_per_face)
        return -3;

    count = (size_t)face_count * (size_t)max_edges_per_face;

    free(g_face_boundary_edges);
    free(g_face_boundary_orientations);
    g_face_boundary_edges = (int32_t*)malloc(count * sizeof(int32_t));
    g_face_boundary_orientations = (int32_t*)malloc(count * sizeof(int32_t));
    if (!g_face_boundary_edges || !g_face_boundary_orientations)
    {
        free(g_face_boundary_edges);
        free(g_face_boundary_orientations);
        g_face_boundary_edges = NULL;
        g_face_boundary_orientations = NULL;
        return -4;
    }

    memcpy(g_face_boundary_edges, face_boundary_edges, count * sizeof(int32_t));
    memcpy(g_face_boundary_orientations, face_boundary_orientations, count * sizeof(int32_t));
    return 0;
}

int gu_phase2_upload_structure_constants(
    const double* structure_constants,
    int dim_g)
{
    size_t count;

    if (!g_initialized)
        return -1;
    if (!structure_constants)
        return -2;
    if (dim_g != g_config.dim_g)
        return -3;

    count = (size_t)dim_g * (size_t)dim_g * (size_t)dim_g;

    free(g_structure_constants);
    g_structure_constants = (double*)malloc(count * sizeof(double));
    if (!g_structure_constants)
        return -4;

    memcpy(g_structure_constants, structure_constants, count * sizeof(double));
    return 0;
}

int gu_phase2_upload_background_connection(
    const double* a0,
    int edge_count,
    int dim_g)
{
    size_t count;

    if (!g_initialized)
        return -1;
    if (!a0)
        return -2;
    if (edge_count != g_config.edge_count || dim_g != g_config.dim_g)
        return -3;

    count = (size_t)edge_count * (size_t)dim_g;

    free(g_background_connection);
    g_background_connection = (double*)malloc(count * sizeof(double));
    if (!g_background_connection)
        return -4;

    memcpy(g_background_connection, a0, count * sizeof(double));
    return 0;
}

int gu_phase2_is_initialized(void)
{
    return g_initialized;
}

const gu_phase2_config_t* gu_phase2_get_config(void)
{
    return g_initialized ? &g_config : NULL;
}

const int32_t* gu_phase2_get_face_boundary_edges(void)
{
    return g_face_boundary_edges;
}

const int32_t* gu_phase2_get_face_boundary_orientations(void)
{
    return g_face_boundary_orientations;
}

const double* gu_phase2_get_structure_constants(void)
{
    return g_structure_constants;
}

const double* gu_phase2_get_background_connection(void)
{
    return g_background_connection;
}
