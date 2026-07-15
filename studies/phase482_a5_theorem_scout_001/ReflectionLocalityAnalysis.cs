// Deterministic local certificate for the Phase482 reflection/locality obstruction.
//
// The committed periodic mesh is assembled by translating the 24 Kuhn simplices of
// one four-cube.  Consequently, closure (or failure of closure) under an axis
// reflection is decided exactly on the 16 bit-vector vertices of one cube.  No
// floating-point geometry or sampled field configuration enters this analysis.

internal static class ReflectionLocalityAnalysis
{
    private const int Dimension = 4;
    private const int TimeAxis = 0;
    private const int FullVertex = (1 << Dimension) - 1;

    internal static ReflectionLocalityResult Run()
    {
        var permutations = Permutations(Dimension).ToArray();
        var simplices = permutations.Select(KuhnSimplex).ToArray();
        var simplexKeys = simplices.Select(Key).ToHashSet(StringComparer.Ordinal);
        var reflectedSimplices = simplices
            .Select(simplex => simplex.Select(ReflectTime).Order().ToArray())
            .ToArray();

        int reflectedSimplexClosureCount = reflectedSimplices.Count(simplex => simplexKeys.Contains(Key(simplex)));
        bool reflectionInvolution = Enumerable.Range(0, 1 << Dimension)
            .All(vertex => ReflectTime(ReflectTime(vertex)) == vertex);
        bool reflectionVertexBijection = Enumerable.Range(0, 1 << Dimension)
            .Select(ReflectTime).Distinct().Count() == 1 << Dimension;

        var faces = simplices
            .SelectMany(TriangularFaces)
            .GroupBy(Key, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        var faceKeys = faces.Select(Key).ToHashSet(StringComparer.Ordinal);
        var reflectedFaces = faces
            .Select(face => face.Select(ReflectTime).Order().ToArray())
            .ToArray();
        int reflectedFaceClosureCount = reflectedFaces.Count(face => faceKeys.Contains(Key(face)));

        const int periodicExtent = 4;
        var periodicSimplices = PeriodicKuhnSimplices(periodicExtent, permutations).ToArray();
        var periodicSimplexKeys = periodicSimplices.Select(Key).ToHashSet(StringComparer.Ordinal);
        int siteReflectedPeriodicSimplexClosureCount = periodicSimplices.Count(simplex =>
            periodicSimplexKeys.Contains(Key(simplex.Select(vertex => ReflectPeriodic(vertex, periodicExtent, linkCentered: false)).ToArray())));
        int linkReflectedPeriodicSimplexClosureCount = periodicSimplices.Count(simplex =>
            periodicSimplexKeys.Contains(Key(simplex.Select(vertex => ReflectPeriodic(vertex, periodicExtent, linkCentered: true)).ToArray())));
        bool siteReflectionPeriodicInvolution = Enumerable.Range(0, Pow(periodicExtent, Dimension))
            .All(vertex => ReflectPeriodic(ReflectPeriodic(vertex, periodicExtent, linkCentered: false), periodicExtent, linkCentered: false) == vertex);
        bool linkReflectionPeriodicInvolution = Enumerable.Range(0, Pow(periodicExtent, Dimension))
            .All(vertex => ReflectPeriodic(ReflectPeriodic(vertex, periodicExtent, linkCentered: true), periodicExtent, linkCentered: true) == vertex);

        int[] witnessSimplex = KuhnSimplex(new[] { 0, 1, 2, 3 });
        int[] reflectedWitnessSimplex = witnessSimplex.Select(ReflectTime).Order().ToArray();
        int[] witnessFace = { 0, 1, 3 };
        int[] reflectedWitnessFace = witnessFace.Select(ReflectTime).Order().ToArray();

        bool witnessSimplexWasCommitted = simplexKeys.Contains(Key(witnessSimplex));
        bool reflectedWitnessSimplexIsCommitted = simplexKeys.Contains(Key(reflectedWitnessSimplex));
        bool witnessFaceWasCommitted = faceKeys.Contains(Key(witnessFace));
        bool reflectedWitnessFaceIsCommitted = faceKeys.Contains(Key(reflectedWitnessFace));

        // A 4-simplex has C(5,3)=10 triangular faces.  The committed operator gathers
        // this entire block before applying its per-cell face map.  The following
        // certificate is purely about that exact support hypergraph: it does not assume
        // that every member has a non-zero coefficient between every pair of faces.
        var representativeCellFaces = TriangularFaces(witnessSimplex).ToArray();
        var representativeCellEdges = representativeCellFaces
            .SelectMany(Edges)
            .Distinct()
            .ToArray();
        int[] overlapFaceA = { 0, 1, 3 };
        int[] overlapFaceB = { 0, 1, 7 };
        var sharedEdges = Edges(overlapFaceA).Intersect(Edges(overlapFaceB)).ToArray();

        bool primitiveFaceSupportExact = faces.All(face => face.Length == 3 && Edges(face).Count() == 3);
        bool singleSiteSupportRefuted = witnessFace.Distinct().Count() == 3;
        bool independentFaceVariableFactorizationRefuted = sharedEdges.Length > 0;
        bool standardReflectionProofGeometryGate =
            reflectionInvolution && reflectionVertexBijection &&
            reflectedSimplexClosureCount == simplices.Length &&
            reflectedFaceClosureCount == faces.Length &&
            (siteReflectedPeriodicSimplexClosureCount == periodicSimplices.Length ||
             linkReflectedPeriodicSimplexClosureCount == periodicSimplices.Length);
        bool inputsValid =
            permutations.Length == 24 && simplices.Length == 24 && faces.Length == 110 &&
            periodicSimplices.Length == 24 * Pow(periodicExtent, Dimension) &&
            periodicSimplexKeys.Count == periodicSimplices.Length &&
            reflectionInvolution && reflectionVertexBijection &&
            siteReflectionPeriodicInvolution && linkReflectionPeriodicInvolution &&
            witnessSimplexWasCommitted && !reflectedWitnessSimplexIsCommitted &&
            witnessFaceWasCommitted && !reflectedWitnessFaceIsCommitted &&
            primitiveFaceSupportExact && singleSiteSupportRefuted &&
            representativeCellFaces.Length == 10 && representativeCellEdges.Length == 10 &&
            independentFaceVariableFactorizationRefuted;
        bool reflectionObstructionSurvives = inputsValid && !standardReflectionProofGeometryGate;
        bool faceLocalityObstructionSurvives = inputsValid &&
            singleSiteSupportRefuted && independentFaceVariableFactorizationRefuted;

        return new ReflectionLocalityResult
        {
            AnalysisId = "phase482-kuhn-time-reflection-and-locality-v1",
            InputsValid = inputsValid,
            Dimension = Dimension,
            TimeAxis = TimeAxis,
            VertexCountPerCube = 1 << Dimension,
            KuhnSimplexCountPerCube = simplices.Length,
            TriangularFaceCountPerCube = faces.Length,
            ReflectionDefinition = "local translated axis-0 reflection x0 -> 1-x0 (bit 0 complement)",
            ReflectionIsExactInvolution = reflectionInvolution,
            ReflectionIsVertexBijection = reflectionVertexBijection,
            ReflectedSimplexClosureCount = reflectedSimplexClosureCount,
            ReflectedFaceClosureCount = reflectedFaceClosureCount,
            SimplicialReflectionAutomorphism = reflectedSimplexClosureCount == simplices.Length,
            FaceSetReflectionClosed = reflectedFaceClosureCount == faces.Length,
            PeriodicExtent = periodicExtent,
            PeriodicSimplexCount = periodicSimplices.Length,
            SiteReflectionPeriodicInvolution = siteReflectionPeriodicInvolution,
            LinkReflectionPeriodicInvolution = linkReflectionPeriodicInvolution,
            SiteReflectedPeriodicSimplexClosureCount = siteReflectedPeriodicSimplexClosureCount,
            LinkReflectedPeriodicSimplexClosureCount = linkReflectedPeriodicSimplexClosureCount,
            SiteReflectionPeriodicAutomorphism = siteReflectedPeriodicSimplexClosureCount == periodicSimplices.Length,
            LinkReflectionPeriodicAutomorphism = linkReflectedPeriodicSimplexClosureCount == periodicSimplices.Length,
            WitnessSimplex = FormatVertices(witnessSimplex),
            ReflectedWitnessSimplex = FormatVertices(reflectedWitnessSimplex),
            WitnessSimplexWasCommitted = witnessSimplexWasCommitted,
            ReflectedWitnessSimplexIsCommitted = reflectedWitnessSimplexIsCommitted,
            WitnessFace = FormatVertices(witnessFace),
            ReflectedWitnessFace = FormatVertices(reflectedWitnessFace),
            WitnessFaceWasCommitted = witnessFaceWasCommitted,
            ReflectedWitnessFaceIsCommitted = reflectedWitnessFaceIsCommitted,
            PrimitiveCurvatureSupportIsFaceLocal = primitiveFaceSupportExact,
            PrimitiveFaceVertexCount = witnessFace.Length,
            PrimitiveFaceEdgeCount = Edges(witnessFace).Count(),
            SingleSiteInteractionHypothesisRefuted = singleSiteSupportRefuted,
            RepresentativeCellFaceCount = representativeCellFaces.Length,
            RepresentativeCellEdgeCount = representativeCellEdges.Length,
            SharedEdgeFaceWitnessA = FormatVertices(overlapFaceA),
            SharedEdgeFaceWitnessB = FormatVertices(overlapFaceB),
            SharedEdgeWitness = sharedEdges.Select(FormatEdge).ToArray(),
            IndependentFaceVariableFactorizationRefuted = independentFaceVariableFactorizationRefuted,
            CellFaceMixingSupportExists = representativeCellFaces.Length == 10,
            StrictFaceFactorizationEstablished = false,
            StandardReflectionPositivityProofGeometryGate = standardReflectionProofGeometryGate,
            ReflectionPositivityEstablished = false,
            ReflectionPositivityRefuted = false,
            ReflectionObstructionSurvives = reflectionObstructionSurvives,
            FaceLocalityObstructionSurvives = faceLocalityObstructionSurvives,
            ProofGateCountImplemented = 10,
            ProofGateCountPassed = new[]
            {
                reflectionInvolution,
                reflectionVertexBijection,
                siteReflectionPeriodicInvolution,
                linkReflectionPeriodicInvolution,
                reflectedSimplexClosureCount == 0,
                reflectedFaceClosureCount < faces.Length,
                siteReflectedPeriodicSimplexClosureCount == 0,
                linkReflectedPeriodicSimplexClosureCount == 0,
                singleSiteSupportRefuted,
                independentFaceVariableFactorizationRefuted,
            }.Count(value => value),
            Finding = standardReflectionProofGeometryGate
                ? "The local simplicial geometry is reflection closed; a separate coefficient-positivity factorization would still be required."
                : "The committed Kuhn simplices and triangular faces are not closed under the axis-0 reflection. This blocks the standard geometry-preserving reflection-positivity factorization; it neither establishes nor refutes reflection positivity by another construction.",
            LocalityFinding = independentFaceVariableFactorizationRefuted
                ? "Curvature is supported on triangular faces, but a face uses three vertices and three edges, distinct faces share edge variables, and the committed per-cell operator admits a ten-face gather. A single-site or independent-face product-measure hypothesis therefore does not type-check."
                : "The exact support witness was not obtained; locality remains unresolved.",
            OverallPhase482Recommendation = inputsValid && reflectionObstructionSurvives && faceLocalityObstructionSurvives
                ? "obstructions-survive-no-theorem"
                : "invalid-proof-package",
        };
    }

    private static int[] KuhnSimplex(int[] permutation)
    {
        var simplex = new int[Dimension + 1];
        int vertex = 0;
        simplex[0] = vertex;
        for (int i = 0; i < permutation.Length; i++)
        {
            vertex |= 1 << permutation[i];
            simplex[i + 1] = vertex;
        }

        if (simplex[^1] != FullVertex)
            throw new InvalidOperationException("Kuhn chain did not terminate at the opposite cube vertex.");
        return simplex;
    }

    private static int ReflectTime(int vertex) => vertex ^ (1 << TimeAxis);

    private static IEnumerable<int[]> PeriodicKuhnSimplices(int extent, IEnumerable<int[]> permutations)
    {
        int vertexCount = Pow(extent, Dimension);
        foreach (int baseVertex in Enumerable.Range(0, vertexCount))
        {
            int[] baseCoordinates = Coordinates(baseVertex, extent);
            foreach (int[] permutation in permutations)
            {
                var coordinates = (int[])baseCoordinates.Clone();
                var simplex = new int[Dimension + 1];
                simplex[0] = VertexId(coordinates, extent);
                for (int i = 0; i < permutation.Length; i++)
                {
                    int axis = permutation[i];
                    coordinates[axis] = (coordinates[axis] + 1) % extent;
                    simplex[i + 1] = VertexId(coordinates, extent);
                }
                yield return simplex;
            }
        }
    }

    private static int ReflectPeriodic(int vertex, int extent, bool linkCentered)
    {
        int[] coordinates = Coordinates(vertex, extent);
        coordinates[TimeAxis] = Mod((linkCentered ? 1 : 0) - coordinates[TimeAxis], extent);
        return VertexId(coordinates, extent);
    }

    private static int[] Coordinates(int vertex, int extent)
    {
        var coordinates = new int[Dimension];
        for (int axis = Dimension - 1; axis >= 0; axis--)
        {
            coordinates[axis] = vertex % extent;
            vertex /= extent;
        }
        return coordinates;
    }

    private static int VertexId(int[] coordinates, int extent)
    {
        int vertex = 0;
        for (int axis = 0; axis < Dimension; axis++) vertex = vertex * extent + coordinates[axis];
        return vertex;
    }

    private static int Pow(int value, int exponent)
    {
        int result = 1;
        for (int i = 0; i < exponent; i++) result *= value;
        return result;
    }

    private static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;

    private static IEnumerable<int[]> TriangularFaces(int[] simplex)
    {
        for (int i = 0; i < simplex.Length; i++)
            for (int j = i + 1; j < simplex.Length; j++)
                for (int k = j + 1; k < simplex.Length; k++)
                    yield return new[] { simplex[i], simplex[j], simplex[k] }.Order().ToArray();
    }

    private static IEnumerable<(int A, int B)> Edges(int[] face)
    {
        for (int i = 0; i < face.Length; i++)
            for (int j = i + 1; j < face.Length; j++)
                yield return (System.Math.Min(face[i], face[j]), System.Math.Max(face[i], face[j]));
    }

    private static IEnumerable<int[]> Permutations(int count)
    {
        var values = Enumerable.Range(0, count).ToArray();
        return Generate(0);

        IEnumerable<int[]> Generate(int start)
        {
            if (start == values.Length)
            {
                yield return (int[])values.Clone();
                yield break;
            }

            for (int i = start; i < values.Length; i++)
            {
                (values[start], values[i]) = (values[i], values[start]);
                foreach (var permutation in Generate(start + 1)) yield return permutation;
                (values[start], values[i]) = (values[i], values[start]);
            }
        }
    }

    private static string Key(int[] vertices) => string.Join(",", vertices.Order());

    private static string[] FormatVertices(int[] vertices) => vertices.Select(FormatVertex).ToArray();

    private static string FormatVertex(int vertex) => Convert.ToString(vertex, 2).PadLeft(Dimension, '0');

    private static string FormatEdge((int A, int B) edge) => $"{FormatVertex(edge.A)}--{FormatVertex(edge.B)}";
}

internal sealed class ReflectionLocalityResult
{
    public required string AnalysisId { get; init; }
    public required bool InputsValid { get; init; }
    public required int Dimension { get; init; }
    public required int TimeAxis { get; init; }
    public required int VertexCountPerCube { get; init; }
    public required int KuhnSimplexCountPerCube { get; init; }
    public required int TriangularFaceCountPerCube { get; init; }
    public required string ReflectionDefinition { get; init; }
    public required bool ReflectionIsExactInvolution { get; init; }
    public required bool ReflectionIsVertexBijection { get; init; }
    public required int ReflectedSimplexClosureCount { get; init; }
    public required int ReflectedFaceClosureCount { get; init; }
    public required bool SimplicialReflectionAutomorphism { get; init; }
    public required bool FaceSetReflectionClosed { get; init; }
    public required int PeriodicExtent { get; init; }
    public required int PeriodicSimplexCount { get; init; }
    public required bool SiteReflectionPeriodicInvolution { get; init; }
    public required bool LinkReflectionPeriodicInvolution { get; init; }
    public required int SiteReflectedPeriodicSimplexClosureCount { get; init; }
    public required int LinkReflectedPeriodicSimplexClosureCount { get; init; }
    public required bool SiteReflectionPeriodicAutomorphism { get; init; }
    public required bool LinkReflectionPeriodicAutomorphism { get; init; }
    public required string[] WitnessSimplex { get; init; }
    public required string[] ReflectedWitnessSimplex { get; init; }
    public required bool WitnessSimplexWasCommitted { get; init; }
    public required bool ReflectedWitnessSimplexIsCommitted { get; init; }
    public required string[] WitnessFace { get; init; }
    public required string[] ReflectedWitnessFace { get; init; }
    public required bool WitnessFaceWasCommitted { get; init; }
    public required bool ReflectedWitnessFaceIsCommitted { get; init; }
    public required bool PrimitiveCurvatureSupportIsFaceLocal { get; init; }
    public required int PrimitiveFaceVertexCount { get; init; }
    public required int PrimitiveFaceEdgeCount { get; init; }
    public required bool SingleSiteInteractionHypothesisRefuted { get; init; }
    public required int RepresentativeCellFaceCount { get; init; }
    public required int RepresentativeCellEdgeCount { get; init; }
    public required string[] SharedEdgeFaceWitnessA { get; init; }
    public required string[] SharedEdgeFaceWitnessB { get; init; }
    public required string[] SharedEdgeWitness { get; init; }
    public required bool IndependentFaceVariableFactorizationRefuted { get; init; }
    public required bool CellFaceMixingSupportExists { get; init; }
    public required bool StrictFaceFactorizationEstablished { get; init; }
    public required bool StandardReflectionPositivityProofGeometryGate { get; init; }
    public required bool ReflectionPositivityEstablished { get; init; }
    public required bool ReflectionPositivityRefuted { get; init; }
    public required bool ReflectionObstructionSurvives { get; init; }
    public required bool FaceLocalityObstructionSurvives { get; init; }
    public required int ProofGateCountImplemented { get; init; }
    public required int ProofGateCountPassed { get; init; }
    public required string Finding { get; init; }
    public required string LocalityFinding { get; init; }
    public required string OverallPhase482Recommendation { get; init; }
}
