/**
 * gu_cuda_kernels.cu
 *
 * CUDA/CPU kernel implementations for Geometric Unity physics operators.
 *
 * Real physics kernels (GAP-2):
 *   - Curvature: F = d(omega) + (1/2)[omega, omega]
 *   - Augmented torsion: T^aug = d_{A0}(omega - A0)
 *   - Identity Shiab: S = F
 *   - Solver primitives: axpy, inner_product, scale, copy
 *
 * All kernels match the C# reference implementations in Gu.ReferenceCpu:
 *   - CurvatureAssembler.cs
 *   - AugmentedTorsionCpu.cs
 *   - IdentityShiabCpu.cs
 */

#include "gu_cuda_kernels.h"
#include <stdio.h>
#include <math.h>
#include <string.h>
#include <stdlib.h>

/* =========================================================================
 * Lie bracket helper
 *
 * Computes [x, y]_c = sum_{a,b} f^c_{ab} * x_a * y_b
 * where structure_constants are indexed as [a * dim * dim + b * dim + c].
 * ========================================================================= */

static void lie_bracket(
    const double* x, const double* y,
    const double* structure_constants,
    int dim_g, double* result)
{
    for (int c = 0; c < dim_g; c++) {
        double sum = 0.0;
        for (int a = 0; a < dim_g; a++) {
            for (int b = 0; b < dim_g; b++) {
                sum += structure_constants[a * dim_g * dim_g + b * dim_g + c] * x[a] * y[b];
            }
        }
        result[c] = sum;
    }
}

/* =========================================================================
 * Physics kernel implementations (CPU path)
 *
 * These implement the real discrete operators. The CUDA device kernel
 * versions (under #ifdef __CUDACC__) would parallelize over faces.
 * ========================================================================= */

#ifdef __CUDACC__

/* ---- CUDA device kernels ------------------------------------------------ */

/**
 * CUDA kernel: curvature assembly, one thread per face.
 *
 * F[face,a] = d(omega)[face,a] + 0.5 * sum_{i<j} bracket(si*omega_i, sj*omega_j)[a]
 */
__global__ void gu_curvature_physics_kernel(
    const double* __restrict__ omega,
    double* __restrict__ curvature_out,
    const int32_t* __restrict__ face_boundary_edges,
    const int32_t* __restrict__ face_boundary_orientations,
    const double* __restrict__ structure_constants,
    int face_count, int dim_g, int max_edges_per_face)
{
    int fi = blockIdx.x * blockDim.x + threadIdx.x;
    if (fi >= face_count) return;

    /* Shared scratch arrays -- use local memory since dim_g is small (3-8) */
    double dOmega[16]; /* dim_g <= 16 assumed */
    double wedgeTerm[16];
    double omegaI[16], omegaJ[16];
    double bracket[16];

    for (int a = 0; a < dim_g; a++) {
        dOmega[a] = 0.0;
        wedgeTerm[a] = 0.0;
    }

    const int32_t* edges = &face_boundary_edges[fi * max_edges_per_face];
    const int32_t* orients = &face_boundary_orientations[fi * max_edges_per_face];

    /* Count actual boundary edges (stop at -1 padding) */
    int nEdges = 0;
    for (int i = 0; i < max_edges_per_face; i++) {
        if (edges[i] < 0) break;
        nEdges++;
    }

    /* 1. d(omega): sum of signed omega values on boundary edges */
    for (int i = 0; i < nEdges; i++) {
        int edgeIdx = edges[i];
        int sign = orients[i];
        for (int a = 0; a < dim_g; a++) {
            dOmega[a] += sign * omega[edgeIdx * dim_g + a];
        }
    }

    /* 2. (1/2)[omega, omega]: bracket over edge pairs */
    for (int i = 0; i < nEdges; i++) {
        for (int j = i + 1; j < nEdges; j++) {
            int ei = edges[i], ej = edges[j];
            int si = orients[i], sj = orients[j];

            for (int a = 0; a < dim_g; a++) {
                omegaI[a] = si * omega[ei * dim_g + a];
                omegaJ[a] = sj * omega[ej * dim_g + a];
            }

            /* Compute Lie bracket */
            for (int c = 0; c < dim_g; c++) {
                double sum = 0.0;
                for (int a = 0; a < dim_g; a++) {
                    for (int b = 0; b < dim_g; b++) {
                        sum += structure_constants[a * dim_g * dim_g + b * dim_g + c]
                             * omegaI[a] * omegaJ[b];
                    }
                }
                wedgeTerm[c] += sum;
            }
        }
    }

    /* F = d(omega) + 0.5 * [omega, omega] */
    for (int a = 0; a < dim_g; a++) {
        curvature_out[fi * dim_g + a] = dOmega[a] + 0.5 * wedgeTerm[a];
    }
}

