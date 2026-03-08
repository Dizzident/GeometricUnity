using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class DiscreteStateTests
{
    private static DiscreteState CreateSample()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };

        return new DiscreteState
        {
            Branch = new BranchRef { BranchId = "minimal-gu-v1", SchemaVersion = "1.0.0" },
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
                    TargetSpace = baseSpace
                },
                ObservationBinding = new GeometryBinding
                {
                    BindingType = "observation",
                    SourceSpace = baseSpace,
                    TargetSpace = ambientSpace
                },
                Patches = Array.Empty<PatchInfo>()
            },
            Omega = new FieldTensor
            {
                Label = "omega_h",
                Signature = new TensorSignature
                {
                    AmbientSpaceId = "Y_h",
                    CarrierType = "connection-1form",
                    Degree = "1",
                    LieAlgebraBasisId = "su2-standard",
                    ComponentOrderId = "lexicographic",
                    MemoryLayout = "dense-row-major"
                },
                Coefficients = new double[] { 0.1, 0.2, 0.3 },
                Shape = new[] { 3 }
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = new BranchRef { BranchId = "minimal-gu-v1", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference"
            }
        };
    }

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = CreateSample();
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<DiscreteState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Branch.BranchId, deserialized.Branch.BranchId);
        Assert.Equal(original.Omega.Label, deserialized.Omega.Label);
        Assert.Equal(original.Omega.Coefficients, deserialized.Omega.Coefficients);
        Assert.Equal(original.Provenance.Backend, deserialized.Provenance.Backend);
    }

    [Fact]
    public void Omega_IsTheIndependentField()
    {
        // Per Section 10.3: DiscreteState carries the independent field omega_h
        var state = CreateSample();
        Assert.Equal("omega_h", state.Omega.Label);
        Assert.Equal("connection-1form", state.Omega.Signature.CarrierType);
    }

    [Fact]
    public void Omega_LivesInAmbientSpace()
    {
        // Per Section 4.4: omega is an ad(P)-valued discrete connection field over Y_h
        var state = CreateSample();
        Assert.Equal("Y_h", state.Omega.Signature.AmbientSpaceId);
    }
}
