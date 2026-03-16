using Gu.Core;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Builds and validates spinor representation specs against the resolved run context.
/// </summary>
public static class SpinorSpecResolver
{
    public static SpinorRepresentationSpec BuildDerivedSpec(
        int ambientDimension,
        int baseDimension,
        ProvenanceMeta provenance,
        CliffordSignature? signature = null,
        string? spinorSpecId = null)
    {
        ArgumentNullException.ThrowIfNull(provenance);

        if (ambientDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(ambientDimension), "Ambient dimension must be positive.");
        if (baseDimension < 0 || baseDimension > ambientDimension)
            throw new ArgumentOutOfRangeException(nameof(baseDimension), "Base dimension must be in [0, ambientDimension].");

        var resolvedSignature = signature ?? new CliffordSignature { Positive = ambientDimension, Negative = 0 };
        int spinorComponents = 1 << (ambientDimension / 2);
        bool hasChirality = ambientDimension % 2 == 0;
        int fiberDimension = ambientDimension - baseDimension;

        return new SpinorRepresentationSpec
        {
            SpinorSpecId = spinorSpecId ?? $"spinor-cl{resolvedSignature.Positive}-{resolvedSignature.Negative}-dim{ambientDimension}-derived-v1",
            SpacetimeDimension = ambientDimension,
            CliffordSignature = resolvedSignature,
            GammaConvention = new GammaConventionSpec
            {
                ConventionId = "dirac-tensor-product-v1",
                Signature = resolvedSignature,
                Representation = "standard",
                SpinorDimension = spinorComponents,
                HasChirality = hasChirality,
            },
            ChiralityConvention = new ChiralityConventionSpec
            {
                ConventionId = hasChirality ? "chirality-standard-derived-v1" : "chirality-trivial-derived-v1",
                SignConvention = "left-is-minus",
                PhaseFactor = hasChirality ? "-1" : "trivial",
                HasChirality = hasChirality,
                FullChiralityOperator = hasChirality ? "Gamma_chi" : null,
                BaseChiralityOperator = baseDimension % 2 == 0 ? "X-chirality" : null,
                FiberChiralityOperator = fiberDimension % 2 == 0 ? "fiber-chirality" : null,
                BaseDimension = baseDimension,
                FiberDimension = fiberDimension,
            },
            ConjugationConvention = new ConjugationConventionSpec
            {
                ConventionId = "hermitian-v1",
                ConjugationType = "hermitian",
                HasChargeConjugation = true,
            },
            SpinorComponents = spinorComponents,
            ChiralitySplit = hasChirality ? spinorComponents / 2 : 0,
            InsertedAssumptionIds = new List<string> { "P4-IA-001", "P4-IA-003" },
            Provenance = provenance,
        };
    }

    public static IReadOnlyList<string> ValidateCompatibility(
        SpinorRepresentationSpec spec,
        int ambientDimension,
        int baseDimension)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var errors = new List<string>();
        int expectedSpinorComponents = 1 << (ambientDimension / 2);
        bool expectedHasChirality = ambientDimension % 2 == 0;
        int expectedChiralitySplit = expectedHasChirality ? expectedSpinorComponents / 2 : 0;
        int expectedFiberDimension = ambientDimension - baseDimension;

        if (spec.SpacetimeDimension != ambientDimension)
            errors.Add($"Spinor spec spacetime dimension {spec.SpacetimeDimension} does not match ambient dimension {ambientDimension}.");

        if (spec.GammaConvention.SpinorDimension != spec.SpinorComponents)
        {
            errors.Add(
                $"Gamma convention spinor dimension {spec.GammaConvention.SpinorDimension} does not match spinorComponents {spec.SpinorComponents}.");
        }

        if (spec.SpinorComponents != expectedSpinorComponents)
        {
            errors.Add(
                $"Spinor component count {spec.SpinorComponents} does not match expected 2^floor(dim/2)={expectedSpinorComponents} for dimY={ambientDimension}.");
        }

        if (spec.GammaConvention.HasChirality != expectedHasChirality)
            errors.Add($"Gamma convention chirality flag {spec.GammaConvention.HasChirality} is incompatible with dimY={ambientDimension}.");

        if (spec.ChiralityConvention.HasChirality != expectedHasChirality)
        {
            errors.Add(
                $"Chirality convention flag {spec.ChiralityConvention.HasChirality} is incompatible with dimY={ambientDimension}.");
        }

        if (spec.ChiralitySplit != expectedChiralitySplit)
        {
            errors.Add(
                $"Chirality split {spec.ChiralitySplit} does not match expected value {expectedChiralitySplit} for dimY={ambientDimension}.");
        }

        if (spec.ChiralityConvention.BaseDimension.HasValue && spec.ChiralityConvention.BaseDimension.Value != baseDimension)
        {
            errors.Add(
                $"Chirality convention base dimension {spec.ChiralityConvention.BaseDimension.Value} does not match base dimension {baseDimension}.");
        }

        if (spec.ChiralityConvention.FiberDimension.HasValue
            && spec.ChiralityConvention.FiberDimension.Value != expectedFiberDimension)
        {
            errors.Add(
                $"Chirality convention fiber dimension {spec.ChiralityConvention.FiberDimension.Value} does not match fiber dimension {expectedFiberDimension}.");
        }

        return errors;
    }
}
