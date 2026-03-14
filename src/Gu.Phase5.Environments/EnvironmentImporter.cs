using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;

namespace Gu.Phase5.Environments;

/// <summary>
/// Imports external geometry data into an EnvironmentRecord.
///
/// Supported formats:
///   "gu-json"         -- native GU JSON (serialized SimplicialMesh)
///   "simplicial-json" -- minimal mesh format: { vertexCoordinates, edges, faces, embeddingDimension }
///
/// Validates admissibility before accepting. Throws if admissibility fails.
/// </summary>
public static class EnvironmentImporter
{
    public static EnvironmentRecord Import(EnvironmentImportSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        if (!File.Exists(spec.SourcePath))
            throw new FileNotFoundException($"Source path not found: {spec.SourcePath}", spec.SourcePath);

        SimplicialMesh mesh = spec.SourceFormat switch
        {
            "gu-json" => LoadGuJson(spec.SourcePath),
            "simplicial-json" => LoadSimplicialJson(spec.SourcePath),
            _ => throw new NotSupportedException(
                $"Source format '{spec.SourceFormat}' is not supported. " +
                $"Supported formats: gu-json, simplicial-json."),
        };

        int baseDim = mesh.EmbeddingDimension;
        int ambientDim = mesh.EmbeddingDimension;
        double[] volumes = ComputeFaceAreas(mesh);

        var admissibility = EnvironmentAdmissibilityChecker.Check(
            baseDim: baseDim, ambientDim: ambientDim,
            edgeCount: mesh.EdgeCount, faceCount: mesh.FaceCount,
            volumes: volumes);

        if (!admissibility.Passed)
            throw new InvalidOperationException(
                $"Imported geometry failed admissibility checks: {admissibility.Notes}");

        string fingerprint = ComputeMeshFingerprint(mesh);

        return new EnvironmentRecord
        {
            EnvironmentId = spec.EnvironmentId,
            GeometryTier = spec.GeometryTier,
            GeometryFingerprint = fingerprint,
            BaseDimension = baseDim,
            AmbientDimension = ambientDim,
            EdgeCount = mesh.EdgeCount,
            FaceCount = mesh.FaceCount,
            Admissibility = admissibility,
            SourceSpec = spec.SourcePath,
            Provenance = spec.Provenance,
        };
    }

    private static SimplicialMesh LoadGuJson(string path)
    {
        string json = File.ReadAllText(path);
        return GuJsonDefaults.Deserialize<SimplicialMesh>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize SimplicialMesh from {path}.");
    }

    private static SimplicialMesh LoadSimplicialJson(string path)
    {
        string json = File.ReadAllText(path);
        // Deserialize as the native type — "simplicial-json" uses the same schema as gu-json
        // for forward compatibility.
        return GuJsonDefaults.Deserialize<SimplicialMesh>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize SimplicialMesh from {path}.");
    }

    private static double[] ComputeFaceAreas(SimplicialMesh mesh)
    {
        var areas = new double[mesh.FaceCount];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            var face = mesh.Faces[f];
            if (face.Length < 3) continue;
            var v0 = mesh.GetVertexCoordinates(face[0]);
            var v1 = mesh.GetVertexCoordinates(face[1]);
            var v2 = mesh.GetVertexCoordinates(face[2]);
            double ax = v1[0] - v0[0], ay = v1[1] - v0[1];
            double bx = v2[0] - v0[0], by = v2[1] - v0[1];
            areas[f] = System.Math.Abs(ax * by - ay * bx) * 0.5;
        }
        return areas;
    }

    private static string ComputeMeshFingerprint(SimplicialMesh mesh)
    {
        double coordSum = 0.0;
        int nCoords = System.Math.Min(mesh.VertexCoordinates.Length, 32);
        for (int i = 0; i < nCoords; i++)
            coordSum += mesh.VertexCoordinates[i];
        string content = $"v={mesh.VertexCount},e={mesh.EdgeCount},f={mesh.FaceCount},cs={coordSum:G8}";
        return IntegrityHasher.ComputeHash(content);
    }
}
