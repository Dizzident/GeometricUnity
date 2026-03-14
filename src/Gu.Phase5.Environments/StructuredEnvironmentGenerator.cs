using Gu.Artifacts;
using Gu.Core;
using Gu.Geometry;

namespace Gu.Phase5.Environments;

/// <summary>
/// Generates structured analytic environments beyond ToyGeometryFactory.
///
/// Supported generators:
///   "refined-toy-2d" -- Toy2D with configurable refinement level (parameter: "refinement", integer)
///   "structured-3d"  -- 3D simplicial complex with configurable parameters (parameter: "n", integer)
///   "flat-torus-2d"  -- 2D flat torus approximated by uniform 2D grid (parameter: "size", integer)
/// </summary>
public static class StructuredEnvironmentGenerator
{
    private const string SchemaVersion = "1.0";

    public static EnvironmentRecord Generate(StructuredEnvironmentSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        return spec.GeneratorId switch
        {
            "refined-toy-2d" => GenerateRefinedToy2D(spec),
            "structured-3d"  => GenerateStructured3D(spec),
            "flat-torus-2d"  => GenerateFlatTorus2D(spec),
            _ => throw new NotSupportedException(
                $"Generator '{spec.GeneratorId}' is not supported. " +
                $"Supported generators: refined-toy-2d, structured-3d, flat-torus-2d."),
        };
    }

    private static EnvironmentRecord GenerateRefinedToy2D(StructuredEnvironmentSpec spec)
    {
        int refinement = GetIntParameter(spec, "refinement", defaultValue: 2);
        if (refinement < 1)
            throw new ArgumentException("Parameter 'refinement' must be >= 1.");

        var mesh = SimplicialMeshGenerator.CreateUniform2D(rows: refinement, cols: refinement);
        var volumes = ComputeTriangleAreas(mesh);
        var admissibility = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2,
            edgeCount: mesh.EdgeCount, faceCount: mesh.FaceCount,
            volumes: volumes);

        string fingerprint = ComputeMeshFingerprint(mesh);

        return new EnvironmentRecord
        {
            EnvironmentId = spec.EnvironmentId,
            GeometryTier = "structured",
            GeometryFingerprint = fingerprint,
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = mesh.EdgeCount,
            FaceCount = mesh.FaceCount,
            Admissibility = admissibility,
            SourceSpec = spec.EnvironmentId,
            Provenance = spec.Provenance,
        };
    }

    private static EnvironmentRecord GenerateStructured3D(StructuredEnvironmentSpec spec)
    {
        int n = GetIntParameter(spec, "n", defaultValue: 2);
        if (n < 1)
            throw new ArgumentException("Parameter 'n' must be >= 1.");

        var mesh = SimplicialMeshGenerator.CreateUniform3D(n);
        var volumes = ComputeTetrahedronVolumes(mesh);
        var admissibility = EnvironmentAdmissibilityChecker.Check(
            baseDim: 3, ambientDim: 3,
            edgeCount: mesh.EdgeCount, faceCount: mesh.FaceCount,
            volumes: volumes);

        string fingerprint = ComputeMeshFingerprint(mesh);

        return new EnvironmentRecord
        {
            EnvironmentId = spec.EnvironmentId,
            GeometryTier = "structured",
            GeometryFingerprint = fingerprint,
            BaseDimension = 3,
            AmbientDimension = 3,
            EdgeCount = mesh.EdgeCount,
            FaceCount = mesh.FaceCount,
            Admissibility = admissibility,
            SourceSpec = spec.EnvironmentId,
            Provenance = spec.Provenance,
        };
    }

    private static EnvironmentRecord GenerateFlatTorus2D(StructuredEnvironmentSpec spec)
    {
        int size = GetIntParameter(spec, "size", defaultValue: 3);
        if (size < 2)
            throw new ArgumentException("Parameter 'size' must be >= 2.");

        // Flat torus approximated by uniform 2D grid (periodic BCs not enforced at mesh level)
        var mesh = SimplicialMeshGenerator.CreateUniform2D(rows: size, cols: size);
        var volumes = ComputeTriangleAreas(mesh);
        var admissibility = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2,
            edgeCount: mesh.EdgeCount, faceCount: mesh.FaceCount,
            volumes: volumes);

        string fingerprint = ComputeMeshFingerprint(mesh);

        return new EnvironmentRecord
        {
            EnvironmentId = spec.EnvironmentId,
            GeometryTier = "structured",
            GeometryFingerprint = fingerprint,
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = mesh.EdgeCount,
            FaceCount = mesh.FaceCount,
            Admissibility = admissibility,
            SourceSpec = spec.EnvironmentId,
            Provenance = spec.Provenance,
        };
    }

    private static int GetIntParameter(StructuredEnvironmentSpec spec, string key, int defaultValue)
    {
        if (spec.Parameters.TryGetValue(key, out double val))
            return (int)System.Math.Round(val);
        return defaultValue;
    }

    private static double[] ComputeTriangleAreas(SimplicialMesh mesh)
    {
        var areas = new double[mesh.FaceCount];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            var face = mesh.Faces[f];
            if (face.Length < 3) continue;
            var v0 = mesh.GetVertexCoordinates(face[0]);
            var v1 = mesh.GetVertexCoordinates(face[1]);
            var v2 = mesh.GetVertexCoordinates(face[2]);
            // Area = 0.5 * ||(v1-v0) x (v2-v0)||
            double ax = v1[0] - v0[0], ay = v1[1] - v0[1];
            double bx = v2[0] - v0[0], by = v2[1] - v0[1];
            areas[f] = System.Math.Abs(ax * by - ay * bx) * 0.5;
        }
        return areas;
    }

    private static double[] ComputeTetrahedronVolumes(SimplicialMesh mesh)
    {
        var volumes = new double[mesh.FaceCount];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            var face = mesh.Faces[f];
            if (face.Length < 4) continue;
            var v0 = mesh.GetVertexCoordinates(face[0]);
            var v1 = mesh.GetVertexCoordinates(face[1]);
            var v2 = mesh.GetVertexCoordinates(face[2]);
            var v3 = mesh.GetVertexCoordinates(face[3]);
            // Vol = (1/6) |det[v1-v0, v2-v0, v3-v0]|
            double ax = v1[0] - v0[0], ay = v1[1] - v0[1], az = v1[2] - v0[2];
            double bx = v2[0] - v0[0], by = v2[1] - v0[1], bz = v2[2] - v0[2];
            double cx = v3[0] - v0[0], cy = v3[1] - v0[1], cz = v3[2] - v0[2];
            double det = ax * (by * cz - bz * cy)
                       - ay * (bx * cz - bz * cx)
                       + az * (bx * cy - by * cx);
            volumes[f] = System.Math.Abs(det) / 6.0;
        }
        return volumes;
    }

    private static string ComputeMeshFingerprint(SimplicialMesh mesh)
    {
        // Fingerprint based on vertex count, edge count, face count, and first coordinate sum
        double coordSum = 0.0;
        int nCoords = System.Math.Min(mesh.VertexCoordinates.Length, 32);
        for (int i = 0; i < nCoords; i++)
            coordSum += mesh.VertexCoordinates[i];
        string content = $"v={mesh.VertexCount},e={mesh.EdgeCount},f={mesh.FaceCount},cs={coordSum:G8}";
        return IntegrityHasher.ComputeHash(content);
    }
}
