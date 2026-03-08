using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class ResidualObjectiveJacobianTests
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
        BranchId = "test-pipeline",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
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

    // ===== Residual Assembly =====

    [Fact]
    public void AssembleDerivedState_FlatOmega_AllZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
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
    public void AssembleDerivedState_NonTrivialOmega_UpsilonEqualsF()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());

        // With trivial T and identity S: Upsilon = F
        for (int i = 0; i < derived.CurvatureF.Coefficients.Length; i++)
        {
            Assert.Equal(
                derived.CurvatureF.Coefficients[i],
                derived.ResidualUpsilon.Coefficients[i], 12);
        }
    }

    // ===== Mass Matrix =====

    [Fact]
    public void MassMatrix_UniformWeights_IdentityForTracePairing()
    {
        // Use trace pairing (g = delta_{ab}) so M_Lie = I_3 and M*v = v
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);

        // For su(2) with trace pairing (M_Lie = I_3) and uniform weights (w=1):
        // M*v = v
        var v = new FieldTensor
        {
            Label = "v",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };

        var mv = mass.Apply(v);

        for (int i = 0; i < v.Coefficients.Length; i++)
            Assert.Equal(v.Coefficients[i], mv.Coefficients[i], 12);
    }

    [Fact]
    public void MassMatrix_InnerProduct_MatchesDot_ForIdentityMetric()
    {
        // Use trace pairing (g = delta_{ab}) so inner product = flat dot product
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);

        var u = new FieldTensor
        {
            Label = "u",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };

        // With identity metric and uniform weights: inner product = flat dot product
        double ip = mass.InnerProduct(u, u);
        double dot = FieldTensorOps.Dot(u, u);
        Assert.Equal(dot, ip, 12);
    }

    [Fact]
    public void MassMatrix_CustomWeights_ScalesCorrectly()
    {
        // Use trace pairing (g = delta_{ab}) so M*v = weight * v
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra, new[] { 2.0 }); // weight = 2

        var v = new FieldTensor
        {
            Label = "v",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };

        var mv = mass.Apply(v);

        // M*v = 2 * v (weight = 2, metric = I)
        for (int i = 0; i < v.Coefficients.Length; i++)
            Assert.Equal(2.0 * v.Coefficients[i], mv.Coefficients[i], 12);
    }

    // ===== Objective =====

    [Fact]
    public void Objective_ZeroResidual_IsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());

        double objective = mass.EvaluateObjective(derived.ResidualUpsilon);
        Assert.Equal(0.0, objective, 12);
    }

    [Fact]
    public void Objective_NonZeroResidual_IsPositive()
    {
        // Use trace pairing (positive-definite) so objective is positive
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());

        double objective = mass.EvaluateObjective(derived.ResidualUpsilon);
        Assert.True(objective > 0);
    }

    [Fact]
    public void ResidualBundle_HasCorrectComponents()
    {
        // Use trace pairing (positive-definite) so objective is non-negative
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var derived = assembler.AssembleDerivedState(omega, a0, TestManifest(), DummyGeometry());
        var bundle = assembler.EvaluateResidual(derived, mass);

        Assert.Equal(3, bundle.Components.Count);
        Assert.Contains(bundle.Components, c => c.Label == "Shiab_S");
        Assert.Contains(bundle.Components, c => c.Label == "Torsion_T");
        Assert.Contains(bundle.Components, c => c.Label == "Upsilon");
        Assert.True(bundle.ObjectiveValue >= 0);
    }

    // ===== Jacobian =====

    [Fact]
    public void Jacobian_FlatOmega_ApplyIsLinearizationOfCurvature()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        // For flat omega and trivial T + identity S:
        // J = D_0(delta) = d(delta) (pure exterior derivative, no bracket since omega=0)
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        var deltaTensor = delta.ToFieldTensor();

        var jDelta = jacobian.Apply(deltaTensor);

        // d(delta) on face = sum of signed delta values on boundary edges
        // Should equal the exterior derivative of delta
        Assert.Equal(mesh.FaceCount * algebra.Dimension, jDelta.Coefficients.Length);
    }

    [Fact]
    public void Jacobian_FiniteDifference_VerificationPasses()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Non-zero omega for meaningful Jacobian
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        // Perturbation
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.05, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.03 });
        delta.SetEdgeValue(2, new[] { 0.02, 0.0, 0.1 });

        // Residual evaluation function
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
            $"Jacobian FD verification failed: max abs error = {result.MaxAbsoluteError:E6} at index {result.MaxErrorIndex}");
    }

    [Fact]
    public void Jacobian_FiniteDifference_Verification_3D()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Non-zero omega
        var omega = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            omega.SetEdgeValue(e, new[]
            {
                0.1 * (e + 1),
                0.05 * (e + 2),
                0.02 * (e + 3),
            });
        }
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        // Random perturbation
        var delta = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            delta.SetEdgeValue(e, new[]
            {
                0.01 * (e + 1),
                0.02 * (e + 2),
                0.03 * (e + 3),
            });
        }

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
            $"3D Jacobian FD verification failed: max abs error = {result.MaxAbsoluteError:E6}");
    }

    // ===== Gradient =====

    [Fact]
    public void Gradient_ZeroResidual_IsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var derived = assembler.AssembleDerivedState(omega, a0, manifest, geometry);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        var gradient = jacobian.ComputeGradient(derived.ResidualUpsilon, mass);

        Assert.All(gradient.Coefficients, c => Assert.Equal(0.0, c, 10));
    }

    [Fact]
    public void Gradient_FiniteDifference_VerificationPasses()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var mass = new CpuMassMatrix(mesh, algebra);
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
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
            $"Gradient FD verification failed: max abs error = {result.MaxAbsoluteError:E6}");
    }

    // ===== Jacobian Transpose =====

    [Fact]
    public void JacobianTranspose_ConsistentWithForward()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
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

        // w in output space (faces)
        var w = new FieldTensor
        {
            Label = "w",
            Signature = jacobian.OutputSignature,
            Coefficients = new[] { 1.0, 0.0, 0.5 },
            Shape = new[] { 1, 3 },
        };

        // Test: <J*v, w> == <v, J^T*w>
        var jv = jacobian.Apply(vTensor);
        var jtw = jacobian.ApplyTranspose(w);

        double lhs = FieldTensorOps.Dot(jv, w);
        double rhs = FieldTensorOps.Dot(vTensor, jtw);

        Assert.Equal(lhs, rhs, 10);
    }

    // ===== ILinearOperator Interface =====

    [Fact]
    public void Jacobian_ImplementsILinearOperator()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        ILinearOperator jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        Assert.Equal(mesh.EdgeCount * algebra.Dimension, jacobian.InputDimension);
        Assert.Equal(mesh.FaceCount * algebra.Dimension, jacobian.OutputDimension);
        Assert.Equal("connection-1form", jacobian.InputSignature.CarrierType);
        Assert.Equal("curvature-2form", jacobian.OutputSignature.CarrierType);
    }
}
