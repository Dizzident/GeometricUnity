using System.Numerics;
using Gu.Core;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// CPU reference implementation of IDiracOperatorAssembler.
///
/// Implements the discrete Dirac operator using a vertex-centered
/// finite-difference scheme on SimplicialMesh:
///
///   (D_h psi)_v = sum_{e=(v,w) in edges} Gamma_hat(e) * (psi_w - psi_v) / |e|
///
/// where Gamma_hat(e) is the gamma matrix for the principal direction of edge e
/// (direction mu = argmax |e_mu|).
///
/// PhysicsNote: This is a minimal vertex-based discrete Dirac operator.
/// Accuracy improves with mesh refinement.
/// </summary>
public sealed class CpuDiracOperatorAssembler : IDiracOperatorAssembler
{
    private const int MaxExplicitMatrixDof = 4096;
    private const double DefaultHermiticityTolerance = 1e-10;

    /// <inheritdoc />
    public DiracOperatorBundle Assemble(
        SpinConnectionBundle connection,
        GammaOperatorBundle gammas,
        FermionFieldLayout layout,
        Gu.Geometry.SimplicialMesh mesh,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(provenance);

        string fermionBackgroundId = connection.BackgroundId;
        int cellCount = mesh.CellCount;

        int spinorDim = gammas.SpinorDimension;
        int gaugeDim = layout.SpinorBlocks
            .Where(b => b.Role == "primal")
            .Select(b => b.GaugeDimension)
            .FirstOrDefault(1);
        int dofsPerCell = spinorDim * gaugeDim;
        // Vertex-based DOF count (ApplyMatrixFree uses vertex indexing)
        int totalDof = mesh.VertexCount * dofsPerCell;

        bool hasExplicit = totalDof <= MaxExplicitMatrixDof;
        double[]? explicitMatrix = null;
        double hermResidual = 0.0;

        if (hasExplicit)
        {
            explicitMatrix = BuildExplicitComplexMatrix(connection, gammas, mesh, spinorDim, gaugeDim, dofsPerCell, totalDof);
            hermResidual = ComputeHermiticityResidual(explicitMatrix, totalDof);
        }

        bool isHermitian = hermResidual <= DefaultHermiticityTolerance;
        string operatorId = $"dirac-{fermionBackgroundId}-{connection.SpinorSpecId}";

        return new DiracOperatorBundle
        {
            OperatorId = operatorId,
            FermionBackgroundId = fermionBackgroundId,
            LayoutId = layout.LayoutId,
            SpinConnectionId = connection.ConnectionId,
            MatrixShape = new[] { totalDof, totalDof },
            HasExplicitMatrix = hasExplicit,
            ExplicitMatrix = explicitMatrix,
            ExplicitMatrixRef = null,
            IsHermitian = isHermitian,
            HermiticityResidual = hermResidual,
            HermiticityTolerance = DefaultHermiticityTolerance,
            MassBranchTermIncluded = false,
            CorrectionTermIncluded = false,
            GaugeReductionApplied = false,
            CellCount = mesh.VertexCount,
            DofsPerCell = dofsPerCell,
            Provenance = provenance,
        };
    }

    /// <inheritdoc />
    public double[] Apply(DiracOperatorBundle bundle, double[] psi)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(psi);

        int totalDof = bundle.TotalDof;
        if (psi.Length != 2 * totalDof)
            throw new ArgumentException(
                $"psi length {psi.Length} does not match expected {2 * totalDof} (2 * {totalDof} dofs).");

        if (bundle.HasExplicitMatrix && bundle.ExplicitMatrix != null)
            return ApplyExplicitMatrix(bundle.ExplicitMatrix, totalDof, psi);

