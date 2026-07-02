namespace Gu.Geometry;

/// <summary>
/// Pure topology-driven discrete exterior derivative (simplicial coboundary).
/// Operates on flat coefficient arrays with an explicit componentsPerCarrier
/// (the ad-valued layout [carrierIdx * componentsPerCarrier + component]).
/// The boundary orientation signs precomputed by <see cref="MeshTopologyBuilder"/>
/// guarantee d∘d = 0 combinatorially.
/// </summary>
public static class DiscreteExteriorDerivative
{
    /// <summary>
    /// (d omega)[face] = Σ_i FaceBoundaryOrientations[face][i] * omega[edge_i].
    /// The 1-form → 2-form exterior derivative. Mirrors the linear (d) part of
    /// the curvature assembly; provided here for completeness of the cochain ladder.
    /// </summary>
    public static double[] EdgeToFace(SimplicialMesh mesh, double[] edgeCoeffs, int componentsPerCarrier)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(edgeCoeffs);
        if (componentsPerCarrier < 1)
            throw new ArgumentOutOfRangeException(nameof(componentsPerCarrier), "Must be >= 1.");
        if (edgeCoeffs.Length != mesh.EdgeCount * componentsPerCarrier)
            throw new ArgumentException(
                $"Edge coefficient length {edgeCoeffs.Length} does not match EdgeCount * componentsPerCarrier = {mesh.EdgeCount * componentsPerCarrier}.",
                nameof(edgeCoeffs));

        var result = new double[mesh.FaceCount * componentsPerCarrier];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            int[] boundaryEdges = mesh.FaceBoundaryEdges[f];
            int[] orientations = mesh.FaceBoundaryOrientations[f];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edge = boundaryEdges[i];
                double sign = orientations[i];
                int edgeBase = edge * componentsPerCarrier;
                int faceBase = f * componentsPerCarrier;
                for (int c = 0; c < componentsPerCarrier; c++)
                    result[faceBase + c] += sign * edgeCoeffs[edgeBase + c];
            }
        }
        return result;
    }

    /// <summary>
    /// (d alpha)[volume] = Σ_i VolumeBoundaryOrientations[volume][i] * alpha[face_i].
    /// The 2-form → 3-form exterior derivative on a 4D mesh.
    /// </summary>
    public static ThreeFormField FaceToVolume(SimplicialMesh mesh, double[] faceCoeffs, int componentsPerCarrier)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(faceCoeffs);
        if (componentsPerCarrier < 1)
            throw new ArgumentOutOfRangeException(nameof(componentsPerCarrier), "Must be >= 1.");
        if (faceCoeffs.Length != mesh.FaceCount * componentsPerCarrier)
            throw new ArgumentException(
                $"Face coefficient length {faceCoeffs.Length} does not match FaceCount * componentsPerCarrier = {mesh.FaceCount * componentsPerCarrier}.",
                nameof(faceCoeffs));

        var result = new double[mesh.VolumeCount * componentsPerCarrier];
        for (int v = 0; v < mesh.VolumeCount; v++)
        {
            int[] boundaryFaces = mesh.VolumeBoundaryFaces[v];
            int[] orientations = mesh.VolumeBoundaryOrientations[v];
            for (int i = 0; i < boundaryFaces.Length; i++)
            {
                int face = boundaryFaces[i];
                double sign = orientations[i];
                int faceBase = face * componentsPerCarrier;
                int volBase = v * componentsPerCarrier;
                for (int c = 0; c < componentsPerCarrier; c++)
                    result[volBase + c] += sign * faceCoeffs[faceBase + c];
            }
        }
        return new ThreeFormField(mesh, componentsPerCarrier, result);
    }
}
