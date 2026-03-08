using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class AugmentedTorsionCpuTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static SimplicialMesh SingleTetrahedron() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

    private static BranchManifest TestManifest() => new()
    {
        BranchId = "test-augmented",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "augmented-torsion",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    private static GeometryContext DummyGeometry() => new()
    {
        BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        },
        ObservationBinding = new GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        },
        Patches = Array.Empty<PatchInfo>(),
    };

    // ===== Basic properties =====

    [Fact]
    public void BranchIdAndCarrier()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);

        Assert.Equal("augmented-torsion", torsion.BranchId);
        Assert.Equal("curvature-2form", torsion.OutputCarrierType);
    }

    [Fact]
    public void CarrierMatch_WithIdentityShiab()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Should not throw
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    // ===== omega = A0 => T = 0 =====

    [Fact]
    public void OmegaEqualsA0_TorsionIsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);

        // Any non-zero A0, but omega = A0
        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 1.0, 0.5, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0Tensor = a0.ToFieldTensor();

        // omega = A0 exactly
        var omega = new ConnectionField(mesh, algebra);
        Array.Copy(a0.Coefficients, omega.Coefficients, a0.Coefficients.Length);
        var omegaTensor = omega.ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0Tensor, TestManifest(), DummyGeometry());

        Assert.Equal(mesh.FaceCount * algebra.Dimension, result.Coefficients.Length);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    // ===== A0 = 0 => T^aug = d(omega) (just the exterior derivative) =====

    [Fact]
    public void A0IsZero_TorsionIsDOmega()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);

        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 1.0 });
        var omegaTensor = omega.ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0, TestManifest(), DummyGeometry());

        // When A0 = 0, alpha = omega, and d_{A0}(alpha) = d(omega) + [0, omega] = d(omega)
        // d(omega) on face = sum of signed omega values on boundary edges
        // This should match the dOmega part of curvature (without the wedge term)
        int dimG = algebra.Dimension;
        var expected = new double[mesh.FaceCount * dimG];
        for (int fi = 0; fi < mesh.FaceCount; fi++)
        {
            var boundaryEdges = mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = mesh.FaceBoundaryOrientations[fi];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < dimG; a++)
                {
                    expected[fi * dimG + a] += sign * omegaTensor.Coefficients[edgeIdx * dimG + a];
                }
            }
        }

        for (int i = 0; i < result.Coefficients.Length; i++)
        {
            Assert.Equal(expected[i], result.Coefficients[i], 12);
        }
    }

    // ===== Non-trivial test with bracket term =====

    [Fact]
    public void NonZeroA0_BracketTermContributes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        a0.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });
        var a0Tensor = a0.ToFieldTensor();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 2.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 2.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 1.0 });
        var omegaTensor = omega.ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0Tensor, TestManifest(), DummyGeometry());

        // alpha = omega - A0 = [(1,0,0), (0,1,0), (0,0,1)]
        // d(alpha) is the exterior derivative part
        // [A0 wedge alpha] adds bracket terms with non-zero A0
        // Result should be non-zero (bracket of A0 and alpha contributes)
        bool hasNonZero = false;
        for (int i = 0; i < result.Coefficients.Length; i++)
        {
            if (System.Math.Abs(result.Coefficients[i]) > 1e-12)
            {
                hasNonZero = true;
                break;
            }
        }
        Assert.True(hasNonZero, "Expected non-zero torsion with bracket contribution");
    }

    // ===== Correct shape for 3D mesh =====

    [Fact]
    public void CorrectShape_Tetrahedron()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        // Tet has 4 faces, su(2) has dim 3
        Assert.Equal(4 * 3, result.Coefficients.Length);
        Assert.Equal(2, result.Shape.Count);
        Assert.Equal(4, result.Shape[0]);
        Assert.Equal(3, result.Shape[1]);
    }

    // ===== Linearity: T(omega1 + omega2) - T(omega1) = T(omega2) when A0 = 0 =====

    [Fact]
    public void Linearity_InOmega()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.0, 0.3 });
        var a0Tensor = a0.ToFieldTensor();

        var omega1 = new ConnectionField(mesh, algebra);
        omega1.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega1.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega1.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });

        var omega2 = new ConnectionField(mesh, algebra);
        omega2.SetEdgeValue(0, new[] { 0.2, 0.0, 0.1 });
        omega2.SetEdgeValue(1, new[] { 0.1, 0.1, 0.0 });
        omega2.SetEdgeValue(2, new[] { 0.0, 0.2, 0.1 });

        // omega_sum = omega1 + omega2
        var omegaSum = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega1.Coefficients.Length; i++)
            omegaSum.Coefficients[i] = omega1.Coefficients[i] + omega2.Coefficients[i];

        // T^aug is d_{A0}(omega - A0) which is linear in omega
        // So T(omega1 + omega2, A0) should equal T(omega1, A0) + T(omega2, A0) - T(A0, A0)
        // Because d_{A0}((omega1+omega2) - A0) = d_{A0}(omega1 - A0) + d_{A0}(omega2 - A0) - d_{A0}(A0 - A0)
        // Hmm, that's not quite right. Let's test: T(c*omega + (1-c)*A0, A0) = c * T(omega, A0)
        // Since d_{A0}(c*(omega-A0)) = c * d_{A0}(omega-A0) by linearity of d_{A0}

        double c = 2.5;
        var omegaScaled = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega1.Coefficients.Length; i++)
            omegaScaled.Coefficients[i] = c * omega1.Coefficients[i] + (1.0 - c) * a0.Coefficients[i];

        var t1 = torsion.Evaluate(omega1.ToFieldTensor(), a0Tensor, manifest, geometry);
        var tScaled = torsion.Evaluate(omegaScaled.ToFieldTensor(), a0Tensor, manifest, geometry);

        for (int i = 0; i < t1.Coefficients.Length; i++)
        {
            Assert.Equal(c * t1.Coefficients[i], tScaled.Coefficients[i], 10);
        }
    }

    // ===== Linearization matches finite difference =====

    [Fact]
    public void Linearize_MatchesFiniteDifference()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Background connection
        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.0, 0.3 });
        var a0Tensor = a0.ToFieldTensor();

        // Base point omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var omegaTensor = omega.ToFieldTensor();

        // Perturbation
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.02, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.03 });
        delta.SetEdgeValue(2, new[] { 0.02, 0.0, 0.1 });
        var deltaTensor = delta.ToFieldTensor();

        // Exact linearization
        var dT = torsion.Linearize(omegaTensor, a0Tensor, deltaTensor, manifest, geometry);

        // Finite difference: (T(omega + eps*delta) - T(omega)) / eps
        double eps = 1e-7;
        var omegaPerturbed = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbed.Coefficients[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var tBase = torsion.Evaluate(omegaTensor, a0Tensor, manifest, geometry);
        var tPerturbed = torsion.Evaluate(omegaPerturbed.ToFieldTensor(), a0Tensor, manifest, geometry);

        for (int i = 0; i < dT.Coefficients.Length; i++)
        {
            double fdApprox = (tPerturbed.Coefficients[i] - tBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dT.Coefficients[i], 4);
        }
    }

    // ===== Linearization is independent of omega (constant Jacobian) =====

    [Fact]
    public void Linearize_IndependentOfOmega()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.0, 0.3 });
        var a0Tensor = a0.ToFieldTensor();

        // Two different omega values
        var omega1 = new ConnectionField(mesh, algebra);
        omega1.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega1.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega1.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });

        var omega2 = new ConnectionField(mesh, algebra);
        omega2.SetEdgeValue(0, new[] { 1.0, 0.5, 0.3 });
        omega2.SetEdgeValue(1, new[] { 0.2, 0.1, 0.0 });
        omega2.SetEdgeValue(2, new[] { 0.0, 0.7, 0.2 });

        // Same perturbation
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        delta.SetEdgeValue(2, new[] { 0.0, 0.0, 0.1 });
        var deltaTensor = delta.ToFieldTensor();

        var dT1 = torsion.Linearize(omega1.ToFieldTensor(), a0Tensor, deltaTensor, manifest, geometry);
        var dT2 = torsion.Linearize(omega2.ToFieldTensor(), a0Tensor, deltaTensor, manifest, geometry);

        // dT/domega is d_{A0}, which doesn't depend on omega
        for (int i = 0; i < dT1.Coefficients.Length; i++)
        {
            Assert.Equal(dT1.Coefficients[i], dT2.Coefficients[i], 12);
        }
    }

    // ===== Abelian algebra: bracket term vanishes, T^aug = d(alpha) =====

    [Fact]
    public void AbelianAlgebra_BracketTermVanishes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateAbelian(3);
        var torsion = new AugmentedTorsionCpu(mesh, algebra);

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 1.0, 0.5, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0Tensor = a0.ToFieldTensor();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 2.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 2.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 2.0 });
        var omegaTensor = omega.ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0Tensor, TestManifest(), DummyGeometry());

        // For abelian algebra, [A0, alpha] = 0, so T^aug = d(alpha)
        int dimG = algebra.Dimension;
        var alpha = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < alpha.Length; i++)
            alpha[i] = omegaTensor.Coefficients[i] - a0Tensor.Coefficients[i];

        var expected = new double[mesh.FaceCount * dimG];
        for (int fi = 0; fi < mesh.FaceCount; fi++)
        {
            var boundaryEdges = mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = mesh.FaceBoundaryOrientations[fi];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < dimG; a++)
                {
                    expected[fi * dimG + a] += sign * alpha[edgeIdx * dimG + a];
                }
            }
        }

        for (int i = 0; i < result.Coefficients.Length; i++)
        {
            Assert.Equal(expected[i], result.Coefficients[i], 12);
        }
    }

    // ===== Pipeline: Upsilon = S - T with augmented torsion =====

    [Fact]
    public void Pipeline_AugmentedTorsion_UpsilonEqualsF_MinusT()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.0, 0.3 });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });

        var omegaTensor = omega.ToFieldTensor();
        var a0Tensor = a0.ToFieldTensor();

        // Curvature
        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // T_h (augmented torsion)
        var tH = torsion.Evaluate(omegaTensor, a0Tensor, manifest, geometry);

        // S_h = F (identity shiab)
        var sH = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);

        // Upsilon = S - T
        var upsilon = FieldTensorOps.Subtract(sH, tH);

        // Verify Upsilon = F - T^aug (not equal to F, since T^aug != 0)
        for (int i = 0; i < upsilon.Coefficients.Length; i++)
        {
            double expected = curvatureTensor.Coefficients[i] - tH.Coefficients[i];
            Assert.Equal(expected, upsilon.Coefficients[i], 12);
        }

        // Verify T^aug is actually non-zero (since omega != A0)
        bool torsionNonZero = false;
        for (int i = 0; i < tH.Coefficients.Length; i++)
        {
            if (System.Math.Abs(tH.Coefficients[i]) > 1e-12)
            {
                torsionNonZero = true;
                break;
            }
        }
        Assert.True(torsionNonZero, "Torsion should be non-zero when omega != A0");
    }

    // ===== FD verification on tetrahedron (4 faces) =====

    [Fact]
    public void Linearize_Tetrahedron_MatchesFiniteDifference()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var a0 = new ConnectionField(mesh, algebra);
        var omega = new ConnectionField(mesh, algebra);
        var delta = new ConnectionField(mesh, algebra);

        // Set random-ish values on all edges (tet has 6 edges)
        var rng = new Random(42);
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var a0Val = new double[algebra.Dimension];
            var omVal = new double[algebra.Dimension];
            var deVal = new double[algebra.Dimension];
            for (int a = 0; a < algebra.Dimension; a++)
            {
                a0Val[a] = rng.NextDouble() - 0.5;
                omVal[a] = rng.NextDouble() - 0.5;
                deVal[a] = 0.1 * (rng.NextDouble() - 0.5);
            }
            a0.SetEdgeValue(e, a0Val);
            omega.SetEdgeValue(e, omVal);
            delta.SetEdgeValue(e, deVal);
        }

        var a0Tensor = a0.ToFieldTensor();
        var omegaTensor = omega.ToFieldTensor();
        var deltaTensor = delta.ToFieldTensor();

        var dT = torsion.Linearize(omegaTensor, a0Tensor, deltaTensor, manifest, geometry);

        double eps = 1e-7;
        var omegaPerturbed = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbed.Coefficients[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var tBase = torsion.Evaluate(omegaTensor, a0Tensor, manifest, geometry);
        var tPerturbed = torsion.Evaluate(omegaPerturbed.ToFieldTensor(), a0Tensor, manifest, geometry);

        for (int i = 0; i < dT.Coefficients.Length; i++)
        {
            double fdApprox = (tPerturbed.Coefficients[i] - tBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dT.Coefficients[i], 4);
        }
    }
}