/**
 * CUDA kernel: augmented torsion, one thread per face.
 * T^aug = d_{A0}(alpha) where alpha = omega - A0
 */
__global__ void gu_torsion_physics_kernel(
    const double* __restrict__ omega,
    const double* __restrict__ a0,
    double* __restrict__ torsion_out,
    const int32_t* __restrict__ face_boundary_edges,
    const int32_t* __restrict__ face_boundary_orientations,
    const double* __restrict__ structure_constants,
    int face_count, int edge_count, int dim_g, int max_edges_per_face)
{
    int fi = blockIdx.x * blockDim.x + threadIdx.x;
    if (fi >= face_count) return;

    double dAlpha[16];
    double bracketTerm[16];
    double a0I[16], a0J[16], alphaI[16], alphaJ[16];
    double bracket1[16], bracket2[16];

    for (int a = 0; a < dim_g; a++) {
        dAlpha[a] = 0.0;
        bracketTerm[a] = 0.0;
    }

    const int32_t* edges = &face_boundary_edges[fi * max_edges_per_face];
    const int32_t* orients = &face_boundary_orientations[fi * max_edges_per_face];

    int nEdges = 0;
    for (int i = 0; i < max_edges_per_face; i++) {
        if (edges[i] < 0) break;
        nEdges++;
    }

    /* 1. d(alpha): alpha = omega - A0 */
    for (int i = 0; i < nEdges; i++) {
        int edgeIdx = edges[i];
        int sign = orients[i];
        for (int a = 0; a < dim_g; a++) {
            double alpha_val = omega[edgeIdx * dim_g + a] - a0[edgeIdx * dim_g + a];
            dAlpha[a] += sign * alpha_val;
        }
    }

    /* 2. [A0 wedge alpha]: cross bracket terms */
    for (int i = 0; i < nEdges; i++) {
        for (int j = i + 1; j < nEdges; j++) {
            int ei = edges[i], ej = edges[j];
            int si = orients[i], sj = orients[j];

            for (int a = 0; a < dim_g; a++) {
                a0I[a] = si * a0[ei * dim_g + a];
                a0J[a] = sj * a0[ej * dim_g + a];
                alphaI[a] = si * (omega[ei * dim_g + a] - a0[ei * dim_g + a]);
                alphaJ[a] = sj * (omega[ej * dim_g + a] - a0[ej * dim_g + a]);
            }

            /* [A0_i, alpha_j] + [alpha_i, A0_j] */
            for (int c = 0; c < dim_g; c++) {
                double sum1 = 0.0, sum2 = 0.0;
                for (int aa = 0; aa < dim_g; aa++) {
                    for (int bb = 0; bb < dim_g; bb++) {
                        double f = structure_constants[aa * dim_g * dim_g + bb * dim_g + c];
                        sum1 += f * a0I[aa] * alphaJ[bb];
                        sum2 += f * alphaI[aa] * a0J[bb];
                    }
                }
                bracketTerm[c] += sum1 + sum2;
            }
        }
    }

    /* T^aug = d(alpha) + [A0 wedge alpha] */
    for (int a = 0; a < dim_g; a++) {
        torsion_out[fi * dim_g + a] = dAlpha[a] + bracketTerm[a];
    }
}

