using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class SerializationRoundTripTests
{
    [Fact]
    public void BranchManifest_RoundTrips()
    {
        var original = BranchManifestFactory.CreateEmpty("test-branch");
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<BranchManifest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.BranchId, deserialized!.BranchId);
        Assert.Equal(original.SchemaVersion, deserialized.SchemaVersion);
        Assert.Equal(original.BaseDimension, deserialized.BaseDimension);
        Assert.Equal(original.AmbientDimension, deserialized.AmbientDimension);
        Assert.Equal(original.ActiveGaugeStrategy, deserialized.ActiveGaugeStrategy);
        Assert.Equal(original.LieAlgebraId, deserialized.LieAlgebraId);
    }

    [Fact]
    public void EnvironmentSpec_RoundTrips()
    {
        var original = EnvironmentSpecFactory.CreateEmpty("test-env", "test-branch");
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<EnvironmentSpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.EnvironmentId, deserialized!.EnvironmentId);
        Assert.Equal(original.Branch.BranchId, deserialized.Branch.BranchId);
        Assert.Equal(original.ScenarioType, deserialized.ScenarioType);
        Assert.Equal(original.Geometry.BaseSpace.Dimension, deserialized.Geometry.BaseSpace.Dimension);
    }

    [Fact]
    public void SpaceRef_RoundTrips()
    {
        var original = new SpaceRef { SpaceId = "Y_h", Dimension = 14, Label = "ambient" };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<SpaceRef>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("Y_h", deserialized!.SpaceId);
        Assert.Equal(14, deserialized.Dimension);
        Assert.Equal("ambient", deserialized.Label);
    }

    [Fact]
    public void BranchRef_RoundTrips()
    {
        var original = new BranchRef { BranchId = "test", SchemaVersion = "1.0.0" };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<BranchRef>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("test", deserialized!.BranchId);
        Assert.Equal("1.0.0", deserialized.SchemaVersion);
    }

    [Fact]
    public void ProvenanceMeta_RoundTrips()
    {
        var original = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "abc123",
            Branch = new BranchRef { BranchId = "b1", SchemaVersion = "1.0.0" },
            Backend = "cpu-reference",
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ProvenanceMeta>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("abc123", deserialized!.CodeRevision);
        Assert.Equal("cpu-reference", deserialized.Backend);
    }

    [Fact]
    public void TensorSignature_RoundTrips()
    {
        var original = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "row-major",
            MemoryLayout = "dense-row-major",
            NumericPrecision = "float64",
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<TensorSignature>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("connection-1form", deserialized!.CarrierType);
        Assert.Equal("float64", deserialized.NumericPrecision);
    }

    [Fact]
    public void FieldTensor_RoundTrips()
    {
        var original = new FieldTensor
        {
            Label = "omega_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "standard",
                ComponentOrderId = "row-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0 },
            Shape = new[] { 2, 2 },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<FieldTensor>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("omega_h", deserialized!.Label);
        Assert.Equal(4, deserialized.Coefficients.Length);
        Assert.Equal(new[] { 2, 2 }, deserialized.Shape);
    }

    [Fact]
    public void DiscreteState_RoundTrips()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "row-major",
            MemoryLayout = "dense-row-major",
        };
        var branch = new BranchRef { BranchId = "b1", SchemaVersion = "1.0.0" };
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };

        var original = new DiscreteState
        {
            Branch = branch,
            Geometry = new GeometryContext
            {
                BaseSpace = baseSpace,
                AmbientSpace = ambientSpace,
                DiscretizationType = "simplicial",
                QuadratureRuleId = "gauss-2",
                BasisFamilyId = "lagrange-p1",
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
                Patches = new[] { new PatchInfo { PatchId = "p0", ElementCount = 10 } },
            },
            Omega = new FieldTensor
            {
                Label = "omega_h",
                Signature = sig,
                Coefficients = new double[] { 0.0 },
                Shape = new[] { 1 },
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "test",
                Branch = branch,
            },
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<DiscreteState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("b1", deserialized!.Branch.BranchId);
        Assert.Equal("simplicial", deserialized.Geometry.DiscretizationType);
        Assert.Equal("omega_h", deserialized.Omega.Label);
    }

    [Fact]
    public void DerivedState_RoundTrips()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "row-major",
            MemoryLayout = "dense-row-major",
        };
        var field = new FieldTensor
        {
            Label = "F_h",
            Signature = sig,
            Coefficients = new double[] { 1.0 },
            Shape = new[] { 1 },
        };

        var original = new DerivedState
        {
            CurvatureF = field,
            TorsionT = new FieldTensor { Label = "T_h", Signature = sig, Coefficients = new double[] { 1.0 }, Shape = new[] { 1 } },
            ShiabS = new FieldTensor { Label = "S_h", Signature = sig, Coefficients = new double[] { 1.0 }, Shape = new[] { 1 } },
            ResidualUpsilon = new FieldTensor { Label = "Upsilon_h", Signature = sig, Coefficients = new double[] { 1.0 }, Shape = new[] { 1 } },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<DerivedState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("F_h", deserialized!.CurvatureF.Label);
        Assert.Equal("Upsilon_h", deserialized.ResidualUpsilon.Label);
    }

    [Fact]
    public void LinearizationState_RoundTrips()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "gradient",
            Degree = "1",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "row-major",
            MemoryLayout = "dense-row-major",
        };

        var original = new LinearizationState
        {
            Jacobian = new LinearOperatorModel
            {
                Label = "jacobian",
                RealizationType = "dense",
                Rows = 4,
                Cols = 4,
                Values = new double[] { 1, 0, 0, 1 },
            },
            GradientLikeResidual = new FieldTensor
            {
                Label = "G_h",
                Signature = sig,
                Coefficients = new double[] { 0.1, 0.2 },
                Shape = new[] { 2 },
            },
            SpectralDiagnostics = new Dictionary<string, double> { ["conditionNumber"] = 1.5 },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<LinearizationState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("jacobian", deserialized!.Jacobian.Label);
        Assert.Equal(4, deserialized.Jacobian.Rows);
    }

    [Fact]
    public void ObservedState_RoundTrips()
    {
        var branch = new BranchRef { BranchId = "b1", SchemaVersion = "1.0.0" };
        var original = new ObservedState
        {
            ObservationBranchId = "obs-1",
            Observables = new Dictionary<string, ObservableSnapshot>
            {
                ["metric"] = new ObservableSnapshot
                {
                    ObservableId = "metric",
                    OutputType = OutputType.Quantitative,
                    Values = new double[] { 1.0, 0.0, 0.0, 1.0 },
                },
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "test",
                Branch = branch,
            },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ObservedState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("obs-1", deserialized!.ObservationBranchId);
        Assert.Single(deserialized.Observables);
        Assert.Equal(4, deserialized.Observables["metric"].Values.Length);
    }

    [Fact]
    public void ValidationBundle_RoundTrips()
    {
        var branch = new BranchRef { BranchId = "b1", SchemaVersion = "1.0.0" };
        var original = new ValidationBundle
        {
            Branch = branch,
            Records = new[]
            {
                new ValidationRecord
                {
                    RuleId = "parity-check",
                    Category = "parity",
                    Passed = true,
                    MeasuredValue = 1e-12,
                    Tolerance = 1e-10,
                    Timestamp = DateTimeOffset.UtcNow,
                },
            },
            AllPassed = true,
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ValidationBundle>(json);

        Assert.NotNull(deserialized);
        Assert.True(deserialized!.AllPassed);
        Assert.Single(deserialized.Records);
        Assert.Equal("parity-check", deserialized.Records[0].RuleId);
    }

    [Fact]
    public void ResidualBundle_RoundTrips()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "residual-2form",
            Degree = "2",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "row-major",
            MemoryLayout = "dense-row-major",
        };
        var original = new ResidualBundle
        {
            Components = new[]
            {
                new ResidualComponent
                {
                    Label = "shiab-torsion",
                    Norm = 0.01,
                    Field = new FieldTensor
                    {
                        Label = "Upsilon_h",
                        Signature = sig,
                        Coefficients = new double[] { 0.01 },
                        Shape = new[] { 1 },
                    },
                },
            },
            ObjectiveValue = 5e-5,
            TotalNorm = 0.01,
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ResidualBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(5e-5, deserialized!.ObjectiveValue);
        Assert.Single(deserialized.Components);
    }

    [Fact]
    public void ArtifactBundle_RoundTrips()
    {
        var manifest = BranchManifestFactory.CreateEmpty("art-branch");
        var branch = new BranchRef { BranchId = "art-branch", SchemaVersion = "1.0.0" };
        var now = DateTimeOffset.UtcNow;

        var original = new ArtifactBundle
        {
            ArtifactId = "artifact-001",
            Branch = branch,
            ReplayContract = new ReplayContract
            {
                BranchManifest = manifest,
                Deterministic = true,
                BackendId = "cpu-reference",
                ReplayTier = "R2",
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = now,
                CodeRevision = "abc",
                Branch = branch,
            },
            CreatedAt = now,
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ArtifactBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("artifact-001", deserialized!.ArtifactId);
        Assert.True(deserialized.ReplayContract.Deterministic);
        Assert.Equal("cpu-reference", deserialized.ReplayContract.BackendId);
    }

    [Fact]
    public void NormalizationMeta_RoundTrips()
    {
        var original = new NormalizationMeta
        {
            SchemeId = "unit-norm",
            ScaleFactor = 2.5,
            Description = "test normalization",
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<NormalizationMeta>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("unit-norm", deserialized!.SchemeId);
        Assert.Equal(2.5, deserialized.ScaleFactor);
    }

    [Fact]
    public void GeometryContext_RoundTrips()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };
        var original = new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "gauss-2",
            BasisFamilyId = "lagrange-p1",
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
            Patches = new[]
            {
                new PatchInfo { PatchId = "p0", ElementCount = 100, TopologyType = "simplicial" },
            },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<GeometryContext>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(4, deserialized!.BaseSpace.Dimension);
        Assert.Equal(14, deserialized.AmbientSpace.Dimension);
        Assert.Single(deserialized.Patches);
    }

    [Fact]
    public void PatchInfo_RoundTrips()
    {
        var original = new PatchInfo
        {
            PatchId = "patch-0",
            ElementCount = 256,
            TopologyType = "structured",
            Metadata = new Dictionary<string, string> { ["order"] = "2" },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<PatchInfo>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("patch-0", deserialized!.PatchId);
        Assert.Equal(256, deserialized.ElementCount);
        Assert.Equal("2", deserialized.Metadata!["order"]);
    }

    [Fact]
    public void ValidationStamp_RoundTrips()
    {
        var original = new ValidationStamp
        {
            RuleId = "schema-check",
            Passed = true,
            Timestamp = DateTimeOffset.UtcNow,
            Detail = "All fields valid",
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ValidationStamp>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("schema-check", deserialized!.RuleId);
        Assert.True(deserialized.Passed);
    }

    [Fact]
    public void BoundaryConditionBundle_RoundTrips()
    {
        var original = new BoundaryConditionBundle
        {
            ConditionType = "Dirichlet",
            Parameters = new Dictionary<string, string> { ["value"] = "0.0" },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<BoundaryConditionBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("Dirichlet", deserialized!.ConditionType);
    }

    [Fact]
    public void InitialConditionBundle_RoundTrips()
    {
        var original = new InitialConditionBundle
        {
            ConditionType = "flat-connection",
            Parameters = new Dictionary<string, string> { ["seed"] = "42" },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<InitialConditionBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("flat-connection", deserialized!.ConditionType);
    }

    [Fact]
    public void GaugeConditionBundle_RoundTrips()
    {
        var original = new GaugeConditionBundle
        {
            StrategyType = "penalty",
            PenaltyCoefficient = 1.0,
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<GaugeConditionBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("penalty", deserialized!.StrategyType);
        Assert.Equal(1.0, deserialized.PenaltyCoefficient);
    }

    [Fact]
    public void ObservableRequest_RoundTrips()
    {
        var original = new ObservableRequest
        {
            ObservableId = "curvature-scalar",
            OutputType = OutputType.Quantitative,
            Normalization = new NormalizationMeta { SchemeId = "natural" },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ObservableRequest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("curvature-scalar", deserialized!.ObservableId);
        Assert.Equal(OutputType.Quantitative, deserialized.OutputType);
    }

    [Fact]
    public void IntegrityBundle_RoundTrips()
    {
        var original = new IntegrityBundle
        {
            ContentHash = "abc123def456",
            HashAlgorithm = "SHA-256",
            ComputedAt = DateTimeOffset.UtcNow,
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<IntegrityBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("abc123def456", deserialized!.ContentHash);
    }

    [Fact]
    public void ReplayContract_RoundTrips()
    {
        var manifest = BranchManifestFactory.CreateEmpty("replay-branch");
        var original = new ReplayContract
        {
            BranchManifest = manifest,
            Deterministic = true,
            RandomSeed = 42,
            BackendId = "cpu-reference",
            ReplayTier = "R2",
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ReplayContract>(json);

        Assert.NotNull(deserialized);
        Assert.True(deserialized!.Deterministic);
        Assert.Equal(42, deserialized.RandomSeed);
    }

    [Fact]
    public void LinearOperatorModel_RoundTrips()
    {
        var original = new LinearOperatorModel
        {
            Label = "test-operator",
            RealizationType = "sparse-csr",
            Rows = 3,
            Cols = 3,
            Values = new double[] { 1.0, 2.0, 3.0 },
            RowPointers = new int[] { 0, 1, 2, 3 },
            ColIndices = new int[] { 0, 1, 2 },
        };
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<LinearOperatorModel>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("sparse-csr", deserialized!.RealizationType);
        Assert.Equal(3, deserialized.Values!.Length);
    }
}