        throw new InvalidOperationException(
            "DiracOperatorBundle has no explicit matrix. Use Assemble with a small system " +
            "or use the extended Apply overload with mesh/gammas/layout.");
    }

    /// <summary>
    /// Extended matrix-free apply providing mesh and gamma context directly.
    /// Used by FermionSpectralSolver for large systems.
    /// </summary>
    public double[] Apply(
        DiracOperatorBundle bundle,
        SpinConnectionBundle connection,
        GammaOperatorBundle gammas,
        FermionFieldLayout layout,
        Gu.Geometry.SimplicialMesh mesh,
        double[] psi)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(gammas);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(psi);

        if (bundle.HasExplicitMatrix && bundle.ExplicitMatrix != null)
            return ApplyExplicitMatrix(bundle.ExplicitMatrix, bundle.TotalDof, psi);

        int spinorDim = gammas.SpinorDimension;
        int gaugeDim = layout.SpinorBlocks
            .Where(b => b.Role == "primal")
            .Select(b => b.GaugeDimension)
            .FirstOrDefault(1);
        int dofsPerCell = spinorDim * gaugeDim;
        int vertexCount = mesh.VertexCount;
        int totalDof = vertexCount * dofsPerCell;

        if (psi.Length != 2 * totalDof)
            throw new ArgumentException(
                $"psi length {psi.Length} does not match expected {2 * totalDof}.");

        return ApplyMatrixFree(connection, gammas, mesh, spinorDim, gaugeDim, dofsPerCell, psi);
    }

    // -------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------

    private static double[] BuildExplicitComplexMatrix(
        SpinConnectionBundle connection,
        GammaOperatorBundle gammas,
        Gu.Geometry.SimplicialMesh mesh,
        int spinorDim,
        int gaugeDim,
        int dofsPerCell,
        int totalDof)
    {
        var M = new double[totalDof * totalDof * 2];
        var e = new double[2 * totalDof];

        for (int j = 0; j < totalDof; j++)
        {
            if (j > 0) e[(j - 1) * 2] = 0.0;
            e[j * 2] = 1.0;

            var col = ApplyMatrixFree(connection, gammas, mesh, spinorDim, gaugeDim, dofsPerCell, e);
            for (int i = 0; i < totalDof; i++)
            {
                M[(i * totalDof + j) * 2] = col[i * 2];
                M[(i * totalDof + j) * 2 + 1] = col[i * 2 + 1];
            }
            e[j * 2] = 0.0;
        }

        return M;
    }

    private static double[] ApplyMatrixFree(
        SpinConnectionBundle connection,
        GammaOperatorBundle gammas,
        Gu.Geometry.SimplicialMesh mesh,
        int spinorDim,
        int gaugeDim,
        int dofsPerCell,
        double[] psi)
    {
        var output = new double[psi.Length];
        double[] coords = mesh.VertexCoordinates;
        int embDim = mesh.EmbeddingDimension;
        int nGammas = gammas.GammaMatrices.Length;

        for (int edgeIdx = 0; edgeIdx < mesh.Edges.Length; edgeIdx++)
        {
            int[] edge = mesh.Edges[edgeIdx];
            int v = edge[0];
            int w = edge[1];

            int mu = DominantDirection(coords, v, w, embDim, out double length);
            if (length < 1e-14 || mu >= nGammas) continue;

            var gamma = gammas.GammaMatrices[mu];

            for (int g = 0; g < gaugeDim; g++)
            {
                int vBase = 2 * (v * dofsPerCell + g * spinorDim);
                int wBase = 2 * (w * dofsPerCell + g * spinorDim);

                var diff = new Complex[spinorDim];
                for (int s = 0; s < spinorDim; s++)
                {
                    double dRe = (psi[wBase + 2 * s] - psi[vBase + 2 * s]) / length;
                    double dIm = (psi[wBase + 2 * s + 1] - psi[vBase + 2 * s + 1]) / length;
                    diff[s] = new Complex(dRe, dIm);
                }

                // Contribution to vertex v: +Gamma * diff
                for (int r = 0; r < spinorDim; r++)
                {
                    Complex sum = Complex.Zero;
                    for (int s = 0; s < spinorDim; s++)
                        sum += gamma[r, s] * diff[s];
                    output[vBase + 2 * r] += sum.Real;
                    output[vBase + 2 * r + 1] += sum.Imaginary;
                }

                // Contribution to vertex w: -Gamma * diff (antisymmetric)
                for (int r = 0; r < spinorDim; r++)
                {
                    Complex sum = Complex.Zero;
                    for (int s = 0; s < spinorDim; s++)
                        sum += gamma[r, s] * diff[s];
                    output[wBase + 2 * r] -= sum.Real;
                    output[wBase + 2 * r + 1] -= sum.Imaginary;
                }
            }

            AddGaugeCouplingContribution(
                output,
                psi,
                connection,
                gamma,
                edgeIdx,
                length,
                v,
                w,
                spinorDim,
                gaugeDim,
                dofsPerCell);
        }

        return output;
    }

    private static void AddGaugeCouplingContribution(
        double[] output,
        double[] psi,
        SpinConnectionBundle connection,
        Complex[,] gamma,
        int edgeIdx,
        double length,
        int v,
        int w,
        int spinorDim,
        int gaugeDim,
        int dofsPerCell)
    {
        if (length < 1e-14 || connection.GaugeCouplingCoefficients.Length == 0)
            return;

        double invLength = 1.0 / length;
        int edgeOffset = edgeIdx * gaugeDim * gaugeDim * 2;
        int vBase = 2 * (v * dofsPerCell);
        int wBase = 2 * (w * dofsPerCell);

        for (int sRow = 0; sRow < spinorDim; sRow++)
        {
            for (int gRow = 0; gRow < gaugeDim; gRow++)
            {
                double sumVRe = 0.0;
                double sumVIm = 0.0;
                double sumWRe = 0.0;
                double sumWIm = 0.0;

                for (int sCol = 0; sCol < spinorDim; sCol++)
                {
                    for (int gCol = 0; gCol < gaugeDim; gCol++)
                    {
                        int gaugeOffsetForward = edgeOffset + (gRow * gaugeDim + gCol) * 2;
                        int gaugeOffsetBackward = edgeOffset + (gCol * gaugeDim + gRow) * 2;

                        Complex gammaForward = gamma[sRow, sCol];
                        Complex gammaBackward = Complex.Conjugate(gamma[sCol, sRow]);
                        Complex gaugeForward = new(
                            connection.GaugeCouplingCoefficients[gaugeOffsetForward],
                            connection.GaugeCouplingCoefficients[gaugeOffsetForward + 1]);
                        Complex gaugeBackward = Complex.Conjugate(new Complex(
                            connection.GaugeCouplingCoefficients[gaugeOffsetBackward],
                            connection.GaugeCouplingCoefficients[gaugeOffsetBackward + 1]));

                        Complex coeffForward = gammaForward * gaugeForward * invLength;
                        Complex coeffBackward = gammaBackward * gaugeBackward * invLength;

                        int sourceW = wBase + 2 * (gCol * spinorDim + sCol);
                        int sourceV = vBase + 2 * (gCol * spinorDim + sCol);
                        Complex psiW = new(psi[sourceW], psi[sourceW + 1]);
                        Complex psiV = new(psi[sourceV], psi[sourceV + 1]);

                        Complex contribV = coeffForward * psiW;
                        Complex contribW = coeffBackward * psiV;
                        sumVRe += contribV.Real;
                        sumVIm += contribV.Imaginary;
                        sumWRe += contribW.Real;
                        sumWIm += contribW.Imaginary;
                    }
                }

                int targetV = vBase + 2 * (gRow * spinorDim + sRow);
                int targetW = wBase + 2 * (gRow * spinorDim + sRow);
                output[targetV] += sumVRe;
                output[targetV + 1] += sumVIm;
                output[targetW] += sumWRe;
                output[targetW + 1] += sumWIm;
            }
        }
    }

    private static double[] ApplyExplicitMatrix(double[] M, int n, double[] psi)
    {
        var result = new double[2 * n];
        for (int i = 0; i < n; i++)
        {
            double re = 0, im = 0;
            for (int j = 0; j < n; j++)
            {
                double mRe = M[(i * n + j) * 2];
                double mIm = M[(i * n + j) * 2 + 1];
                double pRe = psi[j * 2];
                double pIm = psi[j * 2 + 1];
                re += mRe * pRe - mIm * pIm;
                im += mRe * pIm + mIm * pRe;
            }
            result[i * 2] = re;
            result[i * 2 + 1] = im;
        }
        return result;
    }

    private static double ComputeHermiticityResidual(double[] M, int n)
    {
        double normSq = 0.0;
        double diffSq = 0.0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double mRe = M[(i * n + j) * 2];
                double mIm = M[(i * n + j) * 2 + 1];
                double dagRe = M[(j * n + i) * 2];
                double dagIm = -M[(j * n + i) * 2 + 1];
                double drRe = mRe - dagRe;
                double drIm = mIm - dagIm;
                diffSq += drRe * drRe + drIm * drIm;
                normSq += mRe * mRe + mIm * mIm;
            }
        }
        double norm = System.Math.Sqrt(normSq);
        double diff = System.Math.Sqrt(diffSq);
        return norm > 1e-14 ? diff / norm : diff;
    }

    private static int DominantDirection(double[] coords, int v, int w, int embDim, out double length)
    {
        int vBase = v * embDim;
        int wBase = w * embDim;
        double sumSq = 0;
        int dominant = 0;
        double maxAbs = 0;
        for (int d = 0; d < embDim; d++)
        {
            double delta = coords[wBase + d] - coords[vBase + d];
            sumSq += delta * delta;
            double abs = System.Math.Abs(delta);
            if (abs > maxAbs) { maxAbs = abs; dominant = d; }
        }
        length = System.Math.Sqrt(sumSq);
        return dominant;
    }
}
