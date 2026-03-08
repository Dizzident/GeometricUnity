using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

/// <summary>
/// Integration tests for the full CPU residual pipeline (Milestone 5 acceptance criteria).
/// Validates:
/// - Upsilon = S - T for both operator families
/// - I2_h = (1/2) Upsilon^T M Upsilon is non-negative
/// - I2_h = 0 for flat connections
/// - J = dS/domega - dT/domega FD verification with non-trivial operators
/// - Gradient G = J^T M Upsilon FD verification with non-trivial operators
/// </summary>
public class ResidualPipelineIntegrationTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static BranchManifest TestManifest() => new()
    {
        BranchId = "test-pipeline",
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

    // ===== Non-trivial operator tests (LocalAlgebraicTorsion + FirstOrderShiab) =====

    [Fact]
    public void NonTrivialOperators_FlatOmega_AllFieldsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());

        Assert.All(derived.CurvatureF.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.All(derived.TorsionT.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.All(derived.ShiabS.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.All(derived.ResidualUpsilon.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void NonTrivialOperators_Upsilon_Equals_S_Minus_T()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.5 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());

        // Verify Upsilon = S - T coefficient by coefficient
        var upsilonCheck = FieldTensorOps.Subtract(derived.ShiabS, derived.TorsionT);
        for (int i = 0; i < derived.ResidualUpsilon.Coefficients.Length; i++)
        {
            Assert.Equal(
                upsilonCheck.Coefficients[i],
                derived.ResidualUpsilon.Coefficients[i], 12);
        }
    }

    [Fact]
    public void NonTrivialOperators_ObjectiveNonNegative()
    {
        // Use trace pairing (positive-definite) so the objective I2_h >= 0
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());
        double objective = mass.EvaluateObjective(derived.ResidualUpsilon);

        Assert.True(objective >= 0, $"Objective I2_h must be non-negative, got {objective}");
    }

    [Fact]
    public void NonTrivialOperators_ObjectiveZero_ForFlatConnection()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());
        double objective = mass.EvaluateObjective(derived.ResidualUpsilon);

        Assert.Equal(0.0, objective, 12);
    }

    [Fact]
    public void NonTrivialOperators_ResidualBundle_HasCorrectComponents()
    {
        // Use trace pairing (positive-definite) so objective and norm are non-negative
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());
        var bundle = assembler.EvaluateResidual(derived, mass);

        Assert.Equal(3, bundle.Components.Count);
        Assert.Contains(bundle.Components, c => c.Label == "Shiab_S");
        Assert.Contains(bundle.Components, c => c.Label == "Torsion_T");
        Assert.Contains(bundle.Components, c => c.Label == "Upsilon");
        Assert.True(bundle.ObjectiveValue >= 0);
        Assert.True(bundle.TotalNorm >= 0);
    }

    // ===== Jacobian finite-difference verification with non-trivial operators =====

    [Fact]
    public void NonTrivialOperators_Jacobian_FiniteDifference_Passes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.05, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.03 });
        delta.SetEdgeValue(2, new[] { 0.02, 0.0, 0.1 });

        FieldTensor EvalUpsilon(FieldTensor omegaTensor)
        {
            var conn = new ConnectionField(mesh, algebra, (double[])omegaTensor.Coefficients.Clone());
            var derived = assembler.AssembleDerivedState(conn, a0, manifest, geometry);
            return derived.ResidualUpsilon;
        }

        var result = FiniteDifferenceVerifier.Verify(
            jacobian, EvalUpsilon,
            omega.ToFieldTensor(), delta.ToFieldTensor());

        Assert.True(result.Passed,
            $"Non-trivial operator Jacobian FD verification failed: max abs error = {result.MaxAbsoluteError:E6} at index {result.MaxErrorIndex}");
    }

    [Fact]
    public void NonTrivialOperators_Gradient_FiniteDifference_Passes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        delta.SetEdgeValue(2, new[] { 0.0, 0.0, 0.1 });

        FieldTensor EvalUpsilon(FieldTensor omegaTensor)
        {
            var conn = new ConnectionField(mesh, algebra, (double[])omegaTensor.Coefficients.Clone());
            var derived = assembler.AssembleDerivedState(conn, a0, manifest, geometry);
            return derived.ResidualUpsilon;
        }

        var result = FiniteDifferenceVerifier.VerifyGradient(
            jacobian, mass, EvalUpsilon,
            omega.ToFieldTensor(), delta.ToFieldTensor());

        Assert.True(result.Passed,
            $"Non-trivial operator gradient FD verification failed: max abs error = {result.MaxAbsoluteError:E6}");
    }

    // ===== Jacobian transpose consistency with non-trivial operators =====

    [Fact]
    public void NonTrivialOperators_JacobianTranspose_Consistent()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        // v in input space (edges)
        var v = new ConnectionField(mesh, algebra);
        v.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        v.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var vTensor = v.ToFieldTensor();

        // w in output space (faces) - use same signature as Jacobian output
        var wCoeffs = new[] { 1.0, 0.0, 0.5 };
        var w = new FieldTensor
        {
            Label = "w",
            Signature = jacobian.OutputSignature,
            Coefficients = wCoeffs,
            Shape = new[] { 1, 3 },
        };

        // Adjoint test: <J*v, w> == <v, J^T*w>
        // We compute the dot products manually since carrier types differ between input/output space
        var jv = jacobian.Apply(vTensor);
        var jtw = jacobian.ApplyTranspose(w);

        double lhs = 0;
        for (int i = 0; i < jv.Coefficients.Length; i++)
            lhs += jv.Coefficients[i] * w.Coefficients[i];

        double rhs = 0;
        for (int i = 0; i < vTensor.Coefficients.Length; i++)
            rhs += vTensor.Coefficients[i] * jtw.Coefficients[i];

        Assert.Equal(lhs, rhs, 10);
    }

    // ===== Cross-operator family consistency =====

    [Fact]
    public void BothOperatorFamilies_CarrierTypeCompatibility()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();

        // Family 1: Trivial + Identity
        var torsion1 = new TrivialTorsionCpu(mesh, algebra);
        var shiab1 = new IdentityShiabCpu(mesh, algebra);
        Assert.Equal(torsion1.OutputCarrierType, shiab1.OutputCarrierType);

        // Family 2: LocalAlgebraic + FirstOrder
        var torsion2 = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab2 = new FirstOrderShiabOperator(mesh, algebra);
        Assert.Equal(torsion2.OutputCarrierType, shiab2.OutputCarrierType);

        // All operators share the common residual carrier type "curvature-2form"
        Assert.Equal(torsion1.OutputCarrierType, torsion2.OutputCarrierType);
        Assert.Equal("curvature-2form", torsion1.OutputCarrierType);
    }

    [Fact]
    public void CrossFamily_CarrierMatch_AllowsMixing()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();

        // Cross-family mixing is allowed because all operators share "curvature-2form"
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        // Should not throw -- carrier types now match
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        Assert.NotNull(assembler);
    }

    // ===== Mass matrix symmetry =====

    [Fact]
    public void MassMatrix_InnerProduct_IsSymmetric()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);

        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };

        var u = new FieldTensor
        {
            Label = "u",
            Signature = sig,
            Coefficients = new[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };

        var v = new FieldTensor
        {
            Label = "v",
            Signature = sig,
            Coefficients = new[] { 4.0, 5.0, 6.0 },
            Shape = new[] { 1, 3 },
        };

        double ipUV = mass.InnerProduct(u, v);
        double ipVU = mass.InnerProduct(v, u);

        Assert.Equal(ipUV, ipVU, 12);
    }

    [Fact]
    public void MassMatrix_InnerProduct_IsPositiveDefinite()
    {
        // Use trace pairing (positive-definite) so inner product is positive-definite
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);

        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };

        var u = new FieldTensor
        {
            Label = "u",
            Signature = sig,
            Coefficients = new[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };

        double ip = mass.InnerProduct(u, u);
        Assert.True(ip > 0, "Inner product of non-zero vector with itself must be positive.");
    }

    // ===== Objective formula =====

    [Fact]
    public void Objective_Is_HalfNormSquared()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);

        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };

        var upsilon = new FieldTensor
        {
            Label = "Upsilon",
            Signature = sig,
            Coefficients = new[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };

        double objective = mass.EvaluateObjective(upsilon);
        double normSq = mass.InnerProduct(upsilon, upsilon);

        Assert.Equal(0.5 * normSq, objective, 12);
    }
}