/* ---- CUDA solver primitive kernels -------------------------------------- */

__global__ void gu_axpy_kernel(double* y, double alpha, const double* x, int n) {
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx < n) y[idx] += alpha * x[idx];
}

__global__ void gu_scale_kernel(double* x, double alpha, int n) {
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx < n) x[idx] *= alpha;
}

__global__ void gu_dot_reduce_kernel(
    const double* u, const double* v, double* partial, int n)
{
    extern __shared__ double sdata[];
    int tid = threadIdx.x;
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    sdata[tid] = (idx < n) ? u[idx] * v[idx] : 0.0;
    __syncthreads();
    for (int s = blockDim.x / 2; s > 0; s >>= 1) {
        if (tid < s) sdata[tid] += sdata[tid + s];
        __syncthreads();
    }
    if (tid == 0) partial[blockIdx.x] = sdata[0];
}

/* ---- Legacy stub kernels (for backward compat) -------------------------- */

__global__ void gu_curvature_kernel(
    const double* __restrict__ omega,
    double* __restrict__ curvature_out, int n)
{
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx < n) curvature_out[idx] = omega[idx];
}

__global__ void gu_torsion_kernel(
    const double* __restrict__ omega,
    double* __restrict__ torsion_out, int n)
{
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx < n) torsion_out[idx] = 0.0;
}

__global__ void gu_shiab_kernel(
    const double* __restrict__ omega,
    double* __restrict__ shiab_out, int n)
{
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx < n) shiab_out[idx] = omega[idx];
}

__global__ void gu_residual_kernel(
    const double* __restrict__ shiab,
    const double* __restrict__ torsion,
    double* __restrict__ residual_out, int n)
{
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    if (idx < n) residual_out[idx] = shiab[idx] - torsion[idx];
}

__global__ void gu_objective_reduce_kernel(
    const double* __restrict__ residual,
    double* __restrict__ partial_sums, int n)
{
    extern __shared__ double sdata[];
    int tid = threadIdx.x;
    int idx = blockIdx.x * blockDim.x + threadIdx.x;
    sdata[tid] = (idx < n) ? residual[idx] * residual[idx] : 0.0;
    __syncthreads();
    for (int s = blockDim.x / 2; s > 0; s >>= 1) {
        if (tid < s) sdata[tid] += sdata[tid + s];
        __syncthreads();
    }
    if (tid == 0) partial_sums[blockIdx.x] = sdata[0];
}

/* ---- Host dispatch functions -------------------------------------------- */

