using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

/// <summary>
/// Comprehensive tests for LocalAlgebraicTorsionOperator and FirstOrderShiabOperator.
/// Validates carrier type compatibility, correctness, subtractability (Upsilon = S - T),
/// linearization via finite differences, and branch metadata.
/// </summary>
public class TorsionShiabTests
{
    // ===== Test Infrastructure =====

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

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 2, 3 } });

    private static BranchManifest TestManifest() => new()
    {
        BranchId = "test-m4",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
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

    private static GeometryContext DummyGeometry()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };
        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace,
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace,
            },
            Patches = Array.Empty<PatchInfo>(),
        };
    }

    // ===== Branch Metadata Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_BranchId_IsCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        Assert.Equal("local-algebraic", torsion.BranchId);
    }

    [Fact]
    public void LocalAlgebraicTorsion_OutputCarrierType_IsCurvature2Form()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        Assert.Equal("curvature-2form", torsion.OutputCarrierType);
    }

    [Fact]
    public void FirstOrderShiab_BranchId_IsCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        Assert.Equal("first-order-curvature", shiab.BranchId);
    }

    [Fact]
    public void FirstOrderShiab_OutputCarrierType_IsCurvature2Form()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        Assert.Equal("curvature-2form", shiab.OutputCarrierType);
    }

    // ===== Carrier Type Compatibility =====

    [Fact]
    public void CarrierTypeMatch_TorsionAndShiab_AreCompatible()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        Assert.Equal(torsion.OutputCarrierType, shiab.OutputCarrierType);
    }

    [Fact]
    public void CarrierTypeMatch_ValidateCarrierMatch_DoesNotThrow()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        // Should not throw -- both produce "curvature-2form"
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    [Fact]
    public void EvaluatedOutputs_HaveMatchingCarrierType()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        var tH = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var sH = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);

        Assert.Equal("curvature-2form", tH.Signature.CarrierType);
        Assert.Equal("curvature-2form", sH.Signature.CarrierType);
        Assert.Equal(tH.Signature.CarrierType, sH.Signature.CarrierType);
    }

    // ===== Zero Connection Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_ZeroConnection_ProducesZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        Assert.Equal(mesh.FaceCount * algebra.Dimension, result.Coefficients.Length);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void LocalAlgebraicTorsion_ZeroConnection_3D_ProducesZero()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        // Tet has 4 faces, su(2) has dim 3
        Assert.Equal(4 * 3, result.Coefficients.Length);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void FirstOrderShiab_ZeroConnection_ProducesZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var result = shiab.Evaluate(
            curvature.ToFieldTensor(), omega.ToFieldTensor(), TestManifest(), DummyGeometry());

        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    // ===== Output Shape Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_CorrectShape_Triangle()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        Assert.Equal(2, result.Shape.Count);
        Assert.Equal(mesh.FaceCount, result.Shape[0]);
        Assert.Equal(algebra.Dimension, result.Shape[1]);
    }

    [Fact]
    public void LocalAlgebraicTorsion_CorrectShape_Tetrahedron()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        Assert.Equal(2, result.Shape.Count);
        Assert.Equal(4, result.Shape[0]);  // 4 faces in a tet
        Assert.Equal(3, result.Shape[1]);  // dim(su2) = 3
    }

    [Fact]
    public void FirstOrderShiab_CorrectShape()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var result = shiab.Evaluate(
            curvature.ToFieldTensor(), omega.ToFieldTensor(), TestManifest(), DummyGeometry());

        Assert.Equal(2, result.Shape.Count);
        Assert.Equal(mesh.FaceCount, result.Shape[0]);
        Assert.Equal(algebra.Dimension, result.Shape[1]);
    }

    // ===== Subtractability: Upsilon = S - T =====

    [Fact]
    public void Upsilon_CanBeFormed_SMinusT()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 1.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        var tH = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var sH = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);

        // Subtraction should succeed without throwing (carrier types match)
        var upsilon = FieldTensorOps.Subtract(sH, tH);

        Assert.Equal("curvature-2form", upsilon.Signature.CarrierType);
        Assert.Equal(sH.Coefficients.Length, upsilon.Coefficients.Length);
    }

    [Fact]
    public void Upsilon_FlatConnection_IsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var omegaTensor = omega.ToFieldTensor();
        var a0 = omegaTensor;
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var curvature = CurvatureAssembler.Assemble(omega);

        var tH = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var sH = shiab.Evaluate(curvature.ToFieldTensor(), omegaTensor, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(sH, tH);

        // Both S and T are zero for flat connection, so Upsilon = 0
        Assert.All(upsilon.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void Upsilon_NonZeroConnection_IsNonTrivial()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var curvature = CurvatureAssembler.Assemble(omega);

        var tH = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var sH = shiab.Evaluate(curvature.ToFieldTensor(), omegaTensor, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(sH, tH);

        // With non-zero non-abelian connection, Upsilon should generally be non-zero
        double norm = FieldTensorOps.L2Norm(upsilon);
        Assert.True(norm > 0, "Upsilon should be non-zero for non-trivial non-abelian connection.");
    }

    // ===== Torsion Value Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_AbelianAlgebra_ProducesZero()
    {
        // For an abelian algebra, all brackets are zero, so torsion = 0
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateAbelian(1);
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0 });
        omega.SetEdgeValue(1, new[] { 2.0 });
        omega.SetEdgeValue(2, new[] { 3.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0, TestManifest(), DummyGeometry());

        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void LocalAlgebraicTorsion_NonAbelian_ProducesNonZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 }); // T_1 direction
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 }); // T_2 direction
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0, TestManifest(), DummyGeometry());

        double norm = FieldTensorOps.L2Norm(result);
        Assert.True(norm > 0, "Torsion should be non-zero for non-abelian connection in orthogonal directions.");
    }

    [Fact]
    public void LocalAlgebraicTorsion_WithFlatA0_SpecificValues()
    {
        // With A0 = 0: A = omega, B = -omega
        // T[face] = sum_{i<j} s_i * s_j * [A(e_i), B(e_j)]
        //         = sum_{i<j} s_i * s_j * [omega(e_i), -omega(e_j)]
        //         = -sum_{i<j} s_i * s_j * [omega(e_i), omega(e_j)]
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 }); // T_1
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 }); // T_2
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 }); // zero
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omegaTensor, a0, TestManifest(), DummyGeometry());

        // Face boundary: edges 0, 1, 2 with orientations +1, -1, +1
        // Pair (0,1): s0*s1 * [A(e0), B(e1)] = (+1)*(-1) * [(1,0,0), -(0,1,0)]
        //           = -1 * [T_1, -T_2] = -1 * (-T_3) = (0, 0, 1)
        // Pair (0,2): [A(e0), B(e2)] = [(1,0,0), -(0,0,0)] = 0
        // Pair (1,2): [A(e1), B(e2)] = [(0,-1,0), (0,0,0)] = 0 (oriented: s1=-1 for A, s2=+1 for B)
        //   Wait, need to be precise: A(e1) = omega(e1) = (0,1,0), oriented by s1=-1 -> (-1)*(0,1,0)=(0,-1,0)
        //   B(e2) = -omega(e2) = (0,0,0), oriented by s2=+1 -> (0,0,0)
        //   So pair (1,2) contributes [(-0,-1,0), (0,0,0)] = 0
        // T = (0, 0, 1)
        Assert.Equal(0.0, result.Coefficients[0], 10); // T_1 component
        Assert.Equal(0.0, result.Coefficients[1], 10); // T_2 component
        Assert.Equal(1.0, result.Coefficients[2], 10); // T_3 component
    }

    // ===== Shiab Value Tests =====

    [Fact]
    public void FirstOrderShiab_EqualsCurvature()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();
        var omegaTensor = omega.ToFieldTensor();

        var sH = shiab.Evaluate(curvatureTensor, omegaTensor, TestManifest(), DummyGeometry());

        // S = F, so coefficients must be identical
        for (int i = 0; i < curvatureTensor.Coefficients.Length; i++)
        {
            Assert.Equal(curvatureTensor.Coefficients[i], sH.Coefficients[i], 12);
        }

        Assert.Equal("S_h", sH.Label);
    }

    [Fact]
    public void FirstOrderShiab_OutputSignature_IsComplete()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var result = shiab.Evaluate(
            curvature.ToFieldTensor(), omega.ToFieldTensor(), TestManifest(), DummyGeometry());

        Assert.Equal("Y_h", result.Signature.AmbientSpaceId);
        Assert.Equal("curvature-2form", result.Signature.CarrierType);
        Assert.Equal("2", result.Signature.Degree);
        Assert.Equal("float64", result.Signature.NumericPrecision);
        Assert.Equal("dense-row-major", result.Signature.MemoryLayout);
    }

    // ===== Torsion Output Signature Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_OutputSignature_IsComplete()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        Assert.Equal("Y_h", result.Signature.AmbientSpaceId);
        Assert.Equal("curvature-2form", result.Signature.CarrierType);
        Assert.Equal("2", result.Signature.Degree);
        Assert.Equal("canonical", result.Signature.LieAlgebraBasisId);
        Assert.Equal("face-major", result.Signature.ComponentOrderId);
        Assert.Equal("float64", result.Signature.NumericPrecision);
        Assert.Equal("dense-row-major", result.Signature.MemoryLayout);
    }

    // ===== Linearization Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_Linearize_ZeroOmegaZeroA0_ZeroDelta_ProducesZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var delta = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Linearize(omega, a0, delta, TestManifest(), DummyGeometry());

        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.Equal("curvature-2form", result.Signature.CarrierType);
    }

    [Fact]
    public void FirstOrderShiab_Linearize_ZeroOmega_IsExteriorDerivative()
    {
        // When omega = 0, linearization D_omega(delta) = d(delta) + [0, delta] = d(delta)
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        delta.SetEdgeValue(2, new[] { 0.0, 0.0, 1.0 });

        var dS = shiab.Linearize(
            curvature.ToFieldTensor(), omega.ToFieldTensor(), delta.ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // d(delta)[face] = s0*delta(e0) + s1*delta(e1) + s2*delta(e2)
        //                = (+1)*(1,0,0) + (-1)*(0,1,0) + (+1)*(0,0,1) = (1, -1, 1)
        Assert.Equal(1.0, dS.Coefficients[0], 10);
        Assert.Equal(-1.0, dS.Coefficients[1], 10);
        Assert.Equal(1.0, dS.Coefficients[2], 10);
    }

    // ===== Linearization Finite Difference Verification =====

    [Fact]
    public void FirstOrderShiab_Linearize_MatchesFiniteDifference()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Base point omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var omegaTensor = omega.ToFieldTensor();

        // Perturbation direction
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        delta.SetEdgeValue(2, new[] { 0.0, 0.0, 0.1 });
        var deltaTensor = delta.ToFieldTensor();

        // Curvature at omega
        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // Exact linearization
        var dS = shiab.Linearize(curvatureTensor, omegaTensor, deltaTensor, manifest, geometry);

        // Finite difference: [S(omega + eps*delta) - S(omega)] / eps
        double eps = 1e-7;
        var omegaPerturbed = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbed.Coefficients[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var curvaturePerturbed = CurvatureAssembler.Assemble(omegaPerturbed);
        var sBase = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);
        var sPerturbed = shiab.Evaluate(
            curvaturePerturbed.ToFieldTensor(), omegaPerturbed.ToFieldTensor(), manifest, geometry);

        for (int i = 0; i < dS.Coefficients.Length; i++)
        {
            double fdApprox = (sPerturbed.Coefficients[i] - sBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dS.Coefficients[i], 4);
        }
    }

    [Fact]
    public void LocalAlgebraicTorsion_Linearize_MatchesFiniteDifference()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Base point omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var omegaTensor = omega.ToFieldTensor();

        // A0 (non-zero to exercise full path)
        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.2, 0.0, 0.1 });
        a0.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        a0.SetEdgeValue(2, new[] { 0.1, 0.1, 0.0 });
        var a0Tensor = a0.ToFieldTensor();

        // Perturbation
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        delta.SetEdgeValue(2, new[] { 0.0, 0.0, 0.1 });
        var deltaTensor = delta.ToFieldTensor();

        // Exact linearization
        var dT = torsion.Linearize(omegaTensor, a0Tensor, deltaTensor, manifest, geometry);

        // Finite difference: [T(omega + eps*delta, a0) - T(omega, a0)] / eps
        double eps = 1e-7;
        var omegaPerturbedCoeffs = new double[omega.Coefficients.Length];
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbedCoeffs[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var omegaPerturbedTensor = new FieldTensor
        {
            Label = "omega_h",
            Signature = omegaTensor.Signature,
            Coefficients = omegaPerturbedCoeffs,
            Shape = omegaTensor.Shape,
        };

        var tBase = torsion.Evaluate(omegaTensor, a0Tensor, manifest, geometry);
        var tPerturbed = torsion.Evaluate(omegaPerturbedTensor, a0Tensor, manifest, geometry);

        for (int i = 0; i < dT.Coefficients.Length; i++)
        {
            double fdApprox = (tPerturbed.Coefficients[i] - tBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dT.Coefficients[i], 4);
        }
    }

    [Fact]
    public void LocalAlgebraicTorsion_Linearize_MatchesFiniteDifference_3D()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Base point omega with all edges active
        var omega = new ConnectionField(mesh, algebra);
        int edgeCount = mesh.EdgeCount;
        var rng = new Random(42);
        for (int e = 0; e < edgeCount; e++)
        {
            var vals = new double[algebra.Dimension];
            for (int d = 0; d < algebra.Dimension; d++)
                vals[d] = rng.NextDouble() - 0.5;
            omega.SetEdgeValue(e, vals);
        }
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        // Random perturbation
        var delta = new ConnectionField(mesh, algebra);
        for (int e = 0; e < edgeCount; e++)
        {
            var vals = new double[algebra.Dimension];
            for (int d = 0; d < algebra.Dimension; d++)
                vals[d] = rng.NextDouble() - 0.5;
            delta.SetEdgeValue(e, vals);
        }
        var deltaTensor = delta.ToFieldTensor();

        // Exact linearization
        var dT = torsion.Linearize(omegaTensor, a0, deltaTensor, manifest, geometry);

        // Finite difference
        double eps = 1e-7;
        var omegaPerturbedCoeffs = new double[omega.Coefficients.Length];
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbedCoeffs[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var omegaPerturbedTensor = new FieldTensor
        {
            Label = "omega_h",
            Signature = omegaTensor.Signature,
            Coefficients = omegaPerturbedCoeffs,
            Shape = omegaTensor.Shape,
        };

        var tBase = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var tPerturbed = torsion.Evaluate(omegaPerturbedTensor, a0, manifest, geometry);

        for (int i = 0; i < dT.Coefficients.Length; i++)
        {
            double fdApprox = (tPerturbed.Coefficients[i] - tBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dT.Coefficients[i], 4);
        }
    }

    [Fact]
    public void FirstOrderShiab_Linearize_MatchesFiniteDifference_3D()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Random omega
        var omega = new ConnectionField(mesh, algebra);
        int edgeCount = mesh.EdgeCount;
        var rng = new Random(123);
        for (int e = 0; e < edgeCount; e++)
        {
            var vals = new double[algebra.Dimension];
            for (int d = 0; d < algebra.Dimension; d++)
                vals[d] = rng.NextDouble() - 0.5;
            omega.SetEdgeValue(e, vals);
        }
        var omegaTensor = omega.ToFieldTensor();

        // Random perturbation
        var delta = new ConnectionField(mesh, algebra);
        for (int e = 0; e < edgeCount; e++)
        {
            var vals = new double[algebra.Dimension];
            for (int d = 0; d < algebra.Dimension; d++)
                vals[d] = rng.NextDouble() - 0.5;
            delta.SetEdgeValue(e, vals);
        }
        var deltaTensor = delta.ToFieldTensor();

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // Exact linearization
        var dS = shiab.Linearize(curvatureTensor, omegaTensor, deltaTensor, manifest, geometry);

        // Finite difference
        double eps = 1e-7;
        var omegaPerturbed = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbed.Coefficients[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var curvaturePerturbed = CurvatureAssembler.Assemble(omegaPerturbed);
        var sBase = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);
        var sPerturbed = shiab.Evaluate(
            curvaturePerturbed.ToFieldTensor(), omegaPerturbed.ToFieldTensor(), manifest, geometry);

        for (int i = 0; i < dS.Coefficients.Length; i++)
        {
            double fdApprox = (sPerturbed.Coefficients[i] - sBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dS.Coefficients[i], 4);
        }
    }

    // ===== Multi-Face Mesh Tests =====

    [Fact]
    public void LocalAlgebraicTorsion_TwoTriangles_CorrectFaceCount()
    {
        var mesh = TwoTriangles();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        // Two triangles -> 2 faces (could share edges)
        Assert.Equal(mesh.FaceCount, result.Shape[0]);
        Assert.Equal(algebra.Dimension, result.Shape[1]);
        Assert.Equal(mesh.FaceCount * algebra.Dimension, result.Coefficients.Length);
    }

    // ===== Linearized Upsilon Test =====

    [Fact]
    public void LinearizedUpsilon_CanBeFormed_dSMinusdT()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.2 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        var deltaTensor = delta.ToFieldTensor();

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        var dT = torsion.Linearize(omegaTensor, a0, deltaTensor, manifest, geometry);
        var dS = shiab.Linearize(curvatureTensor, omegaTensor, deltaTensor, manifest, geometry);

        // dUpsilon = dS - dT should succeed (matching carrier types)
        var dUpsilon = FieldTensorOps.Subtract(dS, dT);

        Assert.Equal("curvature-2form", dUpsilon.Signature.CarrierType);
        Assert.Equal(dS.Coefficients.Length, dUpsilon.Coefficients.Length);
    }

    [Fact]
    public void LinearizedUpsilon_MatchesFiniteDifference()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Base omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.3 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        // Perturbation
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.05, 0.02, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.03, 0.01 });
        delta.SetEdgeValue(2, new[] { 0.01, 0.0, 0.04 });
        var deltaTensor = delta.ToFieldTensor();

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // Exact linearized Upsilon = dS - dT
        var dT = torsion.Linearize(omegaTensor, a0, deltaTensor, manifest, geometry);
        var dS = shiab.Linearize(curvatureTensor, omegaTensor, deltaTensor, manifest, geometry);
        var dUpsilon = FieldTensorOps.Subtract(dS, dT);

        // Finite difference Upsilon
        double eps = 1e-7;
        var omegaPertCoeffs = new double[omega.Coefficients.Length];
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPertCoeffs[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var omegaPert = new ConnectionField(mesh, algebra, omegaPertCoeffs);
        var omegaPertTensor = omegaPert.ToFieldTensor();

        // Upsilon(omega) = S(F(omega), omega) - T(omega, a0)
        var tBase = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var sBase = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);
        var upsilonBase = FieldTensorOps.Subtract(sBase, tBase);

        var curvaturePert = CurvatureAssembler.Assemble(omegaPert);
        var tPert = torsion.Evaluate(omegaPertTensor, a0, manifest, geometry);
        var sPert = shiab.Evaluate(curvaturePert.ToFieldTensor(), omegaPertTensor, manifest, geometry);
        var upsilonPert = FieldTensorOps.Subtract(sPert, tPert);

        for (int i = 0; i < dUpsilon.Coefficients.Length; i++)
        {
            double fdApprox = (upsilonPert.Coefficients[i] - upsilonBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dUpsilon.Coefficients[i], 4);
        }
    }

    // ===== Scaling and Linearity Tests =====

    [Fact]
    public void FirstOrderShiab_Linearize_IsLinearInDelta()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        var omegaTensor = omega.ToFieldTensor();
        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        var delta1 = new ConnectionField(mesh, algebra);
        delta1.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        var delta1Tensor = delta1.ToFieldTensor();

        var delta2 = new ConnectionField(mesh, algebra);
        delta2.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        var delta2Tensor = delta2.ToFieldTensor();

        // Compute dS(delta1) and dS(delta2) and dS(delta1 + delta2)
        var dS1 = shiab.Linearize(curvatureTensor, omegaTensor, delta1Tensor, manifest, geometry);
        var dS2 = shiab.Linearize(curvatureTensor, omegaTensor, delta2Tensor, manifest, geometry);

        var delta12 = FieldTensorOps.Add(delta1Tensor, delta2Tensor);
        var dS12 = shiab.Linearize(curvatureTensor, omegaTensor, delta12, manifest, geometry);

        // Linearity: dS(delta1 + delta2) = dS(delta1) + dS(delta2)
        var dS1PlusdS2 = FieldTensorOps.Add(dS1, dS2);
        for (int i = 0; i < dS12.Coefficients.Length; i++)
        {
            Assert.Equal(dS1PlusdS2.Coefficients[i], dS12.Coefficients[i], 10);
        }
    }

    [Fact]
    public void LocalAlgebraicTorsion_Linearize_IsLinearInDelta()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var delta1 = new ConnectionField(mesh, algebra);
        delta1.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        var delta1Tensor = delta1.ToFieldTensor();

        var delta2 = new ConnectionField(mesh, algebra);
        delta2.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        var delta2Tensor = delta2.ToFieldTensor();

        var dT1 = torsion.Linearize(omegaTensor, a0, delta1Tensor, manifest, geometry);
        var dT2 = torsion.Linearize(omegaTensor, a0, delta2Tensor, manifest, geometry);

        var delta12 = FieldTensorOps.Add(delta1Tensor, delta2Tensor);
        var dT12 = torsion.Linearize(omegaTensor, a0, delta12, manifest, geometry);

        // Linearity: dT(delta1 + delta2) = dT(delta1) + dT(delta2)
        var dT1PlusdT2 = FieldTensorOps.Add(dT1, dT2);
        for (int i = 0; i < dT12.Coefficients.Length; i++)
        {
            Assert.Equal(dT1PlusdT2.Coefficients[i], dT12.Coefficients[i], 10);
        }
    }

    // ===== Non-Trivial A0 Test =====

    [Fact]
    public void LocalAlgebraicTorsion_NonTrivialA0_ProducesNonZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);

        // With A0 non-zero and omega = 0:
        // A = A0, B = A0 (since B = A0 - 0 = A0)
        // T[face] = sum [A0(ei), A0(ej)] which is generally non-zero for non-abelian
        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 }); // T_1
        a0.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 }); // T_2
        a0.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });
        var a0Tensor = a0.ToFieldTensor();

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0Tensor, TestManifest(), DummyGeometry());

        double norm = FieldTensorOps.L2Norm(result);
        Assert.True(norm > 0, "Torsion should be non-zero when A0 is non-trivial in non-abelian algebra.");
    }
}
