/**
 * gu_phase2_init.cu
 *
 * Initialization and shutdown for Phase II CUDA kernels.
 */

#include "gu_phase2_cuda.h"
#include <stdlib.h>

static gu_phase2_config_t g_config;
static int g_initialized = 0;

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

    g_initialized = 0;
    return 0;
}