extern "C" {

#define GU_CUDA_BLOCK_SIZE 256

/* --- Real physics dispatch --- */

int gu_curvature_assemble_physics(
    const double* omega, double* curvature_out,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count, int edge_count, int dim_g, int max_edges_per_face)
{
    int blocks = (face_count + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_curvature_physics_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(
        omega, curvature_out, face_boundary_edges, face_boundary_orientations,
        structure_constants, face_count, dim_g, max_edges_per_face);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_torsion_assemble_physics(
    const double* omega, const double* a0, double* torsion_out,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count, int edge_count, int dim_g, int max_edges_per_face)
{
    int blocks = (face_count + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_torsion_physics_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(
        omega, a0, torsion_out, face_boundary_edges, face_boundary_orientations,
        structure_constants, face_count, edge_count, dim_g, max_edges_per_face);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_shiab_identity(const double* curvature_in, double* shiab_out, int n) {
    cudaError_t err = cudaMemcpy(shiab_out, curvature_in, n * sizeof(double), cudaMemcpyDeviceToDevice);
    return (err == cudaSuccess) ? 0 : -1;
}

/* --- Legacy stubs --- */

int gu_curvature_assemble_gpu(const double* d_omega, double* d_curvature, int n) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_curvature_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(d_omega, d_curvature, n);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_torsion_assemble_gpu(const double* d_omega, double* d_torsion, int n) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_torsion_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(d_omega, d_torsion, n);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_shiab_assemble_gpu(const double* d_omega, double* d_shiab, int n) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_shiab_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(d_omega, d_shiab, n);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_residual_assemble_gpu(
    const double* d_shiab, const double* d_torsion, double* d_residual, int n)
{
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_residual_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(d_shiab, d_torsion, d_residual, n);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_objective_assemble_gpu(const double* d_residual, int n, double* objective_out) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    double* d_partial;
    cudaError_t err = cudaMalloc(&d_partial, blocks * sizeof(double));
    if (err != cudaSuccess) return -1;
    gu_objective_reduce_kernel<<<blocks, GU_CUDA_BLOCK_SIZE,
        GU_CUDA_BLOCK_SIZE * sizeof(double)>>>(d_residual, d_partial, n);
    err = cudaGetLastError();
    if (err != cudaSuccess) { cudaFree(d_partial); return -1; }
    err = cudaDeviceSynchronize();
    if (err != cudaSuccess) { cudaFree(d_partial); return -1; }
    double* h_partial = (double*)malloc(blocks * sizeof(double));
    if (!h_partial) { cudaFree(d_partial); return -1; }
    err = cudaMemcpy(h_partial, d_partial, blocks * sizeof(double), cudaMemcpyDeviceToHost);
    cudaFree(d_partial);
    if (err != cudaSuccess) { free(h_partial); return -1; }
    double sum = 0.0;
    for (int i = 0; i < blocks; i++) sum += h_partial[i];
    free(h_partial);
    *objective_out = 0.5 * sum;
    return 0;
}

/* --- Solver primitives (CUDA) --- */

int gu_axpy_gpu(double* d_y, double alpha, const double* d_x, int n) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_axpy_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(d_y, alpha, d_x, n);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_inner_product_gpu(const double* d_u, const double* d_v, int n, double* result_out) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    double* d_partial;
    cudaError_t err = cudaMalloc(&d_partial, blocks * sizeof(double));
    if (err != cudaSuccess) return -1;
    gu_dot_reduce_kernel<<<blocks, GU_CUDA_BLOCK_SIZE,
        GU_CUDA_BLOCK_SIZE * sizeof(double)>>>(d_u, d_v, d_partial, n);
    err = cudaGetLastError();
    if (err != cudaSuccess) { cudaFree(d_partial); return -1; }
    err = cudaDeviceSynchronize();
    if (err != cudaSuccess) { cudaFree(d_partial); return -1; }
    double* h_partial = (double*)malloc(blocks * sizeof(double));
    if (!h_partial) { cudaFree(d_partial); return -1; }
    err = cudaMemcpy(h_partial, d_partial, blocks * sizeof(double), cudaMemcpyDeviceToHost);
    cudaFree(d_partial);
    if (err != cudaSuccess) { free(h_partial); return -1; }
    double sum = 0.0;
    for (int i = 0; i < blocks; i++) sum += h_partial[i];
    free(h_partial);
    *result_out = sum;
    return 0;
}

int gu_scale_gpu(double* d_x, double alpha, int n) {
    int blocks = (n + GU_CUDA_BLOCK_SIZE - 1) / GU_CUDA_BLOCK_SIZE;
    gu_scale_kernel<<<blocks, GU_CUDA_BLOCK_SIZE>>>(d_x, alpha, n);
    cudaError_t err = cudaGetLastError();
    if (err != cudaSuccess) return -1;
    err = cudaDeviceSynchronize();
    return (err == cudaSuccess) ? 0 : -1;
}

int gu_copy_gpu(double* d_dst, const double* d_src, int n) {
    cudaError_t err = cudaMemcpy(d_dst, d_src, n * sizeof(double), cudaMemcpyDeviceToDevice);
    return (err == cudaSuccess) ? 0 : -1;
}

} /* extern "C" */

#else /* !__CUDACC__ -- Plain C compilation for reference/testing */

/* =========================================================================
 * CPU implementations of physics kernels
 * These match the C# reference implementations exactly.
 * ========================================================================= */

int gu_curvature_assemble_physics(
    const double* omega, double* curvature_out,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count, int edge_count, int dim_g, int max_edges_per_face)
{
    /* Allocate scratch arrays for bracket computation */
    double* omegaI = (double*)malloc(dim_g * sizeof(double));
    double* omegaJ = (double*)malloc(dim_g * sizeof(double));
    double* bracket = (double*)malloc(dim_g * sizeof(double));
    if (!omegaI || !omegaJ || !bracket) {
        free(omegaI); free(omegaJ); free(bracket);
        return -1;
    }

    for (int fi = 0; fi < face_count; fi++) {
        const int32_t* edges = &face_boundary_edges[fi * max_edges_per_face];
        const int32_t* orients = &face_boundary_orientations[fi * max_edges_per_face];

        /* Count actual boundary edges */
        int nEdges = 0;
        for (int i = 0; i < max_edges_per_face; i++) {
            if (edges[i] < 0) break;
            nEdges++;
        }

        /* 1. d(omega) on this face */
        for (int a = 0; a < dim_g; a++) {
            double d_omega_a = 0.0;
            for (int i = 0; i < nEdges; i++) {
                d_omega_a += orients[i] * omega[edges[i] * dim_g + a];
            }
            curvature_out[fi * dim_g + a] = d_omega_a;
        }

        /* 2. (1/2)[omega, omega]: bracket over boundary edge pairs */
        for (int i = 0; i < nEdges; i++) {
            for (int j = i + 1; j < nEdges; j++) {
                int ei = edges[i], ej = edges[j];
                int si = orients[i], sj = orients[j];

                for (int a = 0; a < dim_g; a++) {
                    omegaI[a] = si * omega[ei * dim_g + a];
                    omegaJ[a] = sj * omega[ej * dim_g + a];
                }

                lie_bracket(omegaI, omegaJ, structure_constants, dim_g, bracket);

                for (int a = 0; a < dim_g; a++) {
                    curvature_out[fi * dim_g + a] += 0.5 * bracket[a];
                }
            }
        }
    }

    free(omegaI); free(omegaJ); free(bracket);
    return 0;
}

int gu_torsion_assemble_physics(
    const double* omega, const double* a0, double* torsion_out,
    const int32_t* face_boundary_edges,
    const int32_t* face_boundary_orientations,
    const double* structure_constants,
    int face_count, int edge_count, int dim_g, int max_edges_per_face)
{
    double* a0I = (double*)malloc(dim_g * sizeof(double));
    double* a0J = (double*)malloc(dim_g * sizeof(double));
    double* alphaI = (double*)malloc(dim_g * sizeof(double));
    double* alphaJ = (double*)malloc(dim_g * sizeof(double));
    double* bracket1 = (double*)malloc(dim_g * sizeof(double));
    double* bracket2 = (double*)malloc(dim_g * sizeof(double));
    if (!a0I || !a0J || !alphaI || !alphaJ || !bracket1 || !bracket2) {
        free(a0I); free(a0J); free(alphaI); free(alphaJ);
        free(bracket1); free(bracket2);
        return -1;
    }

    for (int fi = 0; fi < face_count; fi++) {
        const int32_t* edges = &face_boundary_edges[fi * max_edges_per_face];
        const int32_t* orients = &face_boundary_orientations[fi * max_edges_per_face];

        int nEdges = 0;
        for (int i = 0; i < max_edges_per_face; i++) {
            if (edges[i] < 0) break;
            nEdges++;
        }

        /* 1. d(alpha) where alpha = omega - A0 */
        for (int a = 0; a < dim_g; a++) {
            double d_alpha_a = 0.0;
            for (int i = 0; i < nEdges; i++) {
                double alpha_val = omega[edges[i] * dim_g + a] - a0[edges[i] * dim_g + a];
                d_alpha_a += orients[i] * alpha_val;
            }
            torsion_out[fi * dim_g + a] = d_alpha_a;
        }

        /* 2. [A0 wedge alpha]: cross bracket terms */
        for (int i = 0; i < nEdges; i++) {
            for (int j = i + 1; j < nEdges; j++) {
                int ei = edges[i], ej = edges[j];
                int si = orients[i], sj = orients[j];

                for (int a = 0; a < dim_g; a++) {
                    a0I[a] = si * a0[ei * dim_g + a];
                    a0J[a] = sj * a0[ej * dim_g + a];
                    alphaI[a] = si * (omega[ei * dim_g + a] - a0[ei * dim_g + a]);
                    alphaJ[a] = sj * (omega[ej * dim_g + a] - a0[ej * dim_g + a]);
                }

                /* [A0_i, alpha_j] + [alpha_i, A0_j] (NO 0.5 factor) */
                lie_bracket(a0I, alphaJ, structure_constants, dim_g, bracket1);
                lie_bracket(alphaI, a0J, structure_constants, dim_g, bracket2);

                for (int a = 0; a < dim_g; a++) {
                    torsion_out[fi * dim_g + a] += bracket1[a] + bracket2[a];
                }
            }
        }
    }

    free(a0I); free(a0J); free(alphaI); free(alphaJ);
    free(bracket1); free(bracket2);
    return 0;
}

int gu_shiab_identity(const double* curvature_in, double* shiab_out, int n) {
    memcpy(shiab_out, curvature_in, n * sizeof(double));
    return 0;
}

/* =========================================================================
 * Legacy stub implementations (CPU fallback)
 * ========================================================================= */

int gu_curvature_assemble_gpu(const double* omega, double* curvature, int n) {
    for (int i = 0; i < n; i++) curvature[i] = omega[i];
    return 0;
}

int gu_torsion_assemble_gpu(const double* omega, double* torsion, int n) {
    for (int i = 0; i < n; i++) torsion[i] = 0.0;
    return 0;
}

int gu_shiab_assemble_gpu(const double* omega, double* shiab, int n) {
    for (int i = 0; i < n; i++) shiab[i] = omega[i];
    return 0;
}

int gu_residual_assemble_gpu(
    const double* shiab, const double* torsion, double* residual, int n)
{
    for (int i = 0; i < n; i++) residual[i] = shiab[i] - torsion[i];
    return 0;
}

int gu_objective_assemble_gpu(const double* residual, int n, double* objective_out) {
    double sum = 0.0;
    for (int i = 0; i < n; i++) sum += residual[i] * residual[i];
    *objective_out = 0.5 * sum;
    return 0;
}

/* Solver primitives (CPU) */

int gu_axpy_gpu(double* y, double alpha, const double* x, int n) {
    for (int i = 0; i < n; i++) y[i] += alpha * x[i];
    return 0;
}

int gu_inner_product_gpu(const double* u, const double* v, int n, double* result_out) {
    double sum = 0.0;
    for (int i = 0; i < n; i++) sum += u[i] * v[i];
    *result_out = sum;
    return 0;
}

int gu_scale_gpu(double* x, double alpha, int n) {
    for (int i = 0; i < n; i++) x[i] *= alpha;
    return 0;
}

int gu_copy_gpu(double* dst, const double* src, int n) {
    for (int i = 0; i < n; i++) dst[i] = src[i];
    return 0;
}

#endif /* __CUDACC__ */
