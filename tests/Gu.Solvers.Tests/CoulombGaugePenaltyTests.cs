using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.Solvers.Tests;

public class CoulombGaugePenaltyTests
{
    /// <summary>
    /// Build a simple triangle mesh: 3 vertices, 1 triangle, 3 edges.
    ///
    /// Vertices: v0=(0,0), v1=(1,0), v2=(0,1)
    /// Edges: e0={0,1}, e1={0,2}, e2={1,2}
    /// Faces: f0={0,1,2}
    /// </summary>
    private static SimplicialMesh CreateTriangleMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: [0, 0, 1, 0, 0, 1],
            vertexCount: 3,
            cellVertices: [new[] { 0, 1, 2 }]);
    }

    /// <summary>
    /// Build a mesh with two triangles sharing an edge: 4 vertices, 2 triangles, 5 edges.
    ///
    /// Vertices: v0=(0,0), v1=(1,0), v2=(0,1), v3=(1,1)
    /// Cells: {0,1,2}, {1,2,3}
    /// </summary>
    private static SimplicialMesh CreateTwoTriangleMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: [0, 0, 1, 0, 0, 1, 1, 1],
            vertexCount: 4,
            cellVertices: [new[] { 0, 1, 2 }, new[] { 1, 2, 3 }]);
    }

    private static FieldTensor MakeEdgeField(int edgeCount, int dimG, double[] coefficients)
    {
        return new FieldTensor
        {
            Label = "test_omega",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "test",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "test",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coefficients,
            Shape = [edgeCount, dimG],
        };
    }

    private static FieldTensor MakeZeroEdgeField(int edgeCount, int dimG)
    {
        return MakeEdgeField(edgeCount, dimG, new double[edgeCount * dimG]);
    }

    [Fact]
    public void ZeroOmega_ZeroViolation()
    {
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);
        var omega = MakeZeroEdgeField(mesh.EdgeCount, 1);

        Assert.Equal(0.0, penalty.EvaluateObjective(omega));
        Assert.Equal(0.0, penalty.ComputeViolationNorm(omega));

        var grad = penalty.EvaluateGradient(omega);
        Assert.All(grad.Coefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void LambdaZero_DisablesPenalty()
    {
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 0.0);
        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);

        Assert.Equal(0.0, penalty.EvaluateObjective(omega));

        var grad = penalty.EvaluateGradient(omega);
        Assert.All(grad.Coefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void NegativeLambda_Throws()
    {
        var mesh = CreateTriangleMesh();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CoulombGaugePenalty(mesh, dimG: 1, lambda: -1.0));
    }

    [Fact]
    public void InvalidDimG_Throws()
    {
        var mesh = CreateTriangleMesh();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CoulombGaugePenalty(mesh, dimG: 0, lambda: 1.0));
    }

    [Fact]
    public void NullMesh_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CoulombGaugePenalty(null!, dimG: 1, lambda: 1.0));
    }

    [Fact]
    public void OmegaRefLengthMismatch_Throws()
    {
        var mesh = CreateTriangleMesh();
        var badRef = MakeEdgeField(1, 1, [1.0]); // wrong length
        Assert.Throws<ArgumentException>(() =>
            new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0, omegaRef: badRef));
    }

    [Fact]
    public void CodifferentialOnTriangle_ScalarCase()
    {
        // Triangle: 3 vertices, 3 edges
        // e0={0,1}, e1={0,2}, e2={1,2}
        // Vertex 0: incident to e0(sign +1), e1(sign +1)
        // Vertex 1: incident to e0(sign -1), e2(sign +1)
        // Vertex 2: incident to e1(sign -1), e2(sign -1)
        //
        // omega = [1, 2, 3] on edges e0, e1, e2
        // d^* is the transpose of d: d[e={v0,v1}] = phi[v1] - phi[v0]
        // so d^T[v,e] = -sign(v,e) where sign is the VertexEdgeOrientation.
        // d^*(omega)[v0] = -1*omega[e0] + (-1)*omega[e1] = -1 - 2 = -3
        // d^*(omega)[v1] = +1*omega[e0] + (-1)*omega[e2] = 1 - 3 = -2
        // d^*(omega)[v2] = +1*omega[e1] + (+1)*omega[e2] = 2 + 3 = 5
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);

        double[] edgeField = [1.0, 2.0, 3.0];
        double[] dStar = penalty.ApplyCodifferential(edgeField);

        Assert.Equal(3, dStar.Length);
        Assert.Equal(-3.0, dStar[0], 1e-12);
        Assert.Equal(-2.0, dStar[1], 1e-12);
        Assert.Equal(5.0, dStar[2], 1e-12);
    }

    [Fact]
    public void ExteriorDerivativeOnTriangle_ScalarCase()
    {
        // Triangle: edges e0={0,1}, e1={0,2}, e2={1,2}
        // phi = [1, 3, 7] on vertices
        // d(phi)[e0] = phi[1] - phi[0] = 3 - 1 = 2
        // d(phi)[e1] = phi[2] - phi[0] = 7 - 1 = 6
        // d(phi)[e2] = phi[2] - phi[1] = 7 - 3 = 4
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);

        double[] vertexField = [1.0, 3.0, 7.0];
        double[] dPhi = penalty.ApplyExteriorDerivative(vertexField);

        Assert.Equal(3, dPhi.Length);
        Assert.Equal(2.0, dPhi[0], 1e-12);
        Assert.Equal(6.0, dPhi[1], 1e-12);
        Assert.Equal(4.0, dPhi[2], 1e-12);
    }

    [Fact]
    public void GraphLaplacian_IdentityCheck()
    {
        // d(d^*(omega)) should be the graph Laplacian on 1-forms.
        // For the triangle with omega = [1, 2, 3]:
        // d^* omega = [-3, -2, 5] (from the codifferential test above)
        // d(d^* omega)[e0={0,1}] = d^*[1] - d^*[0] = -2 - (-3) = 1
        // d(d^* omega)[e1={0,2}] = d^*[2] - d^*[0] = 5 - (-3) = 8
        // d(d^* omega)[e2={1,2}] = d^*[2] - d^*[1] = 5 - (-2) = 7
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);

        double[] omega = [1.0, 2.0, 3.0];
        double[] dStar = penalty.ApplyCodifferential(omega);
        double[] ddStar = penalty.ApplyExteriorDerivative(dStar);

        Assert.Equal(3, ddStar.Length);
        Assert.Equal(1.0, ddStar[0], 1e-12);
        Assert.Equal(8.0, ddStar[1], 1e-12);
        Assert.Equal(7.0, ddStar[2], 1e-12);
    }

    [Fact]
    public void Objective_MatchesManualComputation()
    {
        var mesh = CreateTriangleMesh();
        double lambda = 2.5;
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: lambda);

        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);

        // d^* omega = [3, 2, -5]
        // ||d^* omega||^2 = 9 + 4 + 25 = 38
        // objective = (lambda/2) * 38 = 1.25 * 38 = 47.5
        double expected = 0.5 * lambda * (9 + 4 + 25);
        Assert.Equal(expected, penalty.EvaluateObjective(omega), 1e-12);
    }

    [Fact]
    public void Gradient_MatchesManualComputation()
    {
        var mesh = CreateTriangleMesh();
        double lambda = 2.5;
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: lambda);

        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);

        // gradient = lambda * d(d^* omega) = 2.5 * [1, 8, 7]
        var grad = penalty.EvaluateGradient(omega);

        Assert.Equal(3, grad.Coefficients.Length);
        Assert.Equal(2.5 * 1.0, grad.Coefficients[0], 1e-12);
        Assert.Equal(2.5 * 8.0, grad.Coefficients[1], 1e-12);
        Assert.Equal(2.5 * 7.0, grad.Coefficients[2], 1e-12);
    }

    [Fact]
    public void FiniteDifference_GradientVerification()
    {
        // Verify that the gradient matches finite differences of the objective.
        var mesh = CreateTwoTriangleMesh();
        double lambda = 1.5;
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: lambda);

        double[] coeffs = [0.3, -0.7, 1.2, 0.5, -0.9];
        var omega = MakeEdgeField(mesh.EdgeCount, 1, coeffs);

        var grad = penalty.EvaluateGradient(omega);
        double obj0 = penalty.EvaluateObjective(omega);

        double eps = 1e-7;
        for (int i = 0; i < coeffs.Length; i++)
        {
            var perturbedCoeffs = (double[])coeffs.Clone();
            perturbedCoeffs[i] += eps;
            var perturbedOmega = MakeEdgeField(mesh.EdgeCount, 1, perturbedCoeffs);
            double objPlus = penalty.EvaluateObjective(perturbedOmega);

            double fdGrad = (objPlus - obj0) / eps;
            Assert.Equal(grad.Coefficients[i], fdGrad, 1e-5);
        }
    }

    [Fact]
    public void FiniteDifference_GradientVerification_MultiComponent()
    {
        // Verify FD with dimG > 1 (Lie-algebra-valued).
        var mesh = CreateTriangleMesh();
        int dimG = 3;
        double lambda = 2.0;
        var penalty = new CoulombGaugePenalty(mesh, dimG: dimG, lambda: lambda);

        // 3 edges * 3 components = 9 coefficients
        var rng = new Random(42);
        double[] coeffs = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] = rng.NextDouble() * 2 - 1;

        var omega = MakeEdgeField(mesh.EdgeCount, dimG, coeffs);
        var grad = penalty.EvaluateGradient(omega);
        double obj0 = penalty.EvaluateObjective(omega);

        double eps = 1e-7;
        for (int i = 0; i < coeffs.Length; i++)
        {
            var perturbedCoeffs = (double[])coeffs.Clone();
            perturbedCoeffs[i] += eps;
            var perturbedOmega = MakeEdgeField(mesh.EdgeCount, dimG, perturbedCoeffs);
            double objPlus = penalty.EvaluateObjective(perturbedOmega);

            double fdGrad = (objPlus - obj0) / eps;
            Assert.Equal(grad.Coefficients[i], fdGrad, 1e-5);
        }
    }

    [Fact]
    public void ConstantOmega_GaugeInvariant_OnTriangle()
    {
        // On a single triangle, ALL vertices are boundary vertices, so d^* of
        // a constant field does NOT vanish (unlike an interior vertex).
        // But the gradient d(d^*) applied to constant omega gives a non-trivial result.
        // This is the correct discrete behavior: only on a mesh with interior vertices
        // does a truly constant field have zero codifferential at interior points.
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);

        // Constant omega: same value on all edges
        var omega = MakeEdgeField(mesh.EdgeCount, 1, [5.0, 5.0, 5.0]);

        // d^*(omega)[v0] = +1*5 + 1*5 = 10
        // d^*(omega)[v1] = -1*5 + 1*5 = 0
        // d^*(omega)[v2] = -1*5 + (-1)*5 = -10
        // v1 is the only vertex where d^* vanishes -- it has balanced incidence signs.
        double violation = penalty.ComputeViolationNorm(omega);
        Assert.True(violation > 0, "On a boundary-only mesh, constant omega has nonzero codifferential.");
    }

    [Fact]
    public void TwoTriangleMesh_InteriorVertex_ConstantOmega()
    {
        // On the two-triangle mesh, vertices 1 and 2 are interior (shared edge).
        // For a truly constant 1-form, d^* at an interior vertex may or may not vanish
        // depending on the topology. Let's just verify the computation is consistent.
        var mesh = CreateTwoTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);

        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 1.0, 1.0, 1.0, 1.0]);

        double violation = penalty.ComputeViolationNorm(omega);
        double objective = penalty.EvaluateObjective(omega);

        // Violation and objective should be consistent
        Assert.Equal(0.5 * violation * violation, objective, 1e-12);
    }

    [Fact]
    public void AddToObjective_AddsCorrectly()
    {
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);
        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);

        double physicsObj = 10.0;
        double total = penalty.AddToObjective(physicsObj, omega);
        double gaugeObj = penalty.EvaluateObjective(omega);

        Assert.Equal(physicsObj + gaugeObj, total, 1e-12);
    }

    [Fact]
    public void AddToGradient_AddsCorrectly()
    {
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);
        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);

        var physicsGrad = MakeEdgeField(mesh.EdgeCount, 1, [10.0, 20.0, 30.0]);
        var total = penalty.AddToGradient(physicsGrad, omega);
        var gaugeGrad = penalty.EvaluateGradient(omega);

        for (int i = 0; i < total.Coefficients.Length; i++)
        {
            Assert.Equal(
                physicsGrad.Coefficients[i] + gaugeGrad.Coefficients[i],
                total.Coefficients[i],
                1e-12);
        }
    }

    [Fact]
    public void AddToGradient_LambdaZero_ReturnsPhysicsGradient()
    {
        var mesh = CreateTriangleMesh();
        var penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 0.0);
        var omega = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);
        var physicsGrad = MakeEdgeField(mesh.EdgeCount, 1, [10.0, 20.0, 30.0]);

        var total = penalty.AddToGradient(physicsGrad, omega);
        Assert.Same(physicsGrad, total); // Should return the same object when lambda=0
    }

    [Fact]
    public void WithOmegaRef_ShiftsCorrectly()
    {
        var mesh = CreateTriangleMesh();
        double lambda = 1.0;

        // omega_ref = [1, 1, 1], omega = [2, 3, 4]
        // difference = [1, 2, 3]
        // This should match the penalty on omega=[1,2,3] with zero reference
        var omegaRef = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 1.0, 1.0]);
        var penaltyWithRef = new CoulombGaugePenalty(mesh, dimG: 1, lambda: lambda, omegaRef: omegaRef);

        var penaltyNoRef = new CoulombGaugePenalty(mesh, dimG: 1, lambda: lambda);

        var omega = MakeEdgeField(mesh.EdgeCount, 1, [2.0, 3.0, 4.0]);
        var omegaDiff = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);

        Assert.Equal(
            penaltyNoRef.EvaluateObjective(omegaDiff),
            penaltyWithRef.EvaluateObjective(omega),
            1e-12);
    }

    [Fact]
    public void MultiComponent_IndependentChannels()
    {
        // With dimG=2, each algebra component should be independent.
        // Set component 0 to one pattern and component 1 to another;
        // the total objective should be sum of individual objectives.
        var mesh = CreateTriangleMesh();
        int dimG = 2;
        double lambda = 1.0;
        var penalty = new CoulombGaugePenalty(mesh, dimG: dimG, lambda: lambda);

        // omega: edge-major layout [e0_a0, e0_a1, e1_a0, e1_a1, e2_a0, e2_a1]
        // Component 0: [1, 2, 3], Component 1: [4, 5, 6]
        double[] coeffs = [1.0, 4.0, 2.0, 5.0, 3.0, 6.0];
        var omega = MakeEdgeField(mesh.EdgeCount, dimG, coeffs);

        double totalObj = penalty.EvaluateObjective(omega);

        // Compute individually with dimG=1
        var penalty1 = new CoulombGaugePenalty(mesh, dimG: 1, lambda: lambda);
        var omega0 = MakeEdgeField(mesh.EdgeCount, 1, [1.0, 2.0, 3.0]);
        var omega1 = MakeEdgeField(mesh.EdgeCount, 1, [4.0, 5.0, 6.0]);

        double obj0 = penalty1.EvaluateObjective(omega0);
        double obj1 = penalty1.EvaluateObjective(omega1);

        Assert.Equal(obj0 + obj1, totalObj, 1e-12);
    }

    [Fact]
    public void ViolationNorm_MatchesObjective()
    {
        var mesh = CreateTwoTriangleMesh();
        double lambda = 3.0;
        var penalty = new CoulombGaugePenalty(mesh, dimG: 2, lambda: lambda);

        var rng = new Random(123);
        double[] coeffs = new double[mesh.EdgeCount * 2];
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] = rng.NextDouble() * 2 - 1;
        var omega = MakeEdgeField(mesh.EdgeCount, 2, coeffs);

        double violation = penalty.ComputeViolationNorm(omega);
        double objective = penalty.EvaluateObjective(omega);

        // objective = (lambda/2) * violation^2
        Assert.Equal(0.5 * lambda * violation * violation, objective, 1e-12);
    }

    [Fact]
    public void IGaugePenalty_Interface_IsImplemented()
    {
        var mesh = CreateTriangleMesh();
        IGaugePenalty penalty = new CoulombGaugePenalty(mesh, dimG: 1, lambda: 1.0);
        Assert.Equal(1.0, penalty.Lambda);
    }

    [Fact]
    public void GaugePenaltyTerm_Implements_IGaugePenalty()
    {
        IGaugePenalty penalty = new GaugePenaltyTerm(0.5);
        Assert.Equal(0.5, penalty.Lambda);

        var omega = MakeEdgeField(3, 1, [1.0, 2.0, 3.0]);
        double obj = penalty.EvaluateObjective(omega);
        Assert.True(obj > 0);
    }

    [Fact]
    public void GaugeStrategy_Enum_HasExpectedValues()
    {
        var strategies = Enum.GetValues<GaugeStrategy>();
        Assert.Equal(2, strategies.Length);
        Assert.Contains(GaugeStrategy.L2Penalty, strategies);
        Assert.Contains(GaugeStrategy.Coulomb, strategies);
    }

    [Fact]
    public void SolverOptions_GaugeStrategy_DefaultIsL2()
    {
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        Assert.Equal(GaugeStrategy.L2Penalty, options.GaugeStrategy);
    }

    [Fact]
    public void SolverOptions_GaugeStrategy_CanBeSetToCoulomb()
    {
        var options = new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            GaugeStrategy = GaugeStrategy.Coulomb,
            GaugePenaltyLambda = 0.1,
        };
        Assert.Equal(GaugeStrategy.Coulomb, options.GaugeStrategy);
    }

    [Fact]
    public void Codifferential_Adjoint_DotProductIdentity()
    {
        // Verify that <d^* omega, phi> = <omega, d phi> (adjointness).
        // This is the defining property of the codifferential.
        var mesh = CreateTwoTriangleMesh();
        int dimG = 2;
        var penalty = new CoulombGaugePenalty(mesh, dimG: dimG, lambda: 1.0);

        var rng = new Random(999);
        double[] edgeCoeffs = new double[mesh.EdgeCount * dimG];
        double[] vertexCoeffs = new double[mesh.VertexCount * dimG];
        for (int i = 0; i < edgeCoeffs.Length; i++)
            edgeCoeffs[i] = rng.NextDouble() * 2 - 1;
        for (int i = 0; i < vertexCoeffs.Length; i++)
            vertexCoeffs[i] = rng.NextDouble() * 2 - 1;

        double[] dStar = penalty.ApplyCodifferential(edgeCoeffs);
        double[] dPhi = penalty.ApplyExteriorDerivative(vertexCoeffs);

        // <d^* omega, phi>_vertex
        double lhs = 0;
        for (int i = 0; i < dStar.Length; i++)
            lhs += dStar[i] * vertexCoeffs[i];

        // <omega, d phi>_edge
        double rhs = 0;
        for (int i = 0; i < edgeCoeffs.Length; i++)
            rhs += edgeCoeffs[i] * dPhi[i];

        Assert.Equal(lhs, rhs, 1e-12);
    }

    [Fact]
    public void GradientIsSymmetric_ddStar()
    {
        // The graph Laplacian dd^* is a symmetric operator.
        // Verify: <dd^*(omega), eta> = <omega, dd^*(eta)>
        var mesh = CreateTwoTriangleMesh();
        int dimG = 1;
        double lambda = 1.0;
        var penalty = new CoulombGaugePenalty(mesh, dimG: dimG, lambda: lambda);

        var rng = new Random(777);
        double[] coeffsA = new double[mesh.EdgeCount * dimG];
        double[] coeffsB = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < coeffsA.Length; i++)
        {
            coeffsA[i] = rng.NextDouble() * 2 - 1;
            coeffsB[i] = rng.NextDouble() * 2 - 1;
        }

        var omegaA = MakeEdgeField(mesh.EdgeCount, dimG, coeffsA);
        var omegaB = MakeEdgeField(mesh.EdgeCount, dimG, coeffsB);

        var gradA = penalty.EvaluateGradient(omegaA);
        var gradB = penalty.EvaluateGradient(omegaB);

        // <grad(A), B> should equal <A, grad(B)>
        double lhs = 0, rhs = 0;
        for (int i = 0; i < coeffsA.Length; i++)
        {
            lhs += gradA.Coefficients[i] * coeffsB[i];
            rhs += coeffsA[i] * gradB.Coefficients[i];
        }

        Assert.Equal(lhs, rhs, 1e-12);
    }
}
