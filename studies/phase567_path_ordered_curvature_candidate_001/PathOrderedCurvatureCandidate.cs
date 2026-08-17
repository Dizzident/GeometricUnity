using Gu.Geometry;
using Gu.Math;

namespace Phase567;

/// <summary>
/// Study-local second-order BCH curvature on the composable face path
/// v0 -> v1 -> v2 -> v0. This is a workbench candidate, not a registered
/// replacement for <c>Gu.ReferenceCpu.CurvatureAssembler</c>.
/// </summary>
public static class PathOrderedCurvatureCandidate
{
    private static readonly int[] ComposableBoundaryPositions = [0, 2, 1];

    public const string CandidateId = "a35-path-ordered-bch2-v1";

    public static double[] Assemble(SimplicialMesh mesh, LieAlgebra algebra, double[] omega)
    {
        ValidateConnection(mesh, algebra, omega, nameof(omega));
        int dim = algebra.Dimension;
        var result = new double[mesh.FaceCount * dim];

        for (int face = 0; face < mesh.FaceCount; face++)
        {
            double[][] q = DirectedPathValues(mesh, algebra, omega, face);
            for (int a = 0; a < dim; a++)
                result[face * dim + a] = q[0][a] + q[1][a] + q[2][a];

            for (int i = 0; i < q.Length; i++)
                for (int j = i + 1; j < q.Length; j++)
                {
                    double[] bracket = algebra.Bracket(q[i], q[j]);
                    for (int a = 0; a < dim; a++)
                        result[face * dim + a] += 0.5 * bracket[a];
                }
        }

        return result;
    }

    public static double[] Linearize(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        double[] deltaOmega)
    {
        ValidateConnection(mesh, algebra, omega, nameof(omega));
        ValidateConnection(mesh, algebra, deltaOmega, nameof(deltaOmega));
        int dim = algebra.Dimension;
        var result = new double[mesh.FaceCount * dim];

        for (int face = 0; face < mesh.FaceCount; face++)
        {
            double[][] q = DirectedPathValues(mesh, algebra, omega, face);
            double[][] dq = DirectedPathValues(mesh, algebra, deltaOmega, face);
            for (int a = 0; a < dim; a++)
                result[face * dim + a] = dq[0][a] + dq[1][a] + dq[2][a];

            for (int i = 0; i < q.Length; i++)
                for (int j = i + 1; j < q.Length; j++)
                {
                    double[] left = algebra.Bracket(dq[i], q[j]);
                    double[] right = algebra.Bracket(q[i], dq[j]);
                    for (int a = 0; a < dim; a++)
                        result[face * dim + a] += 0.5 * (left[a] + right[a]);
                }
        }

        return result;
    }

    public static double[] LinearizeTranspose(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] omega,
        double[] faceCovector)
    {
        ValidateConnection(mesh, algebra, omega, nameof(omega));
        int dim = algebra.Dimension;
        if (faceCovector.Length != mesh.FaceCount * dim)
            throw new ArgumentException(
                $"Expected {mesh.FaceCount * dim} face coefficients, got {faceCovector.Length}.",
                nameof(faceCovector));

        var result = new double[mesh.EdgeCount * dim];
        for (int face = 0; face < mesh.FaceCount; face++)
        {
            double[][] q = DirectedPathValues(mesh, algebra, omega, face);
            var w = new double[dim];
            Array.Copy(faceCovector, face * dim, w, 0, dim);

            for (int i = 0; i < q.Length; i++)
                ScatterDirected(mesh, face, i, w, result, 1.0, dim);

            for (int i = 0; i < q.Length; i++)
                for (int j = i + 1; j < q.Length; j++)
                {
                    double[] toI = AdTransposeApply(algebra, q[j], w);
                    double[] toJ = AdTransposeApply(algebra, q[i], w);
                    ScatterDirected(mesh, face, i, toI, result, -0.5, dim);
                    ScatterDirected(mesh, face, j, toJ, result, 0.5, dim);
                }
        }

        return result;
    }

    private static double[][] DirectedPathValues(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] source,
        int face)
    {
        int dim = algebra.Dimension;
        var values = new double[ComposableBoundaryPositions.Length][];
        for (int pathPosition = 0; pathPosition < ComposableBoundaryPositions.Length; pathPosition++)
        {
            int boundaryPosition = ComposableBoundaryPositions[pathPosition];
            int edge = mesh.FaceBoundaryEdges[face][boundaryPosition];
            int sign = mesh.FaceBoundaryOrientations[face][boundaryPosition];
            var value = new double[dim];
            for (int a = 0; a < dim; a++)
                value[a] = sign * source[edge * dim + a];
            values[pathPosition] = value;
        }

        return values;
    }

    private static void ScatterDirected(
        SimplicialMesh mesh,
        int face,
        int pathPosition,
        double[] value,
        double[] target,
        double factor,
        int dim)
    {
        int boundaryPosition = ComposableBoundaryPositions[pathPosition];
        int edge = mesh.FaceBoundaryEdges[face][boundaryPosition];
        int sign = mesh.FaceBoundaryOrientations[face][boundaryPosition];
        for (int a = 0; a < dim; a++)
            target[edge * dim + a] += factor * sign * value[a];
    }

    private static double[] AdTransposeApply(LieAlgebra algebra, double[] x, double[] w)
    {
        int dim = algebra.Dimension;
        var result = new double[dim];
        for (int b = 0; b < dim; b++)
            for (int a = 0; a < dim; a++)
                for (int c = 0; c < dim; c++)
                    result[b] += algebra.GetStructureConstant(a, b, c) * x[a] * w[c];
        return result;
    }

    private static void ValidateConnection(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        double[] coefficients,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(algebra);
        ArgumentNullException.ThrowIfNull(coefficients);
        if (coefficients.Length != mesh.EdgeCount * algebra.Dimension)
            throw new ArgumentException(
                $"Expected {mesh.EdgeCount * algebra.Dimension} edge coefficients, got {coefficients.Length}.",
                parameterName);
    }
}
