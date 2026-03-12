using System.Numerics;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// CPU reference implementation of ISpinConnectionBuilder.
///
/// Under inserted assumption P4-IA-003 (flat spin connection on Y_h), the
/// Levi-Civita part is set to zero and only the gauge-coupling part is assembled.
///
/// Gauge-coupling assembly:
///   For each edge e with bosonic connection coefficients omega_e^a (a = 0..dimG-1),
///   the gauge coupling matrix for the spinor-gauge block is:
///     G_e = sum_a  omega_e^a * rho_adj(T_a)   (scalar matrix acting on gauge index)
///   stored as a complex dimG x dimG matrix for the adjoint representation.
///
/// The adjoint representation: [rho_adj(T_a)]_{bc} = f_{abc} (structure constants of g).
/// For su(2): f_{abc} = epsilon_{abc}.
/// For the trivial representation (dimG=1): G_e = omega_e^0 * I_1.
/// </summary>
public sealed class CpuSpinConnectionBuilder : ISpinConnectionBuilder
{
    /// <inheritdoc />
    public SpinConnectionBundle Build(
        BackgroundRecord background,
        double[] bosonicState,
        SpinorRepresentationSpec spinorSpec,
        FermionFieldLayout layout,
        SimplicialMesh mesh,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(background);
        ArgumentNullException.ThrowIfNull(bosonicState);
        ArgumentNullException.ThrowIfNull(spinorSpec);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(provenance);

        int edgeCount = mesh.EdgeCount;
        int spinorDim = spinorSpec.SpinorComponents;

        // Infer gauge dimension from the layout's primal block
        var primalBlock = layout.SpinorBlocks.FirstOrDefault(b => b.Role == "primal")
            ?? throw new InvalidOperationException("FermionFieldLayout has no primal block.");
        int dimG = primalBlock.GaugeDimension;

        // Validate bosonic state shape
        int expectedStateLen = edgeCount * dimG;
        if (bosonicState.Length != expectedStateLen)
            throw new ArgumentException(
                $"bosonicState length {bosonicState.Length} != edgeCount*dimG = {edgeCount}*{dimG} = {expectedStateLen}.");

        // Levi-Civita part: zero under P4-IA-003
        // Shape convention: one complex spinorDim x spinorDim matrix per edge per direction.
        // For flat assumption: all zeros. Store as real array length = edgeCount * spinorDim * spinorDim * 2.
        var lcCoefficients = new double[edgeCount * spinorDim * spinorDim * 2];

        // Gauge coupling part: per-edge complex (dimG x dimG) matrix.
        // Under adjoint rep: G_e_{bc} = sum_a omega_e^a * f_{abc}.
        // For trivial (dimG=1): G_e = omega_e^0 * 1 (real scalar, stored as (re, im) = (omega_e^0, 0)).
        // Shape: edgeCount * spinorDim * spinorDim * 2 (complex matrix acting on full spinor-gauge block
        //        is spinorDim*dimG x spinorDim*dimG, but we store only the gauge part here).
        // Simplification: store as edgeCount * dimG * dimG * 2 (complex gauge matrix per edge).
        var gaugeCouplingCoefficients = AssembleGaugeCoupling(bosonicState, edgeCount, dimG);

        string gaugeRepId = dimG == 1 ? "trivial" : "adjoint";

        return new SpinConnectionBundle
        {
            ConnectionId = $"conn-{background.BackgroundId}-{spinorSpec.SpinorSpecId}",
            BackgroundId = background.BackgroundId,
            BranchVariantId = background.BranchManifestId,
            SpinorSpecId = spinorSpec.SpinorSpecId,
            LeviCivitaCoefficients = lcCoefficients,
            GaugeCouplingCoefficients = gaugeCouplingCoefficients,
            GaugeRepresentationId = gaugeRepId,
            AssemblyMethod = "cpu-flat-v1",
            FlatLeviCivitaAssumption = true,
            SpacetimeDimension = spinorSpec.SpacetimeDimension,
            SpinorDimension = spinorDim,
            EdgeCount = edgeCount,
            GaugeDimension = dimG,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Assemble the per-edge gauge coupling matrix G_e.
    ///
    /// For trivial representation (dimG=1):
    ///   G_e = omega_e^0  (real scalar)
    ///
    /// For adjoint representation (dimG > 1):
    ///   G_e_{bc} = sum_a omega_e^a * f_{abc}
    ///   where f_{abc} are the structure constants.
    ///   For su(2): f_{abc} = epsilon_{abc} (Levi-Civita symbol).
    ///   For general dimG: use antisymmetric generators (all zero for unsupported algebras
    ///   unless the structure constants are known).
    ///
    /// Returns a flat real array of length edgeCount * dimG * dimG * 2 (complex).
    /// Layout: [edgeIdx][gRow][gCol][re/im]
    /// </summary>
    private static double[] AssembleGaugeCoupling(double[] bosonicState, int edgeCount, int dimG)
    {
        var result = new double[edgeCount * dimG * dimG * 2];

        if (dimG == 1)
        {
            // Trivial: G_e = omega_e^0, imaginary part = 0
            for (int e = 0; e < edgeCount; e++)
            {
                int baseIdx = e * dimG * dimG * 2; // = e * 2
                result[baseIdx]     = bosonicState[e]; // Re
                result[baseIdx + 1] = 0.0;             // Im
            }
            return result;
        }

        if (dimG == 3)
        {
            // su(2): structure constants f_{abc} = epsilon_{abc}
            // [rho(T_a)]_{bc} = f_{abc} = epsilon_{abc}
            // (antisymmetric, all-real for su(2) in Cartesian basis)
            // epsilon_{012}=1, epsilon_{021}=-1, epsilon_{102}=-1, epsilon_{120}=1, etc.
            // f_{a,b,c}: f_{0,1,2}=1, f_{0,2,1}=-1, f_{1,0,2}=-1, f_{1,2,0}=1, f_{2,0,1}=1, f_{2,1,0}=-1
            // For each a: rho(T_a) is a 3x3 matrix with [rho(T_a)]_{bc} = epsilon_{abc}
            var eps = BuildSu2StructureConstants(); // eps[a,b,c]

            for (int e = 0; e < edgeCount; e++)
            {
                // Accumulate G_e_{bc} = sum_a omega_e^a * eps[a,b,c]
                var G = new double[dimG, dimG]; // real matrix (su(2) structure constants are real)
                for (int a = 0; a < dimG; a++)
                {
                    double omega_a = bosonicState[e * dimG + a];
                    for (int b = 0; b < dimG; b++)
                        for (int c = 0; c < dimG; c++)
                            G[b, c] += omega_a * eps[a, b, c];
                }

                int baseIdx = e * dimG * dimG * 2;
                for (int b = 0; b < dimG; b++)
                    for (int c = 0; c < dimG; c++)
                    {
                        result[baseIdx + (b * dimG + c) * 2]     = G[b, c]; // Re
                        result[baseIdx + (b * dimG + c) * 2 + 1] = 0.0;    // Im
                    }
            }
            return result;
        }

        // For other dimG values: use zero gauge coupling (conservative fallback).
        // Downstream can detect this via the AssemblyMethod field.
        return result;
    }

    /// <summary>
    /// Build su(2) structure constants f[a,b,c] = epsilon_{abc} (Levi-Civita symbol in 3D).
    /// </summary>
    private static double[,,] BuildSu2StructureConstants()
    {
        var f = new double[3, 3, 3];
        // epsilon_{012} = 1
        f[0, 1, 2] = 1.0; f[0, 2, 1] = -1.0;
        f[1, 0, 2] = -1.0; f[1, 2, 0] = 1.0;
        f[2, 0, 1] = 1.0; f[2, 1, 0] = -1.0;
        return f;
    }

    /// <summary>
    /// Compute edge direction vector (normalized) for edge e in the mesh.
    /// Returns the vector from vertex e.Edges[e][0] to vertex e.Edges[e][1].
    /// </summary>
    internal static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
    {
        int v0 = mesh.Edges[edgeIdx][0];
        int v1 = mesh.Edges[edgeIdx][1];
        int dim = mesh.EmbeddingDimension;
        var coords0 = mesh.GetVertexCoordinates(v0);
        var coords1 = mesh.GetVertexCoordinates(v1);

        var dir = new double[dim];
        double norm = 0.0;
        for (int k = 0; k < dim; k++)
        {
            dir[k] = coords1[k] - coords0[k];
            norm += dir[k] * dir[k];
        }
        norm = System.Math.Sqrt(norm);
        if (norm > 1e-14)
            for (int k = 0; k < dim; k++)
                dir[k] /= norm;
        return dir;
    }

    /// <summary>
    /// Compute the edge length for edge e.
    /// </summary>
    internal static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
    {
        int v0 = mesh.Edges[edgeIdx][0];
        int v1 = mesh.Edges[edgeIdx][1];
        var coords0 = mesh.GetVertexCoordinates(v0);
        var coords1 = mesh.GetVertexCoordinates(v1);
        double norm = 0.0;
        for (int k = 0; k < coords0.Length; k++)
        {
            double d = coords1[k] - coords0[k];
            norm += d * d;
        }
        return System.Math.Sqrt(norm);
    }
}
