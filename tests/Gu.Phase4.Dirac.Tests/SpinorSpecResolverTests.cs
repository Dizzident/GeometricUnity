using Gu.Core;
using Gu.Phase4.Spin;
using Xunit;

namespace Gu.Phase4.Dirac.Tests;

public sealed class SpinorSpecResolverTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-p12-spinor",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    [Fact]
    public void BuildDerivedSpec_UsesRunDimensions()
    {
        var spec = SpinorSpecResolver.BuildDerivedSpec(ambientDimension: 5, baseDimension: 2, TestProvenance());

        Assert.Equal(5, spec.SpacetimeDimension);
        Assert.Equal(4, spec.SpinorComponents);
        Assert.False(spec.ChiralityConvention.HasChirality);
        Assert.Equal(2, spec.ChiralityConvention.BaseDimension);
        Assert.Equal(3, spec.ChiralityConvention.FiberDimension);
    }

    [Fact]
    public void ValidateCompatibility_ReturnsNoErrors_ForMatchingDerivedSpec()
    {
        var spec = SpinorSpecResolver.BuildDerivedSpec(ambientDimension: 6, baseDimension: 2, TestProvenance());

        var errors = SpinorSpecResolver.ValidateCompatibility(spec, ambientDimension: 6, baseDimension: 2);

        Assert.Empty(errors);
        Assert.True(spec.ChiralityConvention.HasChirality);
        Assert.Equal(4, spec.ChiralitySplit);
    }

    [Fact]
    public void ValidateCompatibility_RejectsDimensionMismatch()
    {
        var spec = new SpinorRepresentationSpec
        {
            SpinorSpecId = "invalid-dim",
            SpacetimeDimension = 5,
            CliffordSignature = new CliffordSignature { Positive = 5, Negative = 0 },
            GammaConvention = new GammaConventionSpec
            {
                ConventionId = "dirac-tensor-product-v1",
                Signature = new CliffordSignature { Positive = 5, Negative = 0 },
                Representation = "standard",
                SpinorDimension = 4,
                HasChirality = false,
            },
            ChiralityConvention = new ChiralityConventionSpec
            {
                ConventionId = "chirality-trivial-v1",
                SignConvention = "left-is-minus",
                PhaseFactor = "trivial",
                HasChirality = false,
                BaseDimension = 2,
                FiberDimension = 3,
            },
            ConjugationConvention = new ConjugationConventionSpec
            {
                ConventionId = "hermitian-v1",
                ConjugationType = "hermitian",
                HasChargeConjugation = true,
            },
            SpinorComponents = 4,
            ChiralitySplit = 0,
            Provenance = TestProvenance(),
        };

        var errors = SpinorSpecResolver.ValidateCompatibility(spec, ambientDimension: 4, baseDimension: 2);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("spacetime dimension", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateCompatibility_RejectsInconsistentChirality()
    {
        var spec = SpinorSpecResolver.BuildDerivedSpec(ambientDimension: 6, baseDimension: 2, TestProvenance());
        spec = new SpinorRepresentationSpec
        {
            SpinorSpecId = spec.SpinorSpecId,
            SpacetimeDimension = spec.SpacetimeDimension,
            CliffordSignature = spec.CliffordSignature,
            GammaConvention = new GammaConventionSpec
            {
                ConventionId = spec.GammaConvention.ConventionId,
                Signature = spec.GammaConvention.Signature,
                Representation = spec.GammaConvention.Representation,
                SpinorDimension = spec.GammaConvention.SpinorDimension,
                HasChirality = false,
            },
            ChiralityConvention = new ChiralityConventionSpec
            {
                ConventionId = spec.ChiralityConvention.ConventionId,
                SignConvention = spec.ChiralityConvention.SignConvention,
                PhaseFactor = spec.ChiralityConvention.PhaseFactor,
                HasChirality = false,
                BaseDimension = spec.ChiralityConvention.BaseDimension,
                FiberDimension = spec.ChiralityConvention.FiberDimension,
            },
            ConjugationConvention = spec.ConjugationConvention,
            SpinorComponents = spec.SpinorComponents,
            ChiralitySplit = 0,
            Provenance = spec.Provenance,
        };

        var errors = SpinorSpecResolver.ValidateCompatibility(spec, ambientDimension: 6, baseDimension: 2);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("chirality", StringComparison.OrdinalIgnoreCase));
    }
}
